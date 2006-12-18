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
 *	Copyright (c) 2005 Carlos Guzmán Álvarez
 *	All Rights Reserved.
 * 
 *  Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Web.Providers
{
	/// <summary>
	/// References:
	///		http://msdn2.microsoft.com/en-us/library/f1kyba5e.aspx
	///		http://msdn2.microsoft.com/en-us/library/6tc47t75.aspx
	/// </summary>
	public class FbMembershipProvider : MembershipProvider
	{
        #region · Fields ·

        private int newPasswordLength;
        private string eventSource;
        private string eventLog;
        private string exceptionMessage;
        private string tableName;
        private string connectionectionString;
        private string applicationName;
        private bool enablePasswordReset;
        private bool enablePasswordRetrieval;
        private bool requiresQuestionAndAnswer;
        private bool requiresUniqueEmail;
        private int maxInvalidPasswordAttempts;
        private int passwordAttemptWindow;
        private MembershipPasswordFormat passwordFormat;
        private MachineKeySection machineKey;
        private bool writeExceptionsToEventLog;
        private int minRequiredNonAlphanumericCharacters;
        private int minRequiredPasswordLength;
        private string passwordStrengthRegularExpression;

        #endregion

        #region · Properties ·

        public bool WriteExceptionsToEventLog
        {
            get { return this.writeExceptionsToEventLog; }
            set { this.writeExceptionsToEventLog = value; }
        }

        #endregion

        #region · MembershipProvider Properties ·

        public override string ApplicationName
        {
            get { return this.applicationName; }
            set { this.applicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return this.enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return this.enablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return this.requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return this.requiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return this.maxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return this.passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return this.passwordFormat; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return this.minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return this.minRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return this.passwordStrengthRegularExpression; }
        }

        #endregion

        #region · Constructors ·

        public FbMembershipProvider()
        {
            this.newPasswordLength  = 8;
            this.eventSource        = "FbNewMembershipProvider";
            this.eventLog           = "Application";
            this.exceptionMessage   = "An exception occurred. Please check the Event Log.";
            this.tableName          = "Users";
        }

        #endregion

        #region · ProviderBase Overriden Methods ·

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (name == null || name.Length == 0)
            {
                name = "FirebirdNewMembershipProvider";
            }

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Firebird New Membership provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            this.applicationName            = GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);
            this.maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            this.passwordAttemptWindow      = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            this.minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            this.minRequiredPasswordLength  = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            this.passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            this.enablePasswordReset        = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            this.enablePasswordRetrieval    = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            this.requiresQuestionAndAnswer  = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            this.requiresUniqueEmail        = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            this.writeExceptionsToEventLog  = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            string temp_format = config["passwordFormat"];

            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    this.passwordFormat = MembershipPasswordFormat.Hashed;
                    break;

                case "Encrypted":
                    this.passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;

                case "Clear":
                    this.passwordFormat = MembershipPasswordFormat.Clear;
                    break;

                default:
                    throw new ProviderException("Password format not supported.");
            }

            //
            // Initialize FbConnection.
            //
            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionectionString = ConnectionStringSettings.ConnectionString;

            // Get encryption and decryption key information from the configuration.
            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (machineKey.ValidationKey.Contains("AutoGenerate"))
            {
                if (this.PasswordFormat != MembershipPasswordFormat.Clear)
                {
                    throw new ProviderException("Hashed or Encrypted passwords are not supported with auto-generated keys.");
                }
            }
        }

        #endregion

        #region · MembershipProvider Override Methods ·

        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            int rowsAffected = 0;

            if (!this.ValidateUser(username, oldPwd))
            {
                return false;
            }

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPwd, true);

            this.OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                else
                {
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
                }
            }

            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("UPDATE " + this.tableName + "" +
                " SET UserPassword = @Password, LastPasswordChangedDate = @LastPasswordChangedDate " +
                " WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Password", FbDbType.VarChar, 255).Value                = this.EncodePassword(newPwd);
            command.Parameters.Add("@LastPasswordChangedDate", FbDbType.TimeStamp).Value    = DateTime.Now;
            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value                = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value         = this.applicationName;

            try
            {
                connection.Open();

                rowsAffected = command.ExecuteNonQuery();
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "ChangePassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                connection.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public override bool ChangePasswordQuestionAndAnswer(
            string username,
            string password,
            string newPwdQuestion,
            string newPwdAnswer)
        {
            int rowsAffected = 0;

            if (!this.ValidateUser(username, password))
            {
                return false;
            }

            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("UPDATE " + this.tableName + "" +
                " SET PasswordQuestion = @Question, PasswordAnswer = @Answer" +
                " WHERE Username = @UserName AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Question", FbDbType.VarChar, 255).Value = newPwdQuestion;
            command.Parameters.Add("@Answer", FbDbType.VarChar, 255).Value = this.EncodePassword(newPwdAnswer);
            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            try
            {
                connection.Open();

                rowsAffected = command.ExecuteNonQuery();
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "ChangePasswordQuestionAndAnswer");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                connection.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public override MembershipUser CreateUser(
            string username,
            string password,
            string email,
            string passwordQuestion,
            string passwordAnswer,
            bool isApproved,
            object providerUserKey,
            out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            this.OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (this.RequiresUniqueEmail && this.GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = this.GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                FbConnection connection = new FbConnection(connectionectionString);
                FbCommand command = new FbCommand("INSERT INTO " + this.tableName + "" +
                    " (PKID, Username, UserPassword, Email, PasswordQuestion, " +
                    " PasswordAnswer, IsApproved," +
                    " Comment, CreationDate, LastPasswordChangedDate, LastActivityDate," +
                    " ApplicationName, IsLockedOut, LastLockedOutDate," +
                    " FailedPasswordAttemptCount, FailedPasswordAttemptStart, " +
                    " FailedPasswordAnswerCount, FailedPasswordAnswerStart)" +
                    " Values(@PKID, @Username, @Password, @Email, @PasswordQuestion, @PasswordAnswer, @IsApproved, " +
                    "@Comment, @CreationDate, @LastPasswordChangedDate, @LastActivityDate, @ApplicationName, " +
                    "@IsLockedOut, @LastLockedOutDate, @FailedPasswordAttemptCount, @FailedPasswordAttemptWindowStart, " +
                    "@FailedPasswordAnswerAttemptCount, @FailedPasswordAnswerAttemptWindowStart)", connection);

                command.Parameters.Add("@PKID", FbDbType.Guid).Value = providerUserKey;
                command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                command.Parameters.Add("@Password", FbDbType.VarChar, 255).Value = this.EncodePassword(password);
                command.Parameters.Add("@Email", FbDbType.VarChar, 128).Value = email;
                command.Parameters.Add("@PasswordQuestion", FbDbType.VarChar, 255).Value = passwordQuestion;
                command.Parameters.Add("@PasswordAnswer", FbDbType.VarChar, 255).Value = this.EncodePassword(passwordAnswer);
                command.Parameters.Add("@IsApproved", FbDbType.SmallInt).Value = (isApproved ? 1 : 0);
                command.Parameters.Add("@Comment", FbDbType.VarChar, 255).Value = "";
                command.Parameters.Add("@CreationDate", FbDbType.TimeStamp).Value = createDate;
                command.Parameters.Add("@LastPasswordChangedDate", FbDbType.TimeStamp).Value = createDate;
                command.Parameters.Add("@LastActivityDate", FbDbType.TimeStamp).Value = createDate;
                command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;
                command.Parameters.Add("@IsLockedOut", FbDbType.SmallInt).Value = 0;
                command.Parameters.Add("@LastLockedOutDate", FbDbType.TimeStamp).Value = createDate;
                command.Parameters.Add("@FailedPasswordAttemptCount", FbDbType.Integer).Value = 0;
                command.Parameters.Add("@FailedPasswordAttemptWindowStart", FbDbType.TimeStamp).Value = createDate;
                command.Parameters.Add("@FailedPasswordAnswerAttemptCount", FbDbType.Integer).Value = 0;
                command.Parameters.Add("@FailedPasswordAnswerAttemptWindowStart", FbDbType.TimeStamp).Value = createDate;

                try
                {
                    connection.Open();

                    int recAdded = command.ExecuteNonQuery();

                    if (recAdded > 0)
                    {
                        status = MembershipCreateStatus.Success;
                    }
                    else
                    {
                        status = MembershipCreateStatus.UserRejected;
                    }
                }
                catch (FbException e)
                {
                    if (this.WriteExceptionsToEventLog)
                    {
                        this.WriteToEventLog(e, "CreateUser");
                    }

                    status = MembershipCreateStatus.ProviderError;
                }
                finally
                {
                    connection.Close();
                }

                return this.GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }

            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            int rowsAffected = 0;

            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("DELETE FROM " + this.tableName + "" +
                " WHERE Username = @Username AND Applicationname = @ApplicationName", connection);

            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value        = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            try
            {
                connection.Open();

                rowsAffected = command.ExecuteNonQuery();

                if (deleteAllRelatedData)
                {
                    DeleteAllRelatedData(username);
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "DeleteUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                connection.Close();
            }

            return (rowsAffected > 0);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            FbConnection    connection  = new FbConnection(connectionectionString);
            FbCommand       command     = new FbCommand("SELECT Count(*) FROM " + this.tableName + " WHERE ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.ApplicationName;

            MembershipUserCollection users = new MembershipUserCollection();
            FbDataReader reader = null;
            totalRecords = 0;

            try
            {
                connection.Open();
                totalRecords = (int)command.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                int startIndex  = pageSize * pageIndex;
                int endIndex    = startIndex + pageSize - 1;

                command.CommandText = "SELECT FIRST (@first) SKIP (@skip) PKID, Username, Email, PasswordQuestion," +
                    " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                    " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " +
                    " FROM " + this.tableName +
                    " WHERE ApplicationName = @ApplicationName " +
                    " ORDER BY Username Asc";

                command.Parameters.Add("@first", FbDbType.Integer).Value    = pageSize;
                command.Parameters.Add("@skip", FbDbType.Integer).Value     = startIndex;

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MembershipUser u = this.GetUserFromReader(reader);
                    users.Add(u);
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "GetAllUsers");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            int         numOnline   = 0;
            TimeSpan    onlineSpan  = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime    compareTime = DateTime.Now.Subtract(onlineSpan);

            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT Count(*) FROM " + this.tableName + "" +
                " WHERE LastActivityDate > @CompareDate AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@CompareDate", FbDbType.TimeStamp).Value = compareTime;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            try
            {
                connection.Open();

                numOnline = (int)command.ExecuteScalar();
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "GetNumberOfUsersOnline");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                connection.Close();
            }

            return numOnline;
        }

        public override string GetPassword(string username, string answer)
        {
            if (!this.EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (this.PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT UserPassword, PasswordAnswer, IsLockedOut FROM " + this.tableName + "" +
                " WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            string password = "";
            string passwordAnswer = "";
            FbDataReader reader = null;

            try
            {
                connection.Open();

                reader = command.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.Read())
                {

                    if (reader.GetBoolean(2))
                    {
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    }

                    password = reader.GetString(0);
                    passwordAnswer = reader.GetString(1);
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "GetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            if (this.RequiresQuestionAndAnswer && !this.CheckPassword(answer, passwordAnswer))
            {
                this.UpdateFailureCount(username, "passwordAnswer");

                throw new MembershipPasswordException("Incorrect password answer.");
            }

            if (this.PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = this.UnEncodePassword(password);
            }

            return password;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT PKID, Username, Email, PasswordQuestion," +
                " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
                " FROM " + this.tableName + " WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            MembershipUser u = null;
            FbDataReader reader = null;

            try
            {
                connection.Open();

                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    u = this.GetUserFromReader(reader);
                }

                if (userIsOnline)
                {
                    FbCommand updateCmd = new FbCommand("UPDATE " + this.tableName + " " +
                        "SET LastActivityDate = @LastActivityDate " +
                        "WHERE Username = @Username AND Applicationname = @ApplicationName", connection);

                    updateCmd.Parameters.Add("@LastActivityDate", FbDbType.TimeStamp).Value = DateTime.Now;
                    updateCmd.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                    updateCmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "GetUser(String, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            return u;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT PKID, Username, Email, PasswordQuestion," +
                " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
                " FROM " + this.tableName + " WHERE PKID = @PKID", connection);

            command.Parameters.Add("@PKID", FbDbType.Guid).Value = providerUserKey;

            MembershipUser u = null;
            FbDataReader reader = null;

            try
            {
                connection.Open();

                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    u = this.GetUserFromReader(reader);
                }

                if (userIsOnline)
                {
                    FbCommand updateCmd = new FbCommand("UPDATE " + this.tableName + " " +
                        "SET LastActivityDate = @LastActivityDate " +
                        "WHERE PKID = @PKID", connection);

                    updateCmd.Parameters.Add("@LastActivityDate", FbDbType.TimeStamp).Value = DateTime.Now;
                    updateCmd.Parameters.Add("@PKID", FbDbType.Guid).Value = providerUserKey;

                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "GetUser(Object, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            return u;
        }


        public override bool UnlockUser(string username)
        {
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("UPDATE " + this.tableName + " " +
                " SET IsLockedOut = 0, LastLockedOutDate = @LastLockedOutDate " +
                " WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@LastLockedOutDate", FbDbType.TimeStamp).Value = DateTime.Now;
            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            int rowsAffected = 0;

            try
            {
                connection.Open();

                rowsAffected = command.ExecuteNonQuery();
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "UnlockUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                connection.Close();
            }

            return (rowsAffected > 0);
        }

        //
        // MembershipProvider.GetUserNameByEmail
        //
        public override string GetUserNameByEmail(string email)
        {
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT Username" +
                " FROM " + this.tableName + " WHERE Email = @Email AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Email", FbDbType.VarChar, 128).Value = email;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            string username = "";

            try
            {
                connection.Open();

                username = (string)command.ExecuteScalar();
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "GetUserNameByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                connection.Close();
            }

            return ((username == null) ? "" : username);
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!this.EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && this.RequiresQuestionAndAnswer)
            {
                this.UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword = Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            this.OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                else
                {
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");
                }
            }

            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT PasswordAnswer, IsLockedOut FROM " + this.tableName + "" +
                " WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            int             rowsAffected    = 0;
            string          passwordAnswer  = "";
            FbDataReader    reader          = null;

            try
            {
                connection.Open();

                reader = command.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.Read())
                {
                    if (reader.GetBoolean(1))
                    {
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    }

                    passwordAnswer = reader.GetString(0);
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }

                if (this.RequiresQuestionAndAnswer && !this.CheckPassword(answer, passwordAnswer))
                {
                    this.UpdateFailureCount(username, "passwordAnswer");

                    throw new MembershipPasswordException("Incorrect password answer.");
                }

                FbCommand updateCmd = new FbCommand("UPDATE " + this.tableName + "" +
                    " SET UserPassword = @Password, LastPasswordChangedDate = @LastPasswordChangedDate" +
                    " WHERE Username = @Username AND ApplicationName = @ApplicationName AND IsLockedOut = 0", connection);

                updateCmd.Parameters.Add("@Password", FbDbType.VarChar, 255).Value = EncodePassword(newPassword);
                updateCmd.Parameters.Add("@LastPasswordChangedDate", FbDbType.TimeStamp).Value = DateTime.Now;
                updateCmd.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                updateCmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

                rowsAffected = updateCmd.ExecuteNonQuery();
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "ResetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            if (rowsAffected > 0)
            {
                return newPassword;
            }
            else
            {
                throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand commandCheck = new FbCommand("SELECT PKID" +
                    " FROM " + this.tableName + " WHERE Email = @Email and PKID != @PKID  AND ApplicationName = @APP", connection);
            commandCheck.Parameters.Add("@PKID", FbDbType.Guid).Value = user.ProviderUserKey;
            commandCheck.Parameters.Add("@Email", FbDbType.VarChar).Value = user.Email;
            commandCheck.Parameters.Add("@APP", FbDbType.VarChar).Value = this.applicationName;
            FbCommand command = new FbCommand("UPDATE " + this.tableName + "" +
                " SET Email = @Email, Comment = @Comment," +
                " IsApproved = @IsApproved" +
                " WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Email", FbDbType.VarChar, 128).Value = user.Email;
            command.Parameters.Add("@Comment", FbDbType.VarChar, 255).Value = user.Comment;
            command.Parameters.Add("@IsApproved", FbDbType.SmallInt).Value = (user.IsApproved ? 1 : 0);
            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = user.UserName;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            try
            {
                connection.Open();
                
                if (this.RequiresUniqueEmail)
                {
                    object exists = commandCheck.ExecuteScalar();
                    if (exists != null)
                    {
                        throw new ProviderException("The E-mail supplied is invalid.");
                    }
                }
                command.ExecuteNonQuery();
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "UpdateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                connection.Close();
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT UserPassword, IsApproved FROM " + this.tableName +
                " WHERE Username = @Username AND ApplicationName = @ApplicationName AND IsLockedOut = 0", connection);

            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            FbDataReader reader = null;
            bool isApproved = false;
            string pwd = "";

            try
            {
                connection.Open();

                reader = command.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.Read())
                {
                    pwd = reader.GetString(0);
                    isApproved = reader.GetBoolean(1);
                }
                else
                {
                    return false;
                }

                reader.Close();

                if (this.CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        isValid = true;

                        FbCommand updateCmd = new FbCommand("UPDATE " + this.tableName + " SET LastLoginDate = @LastLoginDate" +
                            " WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

                        updateCmd.Parameters.Add("@LastLoginDate", FbDbType.TimeStamp).Value = DateTime.Now;
                        updateCmd.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                        updateCmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    connection.Close();

                    this.UpdateFailureCount(username, "password");
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "ValidateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            return isValid;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT Count(*) FROM " + this.tableName + " " +
                "WHERE Username LIKE @UsernameSearch AND ApplicationName = @ApplicationName", connection);
            command.Parameters.Add("@UsernameSearch", FbDbType.VarChar, 255).Value = usernameToMatch;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            MembershipUserCollection users = new MembershipUserCollection();

            FbDataReader reader = null;

            try
            {
                connection.Open();
                totalRecords = (int)command.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                int startIndex  = pageSize * pageIndex;
                int endIndex    = startIndex + pageSize - 1;

                command.CommandText = "SELECT FIRST (@first) SKIP (@skip) PKID, Username, Email, PasswordQuestion," +
                    " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                    " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " +
                    " FROM " + this.tableName +
                    " WHERE Username LIKE @UsernameSearch AND ApplicationName = @ApplicationName " +
                    " ORDER BY Username Asc";

                command.Parameters.Add("@first", FbDbType.Integer).Value = pageSize;
                command.Parameters.Add("@skip", FbDbType.Integer).Value = startIndex;

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MembershipUser u = this.GetUserFromReader(reader);
                    users.Add(u);
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "FindUsersByName");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            return users;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT Count(*) FROM " + this.tableName + " " +
                "WHERE Email LIKE @EmailSearch AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@EmailSearch", FbDbType.VarChar, 255).Value = emailToMatch;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.ApplicationName;

            MembershipUserCollection users = new MembershipUserCollection();

            FbDataReader reader = null;
            totalRecords = 0;

            try
            {
                connection.Open();
                totalRecords = (int)command.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                command.CommandText = "SELECT FIRST (@first) SKIP (@skip) PKID, Username, Email, PasswordQuestion," +
                    " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                    " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " +
                    " FROM " + this.tableName +
                    " WHERE Email LIKE @EmailSearch AND ApplicationName = @ApplicationName " +
                    " ORDER BY Username Asc";

                command.Parameters.Add("@first", FbDbType.Integer).Value = pageSize;
                command.Parameters.Add("@skip", FbDbType.Integer).Value = startIndex;

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MembershipUser u = this.GetUserFromReader(reader);
                    users.Add(u);
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "FindUsersByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }

            return users;
        }

        #endregion

        #region · Protected Methods ·

        protected virtual void DeleteAllRelatedData(string username)
        {
        }

        #endregion

        #region · Private Methods ·

        //
        // GetUserFromReader
        //    A helper function that takes the current row from the FbDataReader
        // and hydrates a MembershiUser from the values. Called by the 
        // MembershipUser.GetUser implementation.
        //
        private MembershipUser GetUserFromReader(FbDataReader reader)
        {
            object providerUserKey  = reader.GetValue(0);
            string username         = reader.GetString(1);
            string email            = reader.GetString(2);

            string passwordQuestion = "";
            if (!reader.IsDBNull(3))
            {
                passwordQuestion = reader.GetString(3);
            }

            string comment = "";
            if (!reader.IsDBNull(4))
            {
                comment = reader.GetString(4);
            }

            bool isApproved         = reader.GetBoolean(5);
            bool isLockedOut        = reader.GetBoolean(6);
            DateTime creationDate   = reader.GetDateTime(7);

            DateTime lastLoginDate = new DateTime();
            if (!reader.IsDBNull(8))
            {
                lastLoginDate = reader.GetDateTime(8);
            }

            DateTime lastActivityDate = reader.GetDateTime(9);
            DateTime lastPasswordChangedDate = reader.GetDateTime(10);

            DateTime lastLockedOutDate = new DateTime();
            if (!reader.IsDBNull(11))
            {
                lastLockedOutDate = reader.GetDateTime(11);
            }

            MembershipUser membershipUser = new MembershipUser(
                this.Name,
                username,
                providerUserKey,
                email,
                passwordQuestion,
                comment,
                isApproved,
                isLockedOut,
                creationDate,
                lastLoginDate,
                lastActivityDate,
                lastPasswordChangedDate,
                lastLockedOutDate);

            return membershipUser;
        }

        //
        // A helper function to retrieve config values from the configuration file.
        //
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
            {
                return defaultValue;
            }

            return configValue;
        }

        //
        // UpdateFailureCount
        //   A helper method that performs the checks and updates associated with
        // password failure tracking.
        //
        private void UpdateFailureCount(string username, string failureType)
        {
            FbConnection connection = new FbConnection(connectionectionString);
            FbCommand command = new FbCommand("SELECT FailedPasswordAttemptCount, " +
                "  FailedPasswordAttemptStart, " +
                "  FailedPasswordAnswerCount, " +
                "  FailedPasswordAnswerStart " +
                "  FROM " + this.tableName + " " +
                "  WHERE Username = @Username AND ApplicationName = @ApplicationName", connection);

            command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
            command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

            FbDataReader reader = null;
            DateTime windowStart = new DateTime();
            int failureCount = 0;

            try
            {
                connection.Open();

                reader = command.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.Read())
                {
                    if (failureType == "password")
                    {
                        failureCount = reader.GetInt32(0);
                        windowStart = reader.GetDateTime(1);
                    }

                    if (failureType == "passwordAnswer")
                    {
                        failureCount = reader.GetInt32(2);
                        windowStart = reader.GetDateTime(3);
                    }
                }

                reader.Close();

                DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                if (failureCount == 0 || DateTime.Now > windowEnd)
                {
                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.
                    if (failureType == "password")
                    {
                        command.CommandText = "UPDATE " + this.tableName + " " +
                            "  SET FailedPasswordAttemptCount = @Count, " +
                            "      FailedPasswordAttemptStart = @WindowStart " +
                            "  WHERE Username = @Username AND ApplicationName = @ApplicationName";
                    }

                    if (failureType == "passwordAnswer")
                    {
                        command.CommandText = "UPDATE " + this.tableName + " " +
                            "  SET FailedPasswordAnswerCount = @Count, " +
                            "      FailedPasswordAnswerStart = @WindowStart " +
                            "  WHERE Username = @Username AND ApplicationName = @ApplicationName";
                    }

                    command.Parameters.Clear();

                    command.Parameters.Add("@Count", FbDbType.Integer).Value = 1;
                    command.Parameters.Add("@WindowStart", FbDbType.TimeStamp).Value = DateTime.Now;
                    command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                    command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = applicationName;

                    if (command.ExecuteNonQuery() < 0)
                    {
                        throw new ProviderException("Unable to update failure count and window start.");
                    }
                }
                else
                {
                    if (failureCount++ >= this.MaxInvalidPasswordAttempts)
                    {
                        // Password attempts have exceeded the failure threshold. Lock out
                        // the user.
                        command.CommandText = "UPDATE " + this.tableName + " " +
                            "  SET IsLockedOut = @IsLockedOut, LastLockedOutDate = @LastLockedOutDate " +
                            "  WHERE Username = @Username AND ApplicationName = @ApplicationName";

                        command.Parameters.Clear();

                        command.Parameters.Add("@IsLockedOut", FbDbType.SmallInt).Value = 1;
                        command.Parameters.Add("@LastLockedOutDate", FbDbType.TimeStamp).Value = DateTime.Now;
                        command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                        command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

                        if (command.ExecuteNonQuery() < 0)
                        {
                            throw new ProviderException("Unable to lock out user.");
                        }
                    }
                    else
                    {
                        // Password attempts have not exceeded the failure threshold. Update
                        // the failure counts. Leave the window the same.
                        if (failureType == "password")
                        {
                            command.CommandText = "UPDATE " + this.tableName + " " +
                                "  SET FailedPasswordAttemptCount = @Count" +
                                "  WHERE Username = @Username AND ApplicationName = @ApplicationName";
                        }

                        if (failureType == "passwordAnswer")
                        {
                            command.CommandText = "UPDATE " + this.tableName + " " +
                                "  SET FailedPasswordAnswerCount = @Count" +
                                "  WHERE Username = @Username AND ApplicationName = @ApplicationName";
                        }

                        command.Parameters.Clear();

                        command.Parameters.Add("@Count", FbDbType.Integer).Value = failureCount;
                        command.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                        command.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = this.applicationName;

                        if (command.ExecuteNonQuery() < 0)
                        {
                            throw new ProviderException("Unable to update failure count.");
                        }
                    }
                }
            }
            catch (FbException e)
            {
                if (this.WriteExceptionsToEventLog)
                {
                    this.WriteToEventLog(e, "UpdateFailureCount");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                connection.Close();
            }
        }

        //
        // CheckPassword
        //   Compares password values based on the MembershipPasswordFormat.
        //
        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (this.PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = this.UnEncodePassword(dbpassword);
                    break;

                case MembershipPasswordFormat.Hashed:
                    pass1 = this.EncodePassword(password);
                    break;

                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        //
        // EncodePassword
        //   Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
        //
        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            if (password != null)
            {
                switch (PasswordFormat)
                {
                    case MembershipPasswordFormat.Clear:
                        break;

                    case MembershipPasswordFormat.Encrypted:
                        encodedPassword = Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                        break;

                    case MembershipPasswordFormat.Hashed:
                        HMACSHA1 hash = new HMACSHA1();
                        hash.Key = HexToByte(machineKey.ValidationKey);
                        encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                        break;

                    default:
                        throw new ProviderException("Unsupported password format.");
                }
            }

            return encodedPassword;
        }

        //
        // UnEncodePassword
        //   Decrypts or leaves the password clear based on the PasswordFormat.
        //
        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (this.PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;

                case MembershipPasswordFormat.Encrypted:
                    password = Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;

                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");

                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        //
        // HexToByte
        //   Converts a hexadecimal string to a byte array. Used to convert encryption
        // key values from the configuration.
        //
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return returnBytes;
        }

        //
        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        //
        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

        #endregion
    }
}