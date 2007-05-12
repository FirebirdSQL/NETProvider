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
 *  Copyright (c) 2007 Jiri Cincura
 *	All Rights Reserved.
 */

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Text;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Web.Providers
{
    /// <summary>
    /// References:
    ///		http://msdn2.microsoft.com/en-us/library/tksy7hd7(VS.80).aspx
    ///		http://msdn2.microsoft.com/en-us/library/317sza4k.aspx
    /// </summary>
    public sealed class FbRoleProvider : RoleProvider
    {
        #region � Fields �
        //
        // Global connection string, generic exception message, event log info.
        //
        private string eventSource = "FbRoleProvider";
        private string eventLog = "Application";
        private ConnectionStringSettings connectionStringSettings;
        private string connectionString;
        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //
        private bool writeExceptionsToEventLog = false;
        private string applicationName;
        #endregion

        #region � Properties �

        public bool WriteExceptionsToEventLog
        {
            get { return writeExceptionsToEventLog; }
            set { writeExceptionsToEventLog = value; }
        }

        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        #endregion

        #region � Overriden Methods �

        public override void Initialize(string name, NameValueCollection config)
        {
            //
            // Initialize values from web.config.
            //

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (name == null || name.Length == 0)
            {
                name = "FbRoleProvider";
            }

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "FB Role Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                applicationName = config["applicationName"];
            }

            if (config["writeExceptionsToEventLog"] != null)
            {
                if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
                {
                    writeExceptionsToEventLog = true;
                }
            }

            //
            // Initialize FbConnection.
            //

            connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (connectionStringSettings == null || connectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = connectionStringSettings.ConnectionString;
        }

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                if (username.Contains(","))
                {
                    throw new ArgumentException("User names cannot contain commas.");
                }

                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_ADDUSERTOROLE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    FbParameter roleParameter = cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 100);
                    FbParameter userParameter = cmd.Parameters.Add("@Username", FbDbType.VarChar, 100);
                    FbTransaction trans = conn.BeginTransaction();
                    cmd.Transaction = trans;

                    try
                    {
                        foreach (string username in usernames)
                        {
                            foreach (string rolename in rolenames)
                            {
                                userParameter.Value = username;
                                roleParameter.Value = rolename;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                    catch (FbException ex)
                    {
                        trans.Rollback();

                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "AddUsersToRoles");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        public override void CreateRole(string rolename)
        {
            if (rolename.Contains(","))
            {
                throw new ArgumentException("Role names cannot contain commas.");
            }

            if (RoleExists(rolename))
            {
                throw new ProviderException("Role name already exists.");
            }

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_CREATEROLE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 100).Value = rolename;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (FbException ex)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "CreateRole");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            if (!RoleExists(rolename))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_DELETEROLE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 100).Value = rolename;
                    FbTransaction tran = conn.BeginTransaction();
                    cmd.Transaction = tran;

                    try
                    {
                        cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                    catch (FbException ex)
                    {
                        tran.Rollback();

                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "DeleteRole");

                            return false;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
            return true;
        }

        public override string[] GetAllRoles()
        {
            StringBuilder roleNames = new StringBuilder();

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_GETALLROLES", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;

                    try
                    {
                        using (FbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roleNames.Append(reader.GetString(0));
                                roleNames.Append(",");
                            }
                        }
                    }
                    catch (FbException ex)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "GetAllRoles");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            if (roleNames.Length > 0)
            {
                // Remove trailing comma.
                roleNames.Remove(roleNames.Length - 1, 1);

                return roleNames.ToString().Split(',');
            }
            return new string[0];
        }

        public override string[] GetRolesForUser(string username)
        {
            StringBuilder roleNames = new StringBuilder();

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_GETUSERROLES", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Username", FbDbType.VarChar, 100).Value = username;
                    try
                    {
                        using (FbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roleNames.Append(reader.GetString(0));
                                roleNames.Append(",");
                            }
                        }
                    }
                    catch (FbException ex)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "GetRolesForUser");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            if (roleNames.Length > 0)
            {
                // Remove trailing comma.
                roleNames.Remove(roleNames.Length - 1, 1);

                return roleNames.ToString().Split(',');
            }
            return new string[0];
        }

        public override string[] GetUsersInRole(string rolename)
        {
            StringBuilder userNames = new StringBuilder();

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_GETROLEUSERS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 100).Value = rolename;

                    try
                    {
                        using (FbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                userNames.Append(reader.GetString(0));
                                userNames.Append(",");
                            }
                        }
                    }
                    catch (FbException ex)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "GetUsersInRole");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            if (userNames.Length > 0)
            {
                // Remove trailing comma.
                userNames.Remove(userNames.Length - 1, 1);

                return userNames.ToString().Split(',');
            }
            return new string[0];
        }

        public override bool IsUserInRole(string username, string rolename)
        {
            bool userIsInRole = false;

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_ISUSERINROLE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 100).Value = rolename;
                    cmd.Parameters.Add("@Username", FbDbType.VarChar, 100).Value = username;

                    try
                    {
                        int numRecs = (int)cmd.ExecuteScalar();

                        userIsInRole = numRecs > 0;
                    }
                    catch (FbException ex)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "IsUserInRole");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            return userIsInRole;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_DELETEUSERROLE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    FbParameter roleParameter = cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 100);
                    FbParameter userParameter = cmd.Parameters.Add("@Username", FbDbType.VarChar, 100);
                    FbTransaction trans = conn.BeginTransaction();
                    cmd.Transaction = trans;

                    try
                    {
                        foreach (string username in usernames)
                        {
                            foreach (string rolename in rolenames)
                            {
                                userParameter.Value = username;
                                roleParameter.Value = rolename;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                    catch (FbException ex)
                    {
                        trans.Rollback();

                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "RemoveUsersFromRoles");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        public override bool RoleExists(string rolename)
        {
            bool exists = false;

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_ISEXISTS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 100).Value = rolename;

                    try
                    {
                        int numRecs = (int)cmd.ExecuteScalar();

                        exists = numRecs > 0;
                    }
                    catch (FbException ex)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "RoleExists");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            return exists;
        }

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            StringBuilder userNames = new StringBuilder();

            using (FbConnection conn = new FbConnection(connectionString))
            {
                conn.Open();
                using (FbCommand cmd = new FbCommand("ROLES_FINDROLEUSERS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = applicationName;
                    cmd.Parameters.Add("@RoleName", FbDbType.VarChar, 100).Value = rolename;
                    cmd.Parameters.Add("@UsernameSearch", FbDbType.VarChar, 100).Value = usernameToMatch.ToUpper();


                    try
                    {
                        using (FbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                userNames.Append(reader.GetString(0));
                                userNames.Append(",");
                            }
                        }
                    }
                    catch (FbException ex)
                    {
                        if (WriteExceptionsToEventLog)
                        {
                            WriteToEventLog(ex, "FindUsersInRole");
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }

            if (userNames.Length > 0)
            {
                // Remove trailing comma.
                userNames.Remove(userNames.Length - 1, 1);

                return userNames.ToString().Split(',');
            }
            return new string[0];
        }

        #endregion

        #region � Private Methods �

        private void WriteToEventLog(FbException e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = string.Empty;
            message += "An exception occurred. Please check the Event Log." + "\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

        #endregion
    }
}
