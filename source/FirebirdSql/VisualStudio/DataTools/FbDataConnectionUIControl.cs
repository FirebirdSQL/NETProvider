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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Data;

namespace FirebirdSql.VisualStudio.DataTools
{
    public partial class FbDataConnectionUIControl : DataConnectionUIControl
    {
        #region · Static Methods ·

        // This	is somethig	that should	be needed in .NET 2.0
        // for use with	the	DbConnectionOptions	or DbConnectionString classes.
        private static Hashtable GetSynonyms()
        {
            Hashtable synonyms = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());

            synonyms.Add("data source", "data source");
            synonyms.Add("datasource", "data source");
            synonyms.Add("server", "data source");
            synonyms.Add("host", "data source");
            synonyms.Add("port", "port number");
            synonyms.Add("port number", "port number");
            synonyms.Add("database", "initial catalog");
            synonyms.Add("initial catalog", "initial catalog");
            synonyms.Add("user id", "user id");
            synonyms.Add("userid", "user id");
            synonyms.Add("uid", "user id");
            synonyms.Add("user", "user id");
            synonyms.Add("user name", "user id");
            synonyms.Add("username", "user id");
            synonyms.Add("password", "password");
            synonyms.Add("user password", "password");
            synonyms.Add("userpassword", "password");
            synonyms.Add("dialect", "dialect");
            synonyms.Add("pooling", "pooling");
            synonyms.Add("max pool size", "max pool size");
            synonyms.Add("maxpoolsize", "max pool size");
            synonyms.Add("min pool size", "min pool size");
            synonyms.Add("minpoolsize", "min pool size");
            synonyms.Add("character set", "character set");
            synonyms.Add("charset", "character set");
            synonyms.Add("connection lifetime", "connection lifetime");
            synonyms.Add("connectionlifetime", "connection lifetime");
            synonyms.Add("timeout", "connection timeout");
            synonyms.Add("connection timeout", "connection timeout");
            synonyms.Add("connectiontimeout", "connection timeout");
            synonyms.Add("packet size", "packet size");
            synonyms.Add("packetsize", "packet size");
            synonyms.Add("role", "role name");
            synonyms.Add("role name", "role name");
            synonyms.Add("fetch size", "fetch size");
            synonyms.Add("fetchsize", "fetch size");
            synonyms.Add("server type", "server type");
            synonyms.Add("servertype", "server type");
            synonyms.Add("isolation level", "isolation level");
            synonyms.Add("isolationlevel", "isolation level");
            synonyms.Add("records affected", "records affected");
            synonyms.Add("context connection", "context connection");

            return synonyms;
        }

        #endregion

        #region · Constructors ·

        public FbDataConnectionUIControl()
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionUIControl()");
            InitializeComponent();
        }

        #endregion

        #region · Methods ·

        public override void LoadProperties()
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionUIControl::LoadProperties()");

            try
            {
                this.txtDataSource.Text = (string)ConnectionProperties["Data Source"];
                this.txtUserName.Text   = (string)ConnectionProperties["User ID"];
                this.txtDatabase.Text   = (string)ConnectionProperties["Initial Catalog"];
                this.txtPassword.Text   = (string)ConnectionProperties["Password"];
                this.txtRole.Text       = (string)ConnectionProperties["Role"];
                this.cboCharset.Text    = (string)ConnectionProperties["Character Set"];
                if (this.ConnectionProperties.Contains("Port Number"))
                {
                    this.txtPort.Text = ConnectionProperties["Port Number"].ToString();
                }

                if (this.ConnectionProperties.Contains("Dialect"))
                {
                    if (Convert.ToInt32(ConnectionProperties["Dialect"]) == 1)
                    {
                        this.cboDialect.SelectedIndex = 0;
                    }
                    else
                    {
                        this.cboDialect.SelectedIndex = 1;
                    }
                }

                if (this.ConnectionProperties.Contains("Server Type"))
                {
                    if (Convert.ToInt32(ConnectionProperties["Server Type"]) == 0)
                    {
                        this.cboServerType.SelectedIndex = 0;
                    }
                    else
                    {
                        this.cboServerType.SelectedIndex = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region · Private Methods ·

        private void SetProperty(string propertyName, object value)
        {
            this.ConnectionProperties[propertyName] = value;
        }

        #endregion

        #region · Event Handlers ·

        private void SetProperty(object sender, EventArgs e)
        {
            if (sender.Equals(this.txtDataSource))
            {
                this.SetProperty("Data Source", this.txtDataSource.Text);
            } 
            else if (sender.Equals(this.txtDatabase))
            {
                this.SetProperty("Initial Catalog", this.txtDatabase.Text);
            }
            else if (sender.Equals(this.txtUserName))
            {
                this.SetProperty("User ID", this.txtUserName.Text);
            }
            else if (sender.Equals(this.txtPassword))
            {
                this.SetProperty("Password", this.txtPassword.Text);
            }
            else if (sender.Equals(this.txtRole))
            {
                this.SetProperty("Role", this.txtRole.Text);
            }
            else if (sender.Equals(this.txtPort))
            {
                this.SetProperty("Port Number", Convert.ToInt32(this.txtPort.Text));
            }
            else if (sender.Equals(this.cboCharset))
            {
                this.SetProperty("Character Set", this.cboCharset.Text);
            }
            else if (sender.Equals(this.cboDialect))
            {
                this.SetProperty("Dialect", Convert.ToInt32(this.cboDialect.Text));
            }
            else if (sender.Equals(this.cboServerType))
            {
                this.SetProperty("Server Type", Convert.ToInt32(this.cboServerType.SelectedIndex));
            }
        }

        private void cmdGetFile_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.txtDatabase.Text = this.openFileDialog.FileName;
            }
        }

        #endregion
    }
}
