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
using System.ComponentModel;
using System.ComponentModel.Design;

namespace FirebirdSql.Data.Firebird.Design
{
	/// <summary>
	/// Designer for the <see cref="FbCommand" /> class.
	/// </summary>
	internal class FbCommandDesigner : ComponentDesigner
	{
		#region Properties

		/// <summary>
		/// Gets the design-time verbs supported by the	component that is associated with the designer.
		/// </summary>
		/// <value>
		/// A DesignerVerbCollection of	DesignerVerb objects, or null if no	designer verbs are available. 
		/// </value>
		public override DesignerVerbCollection Verbs
		{
			get { return null; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes	a new Instance of the <b>FbCommandDesigner</b> class.
		/// </summary>
		public FbCommandDesigner() : base()
		{
		}

		#endregion

		#region Initialization methods

		// This	method provides	an opportunity to perform processing when a	designer is	initialized.
		// The component parameter is the component	that the designer is associated	with.
		public override void Initialize(System.ComponentModel.IComponent component)
		{
			// Always call the base	Initialize method in an	override of	this method.
			base.Initialize(component);
		}

		#endregion

		#region PreFilter Methods

		protected override void PreFilterAttributes(IDictionary attributes)
		{
			base.PreFilterAttributes(attributes);

			if (this.Component != null)
			{
				FbCommand command = this.Component as FbCommand;

				if (attributes.Contains("DesignTimeVisible"))
				{
					attributes.Remove("DesignTimeVisible");
				}

				attributes.Add("DesignTimeVisible", new DesignTimeVisibleAttribute(command.DesignTimeVisible));
			}
		}

		#endregion
	}
}

#endif