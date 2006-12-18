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
using System.Collections;
using System.Globalization;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/overview/*'/>
	[ListBindable(false),
	Editor(typeof(Design.ParameterCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public sealed class FbParameterCollection : MarshalByRefObject, IList, ICollection, IDataParameterCollection
	{	
		#region FIELDS

		private ArrayList parameters;

		#endregion

		#region PROPERTIES

		object IDataParameterCollection.this[string parameterName] 
		{
			get { return this[parameterName]; }
			set { this[parameterName] = (FbParameter)value; }
		}

		object IList.this[int parameterIndex]
		{
			get { return (FbParameter)parameters[parameterIndex]; }
			set { parameters[parameterIndex] = (FbParameter)value; }
		}
		
		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/indexer[@name="Item(System.String)"]/*'/>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public FbParameter this[string parameterName]
		{
			get { return (FbParameter)this[IndexOf(parameterName)]; }
			set { this[IndexOf(parameterName)] = (FbParameter)value; }
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/indexer[@name="Item(System.Int32)"]/*'/>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public FbParameter this[int parameterIndex]
		{
			get { return (FbParameter)parameters[parameterIndex]; }
			set { parameters[parameterIndex] = (FbParameter)value; }
		}
		
		#endregion

		#region CONSTRUCTORS

		internal FbParameterCollection()
		{
			this.parameters = ArrayList.Synchronized(new ArrayList());
		}

		#endregion

		#region ILIST_PROPERTIES

		bool IList.IsFixedSize
		{
			get { return parameters.IsFixedSize; }
		}

		bool IList.IsReadOnly
		{
			get { return parameters.IsReadOnly; }
		}

		#endregion

		#region ICOLLECTION_PROPERTIES

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/property[@name="Count"]/*'/>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Count 
		{
			get { return parameters.Count; }
		}
		
		bool ICollection.IsSynchronized 
		{
			get { return parameters.IsSynchronized; }
		}

		object ICollection.SyncRoot 
		{
			get { return parameters.SyncRoot; }
		}

		#endregion

		#region ICOLLECTION_METHODS

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="CopyTo(System.Array,System.Int32)"]/*'/>
		public void CopyTo(Array array, int index)
		{
			parameters.CopyTo(array, index);
		}

		#endregion

		#region ILIST_METHODS

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Clear"]/*'/>
		public void Clear()
		{
			parameters.Clear();
		}

		#endregion

		#region IENUMERABLE_METHODS

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="GetEnumerator"]/*'/>
		public IEnumerator GetEnumerator()
		{
			return parameters.GetEnumerator();
		}

		#endregion

		#region METHODS

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Contains(System.Object)"]/*'/>
		public bool Contains(object value)
		{
			return parameters.Contains(value);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Contains(System.String)"]/*'/>
		public bool Contains(string parameterName)
		{
			return(-1 != IndexOf(parameterName));
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="IndexOf(System.Object)"]/*'/>
		public int IndexOf(object value)
		{
			return parameters.IndexOf(value);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="IndexOf(System.String)"]/*'/>
		public int IndexOf(string parameterName)
		{
			int index = 0;
			foreach (FbParameter item in this.parameters)
			{
				if (cultureAwareCompare(item.ParameterName, parameterName))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Insert(System.Int32,System.Object)"]/*'/>
		public void Insert(int index, object value)			
		{
			parameters.Insert(index, value);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Remove(System.Object)"]/*'/>
		public void Remove(object value)
		{
			if (!(value is FbParameter))
			{
				throw new InvalidCastException("The parameter passed was not a FbParameter.");
			}

			if (!this.Contains(value))
			{
				throw new SystemException("The parameter does not exist in the collection.");
			}

			this.parameters.Remove(value);

			((FbParameter)value).Parent = null;
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="RemoveAt(System.Int32)"]/*'/>
		public void RemoveAt(int index)
		{
			if (index < 0 || index > this.Count)
			{
				throw new IndexOutOfRangeException("The specified index does not exist.");
			}
			FbParameter parameter = this[index];
			this.parameters.RemoveAt(index);
			parameter.Parent = null;
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="RemoveAt(System.String)"]/*'/>
		public void RemoveAt(string parameterName)
		{
			this.RemoveAt(this.IndexOf(parameterName));
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,System.Object)"]/*'/>
		public FbParameter Add(string parameterName, object value)
		{
			FbParameter param = new FbParameter(parameterName, value);

			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,FbDbType)"]/*'/>
		public FbParameter Add(string parameterName, FbDbType type)
		{
			FbParameter param = new FbParameter(parameterName, type);			
			
			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,FbDbType,System.Int32)"]/*'/>
		public FbParameter Add(string parameterName, FbDbType fbType, int size)
		{
			FbParameter param = new FbParameter(parameterName, fbType, size);

			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,FbDbType,System.Int32,System.String)"]/*'/>
		public FbParameter Add(
			string		parameterName, 
			FbDbType	fbType, 
			int			size, 
			string		sourceColumn)
		{
			FbParameter param = new FbParameter(
				parameterName, 
				fbType, 
				size, 
				sourceColumn);

			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.Object)"]/*'/>
		public int Add(object value)
		{
			if (!(value is FbParameter))
			{
				throw new InvalidCastException("The parameter passed was not a FbParameter.");
			}
			
			return this.IndexOf(this.Add(value as FbParameter));
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml' path='doc/class[@name="FbParameterCollection"]/method[@name="Add(FbParameter)"]/*'/>
		public FbParameter Add(FbParameter value)
		{
			lock (this.parameters.SyncRoot)
			{
				if (value == null)
				{
					throw new ArgumentException("The value parameter is null.");
				}
				if (value.Parent != null)
				{
					throw new ArgumentException("The FbParameter specified in the value parameter is already added to this or another FbParameterCollection.");
				}
				if (value.ParameterName == null || 
					value.ParameterName.Length == 0)
				{
					value.ParameterName = this.generateParameterName();
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

		#endregion

		#region PRIVATE_METHODS

		private string generateParameterName()
		{
			int		index	= this.Count + 1;
			string	name	= String.Empty;

			while (index > 0)
			{
				name = "Parameter" + index.ToString();

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

		private bool cultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(
				strA, 
				strB, 
				CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | 
				CompareOptions.IgnoreCase) == 0 ? true : false;
		}

		#endregion
	}
}