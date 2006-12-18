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

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="T:FbParameter"]/*'/>
	public sealed class FbParameter : MarshalByRefObject, IDataParameter, 
										IDbDataParameter, ICloneable
	{
		#region FIELDS
		
		FbType				fbType;
		bool				fbTypeSet;	
		ParameterDirection	direction;
		DataRowVersion		sourceVersion;
		bool				nullable;
		string				paramName;
		string				sourceColumn;
		object				paramValue;
		byte				paramPrecision;
		byte				paramScale;
		int					paramSize;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:Precision"]/*'/>
		public byte Precision
		{
			get { return paramPrecision; }
			set { paramPrecision = value; }
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:Scale"]/*'/>
		public byte Scale
		{
			get { return paramScale; }
			set { paramScale = value; }
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:Size"]/*'/>
		public int Size
		{
			get { return paramSize; }
			set { paramSize = value; }
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:DbType"]/*'/>
		public DbType DbType 
		{
			get  
			{ 
				if (!fbTypeSet) 
				{
					throw new InvalidOperationException("DbType has not been set yet.");
				}
				return FbTypeToDbType(fbType); 
			}
			set  
			{ 
				fbType		= DbTypeToFbType(value); 
				fbTypeSet	= true; 
			}
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:Direction"]/*'/>
		public ParameterDirection Direction 
		{
			get { return direction; }
			set { direction = value; }
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:IsNullable"]/*'/>
		public Boolean IsNullable 
		{
			get { return nullable; }
			set { nullable = value; }  
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:ParameterName"]/*'/>
		public String ParameterName 
		{
			get { return paramName; }
			set { paramName = value; }
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:SourceColumn"]/*'/>
		public String SourceColumn 
		{
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:SourceVersion"]/*'/>
		public DataRowVersion SourceVersion 
		{
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="P:Value"]/*'/>
		public object Value 
		{
			get { return paramValue; }
			set
			{ 
				if (fbTypeSet && !(value is Array))
				{
					if (value != null && !Convert.IsDBNull(value))
					{
						paramValue = Convert.ChangeType(value, FbTypeToTypeCode(fbType));
					}
					else
					{
						paramValue = DBNull.Value;
					}
				}
				else
				{
					paramValue = value;
					if (paramValue == null || Convert.IsDBNull(paramValue))
					{
						throw new ArgumentException("Type infering from null or DBNull is not supported.", "paramValue");
					}
					fbType		= GetFbType(paramValue); 
					fbTypeSet	= true;
				}
			}
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public FbParameter()
		{
			fbTypeSet		= false;	
			direction		= ParameterDirection.Input;
			sourceVersion	= DataRowVersion.Current;
			nullable		= false;
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="M:#ctor(System.String,FirebirdSql.Data.Firebird.FbType)"]/*'/>
		public FbParameter(string parameterName, FbType fbType) : this()
		{
			this.paramName	= parameterName;
			this.fbType		= fbType;
			this.fbTypeSet	= true;
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="M:#ctor(System.String,System.Object)"]/*'/>
		public FbParameter(string parameterName, object paramValue)  : this()
		{
			this.paramName = parameterName;
			// Setting the value also infers the type.
			this.Value = paramValue;
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="M:#ctor(System.String,FirebirdSql.Data.Firebird.FbType,System.String)"]/*'/>
		public FbParameter(string parameterName, FbType fbType, string sourceColumn) : this()
		{
			this.paramName		= parameterName;
			this.fbType			= fbType;
			this.sourceColumn	= sourceColumn;
			this.fbTypeSet		= true;
		}

		/// <include file='xmldoc/fbparameter.xml' path='doc/member[@name="M:#ctor(System.String,FirebirdSql.Data.Firebird.FbType,System.Int32,System.String)"]/*'/>
		public FbParameter(string parameterName, FbType fbType, 
							int size, string sourceColumn) : this()
		{
			this.paramName		= parameterName;
			this.fbType			= fbType;
			this.fbTypeSet		= true;
			this.sourceColumn	= sourceColumn;
			this.paramSize		= size;
		}

		#endregion

		#region ICLONEABLE_METHOD

		object ICloneable.Clone()
		{
			FbParameter parameter = new FbParameter(ParameterName, fbType, Size, SourceColumn);
			
			parameter.SourceVersion = this.SourceVersion;
			parameter.Direction		= this.Direction;
			parameter.DbType		= this.DbType;
			parameter.IsNullable	= this.IsNullable;
			parameter.Precision		= this.Precision;
			parameter.Scale			= this.Scale;
			parameter.Value			= this.Value;


			return parameter;
		}

		#endregion

		#region METHODS

		private FbType DbTypeToFbType(DbType dbType)
		{			
			switch(dbType)
			{
				case DbType.Boolean:
				case DbType.Byte:
				case DbType.Currency:
				case DbType.Guid:
				case DbType.VarNumeric:					
				case DbType.SByte:
					throw new SystemException("Invalid data type");

				case DbType.AnsiString:
					return FbType.VarChar;
				
				case DbType.AnsiStringFixedLength:
					return FbType.Char;
				
				case DbType.Binary:
					return FbType.Binary;
				
				case DbType.Date:
					return FbType.Date;

				case DbType.DateTime:
					return FbType.TimeStamp;

				case DbType.Decimal:
					return FbType.Decimal;

				case DbType.Double:
					return FbType.Double;

				case DbType.Int16:
					return FbType.SmallInt;

				case DbType.Int32:
					return FbType.Integer;

				case DbType.Int64:
					return FbType.BigInt;

				case DbType.Object:
					return FbType.Binary;

				case DbType.Single:
					return FbType.Float;

				case DbType.String:
					return FbType.VarChar;

				case DbType.StringFixedLength:
					return FbType.Char;

				case DbType.Time:
					return FbType.Time;

				case DbType.UInt16:
					return FbType.SmallInt;

				case DbType.UInt32:
					return FbType.Integer;

				case DbType.UInt64:
					return  FbType.BigInt;

				default:
					throw new SystemException("Invalid data type");
			}
		}

		private DbType FbTypeToDbType(FbType fbType)
		{
			switch(fbType)
			{				
				case FbType.BigInt:
					return DbType.Int64;
				
				case FbType.Binary:
					return DbType.Binary;

				case FbType.Char:
					return DbType.AnsiString;

				case FbType.Date:
					return DbType.Date;

				case FbType.Time:
					return DbType.Time;

				case FbType.TimeStamp:
					return DbType.DateTime;

				case FbType.Decimal:
					return DbType.Decimal;

				case FbType.Double:
					return DbType.Double;

				case FbType.Integer:
					return DbType.Int32;

				case FbType.LongVarBinary:
					return DbType.Binary;

				case FbType.Text:
					return DbType.String;

				case FbType.NText:
					return DbType.AnsiString;

				case FbType.Numeric:
					return DbType.Decimal;

				case FbType.Float:
					return DbType.Single;

				case FbType.SmallInt:
					return DbType.Int16;

				case FbType.VarChar:
					return DbType.String;

				case FbType.NVarChar:
					return DbType.String;

				case FbType.NChar:
					return DbType.String;

				default:
					throw new SystemException("Invalid data type");
			}			
		}

		private FbType GetFbType(Object value)
		{
			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Empty:
					throw new SystemException("Invalid data type");

				case TypeCode.DBNull:				
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					// Throw a SystemException for unsupported data types.
					throw new SystemException("Invalid data type");

				case TypeCode.Object:
					return FbType.Binary;

				case TypeCode.Char:
					return FbType.Char;

				case TypeCode.Int16:
					return FbType.SmallInt;

				case TypeCode.Int32:
					return FbType.Integer;

				case TypeCode.Int64:
					return FbType.BigInt;

				case TypeCode.Single:
					return FbType.Float;

				case TypeCode.Double:
					return FbType.Double;

				case TypeCode.Decimal:
					return FbType.Decimal;

				case TypeCode.DateTime:
					return FbType.TimeStamp;

				case TypeCode.String:
					return FbType.Char;

				default:
					throw new SystemException("Value is of unknown data type");
			}
		}
			
		private TypeCode FbTypeToTypeCode(FbType fbType)
		{
			switch (fbType)
			{
				case FbType.Array:
					return Type.GetTypeCode(typeof(System.Array));
					
				case FbType.BigInt:
					return Type.GetTypeCode(typeof(long));

				case FbType.Binary:
					return Type.GetTypeCode(typeof(byte[]));

				case FbType.Char:
					return Type.GetTypeCode(typeof(string));

				case FbType.Date:
					return Type.GetTypeCode(typeof(DateTime));

				case FbType.Time:
					return Type.GetTypeCode(typeof(DateTime));

				case FbType.TimeStamp:
					return Type.GetTypeCode(typeof(DateTime));

				case FbType.Decimal:
					return Type.GetTypeCode(typeof(decimal));

				case FbType.Double:
					return Type.GetTypeCode(typeof(double));

				case FbType.Integer:
					return Type.GetTypeCode(typeof(int));

				case FbType.LongVarBinary:
					return Type.GetTypeCode(typeof(byte[]));

				case FbType.Text:
					return Type.GetTypeCode(typeof(string));

				case FbType.NText:
					return Type.GetTypeCode(typeof(string));

				case FbType.Numeric:
					return Type.GetTypeCode(typeof(decimal));

				case FbType.Float:
					return Type.GetTypeCode(typeof(float));

				case FbType.SmallInt:
					return Type.GetTypeCode(typeof(short));

				case FbType.VarChar:
					return Type.GetTypeCode(typeof(string));

				case FbType.NVarChar:
					return Type.GetTypeCode(typeof(string));

				case FbType.NChar:
					return Type.GetTypeCode(typeof(string));

				default:
					throw new SystemException("Invalid data type");
			}
		}

		#endregion
	}
}
