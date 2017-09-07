using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal
{
	public class FbModificationCommandBatch : AffectedCountModificationCommandBatch
	{
		public FbModificationCommandBatch(IRelationalCommandBuilderFactory commandBuilderFactory, ISqlGenerationHelper sqlGenerationHelper, IUpdateSqlGenerator updateSqlGenerator, IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
			: base(commandBuilderFactory, sqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
		{ }

		protected override bool CanAddCommand(ModificationCommand modificationCommand)
		{
#warning Finish
			// use EXECUTE BLOCK?
			return false;
		}

		protected override bool IsCommandTextValid() => true;
	}
}
