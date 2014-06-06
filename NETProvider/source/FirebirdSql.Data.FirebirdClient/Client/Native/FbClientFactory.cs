/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2007 Dean Harding
 *	All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Client.Native
{
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
		private static IDictionary<string, IFbClient> cache;

		/// <summary>
		/// Static constructor sets up member variables.
		/// </summary>
		static FbClientFactory()
		{
			cache = new SortedDictionary<string, IFbClient>();
		}

		/// <summary>
		/// Dynamically generates a class that will load the "fbembed.dll" given in <c>dllName</c>, and that
		/// also implements <see cref="IFbClient"/>, which you can use to call the library.
		/// </summary>
		/// <param name="dllName">The name of the DLL to load (e.g. "fbembed", "C:\path\to\fbembed.dll", etc)</param>
		/// <returns>A class that implements <see cref="IFbClient"/> and calls into the native library you specify.</returns>
		public static IFbClient GetFbClient(string dllName)
		{
			if (string.IsNullOrEmpty(dllName))
			{
				dllName = DefaultDllName;
			}

			IFbClient fbClient;

			// First, try to get the IFbClient from the cache.
			lock(cache)
			{
				if (cache.TryGetValue(dllName, out fbClient))
				{
					// We got one!
					return fbClient;
				}
			}

			// If we didn't get one, then generate a new one (note: because we're outside the lock, we
			// may end up generating two different classes for the same DLL if we're called multiple times
			// initially, but that's OK - only one is added to the cache and it only happens on startup)
			fbClient = GenerateFbClient(dllName);

			// Add it into the cache for next time
			lock(cache)
			{
				if (cache.ContainsKey(dllName))
				{
					// If there's one in there now, it means somebody else already generated one while
					// we were generating ours. Just use theirs... oh well
					fbClient = cache[dllName];
				}
				else
				{
					// Nothing in there yet, we must've been the first. Add it now.
					cache.Add(dllName, fbClient);
				}
			}

			return fbClient;
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
		private static IFbClient GenerateFbClient(string dllName)
		{
			// Get the initial TypeBuilder, with a "blank" class definition
			TypeBuilder tb = CreateTypeBuilder(dllName);

			// It needs to implement IFbClient, obviously!
			tb.AddInterfaceImplementation(typeof (IFbClient));

			// Now, go through all the methods in IFbClient and generate the corresponding methods
			// in our dynamic type.
			foreach(MethodInfo mi in typeof(IFbClient).GetMethods())
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
			List<ParameterInfo> pis = new List<ParameterInfo>(mi.GetParameters());

			// We need to keep the parameter types and attributes in a separate array.
			Type[] ptypes = new Type[pis.Count];
			ParameterAttributes[] attrs = new ParameterAttributes[pis.Count];
			for (int i = 0; i < pis.Count; i++)
			{
				ptypes[i] = pis[i].ParameterType;
				attrs[i] = pis[i].Attributes;
			}

			// We actually need to create TWO methods - one for the interface implementation, and one for the
			// P/Invoke declaration. We'll create the P/Invoke definition first.
			MethodBuilder smb = tb.DefineMethod(
				mi.Name, // The name is the same as the interface name
				// P/Invoke methods need special attributes...
				MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig,
				mi.ReturnType, ptypes);

			// Get the type of the DllImportAttribute, which we'll attach to this method
			Type diaType = typeof (DllImportAttribute);

			// Create a CustomAttributeBuilder for the DLLImportAttribute, specifying the constructor that takes a string argument.
			ConstructorInfo ctor = diaType.GetConstructor(new Type[] { typeof(string) });
			CustomAttributeBuilder cab = new CustomAttributeBuilder(ctor, new object[] { dllName });

			// Assign the DllImport attribute to the smb
			smb.SetCustomAttribute(cab);

			// Also, any attributes on the actual parameters need to be copied to the P/Invoke declaration as well.
			for (int i = 0; i < attrs.Length; i++)
			{
				smb.DefineParameter(i + 1, attrs[i], pis[i].Name);
			}

			// Now create the interface implementation method
			MethodBuilder mb = tb.DefineMethod(
				"IFbClient." + mi.Name, // We use the standard "Interface.Method" to do an explicit interface implementation
				MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual |
				MethodAttributes.Final,
				mi.ReturnType, ptypes);

			// Also, any attributes on the actual parameters need to be copied to the P/Invoke declaration as well.
			for (int i = 0; i < attrs.Length; i++)
			{
				mb.DefineParameter(i + 1, attrs[i], pis[i].Name);
			}

			// We need to generate a little IL here to actually call the P/Invoke declaration. Luckily for us, since we're just
			// going to pass our parameters to the P/Invoke method as-is, we don't need to muck with the eval stack ;-)
			ILGenerator il = mb.GetILGenerator();
			for (int i = 1; i <= pis.Count; i++)
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

			il.EmitCall(OpCodes.Call, smb, null);
			il.Emit(OpCodes.Ret);

			// Define the fact that our IFbClient.Method is the explicit interface implementation of that method
			tb.DefineMethodOverride(mb, mi);
		}

		/// <summary>
		/// Creates an instance of the type itself and returns it. Cool!!
		/// </summary>
		/// <param name="tb">The <see cref="TypeBuilder"/> that we created our type with.</param>
		/// <returns>An instance of our type, cast as an <see cref="IFbClient"/>.</returns>
		private static IFbClient CreateInstance(TypeBuilder tb)
		{
			Type t = tb.CreateType();

#if (DEBUG)
			// In debug mode, we'll save the assembly out to disk, so we can look at it in Reflector.
			AssemblyBuilder ab = (AssemblyBuilder) tb.Assembly;
			ab.Save("DynamicAssembly.dll");
#endif

			// Create an instance of the type and return it.
			return (IFbClient) Activator.CreateInstance(t);
		}

		/// <summary>
		/// Creates the assembly and module into which we'll generate our class, and returns
		/// a <see cref="TypeBuilder"/> we can use for building up our type.
		/// </summary>
		/// <param name="baseName">The "base name" to use for the name of the assembly and mode.</param>
		/// <returns>A <see cref="TypeBuilder"/> which we can use for building our type.</returns>
		/// <remarks>
		/// <para>Notice that we actually generate a new assembly for every different <c>dllName</c> that is
		/// passed into <see cref="GenerateFbClient"/>. This might be inefficient, but since we're mostly
		/// only ever going to have one (or maybe two) different <c>dllName</c>s, it's not a big deal.</para>
		/// </remarks>
		private static TypeBuilder CreateTypeBuilder(string baseName)
		{
			baseName = SanitizeBaseName(baseName);

			// Generate a name for our assembly, based on the name of the DLL.
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = baseName + "_Assembly";

			// We create the dynamic assembly in our current AppDomain
			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
					assemblyName,
					AssemblyBuilderAccess.RunAndSave);

			// Generate the actual module (which is the DLL itself)
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
					baseName + "_Module",
					baseName + ".dll");

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
}
