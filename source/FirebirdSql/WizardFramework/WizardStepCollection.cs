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
 *	Copyright (c) 2002, 2006 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if	(NET)

using System;
using System.ComponentModel;
using System.Collections;

namespace FirebirdSql.WizardFramework
{
	/// <summary>
	/// Represents a collection	of Wizard Steps
	/// </summary>
	[ListBindable(false)]
	internal class WizardStepCollection : CollectionBase
	{
		#region · Indexers ·

		/// <summary>
		/// Gets the step at the specified index.
		/// </summary>
		/// <param name="index">The	zero-based index of	the	element	to get or set. </param>
		/// <returns>The element at	the	specified index</returns>
		public WizardStep this[int index]
		{
			get { return ((WizardStep)this.List[index]); }
		}

		#endregion

		#region · Constructors ·

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStepCollection</b> class.
		/// </summary>
		public WizardStepCollection()
		{
		}

		#endregion

		#region · Methods ·

		/// <summary>
		/// Adds a new stpe	to the collection
		/// </summary>
		/// <param name="step"></param>
		/// <returns></returns>
		public int Add(WizardStep step)
		{
			return this.List.Add(step);
		}

		/// <summary>
		/// Returns	the	zero-based index of	the	requested step.
		/// </summary>
		/// <param name="step">A <see cref="WizardStep"/> object.</param>
		/// <returns>The zero-based	index of the requested step.</returns>
		public int IndexOf(WizardStep step)
		{
			return this.List.IndexOf(step);
		}

		#endregion
	}
}

#endif