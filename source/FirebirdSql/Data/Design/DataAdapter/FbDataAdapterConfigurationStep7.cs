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

// #if	(NET)

using System;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

using FirebirdSql.WizardFramework;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Design.DataAdapter
{
	/// <summary>
	/// Step 7 of the Firebird Data	Adapter	Configuration Wizard
	/// </summary>
	internal class FbDataAdapterConfigurationStep7 : ActionStep
	{
		private Panel result3;
		private Label lblMessage3;
		private Panel result2;
		private Label lblMessage2;
		private Panel result1;
		private Label lblMessage1;
		private Label label3;
		private Label lblFinish;
		private Label lblMessage;
		private TextBox txtExceptionDetails;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region · Constructors ·

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStep</b>	class.
		/// </summary>
		public FbDataAdapterConfigurationStep7()
		{
			InitializeComponent();
		}

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method	for	Designer support - do not modify 
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblMessage1 = new System.Windows.Forms.Label();
            this.result1 = new System.Windows.Forms.Panel();
            this.result2 = new System.Windows.Forms.Panel();
            this.lblMessage2 = new System.Windows.Forms.Label();
            this.result3 = new System.Windows.Forms.Panel();
            this.lblMessage3 = new System.Windows.Forms.Label();
            this.lblFinish = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.txtExceptionDetails = new System.Windows.Forms.TextBox();
            this.designArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(353, 22);
            this.lblCaption.Text = "Data Adapter configuration results";
            // 
            // lblDescription
            // 
            this.lblDescription.Size = new System.Drawing.Size(366, 17);
            this.lblDescription.Text = "Review the list of tasks the wizard has performed";
            // 
            // designArea
            // 
            this.designArea.Controls.Add(this.txtExceptionDetails);
            this.designArea.Controls.Add(this.lblMessage);
            this.designArea.Controls.Add(this.label3);
            this.designArea.Controls.Add(this.lblFinish);
            this.designArea.Controls.Add(this.result3);
            this.designArea.Controls.Add(this.lblMessage3);
            this.designArea.Controls.Add(this.result2);
            this.designArea.Controls.Add(this.lblMessage2);
            this.designArea.Controls.Add(this.result1);
            this.designArea.Controls.Add(this.lblMessage1);
            // 
            // lblMessage1
            // 
            this.lblMessage1.AutoSize = true;
            this.lblMessage1.Location = new System.Drawing.Point(29, 66);
            this.lblMessage1.Name = "lblMessage1";
            this.lblMessage1.Size = new System.Drawing.Size(145, 13);
            this.lblMessage1.TabIndex = 0;
            this.lblMessage1.Text = "Generated SELECT statement";
            this.lblMessage1.Visible = false;
            // 
            // result1
            // 
            this.result1.BackColor = System.Drawing.Color.PaleGreen;
            this.result1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.result1.Location = new System.Drawing.Point(13, 68);
            this.result1.Name = "result1";
            this.result1.Size = new System.Drawing.Size(10, 10);
            this.result1.TabIndex = 1;
            this.result1.Visible = false;
            // 
            // result2
            // 
            this.result2.BackColor = System.Drawing.Color.PaleGreen;
            this.result2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.result2.Location = new System.Drawing.Point(13, 93);
            this.result2.Name = "result2";
            this.result2.Size = new System.Drawing.Size(10, 10);
            this.result2.TabIndex = 4;
            this.result2.Visible = false;
            // 
            // lblMessage2
            // 
            this.lblMessage2.AutoSize = true;
            this.lblMessage2.Location = new System.Drawing.Point(29, 91);
            this.lblMessage2.Name = "lblMessage2";
            this.lblMessage2.Size = new System.Drawing.Size(254, 13);
            this.lblMessage2.TabIndex = 3;
            this.lblMessage2.Text = "Generated INSERT, UPDATE and delete statements.";
            this.lblMessage2.Visible = false;
            // 
            // result3
            // 
            this.result3.BackColor = System.Drawing.Color.PaleGreen;
            this.result3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.result3.Location = new System.Drawing.Point(13, 118);
            this.result3.Name = "result3";
            this.result3.Size = new System.Drawing.Size(10, 10);
            this.result3.TabIndex = 7;
            this.result3.Visible = false;
            // 
            // lblMessage3
            // 
            this.lblMessage3.AutoSize = true;
            this.lblMessage3.Location = new System.Drawing.Point(29, 116);
            this.lblMessage3.Name = "lblMessage3";
            this.lblMessage3.Size = new System.Drawing.Size(132, 13);
            this.lblMessage3.TabIndex = 6;
            this.lblMessage3.Text = "Generated TableMappings.";
            this.lblMessage3.Visible = false;
            // 
            // lblFinish
            // 
            this.lblFinish.AutoSize = true;
            this.lblFinish.Location = new System.Drawing.Point(13, 217);
            this.lblFinish.Name = "lblFinish";
            this.lblFinish.Size = new System.Drawing.Size(278, 13);
            this.lblFinish.TabIndex = 8;
            this.lblFinish.Text = "To apply these settings to your DataAdapter, click Finish.";
            this.lblFinish.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Details:";
            this.label3.Visible = false;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(13, 17);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(234, 13);
            this.lblMessage.TabIndex = 10;
            this.lblMessage.Text = "The DataAdapter {0} was configurred correctly.";
            this.lblMessage.Visible = false;
            // 
            // txtExceptionDetails
            // 
            this.txtExceptionDetails.AutoSize = false;
            this.txtExceptionDetails.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.txtExceptionDetails.Location = new System.Drawing.Point(9, 138);
            this.txtExceptionDetails.Multiline = true;
            this.txtExceptionDetails.Name = "txtExceptionDetails";
            this.txtExceptionDetails.ReadOnly = true;
            this.txtExceptionDetails.Size = new System.Drawing.Size(470, 73);
            this.txtExceptionDetails.TabIndex = 11;
            this.txtExceptionDetails.Visible = false;
            // 
            // FbDataAdapterConfigurationStep7
            // 
            this.CanFinish = true;
            this.CanMoveNext = false;
            this.Name = "FbDataAdapterConfigurationStep7";
            this.designArea.ResumeLayout(false);
            this.designArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		#region · Overriden methods ·

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

		protected override WizardStep GetNextStep()
		{
			int ct = FbDataAdapterWizardSettings.GetCommandType();

			return this.BackSteps[ct];
		}

		public override void ShowStep()
		{
			int commandType = FbDataAdapterWizardSettings.GetCommandType();

			try
			{
				switch (commandType)
				{
					case 0:
						this.ConfigureWithSqlStatements();
						break;

					case 1:
						this.ConfigureWithNewStoredProcedures();
						break;

					case 2:
						this.ConfigureWithExistingStoredProcedures();
						break;
				}

				string name = FbDataAdapterWizardSettings.GetDataAdapter().Site.Name;

				this.lblMessage.Text				= String.Format(this.lblMessage.Text, name);
				this.lblMessage.Visible				= true;
				this.lblFinish.Visible				= true;
				this.txtExceptionDetails.Visible	= false;
				this.txtExceptionDetails.Text		= String.Empty;
			}
			catch
			{
			}

			this.PerformValidation();
		}

		public override void HideStep()
		{
			this.Initialize();
		}

		/// <summary>
		/// Performs the step validations.
		/// </summary>
		/// <returns></returns>
		public override bool IsValid()
		{
			return this.lblFinish.Visible;
		}

		#endregion

		#region · FbDataAdapter configuration methods ·

		private void ConfigureWithSqlStatements()
		{
			FbConnection connection = FbDataAdapterWizardSettings.GetConnection();
			FbDataAdapter adapter = FbDataAdapterWizardSettings.GetDataAdapter();
			bool generateDml = FbDataAdapterWizardSettings.GetDmlGeneration();

			try
			{
				// Configure the SELECT	command
				adapter.SelectCommand.Connection	= connection;
				adapter.SelectCommand.CommandType	= CommandType.Text;
				adapter.SelectCommand.CommandText	= FbDataAdapterWizardSettings.GetCommandText();

				// Try to prepare it to	see	if it's	valid.
				FbCommand command = new FbCommand(adapter.SelectCommand.CommandText, connection);
				try
				{
					command.Connection = connection;
					command.Connection.Open();
					command.Prepare();
				}
				catch
				{
					throw;
				}
				finally
				{
					if (command != null)
					{
						command.Connection.Close();
						command.Dispose();
					}
				}

				this.SetMessage("Generated SELECT statement", true);
			}
			catch (Exception ex)
			{
				this.SetMessage("Generated SELECT statement", false);
				this.ShowException(ex);
				throw;
			}

			if (generateDml)
			{
				this.GenerateDmlStatements(adapter);
			}
		}

		private void ConfigureWithNewStoredProcedures()
		{
		}

		private void ConfigureWithExistingStoredProcedures()
		{
			try
			{
				FbConnection	connection	= FbDataAdapterWizardSettings.GetConnection();
				FbDataAdapter	adapter		= FbDataAdapterWizardSettings.GetDataAdapter();

				// Select Stored Procedure
				adapter.SelectCommand.Connection	= connection;
				adapter.SelectCommand.CommandType	= CommandType.StoredProcedure;
				adapter.SelectCommand.CommandText	= FbDataAdapterWizardSettings.GetSelectStoredProcedure();
				this.DiscoverParameters(adapter.SelectCommand);

				// Insert Stored Procedure
				adapter.InsertCommand.Connection	= connection;
				adapter.InsertCommand.CommandType	= CommandType.StoredProcedure;
				adapter.InsertCommand.CommandText	= FbDataAdapterWizardSettings.GetInsertStoredProcedure();
				this.DiscoverParameters(adapter.InsertCommand);

				// Update Stored Procedure
				adapter.UpdateCommand.Connection	= connection;
				adapter.UpdateCommand.CommandType	= CommandType.StoredProcedure;
				adapter.UpdateCommand.CommandText	= FbDataAdapterWizardSettings.GetUpdateStoredProcedure();
				this.DiscoverParameters(adapter.UpdateCommand);

				// Delete Stored Procedure
				adapter.DeleteCommand.Connection	= connection;
				adapter.DeleteCommand.CommandType	= CommandType.StoredProcedure;
				adapter.DeleteCommand.CommandText	= FbDataAdapterWizardSettings.GetDeleteStoredProcedure();
				this.DiscoverParameters(adapter.DeleteCommand);

				this.SetMessage("Generated SELECT, INSERT, UPDATE and DELETE Stored Procedures.", true);
			}
			catch (Exception ex)
			{
				this.SetMessage("Generated SELECT, INSERT, UPDATE and DELETE Stored Procedures.", false);
				this.ShowException(ex);

				throw;
			}
		}

		private void GenerateDmlStatements(FbDataAdapter adapter)
		{
			FbConnection		connection	= FbDataAdapterWizardSettings.GetConnection();
			FbCommand			command		= new FbCommand(adapter.SelectCommand.CommandText, connection);
			FbCommandBuilder	builder		= new FbCommandBuilder(new FbDataAdapter(command));
			bool useQuotedIdentifiers = FbDataAdapterWizardSettings.GetUseQuotedIdentifiers();
            
			try
			{
				if (!useQuotedIdentifiers)
				{
					builder.QuotePrefix = "";
					builder.QuoteSuffix = "";
				}

                FbCommand insert = builder.GetInsertCommand();
                FbCommand update = builder.GetUpdateCommand();
                FbCommand delete = builder.GetDeleteCommand();

				// Insert command
				adapter.InsertCommand.CommandText	= builder.GetInsertCommand().CommandText;
				adapter.InsertCommand.Connection	= adapter.SelectCommand.Connection;
				adapter.InsertCommand.CommandType	= CommandType.Text;
				this.CopyParameters(insert, adapter.InsertCommand);

				// Update command
				adapter.UpdateCommand.CommandText	= builder.GetUpdateCommand().CommandText;
				adapter.UpdateCommand.Connection	= adapter.SelectCommand.Connection;
				adapter.UpdateCommand.CommandType	= CommandType.Text;
				this.CopyParameters(update, adapter.UpdateCommand);

				// Delete command
				adapter.DeleteCommand.CommandText	= builder.GetDeleteCommand().CommandText;
				adapter.DeleteCommand.Connection	= adapter.SelectCommand.Connection;
				adapter.DeleteCommand.CommandType	= CommandType.Text;
				this.CopyParameters(delete, adapter.DeleteCommand);

				this.SetMessage("Generated INSERT, UPDATE and DELETE statements.", true);
			}
			catch (Exception ex)
			{
				this.SetMessage("Generated INSERT, UPDATE and DELETE statements.", false);
				this.ShowException(ex);

				throw;
			}
			finally
			{
				builder.Dispose();
			}
		}

		private void SetMessage(string message, bool isValid)
		{
			if (!this.lblMessage1.Visible)
			{
				this.lblMessage1.Text = message;
				this.lblMessage1.Visible = true;

				this.result1.BackColor = ((isValid) ? Color.LimeGreen : Color.Red);
				this.result1.Visible = true;
			}
			else if (!this.lblMessage2.Visible)
			{
				this.lblMessage2.Text = message;
				this.lblMessage2.Visible = true;

				this.result2.BackColor = ((isValid) ? Color.LimeGreen : Color.Red);
				this.result2.Visible = true;
			}
			else if (!this.lblMessage3.Visible)
			{
				this.lblMessage3.Text = message;
				this.lblMessage3.Visible = true;

				this.result3.BackColor = ((isValid) ? Color.LimeGreen : Color.Red);
				this.result3.Visible = true;
			}
		}

		private void Initialize()
		{
			this.lblMessage.Visible				= false;
			this.lblMessage1.Visible			= false;
			this.lblMessage2.Visible			= false;
			this.lblMessage3.Visible			= false;
			this.result1.Visible				= false;
			this.result2.Visible				= false;
			this.result3.Visible				= false;
			this.lblFinish.Visible				= false;
			this.txtExceptionDetails.Visible	= false;
		}

		private void ShowException(Exception ex)
		{
			if (this.txtExceptionDetails.Text.Length == 0)
			{
				this.txtExceptionDetails.Text = ex.ToString();
				this.txtExceptionDetails.Visible = true;
			}
		}

		private void DiscoverParameters(FbCommand command)
		{
			if (command.CommandType == CommandType.StoredProcedure)
			{
				try
				{
					FbCommandBuilder.DeriveParameters(command);
				}
				catch
				{
				}
			}
		}

		private void CopyParameters(FbCommand source, FbCommand target)
		{
			target.Parameters.Clear();

			foreach (FbParameter parameter in source.Parameters)
			{
				target.Parameters.Add(((ICloneable)parameter).Clone());
			}
		}

		#endregion
	}
}

// #endif