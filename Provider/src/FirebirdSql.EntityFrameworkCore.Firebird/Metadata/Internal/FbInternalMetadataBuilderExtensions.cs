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
	public static class FbInternalMetadataBuilderExtensions
	{
		public static RelationalModelBuilderAnnotations Firebird(this InternalModelBuilder builder, ConfigurationSource configurationSource)
			   => new RelationalModelBuilderAnnotations(builder, configurationSource);

		public static FbPropertyBuilderAnnotations Firebird(this InternalPropertyBuilder builder, ConfigurationSource configurationSource)
			=> new FbPropertyBuilderAnnotations(builder, configurationSource);

		public static RelationalEntityTypeBuilderAnnotations Firebird(this InternalEntityTypeBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalEntityTypeBuilderAnnotations(builder, configurationSource);

		public static RelationalKeyBuilderAnnotations Firebird(this InternalKeyBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalKeyBuilderAnnotations(builder, configurationSource);

		public static RelationalIndexBuilderAnnotations Firebird(this InternalIndexBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalIndexBuilderAnnotations(builder, configurationSource);

		public static RelationalForeignKeyBuilderAnnotations Firebird(this InternalRelationshipBuilder builder, ConfigurationSource configurationSource)
			=> new RelationalForeignKeyBuilderAnnotations(builder, configurationSource);
	}
}
