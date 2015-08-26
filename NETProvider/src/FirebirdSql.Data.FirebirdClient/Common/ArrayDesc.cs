/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Common
{
	internal struct ArrayDesc
	{
		#region Fields

		private byte	dataType;
		private short	scale;
		private short	length;
		private string	fieldName;
		private string	relationName;
		private short	dimensions;
		private short	flags;
		private ArrayBound[] bounds;

		#endregion

		#region Properties

		public byte DataType
		{
			get { return dataType; }
			set { dataType = value; }
		}

		// Scale for numeric datatypes
		public short Scale
		{
			get { return scale; }
			set { scale = value; }
		}

		// Legth in bytes of each array element
		public short Length
		{
			get { return length; }
			set { length = value; }
		}

		// Column name - 32
		public string FieldName
		{
			get { return fieldName; }
			set { fieldName = value; }
		}

		// Table name -32
		public string RelationName
		{
			get { return relationName; }
			set { relationName = value; }
		}

		// Number of array dimensions 
		public short Dimensions
		{
			get { return dimensions; }
			set { dimensions = value; }
		}

		// Specifies wheter array is to be accesed in
		// row mayor or column-mayor order
		public short Flags
		{
			get { return flags; }
			set { flags = value; }
		}

		// Lower and upper bounds for each dimension - 16
		public ArrayBound[] Bounds
		{
			get { return bounds; }
			set { bounds = value; }
		}

		#endregion
	}
}
