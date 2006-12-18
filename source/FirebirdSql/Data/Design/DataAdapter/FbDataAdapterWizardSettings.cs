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

#if	(NET)

using System;
using System.ComponentModel;
using System.ComponentModel.Design;

using FirebirdSql.WizardFramework;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Design.DataAdapter
{
	internal sealed class FbDataAdapterWizardSettings
	{
		#region · Constants ·

		public const string SettingsKey = "FbDataAdapterConfigurationWizardSettings";
		public const string DesignerHostKey = "DesignerHost";
		public const string DataAdapterKey = "DataAdapter";
		public const string ConnectionKey = "Connection";
		public const string CommandTypeKey = "CommandType";
		public const string CommandTextKey = "CommandText";
		public const string DmlGenerationKey = "DMLGeneration";
		public const string SelectSPKey = "SelectStoredProcedure";
		public const string InsertSPKey = "InsertStoredProcedure";
		public const string UpdateSPKey = "UpdateStoredProcedure";
		public const string DeleteSPKey = "DeleteStoredProcedure";
		public const string UseQuotedIdentifiers = "UseQuotedIdentifiers";

		#endregion

		#region · Constructors ·

		private FbDataAdapterWizardSettings()
		{
		}

		#endregion

		#region · Static Methods ·

		public static WizardSettings Register()
		{
			return WizardSettingsManager.Instance.RegisterSettings(SettingsKey);
		}

		public static void Unregister()
		{
			try
			{
				WizardSettingsManager.Instance.UnregisterSettings(SettingsKey);
			}
			catch (ArgumentException)
			{
				// if the key didn't exits there shouldn't be real problems
			}
		}

		public static WizardSettings GetSettings()
		{
			return WizardSettingsManager.Instance.GetSettings(SettingsKey);
		}

		public static object GetSetting(string name)
		{
			if (!FindSetting(name))
			{
				return null;
			}

			return GetSettings()[name];
		}

		public static bool FindSetting(string name)
		{
			return GetSettings().Contains(name);
		}

		public static void UpdateSetting(string name, object value)
		{
			WizardSettings settings = WizardSettingsManager.Instance.GetSettings(SettingsKey);

			if (!settings.Contains(name))
			{
				settings.Add(name);
			}

			settings[name] = value;
		}

		#endregion

        #region · Specific Setting Handling Methods ·

        public static IDesignerHost GetDesignerHost()
		{
			return (IDesignerHost)GetSetting(DesignerHostKey);
		}

		public static FbDataAdapter GetDataAdapter()
		{
			return (FbDataAdapter)GetSetting(DataAdapterKey);
		}

		public static FbConnection GetConnection()
		{
			return (FbConnection)GetSetting(ConnectionKey);
		}

		public static void UpdateConnection(object connection)
		{
			UpdateSetting(ConnectionKey, (FbConnection)connection);
		}

		public static int GetCommandType()
		{
			return (int)GetSetting(CommandTypeKey);
		}

		public static void UpdateCommandType(object commandType)
		{
			UpdateSetting(CommandTypeKey, (int)commandType);
		}

		public static string GetCommandText()
		{
			return (string)GetSetting(CommandTextKey);
		}

		public static void UpdateCommandText(string commandText)
		{
			UpdateSetting(CommandTextKey, commandText);
		}

		public static bool GetDmlGeneration()
		{
			return (bool)GetSetting(DmlGenerationKey);
		}

		public static void UpdateDmlGeneration(bool generate)
		{
			UpdateSetting(DmlGenerationKey, generate);
		}

		public static string GetSelectStoredProcedure()
		{
			return (string)GetSetting(SelectSPKey);
		}

		public static void UpdateSelectStoredProcedure(string spName)
		{
			UpdateSetting(SelectSPKey, spName);
		}

		public static string GetInsertStoredProcedure()
		{
			return (string)GetSetting(InsertSPKey);
		}

		public static void UpdateInsertStoredProcedure(string spName)
		{
			UpdateSetting(InsertSPKey, spName);
		}

		public static string GetUpdateStoredProcedure()
		{
			return (string)GetSetting(UpdateSPKey);
		}

		public static void UpdateUpdateStoredProcedure(string spName)
		{
			UpdateSetting(UpdateSPKey, spName);
		}

		public static string GetDeleteStoredProcedure()
		{
			return (string)GetSetting(DeleteSPKey);
		}

		public static void UpdateDeleteStoredProcedure(string spName)
		{
			UpdateSetting(DeleteSPKey, spName);
		}

		public static bool GetUseQuotedIdentifiers()
		{
			return (bool)GetSetting(UseQuotedIdentifiers);
		}

		public static void UpdateUseQuotedIdentifiers(bool useQuotedIdentifiers)
		{
			UpdateSetting(UseQuotedIdentifiers, useQuotedIdentifiers);
		}

		#endregion
	}
}

#endif