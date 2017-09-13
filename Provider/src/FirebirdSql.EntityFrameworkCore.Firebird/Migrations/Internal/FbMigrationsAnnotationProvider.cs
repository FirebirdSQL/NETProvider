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

using System.Collections.Generic;
using FirebirdSql.EntityFrameworkCore.Firebird.Extensions;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Migrations.Internal
{
	public class FbMigrationsAnnotationProvider : MigrationsAnnotationProvider
	{
		public FbMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
			: base(dependencies)
		{ }

		public override IEnumerable<IAnnotation> For(IProperty property)
		{
			if (property.Firebird().ValueGenerationStrategy != null)
			{
				yield return new Annotation(FbAnnotationNames.ValueGenerationStrategy, property.Firebird().ValueGenerationStrategy);
			}
		}
	}
}
