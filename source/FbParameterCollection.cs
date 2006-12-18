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
using System.Collections;
using System.Globalization;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="T:FbParameterCollection"]/*'/>
	public sealed class FbParameterCollection : MarshalByRefObject, IList, ICollection, IDataParameterCollection
	{	
		#region FIELDS

		private ArrayList parameters = new ArrayList();

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="P:Item(System.String)"]/*'/>
		object IDataParameterCollection.this[string parameterName] 
		{
			get { return this[parameterName]; }
			set { this[parameterName] = (FbParameter)value; }
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="P:Item(System.String)"]/*'/>
		public FbParameter this[string parameterName]
		{
			get { return (FbParameter)this[IndexOf(parameterName)]; }
			set { this[IndexOf(parameterName)] = (FbParameter)value; }
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="P:Item(System.Int32)"]/*'/>
		object IList.this[int parameterIndex]
		{
			get { return (FbParameter)parameters[parameterIndex]; }
			set { parameters[parameterIndex] = (FbParameter)value; }
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="P:Item(System.Int32)"]/*'/>
		public FbParameter this[int parameterIndex]
		{
			get { return (FbParameter)parameters[parameterIndex]; }
			set { parameters[parameterIndex] = (FbParameter)value; }
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

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="P:Count"]/*'/>
		public int Count 
		{
			get { return parameters.Count; }
		}
		
		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="P:ICollection.IsSynchronized"]/*'/>
		bool ICollection.IsSynchronized 
		{
			get { return parameters.IsSynchronized; }
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="P:ICollection.SyncRoot"]/*'/>
		object ICollection.SyncRoot 
		{
			get { return parameters.SyncRoot; }
		}

		#endregion

		#region ICOLLECTION_METHODS

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:CopyTo(System.Array,System.Int32)"]/*'/>		
		public void CopyTo(Array array, int index)
		{
			parameters.CopyTo(array, index);
		}

		#endregion

		#region ILIST_METHODS

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Clear()"]/*'/>
		public void Clear()
		{
			parameters.Clear();
		}

		#endregion

		#region IENUMERABLE_METHODS

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:GetEnumerator()"]/*'/>
		public IEnumerator GetEnumerator()
		{
			return parameters.GetEnumerator();
		}

		#endregion

		#region METHODS

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Contains(System.Object)"]/*'/>
		public bool Contains(object value)
		{
			return parameters.Contains(value);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Contains(System.String)"]/*'/>
		public bool Contains(string parameterName)
		{
			return(-1 != IndexOf(parameterName));
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:IndexOf(System.Object)"]/*'/>
		public int IndexOf(object value)
		{
			return parameters.IndexOf(value);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:IndexOf(System.String)"]/*'/>
		public int IndexOf(string parameterName)
		{
			int index = 0;
			foreach(FbParameter item in this.parameters)
			{
				if (0 == _cultureAwareCompare(item.ParameterName, parameterName))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Insert(System.Int32,System.Object)"]/*'/>
		public void Insert(int index, object value)
		{
			parameters.Insert(index, value);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Remove(System.Object)"]/*'/>
		public void Remove(object value)
		{
			if (!(value is FbParameter))
			{
				throw new InvalidCastException("The parameter passed was not a FbParameter.");
			}

			if (!Contains(value))
			{
				throw new SystemException("The parameter does not exist in the collection.");
			}

			parameters.Remove(value);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:RemoveAt(System.Int32)"]/*'/>
		public void RemoveAt(int index)
		{
			RemoveAt(this[index].ParameterName);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:RemoveAt(System.String)"]/*'/>
		public void RemoveAt(string parameterName)
		{
			RemoveAt(IndexOf(parameterName));
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Add(System.Object)"]/*'/>
		public int Add(object value)
		{
			if (!(value is FbParameter))
			{
				throw new InvalidCastException("The parameter passed was not a FbParameter.");
			}

			return parameters.Add((FbParameter)value);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Add(FirebirdSql.Data.Firebird.FbParameter)"]/*'/>
		public FbParameter Add(FbParameter param)
		{
			if (param.ParameterName != null)
			{
				parameters.Add(param);
				
				return param;
			}
			else
			{
				throw new ArgumentException("parameter must be named");
			}
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Add(System.String,FirebirdSql.Data.Firebird.FbType)"]/*'/>
		public FbParameter Add(string parameterName, FbType type)
		{
			FbParameter param = new FbParameter(parameterName, type);			
			
			return Add(param);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Add(System.String,System.Object)"]/*'/>
		public FbParameter Add(string parameterName, object value)
		{
			FbParameter param = new FbParameter(parameterName, value);

			return Add(param);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Add(System.String,FirebirdSql.Data.Firebird.FbType,System.String)"]/*'/>
		public FbParameter Add(string parameterName, FbType fbType, 
								string sourceColumn)
		{
			FbParameter param = new FbParameter(parameterName, fbType, sourceColumn);
			
			return Add(param);
		}

		/// <include file='xmldoc/fbparametercollection.xml' path='doc/member[@name="M:Add(System.String,FirebirdSql.Data.Firebird.FbType,System.Int32,System.String)"]/*'/>
		public FbParameter Add(string parameterName, FbType fbType, 
								int size, string sourceColumn)
		{
			FbParameter param = new FbParameter(parameterName, fbType, size, sourceColumn);

			return Add(param);		
		}

		private int _cultureAwareCompare(string strA, string strB)
		{
			#if (_MONO)
			return strA.ToUpper() == strB.ToUpper() ? 0 : 1;
			#else				
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, 
				CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | 
				CompareOptions.IgnoreCase);
			#endif
		}

		#endregion
	}
}
