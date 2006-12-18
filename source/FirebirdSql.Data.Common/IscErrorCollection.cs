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

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class IscErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		private ArrayList errors;

		#endregion

		#region Indexers

		public IscError this[string errorMessage] 
		{
			get { return (IscError)this.errors[IndexOf(errorMessage)]; }
			set { this.errors[IndexOf(errorMessage)] = (IscError)value; }
		}

		public IscError this[int errorIndex] 
		{
			get { return (IscError)this.errors[errorIndex]; }
			set { this.errors[errorIndex] = (IscError)value; }
		}

		#endregion

		#region Constructors

		internal IscErrorCollection()
		{
			this.errors = ArrayList.Synchronized(new ArrayList());
		}

		#endregion

		#region ICollection Properties

		public int Count 
		{
			get { return this.errors.Count; }
		}
		
		bool ICollection.IsSynchronized
		{
			get { return this.errors.IsSynchronized; }
		}

		object ICollection.SyncRoot 
		{
			get { return this.errors.SyncRoot; }
		}

		#endregion

		#region ICollection Methods

		public void CopyTo(Array array, int index)
		{
			this.CopyTo((IscError[])array, index);
		}
		
		#endregion

		#region IEnumerable Methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.errors.GetEnumerator();
		}

		#endregion

		#region Methods

		internal int IndexOf(string errorMessage)
		{
			int index = 0;
			foreach (IscError item in this)
			{
				if (GlobalizationHelper.CultureAwareCompare(item.Message, errorMessage))
				{
					return index;
				}
				index++;
			}

			return -1;
		}
		
		public IscError Add(IscError error)
		{
			this.errors.Add(error);

			return error;
		}

		public IscError Add(int errorCode)
		{
			IscError error = new IscError(errorCode);			

			return this.Add(error);
		}

		public IscError Add(string message)
		{
			IscError error = new IscError(message);			

			return this.Add(error);
		}

		public IscError Add(int type, string strParam)
		{
			IscError error = new IscError(type, strParam);			

			return this.Add(error);
		}

		public IscError Add(int type, int errorCode)
		{
			IscError error = new IscError(type, errorCode);			

			return this.Add(error);
		}

		public IscError Add(int type, int errorCode, string strParam)
		{
			IscError error = new IscError(type, errorCode, strParam);

			return this.Add(error);
		}

		#endregion
	}
}
