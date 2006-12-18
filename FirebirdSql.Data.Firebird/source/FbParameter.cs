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
using System.Data;
using System.ComponentModel;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/overview/*'/>
	[ParenthesizePropertyName(true),
	TypeConverter(typeof(Design.FbParameterConverter))]
	public sealed class FbParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region FIELDS
		
		FbDbType				fbType;
		ParameterDirection		direction;
		DataRowVersion			sourceVersion;
		bool					isNullable;
		string					parameterName;
		string					sourceColumn;
		object					value;
		byte					precision;
		byte					scale;
		int						size;
		bool					inferType;
		FbParameterCollection	parent;

		#endregion

		#region PROPERTIES

		string IDataParameter.ParameterName
		{
			get { return ParameterName; }
			set { ParameterName = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="ParameterName"]/*'/>
		[DefaultValue("")]
		public string ParameterName
		{
			get { return parameterName; }
			set { parameterName = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Precision"]/*'/>
		[Category("Data"), DefaultValue((byte)0)]
		public byte Precision
		{
			get { return precision; }
			set { precision = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Scale"]/*'/>
		[Category("Data"), DefaultValue((byte)0)]
		public byte Scale
		{
			get { return scale; }
			set { scale = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Size"]/*'/>
		[Category("Data"), DefaultValue(0)]
		public int Size
		{
			get { return size; }
			set { size = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="DbType"]/*'/>
		[Browsable(false),
		Category("Data"),
		RefreshProperties(RefreshProperties.All),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DbType DbType 
		{
			get { return fbTypeToDbType(fbType); }
			set { fbType = dbTypeToFbDbType(value); }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="FbDbType"]/*'/>
		[RefreshProperties(RefreshProperties.All),
		Category("Data"),
		DefaultValue(FbDbType.VarChar)]
		public FbDbType FbDbType
		{
			get { return fbType; }
			set { fbType = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Direction"]/*'/>
		[Category("Data"), DefaultValue(ParameterDirection.Input)]
		public ParameterDirection Direction 
		{
			get { return direction; }
			set { direction = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="IsNullable"]/*'/>
		[Browsable(false), 
		DesignOnly(true), 
		DefaultValue(false),
		EditorBrowsable(EditorBrowsableState.Advanced)]
		public Boolean IsNullable
		{
			get { return isNullable; }
			set { isNullable = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="SourceColumn"]/*'/>
		[Category("Data"), DefaultValue("")]
		public string SourceColumn
		{
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="SourceVersion"]/*'/>
		[Category("Data"), DefaultValue(DataRowVersion.Current)]
		public DataRowVersion SourceVersion 
		{
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Value"]/*'/>
		[Category("Data"), 
		TypeConverter(typeof(StringConverter)),
		DefaultValue(null)]
		public object Value
		{
			get { return this.value; }
			set 
			{ 
				if (value == null)
				{
					value = System.DBNull.Value;
				}
				this.value = value;
				if (this.inferType)
				{
					this.setFbDbType(value);
				}
			}
		}

		#endregion

		#region INTERNAL_PROPERTIES

		internal FbParameterCollection Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor"]/*'/>
		public FbParameter()
		{
			this.fbType			= FbDbType.VarChar;
			this.direction		= ParameterDirection.Input;
			this.sourceVersion	= DataRowVersion.Current;
			this.sourceColumn	= String.Empty;
			this.parameterName	= String.Empty;
			this.inferType		= true;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,System.Object)"]/*'/>
		public FbParameter(string parameterName, object value)  : this()
		{
			this.parameterName 	= parameterName;
			this.Value 			= value;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType) : this()
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= fbType;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType, int size) : this()
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= fbType;
			this.size			= size;			
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32,System.String)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType, int size, string sourceColumn) : this()
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= fbType;
			this.size			= size;
			this.sourceColumn	= sourceColumn;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32,System.Data.ParameterDirection,System.Boolean,System.Byte,System.Byte,System.String,System.Data.DataRowVersion,System.Object)"]/*'/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public FbParameter(string parameterName,
							FbDbType dbType,
							int size,
							ParameterDirection direction,
							bool isNullable,
							byte precision,
							byte scale,
							string sourceColumn,
							DataRowVersion sourceVersion,
							object value)
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= dbType;
			this.size			= size;
			this.direction		= direction;
			this.isNullable		= isNullable;
			this.precision		= precision;
			this.scale			= scale;
			this.sourceColumn	= sourceColumn;
			this.sourceVersion	= sourceVersion;
			this.value			= value;
		}

		#endregion

		#region ICLONEABLE_METHOD

		object ICloneable.Clone()
		{
			return new FbParameter(parameterName,
				fbType,
				size,
				direction,
				isNullable,
				precision,
				scale,
				sourceColumn,
				sourceVersion,
				value);
		}

		#endregion

		#region PRIVATE_METHODS

		private FbDbType dbTypeToFbDbType(DbType dbType)
		{			
			switch (dbType)
			{
				case DbType.String:
				case DbType.AnsiString:
					return FbDbType.VarChar;
				
				case DbType.StringFixedLength:
				case DbType.AnsiStringFixedLength:
					return FbDbType.Char;
				
				case DbType.Binary:
				case DbType.Object:
					return FbDbType.Binary;
				
				case DbType.Int16:
				case DbType.UInt16:
					return FbDbType.SmallInt;

				case DbType.Int32:
				case DbType.UInt32:
					return FbDbType.Integer;

				case DbType.Int64:
				case DbType.UInt64:
					return FbDbType.BigInt;

				case DbType.Decimal:
					return FbDbType.Decimal;

				case DbType.Double:
					return FbDbType.Double;

				case DbType.Single:
					return FbDbType.Float;

				case DbType.Date:
					return FbDbType.Date;

				case DbType.Time:
					return FbDbType.Time;

				case DbType.DateTime:
					return FbDbType.TimeStamp;

				default:
					throw new SystemException("Invalid data type");
			}
		}

		private DbType fbTypeToDbType(FbDbType fbType)
		{
			switch (fbType)
			{
				case FbDbType.Char:
				case FbDbType.Text:
				case FbDbType.VarChar:
					return DbType.String;
				
				case FbDbType.Array:
				case FbDbType.Binary:
				case FbDbType.LongVarBinary:
					return DbType.Binary;

				case FbDbType.SmallInt:
					return DbType.Int16;

				case FbDbType.Integer:
					return DbType.Int32;

				case FbDbType.BigInt:
					return DbType.Int64;

				case FbDbType.Float:
					return DbType.Single;

				case FbDbType.Double:
					return DbType.Double;

				case FbDbType.Date:
					return DbType.Date;

				case FbDbType.Time:
					return DbType.Time;

				case FbDbType.TimeStamp:
					return DbType.DateTime;

				case FbDbType.Decimal:
				case FbDbType.Numeric:
					return DbType.Decimal;

				default:
					throw new SystemException("Invalid data type");
			}			
		}

		private void setFbDbType(object value)
		{
			if (value == null)
			{
				value = System.DBNull.Value;
			}

			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.DBNull:
					fbType = FbDbType.Char;
					break;

				case TypeCode.Object:
					fbType = FbDbType.Binary;
					break;

				case TypeCode.Char:
					fbType = FbDbType.Char;
					break;

				case TypeCode.Int16:
					fbType = FbDbType.SmallInt;
					break;

				case TypeCode.Int32:
					fbType = FbDbType.Integer;
					break;

				case TypeCode.Int64:
					fbType = FbDbType.BigInt;
					break;

				case TypeCode.Single:
					fbType = FbDbType.Float;
					break;

				case TypeCode.Double:
					fbType = FbDbType.Double;
					break;

				case TypeCode.Decimal:
					fbType = FbDbType.Decimal;
					break;

				case TypeCode.DateTime:
					fbType = FbDbType.TimeStamp;
					break;

				case TypeCode.String:
					fbType = FbDbType.Char;
					break;

				case TypeCode.Empty:
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				default:
					throw new SystemException("Value is of unknown data type");
			}
		}
			
		private TypeCode fbTypeToTypeCode(FbDbType fbType)
		{
			switch (fbType)
			{
				case FbDbType.BigInt:
					return Type.GetTypeCode(typeof(long));

				case FbDbType.Binary:
					return Type.GetTypeCode(typeof(byte[]));

				case FbDbType.Char:
					return Type.GetTypeCode(typeof(string));

				case FbDbType.Date:
					return Type.GetTypeCode(typeof(DateTime));

				case FbDbType.Time:
					return Type.GetTypeCode(typeof(DateTime));

				case FbDbType.TimeStamp:
					return Type.GetTypeCode(typeof(DateTime));

				case FbDbType.Decimal:
					return Type.GetTypeCode(typeof(decimal));

				case FbDbType.Double:
					return Type.GetTypeCode(typeof(double));

				case FbDbType.Integer:
					return Type.GetTypeCode(typeof(int));

				case FbDbType.LongVarBinary:
					return Type.GetTypeCode(typeof(byte[]));

				case FbDbType.Text:
					return Type.GetTypeCode(typeof(string));

				case FbDbType.Numeric:
					return Type.GetTypeCode(typeof(decimal));

				case FbDbType.Float:
					return Type.GetTypeCode(typeof(float));

				case FbDbType.SmallInt:
					return Type.GetTypeCode(typeof(short));

				case FbDbType.VarChar:
					return Type.GetTypeCode(typeof(string));

				default:
					throw new SystemException("Invalid data type");
			}
		}

		/// <include file='Doc/en_EN/FbCommand.xml' path='doc/class[@name="FbParameter"]/method[@name="ToString"]/*'/>
		public override string ToString()
		{
			return this.parameterName;
		}

		#endregion
	}
}