/*
 *  Visual Studio 2005 DDEX Provider for Firebird
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
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Collections;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataConnectionProperties : AdoDotNetConnectionProperties
    {
        #region · Fields ·

        private Hashtable synonyms;

        #endregion

        #region · Properties ·

        public override bool IsComplete
        {
            get 
            {
                string[] basicProperties = this.GetBasicProperties();

                foreach (string property in basicProperties)
                {
                    if (!(base[property] is string) ||
                        (base[property] as string).Length == 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        #endregion

        #region · Indexers ·

        public override object this[string propertyName]
        {
            get { return base[this.GetKey(propertyName)]; }
            set { base[this.GetKey(propertyName)] = value; }
        }

        #endregion

        #region · Protected Properties ·

        protected Hashtable Synonyms
        {
            get
            {
                if (this.synonyms == null)
                {
                    this.synonyms = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
                }

                return this.synonyms;
            }
        }

        #endregion

        #region · Constructors ·

        public FbDataConnectionProperties() 
            : base("FirebirdSql.Data.FirebirdClient")
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionProperties()");
            this.InitializeSynonyms();
            this.InitializeProperties();
        }

        public FbDataConnectionProperties(string connectionString)
            : base("FirebirdSql.Data.FirebirdClient", connectionString)
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionProperties(string)");
            this.InitializeSynonyms();
            this.InitializeProperties();
        }

        #endregion

        #region · Methods ·

        public override string[] GetBasicProperties()
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionProperties::GetBasicProperties()");

            return new string[] { "Data Source", "Initial Catalog", "User ID", "Password" };
        }

        public override string[] GetSynonyms(string propertyName)
        {
            ArrayList synonymList = new ArrayList();

            if (this.Synonyms.Contains(propertyName))
            {
                string propertyKey = (string)this.Synonyms[propertyName];

                IDictionaryEnumerator e = this.synonyms.GetEnumerator();

                while (e.MoveNext())
                {
                    if (e.Value.ToString() == propertyKey)
                    {
                        synonymList.Add(e.Key);
                    }
                }
            }

            return (string[])synonymList.ToArray(typeof(string));
        }

        #endregion

        #region · Protected Methods ·

        protected override void InitializeSynonyms()
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionProperties::InitializeSynonyms()");

            base.InitializeSynonyms();

            this.Synonyms.Add("data source", "data source");
            this.Synonyms.Add("datasource", "data source");
            this.Synonyms.Add("server", "data source");
            this.Synonyms.Add("host", "data source");
            this.Synonyms.Add("port", "port number");
            this.Synonyms.Add("port number", "port number");
            this.Synonyms.Add("database", "initial catalog");
            this.Synonyms.Add("initial catalog", "initial catalog");
            this.Synonyms.Add("user id", "user id");
            this.Synonyms.Add("userid", "user id");
            this.Synonyms.Add("uid", "user id");
            this.Synonyms.Add("user", "user id");
            this.Synonyms.Add("user name", "user id");
            this.Synonyms.Add("username", "user id");
            this.Synonyms.Add("password", "password");
            this.Synonyms.Add("user password", "password");
            this.Synonyms.Add("userpassword", "password");
            this.Synonyms.Add("dialect", "dialect");
            this.Synonyms.Add("pooling", "pooling");
            this.Synonyms.Add("max pool size", "max pool size");
            this.Synonyms.Add("maxpoolsize", "max pool size");
            this.Synonyms.Add("min pool size", "min pool size");
            this.Synonyms.Add("minpoolsize", "min pool size");
            this.Synonyms.Add("character set", "character set");
            this.Synonyms.Add("charset", "character set");
            this.Synonyms.Add("connection lifetime", "connection lifetime");
            this.Synonyms.Add("connectionlifetime", "connection lifetime");
            this.Synonyms.Add("timeout", "connection timeout");
            this.Synonyms.Add("connection timeout", "connection timeout");
            this.Synonyms.Add("connectiontimeout", "connection timeout");
            this.Synonyms.Add("packet size", "packet size");
            this.Synonyms.Add("packetsize", "packet size");
            this.Synonyms.Add("role", "role name");
            this.Synonyms.Add("role name", "role name");
            this.Synonyms.Add("fetch size", "fetch size");
            this.Synonyms.Add("fetchsize", "fetch size");
            this.Synonyms.Add("server type", "server type");
            this.Synonyms.Add("servertype", "server type");
            this.Synonyms.Add("isolation level", "isolation level");
            this.Synonyms.Add("isolationlevel", "isolation level");
            this.Synonyms.Add("records affected", "records affected");
            this.Synonyms.Add("context connection", "context connection");
        }

        protected override void InitializeProperties()
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionProperties::InitializeProperties()");

            base.InitializeProperties();

            this.AddProperty("data source", typeof(System.String));
            this.AddProperty("port number", typeof(System.String));
            this.AddProperty("initial catalog", typeof(System.String));
            this.AddProperty("user id", typeof(System.String));
            this.AddProperty("password", typeof(System.String));
            this.AddProperty("dialect", typeof(System.Int32));
            this.AddProperty("character set", typeof(System.String));
            this.AddProperty("pooling", typeof(System.Boolean));
            this.AddProperty("max pool size", typeof(System.Int32));
            this.AddProperty("min pool size", typeof(System.Int32));
            this.AddProperty("connection lifetime", typeof(System.Int32));
            this.AddProperty("connection timeout", typeof(System.Int32));
            this.AddProperty("packet size", typeof(System.Int32));
            this.AddProperty("role name", typeof(System.String));
            this.AddProperty("fetch size", typeof(System.Int32));
            this.AddProperty("server type", typeof(System.Int32));
            this.AddProperty("isolation level", typeof(System.Int32));
            this.AddProperty("records affected", typeof(System.Int32));
            this.AddProperty("context connection", typeof(System.Boolean));
        }

        #endregion

        #region · Private Methods ·

        private string GetKey(string propertyName)
        {
            string      propertyKey         = propertyName;
            string[]    propertySynonyms    = this.GetSynonyms(propertyName);

            // First check if there are yet a property for the requested keyword
            foreach (string synonym in propertySynonyms)
            {
                if (this.Contains(synonym))
                {
                    propertyKey = synonym;
                    break;
                }
            }

            return propertyKey;
        }

        #endregion
    }
}
