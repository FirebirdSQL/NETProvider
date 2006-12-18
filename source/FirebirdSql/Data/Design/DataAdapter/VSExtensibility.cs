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
 *	Copyright (c) 2002, 2006 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if	(VISUAL_STUDIO)

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE80;
using VSLangProj80;

namespace FirebirdSql.Data.Design.DataAdapter
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
        #region · Static Methods ·

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

        private static EnvDTE80.DTE2 GetDTE(string productName)
        {
            try
            {
                switch (productName)
                {
                    case "Visual C# Express":
                        return GetCurrentDTEObject(VisualCSharpExpressProgID);

                    case "Visual Studio 2005":
                        return GetCurrentDTEObject(VisualStudio2005ProgID);
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
        private static EnvDTE80.DTE2 GetCurrentDTEObject(string dteName)
        {
            string name = null;
            EnvDTE80.DTE2 dte = null;
            IMoniker[] monikers = null;
            IBindCtx ctx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker monikerEnumerator = null;
            System.Diagnostics.Process currentProcess = null;

            if (GetRunningObjectTable(0, out rot) == 0) // S_OK
            {
                // Get an enumerator to	access the registered objects. 
                rot.EnumRunning(out	monikerEnumerator);

                if (monikerEnumerator != null)
                {
                    // Reset moniker enumerator
                    monikerEnumerator.Reset();

                    // Get current process
                    currentProcess = System.Diagnostics.Process.GetCurrentProcess();

                    // Build DTE instance process name
                    dteName += (":" + currentProcess.Id.ToString());

                    // Set up a	binding	context	so that	we can access 
                    // the monikers. 
                    CreateBindCtx(0, out ctx);

                    // Initialize monikers array
                    monikers = new IMoniker[1];

                    // Process active monikers
                    while (monikerEnumerator.Next(1, monikers, IntPtr.Zero) == 0)
                    {
                        // Get the display string 
                        monikers[0].GetDisplayName(ctx, null, out name);

                        // if this is the one we are interested	in... 
                        if (name.IndexOf(dteName) != -1)
                        {
                            object runningObject;
                            rot.GetObject(monikers[0], out runningObject);

                            dte = (EnvDTE80.DTE2)runningObject;
                            break;
                        }
                    }
                }
            }

            return dte;
        }

        #endregion
        
        #region · PInvoke definitions ·

		/// <summary>
		/// References:
		/// 	http://devresource.hp.com/technical_white_papers/CodeModel1.pdf	
		/// </summary>
		[DllImport("ole32.dll", EntryPoint = "GetRunningObjectTable")]
		static extern int GetRunningObjectTable(int res, out IRunningObjectTable ROT);

		/// <summary>
		/// References:
		/// 	http://devresource.hp.com/technical_white_papers/CodeModel1.pdf	
		/// </summary>
		[DllImport("ole32.dll", EntryPoint = "CreateBindCtx")]
		static extern int CreateBindCtx(int res, out IBindCtx ctx);

		#endregion

		#region · DTE ProgID constants ·

		public static string[] ProductNames =
		{
			"Visual C# Express",
			"Visual Studio 2005"
		};

		public static readonly string VisualCSharpExpressProgID = "VCSExpress.DTE.8.0";
		public static readonly string VisualStudio2005ProgID = "VisualStudio.DTE.8.0";

		#endregion

		#region · Fields ·

		private EnvDTE80.DTE2 dte;
		private string productName;

		#endregion

		#region · Constructors ·

		public VSExtensibility(string productName)
		{
			this.productName = productName;
			this.dte = GetDTE(this.productName);
		}

		#endregion

		#region · Methods ·

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
			EnvDTE.ProjectItem item = this.dte.ItemOperations.AddExistingItem(fileName);

			// Set as an Embedded resource
			foreach (EnvDTE.Property p in item.Properties)
			{
				switch (p.Name.ToLower())
				{
					case "buildaction":
						p.Value = 3;
						break;
				}
			}

			// Try to obtain the namespace for the DataSet
			foreach (EnvDTE.ProjectItem subItem in item.Collection)
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

		#region · Private Methods ·

		private EnvDTE.Project GetActiveProject()
		{
			return this.GetActiveDocument().ProjectItem.ContainingProject;
		}

        private EnvDTE.Document GetActiveDocument()
		{
			return this.dte.ActiveDocument;
		}

		#endregion
	}
}

#endif