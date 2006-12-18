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
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Design
{
	internal class FbParameterCollectionEditor : System.ComponentModel.Design.CollectionEditor
	{
		#region Fields

		private FbParameterCollection parameters;

		#endregion

		#region Constructors

		public FbParameterCollectionEditor(Type type) : base(type)
		{
			parameters = null;
		}

		#endregion

		#region Methods

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			this.parameters = (FbParameterCollection)value;
			return base.EditValue(context, provider, value);
		}

		#endregion

		#region Protected Methods

		protected override object CreateInstance(Type type)
		{
			FbParameter parameter = (FbParameter)base.CreateInstance(type);
			
			parameter.ParameterName = this.generateParameterName("Parameter");

			return parameter;
		}

		#endregion

		#region Private Methods

		private string generateParameterName(string prefix)
		{
			string	parameterName = String.Empty;
			int		index	= parameters.Count + 1;
			bool	valid	= false;
			
			while (!valid)
			{
				parameterName = prefix + index.ToString(CultureInfo.CurrentUICulture);
				if (parameters.IndexOf(parameterName) == -1)
				{
					valid = true;
				}
				index++;
			}

			return parameterName;
		}

		#endregion
	}
}
