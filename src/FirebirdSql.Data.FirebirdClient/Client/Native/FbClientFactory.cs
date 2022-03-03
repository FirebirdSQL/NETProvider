/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Dean Harding, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using FirebirdSql.Data.Client.Native.Handles;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native;

/// <summary>
/// This class generates a dynamic class that implements the <see cref="IFbClient"/> interface and
/// calls the native methods in a given "fbembed.dll" (though you can name it anything you like).
/// </summary>
internal static class FbClientFactory
{
	private static readonly string DefaultDllName = "fbembed";

	/// <summary>
	/// Because generating the class at runtime is expensive, we cache it here based on the name
	/// specified.
	/// </summary>
	private static readonly Dictionary<string, IFbClient> cache;
	private static readonly ReaderWriterLockSlim cacheLock;
	private static readonly HashSet<Type> injectionTypes;

	/// <summary>
	/// Static constructor sets up member variables.
	/// </summary>
	static FbClientFactory()
	{
		cache = new Dictionary<string, IFbClient>();
		cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		injectionTypes = new HashSet<Type>(typeof(FbClientFactory).Assembly.GetTypes()
			.Where(x => !x.IsAbstract && !x.IsInterface)
			.Where(x => typeof(IFirebirdHandle).IsAssignableFrom(x))
			.Select(x => x.MakeByRefType()));
	}

	/// <summary>
	/// Dynamically generates a class that will load the "fbembed.dll" given in <c>dllName</c>, and that
	/// also implements <see cref="IFbClient"/>, which you can use to call the library.
	/// </summary>
	/// <param name="dllName">The name of the DLL to load (e.g. "fbembed", "C:\path\to\fbembed.dll", etc)</param>
	/// <returns>A class that implements <see cref="IFbClient"/> and calls into the native library you specify.</returns>
	public static IFbClient Create(string dllName)
	{
		if (string.IsNullOrEmpty(dllName))
		{
			dllName = DefaultDllName;
		}

		cacheLock.EnterUpgradeableReadLock();
		try
		{
			if (cache.TryGetValue(dllName, out var result))
			{
				return result;
			}
			else
			{
				cacheLock.EnterWriteLock();
				try
				{
					result = BuildFbClient(dllName);
					cache.Add(dllName, result);
					ShutdownHelper.RegisterFbClientShutdown(() => NativeHelpers.CallIfExists(() => result.fb_shutdown(0, 0)));
					return result;
				}
				finally
				{
					cacheLock.ExitWriteLock();
				}
			}
		}
		finally
		{
			cacheLock.ExitUpgradeableReadLock();
		}
	}

	/// <summary>
	/// This method does the "heavy-lifting" of actually generating a dynamic class that implements
	/// <see cref="IFbClient"/>, and calls the native library specified to do the actual work.
	/// </summary>
	/// <param name="dllName">The name of the libarary to use, as passed into the
	/// <see cref="DllImportAttribute"/> that is dynamically generated.</param>
	/// <returns>An implementation of <see cref="IFbClient"/>.</returns>
	/// <remarks>
	/// <para>Note: To be completly generic, we actually reflect through <see cref="IFbClient"/>
	/// to get the methods and parameters to generate.</para>
	/// </remarks>
	private static IFbClient BuildFbClient(string dllName)
	{
		// Get the initial TypeBuilder, with a "blank" class definition
		var tb = CreateTypeBuilder(dllName);

		// It needs to implement IFbClient, obviously!
		tb.AddInterfaceImplementation(typeof(IFbClient));

		// Now, go through all the methods in IFbClient and generate the corresponding methods
		// in our dynamic type.
		foreach (var mi in typeof(IFbClient).GetMethods())
		{
			GenerateMethod(tb, mi, dllName);
		}

		// Finally, create and return an instance of the type itself. Woot!
		return CreateInstance(tb);
	}

	/// <summary>
	/// Generates a method on our <see cref="TypeBuilder"/> for the specified <see cref="MethodInfo"/>
	/// </summary>
	/// <param name="tb">The <see cref="TypeBuilder"/> we're generating our type with.</param>
	/// <param name="mi">The <see cref="MethodInfo"/> which represents the "template" method.</param>
	/// <param name="dllName">The path to the DLL that we'll put in the <see cref="DllImportAttribute"/>.</param>
	private static void GenerateMethod(TypeBuilder tb, MethodInfo mi, string dllName)
	{
		// These are all the parameters in our method
		var pis = new List<ParameterInfo>(mi.GetParameters());

		// We need to keep the parameter types and attributes in a separate array.
		var ptypes = new Type[pis.Count];
		var attrs = new ParameterAttributes[pis.Count];
		for (var i = 0; i < pis.Count; i++)
		{
			ptypes[i] = pis[i].ParameterType;
			attrs[i] = pis[i].Attributes;
		}

		// We actually need to create TWO methods - one for the interface implementation, and one for the
		// P/Invoke declaration. We'll create the P/Invoke definition first.
		var smb = tb.DefineMethod(
			mi.Name, // The name is the same as the interface name
					 // P/Invoke methods need special attributes...
			MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig,
			mi.ReturnType, ptypes);

		// Get the type of the DllImportAttribute, which we'll attach to this method
		var diaType = typeof(DllImportAttribute);

		// Create a CustomAttributeBuilder for the DLLImportAttribute, specifying the constructor that takes a string argument.
		var ctor = diaType.GetConstructor(new Type[] { typeof(string) });
		var cab = new CustomAttributeBuilder(ctor, new object[] { dllName });

		// Assign the DllImport attribute to the smb
		smb.SetCustomAttribute(cab);

		// Also, any attributes on the actual parameters need to be copied to the P/Invoke declaration as well.
		for (var i = 0; i < attrs.Length; i++)
		{
			smb.DefineParameter(i + 1, attrs[i], pis[i].Name);
		}

		// Now create the interface implementation method
		var mb = tb.DefineMethod(
			"IFbClient." + mi.Name, // We use the standard "Interface.Method" to do an explicit interface implementation
			MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual |
			MethodAttributes.Final,
			mi.ReturnType, ptypes);

		// Also, any attributes on the actual parameters need to be copied to the P/Invoke declaration as well.
		for (var i = 0; i < attrs.Length; i++)
		{
			mb.DefineParameter(i + 1, attrs[i], pis[i].Name);
		}

		// We need to generate a little IL here to actually call the P/Invoke declaration. Luckily for us, since we're just
		// going to pass our parameters to the P/Invoke method as-is, we don't need to muck with the eval stack ;-)
		var il = mb.GetILGenerator();
		for (var i = 1; i <= pis.Count; i++)
		{
			EmitLdarg(il, i);
		}

		il.EmitCall(OpCodes.Call, smb, null);

		EmitClientInjectionToFirebirdHandleOjects(mi.ReturnType, pis, il);

		il.Emit(OpCodes.Ret);

		// Define the fact that our IFbClient.Method is the explicit interface implementation of that method
		tb.DefineMethodOverride(mb, mi);
	}

	private static void EmitClientInjectionToFirebirdHandleOjects(
		Type returnType,
		List<ParameterInfo> pis,
		ILGenerator il)
	{
		var injectProperties = pis.Select(x => x.ParameterType).Intersect(injectionTypes).Any();
		if (injectProperties)
		{
			il.DeclareLocal(returnType);
			il.Emit(OpCodes.Stloc_0);
			for (var i = 0; i < pis.Count; i++)
			{
				if (injectionTypes.Contains(pis[i].ParameterType))
				{
					EmitLdarg(il, i + 1);
					il.Emit(OpCodes.Ldind_Ref);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Callvirt, typeof(IFirebirdHandle).GetMethod("SetClient"));
					il.Emit(OpCodes.Nop);
				}
			}

			il.Emit(OpCodes.Ldloc_0);
		}
	}

	private static void EmitLdarg(ILGenerator il, int i)
	{
		if (i == 1)
		{
			il.Emit(OpCodes.Ldarg_1);
		}
		else if (i == 2)
		{
			il.Emit(OpCodes.Ldarg_2);
		}
		else if (i == 3)
		{
			il.Emit(OpCodes.Ldarg_3);
		}
		else
		{
			il.Emit(OpCodes.Ldarg_S, (short)i);
		}
	}

	/// <summary>
	/// Creates an instance of the type itself and returns it. Cool!!
	/// </summary>
	/// <param name="tb">The <see cref="TypeBuilder"/> that we created our type with.</param>
	/// <returns>An instance of our type, cast as an <see cref="IFbClient"/>.</returns>
	private static IFbClient CreateInstance(TypeBuilder tb)
	{
		var t = tb.CreateTypeInfo().AsType();

#if DEBUG
#if NET48
		var ab = (AssemblyBuilder)tb.Assembly;
		ab.Save("DynamicAssembly.dll");
#endif
#endif

		return (IFbClient)Activator.CreateInstance(t);
	}

	/// <summary>
	/// Creates the assembly and module into which we'll generate our class, and returns
	/// a <see cref="TypeBuilder"/> we can use for building up our type.
	/// </summary>
	/// <param name="baseName">The "base name" to use for the name of the assembly and mode.</param>
	/// <returns>A <see cref="TypeBuilder"/> which we can use for building our type.</returns>
	/// <remarks>
	/// <para>Notice that we actually generate a new assembly for every different <c>dllName</c> that is
	/// passed into <see cref="BuildFbClient"/>. This might be inefficient, but since we're mostly
	/// only ever going to have one (or maybe two) different <c>dllName</c>s, it's not a big deal.</para>
	/// </remarks>
	private static TypeBuilder CreateTypeBuilder(string baseName)
	{
		baseName = SanitizeBaseName(baseName);

		// Generate a name for our assembly, based on the name of the DLL.
		var assemblyName = new AssemblyName();
		assemblyName.Name = baseName + "_Assembly";

		// We create the dynamic assembly in our current AppDomain
		var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName,
#if NET48
				AssemblyBuilderAccess.RunAndSave
#else
				AssemblyBuilderAccess.Run
#endif
			);

		// Generate the actual module (which is the DLL itself)
		var moduleBuilder = assemblyBuilder.DefineDynamicModule(baseName + "_Module");

		// Add our type to the module.
		return moduleBuilder.DefineType(baseName + "_Class", TypeAttributes.Class);
	}

	/// <summary>
	/// Because the <c>baseName</c> could include things like '\' and '/' - which are not legal
	/// type names, we need to "sanitize" the name and make it acceptable.
	/// </summary>
	/// <param name="baseName">The "base name" which we'll make sure contains only legal
	/// identfier characters.</param>
	/// <returns>A new string that is a value type name.</returns>
	private static string SanitizeBaseName(string baseName)
	{
		// Note: We could actually go through and replace invalid characters with different
		// characters, and so on, but that's too much work. Besides, you never really see the
		// dynamic type name (expect maybe in a stack trace). If you really don't like this method,
		// you're free to change it ;)

		return "FB_" + Math.Abs(baseName.GetHashCode());
	}
}
