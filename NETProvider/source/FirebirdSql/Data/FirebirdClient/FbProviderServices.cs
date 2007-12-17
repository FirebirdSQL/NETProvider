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
 *  All Rights Reserved.
 *  
 *  Based on the Microsoft Entity Framework Provider Sample Beta 1
 */

#if (NET_35 && ENTITY_FRAMEWORK)

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.IO;
using System.Reflection;
using System.Xml;
using FirebirdSql.Data.Entity;

namespace FirebirdSql.Data.FirebirdClient
{
    public class FbProviderServices : DbProviderServices
    {
        #region · Methods ·

        protected override XmlReader GetDbInformation(string informationType, DbConnection connection)
        {
            if (informationType == DbProviderServices.ProviderManifest)
            {
                if (connection == null)
                {
                    throw new ArgumentNullException("Expected non-null connection on call to GetDbInformation");
                }

                if (connection.GetType() != typeof(FbConnection))
                {
                    throw new ArgumentException(string.Format("Wrong connection type. Expecting FnConnection, received {0}",
                                                              connection));
                }

                return this.GetXmlResource("FirebirdSql.Entity.ProviderManifest.xml");
            }
            else if (informationType == DbProviderServices.StoreSchemaDefinition)
            {
                return this.GetXmlResource("FirebirdSql.Entity.StoreSchemaDefinition.ssdl");
            }
            else if (informationType == DbProviderServices.StoreSchemaMapping)
            {
                return this.GetXmlResource("FirebirdSql.Entity.StoreSchemaMapping.msl");
            }

            throw new NotSupportedException(string.Format("SampleProviderServices does not support informationType of {0}",
                                                          informationType));
        }

        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            return base.CreateCommandDefinition(prototype);
        }

        #endregion

        #region · Protected Methods ·

        protected override DbCommandDefinition CreateDbCommandDefinition(DbConnection connection, DbCommandTree commandTree)
        {
            return this.CreateCommandDefinition(this.CreateCommand(connection, commandTree));
        }

        #endregion

        #region · Internal Methods ·

        internal DbCommand CreateCommand(DbConnection connection, DbCommandTree commandTree)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("Expected non-null connection on call to GetDbInformation");
            }

            if (connection.GetType() != typeof(FbConnection))
            {
                throw new ArgumentException(string.Format("Wrong connection type. Expecting FnConnection, received {0}",
                                                          connection));
            }

            List<DbParameter> parameters = null;

            FbCommand command = new FbCommand(SqlGenerator.GenerateSql(commandTree, out parameters), connection as FbConnection);

            if (parameters != null && parameters.Count > 0)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            return command;
        }

        #endregion

        #region · Private Methods ·

        private XmlReader GetXmlResource(string resourceName)
        {
            Assembly    executingAssembly   = Assembly.GetExecutingAssembly();
            Stream      stream              = executingAssembly.GetManifestResourceStream(resourceName);

            return XmlReader.Create(stream);
        }

        #endregion
    }
}

#endif