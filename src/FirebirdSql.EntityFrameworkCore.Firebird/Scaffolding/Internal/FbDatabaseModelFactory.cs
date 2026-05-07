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
using System.Data;
using System.Data.Common;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using FirebirdSql.EntityFrameworkCore.Firebird.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Scaffolding.Internal;

public class FbDatabaseModelFactory : DatabaseModelFactory
{
	public int MajorVersionNumber { get; private set; }

	public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
	{
		using (var connection = new FbConnection(connectionString))
		{
			return Create(connection, options);
		};
	}

	public override DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
	{
		var databaseModel = new DatabaseModel();

		var connectionStartedOpen = connection.State == ConnectionState.Open;
		if (!connectionStartedOpen)
		{
			connection.Open();
		}
		try
		{
			var serverVersion = FbServerProperties.ParseServerVersion(connection.ServerVersion);
			MajorVersionNumber = serverVersion.Major;

			databaseModel.DatabaseName = connection.Database;
			databaseModel.DefaultSchema = GetDefaultSchema();

			var tableList = options.Tables.ToList();
			var tableFilter = GenerateTableFilter(tableList);

			var tables = GetTables(connection);
			foreach (var table in tables)
			{
				table.Database = databaseModel;
				if (tableFilter.Invoke(table))
				{
					databaseModel.Tables.Add(table);
				}
			}

			return databaseModel;
		}
		finally
		{
			if (!connectionStartedOpen)
			{
				connection.Close();
			}
		}
	}

	private static string GetDefaultSchema() => null;

	private static Func<DatabaseTable, bool> GenerateTableFilter(IReadOnlyList<string> tables) =>
		tables.Any() ? x => tables.Contains(x.Name) : _ => true;

	private const string GetTablesQuery = """
		SELECT
			TRIM(r.rdb$relation_name) relation_name,
			r.rdb$description description,
			r.rdb$relation_type relation_type
		FROM
			rdb$relations r
		WHERE
			r.rdb$system_flag IS DISTINCT FROM 1
		ORDER BY
			r.rdb$relation_name
		""";

	private List<DatabaseTable> GetTables(DbConnection connection)
	{
		using (var command = connection.CreateCommand())
		{
			var tables = new List<DatabaseTable>();
			command.CommandText = GetTablesQuery;

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var name = reader.GetString(0);
					var comment = reader.GetString(1);
					var type = reader.GetInt32(2);

					var table = type == 1
						? new DatabaseView()
						: new DatabaseTable();

					table.Schema = null;
					table.Name = name;
					table.Comment = string.IsNullOrEmpty(comment) ? null : comment;

					tables.Add(table);
				}
			}

			GetColumns(connection, tables);
			GetPrimaryKeys(connection, tables);
			GetIndexes(connection, tables);
			GetConstraints(connection, tables);

			return tables;
		}
	}

	private string GetColumnsQuery() => $"""
		SELECT
			TRIM(rf.rdb$field_name) column_name,
			COALESCE(rf.rdb$null_flag, f.rdb$null_flag, 0) column_required,

			CASE COALESCE(f.rdb$field_type, 0)
				WHEN 7 THEN
					CASE f.rdb$field_sub_type
						WHEN 0 THEN 'SMALLINT'
						WHEN 1 THEN 'NUMERIC(' || (f.rdb$field_precision) || ',' || ABS(f.rdb$field_scale) || ')'
						WHEN 2 THEN 'DECIMAL(' || (f.rdb$field_precision) || ',' || ABS(f.rdb$field_scale) || ')'
						ELSE '?'
					END
				WHEN 8 THEN
					CASE f.rdb$field_sub_type
						WHEN 0 THEN 'INTEGER'
						WHEN 1 THEN 'NUMERIC(' || (f.rdb$field_precision) || ',' || ABS(f.rdb$field_scale) || ')'
						WHEN 2 THEN 'DECIMAL(' || (f.rdb$field_precision) || ',' || ABS(f.rdb$field_scale) || ')'
						ELSE '?'
					END
				WHEN 9 THEN 'QUAD'
				WHEN 10 THEN 'FLOAT'
				WHEN 12 THEN 'DATE'
				WHEN 13 THEN 'TIME'
				WHEN 14 THEN 'CHAR(' || (TRUNC(f.rdb$field_length / ch.rdb$bytes_per_character)) || ')'
				WHEN 16 THEN
					CASE f.rdb$field_sub_type
						WHEN 0 THEN 'BIGINT'
						WHEN 1 THEN 'NUMERIC(' || (f.rdb$field_precision) || ',' || ABS(f.rdb$field_scale) || ')'
						WHEN 2 THEN 'DECIMAL(' || (f.rdb$field_precision) || ',' || ABS(f.rdb$field_scale) || ')'
						ELSE '?'
					END
				WHEN 23 THEN 'BOOLEAN'
				WHEN 24 THEN 'DECFLOAT(' || (f.rdb$field_precision) || ')'
				WHEN 25 THEN 'DECFLOAT(' || (f.rdb$field_precision) || ')'
				WHEN 26 THEN 'INT128'
				WHEN 27 THEN 'DOUBLE PRECISION'
				WHEN 28 THEN 'TIME WITH TIME ZONE'
				WHEN 29 THEN 'TIMESTAMP WITH TIME ZONE'
				WHEN 35 THEN 'TIMESTAMP'
				WHEN 37 THEN 'VARCHAR(' || (TRUNC(f.rdb$field_length / ch.rdb$bytes_per_character)) || ')'
				WHEN 40 THEN 'CSTRING(' || (TRUNC(f.rdb$field_length / ch.rdb$bytes_per_character)) || ')'
				WHEN 45 THEN 'BLOB_ID'
				WHEN 261 THEN 'BLOB SUB_TYPE ' ||
					CASE f.rdb$field_sub_type
						WHEN 0 THEN 'BINARY'
						WHEN 1 THEN 'TEXT'
						ELSE f.rdb$field_sub_type
					END
				ELSE 'RDB$FIELD_TYPE: ' || f.rdb$field_type || '?'
			END column_store_type,

			rf.rdb$field_source column_domain,

			NULLIF(ch.rdb$character_set_name, d.rdb$character_set_name) character_set_name,
			co.rdb$collation_name collation_name,
			COALESCE(f.rdb$segment_length, 0) segment_length,

			COALESCE(rf.rdb$default_source, f.rdb$default_source) column_default,
			f.rdb$computed_source column_computed_source,
			f.rdb$description column_comment,

			COALESCE({(MajorVersionNumber < 3 ? "NULL" : "rf.rdb$identity_type")}, -1) identity_type,
			COALESCE({(MajorVersionNumber < 3 ? "NULL" : "g.rdb$initial_value")}, 1) identity_start,
			COALESCE({(MajorVersionNumber < 3 ? "NULL" : "g.rdb$generator_increment")}, 1) identity_increment
		FROM
			rdb$relation_fields rf
			JOIN rdb$fields f ON f.rdb$field_name = rf.rdb$field_source 
			LEFT JOIN rdb$character_sets ch ON ch.rdb$character_set_id = f.rdb$character_set_id
			LEFT JOIN rdb$collations co ON co.rdb$character_set_id = f.rdb$character_set_id AND co.rdb$collation_id = rf.rdb$collation_id
			{(MajorVersionNumber < 3 ? "" : "LEFT JOIN rdb$generators g ON g.rdb$generator_name = rf.rdb$generator_name")}
			CROSS JOIN rdb$database d
		WHERE
			TRIM(rf.rdb$relation_name) = @RelationName AND COALESCE(rf.rdb$system_flag, 0) = 0
		ORDER BY
			rf.rdb$field_position
		""";

	private void GetColumns(DbConnection connection, IReadOnlyList<DatabaseTable> tables)
	{
		foreach (var table in tables)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = GetColumnsQuery();
				command.Parameters.Add(new FbParameter("@RelationName", table.Name));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var name = reader["COLUMN_NAME"].ToString();
						var isNullable = !Convert.ToBoolean(reader["COLUMN_REQUIRED"]);
						var storeType = reader["COLUMN_STORE_TYPE"].ToString();

						var columnDomain = reader["COLUMN_DOMAIN"].ToString();

						var charset = reader["CHARACTER_SET_NAME"].ToString();
						var collation = reader["COLLATION_NAME"].ToString();
						var segmentSize = Convert.ToInt32(reader["SEGMENT_LENGTH"]);

						var defaultValue = reader["COLUMN_DEFAULT"].ToString();
						var computedSource = reader["COLUMN_COMPUTED_SOURCE"].ToString();
						var comment = reader["COLUMN_COMMENT"].ToString();

						var identityType = Convert.ToInt32(reader["IDENTITY_TYPE"]);
						var identityStart = Convert.ToInt32(reader["IDENTITY_START"]);
						var identityIncrement = Convert.ToInt32(reader["IDENTITY_INCREMENT"]);

						var column = new DatabaseColumn
						{
							Table = table,
							Name = name,
							StoreType = storeType,
							IsNullable = isNullable,

							DefaultValueSql = string.IsNullOrEmpty(defaultValue) ? null : defaultValue.Remove(0, 8),
							ValueGenerated = identityType == -1 ? ValueGenerated.Never : ValueGenerated.OnAdd,
							Comment = string.IsNullOrEmpty(comment) ? null : comment,
							Collation = string.IsNullOrEmpty(collation) ? null : collation.Trim(),
							ComputedColumnSql = string.IsNullOrEmpty(computedSource) ? null : computedSource
						};

						if (segmentSize > 0)
						{
							column.SetAnnotation(FbAnnotationNames.BlobSegmentSize, segmentSize);
						}

						if (!string.IsNullOrEmpty(charset))
						{
							column.SetAnnotation(FbAnnotationNames.CharacterSet, charset.Trim());
						}

						if (!columnDomain.StartsWith("RDB$"))
						{
							column.SetAnnotation(FbAnnotationNames.DomainName, columnDomain.Trim());
						}

						if (identityType != -1)
						{
							column.SetAnnotation(FbAnnotationNames.IdentityType, identityType);
						}

						if (identityStart != 1)
						{
							column.SetAnnotation(FbAnnotationNames.IdentityStart, identityStart);
						}

						if (identityIncrement != 1)
						{
							column.SetAnnotation(FbAnnotationNames.IdentityIncrement, identityIncrement);
						}

						table.Columns.Add(column);
					}
				}
			}
		}
	}

	private const string GetPrimaryKeysQuery = """
		SELECT
			TRIM(i.rdb$index_name) index_name,
			TRIM(sg.rdb$field_name) field_name
		FROM
			rdb$indices i
			LEFT JOIN rdb$index_segments sg ON i.rdb$index_name = sg.rdb$index_name
			LEFT JOIN rdb$relation_constraints rc ON rc.rdb$index_name = i.rdb$index_name
		WHERE
			rc.rdb$constraint_type = 'PRIMARY KEY' AND TRIM(i.rdb$relation_name) = @RelationName
		ORDER BY
			sg.rdb$field_position
		""";

	private static void GetPrimaryKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables)
	{
		foreach (var table in tables)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = GetPrimaryKeysQuery;
				command.Parameters.Add(new FbParameter("@RelationName", table.Name));

				using (var reader = command.ExecuteReader())
				{
					DatabasePrimaryKey index = null;
					while (reader.Read())
					{
						index ??= new DatabasePrimaryKey
						{
							Table = table,
							Name = reader.GetString(0).Trim()
						};
						index.Columns.Add(table.Columns.Single(y => y.Name == reader.GetString(1).Trim()));
						table.PrimaryKey = index;
					}
				}
			}
		}
	}

	private const string GetIndexesQuery = """
		SELECT
			TRIM(i.rdb$index_name) index_name,
			COALESCE(i.rdb$unique_flag, 0) is_unique,
			Coalesce(i.rdb$index_type, 0) is_desc,
			LIST(TRIM(sg.rdb$field_name)) columns
		FROM
			RDB$INDICES i
			LEFT JOIN rdb$index_segments sg ON i.rdb$index_name = sg.rdb$index_name
			LEFT JOIN rdb$relation_constraints rc ON rc.rdb$index_name = i.rdb$index_name
		WHERE
			TRIM(i.rdb$relation_name) = @RelationName
			AND i.RDB$EXPRESSION_SOURCE IS NULL
		GROUP BY
			index_name, is_unique, is_desc
		""";

	/// <remarks>
	/// Primary keys are handled as in <see cref="GetConstraints"/>, not here
	/// </remarks>
	private static void GetIndexes(DbConnection connection, IReadOnlyList<DatabaseTable> tables)
	{
		foreach (var table in tables)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = GetIndexesQuery;
				command.Parameters.Add(new FbParameter("@RelationName", table.Name));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var columns = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
						if (string.IsNullOrEmpty(columns))
						{
							continue;
						}

						var index = new DatabaseIndex
						{
							Table = table,
							Name = reader.GetString(0).Trim(),
							IsUnique = reader.GetBoolean(1),
						};

						foreach (var column in columns.Split(','))
						{
							index.Columns.Add(table.Columns.Single(y => y.Name == column.Trim()));
						}

						if (reader.GetBoolean(2))
						{
							var isDescending = new bool[index.Columns.Count];
							isDescending.AsSpan().Fill(true);
							index.IsDescending = isDescending;
						}

						table.Indexes.Add(index);
					}
				}
			}
		}
	}

	private const string GetConstraintsQuery = """
		SELECT
			TRIM(drs.rdb$constraint_name) constraint_name,
			TRIM(drs.rdb$relation_name) table_name,
			TRIM(mrc.rdb$relation_name) referenced_table_name,
			(
				SELECT
					LIST(TRIM(di.rdb$field_name) || '|' || TRIM(mi.rdb$field_name))
				FROM
					rdb$index_segments di
					join rdb$index_segments mi ON mi.rdb$field_position = di.rdb$field_position AND mi.rdb$index_name = mrc.rdb$index_name
				WHERE
					di.rdb$index_name = drs.rdb$index_name
			) paired_columns,
			TRIM(rc.rdb$delete_rule) delete_rule
		FROM
			rdb$relation_constraints drs
			LEFT JOIN rdb$ref_constraints rc ON drs.rdb$constraint_name = rc.rdb$constraint_name
			LEFT JOIN rdb$relation_constraints mrc ON rc.rdb$const_name_uq = mrc.rdb$constraint_name
		WHERE
			drs.rdb$constraint_type = 'FOREIGN KEY' AND TRIM(drs.rdb$relation_name) = @RelationName
		""";

	private static void GetConstraints(DbConnection connection, IReadOnlyList<DatabaseTable> tables)
	{
		foreach (var table in tables)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = GetConstraintsQuery;
				command.Parameters.Add(new FbParameter("@RelationName", table.Name));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var referencedTableName = reader.GetString(2);
						var referencedTable = tables.First(t => t.Name == referencedTableName);
						var fkInfo = new DatabaseForeignKey { Name = reader.GetString(0), OnDelete = ConvertToReferentialAction(reader.GetString(4)), Table = table, PrincipalTable = referencedTable };
						foreach (var pair in reader.GetString(3).Split(','))
						{
							fkInfo.Columns.Add(table.Columns.Single(y =>
								string.Equals(y.Name, pair.Split('|')[0], StringComparison.OrdinalIgnoreCase)));
							fkInfo.PrincipalColumns.Add(fkInfo.PrincipalTable.Columns.Single(y =>
								string.Equals(y.Name, pair.Split('|')[1], StringComparison.OrdinalIgnoreCase)));
						}

						table.ForeignKeys.Add(fkInfo);
					}
				}
			}
		}
	}

	private static ReferentialAction? ConvertToReferentialAction(string onDeleteAction) =>
		onDeleteAction.ToUpperInvariant() switch
		{
			"RESTRICT" => ReferentialAction.Restrict,
			"CASCADE" => ReferentialAction.Cascade,
			"SET NULL" => ReferentialAction.SetNull,
			"NO ACTION" => ReferentialAction.NoAction,
			_ => null,
		};
}
