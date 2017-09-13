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
using Microsoft.EntityFrameworkCore.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Extensions
{
	public static class FbMetadataExtensions
	{
		public static FbPropertyAnnotations Firebird(this IProperty property)
			=> new FbPropertyAnnotations(property);

		public static RelationalEntityTypeAnnotations Firebird(this IEntityType entityType)
			=> new RelationalEntityTypeAnnotations(entityType);

		public static RelationalKeyAnnotations Firebird(this IKey key)
			=> new RelationalKeyAnnotations(key);

		public static RelationalForeignKeyAnnotations Firebird(this IForeignKey foreignKey)
			=> new RelationalForeignKeyAnnotations(foreignKey);

		public static RelationalIndexAnnotations Firebird(this IIndex index)
			=> new RelationalIndexAnnotations(index);

		public static FbModelAnnotations Firebird(this IModel model)
			=> new FbModelAnnotations(model);
	}
}
