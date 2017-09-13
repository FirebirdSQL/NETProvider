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

using FirebirdSql.EntityFrameworkCore.Firebird.Metadata;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions
{
	public static class FbPropertyBuilderExtensions
    {
		public static PropertyBuilder UseFirebirdIdentityColumn(this PropertyBuilder propertyBuilder)
		{
			GetFbInternalBuilder(propertyBuilder).ValueGenerationStrategy(FbValueGenerationStrategy.IdentityColumn);
			return propertyBuilder;
		}

		public static PropertyBuilder<TProperty> UseFirebirdIdentityColumn<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
			=> (PropertyBuilder<TProperty>)UseFirebirdIdentityColumn((PropertyBuilder)propertyBuilder);

		public static PropertyBuilder UseFirebirdSequenceTrigger(this PropertyBuilder propertyBuilder)
		{
			GetFbInternalBuilder(propertyBuilder).ValueGenerationStrategy(FbValueGenerationStrategy.SequenceTrigger);
			return propertyBuilder;
		}

		public static PropertyBuilder<TProperty> UseFirebirdSequenceTrigger<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
			=> (PropertyBuilder<TProperty>)UseFirebirdSequenceTrigger((PropertyBuilder)propertyBuilder);

		static FbPropertyBuilderAnnotations GetFbInternalBuilder(PropertyBuilder propertyBuilder)
			=> propertyBuilder.GetInfrastructure<InternalPropertyBuilder>().Firebird(ConfigurationSource.Explicit);
	}
}
