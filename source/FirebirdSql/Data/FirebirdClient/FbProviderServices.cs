#if (NET_35)

using System;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.IO;
using System.Reflection;
using System.Xml;

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
            //SQL Generation logic goes here!
            throw new NotImplementedException("SQL Generation logic not yet supplied!");
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