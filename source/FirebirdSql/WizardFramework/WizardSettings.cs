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
using System.Collections;

namespace FirebirdSql.WizardFramework
{
	/// <summary>
	/// Represents a set of	settings for a specific	Wizard instance.
	/// </summary>
	internal class WizardSettings
	{
		#region · Fields ·

		private Hashtable settings;

		#endregion

		#region · Indexers ·

		/// <summary>
		/// Gets or	sets the setting value with	the	specified name.
		/// </summary>
		/// <param name="name">Te name of the setting to retrieve.</param>
		/// <returns>The element with the specified	name.</returns>
		public object this[string name]
		{
			get { return this.settings[name]; }
			set { this.settings[name] = value; }
		}

		#endregion

		#region · Constructors ·

		/// <summary>
		/// Initializes	a new instance of the <b>WizardSettings</b>	class.
		/// </summary>
		public WizardSettings()
		{
			this.settings = new Hashtable();
		}

		#endregion

		#region · Methods ·

		/// <summary>
		/// Adds a new setting with	the	given name.
		/// </summary>
		/// <param name="name">The name	of the setting to be added.</param>
		/// <value>The value of	the	new	setting	will be	<b>null</b></value>
		public void Add(string name)
		{
			this.Add(name, null);
		}

		/// <summary>
		/// Adds a new setting with	the	given name and value.
		/// </summary>
		/// <param name="name">The name	of the setting to be added.</param>
		/// <param name="value">The	value of the setting to	be added.</param>
		public void Add(string name, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name cannot be null.");
			}
			if (settings.Contains(name))
			{
				throw new ArgumentException("The specified setting already exits.");
			}

			this.settings.Add(name, value);
		}

		/// <summary>
		/// Determines wheter a	setting	is defined.
		/// </summary>
		/// <param name="name">The name	of the setting.</param>
		/// <returns><b>true</b> if	the	setting	is defined;	or <b>false</b>	if not.</returns>
		public bool Contains(string name)
		{
			return this.settings.Contains(name);
		}

		#endregion
	}
}

#endif