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
 * 
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	[ParenthesizePropertyName(true)]
	public sealed class FbParameter : DbParameter, ICloneable
	{
		#region Fields

		private FbParameterCollection parent;
		private FbDbType fbDbType;
		private ParameterDirection direction;
		private DataRowVersion sourceVersion;
		private FbCharset charset;
		private bool isNullable;
		private bool sourceColumnNullMapping;
		private byte precision;
		private byte scale;
		private int size;
		private object value;
		private string parameterName;
		private string sourceColumn;

		#endregion

		#region DbParameter properties

		[DefaultValue("")]
		public override string ParameterName
		{
			get { return this.parameterName; }
			set { this.parameterName = value; }
		}

		[Category("Data")]
		[DefaultValue(0)]
		public override int Size
		{
			get
			{
				return (this.HasSize ? this.size : this.RealValueSize ?? 0);
			}
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Size");

				this.size = value;

				// Hack for Clob parameters
				if (value == 2147483647 &&
					(this.FbDbType == FbDbType.VarChar || this.FbDbType == FbDbType.Char))
				{
					this.FbDbType = FbDbType.Text;
				}
			}
		}

		[Category("Data")]
		[DefaultValue(ParameterDirection.Input)]
		public override ParameterDirection Direction
		{
			get { return this.direction; }
			set { this.direction = value; }
		}

		[Browsable(false)]
		[DesignOnly(true)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override bool IsNullable
		{
			get { return this.isNullable; }
			set { this.isNullable = value; }
		}

		[Category("Data")]
		[DefaultValue("")]
		public override string SourceColumn
		{
			get { return this.sourceColumn; }
			set { this.sourceColumn = value; }
		}

		[Category("Data")]
		[DefaultValue(DataRowVersion.Current)]
		public override DataRowVersion SourceVersion
		{
			get { return this.sourceVersion; }
			set { this.sourceVersion = value; }
		}

		[Browsable(false)]
		[Category("Data")]
		[RefreshProperties(RefreshProperties.All)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override DbType DbType
		{
			get { return TypeHelper.GetDbType((DbDataType)this.fbDbType); }
			set { this.FbDbType = (FbDbType)TypeHelper.GetDbDataType(value); }
		}

		[RefreshProperties(RefreshProperties.All)]
		[Category("Data")]
		[DefaultValue(FbDbType.VarChar)]
		public FbDbType FbDbType
		{
			get { return this.fbDbType; }
			set
			{
				this.fbDbType = value;
				this.IsTypeSet = true;
			}
		}

		[Category("Data")]
		[TypeConverter(typeof(StringConverter)), DefaultValue(null)]
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

				if (!this.IsTypeSet)
				{
					this.SetFbDbType(value);
				}
			}
		}

		[Category("Data")]
		[DefaultValue(FbCharset.Default)]
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

		#region Properties

		[Category("Data")]
		[DefaultValue((byte)0)]
		public byte Precision
		{
			get { return this.precision; }
			set { this.precision = value; }
		}

		[Category("Data")]
		[DefaultValue((byte)0)]
		public byte Scale
		{
			get { return this.scale; }
			set { this.scale = value; }
		}

		#endregion

		#region Internal Properties

		internal FbParameterCollection Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		internal string InternalParameterName
		{
			get
			{
				return NormalizeParameterName(this.parameterName);
			}
		}

		internal bool IsTypeSet { get; private set; }

		internal object InternalValue
		{
			get
			{
				string svalue = (this.value as string);
				if (svalue != null)
				{
					return svalue.Substring(0, Math.Min(this.Size, svalue.Length));
				}
				byte[] bvalue = (this.value as byte[]);
				if (bvalue != null)
				{
					byte[] result = new byte[Math.Min(this.Size, bvalue.Length)];
					Array.Copy(bvalue, result, result.Length);
					return result;
				}
				return this.value;
			}
		}

		internal bool HasSize
		{
			get { return this.size != default(int); }
		}

		#endregion

		#region Constructors

		public FbParameter()
		{
			this.fbDbType = FbDbType.VarChar;
			this.direction = ParameterDirection.Input;
			this.sourceVersion = DataRowVersion.Current;
			this.sourceColumn = string.Empty;
			this.parameterName = string.Empty;
			this.charset = FbCharset.Default;
		}

		public FbParameter(string parameterName, object value)
			: this()
		{
			this.parameterName = parameterName;
			this.Value = value;
		}

		public FbParameter(string parameterName, FbDbType fbType)
			: this()
		{
			this.parameterName = parameterName;
			this.FbDbType = fbType;
		}

		public FbParameter(string parameterName, FbDbType fbType, int size)
			: this()
		{
			this.parameterName = parameterName;
			this.FbDbType = fbType;
			this.Size = size;
		}

		public FbParameter(string parameterName, FbDbType fbType, int size, string sourceColumn)
			: this()
		{
			this.parameterName = parameterName;
			this.FbDbType = fbType;
			this.Size = size;
			this.sourceColumn = sourceColumn;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public FbParameter(
			string parameterName,
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
			this.parameterName = parameterName;
			this.FbDbType = dbType;
			this.Size = size;
			this.direction = direction;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;
			this.sourceColumn = sourceColumn;
			this.sourceVersion = sourceVersion;
			this.Value = value;
			this.charset = FbCharset.Default;
		}

		#endregion

		#region ICloneable Methods

		object ICloneable.Clone()
		{
			return new FbParameter(
				this.parameterName,
				this.fbDbType,
				this.size,
				this.direction,
				this.isNullable,
				this.precision,
				this.scale,
				this.sourceColumn,
				this.sourceVersion,
				this.value)
				{
					Charset = this.charset
				};
		}

		#endregion

		#region DbParameter methods

		public override string ToString()
		{
			return this.parameterName;
		}

		public override void ResetDbType()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Private Methods

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

		#region Private Properties

		private int? RealValueSize
		{
			get
			{
				string svalue = (value as string);
				if (svalue != null)
				{
					return svalue.Length;
				}
				byte[] bvalue = (value as byte[]);
				if (bvalue != null)
				{
					return bvalue.Length;
				}
				return null;
			}
		}

		#endregion

		#region Static Methods

		internal static string NormalizeParameterName(string parameterName)
		{
			return !string.IsNullOrEmpty(parameterName) && !parameterName.StartsWith("@")
				? string.Format("@{0}", parameterName)
				: parameterName;
		}

		#endregion
	}
}
