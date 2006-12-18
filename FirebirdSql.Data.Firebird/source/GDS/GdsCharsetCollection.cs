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
using System.Globalization;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsCharsetCollection : ArrayList
	{
		#region PROPERTIES

		public new GdsCharset this[int index]
		{
			get { return (GdsCharset)base[index]; }
			set { base[index] = (GdsCharset)value; }
		}

		public GdsCharset this[string name] 
		{
			get { return (GdsCharset)this[IndexOf(name)]; }
			set { this[IndexOf(name)] = (GdsCharset)value; }
		}

		#endregion

		#region METHODS

		public bool Contains(string name)
		{
			return(-1 != IndexOf(name));
		}

		public int IndexOf(string name)
		{
			int index = 0;
			foreach(GdsCharset item in this)
			{
				if (cultureAwareCompare(item.Name, name))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public int IndexOf(int id)
		{
			int index = 0;
			foreach(GdsCharset item in this)
			{
				if (item.ID == id)
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public void RemoveAt(string charset)
		{
			RemoveAt(IndexOf(charset));
		}

		public GdsCharset Add(GdsCharset charset)
		{
			base.Add(charset);

			return charset;
		}

		public GdsCharset Add(
			int id, 
			string charset, 
			int bytesPerCharacter,
			string systemCharset)
		{
			GdsCharset charSet = new GdsCharset(
				id, 
				charset, 
				bytesPerCharacter, 
				systemCharset);

			base.Add(charSet);

			return charSet;
		}

		public GdsCharset Add(
			int id, 
			string charset, 
			int bytesPerCharacter, 
			int cp)
		{
			GdsCharset charSet = new GdsCharset(
				id, 
				charset, 
				bytesPerCharacter, 
				cp);

			base.Add(charSet);

			return charSet;
		}

		public GdsCharset Add(
			int id, 
			string charset, 
			int bytesPerCharacter,
			Encoding encoding)
		{
			GdsCharset charSet = new GdsCharset(
				id, 
				charset, 
				bytesPerCharacter,
				encoding);

			base.Add(charSet);

			return charSet;
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
