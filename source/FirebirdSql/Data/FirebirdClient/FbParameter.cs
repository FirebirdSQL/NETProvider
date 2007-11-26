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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/overview/*'/>
#if	(NET)
	[ParenthesizePropertyName(true)]
#endif
	public sealed class FbParameter : DbParameter, ICloneable
	{
		#region · Fields ·

		private FbParameterCollection parent;
		private FbDbType			fbDbType;
		private ParameterDirection	direction;
		private DataRowVersion		sourceVersion;
		private FbCharset			charset;
		private bool				isNullable;
		private bool				sourceColumnNullMapping;
		private byte				precision;
		private byte				scale;
		private int					size;
		private object				value;
		private string				parameterName;
		private string				sourceColumn;
        private bool                isTypeSet;

		#endregion

		#region · DbParameter properties ·

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="ParameterName"]/*'/>
#if	(!NETCF)
		[DefaultValue("")]
#endif
		public override string ParameterName
		{
			get { return this.parameterName; }
			set { this.parameterName = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Size"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[DefaultValue(0)]
#endif
		public override int Size
		{
			get { return this.size; }
			set 
            { 
                this.size = value;

                // Hack for Clob parameters
                if (value == 2147483647 &&
                    (this.FbDbType == FbDbType.VarChar || this.FbDbType == FbDbType.Char))
                {
                    this.FbDbType = FbDbType.Text;
                }
            }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Direction"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[DefaultValue(ParameterDirection.Input)]
#endif
		public override ParameterDirection Direction
		{
			get { return this.direction; }
			set { this.direction = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="IsNullable"]/*'/>
#if	(!NETCF)
		[Browsable(false)]
		[DesignOnly(true)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
		public override bool IsNullable
		{
			get { return this.isNullable; }
			set { this.isNullable = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="SourceColumn"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[DefaultValue("")]
#endif
		public override string SourceColumn
		{
			get { return this.sourceColumn; }
			set { this.sourceColumn = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="SourceVersion"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[DefaultValue(DataRowVersion.Current)]
#endif
		public override DataRowVersion SourceVersion
		{
			get { return this.sourceVersion; }
			set { this.sourceVersion = value; }
		}

        /// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="DbType"]/*'/>
#if	(!NETCF)
        [Browsable(false)]
        [Category("Data")]
        [RefreshProperties(RefreshProperties.All)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public override DbType DbType
        {
            get { return TypeHelper.GetDbType((DbDataType)this.fbDbType); }
            set { this.FbDbType = (FbDbType)TypeHelper.GetDbDataType(value); }
        }

        /// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="FbDbType"]/*'/>
#if	(!NETCF)
        [RefreshProperties(RefreshProperties.All)]
        [Category("Data")]
        [DefaultValue(FbDbType.VarChar)]
#endif
        public FbDbType FbDbType
        {
            get { return this.fbDbType; }
            set 
            { 
                this.fbDbType     = value;
                this.isTypeSet  = true;
            }
        }

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Value"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[TypeConverter(typeof(StringConverter)), DefaultValue(null)]
#endif
		public override object Value
		{
			get { return this.value; }
			set
			{
				if (value == null)
				{
					value = System.DBNull.Value;
				}

				if (this.FbDbType == FbDbType.Guid && value != null && 
                    value != DBNull.Value && !(value is Guid) && !(value is byte[]))
				{
					throw new InvalidOperationException("Incorrect Guid value.");
				}

				this.value = value;

				if (!this.isTypeSet)
				{
					this.SetFbDbType(value);
				}
			}
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Charset"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[DefaultValue(FbCharset.Default)]
#endif
		public FbCharset Charset
		{
			get { return this.charset; }
			set { this.charset = value; }
		}

		public override bool SourceColumnNullMapping
		{
			get { return this.sourceColumnNullMapping; }
			set { this.sourceColumnNullMapping = value; }
		}

		#endregion

		#region · Properties ·

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Precision"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[DefaultValue((byte)0)]
#endif
		public byte Precision
		{
			get { return this.precision; }
			set { this.precision = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Scale"]/*'/>
#if	(!NETCF)
		[Category("Data")]
		[DefaultValue((byte)0)]
#endif
		public byte Scale
		{
			get { return this.scale; }
			set { this.scale = value; }
		}

		#endregion

		#region · Internal Properties ·

		internal FbParameterCollection Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

        internal string InternalParameterName
        {
            get
            {
                if (!String.IsNullOrEmpty(this.parameterName) && !this.parameterName.StartsWith("@"))
                {
                    return String.Format("@{0}", this.ParameterName);
                }

                return this.ParameterName;
            }
        }

		#endregion

		#region · Constructors ·

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor"]/*'/>
		public FbParameter()
		{
			this.fbDbType		= FbDbType.VarChar;
			this.direction		= ParameterDirection.Input;
			this.sourceVersion	= DataRowVersion.Current;
			this.sourceColumn	= String.Empty;
			this.parameterName	= String.Empty;
			this.charset		= FbCharset.Default;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,System.Object)"]/*'/>
		public FbParameter(string parameterName, object value) : this()
		{
			this.parameterName	= parameterName;
			this.Value			= value;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType) : this()
		{
			this.parameterName	= parameterName;
			this.FbDbType		= fbType;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType, int size) : this()
		{
			this.parameterName	= parameterName;
			this.FbDbType		= fbType;
			this.size			= size;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32,System.String)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType, int size, string sourceColumn)
			: this()
		{
			this.parameterName	= parameterName;
			this.FbDbType		= fbType;
			this.size			= size;
			this.sourceColumn	= sourceColumn;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32,System.Data.ParameterDirection,System.Boolean,System.Byte,System.Byte,System.String,System.Data.DataRowVersion,System.Object)"]/*'/>
#if	(!NETCF)
		[EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
		public FbParameter(
			string				parameterName, 
			FbDbType			dbType, 
			int					size,
			ParameterDirection	direction,
			bool				isNullable,
			byte				precision,
			byte				scale,
			string				sourceColumn,
			DataRowVersion		sourceVersion,
			object				value)
		{
			this.parameterName	= parameterName;
			this.FbDbType		= dbType;
			this.size			= size;
			this.direction		= direction;
			this.isNullable		= isNullable;
			this.precision		= precision;
			this.scale			= scale;
			this.sourceColumn	= sourceColumn;
			this.sourceVersion	= sourceVersion;
			this.value			= value;
            this.charset        = FbCharset.Default;
		}

		#endregion

		#region · ICloneable Methods ·

		object ICloneable.Clone()
		{
			FbParameter p = new FbParameter(
				this.parameterName,
				this.fbDbType,
				this.size,
				this.direction,
				this.isNullable,
				this.precision,
				this.scale,
				this.sourceColumn,
				this.sourceVersion,
				this.value);

			// Set extra properties
			p.Charset = this.charset;

			return p;
		}

		#endregion

		#region · DbParameter methods ·

		/// <include file='Doc/en_EN/FbCommand.xml'	path='doc/class[@name="FbParameter"]/method[@name="ToString"]/*'/>
		public override string ToString()
		{
			return this.parameterName;
		}

		public override void ResetDbType()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region · Private Methods ·

		private void SetFbDbType(object value)
		{
			if (value == null)
			{
				value = System.DBNull.Value;
			}

			TypeCode code = Type.GetTypeCode(value.GetType());

			switch (code)
			{
				case TypeCode.Char:
					this.fbDbType = FbDbType.Char;
					break;

				case TypeCode.DBNull:
				case TypeCode.String:
					this.fbDbType = FbDbType.VarChar;
					break;

				case TypeCode.Boolean:
                    this.fbDbType = FbDbType.Boolean;
                    break;

				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
					this.fbDbType = FbDbType.SmallInt;
					break;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					this.fbDbType = FbDbType.Integer;
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					this.fbDbType = FbDbType.BigInt;
					break;

				case TypeCode.Single:
					this.fbDbType = FbDbType.Float;
					break;

				case TypeCode.Double:
					this.fbDbType = FbDbType.Double;
					break;

				case TypeCode.Decimal:
					this.fbDbType = FbDbType.Decimal;
					break;

				case TypeCode.DateTime:
					this.fbDbType = FbDbType.TimeStamp;
					break;

				case TypeCode.Empty:
				default:
					if (value is Guid)
					{
						this.fbDbType = FbDbType.Guid;
					}
                    else if (code == TypeCode.Object)
                    {
                        this.fbDbType = FbDbType.Binary;
                    }
                    else
					{
						throw new SystemException("Value is of unknown data type");
					}
					break;
			}
		}

		#endregion
	}
}