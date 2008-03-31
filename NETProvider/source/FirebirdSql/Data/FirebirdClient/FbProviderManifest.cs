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
 *  Copyright (c) 2008 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *  
 *  Based on the Microsoft Entity Framework Provider Sample Beta 3
 */

#if (NET_35 && ENTITY_FRAMEWORK)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Diagnostics;
using System.Xml;
using System.Data;
using System.Data.Metadata.Edm;

namespace FirebirdSql.Data.FirebirdClient
{
    public class FbProviderManifest : DbXmlEnabledProviderManifest
    {
        private string _serverVersion;
        private System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> _primitiveTypes;
        private System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> _functions;

        private const int varcharMaxSize = 8000;
        private const int nvarcharMaxSize = 4000;
        private const int binaryMaxSize = 8000;

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">A DbConnection instance, used to determine the store version</param>
        public FbProviderManifest(DbConnection connection)
            : base(FbProviderManifest.GetProviderManifest(connection))
        {
            FbConnection fbConnection = connection as FbConnection;
            if (fbConnection == null || string.IsNullOrEmpty(fbConnection.ConnectionString))
            {
                throw new ArgumentException("Could not determine store version; a valid store connection or a version hint is required.");
            }

            bool closeConnection = false;
            try
            {
                if (fbConnection.State != ConnectionState.Open)
                {
                    fbConnection.Open();
                    closeConnection = true;
                }
                _serverVersion = fbConnection.ServerVersion;
            }
            finally
            {
                if (closeConnection)
                {
                    fbConnection.Close();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="versionHint">A token used to infer the capabilities of the store</param>
        public FbProviderManifest(string versionHint)
            : base(FbProviderManifest.GetProviderManifest(new FbConnection()))
        {
            Debug.Assert(!string.IsNullOrEmpty(versionHint), "versionHint should not be null/empty");

#warning Token - Finish this for real
            //if (versionHint.StartsWith("", StringComparison.Ordinal))
            if (true)
            {
                _serverVersion = "";
            }
            else
            {
                throw new ArgumentException("Could not determine store version; a valid store connection or a version hint is required.");
            }
        }
        #endregion

        public override string Token
        {
            get
            {
#warning Token - Finish this for real
                return "FB";
                return _serverVersion;
            }
        }

        internal static XmlReader GetProviderManifest(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (connection.GetType() != typeof(FbConnection))
            {
                throw new ArgumentException(string.Format("The connection given is not of type '{0}'.", typeof(FbConnection)));
            }

            return FbProviderServices.GetXmlResource("FirebirdSql.Entity.ProviderManifest.xml");
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
            if (informationType == DbProviderManifest.StoreSchemaDefinition)
            {
                return GetStoreSchemaDescription();
            }

            if (informationType == DbProviderManifest.StoreSchemaMapping)
            {
                return GetStoreSchemaMapping();
            }

            throw new ProviderIncompatibleException(string.Format("The provider returned null for the informationType '{0}'.", informationType));
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
                case "bigint":
                case "smallint_bool":
                case "int":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

                case "varchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isUnicode = true; //TODO: hardcoded
                    isFixedLen = false;
                    break;
                case "varchar_max":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = true;
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
                    return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, true, DateTimeKind.Unspecified);
#warning What about TIME?
                //case "time":
                //    return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, true, DateTimeKind.Unspecified);
                case "date":
                    return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, false, DateTimeKind.Unspecified);

                case "float":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);
                case "double":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

                case "decimal":
                case "numeric":
                    {
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
                    }

                default:
                    throw new NotSupportedException(String.Format("The underlying provider does not support the type '{0}'.", storeTypeName));
            }

            Debug.Assert(newPrimitiveTypeKind == PrimitiveTypeKind.String, "at this point only string types should be present");

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
                            precision = 18;
                        }

                        byte scale;
                        if (!TypeHelpers.TryGetScale(edmType, out scale))
                        {
                            scale = 0;
                        }

                        return TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
                    }

                case PrimitiveTypeKind.String:
                    // char, nchar, varchar, nvarchar, varchar(max), nvarchar(max), ntext, text, xml
                    {
                        bool isUnicode = null == facets["Unicode"].Value || (bool)facets["Unicode"].Value;
                        bool isFixedLength = null != facets["FixedLength"].Value && (bool)facets["FixedLength"].Value;
                        Facet f = facets["MaxLength"];
                        // maxlen is true if facet value is unbounded, the value is bigger than the limited string sizes *or* the facet
                        // value is null. this is needed since functions still have maxlength facet value as null
                        bool isMaxLength = f.IsUnbounded || null == f.Value || (int)f.Value > (isUnicode ? nvarcharMaxSize : varcharMaxSize);
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
                                    tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar_max"], true, false);
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
                                    tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar_max"], false, false);
                                }
                                else
                                {
                                    tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar"], false, false, maxLength);
                                }
                            }
                        }
                        return tu;
                    }

                case PrimitiveTypeKind.DateTime:

                    Facet preserveSecondsFacet;
                    bool preserveSeconds;
                    if (edmType.Facets.TryGetValue("PreserveSeconds", true, out preserveSecondsFacet) && null != preserveSecondsFacet.Value)
                    {
                        preserveSeconds = (bool)preserveSecondsFacet.Value;
                    }
                    else
                    {
                        preserveSeconds = true;
                    }
#warning What about TIME?
                    return TypeUsage.CreateDefaultTypeUsage(preserveSeconds ? StoreTypeNameToStorePrimitiveType["datetime"] : StoreTypeNameToStorePrimitiveType["date"]);

                default:
                    throw new NotSupportedException(String.Format("There is no store type corresponding to the EDM type '{0}' of primitive type '{1}'.", edmType, primitiveType.PrimitiveTypeKind));
            }
        }

        private XmlReader GetStoreSchemaMapping()
        {
            return FbProviderServices.GetXmlResource("FirebirdSql.Entity.StoreSchemaMapping.msl");
        }

        private XmlReader GetStoreSchemaDescription()
        {
            return FbProviderServices.GetXmlResource("FirebirdSql.Entity.StoreSchemaDefinition.ssdl");
        }

        class TypeHelpers
        {
            public static bool TryGetPrecision(TypeUsage tu, out byte precision)
            {
                Facet f;

                precision = 0;
                if (tu.Facets.TryGetValue("Precision", false, out f))
                {
                    if (!f.IsUnbounded && f.Value != null)
                    {
                        precision = (byte)f.Value;
                        return true;
                    }
                }
                return false;
            }

            public static bool TryGetMaxLength(TypeUsage tu, out int maxLength)
            {
                Facet f;

                maxLength = 0;
                if (tu.Facets.TryGetValue("MaxLength", false, out f))
                {
                    if (!f.IsUnbounded && f.Value != null)
                    {
                        maxLength = (int)f.Value;
                        return true;
                    }
                }
                return false;
            }

            public static bool TryGetScale(TypeUsage tu, out byte scale)
            {
                Facet f;

                scale = 0;
                if (tu.Facets.TryGetValue("Scale", false, out f))
                {
                    if (!f.IsUnbounded && f.Value != null)
                    {
                        scale = (byte)f.Value;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
#endif