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

//$Authors = Jiri Cincura (jiri@cincura.net)

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal
{
	public class FbModificationCommandBatchFactory : IModificationCommandBatchFactory
	{
		readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
		readonly ISqlGenerationHelper _sqlGenerationHelper;
		readonly IUpdateSqlGenerator _updateSqlGenerator;
		readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
		readonly IDbContextOptions _options;

		public FbModificationCommandBatchFactory(IRelationalCommandBuilderFactory commandBuilderFactory, ISqlGenerationHelper sqlGenerationHelper, IUpdateSqlGenerator updateSqlGenerator, IRelationalValueBufferFactoryFactory valueBufferFactoryFactory, IDbContextOptions options)
		{
			_commandBuilderFactory = commandBuilderFactory;
			_sqlGenerationHelper = sqlGenerationHelper;
			_updateSqlGenerator = updateSqlGenerator;
			_valueBufferFactoryFactory = valueBufferFactoryFactory;
			_options = options;
		}

		public ModificationCommandBatch Create()
		{
			return new SingularModificationCommandBatch(
				_commandBuilderFactory,
				_sqlGenerationHelper,
				_updateSqlGenerator,
				_valueBufferFactoryFactory);
		}
	}
}
