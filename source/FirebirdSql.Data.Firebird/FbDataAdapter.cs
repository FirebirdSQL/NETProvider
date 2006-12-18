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
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/overview/*'/>
	[ToolboxItem(true),
	ToolboxBitmap(typeof(FbDataAdapter), "Resources.FbDataAdapter.bmp"),
	DefaultEvent("RowUpdated")]
	public sealed class FbDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region Events

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/event[@name="RowUpdated"]/*'/>
		public event FbRowUpdatedEventHandler RowUpdated
		{
			add 
			{ 
				base.Events.AddHandler(EventRowUpdated, value); 
			}
			
			remove 
			{ 
				base.Events.RemoveHandler(EventRowUpdated, value); 
			}
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/event[@name="RowUpdating"]/*'/>
		public event FbRowUpdatingEventHandler RowUpdating
		{
			add 
			{ 
				base.Events.AddHandler(EventRowUpdating, value); 
			}
			
			remove 
			{ 
				base.Events.RemoveHandler(EventRowUpdating, value); 
			}
		}

		#endregion

		#region Static Fields

		private static readonly object EventRowUpdated	= new object(); 
		private static readonly object EventRowUpdating = new object(); 
		
		#endregion

		#region Fields

		private FbCommand	selectCommand;
		private FbCommand	insertCommand;
		private FbCommand	updateCommand;
		private FbCommand	deleteCommand;

		private bool		disposed;

		#endregion

		#region Properties
		
		IDbCommand IDbDataAdapter.SelectCommand 
		{
			get { return this.selectCommand; }
			set { this.selectCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="SelectCommand"]/*'/>
		[Category("Fill"), DefaultValue(null)]
		public FbCommand SelectCommand 
		{
			get { return this.selectCommand; }
			set { this.selectCommand = value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand 
		{
			get { return this.insertCommand; }
			set { this.insertCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="InsertCommand"]/*'/>
		[Category("Update"), DefaultValue(null)]
		public FbCommand InsertCommand 
		{
			get { return this.insertCommand; }
			set { this.insertCommand = value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand 
		{
			get { return this.updateCommand; }
			set { this.updateCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="UpdateCommand"]/*'/>		
		[Category("Update"), DefaultValue(null)]
		public FbCommand UpdateCommand 
		{
			get { return this.updateCommand; }
			set { this.updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand 
		{
			get { return this.deleteCommand; }
			set { this.deleteCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="DeleteCommand"]/*'/>
		[Category("Update"), DefaultValue(null)]
		public FbCommand DeleteCommand 
		{
			get { return this.deleteCommand; }
			set { this.deleteCommand = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor"]/*'/>
		public FbDataAdapter() : base()
		{
			GC.SuppressFinalize(this);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(FbCommand)"]/*'/>
		public FbDataAdapter(FbCommand selectCommand) : this()
		{
			this.SelectCommand = selectCommand;
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(System.String,FbConnection)"]/*'/>		
		public FbDataAdapter(
			string selectCommandText, 
			FbConnection selectConnection) : this()
		{
			this.SelectCommand = new FbCommand(
				selectCommandText, 
				selectConnection);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(System.String,System.String)"]/*'/>
		public FbDataAdapter(
			string selectCommandText, 
			string selectConnectionString) : this()
		{
			this.SelectCommand = new FbCommand(
				selectCommandText, 
				new FbConnection(selectConnectionString));
		}

		#endregion

		#region IDisposable Methods

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						if (disposing)
						{
							if (this.SelectCommand != null)
							{
								this.SelectCommand.Dispose();
							}
							if (this.InsertCommand != null)
							{
								this.InsertCommand.Dispose();
							}
							if (this.UpdateCommand != null)
							{
								this.UpdateCommand.Dispose();
							}
						}
					
						// release any unmanaged resources
					
						this.disposed = true;
					}
					finally 
					{
						base.Dispose(disposing);
					}
				}
			}
		}

		#endregion

		#region Protected Methods

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="CreateRowUpdatingEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
			DataRow				dataRow, 
			IDbCommand			command, 
			StatementType		statementType, 
			DataTableMapping		tableMapping)
		{
			return new FbRowUpdatingEventArgs(
				dataRow, 
				command, 
				statementType, 
				tableMapping);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="CreateRowUpdatedEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
			DataRow				dataRow, 
			IDbCommand			command, 
			StatementType		statementType, 
			DataTableMapping		tableMapping)
		{
			return new FbRowUpdatedEventArgs(
				dataRow, 
				command, 
				statementType, 
				tableMapping);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="OnRowUpdating(System.Data.Common.RowUpdatingEventArgs)"]/*'/>
		protected override void OnRowUpdating(RowUpdatingEventArgs value)
		{
			FbRowUpdatingEventHandler handler = null;
			
			handler = (FbRowUpdatingEventHandler) base.Events[EventRowUpdating];

			if ((null != handler)					&& 
				(value is FbRowUpdatingEventArgs)	&&
				(value != null))
			{
				handler(this, (FbRowUpdatingEventArgs) value);
			}
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="OnRowUpdated(System.Data.Common.RowUpdatedEventArgs)"]/*'/>
		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			FbRowUpdatedEventHandler handler = null;
			
			handler = (FbRowUpdatedEventHandler) base.Events[EventRowUpdated];
			
			if ((handler != null)					&& 
				(value is FbRowUpdatedEventArgs)	&&
				(value != null))
			{
				handler(this, (FbRowUpdatedEventArgs) value);
			}
		}

		#endregion
	}
}
