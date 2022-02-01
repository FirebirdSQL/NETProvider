/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using EntityFramework.Firebird.SqlGen;

namespace EntityFramework.Firebird;

internal static class SsdlToFb
{
	public static string Transform(StoreItemCollection storeItems, string providerManifestToken)
	{
		var result = new StringBuilder();

		if (storeItems != null)
		{
			result.Append(string.Join(Environment.NewLine, Tables(storeItems)));
			result.AppendLine();
			result.Append(string.Join(Environment.NewLine, ForeignKeyConstraints(storeItems)));
			result.AppendLine();
		}

		return result.ToString();
	}

	static IEnumerable<string> Tables(StoreItemCollection storeItems)
	{
		foreach (var entitySet in storeItems.GetItems<EntityContainer>()[0].BaseEntitySets.OfType<EntitySet>())
		{
			var result = new StringBuilder();
			var additionalColumnComments = new Dictionary<string, string>();
			result.AppendFormat("RECREATE TABLE {0} (", SqlGenerator.QuoteIdentifier(MetadataHelpers.GetTableName(entitySet)));
			result.AppendLine();
			foreach (var property in MetadataHelpers.GetProperties(entitySet.ElementType))
			{
				var column = GenerateColumn(property);
				result.Append("\t");
				result.Append(column.ColumnName);
				result.Append(",");
				result.AppendLine();
				foreach (var item in column.ColumnComments)
					additionalColumnComments.Add(item.Key, item.Value);
			}
			result.AppendFormat("CONSTRAINT {0} PRIMARY KEY ({1})",
				SqlGenerator.QuoteIdentifier(string.Format("PK_{0}", MetadataHelpers.GetTableName(entitySet))),
				string.Join(", ", entitySet.ElementType.KeyMembers.Select(pk => SqlGenerator.QuoteIdentifier(pk.Name))));
			result.AppendLine();
			result.Append(");");
			result.AppendLine();
			foreach (var identity in entitySet.ElementType.KeyMembers.Where(pk => MetadataHelpers.IsStoreGeneratedIdentity(pk)).Select(i => i.Name))
			{
				additionalColumnComments.Add(identity, "#PK_GEN#");
			}
			foreach (var comment in additionalColumnComments)
			{
				result.AppendFormat("COMMENT ON COLUMN {0}.{1} IS '{2}';",
					SqlGenerator.QuoteIdentifier(MetadataHelpers.GetTableName(entitySet)),
					SqlGenerator.QuoteIdentifier(comment.Key),
					comment.Value);
				result.AppendLine();
			}
			yield return result.ToString();
		}
	}

	static IEnumerable<string> ForeignKeyConstraints(StoreItemCollection storeItems)
	{
		foreach (var associationSet in storeItems.GetItems<EntityContainer>()[0].BaseEntitySets.OfType<AssociationSet>())
		{
			var result = new StringBuilder();
			var constraint = associationSet.ElementType.ReferentialConstraints.Single<ReferentialConstraint>();
			var end = associationSet.AssociationSetEnds[constraint.FromRole.Name];
			var end2 = associationSet.AssociationSetEnds[constraint.ToRole.Name];
			result.AppendFormat("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2})",
				SqlGenerator.QuoteIdentifier(MetadataHelpers.GetTableName(end2.EntitySet)),
				SqlGenerator.QuoteIdentifier(string.Format("FK_{0}", associationSet.Name)),
				string.Join(", ", constraint.ToProperties.Select(fk => SqlGenerator.QuoteIdentifier(fk.Name))));
			result.AppendLine();
			result.AppendFormat("REFERENCES {0}({1})",
				SqlGenerator.QuoteIdentifier(MetadataHelpers.GetTableName(end.EntitySet)),
				string.Join(", ", constraint.FromProperties.Select(pk => SqlGenerator.QuoteIdentifier(pk.Name))));
			result.AppendLine();
			result.AppendFormat("ON DELETE {0}",
				end.CorrespondingAssociationEndMember.DeleteBehavior == OperationAction.Cascade ? "CASCADE" : "NO ACTION");
			result.Append(";");
			yield return result.ToString();
		}
	}

	class GenerateColumnResult
	{
		public string ColumnName { get; set; }
		public IDictionary<string, string> ColumnComments { get; set; }
	}
	static GenerateColumnResult GenerateColumn(EdmProperty property)
	{
		var column = new StringBuilder();
		var columnComments = new Dictionary<string, string>();
		column.Append(SqlGenerator.QuoteIdentifier(property.Name));
		column.Append(" ");
		column.Append(SqlGenerator.GetSqlPrimitiveType(property.TypeUsage));
		switch (MetadataHelpers.GetEdmType<PrimitiveType>(property.TypeUsage).PrimitiveTypeKind)
		{
			case PrimitiveTypeKind.Boolean:
				column.AppendFormat(" CHECK ({0} IN (1,0))", SqlGenerator.QuoteIdentifier(property.Name));
				columnComments.Add(property.Name, "#BOOL#");
				break;
			case PrimitiveTypeKind.Guid:
				columnComments.Add(property.Name, "#GUID#");
				break;
		}
		if (!property.Nullable)
		{
			column.Append(" NOT NULL");
		}
		return new GenerateColumnResult() { ColumnName = column.ToString(), ColumnComments = columnComments };
	}
}
