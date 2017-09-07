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
			return new FbModificationCommandBatch(
				_commandBuilderFactory,
				_sqlGenerationHelper,
				_updateSqlGenerator,
				_valueBufferFactoryFactory);
		}
	}
}
