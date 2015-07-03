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
 *  Copyright (c) 2008-2014 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Diagnostics;
using System.Xml;
using System.Data;
using System.Reflection;
using System.IO;
#if (!EF_6)
using System.Data.Metadata.Edm;
#else
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
#endif

using FirebirdSql.Data.Entity;

#if (!EF_6)
namespace FirebirdSql.Data.FirebirdClient
#else
namespace FirebirdSql.Data.EntityFramework6
#endif
{
	public class FbProviderManifest : DbXmlEnabledProviderManifest
	{
		#region Private Fields

		internal const int BinaryMaxSize = Int32.MaxValue;
		internal const int AsciiVarcharMaxSize = 32765;
		internal const int UnicodeVarcharMaxSize = AsciiVarcharMaxSize / 4;
		internal const char LikeEscapeCharacter = '\\';

		private System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> _primitiveTypes = null;
		private System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> _functions = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="manifestToken">A token used to infer the capabilities of the store</param>
		public FbProviderManifest(string manifestToken)
			: base(FbProviderManifest.GetProviderManifest())
		{ }

		#endregion

		#region Properties
		#endregion

		internal static XmlReader GetProviderManifest()
		{
			return GetXmlResource(GetManifestResourceName());
		}

		/// <summary>
		/// Providers should override this to return information specific to their provider.
		///
		/// This method should never return null.
		/// </summary>
		/// <param name="informationType">The name of the information to be retrieved.</param>
		/// <returns>An XmlReader at the begining of the information requested.</returns>
		protected override XmlReader GetDbInformation(string informationType)
		{
			if (informationType == DbProviderManifest.StoreSchemaDefinition
#if (NET_45 || EF_6)
 || informationType == DbProviderManifest.StoreSchemaDefinitionVersion3
#endif
)
			{
				return GetStoreSchemaDescription(informationType);
			}
			if (informationType == DbProviderManifest.StoreSchemaMapping
#if (NET_45 || EF_6)
 || informationType == DbProviderManifest.StoreSchemaMappingVersion3
#endif
)
			{
				return GetStoreSchemaMapping(informationType);
			}
#if (NET_45 || EF_6)
			if (informationType == DbProviderManifest.ConceptualSchemaDefinition || informationType == DbProviderManifest.ConceptualSchemaDefinitionVersion3)
			{
				return null;
			}
#endif

			throw new ProviderIncompatibleException(String.Format("The provider returned null for the informationType '{0}'.", informationType));
		}

		public override System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetStoreTypes()
		{
			if (this._primitiveTypes == null)
			{
				this._primitiveTypes = base.GetStoreTypes();
			}

			return this._primitiveTypes;
		}

		public override System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetStoreFunctions()
		{
			if (this._functions == null)
			{
				this._functions = base.GetStoreFunctions();
			}

			return this._functions;
		}

		/// <summary>
		/// This method takes a type and a set of facets and returns the best mapped equivalent type
		/// in EDM.
		/// </summary>
		/// <param name="storeType">A TypeUsage encapsulating a store type and a set of facets</param>
		/// <returns>A TypeUsage encapsulating an EDM type and a set of facets</returns>
		public override TypeUsage GetEdmType(TypeUsage storeType)
		{
			if (storeType == null)
			{
				throw new ArgumentNullException("storeType");
			}

			string storeTypeName = storeType.EdmType.Name.ToLowerInvariant();
			if (!base.StoreTypeNameToEdmPrimitiveType.ContainsKey(storeTypeName))
			{
				throw new ArgumentException(String.Format("The underlying provider does not support the type '{0}'.", storeTypeName));
			}

			PrimitiveType edmPrimitiveType = base.StoreTypeNameToEdmPrimitiveType[storeTypeName];

			int maxLength = 0;
			bool isUnicode = true;
			bool isFixedLen = false;
			bool isUnbounded = true;

			PrimitiveTypeKind newPrimitiveTypeKind;

			switch (storeTypeName)
			{
				// for some types we just go with simple type usage with no facets
				case "smallint":
				case "int":
				case "bigint":
				case "smallint_bool":
				case "float":
				case "double":
				case "guid":
					return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

				case "decimal":
				case "numeric":
					byte precision;
					byte scale;
					if (TypeHelpers.TryGetPrecision(storeType, out precision) && TypeHelpers.TryGetScale(storeType, out scale))
					{
						return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, precision, scale);
					}
					else
					{
						return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType);
					}

				case "varchar":
					newPrimitiveTypeKind = PrimitiveTypeKind.String;
					isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
					isUnicode = true; //TODO: hardcoded
					isFixedLen = false;
					break;

				case "char":
					newPrimitiveTypeKind = PrimitiveTypeKind.String;
					isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
					isUnicode = true; //TODO: hardcoded
					isFixedLen = true;
					break;

				case "timestamp":
					return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, null);
				case "date":
					return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, null);
				case "time":
					return TypeUsage.CreateTimeTypeUsage(edmPrimitiveType, null);

				case "blob":
					newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
					isUnbounded = true;
					isFixedLen = false;
					break;

				case "clob":
					newPrimitiveTypeKind = PrimitiveTypeKind.String;
					isUnbounded = true;
					isUnicode = true; //TODO: hardcoded
					isFixedLen = false;
					break;

				default:
					throw new NotSupportedException(string.Format("The underlying provider does not support the type '{0}'.", storeTypeName));
			}

			Debug.Assert(newPrimitiveTypeKind == PrimitiveTypeKind.String || newPrimitiveTypeKind == PrimitiveTypeKind.Binary, "at this point only string and binary types should be present");

			switch (newPrimitiveTypeKind)
			{
				case PrimitiveTypeKind.String:
					if (!isUnbounded)
					{
						return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, isUnicode, isFixedLen, maxLength);
					}
					else
					{
						return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, isUnicode, isFixedLen);
					}
				case PrimitiveTypeKind.Binary:
					if (!isUnbounded)
					{
						return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, isFixedLen, maxLength);
					}
					else
					{
						return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, isFixedLen);
					}
				default:
					throw new NotSupportedException(String.Format("The underlying provider does not support the type '{0}'.", storeTypeName));
			}
		}

		/// <summary>
		/// This method takes a type and a set of facets and returns the best mapped equivalent type
		/// in SQL Server, taking the store version into consideration.
		/// </summary>
		/// <param name="storeType">A TypeUsage encapsulating an EDM type and a set of facets</param>
		/// <returns>A TypeUsage encapsulating a store type and a set of facets</returns>
		public override TypeUsage GetStoreType(TypeUsage edmType)
		{
			if (edmType == null)
			{
				throw new ArgumentNullException("edmType");
			}
			System.Diagnostics.Debug.Assert(edmType.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType);

			PrimitiveType primitiveType = edmType.EdmType as PrimitiveType;
			if (primitiveType == null)
			{
				throw new ArgumentException(String.Format("The underlying provider does not support the type '{0}'.", edmType));
			}

			ReadOnlyMetadataCollection<Facet> facets = edmType.Facets;

			switch (primitiveType.PrimitiveTypeKind)
			{
				case PrimitiveTypeKind.Boolean:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["smallint_bool"]);

				case PrimitiveTypeKind.Int16:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["smallint"]);

				case PrimitiveTypeKind.Int32:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["int"]);

				case PrimitiveTypeKind.Int64:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bigint"]);

				case PrimitiveTypeKind.Double:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["double"]);

				case PrimitiveTypeKind.Single:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float"]);

				case PrimitiveTypeKind.Decimal: // decimal, numeric
					{
						byte precision;
						if (!TypeHelpers.TryGetPrecision(edmType, out precision))
						{
							precision = 9;
						}

						byte scale;
						if (!TypeHelpers.TryGetScale(edmType, out scale))
						{
							scale = 0;
						}

						return TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
					}

				case PrimitiveTypeKind.Binary: // blob
					{
						bool isFixedLength = null != facets[MetadataHelpers.FixedLengthFacetName].Value && (bool)facets[MetadataHelpers.FixedLengthFacetName].Value;
						Facet f = facets[MetadataHelpers.MaxLengthFacetName];

						bool isMaxLength = f.IsUnbounded || null == f.Value || (int)f.Value > BinaryMaxSize;
						int maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;

						TypeUsage tu;
						if (isFixedLength)
						{
							tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["blob"], true, maxLength);
						}
						else
						{
							if (isMaxLength)
							{
								tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["blob"], false);
								System.Diagnostics.Debug.Assert(tu.Facets["MaxLength"].Description.IsConstant, "blob is not constant!");
							}
							else
							{
								tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["blob"], false, maxLength);
							}
						}
						return tu;
					}

				case PrimitiveTypeKind.String: // char, varchar, text blob
					{
						bool isUnicode = null == facets[MetadataHelpers.UnicodeFacetName].Value || (bool)facets[MetadataHelpers.UnicodeFacetName].Value;
						bool isFixedLength = null != facets[MetadataHelpers.FixedLengthFacetName].Value && (bool)facets[MetadataHelpers.FixedLengthFacetName].Value;
						Facet f = facets[MetadataHelpers.MaxLengthFacetName];
						// maxlen is true if facet value is unbounded, the value is bigger than the limited string sizes *or* the facet
						// value is null. this is needed since functions still have maxlength facet value as null
						bool isMaxLength = f.IsUnbounded || null == f.Value || (int)f.Value > (isUnicode ? UnicodeVarcharMaxSize : AsciiVarcharMaxSize);
						int maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;

						TypeUsage tu;

						if (isUnicode)
						{
							if (isFixedLength)
							{
								tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["char"], true, true, maxLength);
							}
							else
							{
								if (isMaxLength)
								{
									tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["clob"], true, false);
								}
								else
								{
									tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar"], true, false, maxLength);
								}
							}
						}
						else
						{
							if (isFixedLength)
							{
								tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["char"], false, true, maxLength);
							}
							else
							{
								if (isMaxLength)
								{
									tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["clob"], false, false);
								}
								else
								{
									tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar"], false, false, maxLength);
								}
							}
						}
						return tu;
					}

				case PrimitiveTypeKind.DateTime: // datetime, date
					{
						byte precision;
						bool useTimestamp;
						if (TypeHelpers.TryGetPrecision(edmType, out precision))
						{
							if (precision == 0)
								useTimestamp = false;
							else
								useTimestamp = true;
						}
						else
						{
							useTimestamp = true;
						}

						return TypeUsage.CreateDefaultTypeUsage(useTimestamp ? StoreTypeNameToStorePrimitiveType["timestamp"] : StoreTypeNameToStorePrimitiveType["date"]);
					}

				case PrimitiveTypeKind.Time:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["time"]);

				case PrimitiveTypeKind.Guid:
					return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["guid"]);

				default:
					throw new NotSupportedException(string.Format("There is no store type corresponding to the EDM type '{0}' of primitive type '{1}'.", edmType, primitiveType.PrimitiveTypeKind));
			}
		}

		private XmlReader GetStoreSchemaMapping(string mslName)
		{
			return GetXmlResource(GetStoreSchemaResourceName(mslName, "msl"));
		}

		private XmlReader GetStoreSchemaDescription(string ssdlName)
		{
			return GetXmlResource(GetStoreSchemaResourceName(ssdlName, "ssdl"));
		}

		private static XmlReader GetXmlResource(string resourceName)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Stream stream = executingAssembly.GetManifestResourceStream(resourceName);
			return XmlReader.Create(stream);
		}

		private static string GetManifestResourceName()
		{
#if (!EF_6)
			return "FirebirdSql.Data.Entity.ProviderManifest.xml";
#else
			return "FirebirdSql.Data.EntityFramework6.Resources.ProviderManifest.xml";
#endif
		}

		private static string GetStoreSchemaResourceName(string name, string type)
		{
#if (!EF_6)
			return string.Format("FirebirdSql.Data.Entity.{0}.{1}", name, type);
#else
			return string.Format("FirebirdSql.Data.EntityFramework6.Resources.{0}.{1}", name, type);
#endif
		}

		public override bool SupportsEscapingLikeArgument(out char escapeCharacter)
		{
			escapeCharacter = LikeEscapeCharacter;
			return true;
		}

		public override string EscapeLikeArgument(string argument)
		{
			StringBuilder sb = new StringBuilder(argument);
			sb.Replace(LikeEscapeCharacter.ToString(), LikeEscapeCharacter.ToString() + LikeEscapeCharacter.ToString());
			sb.Replace("%", LikeEscapeCharacter + "%");
			sb.Replace("_", LikeEscapeCharacter + "_");
			return sb.ToString();
		}

#if (EF_6)
		public override bool SupportsInExpression()
		{
			return true;
		}
#endif
	}
}