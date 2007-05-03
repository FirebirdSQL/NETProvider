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
 *  
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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Web.Providers
{
    public class FbMembershipProvider : MembershipProvider
    {
        #region � Fields �

        private string fbConnectionString;
        private bool enablePasswordRetrieval;
        private bool enablePasswordReset;
        private bool requiresQuestionAndAnswer;
        private string appName;
        private bool requiresUniqueEmail;
        private int maxInvalidPasswordAttempts;
        private int commandTimeout;
        private int passwordAttemptWindow;
        private int minRequiredPasswordLength;
        private int minRequiredNonalphanumericCharacters;
        private string passwordStrengthRegularExpression;
        private MachineKeySection machineKey;
        private MembershipPasswordFormat passwordFormat;
        private const int PASSWORD_SIZE = 14;

        #endregion

        #region � Properties �

        public override bool EnablePasswordRetrieval
        {
            get { return enablePasswordRetrieval; }
        }

        public override bool EnablePasswordReset
        {
            get { return enablePasswordReset; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return requiresUniqueEmail; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return passwordFormat; }
        }
        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonalphanumericCharacters; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        public override string ApplicationName
        {
            get { return appName; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("application name");

                if (value.Length > 100)
                {
                    throw new ProviderException("The application name is too long.");
                }

                this.appName = value;
            }
        }
        private int CommandTimeout
        {
            get { return commandTimeout; }
        }

        #endregion

        #region � ProviderBase Overriden Methods �

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (String.IsNullOrEmpty(name))
                name = "FbMembershipProvider";
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Firebird Membership provider");
            }
            base.Initialize(name, config);

            enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "false"));
            enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            minRequiredNonalphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonalphanumericCharacters"], "0"));
            passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if (passwordStrengthRegularExpression != null)
            {
                passwordStrengthRegularExpression = passwordStrengthRegularExpression.Trim();
                if (passwordStrengthRegularExpression.Length != 0)
                {
                    try
                    {
                        Regex regex = new Regex(passwordStrengthRegularExpression);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ProviderException(e.Message, e);
                    }
                }
            }
            else
            {
                passwordStrengthRegularExpression = string.Empty;
            }
            if (minRequiredNonalphanumericCharacters > minRequiredPasswordLength)
                throw new HttpException("The minRequiredNonalphanumericCharacters can not be greater than minRequiredPasswordLength.");

            commandTimeout = Convert.ToInt32(GetConfigValue(config["commandTimeout"], "30"));
            appName = config["applicationName"];
            if (string.IsNullOrEmpty(appName))
                appName = HostingEnvironment.ApplicationVirtualPath;

            if (appName.Length > 100)
            {
                throw new ProviderException("The application name is too long.");
            }

            string strTemp = config["passwordFormat"];
            if (strTemp == null)
                strTemp = "Hashed";

            switch (strTemp)
            {
                case "Clear":
                    passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                case "Encrypted":
                    passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Hashed":
                    passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                default:
                    throw new ProviderException("Specified password format is invalid.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed && EnablePasswordRetrieval)
                throw new ProviderException("Configured settings are invalid: Hashed passwords cannot be retrieved. Either set the password format to different type, or set supportsPasswordRetrieval to false.");

            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (machineKey.ValidationKey.Contains("AutoGenerate"))
            {
                if (this.PasswordFormat == MembershipPasswordFormat.Encrypted)
                {
                    throw new ProviderException("Encrypted passwords are not supported with auto-generated keys.");
                }
            }

            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            fbConnectionString = ConnectionStringSettings.ConnectionString;

            config.Remove("connectionStringName");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("commandTimeout");
            config.Remove("passwordFormat");
            config.Remove("name");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException("Attribute not recognized.");
            }
        }

        #endregion

        #region � MembershipProvider Override Methods �

        public override MembershipUser CreateUser(string username,
            string password,
            string email,
            string passwordQuestion,
            string passwordAnswer,
            bool isApproved,
            object providerUserKey,
            out MembershipCreateStatus status)
        {
            if (!ValidateParameter(ref password, true, true, false, 100))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            string salt = GenerateSalt();
            string pass = EncodePassword(password, (int)passwordFormat, salt);
            if (pass.Length > 100)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            string encodedPasswordAnswer;
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }

            if (!string.IsNullOrEmpty(passwordAnswer))
            {
                if (passwordAnswer.Length > 100)
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), (int)passwordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = passwordAnswer;
            }

            if (!ValidateParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, true, false, 100))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            if (!ValidateParameter(ref username, true, true, true, 100))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            if (!ValidateParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 100))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if (!ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 100))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            if (providerUserKey != null)
            {
                if (!(providerUserKey is Guid))
                {
                    status = MembershipCreateStatus.InvalidProviderUserKey;
                    return null;
                }
            }
            else
            {
                providerUserKey = Guid.NewGuid();
            }

            if (password.Length < MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            int count = 0;

            for (int i = 0; i < password.Length; i++)
            {
                if (!char.IsLetterOrDigit(password, i))
                {
                    count++;
                }
            }

            if (count < MinRequiredNonAlphanumericCharacters)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(password, PasswordStrengthRegularExpression))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            DateTime dt = RoundToSeconds(DateTime.UtcNow);

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_CreateUser", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Username", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@Password", FbDbType.VarChar, 100).Value = pass;
                    cmd.Parameters.Add("@PasswordSalt", FbDbType.VarChar, 100).Value = salt;
                    cmd.Parameters.Add("@Email", FbDbType.VarChar, 100).Value = email;
                    cmd.Parameters.Add("@PasswordQuestion", FbDbType.VarChar, 100).Value = passwordQuestion;
                    cmd.Parameters.Add("@PasswordAnswer", FbDbType.VarChar, 100).Value = encodedPasswordAnswer;
                    cmd.Parameters.Add("@IsApproved", FbDbType.SmallInt).Value = isApproved;
                    cmd.Parameters.Add("@UniqueEmail", FbDbType.SmallInt).Value = RequiresUniqueEmail ? 1 : 0;
                    cmd.Parameters.Add("@PasswordFormat", FbDbType.Integer).Value = (int)PasswordFormat;
                    cmd.Parameters.Add("@userid", FbDbType.Guid).Value = providerUserKey;

                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();

                    int iStatus = ((p.Value != null) ? ((int)p.Value) : -1);
                    if (iStatus < 0 || iStatus > (int)MembershipCreateStatus.ProviderError)
                    {
                        iStatus = (int)MembershipCreateStatus.ProviderError;
                    }
                    status = (MembershipCreateStatus)iStatus;
                    if (iStatus != 0)
                    {
                        return null;
                    }
                }
                dt = dt.ToLocalTime();
                return new MembershipUser(this.Name,
                    username,
                    providerUserKey,
                    email,
                    passwordQuestion,
                    null,
                    isApproved,
                    false,
                    dt,
                    dt,
                    dt,
                    dt,
                    new DateTime(1754, 1, 1));
            }
        }

        public override bool ChangePasswordQuestionAndAnswer(string username,
            string password,
            string newPasswordQuestion,
            string newPasswordAnswer)
        {
            CheckParameter(ref username, true, true, true, 100, "username");
            CheckParameter(ref password, true, true, false, 100, "password");

            string salt;
            int passwordFormat;
            if (!CheckPassword(username, password, false, false, out salt, out passwordFormat))
            {
                return false;
            }


            CheckParameter(ref newPasswordQuestion, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 100, "newPasswordQuestion");
            if (newPasswordAnswer != null)
            {
                newPasswordAnswer = newPasswordAnswer.Trim();
            }
            CheckParameter(ref newPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 100, "newPasswordAnswer");
            string encodedPasswordAnswer;
            if (!string.IsNullOrEmpty(newPasswordAnswer))
            {
                encodedPasswordAnswer = EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), (int)passwordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = newPasswordAnswer;
            }
            CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 100, "newPasswordAnswer");

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_PassQuestionAnswer", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@NewPasswordQuestion", FbDbType.VarChar, 100).Value = newPasswordQuestion;
                    cmd.Parameters.Add("@NewPasswordAnswer", FbDbType.VarChar, 100).Value = encodedPasswordAnswer;
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();

                    int status = ((p.Value != null) ? ((int)p.Value) : -1);
                    if (status != 0)
                    {
                        throw new ProviderException(GetExceptionText(status));
                    }

                    return (status == 0);
                }
            }
        }

        public override string GetPassword(string username, string passwordAnswer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new NotSupportedException("This membership provider has not been configured to support password retrieval.");
            }

            CheckParameter(ref username, true, true, true, 100, "username");
            string encodedPasswordAnswer = GetEncodedPasswordAnswer(username, passwordAnswer);
            CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 100, "passwordAnswer");

            string errorText;
            int passwordFormat = 0;
            int status = 0;

            string pass = GetPasswordFromDB(username, encodedPasswordAnswer, RequiresQuestionAndAnswer, out passwordFormat, out status);

            if (pass == null)
            {
                errorText = GetExceptionText(status);

                if (IsStatusDueToBadPassword(status))
                {
                    throw new MembershipPasswordException(errorText);
                }
                else
                {
                    throw new ProviderException(errorText);
                }
            }

            return UnEncodePassword(pass, passwordFormat);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            CheckParameter(ref username, true, true, true, 100, "username");
            CheckParameter(ref oldPassword, true, true, false, 100, "oldPassword");
            CheckParameter(ref newPassword, true, true, false, 100, "newPassword");

            string salt = null;
            int passwordFormat;
            int status;

            if (!CheckPassword(username, oldPassword, false, false, out salt, out passwordFormat))
            {
                return false;
            }

            if (newPassword.Length < MinRequiredPasswordLength)
            {
                throw new ArgumentException("The length of password is to short.");
            }

            int count = 0;
            for (int i = 0; i < newPassword.Length; i++)
            {
                if (!char.IsLetterOrDigit(newPassword, i))
                {
                    count++;
                }
            }

            if (count < MinRequiredNonAlphanumericCharacters)
            {
                throw new ArgumentException("Non alpha numeric characters in password needs to be greater than or equal to " + MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture) + ".");
            }

            if (PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(newPassword, PasswordStrengthRegularExpression))
                {
                    throw new ArgumentException("The password does not match the regular expression specified in config file.");
                }
            }

            string pass = EncodePassword(newPassword, (int)passwordFormat, salt);
            if (pass.Length > 100)
            {
                throw new ArgumentException("The password is too long: it must not exceed 100 chars after encrypting.", "newPassword");
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ArgumentException("The custom password validation failed.", "newPassword");
                }
            }
            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_SetPassword", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@NewPassword", FbDbType.VarChar, 100).Value = pass;
                    cmd.Parameters.Add("@PasswordSalt", FbDbType.VarChar, 100).Value = salt;
                    cmd.Parameters.Add("@PasswordFormat", FbDbType.Integer).Value = passwordFormat;
                    FbParameter p = new FbParameter("@ReturnValue", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    status = ((p.Value != null) ? ((int)p.Value) : -1);
                    if (status != 0)
                    {
                        string errorText = GetExceptionText(status);

                        if (IsStatusDueToBadPassword(status))
                        {
                            throw new MembershipPasswordException(errorText);
                        }
                        else
                        {
                            throw new ProviderException(errorText);
                        }
                    }
                }
            }

            return true;
        }

        public override string ResetPassword(string username, string passwordAnswer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("This provider is not configured to allow password resets. To enable password reset, set enablePasswordReset to \"true\" in the configuration file.");
            }

            CheckParameter(ref username, true, true, true, 100, "username");

            string salt;
            int passwordFormat;
            string passwdFromDB;
            int status;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            bool isApproved;
            DateTime lastLoginDate;
            DateTime lastActivityDate;

            GetPasswordWithFormat(username, false, out status, out passwdFromDB, out passwordFormat,
                out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount,
                out isApproved, out lastLoginDate, out lastActivityDate);
            if (status != 0)
            {
                if (IsStatusDueToBadPassword(status))
                {
                    throw new MembershipPasswordException(GetExceptionText(status));
                }
                else
                {
                    throw new ProviderException(GetExceptionText(status));
                }
            }

            string encodedPasswordAnswer;
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (!string.IsNullOrEmpty(passwordAnswer))
            {
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = passwordAnswer;
            }
            CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 100, "passwordAnswer");
            string newPassword = GeneratePassword();

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ProviderException("The custom password validation failed.");
                }
            }
            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_ResetPassword", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@NewPassword", FbDbType.VarChar, 100).Value = EncodePassword(newPassword, (int)passwordFormat, salt);
                    cmd.Parameters.Add("@MaxInvalidPasswordAttempts", FbDbType.Integer).Value = MaxInvalidPasswordAttempts;
                    cmd.Parameters.Add("@PasswordAttemptWindow", FbDbType.Integer).Value = PasswordAttemptWindow;
                    cmd.Parameters.Add("@PasswordSalt", FbDbType.VarChar, 100).Value = salt;
                    cmd.Parameters.Add("@PasswordFormat", FbDbType.Integer).Value = (int)passwordFormat;
                    cmd.Parameters.Add("@RequiresQuestionAndAnswer", FbDbType.Integer).Value = RequiresQuestionAndAnswer ? 1 : 0;
                    cmd.Parameters.Add("@PasswordAnswer", FbDbType.VarChar, 100).Value = encodedPasswordAnswer;
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    status = ((p.Value != null) ? ((int)p.Value) : -1);

                    if (status != 0)
                    {
                        string errorText = GetExceptionText(status);

                        if (IsStatusDueToBadPassword(status))
                        {
                            throw new MembershipPasswordException(errorText);
                        }
                        else
                        {
                            throw new ProviderException(errorText);
                        }
                    }
                }
            }

            return newPassword;
        }

        public override void UpdateUser(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            string temp = user.UserName;
            CheckParameter(ref temp, true, true, true, 100, "username");
            temp = user.Email;
            CheckParameter(ref temp,
                RequiresUniqueEmail,
                RequiresUniqueEmail,
                false,
                100,
                "Email");
            user.Email = temp;
            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_UpdateUser", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = user.UserName;
                    cmd.Parameters.Add("@Email", FbDbType.VarChar, 100).Value = user.Email;
                    cmd.Parameters.Add("@Comment", FbDbType.VarChar, 100).Value = user.Comment;
                    cmd.Parameters.Add("@IsApproved", FbDbType.Integer).Value = user.IsApproved ? 1 : 0;
                    cmd.Parameters.Add("@LastLoginDate", FbDbType.TimeStamp).Value = user.LastLoginDate.ToUniversalTime();
                    cmd.Parameters.Add("@LastActivityDate", FbDbType.TimeStamp).Value = user.LastActivityDate.ToUniversalTime();
                    cmd.Parameters.Add("@UniqueEmail", FbDbType.Integer).Value = RequiresUniqueEmail ? 1 : 0;
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    int status = ((p.Value != null) ? ((int)p.Value) : -1);
                    if (status != 0)
                    {
                        throw new ProviderException(GetExceptionText(status));
                    }
                }
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            return ValidateParameter(ref username, true, true, true, 100) &&
                ValidateParameter(ref password, true, true, false, 100) &&
                CheckPassword(username, password, true, true);
        }

        public override bool UnlockUser(string username)
        {
            CheckParameter(ref username, true, true, true, 100, "username");
            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_UnlockUser", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();

                    int status = ((p.Value != null) ? ((int)p.Value) : -1);
                    return status == 0;
                }
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null)
            {
                throw new ArgumentNullException("providerUserKey");
            }

            if (!(providerUserKey is Guid))
            {
                throw new ArgumentException("The provider user key supplied is invalid.  It must be of type System.Guid.", "providerUserKey");
            }

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_GetUserByUserId", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserId", FbDbType.Guid).Value = providerUserKey;
                    cmd.Parameters.Add("@UpdateLastActivity", FbDbType.Integer).Value = userIsOnline;
                    using (FbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string email = GetNullableString(reader, 0);
                            string passwordQuestion = GetNullableString(reader, 1);
                            string comment = GetNullableString(reader, 2);
                            bool isApproved = GetNullableBool(reader, 3);
                            DateTime dtCreate = GetNullableDateTime(reader, 4).ToLocalTime();
                            DateTime dtLastLogin = GetNullableDateTime(reader, 5).ToLocalTime();
                            DateTime dtLastActivity = GetNullableDateTime(reader, 6).ToLocalTime();
                            DateTime dtLastPassChange = GetNullableDateTime(reader, 7).ToLocalTime();
                            string userName = GetNullableString(reader, 8);
                            bool isLockedOut = GetNullableBool(reader, 9);
                            DateTime dtLastLockoutDate = GetNullableDateTime(reader, 10).ToLocalTime();
                            return new MembershipUser(this.Name,
                                userName,
                                providerUserKey,
                                email,
                                passwordQuestion,
                                comment,
                                isApproved,
                                isLockedOut,
                                dtCreate,
                                dtLastLogin,
                                dtLastActivity,
                                dtLastPassChange,
                                dtLastLockoutDate);
                        }
                    }
                }
            }
            return null;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            CheckParameter(ref username, true, false, true, 100, "username");

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_GetUserByName", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@UpdateLastActivity", FbDbType.Integer).Value = userIsOnline;
                    using (FbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string email = GetNullableString(reader, 0);
                            string passwordQuestion = GetNullableString(reader, 1);
                            string comment = GetNullableString(reader, 2);
                            bool isApproved = GetNullableBool(reader, 3);
                            DateTime dtCreate = GetNullableDateTime(reader, 4).ToLocalTime();
                            DateTime dtLastLogin = GetNullableDateTime(reader, 5).ToLocalTime();
                            DateTime dtLastActivity = GetNullableDateTime(reader, 6).ToLocalTime();
                            DateTime dtLastPassChange = GetNullableDateTime(reader, 7).ToLocalTime();
                            object userId = reader.GetValue(8);
                            bool isLockedOut = GetNullableBool(reader, 9);
                            DateTime dtLastLockoutDate = GetNullableDateTime(reader, 10).ToLocalTime(); ;
                            return new MembershipUser(this.Name,
                                username,
                                userId,
                                email,
                                passwordQuestion,
                                comment,
                                isApproved,
                                isLockedOut,
                                dtCreate,
                                dtLastLogin,
                                dtLastActivity,
                                dtLastPassChange,
                                dtLastLockoutDate);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            CheckParameter(ref email, false, false, false, 100, "email");

            string username = null;

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_GetUserByEmail", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@Email", FbDbType.VarChar, 100).Value = email;

                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        if (reader.Read())
                        {
                            username = GetNullableString(reader, 0);
                            if (RequiresUniqueEmail && reader.Read())
                            {
                                throw new ProviderException("More than one user has the specified e-mail address.");
                            }
                        }
                    }
                }

                return username;
            }
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            CheckParameter(ref username, true, true, true, 100, "username");

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_DeleteUser", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@DeleteAllRelatedData", FbDbType.Integer).Value = deleteAllRelatedData ? 1 : 0;

                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();

                    int status = ((p.Value != null) ? ((int)p.Value) : -1);

                    return (status > 0);
                }
            }
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException("The pageIndex must be greater than or equal to zero.", "PageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("The pageSize must be greater than zero.", "pageSize");
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException("The combination of pageIndex and pageSize cannot exceed the maximum value of System.Int32.", "pageIndex and pageSize");
            }

            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = 0;

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_GetAllUsers", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@PageIndex", FbDbType.Integer).Value = pageIndex;
                    cmd.Parameters.Add("@PageSize", FbDbType.Integer).Value = pageSize;
                    using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        while (reader.Read())
                        {

                            string username, email, passwordQuestion, comment;
                            bool isApproved;
                            DateTime dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange;
                            object userId;
                            bool isLockedOut;
                            DateTime dtLastLockoutDate;

                            username = GetNullableString(reader, 0);
                            email = GetNullableString(reader, 1);
                            passwordQuestion = GetNullableString(reader, 2);
                            comment = GetNullableString(reader, 3);
                            isApproved = GetNullableBool(reader, 4);
                            dtCreate = GetNullableDateTime(reader, 5).ToLocalTime();
                            dtLastLogin = GetNullableDateTime(reader, 6).ToLocalTime();
                            dtLastActivity = GetNullableDateTime(reader, 7).ToLocalTime();
                            dtLastPassChange = GetNullableDateTime(reader, 8).ToLocalTime();
                            userId = reader.GetValue(9);
                            isLockedOut = GetNullableBool(reader, 10);
                            dtLastLockoutDate = GetNullableDateTime(reader, 11).ToLocalTime();
                            totalRecords = reader.GetInt32(12);
                            users.Add(new MembershipUser(this.Name,
                                username,
                                userId,
                                email,
                                passwordQuestion,
                                comment,
                                isApproved,
                                isLockedOut,
                                dtCreate,
                                dtLastLogin,
                                dtLastActivity,
                                dtLastPassChange,
                                dtLastLockoutDate));
                        }

                    }
                }
            }
            return users;
        }

        public override int GetNumberOfUsersOnline()
        {

            TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);
            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_GetUsersOnline", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@SinceLastInActive", FbDbType.TimeStamp).Value = compareTime;
                    
                    FbParameter p = new FbParameter("@NUMBERUSERS", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    
                    return (p.Value != null) ? ((int)p.Value) : -1;
                }
            }
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameter(ref usernameToMatch, true, true, false, 100, "usernameToMatch");

            if (pageIndex < 0)
            {
                throw new ArgumentException("The pageIndex must be greater than or equal to zero.", "PageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("The pageSize must be greater than zero.", "pageSize");
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException("The combination of pageIndex and pageSize cannot exceed the maximum value of System.Int32.", "pageIndex and pageSize");
            }


            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = 0;

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_FindUsersByName", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserNameToMatch", FbDbType.VarChar, 100).Value = usernameToMatch;
                    cmd.Parameters.Add("@PageIndex", FbDbType.Integer).Value = pageIndex;
                    cmd.Parameters.Add("@PageSize", FbDbType.Integer).Value = pageSize;
                    using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        while (reader.Read())
                        {
                            string username, email, passwordQuestion, comment;
                            bool isApproved;
                            DateTime dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange;
                            object userId;
                            bool isLockedOut;
                            DateTime dtLastLockoutDate;

                            username = GetNullableString(reader, 0);
                            email = GetNullableString(reader, 1);
                            passwordQuestion = GetNullableString(reader, 2);
                            comment = GetNullableString(reader, 3);
                            isApproved = GetNullableBool(reader, 4);
                            dtCreate = GetNullableDateTime(reader, 5).ToLocalTime();
                            dtLastLogin = GetNullableDateTime(reader, 6).ToLocalTime();
                            dtLastActivity = GetNullableDateTime(reader, 7).ToLocalTime();
                            dtLastPassChange = GetNullableDateTime(reader, 8).ToLocalTime();
                            userId = reader.GetValue(9);
                            isLockedOut = GetNullableBool(reader, 10);
                            dtLastLockoutDate = GetNullableDateTime(reader, 11).ToLocalTime();
                            totalRecords = reader.GetInt32(12);
                            users.Add(new MembershipUser(this.Name,
                                username,
                                userId,
                                email,
                                passwordQuestion,
                                comment,
                                isApproved,
                                isLockedOut,
                                dtCreate,
                                dtLastLogin,
                                dtLastActivity,
                                dtLastPassChange,
                                dtLastLockoutDate));
                        }
                    }
                }
            }
            return users;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameter(ref emailToMatch, false, false, false, 100, "emailToMatch");

            if (pageIndex < 0)
            {
                throw new ArgumentException("The pageIndex must be greater than or equal to zero.", "PageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("The pageSize must be greater than zero.", "pageSize");
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException("The combination of pageIndex and pageSize cannot exceed the maximum value of System.Int32.", "pageIndex and pageSize");
            }

            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_FindUsersByEmail", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@EmailToMatch", FbDbType.VarChar, 100).Value = emailToMatch;
                    cmd.Parameters.Add("@PageIndex", FbDbType.Integer).Value = pageIndex;
                    cmd.Parameters.Add("@PageSize", FbDbType.Integer).Value = pageSize;

                    using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        while (reader.Read())
                        {
                            string username, email, passwordQuestion, comment;
                            bool isApproved;
                            DateTime dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange;
                            object userId;
                            bool isLockedOut;
                            DateTime dtLastLockoutDate;

                            username = GetNullableString(reader, 0);
                            email = GetNullableString(reader, 1);
                            passwordQuestion = GetNullableString(reader, 2);
                            comment = GetNullableString(reader, 3);
                            isApproved = GetNullableBool(reader, 4);
                            dtCreate = GetNullableDateTime(reader, 5).ToLocalTime();
                            dtLastLogin = GetNullableDateTime(reader, 6).ToLocalTime();
                            dtLastActivity = GetNullableDateTime(reader, 7).ToLocalTime();
                            dtLastPassChange = GetNullableDateTime(reader, 8).ToLocalTime();
                            userId = reader.GetValue(9);
                            isLockedOut = GetNullableBool(reader, 10);
                            dtLastLockoutDate = GetNullableDateTime(reader, 11).ToLocalTime();
                            totalRecords = reader.GetInt32(12);
                            users.Add(new MembershipUser(this.Name,
                                username,
                                userId,
                                email,
                                passwordQuestion,
                                comment,
                                isApproved,
                                isLockedOut,
                                dtCreate,
                                dtLastLogin,
                                dtLastActivity,
                                dtLastPassChange,
                                dtLastLockoutDate));
                        }
                    }
                }
            }
            return users;
        }

        #endregion

        #region � Private Methods �

        private string GetConfigValue(string configValue, string defaultValue)
        {
            return string.IsNullOrEmpty(configValue) ? defaultValue : configValue;
        }

        private bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }

            param = param.Trim();
            return !((checkIfEmpty && param.Length < 1) ||
                (maxSize > 0 && param.Length > maxSize) ||
                (checkForCommas && param.Contains(",")));

        }

        private string GenerateSalt()
        {
            byte[] buf = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        private string EncodePassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0)
            {
                return pass;
            }

            byte[] bIn = Encoding.Unicode.GetBytes(pass);
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            byte[] bRet = null;

            Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
            if (passwordFormat == 1)
            {
                HashAlgorithm s = HashAlgorithm.Create(Membership.HashAlgorithmType);
                bRet = s.ComputeHash(bAll);
            }
            else
            {
                bRet = EncryptPassword(bAll);
            }

            return Convert.ToBase64String(bRet);
        }

        private string UnEncodePassword(string pass, int passwordFormat)
        {
            switch (passwordFormat)
            {
                case 0:
                    return pass;
                case 1:
                    throw new ProviderException("Hashed passwords cannot be decoded.");
                default:
                    byte[] bIn = Convert.FromBase64String(pass);
                    byte[] bRet = DecryptPassword(bIn);
                    if (bRet == null)
                    {
                        return null;
                    }
                    return Encoding.Unicode.GetString(bRet, 16, bRet.Length - 16);
            }
        }

        private void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
            else
            {
                param = param.Trim();
                if (checkIfEmpty && param.Length < 1)
                {
                    throw new ArgumentException("A parameter must be not empty.", paramName);
                }

                if (maxSize > 0 && param.Length > maxSize)
                {
                    throw new ArgumentException("Parameter is too long !", paramName);
                }

                if (checkForCommas && param.Contains(","))
                {
                    throw new ArgumentException("Parameter must not contain commas.", paramName);
                }
            }
        }

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved)
        {
            string salt;
            int passwordFormat;
            return CheckPassword(username, password, updateLastLoginActivityDate, failIfNotApproved,
                out salt, out passwordFormat);
        }

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out string salt, out int passwordFormat)
        {
            string passwdFromDB;
            int status;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            bool isPasswordCorrect;
            bool isApproved;
            DateTime lastLoginDate;
            DateTime lastActivityDate;

            GetPasswordWithFormat(username, updateLastLoginActivityDate, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount,
                                  out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (status != 0)
            {
                return false;
            }
            if (!isApproved && failIfNotApproved)
            {
                return false;
            }

            string encodedPasswd = EncodePassword(password, passwordFormat, salt);

            isPasswordCorrect = passwdFromDB.Equals(encodedPasswd);

            if (isPasswordCorrect &&
                failedPasswordAttemptCount == 0 &&
                failedPasswordAnswerAttemptCount == 0)
            {
                return true;
            }

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_UpdateUserInfo", con))
                {
                    DateTime dtNow = DateTime.UtcNow;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@IsPasswordCorrect", FbDbType.Integer).Value = isPasswordCorrect;
                    cmd.Parameters.Add("@UpdateLastLoginActivityDate", FbDbType.Integer).Value = updateLastLoginActivityDate;
                    cmd.Parameters.Add("@MaxInvalidPasswordAttempts", FbDbType.Integer).Value = MaxInvalidPasswordAttempts;
                    cmd.Parameters.Add("@PasswordAttemptWindow", FbDbType.Integer).Value = PasswordAttemptWindow;
                    cmd.Parameters.Add("@LastLoginDate", FbDbType.TimeStamp).Value = isPasswordCorrect ? dtNow : lastLoginDate;
                    cmd.Parameters.Add("@LastActivityDate", FbDbType.TimeStamp).Value = isPasswordCorrect ? dtNow : lastActivityDate;
                    
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();

                    status = ((p.Value != null) ? ((int)p.Value) : -1);
                }
            }

            return isPasswordCorrect;
        }

        private void GetPasswordWithFormat(string username,
                                           bool updateLastLoginActivityDate,
                                           out int status,
                                           out string password,
                                           out int passwordFormat,
                                           out string passwordSalt,
                                           out int failedPasswordAttemptCount,
                                           out int failedPasswordAnswerAttemptCount,
                                           out bool isApproved,
                                           out DateTime lastLoginDate,
                                           out DateTime lastActivityDate)
        {
            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_GetPasswordandFormat", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@UpdateLastLoginActivityDate", FbDbType.Integer).Value = updateLastLoginActivityDate;
                    using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        status = -1;
                        if (reader.Read())
                        {
                            password = reader.GetString(0);
                            passwordFormat = GetNullableInt(reader, 1);
                            passwordSalt = GetNullableString(reader, 2);
                            failedPasswordAttemptCount = GetNullableInt(reader, 3);
                            failedPasswordAnswerAttemptCount = GetNullableInt(reader, 4);
                            isApproved = GetNullableBool(reader, 5);
                            lastLoginDate = GetNullableDateTime(reader, 6);
                            lastActivityDate = GetNullableDateTime(reader, 7);
                        }
                        else
                        {
                            password = null;
                            passwordFormat = 0;
                            passwordSalt = null;
                            failedPasswordAttemptCount = 0;
                            failedPasswordAnswerAttemptCount = 0;
                            isApproved = false;
                            lastLoginDate = DateTime.UtcNow;
                            lastActivityDate = DateTime.UtcNow;
                        }
                    }
                }
            }

        }

        private string GetPasswordFromDB(string username,
                                          string passwordAnswer,
                                          bool requiresQuestionAndAnswer,
                                          out int passwordFormat,
                                          out int status)
        {
            string password;

            using (FbConnection con = new FbConnection(fbConnectionString))
            {
                con.Open();
                using (FbCommand cmd = new FbCommand("Membership_GetPassword", con))
                {
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 100).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@MaxInvalidPasswordAttempts", FbDbType.Integer).Value = MaxInvalidPasswordAttempts;
                    cmd.Parameters.Add("@PasswordAttemptWindow", FbDbType.Integer).Value = PasswordAttemptWindow;
                    cmd.Parameters.Add("@requiresQuestionAndAnswer", FbDbType.Integer).Value = requiresQuestionAndAnswer ? 1 : 0;
                    cmd.Parameters.Add("@PasswordAnswer", FbDbType.VarChar, 100).Value = passwordAnswer;
                    using (FbDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            password = reader.GetString(0);
                            passwordFormat = reader.GetInt32(1);
                        }
                        else
                        {
                            password = null;
                            passwordFormat = 0;
                        }
                        status = GetNullableInt(reader, 2);
                    }
                }
            }
            return password;
        }

        private string GetEncodedPasswordAnswer(string username, string passwordAnswer)
        {
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (string.IsNullOrEmpty(passwordAnswer))
            {
                return passwordAnswer;
            }

            int status;
            int passwordFormat;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            string password;
            string passwordSalt;
            bool isApproved;
            DateTime lastLoginDate, lastActivityDate;
            GetPasswordWithFormat(username, false, out status, out password, out passwordFormat, out passwordSalt,
                                  out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (status == 0)
            {
                return EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, passwordSalt);
            }
            else
            {
                throw new ProviderException(GetExceptionText(status));
            }
        }

        public virtual string GeneratePassword()
        {
            return Membership.GeneratePassword(
                      MinRequiredPasswordLength < PASSWORD_SIZE ? PASSWORD_SIZE : MinRequiredPasswordLength,
                      MinRequiredNonAlphanumericCharacters);
        }

        private string GetNullableString(FbDataReader reader, int col)
        {
            return reader.IsDBNull(col) == false ? reader.GetString(col) : null;
        }

        public int GetNullableInt(FbDataReader reader, int col)
        {
            return reader.IsDBNull(col) == false ? reader.GetInt32(col) : 0;
        }

        public bool GetNullableBool(FbDataReader reader, int col)
        {
            return reader.IsDBNull(col) == false ? reader.GetBoolean(col) : false;
        }

        public DateTime GetNullableDateTime(FbDataReader reader, int col)
        {
            return reader.IsDBNull(col) == false ? reader.GetDateTime(col) : DateTime.Parse("1/1/1900");
        }

        private string GetExceptionText(int status)
        {
            switch (status)
            {
                case 0:
                    return String.Empty;
                case 1:
                    return "The user was not found.";
                case 2:
                    return "The password supplied is wrong.";
                case 3:
                    return "The password-answer supplied is wrong.";
                case 4:
                    return "The password supplied is invalid.  Passwords must conform to the password strength requirements configured for the default provider.";
                case 5:
                    return "The password-question supplied is invalid.  Note that the current provider configuration requires a valid password question and answer.  As a result, a CreateUser overload that accepts question and answer parameters must also be used.";
                case 6:
                    return "The password-answer supplied is invalid.";
                case 7:
                    return "The E-mail supplied is invalid.";
                case 99:
                    return "The user account has been locked out.";
                default:
                    return "The Provider encountered an unknown error.";
            }
        }

        private bool IsStatusDueToBadPassword(int status)
        {
            return (status >= 2 && status <= 6) || status == 99;
        }

        private DateTime RoundToSeconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        #endregion
    }
}