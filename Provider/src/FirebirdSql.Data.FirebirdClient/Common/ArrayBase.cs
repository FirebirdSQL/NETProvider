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
using System.Text;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal abstract class ArrayBase
	{
		#region Fields

		private ArrayDesc _descriptor;
		private string _tableName;
		private string _fieldName;
		private string _rdbFieldName;

		#endregion

		#region Properties

		public ArrayDesc Descriptor => _descriptor;

		#endregion

		#region Abstract Properties

		public abstract long Handle { get; set; }
		public abstract IDatabase Database { get; set; }
		public abstract TransactionBase Transaction { get; set; }

		#endregion

		#region Constructors

		protected ArrayBase(ArrayDesc descriptor)
		{
			_tableName = descriptor.RelationName;
			_fieldName = descriptor.FieldName;
			_descriptor = descriptor;
		}

		protected ArrayBase(string tableName, string fieldName)
		{
			_tableName = tableName;
			_fieldName = fieldName;
			_rdbFieldName = string.Empty;
		}

		#endregion

		#region Methods

		public Array Read()
		{
			var slice = GetSlice(GetSliceLength(true));

			return DecodeSlice(slice);
		}

		public void Write(Array sourceArray)
		{
			SetDesc(sourceArray);
			PutSlice(sourceArray, GetSliceLength(false));
		}

		public void SetDesc(Array sourceArray)
		{
			_descriptor.Dimensions = (short)sourceArray.Rank;

			for (var i = 0; i < sourceArray.Rank; i++)
			{
				var lb = _descriptor.Bounds[i].LowerBound;
				var ub = sourceArray.GetLength(i) - 1 + lb;

				_descriptor.Bounds[i].UpperBound = ub;
			}
		}

		public void LookupBounds()
		{
			LookupDesc();

			using (var lookup = Database.CreateStatement(Transaction))
			{
				lookup.Prepare(GetArrayBounds());
				lookup.Execute();

				_descriptor.Bounds = new ArrayBound[16];
				DbValue[] values;
				var i = 0;
				while ((values = lookup.Fetch()) != null)
				{
					_descriptor.Bounds[i].LowerBound = values[0].GetInt32();
					_descriptor.Bounds[i].UpperBound = values[1].GetInt32();

					i++;
				}
			}
		}

		public void LookupDesc()
		{
			using (var lookup = Database.CreateStatement(Transaction))
			{
				lookup.Prepare(GetArrayDesc());
				lookup.Execute();

				_descriptor = new ArrayDesc();
				var values = lookup.Fetch();
				if (values != null && values.Length > 0)
				{
					_descriptor.RelationName = _tableName;
					_descriptor.FieldName = _fieldName;
					_descriptor.DataType = values[0].GetByte();
					_descriptor.Scale = values[1].GetInt16();
					_descriptor.Length = values[2].GetInt16();
					_descriptor.Dimensions = values[3].GetInt16();
					_descriptor.Flags = 0;

					_rdbFieldName = values[4].GetString().Trim();
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}

		#endregion

		#region Protected Methods

		protected int GetSliceLength(bool read)
		{
			var elements = 1;
			for (var i = 0; i < _descriptor.Dimensions; i++)
			{
				var bound = _descriptor.Bounds[i];
				elements *= bound.UpperBound - bound.LowerBound + 1;
			}

			var length = elements * _descriptor.Length;

			switch (_descriptor.DataType)
			{
				case IscCodes.blr_varying:
				case IscCodes.blr_varying2:
					length += elements * 2;
					break;
			}

			return length;
		}

		protected Type GetSystemType()
		{
			return TypeHelper.GetTypeFromBlrType(_descriptor.DataType, default, _descriptor.Scale);
		}

		#endregion

		#region Abstract Methods

		public abstract byte[] GetSlice(int slice_length);
		public abstract void PutSlice(Array source_array, int slice_length);

		#endregion

		#region Protected Abstract Methods

		protected abstract Array DecodeSlice(byte[] slice);

		#endregion

		#region Private Methods

		private string GetArrayDesc()
		{
			var sql = new StringBuilder();

			sql.Append(
				"SELECT Y.RDB$FIELD_TYPE, Y.RDB$FIELD_SCALE, Y.RDB$FIELD_LENGTH, Y.RDB$DIMENSIONS, X.RDB$FIELD_SOURCE " +
				"FROM RDB$RELATION_FIELDS X, RDB$FIELDS Y " +
				"WHERE X.RDB$FIELD_SOURCE = Y.RDB$FIELD_NAME ");

			if (_tableName != null && _tableName.Length != 0)
			{
				sql.AppendFormat(" AND X.RDB$RELATION_NAME = '{0}'", _tableName);
			}

			if (_fieldName != null && _fieldName.Length != 0)
			{
				sql.AppendFormat(" AND X.RDB$FIELD_NAME = '{0}'", _fieldName);
			}

			return sql.ToString();
		}

		private string GetArrayBounds()
		{
			var sql = new StringBuilder();

			sql.Append("SELECT X.RDB$LOWER_BOUND, X.RDB$UPPER_BOUND FROM RDB$FIELD_DIMENSIONS X ");

			if (_fieldName != null && _fieldName.Length != 0)
			{
				sql.AppendFormat("WHERE X.RDB$FIELD_NAME = '{0}'", _rdbFieldName);
			}

			sql.Append(" ORDER BY X.RDB$DIMENSION");

			return sql.ToString();
		}

		#endregion
	}
}
