/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net), Jean Ressouche, Rafael Almeida (ralms@ralms.net)

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal
{
	public class FbPropertyBuilderAnnotations : FbPropertyAnnotations
	{
		public FbPropertyBuilderAnnotations(InternalPropertyBuilder internalBuilder, ConfigurationSource configurationSource)
			: base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
		{ }

		public new virtual bool ColumnName(string value)
			=> SetColumnName(value);

		public new virtual bool ColumnType(string value)
			=> SetColumnType(value);

		public new virtual bool DefaultValueSql(string value)
			=> SetDefaultValueSql(value);

		public new virtual bool ComputedColumnSql(string value)
			=> SetComputedColumnSql(value);

		public new virtual bool DefaultValue(object value)
			=> SetDefaultValue(value);

		public new virtual bool ValueGenerationStrategy(FbValueGenerationStrategy? value)
		{
			if (!SetValueGenerationStrategy(value))
			{
				return false;
			}

			return true;
		}
	}
}
