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
using System.Collections;
using System.Globalization;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="T:FbErrorCollection"]/*'/>	
	[Serializable]
	public sealed class FbErrorCollection : ICollection, IEnumerable
	{
		#region FIELDS

		private ArrayList errors = new ArrayList();

		#endregion

		#region INDEXERS

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="P:Item(System.String)"]/*'/>
		public FbError this[string errorMessage] 
		{
			get { return (FbError)errors[IndexOf(errorMessage)]; }
			set { errors[IndexOf(errorMessage)] = (FbError)value; }
		}

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="P:Item(System.Int32)"]/*'/>
		public FbError this[int errorIndex] 
		{
			get { return (FbError)errors[errorIndex]; }
			set { errors[errorIndex] = (FbError)value; }
		}

		#endregion

		#region ICOLLECTION_PROPERTIES

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="P:Count"]/*'/>
		public int Count 
		{
			get { return errors.Count; }
		}
		
		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="P:ICollection.IsSynchronized"]/*'/>
		bool ICollection.IsSynchronized
		{
			get { return errors.IsSynchronized; }
		}

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="P:ICollection.SyncRoot"]/*'/>
		object ICollection.SyncRoot 
		{
			get { return errors.SyncRoot; }
		}

		#endregion

		#region ICOLLECTION_METHODS

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:CopyTo(System.Array,System.Int32)"]/*'/>
		public void CopyTo(Array array, int index)
		{
			errors.CopyTo(array, index);
		}
		
		#endregion

		#region IENUMERABLE_METHODS

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:IEnumerable.GetEnumerator()"]/*'/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return errors.GetEnumerator();
		}

		#endregion

		#region METHODS
	
		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:Contains(System.String)"]/*'/>
		internal bool Contains(string errorMessage)
		{
			return(-1 != IndexOf(errorMessage));
		}
		
		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:IndexOf(System.String)"]/*'/>
		internal int IndexOf(string errorMessage)
		{
			int index = 0;
			foreach(FbError item in this)
			{
				if (0 == _cultureAwareCompare(item.Message, errorMessage))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:RemoveAt(System.String)"]/*'/>
		internal void RemoveAt(string errorMessage)
		{
			errors.RemoveAt(IndexOf(errorMessage));
		}

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:Add(FirebirdSql.Data.Firebird.FbError)"]/*'/>
		internal FbError Add(FbError error)
		{
			errors.Add(error);

			return error;
		}

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:Add(System.String,System.Int32)"]/*'/>
		internal FbError Add(string errorMessage, int number)
		{
			FbError error = new FbError(errorMessage, number);			

			return Add(error);
		}

		/// <include file='xmldoc/fberrorcollection.xml' path='doc/member[@name="M:Add(System.Byte,System.Int32,System.String,System.Int32)"]/*'/>
		internal FbError Add(byte classError, int line, 
							string errorMessage, int number)
		{
			FbError error = new FbError(classError, line, errorMessage, number);
			
			return Add(error);
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
