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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace FirebirdSql.WizardFramework
{
	/// <summary>
	/// Represents an step of a	Wizard.
	/// </summary>
	internal class WizardStep : UserControl
	{
		#region Events

		public event CancelEventHandler ValidateStep;

		#endregion

		#region Designer Fields

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#endregion

		#region Fields

		private bool canCancel;
		private bool canMoveBack;
		private bool canMoveNext;
		private bool canFinish;
		private WizardStepCollection backSteps;
		private WizardStepCollection nextSteps;
		protected ErrorProvider ErrorProvider;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value that indicates	wheter the panel can navigate back to the
		/// previous panel.
		/// </summary>
		/// <value></value>
		[DefaultValue(false)]
		public bool CanMoveBack
		{
			get { return this.canMoveBack; }
			set { this.canMoveBack = value; }
		}

		/// <summary>
		/// Gets a value that indicates	wheter the panel can navigate to the
		/// next panel.
		/// </summary>
		/// <value></value>
		[DefaultValue(false)]
		public bool CanMoveNext
		{
			get { return this.canMoveNext && this.IsValid(); }
			set { this.canMoveNext = value; }
		}

		/// <summary>
		/// Gets a value that indicates	wheter the panel should	allow navigation to
		/// the	final panel.
		/// </summary>
		/// <value></value>
		[DefaultValue(false)]
		public bool CanFinish
		{
			get { return this.canFinish && this.IsValid(); }
			set { this.canFinish = value; }
		}

		/// <summary>
		/// Gets a value that indicates	if the wizard can be canceled.
		/// </summary>
		/// <value></value>
		[DefaultValue(false)]
		public bool CanCancel
		{
			get { return this.canCancel; }
			set { this.canCancel = value; }
		}

		/// <summary>
		/// Gets or	the	return back	step.
		/// </summary>
		/// <value>The previous	step to	navigate to.</value>
		[Browsable(false)]
		public WizardStep PreviousStep
		{
			get
			{
				if (this.backSteps.Count == 0)
				{
					return null;
				}
				return this.GetBackStep();
			}
		}

		/// <summary>
		/// Gets the next step.
		/// </summary>
		/// <value>The next	step to	navigate to.</value>
		/// <remarks>By	default	it will	return always the step with	index 0	in the collection of <see cref="WizardStep.NextSteps">Next Steps</see></remarks>
		[Browsable(false)]
		public WizardStep NextStep
		{
			get
			{
				if (this.NextSteps.Count == 0)
				{
					return null;
				}
				return this.GetNextStep();
			}
		}

		/// <summary>
		/// Gets the collection	of steps that the current step can navigate	to.
		/// </summary>
		/// <value>An <see cref="WizardStepCollection"/> object.</value>
		/// <remarks>This collection handles the steps we can navigate to througth the next	buttong	of a Wizard.</remarks>
		[Browsable(false)]
		public WizardStepCollection NextSteps
		{
			get
			{
				if (this.nextSteps == null)
				{
					this.nextSteps = new WizardStepCollection();
				}

				return this.nextSteps;
			}
		}

		/// <summary>
		/// Gets the collection	of steps that the current step can navigate	to.
		/// </summary>
		/// <value>An <see cref="WizardStepCollection"/> object.</value>
		/// <remarks>This collection handles the steps we can navigate to througth the next	buttong	of a Wizard.</remarks>
		[Browsable(false)]
		public WizardStepCollection BackSteps
		{
			get
			{
				if (this.backSteps == null)
				{
					this.backSteps = new WizardStepCollection();
				}

				return this.backSteps;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStep</b>	class.
		/// </summary>
		protected WizardStep()
		{
			InitializeComponent();
		}

		#endregion

		#region Overriden methods

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Perform	the	necessary operations before	the	step gets shown	in the Wizard.
		/// </summary>
		public virtual void ShowStep()
		{
			this.PerformValidation();
		}

		/// <summary>
		/// Perform	the	necessary operations before	the	step gets hidden in	the	Wizard.
		/// </summary>
		public virtual void HideStep()
		{
			// Remove the Validation handler
			this.ValidateStep = null;

			// Save	step settings
			this.SaveSettings();
		}

		/// <summary>
		/// Performs the step validations.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsValid()
		{
			return true;
		}


		/// <summary>
		/// Saves step settings.
		/// </summary>
		public virtual void SaveSettings()
		{
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Performs step validation
		/// </summary>
		protected virtual void PerformValidation()
		{
			if (this.ValidateStep != null)
			{
				this.ValidateStep(this, new CancelEventArgs(!this.IsValid()));
			}
		}

		/// <summary>
		/// Returns	the	back step to navigate to.
		/// </summary>
		/// <returns>A <see	cref="WizardStep"/>	object.</returns>
		protected virtual WizardStep GetBackStep()
		{
			return this.BackSteps[0];
		}

		/// <summary>
		/// Returns	the	next step to navigate to.
		/// </summary>
		/// <returns>A <see	cref="WizardStep"/>	object.</returns>
		protected virtual WizardStep GetNextStep()
		{
			return this.NextSteps[0];
		}

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method	for	Designer support - do not modify 
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ErrorProvider = new System.Windows.Forms.ErrorProvider();
			//((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
			// 
			// ErrorProvider
			// 
			this.ErrorProvider.ContainerControl = this;
			// 
			// WizardStep
			// 
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.Name = "WizardStep";
			this.Size = new System.Drawing.Size(492, 317);
			//((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
		}

		#endregion
	}
}

#endif