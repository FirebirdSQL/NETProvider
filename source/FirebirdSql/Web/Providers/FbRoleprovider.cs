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
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private ConnectionStringSettings pConnectionStringSettings;
        private string connectionString;
        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //
        private bool pWriteExceptionsToEventLog = false;
        private string pApplicationName;
        #endregion

        #region � Properties �
        
        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }

        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
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
                config.Add("description", "Fb Role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                pApplicationName = config["applicationName"];
            }

            if (config["writeExceptionsToEventLog"] != null)
            {
                if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
                {
                    pWriteExceptionsToEventLog = true;
                }
            }

            //
            // Initialize FbConnection.
            //

            pConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (pConnectionStringSettings == null || pConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = pConnectionStringSettings.ConnectionString;
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

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("INSERT INTO UsersInRoles " +
                    " (Username, Rolename, ApplicationName) " +
                    " Values(?, ?, ?)", conn);

            FbParameter userParm = cmd.Parameters.Add("@Username", FbDbType.VarChar, 255);
            FbParameter roleParm = cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 255);
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            FbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                foreach (string username in usernames)
                {
                    foreach (string rolename in rolenames)
                    {
                        userParm.Value = username;
                        roleParm.Value = rolename;
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
            }
            catch (FbException e)
            {
                try
                {
                    tran.Rollback();
                }
                catch 
                { 
                }


                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "AddUsersToRoles");
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
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

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("INSERT INTO Roles " +
                    " (Rolename, ApplicationName) " +
                    " Values(?, ?)", conn);

            cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 255).Value = rolename;
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (FbException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "CreateRole");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                conn.Close();
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

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("DELETE FROM Roles " +
                    " WHERE Rolename = ? AND ApplicationName = ?", conn);

            cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 255).Value = rolename;
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;


            FbCommand cmd2 = new FbCommand("DELETE FROM UsersInRoles " +
                    " WHERE Rolename = ? AND ApplicationName = ?", conn);

            cmd2.Parameters.Add("@Rolename", FbDbType.VarChar, 255).Value = rolename;
            cmd2.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            FbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;
                cmd2.Transaction = tran;

                cmd2.ExecuteNonQuery();
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (FbException e)
            {
                try
                {
                    tran.Rollback();
                }
                catch 
                {
                }


                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteRole");

                    return false;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                conn.Close();
            }

            return true;
        }

        public override string[] GetAllRoles()
        {
            string tmpRoleNames = "";

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("SELECT Rolename FROM Roles " +
                      " WHERE ApplicationName = ?", conn);

            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            FbDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpRoleNames += reader.GetString(0) + ",";
                }
            }
            catch (FbException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetAllRoles");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (reader != null) 
                { 
                    reader.Close(); 
                }

                conn.Close();
            }

            if (tmpRoleNames.Length > 0)
            {
                // Remove trailing comma.
                tmpRoleNames = tmpRoleNames.Substring(0, tmpRoleNames.Length - 1);

                return tmpRoleNames.Split(',');
            }

            return new string[0];
        }

        public override string[] GetRolesForUser(string username)
        {
            string tmpRoleNames = "";

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("SELECT Rolename FROM UsersInRoles " +
                    " WHERE Username = ? AND ApplicationName = ?", conn);

            cmd.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            FbDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpRoleNames += reader.GetString(0) + ",";
                }
            }
            catch (FbException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetRolesForUser");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (reader != null) 
                { 
                    reader.Close(); 
                }
                conn.Close();
            }

            if (tmpRoleNames.Length > 0)
            {
                // Remove trailing comma.
                tmpRoleNames = tmpRoleNames.Substring(0, tmpRoleNames.Length - 1);

                return tmpRoleNames.Split(',');
            }

            return new string[0];
        }

        public override string[] GetUsersInRole(string rolename)
        {
            string tmpUserNames = "";

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("SELECT Username FROM UsersInRoles " +
                      " WHERE Rolename = ? AND ApplicationName = ?", conn);

            cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 255).Value = rolename;
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            FbDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpUserNames += reader.GetString(0) + ",";
                }
            }
            catch (FbException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUsersInRole");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (reader != null) 
                { 
                    reader.Close(); 
                }

                conn.Close();
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
            }

            return new string[0];
        }

        public override bool IsUserInRole(string username, string rolename)
        {
            bool userIsInRole = false;

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("SELECT COUNT(*) FROM UsersInRoles " +
                    " WHERE Username = ? AND Rolename = ? AND ApplicationName = ?", conn);

            cmd.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 255).Value = rolename;
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            try
            {
                conn.Open();

                int numRecs = (int)cmd.ExecuteScalar();

                if (numRecs > 0)
                {
                    userIsInRole = true;
                }
            }
            catch (FbException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "IsUserInRole");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                conn.Close();
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

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("DELETE FROM UsersInRoles " +
                    " WHERE Username = ? AND Rolename = ? AND ApplicationName = ?", conn);

            FbParameter userParm = cmd.Parameters.Add("@Username", FbDbType.VarChar, 255);
            FbParameter roleParm = cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 255);
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            FbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                foreach (string username in usernames)
                {
                    foreach (string rolename in rolenames)
                    {
                        userParm.Value = username;
                        roleParm.Value = rolename;
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
            }
            catch (FbException e)
            {
                try
                {
                    tran.Rollback();
                }
                catch 
                { 
                }

                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "RemoveUsersFromRoles");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public override bool RoleExists(string rolename)
        {
            bool exists = false;

            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("SELECT COUNT(*) FROM Roles " +
                      " WHERE Rolename = ? AND ApplicationName = ?", conn);

            cmd.Parameters.Add("@Rolename", FbDbType.VarChar, 255).Value = rolename;
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;

            try
            {
                conn.Open();

                int numRecs = (int)cmd.ExecuteScalar();

                if (numRecs > 0)
                {
                    exists = true;
                }
            }
            catch (FbException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "RoleExists");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                conn.Close();
            }

            return exists;
        }

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            FbConnection conn = new FbConnection(connectionString);
            FbCommand cmd = new FbCommand("SELECT Username FROM UsersInRoles  " +
                      "WHERE Username LIKE ? AND RoleName = ? AND ApplicationName = ?", conn);
            cmd.Parameters.Add("@UsernameSearch", FbDbType.VarChar, 255).Value = usernameToMatch;
            cmd.Parameters.Add("@RoleName", FbDbType.VarChar, 255).Value = rolename;
            cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = pApplicationName;

            string tmpUserNames = "";
            FbDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpUserNames += reader.GetString(0) + ",";
                }
            }
            catch (FbException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersInRole");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (reader != null) { reader.Close(); }

                conn.Close();
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
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

            string message = exceptionMessage + "\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

        #endregion
    }
}
