/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2006 Le Roy Arnaud
 *  Copyright (c) 2007 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Profile;
using System.Web.Util;
using System.Xml.Serialization;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Web.Providers
{
    public class FbProfileProvider : ProfileProvider
    {
        #region � Fields �

        private string appName;
        private string connectionString;
        private int commandTimeout;

        #endregion

        #region � Properties �

        public override string ApplicationName
        {
            get { return this.appName; }
            set
            {
                if (value.Length > 100)
                {
                    throw new ProviderException("The application name is too long.");
                }

                this.appName = value;
            }
        }

        #endregion

        #region � Overriden Methods �

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (name == null || name.Length < 1)
            {
                name = "FbProfileProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Fb Profile Provider");
            }

            base.Initialize(name, config);

            string temp = config["connectionStringName"];

            if (temp == null || temp.Length < 1)
            {
                throw new ProviderException("The attribute 'connectionStringName' is missing or empty.");
            }

            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            this.connectionString = ConnectionStringSettings.ConnectionString;

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                this.appName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                this.appName = config["applicationName"];
            }

            if (!(int.TryParse(config["commandTimeout"], out this.commandTimeout)))
            {
                this.commandTimeout = 30;
            }

            config.Remove("commandTimeout");
            config.Remove("connectionStringName");
            config.Remove("applicationName");

            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);

                if (!String.IsNullOrEmpty(attribUnrecognized))
                {
                    throw new ProviderException("Attributes not recognized");
                }
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sc, SettingsPropertyCollection properties)
        {
            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();

            if (properties.Count < 1)
            {
                return svc;
            }

            string username = (string)sc["UserName"];

            foreach (SettingsProperty prop in properties)
            {
                if (prop.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                {
                    if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                    {
                        prop.SerializeAs = SettingsSerializeAs.String;
                    }
                    else
                    {
                        prop.SerializeAs = SettingsSerializeAs.Xml;
                    }
                }

                svc.Add(new SettingsPropertyValue(prop));
            }

            if (!String.IsNullOrEmpty(username))
            {
                GetPropertyValuesFromDatabase(username, svc);
            }

            return svc;
        }

        public override void SetPropertyValues(SettingsContext sc, SettingsPropertyValueCollection properties)
        {
            string username = (string)sc["UserName"];

            bool userIsAuthenticated = (bool)sc["IsAuthenticated"];

            if (username == null || username.Length < 1 || properties.Count < 1)
            {
                return;
            }

            string names = String.Empty;
            string values = String.Empty;
            byte[] buf = null;

            PrepareDataForSaving(ref names, ref values, ref buf, true, properties, userIsAuthenticated);

            if (names.Length == 0)
            {
                return;
            }
            using (FbConnection conn = new FbConnection(this.connectionString))
            {
                conn.Open();

                using (FbCommand cmd = new FbCommand("Profiles_SetProperties", conn))
                {
                    cmd.CommandTimeout = this.commandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", FbDbType.VarChar, ApplicationName));
                    cmd.Parameters.Add("@PropertyNames", FbDbType.Text, names.Length).Value = names;
                    cmd.Parameters.Add("@PropertyValuesString", FbDbType.Text, values.Length).Value = values;
                    cmd.Parameters.Add("@PropertyValuesBinary", FbDbType.Binary, buf.Length).Value = buf;
                    cmd.Parameters.Add(CreateInputParam("@UserName", FbDbType.VarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@IsUserAnonymous", FbDbType.Boolean, !userIsAuthenticated));
                    cmd.Parameters.Add(CreateInputParam("@CurrentTimeUtc", FbDbType.Date, DateTime.Now));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException("profiles");
            }

            if (profiles.Count < 1)
            {
                throw new ArgumentException("The collection parameter should not be empty.");
            }

            string[] usernames = new string[profiles.Count];

            int iter = 0;
            foreach (ProfileInfo profile in profiles)
            {
                usernames[iter++] = profile.UserName;
            }

            return DeleteProfiles(usernames);
        }

        public override int DeleteProfiles(string[] usernames)
        {
            int numProfilesDeleted = 0;
            bool beginTranCalled = false;
            HttpContext context = HttpContext.Current;

            using (FbConnection conn = new FbConnection(this.connectionString))
            {

                conn.Open();
                FbTransaction trans = conn.BeginTransaction();
                try
                {
                    int numUsersRemaing = usernames.Length;
                    while (numUsersRemaing > 0)
                    {
                        using (FbCommand cmd = new FbCommand("Profiles_DeleteProfile", conn, trans))
                        {
                            cmd.CommandTimeout = this.commandTimeout;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(CreateInputParam("@ApplicationName", FbDbType.VarChar, ApplicationName));
                            cmd.Parameters.Add(CreateInputParam("@UserNames", FbDbType.VarChar, usernames[numUsersRemaing - 1]));
                            cmd.ExecuteNonQuery();
                        }

                        numUsersRemaing--;
                        numProfilesDeleted++;
                    }
                }
                catch
                {
                    beginTranCalled = true;
                }

                if (beginTranCalled)
                {
                    trans.Rollback();
                }
                else
                {
                    trans.Commit();
                }
            }

            return numProfilesDeleted;
        }

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {

            using (FbConnection conn = new FbConnection(this.connectionString))
            {
                conn.Open();

                using (FbCommand cmd = new FbCommand("Profiles_DeleteInactProfiles", conn))
                {
                    cmd.CommandTimeout = this.commandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", FbDbType.VarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@ProfileAuthOptions", FbDbType.Integer, (int)authenticationOption));
                    cmd.Parameters.Add(CreateInputParam("@InactiveSinceDate", FbDbType.Date, userInactiveSinceDate.ToUniversalTime()));

                    object result = cmd.ExecuteNonQuery();

                    if (result == null || !(result is int))
                    {
                        return 0;
                    }
                    else
                    {
                        return (int)result;
                    }
                }
            }
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            using (FbConnection conn = new FbConnection(this.connectionString))
            {
                conn.Open();

                using (FbCommand cmd = new FbCommand("Profiles_GetNbOfInactProfiles", conn))
                {
                    cmd.CommandTimeout = this.commandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", FbDbType.VarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@ProfileAuthOptions", FbDbType.Integer, (int)authenticationOption));
                    cmd.Parameters.Add(CreateInputParam("@InactiveSinceDate", FbDbType.Date, userInactiveSinceDate.ToUniversalTime()));

                    object result = cmd.ExecuteScalar();

                    if (result == null || !(result is int))
                    {
                        return 0;
                    }
                    else
                    {
                        return (int)result;
                    }
                }
            }
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            FbParameter[] args = new FbParameter[2];
            args[1] = CreateInputParam("@InactiveSinceDate", FbDbType.Date, DBNull.Value);
            args[0] = CreateInputParam("@UserNameToMatch", FbDbType.VarChar, DBNull.Value);

            return GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            FbParameter[] args = new FbParameter[2];
            args[1] = CreateInputParam("@InactiveSinceDate", FbDbType.Date, userInactiveSinceDate.ToUniversalTime());
            args[0] = CreateInputParam("@UserNameToMatch", FbDbType.VarChar, DBNull.Value);

            return GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            FbParameter[] args = new FbParameter[2];
            args[0] = CreateInputParam("@UserNameToMatch", FbDbType.VarChar, usernameToMatch.ToUpper());
            args[1] = CreateInputParam("@InactiveSinceDate", FbDbType.Date, DBNull.Value);

            return GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            FbParameter[] args = new FbParameter[2];
            args[0] = CreateInputParam("@UserNameToMatch", FbDbType.VarChar, usernameToMatch.ToUpper());
            args[1] = CreateInputParam("@InactiveSinceDate", FbDbType.Date, userInactiveSinceDate.ToUniversalTime());

            return GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        #endregion

        #region � Private Methods �

        private FbParameter CreateInputParam(string paramName, FbDbType dbType, object objValue)
        {
            FbParameter param = new FbParameter(paramName, dbType);

            if (objValue == null)
            {
                objValue = String.Empty;
            }

            param.Value = objValue;

            return param;
        }

        private void GetPropertyValuesFromDatabase(string userName, SettingsPropertyValueCollection svc)
        {
            HttpContext context = HttpContext.Current;
            string[] names = null;
            string values = null;
            byte[] buf = null;
            string sName = null;

            if (context != null)
            {
                sName = (context.Request.IsAuthenticated ? context.User.Identity.Name : context.Request.AnonymousID);
            }

            using (FbConnection conn = new FbConnection(this.connectionString))
            {
                conn.Open();

                using (FbCommand cmd = new FbCommand("Profiles_GetProperties", conn))
                {
                    cmd.CommandTimeout = this.commandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", FbDbType.VarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@UserName", FbDbType.VarChar, userName));
                    cmd.Parameters.Add(CreateInputParam("@CurrentTimeUtc", FbDbType.Date, DateTime.UtcNow));

                    using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            names = reader.GetString(0).Split(':');
                            values = reader.GetString(1);

                            int size = (int)reader.GetBytes(2, 0, null, 0, 0);

                            buf = new byte[size];
                            reader.GetBytes(2, 0, buf, 0, size);
                        }
                    }
                }

                ParseDataFromDB(names, values, buf, svc);
            }
        }

        private static void ParseDataFromDB(string[] names, string values, byte[] buf, SettingsPropertyValueCollection properties)
        {
            if (names == null || values == null || buf == null || properties == null)
            {
                return;
            }

            for (int iter = 0; iter < names.Length / 4; iter++)
            {
                string name = names[iter * 4];
                SettingsPropertyValue pp = properties[name];

                if (pp == null)
                {
                    continue;
                }

                int startPos = Int32.Parse(names[iter * 4 + 2], CultureInfo.InvariantCulture);
                int length = Int32.Parse(names[iter * 4 + 3], CultureInfo.InvariantCulture);

                if (length == -1 && !pp.Property.PropertyType.IsValueType)
                {
                    pp.PropertyValue = null;
                    pp.IsDirty = false;
                    pp.Deserialized = true;
                }
                if (names[iter * 4 + 1] == "S" && startPos >= 0 && length > 0 && values.Length >= startPos + length)
                {
                    pp.SerializedValue = values.Substring(startPos, length);
                }

                if (names[iter * 4 + 1] == "B" && startPos >= 0 && length > 0 && buf.Length >= startPos + length)
                {
                    byte[] buf2 = new byte[length];

                    Buffer.BlockCopy(buf, startPos, buf2, 0, length);
                    pp.SerializedValue = buf2;
                }
            }
        }

        private static void PrepareDataForSaving(ref string allNames, ref string allValues, ref byte[] buf, bool binarySupported, SettingsPropertyValueCollection properties, bool userIsAuthenticated)
        {
            StringBuilder names = new StringBuilder();
            StringBuilder values = new StringBuilder();

            using (MemoryStream ms = (binarySupported ? new System.IO.MemoryStream() : null))
            {
                bool anyItemsToSave = false;

                foreach (SettingsPropertyValue pp in properties)
                {
                    if (pp.IsDirty)
                    {
                        if (!userIsAuthenticated)
                        {
                            bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];

                            if (!allowAnonymous)
                            {
                                continue;
                            }
                        }

                        anyItemsToSave = true;
                        break;
                    }
                }

                if (!anyItemsToSave)
                {
                    return;
                }

                foreach (SettingsPropertyValue pp in properties)
                {
                    if (!userIsAuthenticated)
                    {
                        bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
                        if (!allowAnonymous)
                        {
                            continue;
                        }
                    }

                    if (!pp.IsDirty && pp.UsingDefaultValue)
                    {
                        continue;
                    }

                    int len = 0;
                    int startPos = 0;
                    string propValue = null;

                    if (pp.Deserialized && pp.PropertyValue == null)
                    {
                        len = -1;
                    }
                    else
                    {
                        object serializedValue = pp.SerializedValue;

                        if (serializedValue == null)
                        {
                            len = -1;
                        }
                        else
                        {
                            if (!(serializedValue is string) && !binarySupported)
                            {
                                serializedValue = Convert.ToBase64String((byte[])serializedValue);
                            }

                            if (serializedValue is string)
                            {
                                propValue = (string)serializedValue;
                                len = propValue.Length;
                                startPos = values.Length;
                            }
                            else
                            {
                                byte[] b2 = (byte[])serializedValue;
                                startPos = (int)ms.Position;
                                ms.Write(b2, 0, b2.Length);
                                ms.Position = startPos + b2.Length;
                                len = b2.Length;
                            }
                        }
                    }

                    names.Append(pp.Name + ":" + ((propValue != null) ? "S" : "B") +
                                 ":" + startPos.ToString(CultureInfo.InvariantCulture) + ":" + len.ToString(CultureInfo.InvariantCulture) + ":");

                    if (propValue != null)
                    {
                        values.Append(propValue);
                    }
                }

                if (binarySupported)
                {
                    buf = ms.ToArray();
                }
            }

            allNames = names.ToString();
            allValues = values.ToString();
        }

        private ProfileInfoCollection GetProfilesForQuery(FbParameter[] args, ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException("The pageIndex must be greater than or equal to zero.");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("The pageSize must be greater than zero.");
            }

            totalRecords = 0;

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;

            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException("The combination of pageIndex and pageSize cannot exceed the maximum value of System.Int32.");
            }

            ProfileInfoCollection profiles = new ProfileInfoCollection();

            using (FbConnection conn = new FbConnection(this.connectionString))
            {
                conn.Open();

                using (FbCommand
                    cmd1 = new FbCommand("Profiles_GetCountProfiles", conn),
                    cmd2 = new FbCommand("Profiles_GetProfiles", conn))
                {
                    cmd1.CommandTimeout = this.commandTimeout;
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.Add(CreateInputParam("@ApplicationName", FbDbType.VarChar, ApplicationName));
                    cmd1.Parameters.Add(CreateInputParam("@ProfileAuthOptions", FbDbType.Integer, (int)authenticationOption));

                    foreach (FbParameter arg in args)
                    {
                        cmd1.Parameters.Add(arg);
                    }

                    totalRecords = (int)cmd1.ExecuteScalar();

                    cmd2.CommandTimeout = this.commandTimeout;
                    cmd2.CommandType = CommandType.StoredProcedure;

                    foreach (FbParameter p in cmd1.Parameters)
                    {
                        cmd2.Parameters.Add(p);
                    }

                    cmd2.Parameters.Add(CreateInputParam("@PageIndex", FbDbType.Integer, pageIndex));
                    cmd2.Parameters.Add(CreateInputParam("@PageSize", FbDbType.Integer, pageSize));
                    using (FbDataReader reader = cmd2.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        while (reader.Read())
                        {
                            string username;
                            DateTime dtLastActivity, dtLastUpdated;
                            bool isAnon;
                            username = reader.GetString(0);
                            isAnon = reader.GetBoolean(1);
                            dtLastActivity = DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc);
                            dtLastUpdated = DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc);
                            profiles.Add(new ProfileInfo(username, isAnon, dtLastActivity, dtLastUpdated, -1));
                        }
                    }
                }
            }

            return profiles;
        }

        #endregion
    }
}