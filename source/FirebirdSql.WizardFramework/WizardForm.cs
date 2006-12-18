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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FirebirdSql.WizardFramework
{
	/// <summary>
	/// Represents the base	form for Wizard	implementations.
	/// </summary>
	internal class WizardForm : Form
	{
		#region Events

		/// <summary>
		/// Occurs during the wizard initialization.
		/// </summary>
		[Category("Wizard")]
		public event EventHandler Initialize;

		/// <summary>
		/// Occurs during the wizard initialization.
		/// </summary>
		/// <remarks>The event allows to define	the	steps that will	be used	in the wizard.</remarks>
		[Category("Wizard")]
		public event EventHandler LoadSteps;

		/// <summary>
		/// Occurs before the <b>Cancel</b>	button is clicked.
		/// </summary>
		[Category("Wizard")]
		public event CancelEventHandler BeforeCancel;

		/// <summary>
		/// Occurs before the <b>Back</b> button is	clicked.
		/// </summary>
		[Category("Wizard")]
		public event CancelEventHandler BeforeMoveBack;

		/// <summary>
		/// Occurs before the <b>Next</b> button is	clicked.
		/// </summary>
		[Category("Wizard")]
		public event CancelEventHandler BeforeMoveNext;

		/// <summary>
		/// Occurs before the <b>Finish</b>	button is clicked.
		/// </summary>
		[Category("Wizard")]
		public event CancelEventHandler BeforeFinish;

		#endregion

		#region Designer Fields

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		protected Panel displayArea;
		private System.Windows.Forms.Panel pnlBottom;
		private System.Windows.Forms.Button cmdFinish;
		private System.Windows.Forms.Button cmdNext;
		private System.Windows.Forms.Button cmdBack;
		private System.Windows.Forms.Button cmdCancel;
		private Splitter separator;

		#endregion

		#region Wizard fields

		private int stepIndex;
		private WizardStepCollection steps;
		private CancelEventHandler validateStepHandler;

		#endregion

		#region Protected Properties

		/// <summary>
		/// Gets the collection	of steps that will be used in the Wizard.
		/// </summary>
		/// <value></value>
		protected WizardStepCollection Steps
		{
			get
			{
				if (this.steps == null)
				{
					this.steps = new WizardStepCollection();
				}

				return this.steps;
			}
		}

		/// <summary>
		/// Gets the zero-based	index of the current step.
		/// </summary>
		/// <value></value>
		protected int StepIndex
		{
			get { return this.stepIndex; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>WizardForm</b>	class.
		/// </summary>
		protected WizardForm()
		{
			InitializeComponent();
		}

		#endregion

		#region Dispose	Method

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

		#region Windows	Form Designer generated	code

		/// <summary>
		/// Required method	for	Designer support - do not modify
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.displayArea = new System.Windows.Forms.Panel();
			this.pnlBottom = new System.Windows.Forms.Panel();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.cmdBack = new System.Windows.Forms.Button();
			this.cmdNext = new System.Windows.Forms.Button();
			this.cmdFinish = new System.Windows.Forms.Button();
			this.separator = new System.Windows.Forms.Splitter();
			this.pnlBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// displayArea
			// 
			this.displayArea.Dock = System.Windows.Forms.DockStyle.Top;
			this.displayArea.Location = new System.Drawing.Point(0, 0);
			this.displayArea.Name = "displayArea";
			this.displayArea.Size = new System.Drawing.Size(493, 310);
			this.displayArea.TabIndex = 2;
			// 
			// pnlBottom
			// 
			this.pnlBottom.Controls.Add(this.cmdCancel);
			this.pnlBottom.Controls.Add(this.cmdBack);
			this.pnlBottom.Controls.Add(this.cmdNext);
			this.pnlBottom.Controls.Add(this.cmdFinish);
			this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlBottom.Location = new System.Drawing.Point(0, 310);
			this.pnlBottom.Name = "pnlBottom";
			this.pnlBottom.Size = new System.Drawing.Size(493, 46);
			this.pnlBottom.TabIndex = 1;
			// 
			// cmdCancel
			// 
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(12, 12);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.TabIndex = 3;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// cmdBack
			// 
			this.cmdBack.Location = new System.Drawing.Point(251, 12);
			this.cmdBack.Name = "cmdBack";
			this.cmdBack.TabIndex = 2;
			this.cmdBack.Text = "&Back";
			this.cmdBack.Click += new System.EventHandler(this.cmdBack_Click);
			// 
			// cmdNext
			// 
			this.cmdNext.Location = new System.Drawing.Point(328, 12);
			this.cmdNext.Name = "cmdNext";
			this.cmdNext.TabIndex = 1;
			this.cmdNext.Text = "&Next";
			this.cmdNext.Click += new System.EventHandler(this.cmdNext_Click);
			// 
			// cmdFinish
			// 
			this.cmdFinish.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdFinish.Location = new System.Drawing.Point(405, 12);
			this.cmdFinish.Name = "cmdFinish";
			this.cmdFinish.TabIndex = 0;
			this.cmdFinish.Text = "&Finish";
			this.cmdFinish.Click += new System.EventHandler(this.cmdFinish_Click);
			// 
			// separator
			// 
			this.separator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.separator.Dock = System.Windows.Forms.DockStyle.Top;
			this.separator.Enabled = false;
			this.separator.Location = new System.Drawing.Point(0, 310);
			this.separator.Name = "separator";
			this.separator.Size = new System.Drawing.Size(493, 4);
			this.separator.TabIndex = 3;
			this.separator.TabStop = false;
			// 
			// WizardForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(493, 356);
			this.Controls.Add(this.separator);
			this.Controls.Add(this.pnlBottom);
			this.Controls.Add(this.displayArea);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WizardForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.pnlBottom.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Initializes	the	Wizard.
		/// </summary>
		protected void InitializeWizard()
		{
			this.stepIndex = 0;

			if (this.Initialize != null)
			{
				this.Initialize(this, new EventArgs());
			}
			if (this.LoadSteps != null)
			{
				this.LoadSteps(this, new EventArgs());
			}

			this.validateStepHandler = new CancelEventHandler(ValidateStep);

			this.ShowStep(this.Steps[this.stepIndex]);
		}

		/// <summary>
		/// Shows the specified	step in	the	Wizard.
		/// </summary>
		/// <param name="step">The <see cref="WizardStep" /> to	show.</param>
		protected void ShowStep(WizardStep step)
		{
			if (step == null)
			{
				throw new ArgumentNullException("step cannot be null.");
			}

			// Configure panel for correct visualization
			step.Dock = DockStyle.Fill;

			// Updated buttons based on	Panel Information
			this.cmdCancel.Enabled	= step.CanCancel;
			this.cmdBack.Enabled	= step.CanMoveBack;
			this.cmdNext.Enabled	= step.CanMoveNext && (this.stepIndex != (this.Steps.Count - 1));
			this.cmdFinish.Enabled	= step.CanFinish;

			// Clear the step that is being	displayed
			this.displayArea.Controls.Clear();

			// Display the new step
			this.displayArea.Controls.Add(step);

			// Add validation handler
			step.ValidateStep += this.validateStepHandler;

			// Perform operations needed on	the	step for showing it
			step.ShowStep();
		}

		#endregion

		#region Protected step handling	methods

		/// <summary>
		/// Returns	the	current	step.
		/// </summary>
		/// <returns>A <see	cref="WizardStep" /> object.</returns>
		protected WizardStep GetCurrentStep()
		{
			return this.Steps[this.stepIndex] as WizardStep;
		}

		/// <summary>
		/// Returns	the	previous step.
		/// </summary>
		/// <returns>A <see	cref="WizardStep" /> object.</returns>
		protected WizardStep GetPreviousStep()
		{
			WizardStep step = this.GetCurrentStep();

			if (step.PreviousStep != null)
			{
				this.stepIndex = this.GetStepIndex(step.PreviousStep);

				return step.PreviousStep;
			}

			if ((this.stepIndex - 1) >= 0)
			{
				return this.Steps[--this.stepIndex] as WizardStep;
			}

			return step;
		}

		/// <summary>
		/// Returns	the	next step.
		/// </summary>
		/// <returns>A <see	cref="WizardStep" /> object.</returns>
		protected WizardStep GetNextStep()
		{
			WizardStep step = this.GetCurrentStep();

			if (step.NextStep != null)
			{
				this.stepIndex = this.GetStepIndex(step.NextStep);

				return step.NextStep;
			}

			if ((this.stepIndex + 1) < this.Steps.Count)
			{
				return this.Steps[++this.stepIndex] as WizardStep;
			}

			return this.GetCurrentStep();
		}

		/// <summary>
		/// Returns	the	zero-based index of	the	given step..
		/// </summary>
		/// <param name="step">A <see cref="WizardStep"/> object.</param>
		/// <returns>The zero-based	index of the step.</returns>
		protected int GetStepIndex(WizardStep step)
		{
			int index = this.Steps.IndexOf(step);

			if (index == -1)
			{
				throw new InvalidOperationException("The previous step cannot be found.");
			}

			return index;
		}

		#endregion

		#region Navigation methods

		private void Cancel()
		{
			if (this.BeforeCancel != null)
			{
				CancelEventArgs e = new CancelEventArgs();
				this.BeforeCancel(this, e);

				if (e.Cancel)
				{
					return;
				}
			}

			this.validateStepHandler = null;
			this.displayArea.Controls.Clear();
			this.Steps.Clear();
			this.Close();
		}

		private void Finish()
		{
			if (this.BeforeFinish != null)
			{
				CancelEventArgs e = new CancelEventArgs();
				this.BeforeFinish(this, e);

				if (e.Cancel)
				{
					return;
				}
			}

			this.validateStepHandler = null;
			this.GetCurrentStep().HideStep();
			this.displayArea.Controls.Clear();
			this.Steps.Clear();
			this.Close();
		}

		private void MoveBack()
		{
			if (this.BeforeMoveBack != null)
			{
				CancelEventArgs e = new CancelEventArgs();
				this.BeforeMoveBack(this, e);

				if (e.Cancel)
				{
					return;
				}
			}

			this.GetCurrentStep().HideStep();
			this.ShowStep(this.GetPreviousStep());
		}

		private void MoveNext()
		{
			if (this.BeforeMoveNext != null)
			{
				CancelEventArgs e = new CancelEventArgs();
				this.BeforeMoveNext(this, e);

				if (e.Cancel)
				{
					return;
				}
			}

			WizardStep step = this.GetCurrentStep();

			if (step.IsValid())
			{
				this.GetCurrentStep().HideStep();
				this.ShowStep(this.GetNextStep());
			}
		}

		#endregion

		#region Step Validation	Event Handler

		private void ValidateStep(object sender, CancelEventArgs e)
		{
			this.cmdNext.Enabled = this.GetCurrentStep().CanMoveNext && !e.Cancel && (this.stepIndex != (this.Steps.Count - 1));
			this.cmdFinish.Enabled = this.GetCurrentStep().CanFinish && !e.Cancel;
		}

		#endregion

		#region CommandButton handlers

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.Cancel();
		}

		private void cmdBack_Click(object sender, EventArgs e)
		{
			this.MoveBack();
		}

		private void cmdNext_Click(object sender, EventArgs e)
		{
			this.MoveNext();
		}

		private void cmdFinish_Click(object sender, EventArgs e)
		{
			this.Finish();
		}

		#endregion
	}
}

#endif