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
 *  Copyright (c) 2007 Carlos Guzman Alvarez
 *  Copyright (c) 2008 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *  
 *  Based on the Microsoft Entity Framework Provider Sample Beta 3
 */

#if (NET_35 && ENTITY_FRAMEWORK)

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

namespace FirebirdSql.Data.FirebirdClient
{
    public class FbProviderServices : DbProviderServices
    {
        internal static readonly FbProviderServices Instance = new FbProviderServices();

        protected override DbCommandDefinition CreateDbCommandDefinition(DbConnection connection, DbCommandTree commandTree)
        {
            DbCommand prototype = CreateCommand(connection, commandTree);
            DbCommandDefinition result = this.CreateCommandDefinition(prototype);
            return result;
        }

        internal DbCommand CreateCommand(DbConnection connection, DbCommandTree commandTree)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (commandTree == null)
                throw new ArgumentNullException("commandTree");

            FbConnection fbConnection = (FbConnection)connection;
            FbCommand command = new FbCommand();

            List<DbParameter> parameters;
            command.CommandText = SqlGenerator.GenerateSql(commandTree, out parameters);
            command.CommandType = CommandType.Text;

            // Now make sure we populate the command's parameters from the CQT's parameters:
            foreach (KeyValuePair<string, TypeUsage> queryParameter in commandTree.Parameters)
            {
                DbParameter parameter = CreateParameterFromQueryParameter(queryParameter);
                command.Parameters.Add(parameter);
            }

            // Now add parameters added as part of SQL gen (note: this feature is only safe for DML SQL gen which
            // does not support user parameters, where there is no risk of name collision)
            if (null != parameters && 0 < parameters.Count)
            {
                foreach (DbParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        private DbParameter CreateParameterFromQueryParameter(KeyValuePair<string, TypeUsage> queryParameter)
        {
            // We really can't have a parameter here that isn't a scalar type...
            Debug.Assert(MetadataHelpers.IsPrimitiveType(queryParameter.Value), "Non-PrimitiveType used as query parameter type");

            DbParameter result = FirebirdClientFactory.Instance.CreateParameter();
            result.ParameterName = queryParameter.Key;
            result.Direction = ParameterDirection.Input;

            return result;
        }

        protected override DbProviderManifest GetDbProviderManifest(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (connection.GetType() != typeof(FbConnection))
            {
                throw new ArgumentException(string.Format("The connection given is not of type '{0}'.", typeof(FbConnection)));
            }

            #warning Implement ProviderManifest for FB
            return new SampleProviderManifest(connection);
        }

        protected override DbProviderManifest GetDbProviderManifest(string versionHint)
        {
            if (string.IsNullOrEmpty(versionHint))
            {
                throw new ArgumentException("Could not determine store version; a valid store connection or a version hint is required.");
            }

            #warning Implement ProviderManifest for FB
            return new SampleProviderManifest(versionHint);
        }

        internal static XmlReader GetXmlResource(string resourceName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream stream = executingAssembly.GetManifestResourceStream(resourceName);
            return XmlReader.Create(stream);
        }

        #region OLD CODE

        //#region · Methods ·

        //protected override XmlReader GetDbInformation(string informationType, DbConnection connection)
        //{
        //    if (informationType == DbProviderServices.ProviderManifest)
        //    {
        //        if (connection == null)
        //        {
        //            throw new ArgumentNullException("Expected non-null connection on call to GetDbInformation");
        //        }

        //        if (connection.GetType() != typeof(FbConnection))
        //        {
        //            throw new ArgumentException(string.Format("Wrong connection type. Expecting FnConnection, received {0}",
        //                                                      connection));
        //        }

        //        return this.GetXmlResource("FirebirdSql.Entity.ProviderManifest.xml");
        //    }
        //    else if (informationType == DbProviderServices.StoreSchemaDefinition)
        //    {
        //        return this.GetXmlResource("FirebirdSql.Entity.StoreSchemaDefinition.ssdl");
        //    }
        //    else if (informationType == DbProviderServices.StoreSchemaMapping)
        //    {
        //        return this.GetXmlResource("FirebirdSql.Entity.StoreSchemaMapping.msl");
        //    }

        //    throw new NotSupportedException(string.Format("SampleProviderServices does not support informationType of {0}",
        //                                                  informationType));
        //}

        //#endregion

        //#region · Protected Methods ·

        //protected override DbProviderManifest GetDbProviderManifest(DbConnection connection)
        //{
        //    if (connection == null)
        //    {
        //        throw new ArgumentNullException("connection");
        //    }

        //    if (connection.GetType() != typeof(FbConnection))
        //    {
        //        throw new ArgumentException(String.Format("The connection given is not of type '{0}'.", typeof(FbConnection)));
        //    }

        //    return new SampleProviderManifest(connection);
        //}

        //protected override DbProviderManifest GetDbProviderManifest(string versionHint)
        //{
        //    if (string.IsNullOrEmpty(versionHint))
        //    {
        //        throw new ArgumentException("Could not determine store version; a valid store connection or a version hint is required.");
        //    }

        //    return new SampleProviderManifest(versionHint);
        //}

        //#endregion

        //#region · Internal Methods ·

        //internal DbCommand CreateCommand(DbConnection connection, DbCommandTree commandTree)
        //{
        //    if (connection == null)
        //    {
        //        throw new ArgumentNullException("Expected non-null connection on call to GetDbInformation");
        //    }

        //    if (connection.GetType() != typeof(FbConnection))
        //    {
        //        throw new ArgumentException(string.Format("Wrong connection type. Expecting FnConnection, received {0}",
        //                                                  connection));
        //    }

        //    List<DbParameter> parameters = null;

        //    FbCommand command = new FbCommand(SqlGenerator.GenerateSql(commandTree, out parameters), connection as FbConnection);

        //    if (parameters != null && parameters.Count > 0)
        //    {
        //        command.Parameters.AddRange(parameters.ToArray());
        //    }

        //    return command;
        //}

        //#endregion

        //#region · Private Methods ·

        //private XmlReader GetXmlResource(string resourceName)
        //{
        //    Assembly executingAssembly = Assembly.GetExecutingAssembly();
        //    Stream stream = executingAssembly.GetManifestResourceStream(resourceName);
        //    return XmlReader.Create(stream);
        //}

        //#endregion

        #endregion OLD CODE
    }
}

#endif