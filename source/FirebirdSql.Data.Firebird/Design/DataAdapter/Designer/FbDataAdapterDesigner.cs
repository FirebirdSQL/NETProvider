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
using System.Windows.Forms;

using FirebirdSql.WizardFramework;

namespace FirebirdSql.Data.Firebird.Design
{
	/// <summary>
	/// Designer for the <see cref="FbDataAdapter" /> class.
	/// </summary>
	internal sealed class FbDataAdapterDesigner : ComponentDesigner
	{
		#region Properties

		/// <summary>
		/// Gets the Designer Component	as a FbDataAdapter instance.
		/// </summary>
		/// <value>An <see cref="FbDataAdapter"/> object</value>
		public FbDataAdapter DataAdapter
		{
			get { return (FbDataAdapter)this.Component; }
		}

		/// <summary>
		/// Gets the design-time verbs supported by the	component that is associated with the designer.
		/// </summary>
		/// <value>
		/// A DesignerVerbCollection of	DesignerVerb objects, or null if no	designer verbs are available. 
		/// </value>
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] verbs = new DesignerVerb[]
					{
						new	DesignerVerb("Configure Data Adapter", new EventHandler(OnConfigureDataAdapter)),
						new	DesignerVerb("Generate DataSet", new EventHandler(OnGenerateTypeDataSet))
					};

				return new DesignerVerbCollection(verbs);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes	a new Instance of the <b>FbDataAdapterDesigner</b> class.
		/// </summary>
		public FbDataAdapterDesigner() : base()
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

#if	(NET_2_0)

		public override void InitializeNewComponent(IDictionary defaultValues)
		{
			base.InitializeNewComponent(defaultValues);

			this.InitializeCommands();

			this.RunConfigurationWizard();
		}

#else

		public override	void OnSetComponentDefaults()
		{
			base.OnSetComponentDefaults();

			this.InitializeCommands();

			this.RunConfigurationWizard();
		}

#endif

		#endregion

		#region DesignerVerbs handlers

		/// <summary>
		/// Handles the "Configure Data Adapter" designer option.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConfigureDataAdapter(object sender, EventArgs e)
		{
			this.RunConfigurationWizard();
		}

		/// <summary>
		/// Handles	the	"Generate Type DataSet"	designer option.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnGenerateTypeDataSet(object sender, EventArgs e)
		{
			this.RunDataSetGenerator();
		}

		#endregion

		#region Private	methods

		private void InitializeCommands()
		{
			FbDataAdapter		adapter		= this.DataAdapter;
			IDesignerHost		host		= (IDesignerHost)this.Component.Site.GetService(typeof(IDesignerHost));
			DesignerTransaction transaction = host.CreateTransaction("CommandInitialization");

			try
			{
				if (adapter.SelectCommand == null)
				{
					FbCommand select = (FbCommand)host.CreateComponent(typeof(FbCommand));
					select.DesignTimeVisible = false;

					adapter.SelectCommand = select;
				}

				if (adapter.InsertCommand == null)
				{
					FbCommand insert = (FbCommand)host.CreateComponent(typeof(FbCommand));
					insert.DesignTimeVisible = false;

					adapter.InsertCommand = insert;
				}

				if (adapter.UpdateCommand == null)
				{
					FbCommand update = (FbCommand)host.CreateComponent(typeof(FbCommand));
					update.DesignTimeVisible = false;

					adapter.UpdateCommand = update;
				}

				if (adapter.DeleteCommand == null)
				{
					FbCommand delete = (FbCommand)host.CreateComponent(typeof(FbCommand));
					delete.DesignTimeVisible = false;

					adapter.DeleteCommand = delete;
				}

				transaction.Commit();
			}
			catch (Exception ex)
			{
				if (transaction != null)
				{
					transaction.Cancel();
				}
				MessageBox.Show(ex.ToString());
			}
		}

		private void RunConfigurationWizard()
		{
			IDesignerHost host = (IDesignerHost)this.Component.Site.GetService(typeof(IDesignerHost));
			DesignerTransaction transaction = host.CreateTransaction("ConfigureDataAdapter");
			FbDataAdapterConfigurationWizard wizard = null;

			try
			{
#if (NET_1_0 || NET_1_1)

				// InitializeCommands
				if (this.DataAdapter.SelectCommand == null)
				{
					this.InitializeCommands();
				}

#endif

				// Register	wizard settings			   
				WizardSettings settings = FbDataAdapterWizardSettings.Register();
				settings.Add(FbDataAdapterWizardSettings.DesignerHostKey, host);
				settings.Add(FbDataAdapterWizardSettings.DataAdapterKey, this.DataAdapter);

				// Create the wizard
				wizard = new FbDataAdapterConfigurationWizard();
				wizard.ShowDialog();

				// Commit or rollback the changes
				if (wizard.DialogResult == DialogResult.OK)
				{
					transaction.Commit();
				}
				else
				{
					transaction.Cancel();
				}
			}
			catch (Exception ex)
			{
				if (transaction != null)
				{
					transaction.Cancel();
				}
				MessageBox.Show(ex.ToString());
				throw;
			}
			finally
			{
				FbDataAdapterWizardSettings.Unregister();
				if (wizard != null)
				{
					wizard.Dispose();
				}
			}
		}

		private void RunDataSetGenerator()
		{
			IDesignerHost host = (IDesignerHost)this.Component.Site.GetService(typeof(IDesignerHost));
			DesignerTransaction transaction = host.CreateTransaction("DataSetGenerator");
			FbDataAdapterDataSetGenerator generator = null;

			try
			{
				generator = new FbDataAdapterDataSetGenerator(host, this.DataAdapter);
				generator.ShowDialog();

				if (generator.DialogResult == DialogResult.OK)
				{
					transaction.Commit();
				}
				else
				{
					transaction.Cancel();
				}
			}
			catch
			{
				if (transaction != null)
				{
					transaction.Cancel();
				}
				throw;
			}
			finally
			{
				if (generator != null)
				{
					generator.Dispose();
				}
			}
		}

		#endregion
	}
}

#endif