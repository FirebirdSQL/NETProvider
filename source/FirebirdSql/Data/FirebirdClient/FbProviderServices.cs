/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2008-2010 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

#if (!(NET_35 && !ENTITY_FRAMEWORK))

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using FirebirdSql.Data.Entity;
using FirebirdSql.Data.Isql;
using FirebirdSql.Data.Services;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public class FbProviderServices : DbProviderServices
	{
		internal static readonly FbProviderServices Instance = new FbProviderServices();

		protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest manifest, DbCommandTree commandTree)
		{
			DbCommand prototype = CreateCommand(manifest, commandTree);
			DbCommandDefinition result = this.CreateCommandDefinition(prototype);
			return result;
		}

		protected FbConnection CheckAndCastToFbConnection(DbConnection connection)
		{
			if (!(connection is FbConnection))
			{
				throw new ArgumentException("The connection is not of type 'FbConnection'.");
			}
			return (FbConnection)connection;
		}

		/// <summary>
		/// Create a SampleCommand object, given the provider manifest and command tree
		/// </summary>
		private DbCommand CreateCommand(DbProviderManifest manifest, DbCommandTree commandTree)
		{
			if (manifest == null)
				throw new ArgumentNullException("manifest");

			if (commandTree == null)
				throw new ArgumentNullException("commandTree");

			FbCommand command = new FbCommand();

			#region Type coercions
			// Set expected column types for DbQueryCommandTree
			DbQueryCommandTree queryTree = commandTree as DbQueryCommandTree;
			if (queryTree != null)
			{
				DbProjectExpression projectExpression = queryTree.Query as DbProjectExpression;
				if (projectExpression != null)
				{
					EdmType resultsType = projectExpression.Projection.ResultType.EdmType;

					StructuralType resultsAsStructuralType = resultsType as StructuralType;
					if (resultsAsStructuralType != null)
					{
						command.ExpectedColumnTypes = new PrimitiveType[resultsAsStructuralType.Members.Count];

						for (int ordinal = 0; ordinal < resultsAsStructuralType.Members.Count; ordinal++)
						{
							EdmMember member = resultsAsStructuralType.Members[ordinal];
							PrimitiveType primitiveType = member.TypeUsage.EdmType as PrimitiveType;
							command.ExpectedColumnTypes[ordinal] = primitiveType;
						}
					}
				}
			}

			// Set expected column types for DbFunctionCommandTree
			DbFunctionCommandTree functionTree = commandTree as DbFunctionCommandTree;
			if (functionTree != null)
			{
				if (functionTree.ResultType != null)
				{
					Debug.Assert(MetadataHelpers.IsCollectionType(functionTree.ResultType.EdmType), "Result type of a function is expected to be a collection of RowType or PrimitiveType");

					EdmType elementType = MetadataHelpers.GetElementTypeUsage(functionTree.ResultType).EdmType;

					if (MetadataHelpers.IsRowType(elementType))
					{
						ReadOnlyMetadataCollection<EdmMember> members = ((RowType)elementType).Members;
						command.ExpectedColumnTypes = new PrimitiveType[members.Count];

						for (int ordinal = 0; ordinal < members.Count; ordinal++)
						{
							EdmMember member = members[ordinal];
							PrimitiveType primitiveType = (PrimitiveType)member.TypeUsage.EdmType;
							command.ExpectedColumnTypes[ordinal] = primitiveType;
						}

					}
					else if (MetadataHelpers.IsPrimitiveType(elementType))
					{
						command.ExpectedColumnTypes = new PrimitiveType[1];
						command.ExpectedColumnTypes[0] = (PrimitiveType)elementType;
					}
					else
					{
						Debug.Fail("Result type of a function is expected to be a collection of RowType or PrimitiveType");
					}
				}
			}
			#endregion

			List<DbParameter> parameters;
			CommandType commandType;

			command.CommandText = SqlGenerator.GenerateSql(commandTree, out parameters, out commandType);
			command.CommandType = commandType;

			// Get the function (if any) implemented by the command tree since this influences our interpretation of parameters
			EdmFunction function = null;
			if (commandTree is DbFunctionCommandTree)
			{
				function = ((DbFunctionCommandTree)commandTree).EdmFunction;
			}

			// Now make sure we populate the command's parameters from the CQT's parameters:
			foreach (KeyValuePair<string, TypeUsage> queryParameter in commandTree.Parameters)
			{
				FbParameter parameter;

				// Use the corresponding function parameter TypeUsage where available (currently, the SSDL facets and 
				// type trump user-defined facets and type in the EntityCommand).
				FunctionParameter functionParameter;
				if (null != function && function.Parameters.TryGetValue(queryParameter.Key, false, out functionParameter))
				{
					parameter = CreateSqlParameter(functionParameter.Name, functionParameter.TypeUsage, functionParameter.Mode, DBNull.Value);
				}
				else
				{
					parameter = CreateSqlParameter(queryParameter.Key, queryParameter.Value, ParameterMode.In, DBNull.Value);
				}

				command.Parameters.Add(parameter);
			}

			// Now add parameters added as part of SQL gen (note: this feature is only safe for DML SQL gen which
			// does not support user parameters, where there is no risk of name collision)
			if (null != parameters && 0 < parameters.Count)
			{
				if (!(commandTree is DbInsertCommandTree) &&
					!(commandTree is DbUpdateCommandTree) &&
					!(commandTree is DbDeleteCommandTree))
				{
					throw new InvalidOperationException("SqlGenParametersNotPermitted");
				}

				foreach (DbParameter parameter in parameters)
				{
					command.Parameters.Add(parameter);
				}
			}

			return command;
		}

		protected override string GetDbProviderManifestToken(DbConnection connection)
		{
			if (connection == null)
				throw new ArgumentException("connection");

			FbConnection fbConnection = CheckAndCastToFbConnection(connection);

			if (string.IsNullOrEmpty(fbConnection.ConnectionString))
			{
				throw new ArgumentException("Could not determine storage version; a valid storage connection is required.");
			}

			try
			{
				FbServerProperties serverProperties = new FbServerProperties() { ConnectionString = fbConnection.ConnectionString };
				Version serverVersion = serverProperties.GetServerVersion().ParseServerVersion();
				return serverVersion.ToString(2);
			}
			catch (FbException ex)
			{
				throw new InvalidOperationException("Could not retrieve storage version.", ex);
			}
		}

		protected override DbProviderManifest GetDbProviderManifest(string versionHint)
		{
			if (string.IsNullOrEmpty(versionHint))
			{
				throw new ArgumentException("Could not determine store version; a valid store connection or a version hint is required.");
			}

			return new FbProviderManifest(versionHint);
		}

		/// <summary>
		/// Creates a SqlParameter given a name, type, and direction
		/// </summary>
		internal static FbParameter CreateSqlParameter(string name, TypeUsage type, ParameterMode mode, object value)
		{
			int? size;

			FbParameter result = new FbParameter(name, value);

			// .Direction
			ParameterDirection direction = MetadataHelpers.ParameterModeToParameterDirection(mode);
			if (result.Direction != direction)
			{
				result.Direction = direction;
			}

			// .Size and .SqlDbType
			// output parameters are handled differently (we need to ensure there is space for return
			// values where the user has not given a specific Size/MaxLength)
			bool isOutParam = mode != ParameterMode.In;
			FbDbType sqlDbType = GetSqlDbType(type, isOutParam, out size);

			if (result.FbDbType != sqlDbType)
			{
				result.FbDbType = sqlDbType;
			}

			// Note that we overwrite 'facet' parameters where either the value is different or
			// there is an output parameter.
			if (size.HasValue && (isOutParam || result.Size != size.Value))
			{
				result.Size = size.Value;
			}

			// .IsNullable
			bool isNullable = MetadataHelpers.IsNullable(type);
			if (isOutParam || isNullable != result.IsNullable)
			{
				result.IsNullable = isNullable;
			}

			return result;
		}

		/// <summary>
		/// Determines SqlDbType for the given primitive type. Extracts facet
		/// information as well.
		/// </summary>
		private static FbDbType GetSqlDbType(TypeUsage type, bool isOutParam, out int? size)
		{
			// only supported for primitive type
			PrimitiveTypeKind primitiveTypeKind = MetadataHelpers.GetPrimitiveTypeKind(type);

			size = default(int?);

			switch (primitiveTypeKind)
			{
				case PrimitiveTypeKind.Boolean:
					return FbDbType.SmallInt;

				case PrimitiveTypeKind.Int16:
					return FbDbType.SmallInt;

				case PrimitiveTypeKind.Int32:
					return FbDbType.Integer;

				case PrimitiveTypeKind.Int64:
					return FbDbType.BigInt;

				case PrimitiveTypeKind.Double:
					return FbDbType.Double;

				case PrimitiveTypeKind.Single:
					return FbDbType.Float;

				case PrimitiveTypeKind.Decimal:
					return FbDbType.Decimal;

				case PrimitiveTypeKind.Binary:
					// for output parameters, ensure there is space...
					size = GetParameterSize(type, isOutParam);
					return GetBinaryDbType(type);

				case PrimitiveTypeKind.String:
					size = GetParameterSize(type, isOutParam);
					return GetStringDbType(type);

				case PrimitiveTypeKind.DateTime:
					return FbDbType.TimeStamp;

				case PrimitiveTypeKind.Time:
					return FbDbType.Time;

				case PrimitiveTypeKind.Guid:
					return FbDbType.Guid;

				default:
					Debug.Fail("unknown PrimitiveTypeKind " + primitiveTypeKind);
					throw new InvalidOperationException("unknown PrimitiveTypeKind " + primitiveTypeKind);
			}
		}

		/// <summary>
		/// Determines preferred value for SqlParameter.Size. Returns null
		/// where there is no preference.
		/// </summary>
		private static int? GetParameterSize(TypeUsage type, bool isOutParam)
		{
			int? maxLength;
			if (MetadataHelpers.TryGetMaxLength(type, out maxLength))
			{
				// if the MaxLength facet has a specific value use it
				return maxLength;
			}
			else if (isOutParam)
			{
				// if the parameter is a return/out/inout parameter, ensure there 
				// is space for any value
				return int.MaxValue;
			}
			else
			{
				// no value
				return default(int?);
			}
		}

		/// <summary>
		/// Chooses the appropriate FbDbType for the given string type.
		/// </summary>
		private static FbDbType GetStringDbType(TypeUsage type)
		{
			Debug.Assert(type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
				PrimitiveTypeKind.String == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for string type");

			FbDbType dbType;
			// Specific type depends on whether the string is a unicode string and whether it is a fixed length string.
			// By default, assume widest type (unicode) and most common type (variable length)
			bool unicode;
			bool fixedLength;
			if (!MetadataHelpers.TryGetIsFixedLength(type, out fixedLength))
			{
				fixedLength = false;
			}

			if (!MetadataHelpers.TryGetIsUnicode(type, out unicode))
			{
				unicode = true;
			}

			if (fixedLength)
			{
				dbType = (unicode ? FbDbType.Char : FbDbType.Char);
			}
			else
			{
				int? maxLength;
				if (!MetadataHelpers.TryGetMaxLength(type, out maxLength))
				{
					maxLength = (unicode ? FbProviderManifest.NVarcharMaxSize : FbProviderManifest.VarcharMaxSize);
				}
				if (maxLength == default(int?) || maxLength > (unicode ? FbProviderManifest.NVarcharMaxSize : FbProviderManifest.VarcharMaxSize))
				{
					dbType = FbDbType.Text;
				}
				else
				{
					dbType = (unicode ? FbDbType.VarChar : FbDbType.VarChar);
				}
			}

			return dbType;
		}

		/// <summary>
		/// Chooses the appropriate FbDbType for the given binary type.
		/// </summary>
		private static FbDbType GetBinaryDbType(TypeUsage type)
		{
			Debug.Assert(type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
				PrimitiveTypeKind.Binary == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for binary type");

			// Specific type depends on whether the binary value is fixed length. By default, assume variable length.
			//bool fixedLength;
			//if (!MetadataHelpers.TryGetIsFixedLength(type, out fixedLength))
			//{
			//    fixedLength = false;
			//}

			return FbDbType.Binary;
		}

#if (!NET_35)
		protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
		{
			FbConnection fbConnection = CheckAndCastToFbConnection(connection);
			string script = DbCreateDatabaseScript(GetDbProviderManifestToken(fbConnection), storeItemCollection);
			FbScript fbScript = new FbScript(script);
			fbScript.Parse();
			FbConnection.CreateDatabase(fbConnection.ConnectionString);
			new FbBatchExecution(fbConnection, fbScript).Execute();	
		}

		protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
		{
			return new SSDLToFB() { StoreItemCollection = storeItemCollection }.TransformText();
		}

		protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
		{
			throw new NotSupportedException("Firebird doesn't allow to check database existence.");
		}

		protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
		{
			FbConnection fbConnection = CheckAndCastToFbConnection(connection);
			FbConnection.DropDatabase(connection.ConnectionString);
		}
#endif
	}
}
#endif
