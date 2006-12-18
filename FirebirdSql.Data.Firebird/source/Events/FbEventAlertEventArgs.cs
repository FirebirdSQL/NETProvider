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

namespace FirebirdSql.Data.Firebird.Events
{
	/// <include file='Doc/en_EN/FbEventAlertEventArgs.xml' path='doc/class[@name="FbEvents"]/delegate[@name="FbEventAlertEventHandler"]/*'/>
	public delegate void FbEventAlertEventHandler(object sender, FbEventAlertEventArgs e);

	/// <include file='Doc/en_EN/FbEventAlertEventArgs.xml' path='doc/class[@name="FbEventAlertEventArgs"]/overview/*'/>
	public sealed class FbEventAlertEventArgs : EventArgs
	{
		#region FIELDS

		private int[] counts;

		#endregion

		#region PROPERTIES

		/// <include file='Doc/en_EN/FbEventAlertEventArgs.xml' path='doc/class[@name="FbEventAlertEventArgs"]/property[@name="Counts"]/*'/>
		public int[] Counts
		{
			get { return counts; }
			set { counts = value; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbEventAlertEventArgs.xml' path='doc/class[@name="FbEventAlertEventArgs"]/constructor[@name="ctor(system.Array)"]/*'/>
		internal FbEventAlertEventArgs(int[] counts)
		{
			this.counts = counts;
		}

		#endregion
	}
}
