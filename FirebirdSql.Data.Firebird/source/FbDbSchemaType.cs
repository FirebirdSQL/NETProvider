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

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/overview/*'/>
	public enum FbDbSchemaType
	{
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Character_Sets"]/*'/>
		Character_Sets,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Check_Constraints"]/*'/>
		Check_Constraints,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Check_Constraints_By_Table"]/*'/>
		Check_Constraints_By_Table,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Collations"]/*'/>
		Collations,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Columns"]/*'/>
		Columns,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Column_Privileges"]/*'/>
		Column_Privileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Domains"]/*'/>
		Domains,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Foreign_Keys"]/*'/>
		Foreign_Keys,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Functions"]/*'/>
		Functions,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Generators"]/*'/>
		Generators,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Indexes"]/*'/>
		Indexes,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Primary_Keys"]/*'/>
		Primary_Keys,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Procedure_Parameters"]/*'/>
		Procedure_Parameters,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Procedure_Privileges"]/*'/>
		Procedure_Privileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Procedures"]/*'/>
		Procedures,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Provider_Types"]/*'/>
		Provider_Types,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Roles"]/*'/>
		Roles,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Statistics"]/*'/>
		Statistics,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Tables"]/*'/>
		Tables,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Table_Constraint"]/*'/>
		Table_Constraint,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Table_Privileges"]/*'/>
		Table_Privileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Table_Statistics"]/*'/>
		Table_Statistics,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Triggers"]/*'/>
		Triggers,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Unique_Keys"]/*'/>
		Unique_Keys,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Usage_Privileges"]/*'/>
		Usage_Privileges,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="View_Column_Usage"]/*'/>
		View_Column_Usage,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="Views"]/*'/>
		Views,
		/// <include file='Doc/en_EN/FbDbSchemaType.xml' path='doc/enum[@name="FbDbSchemaType"]/field[@name="View_Privileges"]/*'/>
		View_Privileges
	}
}
