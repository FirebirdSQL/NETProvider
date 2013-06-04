using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if (!EF_6)
using System.Data.Metadata.Edm;
#else
using System.Data.Entity.Core.Metadata.Edm;
#endif

namespace FirebirdSql.Data.Entity
{
	static class SsdlToFb
	{
		public static string Transform(StoreItemCollection storeItems, string providerManifestToken)
		{
			var result = new StringBuilder();

			if (storeItems == null)
			{
				result.Append("-- No input.");
				return result.ToString();
			}

			result.Append("-- Tables");
			result.AppendLine();
			result.Append(string.Join(Environment.NewLine, Tables(storeItems)));
			result.AppendLine();
			result.Append("-- Foreign Key Constraints");
			result.AppendLine();
			result.Append(string.Join(Environment.NewLine, ForeignKeyConstraints(storeItems)));
			result.AppendLine();
			result.AppendLine();
			result.Append("-- EOF");

			return result.ToString();
		}

		static IEnumerable<string> Tables(StoreItemCollection storeItems)
		{
			foreach (var entitySet in storeItems.GetItems<EntityContainer>()[0].BaseEntitySets.OfType<EntitySet>())
			{
				var result = new StringBuilder();
				var additionalColumnComments = new Dictionary<string, string>();
				result.AppendFormat("RECREATE TABLE {0} (", Quote(TableName(entitySet)));
				result.AppendLine();
				foreach (var property in entitySet.ElementType.Properties)
				{
					var column = GenerateColumn(property);
					result.Append("\t");
					result.Append(column.Item1);
					result.AppendLine();
					foreach (var item in column.Item2)
						additionalColumnComments.Add(item.Key, item.Value);
				}
				result.AppendFormat("CONSTRAINT {0} PRIMARY KEY ({1})",
					Quote(string.Format("PK_{0}", TableName(entitySet))),
					string.Join(", ", entitySet.ElementType.KeyMembers.Select(pk => Quote(ColumnName(pk)))));
				result.Append(");");
				result.AppendLine();
				foreach (var identity in entitySet.ElementType.KeyMembers.Where(pk => pk.TypeUsage.Facets.Contains("StoreGeneratedPattern") && (StoreGeneratedPattern)pk.TypeUsage.Facets["StoreGeneratedPattern"].Value == StoreGeneratedPattern.Identity).Select(i => ColumnName(i)))
				{
					additionalColumnComments.Add(identity, "#PK_GEN#");
				}
				foreach (var comment in additionalColumnComments)
				{
					result.AppendFormat("COMMENT ON COLUMN {0}.{1} IS '{2}'",
						Quote(TableName(entitySet)),
						Quote(comment.Key),
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
				ReferentialConstraint constraint = associationSet.ElementType.ReferentialConstraints.Single<ReferentialConstraint>(); 
				AssociationSetEnd end = associationSet.AssociationSetEnds[constraint.FromRole.Name];
				AssociationSetEnd end2 = associationSet.AssociationSetEnds[constraint.ToRole.Name];
				result.AppendFormat("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2})",
					Quote(TableName(end2.EntitySet)),
					Quote(string.Format("FK_{0}", AssociationSetName(associationSet))),
					string.Join(", ", constraint.ToProperties.Select(fk => Quote(ColumnName(fk)))));
				result.AppendLine();
				result.AppendFormat("REFERENCES {0}({1})",
					Quote(TableName(end.EntitySet)),
					string.Join(", ", constraint.FromProperties.Select(pk => Quote(ColumnName(pk)))));
				result.AppendLine();
				result.AppendFormat("ON DELETE {0}",
					end.CorrespondingAssociationEndMember.DeleteBehavior == OperationAction.Cascade ? "CASCADE" : "NO ACTION");
				result.Append(";");
				yield return result.ToString();
			}
		}

		static string Quote(string s)
		{
			return "\"" + s + "\"";
		}

		static Tuple<string, IDictionary<string, string>> GenerateColumn(EdmProperty property)
		{
			var column = new StringBuilder();
			var columnComments = new Dictionary<string, string>();
			column.Append(Quote(ColumnName(property)));
			column.Append(" ");
			switch (property.TypeUsage.EdmType.Name)
			{
				case "varchar":
				case "char":
					column.Append(property.TypeUsage.EdmType.Name.ToUpperInvariant());
					column.AppendFormat("({0})", property.TypeUsage.Facets["MaxLength"].Value);
					break;
				case "decimal":
				case "numeric":
					column.Append(property.TypeUsage.EdmType.Name.ToUpperInvariant());
					column.AppendFormat("({0},{1})", property.TypeUsage.Facets["Precision"].Value, property.TypeUsage.Facets["Scale"].Value);
					break;
				case "clob":
					column.Append("BLOB SUB_TYPE TEXT");
					break;
				case "blob":
					column.Append("BLOB SUB_TYPE BINARY");
					break;
				case "smallint_bool":
					column.AppendFormat("SMALLINT CHECK ({0} IN (1,0))", Quote(ColumnName(property)));
					columnComments.Add(ColumnName(property), "#BOOL#");
					break;
				case "guid":
					column.Append("CHAR(16) CHARACTER SET OCTETS");
					columnComments.Add(ColumnName(property), "#GUID#");
					break;
				case "double":
					column.Append("DOUBLE PRECISION");
					break;
				default:
					column.Append(property.TypeUsage.EdmType.Name.ToUpperInvariant());
					break;
			}
			if (!property.Nullable)
			{
				column.Append(" NOT NULL");
			}
			return Tuple.Create<string, IDictionary<string, string>>(column.ToString(), columnComments);
		}

		static string ColumnName(EdmMember member)
		{
			return (string)member.MetadataProperties["Name"].Value;
		}

		static string TableName(EntitySet entitySet)
		{
			return (string)entitySet.MetadataProperties["Table"].Value ?? (string)entitySet.MetadataProperties["Name"].Value;
		}

		static string AssociationSetName(AssociationSet associationSet)
		{
			return (string)associationSet.MetadataProperties["Name"].Value;
		}

	}
}
