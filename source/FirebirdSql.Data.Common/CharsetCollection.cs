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
using System.Text;
using System.Collections;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class CharsetCollection : ICollection, IEnumerable
	{
		#region Fields

		private ArrayList charsets;

		#endregion

		#region Indexers

		public Charset this[int index]
		{
			get { return (Charset)this.charsets[index]; }
			set { this.charsets[index] = (Charset)value; }
		}

		public Charset this[string name] 
		{
			get { return (Charset)this[this.IndexOf(name)]; }
			set { this[this.IndexOf(name)] = (Charset)value; }
		}

		#endregion

		#region Constructors

		internal CharsetCollection()
		{
			this.charsets = ArrayList.Synchronized(new ArrayList());
		}

		#endregion

		#region ICollection Properties

		public int Count 
		{
			get { return this.charsets.Count; }
		}
		
		bool ICollection.IsSynchronized
		{
			get { return this.charsets.IsSynchronized; }
		}

		object ICollection.SyncRoot 
		{
			get { return this.charsets.SyncRoot; }
		}

		#endregion

		#region ICollection Methods

		public void CopyTo(Array array, int index)
		{
			this.CopyTo((Charset[])array, index);
		}
		
		#endregion

		#region IEnumerable Methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.charsets.GetEnumerator();
		}

		#endregion

		#region Methods

		public int IndexOf(int id)
		{
			int index = 0;

			foreach (Charset item in this)
			{
				if (item.ID == id)
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		public int IndexOf(string name)
		{
			int index = 0;

			foreach (Charset item in this)
			{
				if (GlobalizationHelper.CultureAwareCompare(item.Name, name))
				{
					return index;
				}
				index++;
			}

			return -1;
		}
		
		internal Charset Add(
			int id, 
			string charset, 
			int bytesPerCharacter,
			string systemCharset)
		{
			Charset charSet = new Charset(
				id, 
				charset, 
				bytesPerCharacter, 
				systemCharset);

			this.Add(charSet);

			return charSet;
		}

		internal Charset Add(
			int id, 
			string charset, 
			int bytesPerCharacter,
			Encoding encoding)
		{
			Charset charSet = new Charset(
				id, 
				charset, 
				bytesPerCharacter,
				encoding);

			this.Add(charSet);

			return charSet;
		}

		internal Charset Add(Charset charset)
		{
			this.charsets.Add(charset);

			return charset;
		}

		#endregion
	}
}
