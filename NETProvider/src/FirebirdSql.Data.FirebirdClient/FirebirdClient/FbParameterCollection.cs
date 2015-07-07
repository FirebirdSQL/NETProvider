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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	[ListBindable(false)]
	public sealed class FbParameterCollection : DbParameterCollection
	{
		#region Fields

		private List<FbParameter> parameters;

		#endregion

		#region Indexers

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new FbParameter this[string parameterName]
		{
			get { return this[this.IndexOf(parameterName)]; }
			set { this[this.IndexOf(parameterName)] = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new FbParameter this[int index]
		{
			get { return this.parameters[index]; }
			set { this.parameters[index] = value; }
		}

		#endregion

		#region DbParameterCollection overriden properties

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int Count
		{
			get { return this.parameters.Count; }
		}

		public override bool IsFixedSize
		{
			get { return ((IList)this.parameters).IsFixedSize; }
		}

		public override bool IsReadOnly
		{
			get { return ((IList)this.parameters).IsReadOnly; }
		}

		public override bool IsSynchronized
		{
			get { return ((ICollection)this.parameters).IsSynchronized; }
		}

		public override object SyncRoot
		{
			get { return ((ICollection)this.parameters).SyncRoot; }
		}

		#endregion

		#region Constructors

		internal FbParameterCollection()
		{
			this.parameters = new List<FbParameter>();
		}

		#endregion

		#region DbParameterCollection overriden methods

		public void AddRange(IEnumerable<FbParameter> values)
		{
			foreach (var p in values)
			{
				this.Add(p);
			}
		}

		public override void AddRange(Array values)
		{
			this.AddRange(values.Cast<object>().Select(x => { EnsureFbParameterType(x); return (FbParameter)x; }));
		}

		public FbParameter AddWithValue(string parameterName, object value)
		{
			return this.Add(new FbParameter(parameterName, value));
		}

		public FbParameter Add(string parameterName, object value)
		{
			return this.Add(new FbParameter(parameterName, value));
		}

		public FbParameter Add(string parameterName, FbDbType type)
		{
			return this.Add(new FbParameter(parameterName, type));
		}

		public FbParameter Add(string parameterName, FbDbType fbType, int size)
		{
			return this.Add(new FbParameter(parameterName, fbType, size));
		}

		public FbParameter Add(string parameterName, FbDbType fbType, int size, string sourceColumn)
		{
			return this.Add(new FbParameter(parameterName, fbType, size, sourceColumn));
		}

		public FbParameter Add(FbParameter value)
		{
			lock (this.SyncRoot)
			{
				if (value == null)
				{
					throw new ArgumentException("The value parameter is null.");
				}
				if (value.Parent != null)
				{
					throw new ArgumentException("The FbParameter specified in the value parameter is already added to this or another FbParameterCollection.");
				}
				if (value.ParameterName == null || value.ParameterName.Length == 0)
				{
					value.ParameterName = this.GenerateParameterName();
				}
				else
				{
					if (this.IndexOf(value) != -1)
					{
						throw new ArgumentException("FbParameterCollection already contains FbParameter with ParameterName '" + value.ParameterName + "'.");
					}
				}

				this.parameters.Add(value);

				return value;
			}
		}

		public override int Add(object value)
		{
			EnsureFbParameterType(value);

			return this.IndexOf(this.Add((FbParameter)value));
		}

		public bool Contains(FbParameter value)
		{
			return this.parameters.Contains(value);
		}

		public override bool Contains(object value)
		{
			EnsureFbParameterType(value);

			return this.Contains((FbParameter)value);
		}

		public override bool Contains(string parameterName)
		{
			return this.IndexOf(parameterName) != -1;
		}

		public int IndexOf(FbParameter value)
		{
			return this.parameters.IndexOf(value);
		}

		public override int IndexOf(object value)
		{
			EnsureFbParameterType(value);

			return this.IndexOf((FbParameter)value);
		}

		public override int IndexOf(string parameterName)
		{
			return this.IndexOf(parameterName, -1);
		}

		internal int IndexOf(string parameterName, int luckyIndex)
		{
			var normalizedParameterName = FbParameter.NormalizeParameterName(parameterName);
			if (luckyIndex != -1 && luckyIndex < this.parameters.Count)
			{
				if (this.parameters[luckyIndex].InternalParameterName.Equals(normalizedParameterName, StringComparison.CurrentCultureIgnoreCase))
				{
					return luckyIndex;
				}
			}
			return this.parameters.FindIndex(x => x.InternalParameterName.Equals(normalizedParameterName, StringComparison.CurrentCultureIgnoreCase));
		}

		public void Insert(int index, FbParameter value)
		{
			this.parameters.Insert(index, value);
		}

		public override void Insert(int index, object value)
		{
			EnsureFbParameterType(value);

			this.Insert(index, (FbParameter)value);
		}

		public void Remove(FbParameter value)
		{
			if (!this.Contains(value))
			{
				throw new SystemException("The parameter does not exist in the collection.");
			}

			this.parameters.Remove(value);
			value.Parent = null;
		}

		public override void Remove(object value)
		{
			EnsureFbParameterType(value);

			this.Remove((FbParameter)value);
		}

		public override void RemoveAt(int index)
		{
			if (index < 0 || index > this.Count)
			{
				throw new IndexOutOfRangeException("The specified index does not exist.");
			}

			FbParameter parameter = this[index];
			this.parameters.RemoveAt(index);
			parameter.Parent = null;
		}

		public override void RemoveAt(string parameterName)
		{
			this.RemoveAt(this.IndexOf(parameterName));
		}

		public void CopyTo(FbParameter[] array, int index)
		{
			this.parameters.CopyTo(array, index);
		}

		public override void CopyTo(Array array, int index)
		{
			((IList)this.parameters).CopyTo(array, index);
		}

		public override void Clear()
		{
			this.parameters.Clear();
		}

		public override IEnumerator GetEnumerator()
		{
			return this.parameters.GetEnumerator();
		}

		#endregion

		#region DbParameterCollection overriden protected methods

		protected override DbParameter GetParameter(string parameterName)
		{
			return this[parameterName];
		}

		protected override DbParameter GetParameter(int index)
		{
			return this[index];
		}

		protected override void SetParameter(int index, DbParameter value)
		{
			this[index] = (FbParameter)value;
		}

		protected override void SetParameter(string parameterName, DbParameter value)
		{
			this[parameterName] = (FbParameter)value;
		}

		#endregion

		#region Private Methods

		private string GenerateParameterName()
		{
			int index = this.Count + 1;
			string name = string.Empty;

			while (index > 0)
			{
				name = "Parameter" + index.ToString(CultureInfo.InvariantCulture);

				if (this.IndexOf(name) == -1)
				{
					index = -1;
				}
				else
				{
					index++;
				}
			}

			return name;
		}

		private void EnsureFbParameterType(object value)
		{
			if (!(value is FbParameter))
			{
				throw new InvalidCastException("The parameter passed was not a FbParameter.");
			}
		}

		#endregion
	}
}
