/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbDbSchemaFactory
	{
		public static IDbSchema GetSchema(FbDbSchemaType schema)
		{
			IDbSchema returnSchema = null;

			switch (schema)
			{
				case FbDbSchemaType.Character_Sets:
					returnSchema = new FbCharacterSetsSchema();
					break;

				case FbDbSchemaType.Check_Constraints:
					returnSchema = new FbCheckConstraintsSchema();
					break;

				case FbDbSchemaType.Check_Constraints_By_Table:
					returnSchema = new FbChecksByTableSchema();
					break;

				case FbDbSchemaType.Collations:
					returnSchema = new FbCollationsSchema();
					break;

				case FbDbSchemaType.Columns:
					returnSchema = new FbColumnsSchema();
					break;

				case FbDbSchemaType.Column_Privileges:
					returnSchema = new FbColumnPrivilegesSchema();
					break;

				case FbDbSchemaType.Domains:
					returnSchema = new FbDomainsSchema();
					break;

				case FbDbSchemaType.Foreign_Keys:
					returnSchema = new FbForeignKeysSchema();
					break;

				case FbDbSchemaType.Functions:
					returnSchema = new FbFunctionsSchema();
					break;

				case FbDbSchemaType.Generators:
					returnSchema = new FbGeneratorsSchema();
					break;

				case FbDbSchemaType.Indexes:
					returnSchema = new FbIndexesSchema();
					break;

				case FbDbSchemaType.Primary_Keys: 
					returnSchema = new FbPrimaryKeysSchema();
					break;

				case FbDbSchemaType.Procedures:
					returnSchema = new FbProceduresSchema();
					break;

				case FbDbSchemaType.Procedure_Parameters: 
					returnSchema = new FbProcedureParametersSchema();
					break;

				case FbDbSchemaType.Procedure_Privileges:
					returnSchema = new FbProcedurePrivilegesSchema();
					break;

				case FbDbSchemaType.Provider_Types:
					returnSchema = new FbProviderTypesSchema();
					break;

				case FbDbSchemaType.Roles:
					returnSchema = new FbRolesSchema();
					break;

				case FbDbSchemaType.Statistics: 
					break;

				case FbDbSchemaType.Tables: 
					returnSchema = new FbTablesSchema();
					break;

				case FbDbSchemaType.Table_Constraint:
					returnSchema = new FbTableConstraintsSchema();
					break;

				case FbDbSchemaType.Table_Privileges:
					returnSchema = new FbTablePrivilegesSchema();					
					break;

				case FbDbSchemaType.Table_Statistics:
					break;

				case FbDbSchemaType.Triggers: 
					returnSchema = new FbTriggersSchema();
					break;

				case FbDbSchemaType.Unique_Keys:
					returnSchema = new FbUniqueKeysSchema();
					break;

				case FbDbSchemaType.View_Column_Usage:
					returnSchema = new FbViewColumnUsage();
					break;

				case FbDbSchemaType.Views:
					returnSchema = new FbViewsSchema();
					break;

				case FbDbSchemaType.View_Privileges:
					returnSchema = new FbViewPrivilegesSchema();
					break;
			}

			return returnSchema;
		}
	}
}