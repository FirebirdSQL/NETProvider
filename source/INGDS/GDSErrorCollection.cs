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

namespace FirebirdSql.Data.INGDS
{
	/// <include file='xmldoc/gdserrorcollection.xml' path='doc/member[@name="T:GDSErrorCollection"]/*'/>
	internal sealed class GDSErrorCollection : ArrayList
	{
		#region PROPERTIES

		/// <include file='xmldoc/gdserrorcollection.xml' path='doc/member[@name="P:Item(System.String)"]/*'/>
		public GDSError this[string errorMessage] 
		{
			get { return (GDSError)this[IndexOf(errorMessage)]; }
			set { this[IndexOf(errorMessage)] = (GDSError)value; }
		}

		/// <include file='xmldoc/gdserrorcollection.xml' path='doc/member[@name="P:Item(System.Int32)"]/*'/>
		public new GDSError this[int errorIndex] 
		{
			get { return (GDSError)base[errorIndex]; }
			set { base[errorIndex] = (GDSError)value; }
		}

		#endregion

		#region METHODS
	
		/// <include file='xmldoc/gdserrorcollection.xml' path='doc/member[@name="M:Contains(System.String)"]/*'/>
		public bool Contains(string errorMessage)
		{
			return(-1 != IndexOf(errorMessage));
		}
		
		/// <include file='xmldoc/gdserrorcollection.xml' path='doc/member[@name="M:IndexOf(System.String)"]/*'/>
		public int IndexOf(string errorMessage)
		{
			int index = 0;
			foreach(GDSError item in this)
			{
				if (0 == _cultureAwareCompare(item.Message, errorMessage))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		/// <include file='xmldoc/gdserrorcollection.xml' path='doc/member[@name="M:RemoveAt(System.String)"]/*'/>
		public void RemoveAt(string errorMessage)
		{
			RemoveAt(IndexOf(errorMessage));
		}

		/// <include file='xmldoc/gdserrorcollection.xml' path='doc/member[@name="M:Add(FirebirdSql.Data.INGDS.GDSError)"]/*'/>
		internal GDSError Add(GDSError error)
		{
			base.Add(error);

			return error;
		}
		
		internal GDSError Add(int errorCode)
		{
			GDSError error = new GDSError(errorCode);			

			return Add(error);
		}

		internal GDSError Add(string message)
		{
			GDSError error = new GDSError(message);			

			return Add(error);
		}

		internal GDSError Add(int type, string strParam)
		{
			GDSError error = new GDSError(type, strParam);			

			return Add(error);
		}

		internal GDSError Add(int type, int errorCode)
		{
			GDSError error = new GDSError(type, errorCode);			

			return Add(error);
		}

		internal GDSError Add(int type, int errorCode, string strParam)
		{
			GDSError error = new GDSError(type, errorCode, strParam);

			return Add(error);
		}

		private int _cultureAwareCompare(string strA, string strB)
		{
			#if (_MONO)
			return strA.ToUpper() == strB.ToUpper() ? 0 : 1;
			#else				
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
			#endif
		}

		#endregion
	}
}
