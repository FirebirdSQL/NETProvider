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
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace FirebirdSql.Data.Firebird.Design
{
	internal class CommandTextUIEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc = null;

		protected virtual void SetEditorProps(FbCommand editingInstance, FbCommand editor) 
		{
			if (editingInstance != null)
			{
				editor.Connection	= editingInstance.Connection;
				editor.Transaction	= editingInstance.Transaction;
				editor.CommandText	= editingInstance.CommandText;
			}
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null &&
				provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

				if (edSvc != null)
				{
					FbCommand command = new FbCommand();
					
					SetEditorProps((FbCommand)context.Instance, command);

					CommandTextEditor editor = new CommandTextEditor(command);
					edSvc.ShowDialog(editor);

					if (editor.DialogResult == DialogResult.OK)
					{
						value = command.CommandText;
					}
				}
			}

			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null) 
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);			
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return false;
		}
	}
}
