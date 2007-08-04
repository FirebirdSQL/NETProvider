#if (NET_35 && ENTITY_FRAMEWORK)

using System;
using System.Xml;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using System.Reflection;
using System.IO;

namespace FirebirdSql.Data.FirebirdClient
{
    public class FbProviderServices : DbProviderServices
    {
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
                    throw new ArgumentException(string.Format("Wrong connection type.  Expecting SampleConnection, received {0}",
                                                              connection));
                }

                return this.GetXmlResource("OrcasSampleProvider.Resources.SampleProviderServices.ProviderManifest.xml");
            }
            else if (informationType == DbProviderServices.StoreSchemaDefinition)
            {
                return this.GetXmlResource("OrcasSampleProvider.Resources.SampleProviderServices.StoreSchemaDefinition.ssdl");
            }
            else if (informationType == DbProviderServices.StoreSchemaMapping)
            {
                return this.GetXmlResource("OrcasSampleProvider.Resources.SampleProviderServices.StoreSchemaMapping.msl");
            }

            throw new NotSupportedException(string.Format("SampleProviderServices does not support informationType of {0}",
                                                          informationType));
        }

        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            return base.CreateCommandDefinition(prototype);
        }

        public override DbCommandDefinition CreateCommandDefinition(DbConnection connection, DbCommandTree commandTree)
        {
            DbCommand           prototype   = this.CreateCommand(connection, commandTree);
            DbCommandDefinition result      = this.CreateCommandDefinition(prototype);

            return result;
        }

        internal DbCommand CreateCommand(DbConnection connection, DbCommandTree commandTree)
        {
            //SQL Generation logic goes here!
            throw new NotImpelementedException("SQL Generation logic not yet supplied!");
        }

        private XmlReader GetXmlResource(string resourceName)
        {
            Assembly    executingAssembly   = Assembly.GetExecutingAssembly();
            Stream      stream              = executingAssembly.GetManifestResourceStream(resourceName);

            return XmlReader.Create(stream);
        }
    }
}

#endif