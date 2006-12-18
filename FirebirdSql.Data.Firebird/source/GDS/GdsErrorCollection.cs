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

namespace FirebirdSql.Data.Firebird.Gds
{
	[Serializable]
	internal class GdsErrorCollection : ArrayList
	{
		#region PROPERTIES

		public GdsError this[string errorMessage] 
		{
			get { return (GdsError)this[IndexOf(errorMessage)]; }
			set { this[IndexOf(errorMessage)] = (GdsError)value; }
		}

		public new GdsError this[int errorIndex] 
		{
			get { return (GdsError)base[errorIndex]; }
			set { base[errorIndex] = (GdsError)value; }
		}

		#endregion

		#region METHODS
	
		public bool Contains(string errorMessage)
		{
			return(-1 != IndexOf(errorMessage));
		}
		
		public int IndexOf(string errorMessage)
		{
			int index = 0;

			foreach (GdsError item in this)
			{
				if (cultureAwareCompare(item.Message, errorMessage))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public void RemoveAt(string errorMessage)
		{
			RemoveAt(IndexOf(errorMessage));
		}

		internal GdsError Add(GdsError error)
		{
			base.Add(error);

			return error;
		}
		
		internal GdsError Add(int errorCode)
		{
			GdsError error = new GdsError(errorCode);			

			return Add(error);
		}

		internal GdsError Add(string message)
		{
			GdsError error = new GdsError(message);			

			return Add(error);
		}

		internal GdsError Add(int type, string strParam)
		{
			GdsError error = new GdsError(type, strParam);			

			return Add(error);
		}

		internal GdsError Add(int type, int errorCode)
		{
			GdsError error = new GdsError(type, errorCode);			

			return Add(error);
		}

		internal GdsError Add(int type, int errorCode, string strParam)
		{
			GdsError error = new GdsError(type, errorCode, strParam);

			return Add(error);
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
