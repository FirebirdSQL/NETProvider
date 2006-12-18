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
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if	(VISUAL_STUDIO)

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using VSLangProj;

namespace FirebirdSql.Data.Firebird.Design
{
	/// <summary>
	/// For	extensibility we need references to	VSLangProj.dll and evndte.dll
	/// </summary>
	/// <remarks>
	/// This class has support only	for	<b>Microsoft Visual	Studio Products</b>
	/// References:
	/// 	http://msdn.microsoft.com/library/default.asp?url=/library/en-us/vsintro7/html/vxoriExtendingVisualStudioEnvironment.asp
	/// 	http://devresource.hp.com/technical_white_papers/CodeModel1.pdf
	/// </remarks>
	internal class VSExtensibility
	{
		#region PInvoke	definitions

		/// <summary>
		/// References:
		/// 	http://devresource.hp.com/technical_white_papers/CodeModel1.pdf	
		/// </summary>
		[DllImport("ole32.dll", EntryPoint = "GetRunningObjectTable")]
		static extern int GetRunningObjectTable(int res, out UCOMIRunningObjectTable ROT);

		/// <summary>
		/// References:
		/// 	http://devresource.hp.com/technical_white_papers/CodeModel1.pdf	
		/// </summary>
		[DllImport("ole32.dll", EntryPoint = "CreateBindCtx")]
		static extern int CreateBindCtx(int res, out UCOMIBindCtx ctx);

		#endregion

		#region DTE	ProgID constants

		public static string[] ProductNames =
		{
			"Visual Studio 2002",
			"Visual Studio 2003",
			"Visual Studio 2005",
			"Visual C# Express"
		};

		public static readonly string VisualStudio2002ProgID = "VisualStudio.DTE.7";
		public static readonly string VisualStudio2003ProgID = "VisualStudio.DTE.7.1";
		public static readonly string VisualStudio2005ProgID = "VisualStudio.DTE.8.0";
		public static readonly string VisualCSharpExpressProgID = "VCSExpress.DTE.8.0";

		#endregion

		#region Fields

		private EnvDTE.DTE dte;
		private string productName;

		#endregion

		#region Constructors

		public VSExtensibility(string productName)
		{
			this.productName = productName;
			this.dte = GetDTE(this.productName);
		}

		#endregion

		#region Static Methods

		public static string[] GetAvailableProductNames()
		{
			ArrayList products = new ArrayList();

			foreach (string product in ProductNames)
			{
				if (IsDTEAvailable(product))
				{
					products.Add(product);
				}
			}

			return (string[])products.ToArray(typeof(String));
		}

		private static bool IsDTEAvailable(string productName)
		{
			return GetDTE(productName) != null;
		}

		private static EnvDTE.DTE GetDTE(string productName)
		{
			try
			{
				switch (productName)
				{
					case "Visual Studio 2002":
						return GetCurrentDTEObject(VisualStudio2002ProgID);

					case "Visual Studio 2003":
						return GetCurrentDTEObject(VisualStudio2003ProgID);

					case "Visual Studio 2005":
						return GetCurrentDTEObject(VisualStudio2005ProgID);

					case "Visual C# Express":
						return GetCurrentDTEObject(VisualCSharpExpressProgID);
				}
			}
			catch
			{
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// References:
		/// 	http://devresource.hp.com/technical_white_papers/CodeModel1.pdf
		/// </remarks>
		private static EnvDTE.DTE GetCurrentDTEObject(string dteName)
		{
			string					name	= null;
			int						numMons = 50;
			int						fetched = 0;
			EnvDTE.DTE				dte		= null;
			UCOMIMoniker[]			aMons	= new UCOMIMoniker[numMons];
			UCOMIBindCtx			ctx		= null;
			UCOMIEnumMoniker		enumMon = null;
			UCOMIRunningObjectTable rot		= null;	// Get the ROT

			int ret = GetRunningObjectTable(0, out rot);

			if (ret == 0) // S_OK
			{
				// Get an enumerator to	access the registered objects. 
				rot.EnumRunning(out	enumMon);

				if (enumMon != null)
				{
					// Just	grab a bunch of	them at	once, 50 should	be 
					// plenty for a	test 
					enumMon.Next(numMons, aMons, out fetched);

					// Set up a	binding	context	so that	we can access 
					// the monikers. 
					ret = CreateBindCtx(0, out ctx);

					// Create the ROT name of the _DTE object using the process id 
					System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

					dteName += (":" + currentProcess.Id.ToString());

					// for each	moniker	retrieved 
					for (int i = 0; i < fetched; i++)
					{
						// Get the display string 
						aMons[i].GetDisplayName(ctx, null, out name);

						// if this is the one we are interested	in... 
						if (name.IndexOf(dteName) != -1)
						{
							object temp;
							rot.GetObject(aMons[i], out	temp);
							dte = (EnvDTE.DTE)temp;
						}
					}
				}
			}

			return dte;
		}

		#endregion

		#region Methods

		public string GetActiveProjectLanguage()
		{
			switch (this.GetActiveProject().Kind)
			{
				case PrjKind.prjKindVBProject:
					return "VisualBasic";

				case PrjKind.prjKindCSharpProject:
				default:
					return "CSharp";
			}
		}

		public string GetActiveProjectPath()
		{
			return Path.GetDirectoryName(this.GetActiveProject().FullName);
		}

		public string GetActiveDocumentPath()
		{
			return this.GetActiveDocument().Path;
		}

		public string GetCurrentNamespace()
		{
			return this.GetActiveProject().Name;
		}

		public string AddTypedDataSet(string fileName)
		{
			ProjectItem item = this.dte.ItemOperations.AddExistingItem(fileName);

			// Set as an Embedded resource
			foreach (Property p in item.Properties)
			{
				switch (p.Name.ToLower())
				{
					case "buildaction":
						p.Value = 3;
						break;

					case "customtool":
						p.Value = "MSDataSetGenerator";
						break;
				}
			}

			// Try to obtain the namespace for the DataSet
			foreach (ProjectItem subItem in item.Collection)
			{
				if (subItem.FileCodeModel != null && 
					subItem.FileCodeModel.CodeElements.Count > 0)
				{
					return subItem.FileCodeModel.CodeElements.Item(1).FullName;
				}
			}

			return null;
		}

		#endregion

		#region Private	methods

		private Project GetActiveProject()
		{
			return this.GetActiveDocument().ProjectItem.ContainingProject;
		}

		private Document GetActiveDocument()
		{
			return this.dte.ActiveDocument;
		}

		#endregion
	}
}

#endif