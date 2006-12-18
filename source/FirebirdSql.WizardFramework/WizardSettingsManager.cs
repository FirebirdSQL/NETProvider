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
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if	(NET)

using System;
using System.Collections;

namespace FirebirdSql.WizardFramework
{
	/// <summary>
	/// Manages	settings of	teh	Wizards.
	/// </summary>
	internal class WizardSettingsManager
	{
		#region Static Properties

		/// <summary>
		/// Singleton instance of the <see cref="WizardSettingsManager"/>
		/// </summary>
		public static readonly WizardSettingsManager Instance = new WizardSettingsManager();

		#endregion

		#region Fields

		private Hashtable settings;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the WizardSettingsManager	class
		/// </summary>
		private WizardSettingsManager()
		{
			this.settings = Hashtable.Synchronized(new Hashtable());
		}

		#endregion

		#region Methods

		/// <summary>
		/// Regioster a	new	set	of settings	using the specified	name
		/// </summary>
		/// <param name="name">The settings	key	name.</param>
		/// <returns></returns>
		public WizardSettings RegisterSettings(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name cannot be null.");
			}
			if (this.settings.Contains(name))
			{
				throw new ArgumentException("The specified wizard settings already exists.");
			}

			WizardSettings settings = new WizardSettings();
			this.settings.Add(name, settings);

			return settings;
		}

		/// <summary>
		/// Returns	the	set	of settings	that matches the give name.
		/// </summary>
		/// <param name="name">The key name.</param>
		/// <returns>An	instance of	the	<see cref="WizardSettings" /> class.</returns>
		public WizardSettings GetSettings(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name cannot be null.");
			}
			if (!settings.Contains(name))
			{
				throw new ArgumentException("The specified wizard settings cannot be found.");
			}

			return (WizardSettings)this.settings[name];
		}

		/// <summary>
		/// Unregister the set of keys that	matches	the	give name.
		/// </summary>
		/// <param name="name">The key name.</param>
		public void UnregisterSettings(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name cannot be null.");
			}
			if (!settings.Contains(name))
			{
				throw new ArgumentException("The specified wizard settings cannot be found.");
			}

			this.settings.Remove(name);
		}

		#endregion
	}
}

#endif