/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

using FirebirdSql.Data.Common;
using System.Text;

namespace FirebirdSql.Data.FirebirdClient
{
	[ParenthesizePropertyName(true)]
	public sealed class FbParameter : DbParameter
#if !NETSTANDARD1_6
		, ICloneable
#endif
	{
		#region Fields

		private FbParameterCollection _parent;
		private FbDbType _fbDbType;
		private ParameterDirection _direction;
#if !NETSTANDARD1_6
		private DataRowVersion _sourceVersion;
#endif
		private FbCharset _charset;
		private bool _isNullable;
		private bool _sourceColumnNullMapping;
		private byte _precision;
		private byte _scale;
		private int _size;
		private object _value;
		private string _parameterName;
		private string _sourceColumn;
		private string _internalParameterName;
		private bool _isUnicodeParameterName;

		#endregion

		#region DbParameter properties

		[DefaultValue("")]
		public override string ParameterName
		{
			get { return _parameterName; }
			set
			{
				_parameterName = value;
				_internalParameterName = NormalizeParameterName(_parameterName);
				_isUnicodeParameterName = IsNonAsciiParameterName(_parameterName);
				_parent?.ParameterNameChanged();
			}
		}

		[Category("Data")]
		[DefaultValue(0)]
		public override int Size
		{
			get
			{
				return (HasSize ? _size : RealValueSize ?? 0);
			}
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException();

				_size = value;

				// Hack for Clob parameters
				if (value == 2147483647 &&
					(FbDbType == FbDbType.VarChar || FbDbType == FbDbType.Char))
				{
					FbDbType = FbDbType.Text;
				}
			}
		}

		[Category("Data")]
		[DefaultValue(ParameterDirection.Input)]
		public override ParameterDirection Direction
		{
			get { return _direction; }
			set { _direction = value; }
		}

		[Browsable(false)]
		[DesignOnly(true)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override bool IsNullable
		{
			get { return _isNullable; }
			set { _isNullable = value; }
		}

		[Category("Data")]
		[DefaultValue("")]
		public override string SourceColumn
		{
			get { return _sourceColumn; }
			set { _sourceColumn = value; }
		}

#if !NETSTANDARD1_6
		[Category("Data")]
		[DefaultValue(DataRowVersion.Current)]
		public override DataRowVersion SourceVersion
		{
			get { return _sourceVersion; }
			set { _sourceVersion = value; }
		}
#endif

		[Browsable(false)]
		[Category("Data")]
		[RefreshProperties(RefreshProperties.All)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override DbType DbType
		{
			get { return TypeHelper.GetDbTypeFromDbDataType((DbDataType)_fbDbType); }
			set { FbDbType = (FbDbType)TypeHelper.GetDbDataTypeFromDbType(value); }
		}

		[RefreshProperties(RefreshProperties.All)]
		[Category("Data")]
		[DefaultValue(FbDbType.VarChar)]
		public FbDbType FbDbType
		{
			get { return _fbDbType; }
			set
			{
				_fbDbType = value;
				IsTypeSet = true;
			}
		}

		[Category("Data")]
		[TypeConverter(typeof(StringConverter)), DefaultValue(null)]
		public override object Value
		{
			get { return _value; }
			set
			{
				if (value == null)
				{
					value = DBNull.Value;
				}

				if (FbDbType == FbDbType.Guid && value != null &&
					value != DBNull.Value && !(value is Guid) && !(value is byte[]))
				{
					throw new InvalidOperationException("Incorrect Guid value.");
				}

				_value = value;

				if (!IsTypeSet)
				{
					SetFbDbType(value);
				}
			}
		}

		[Category("Data")]
		[DefaultValue(FbCharset.Default)]
		public FbCharset Charset
		{
			get { return _charset; }
			set { _charset = value; }
		}

		public override bool SourceColumnNullMapping
		{
			get { return _sourceColumnNullMapping; }
			set { _sourceColumnNullMapping = value; }
		}

		#endregion

		#region Properties

		[Category("Data")]
		[DefaultValue((byte)0)]
		public override byte Precision
		{
			get { return _precision; }
			set { _precision = value; }
		}

		[Category("Data")]
		[DefaultValue((byte)0)]
		public override byte Scale
		{
			get { return _scale; }
			set { _scale = value; }
		}

		#endregion

		#region Internal Properties

		internal FbParameterCollection Parent
		{
			get { return _parent; }
			set
			{
				_parent?.ParameterNameChanged();
				_parent = value;
				_parent?.ParameterNameChanged();
			}
		}

		internal string InternalParameterName
		{
			get
			{
				return _internalParameterName;
			}
		}

		internal bool IsTypeSet { get; private set; }

		internal object InternalValue
		{
			get
			{
				var svalue = (_value as string);
				if (svalue != null)
				{
					return svalue.Substring(0, Math.Min(Size, svalue.Length));
				}
				var bvalue = (_value as byte[]);
				if (bvalue != null)
				{
					var result = new byte[Math.Min(Size, bvalue.Length)];
					Array.Copy(bvalue, result, result.Length);
					return result;
				}
				return _value;
			}
		}

		internal bool HasSize
		{
			get { return _size != default; }
		}

		#endregion

		#region Constructors

		public FbParameter()
		{
			_fbDbType = FbDbType.VarChar;
			_direction = ParameterDirection.Input;
#if !NETSTANDARD1_6
			_sourceVersion = DataRowVersion.Current;
#endif
			_sourceColumn = string.Empty;
			_parameterName = string.Empty;
			_charset = FbCharset.Default;
			_internalParameterName = string.Empty;
		}

		public FbParameter(string parameterName, object value)
			: this()
		{
			ParameterName = parameterName;
			Value = value;
		}

		public FbParameter(string parameterName, FbDbType fbType)
			: this()
		{
			ParameterName = parameterName;
			FbDbType = fbType;
		}

		public FbParameter(string parameterName, FbDbType fbType, int size)
			: this()
		{
			ParameterName = parameterName;
			FbDbType = fbType;
			Size = size;
		}

		public FbParameter(string parameterName, FbDbType fbType, int size, string sourceColumn)
			: this()
		{
			ParameterName = parameterName;
			FbDbType = fbType;
			Size = size;
			_sourceColumn = sourceColumn;
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
#if !NETSTANDARD1_6
			DataRowVersion sourceVersion,
#endif
			object value)
		{
			ParameterName = parameterName;
			FbDbType = dbType;
			Size = size;
			_direction = direction;
			_isNullable = isNullable;
			_precision = precision;
			_scale = scale;
			_sourceColumn = sourceColumn;
#if !NETSTANDARD1_6
			_sourceVersion = sourceVersion;
#endif
			Value = value;
			_charset = FbCharset.Default;
		}

		#endregion

		#region ICloneable Methods
#if NETSTANDARD1_6
		internal object Clone()
#else
		object ICloneable.Clone()
#endif
		{
			return new FbParameter(
				_parameterName,
				_fbDbType,
				_size,
				_direction,
				_isNullable,
				_precision,
				_scale,
				_sourceColumn,
#if !NETSTANDARD1_6
				_sourceVersion,
#endif
				_value)
			{
				Charset = _charset
			};
		}

		#endregion

		#region DbParameter methods

		public override string ToString()
		{
			return _parameterName;
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
				value = DBNull.Value;
			}

			var code = Type.GetTypeCode(value.GetType());

			switch (code)
			{
				case TypeCode.Char:
					_fbDbType = FbDbType.Char;
					break;

#if !NETSTANDARD1_6
				case TypeCode.DBNull:
#endif
				case TypeCode.String:
					_fbDbType = FbDbType.VarChar;
					break;

				case TypeCode.Boolean:
					_fbDbType = FbDbType.Boolean;
					break;

				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
					_fbDbType = FbDbType.SmallInt;
					break;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					_fbDbType = FbDbType.Integer;
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					_fbDbType = FbDbType.BigInt;
					break;

				case TypeCode.Single:
					_fbDbType = FbDbType.Float;
					break;

				case TypeCode.Double:
					_fbDbType = FbDbType.Double;
					break;

				case TypeCode.Decimal:
					_fbDbType = FbDbType.Decimal;
					break;

				case TypeCode.DateTime:
					_fbDbType = FbDbType.TimeStamp;
					break;

				case TypeCode.Empty:
				default:
					if (value is Guid)
					{
						_fbDbType = FbDbType.Guid;
					}
					else if (code == TypeCode.Object)
					{
						_fbDbType = FbDbType.Binary;
					}
					else
					{
						throw new ArgumentException("Parameter type is unknown.");
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
				var svalue = (_value as string);
				if (svalue != null)
				{
					return svalue.Length;
				}
				var bvalue = (_value as byte[]);
				if (bvalue != null)
				{
					return bvalue.Length;
				}
				return null;
			}
		}

		internal bool IsUnicodeParameterName
		{
			get
			{
				return _isUnicodeParameterName;
			}
		}

		#endregion

		#region Static Methods

		internal static string NormalizeParameterName(string parameterName)
		{
			return string.IsNullOrEmpty(parameterName) || parameterName[0] == '@'
				? parameterName
				: "@" + parameterName;
		}

		internal static bool IsNonAsciiParameterName(string parameterName)
		{
			return Encoding.UTF8.GetByteCount(parameterName) != parameterName.Length;
		}

		#endregion
	}
}
