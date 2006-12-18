/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

#if (NET)

using System;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Design.Parameters
{
	internal class FbParameterCollectionEditor : System.ComponentModel.Design.CollectionEditor
	{
		#region · Fields ·

		private FbParameterCollection parameters;

		#endregion

		#region · Constructors ·

		public FbParameterCollectionEditor(Type type) : base(type)
		{
		}

		#endregion

		#region · Methods ·

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			this.parameters = (FbParameterCollection)value;
			return base.EditValue(context, provider, value);
		}

		#endregion

		#region · Protected Methods ·

		protected override object CreateInstance(Type type)
		{
			FbParameter parameter = (FbParameter)base.CreateInstance(type);
			
			parameter.ParameterName = this.GenerateParameterName("Parameter");

			return parameter;
		}

		#endregion

		#region · Private Methods ·

		private string GenerateParameterName(string prefix)
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

#endif