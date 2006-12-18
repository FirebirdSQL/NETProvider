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
    public class FbMembershipProvider2 : MembershipProvider
    {
        #region � Fields �

        private string _fbConnectionString;
        private bool _EnablePasswordRetrieval;
        private bool _EnablePasswordReset;
        private bool _RequiresQuestionAndAnswer;
        private string _AppName;
        private bool _RequiresUniqueEmail;
        private int _MaxInvalidPasswordAttempts;
        private int _CommandTimeout;
        private int _PasswordAttemptWindow;
        private int _MinRequiredPasswordLength;
        private int _MinRequiredNonalphanumericCharacters;
        private string _PasswordStrengthRegularExpression;
        private MachineKeySection machineKey;
        private MembershipPasswordFormat _PasswordFormat;
        private const int PASSWORD_SIZE = 14;

        #endregion

        #region � Properties �

        public override bool EnablePasswordRetrieval 
        { 
            get { return _EnablePasswordRetrieval; } 
        }

        public override bool EnablePasswordReset 
        { 
            get { return _EnablePasswordReset; } 
        }

        public override bool RequiresQuestionAndAnswer 
        { 
            get { return _RequiresQuestionAndAnswer; } 
        }

        public override bool RequiresUniqueEmail                     
        { 
            get { return _RequiresUniqueEmail; }
        }

        public override MembershipPasswordFormat PasswordFormat 
        { 
            get { return _PasswordFormat; } 
        }
        public override int MaxInvalidPasswordAttempts 
        { 
            get { return _MaxInvalidPasswordAttempts; } 
        }

        public override int PasswordAttemptWindow 
        {
            get { return _PasswordAttemptWindow; } 
        }

        public override int MinRequiredPasswordLength
        {
            get { return _MinRequiredPasswordLength; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _MinRequiredNonalphanumericCharacters; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _PasswordStrengthRegularExpression; }
        }

        public override string ApplicationName
        {
            get { return _AppName; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                if (value.Length > 256)
                {
                    throw new ProviderException("The application name is too long.");
                }

                this._AppName = value;
            }
        }
        private int CommandTimeout
        {
            get { return _CommandTimeout; }
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
                config.Add("description", "Firebird New Membership provider");
            }
            base.Initialize(name, config);

            _EnablePasswordRetrieval =  Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "false"));
            _EnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            _RequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _RequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            _MaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _PasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _MinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _MinRequiredNonalphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonalphanumericCharacters"], "0"));
            _PasswordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if (_PasswordStrengthRegularExpression != null)
            {
                _PasswordStrengthRegularExpression = _PasswordStrengthRegularExpression.Trim();
                if (_PasswordStrengthRegularExpression.Length != 0)
                {
                    try
                    {
                        Regex regex = new Regex(_PasswordStrengthRegularExpression);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ProviderException(e.Message, e);
                    }
                }
            }
            else
            {
                _PasswordStrengthRegularExpression = string.Empty;
            }
            if (_MinRequiredNonalphanumericCharacters > _MinRequiredPasswordLength)
                throw new HttpException("The minRequiredNonalphanumericCharacters can not be greater than minRequiredPasswordLength.");

            _CommandTimeout = Convert.ToInt32(GetConfigValue(config["commandTimeout"], "30"));
            _AppName = config["applicationName"];
            if (string.IsNullOrEmpty(_AppName))
                _AppName = HostingEnvironment.ApplicationVirtualPath;

            if (_AppName.Length > 256)
            {
                throw new ProviderException("The application name is too long.");
            }

            string strTemp = config["passwordFormat"];
            if (strTemp == null)
                strTemp = "Hashed";

            switch (strTemp)
            {
                case "Clear":
                    _PasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                case "Encrypted":
                    _PasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Hashed":
                    _PasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                default:
                    throw new ProviderException("Password format specified is invalid.");
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

            _fbConnectionString = ConnectionStringSettings.ConnectionString;

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
                                                   out    MembershipCreateStatus status)
        {
            if (!ValidateParameter(ref password, true, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            string salt = GenerateSalt();
            string pass = EncodePassword(password, (int)_PasswordFormat, salt);
            if (pass.Length > 128)
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
                if (passwordAnswer.Length > 128)
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), (int)_PasswordFormat, salt);
            }
            else
                encodedPasswordAnswer = passwordAnswer;
            if (!ValidateParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            if (!ValidateParameter(ref username, true, true, true, 256))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            if (!ValidateParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 256))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if (!ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 256))
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
                providerUserKey = Guid.NewGuid();

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

            try
            {
                FbConnection con = null;
                DateTime dt = RoundToSeconds(DateTime.UtcNow);
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_CreateUser", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@Username", FbDbType.VarChar, 255).Value = username;
                    cmd.Parameters.Add("@Password", FbDbType.VarChar, 128).Value = pass;
                    cmd.Parameters.Add("@PasswordSalt", FbDbType.VarChar, 128).Value = salt;
                    cmd.Parameters.Add("@Email", FbDbType.VarChar, 128).Value = email;
                    cmd.Parameters.Add("@PasswordQuestion", FbDbType.VarChar, 255).Value = passwordQuestion;
                    cmd.Parameters.Add("@PasswordAnswer", FbDbType.VarChar, 255).Value = encodedPasswordAnswer;
                    cmd.Parameters.Add("@IsApproved", FbDbType.SmallInt).Value = isApproved;
                    cmd.Parameters.Add("@UniqueEmail", FbDbType.SmallInt).Value = RequiresUniqueEmail ? 1 : 0;
                    cmd.Parameters.Add("@PasswordFormat", FbDbType.Integer).Value = (int)PasswordFormat;
                    cmd.Parameters.Add("@userid", FbDbType.Guid).Value = providerUserKey;
                    FbParameter p = new FbParameter("@RETURNCODE",FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    int iStatus = ((p.Value != null) ? ((int)p.Value) : -1);
                    if (iStatus < 0 || iStatus > (int)MembershipCreateStatus.ProviderError)
                        iStatus = (int)MembershipCreateStatus.ProviderError;
                    status = (MembershipCreateStatus)iStatus;
                    if (iStatus != 0)
                        return null;
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
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            CheckParameter(ref username, true, true, true, 256, "username");
            CheckParameter(ref password, true, true, false, 128, "password");

            string salt;
            int passwordFormat;
            if (!CheckPassword(username, password, false, false, out salt, out passwordFormat))
                return false;
            CheckParameter(ref newPasswordQuestion, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 256, "newPasswordQuestion");
            string encodedPasswordAnswer;
            if (newPasswordAnswer != null)
            {
                newPasswordAnswer = newPasswordAnswer.Trim();
            }

            CheckParameter(ref newPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");
            if (!string.IsNullOrEmpty(newPasswordAnswer))
            {
                encodedPasswordAnswer = EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), (int)passwordFormat, salt);
            }
            else
                encodedPasswordAnswer = newPasswordAnswer;
            CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");

            try
            {
                FbConnection con = null;
                
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("MEMBERSHIP_PASSQUESTIONANSWER", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 255).Value = username;
                    cmd.Parameters.Add("@NewPasswordQuestion", FbDbType.VarChar,255).Value = newPasswordQuestion;
                    cmd.Parameters.Add("@NewPasswordAnswer", FbDbType.VarChar,255).Value = encodedPasswordAnswer;
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
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override string GetPassword(string username, string passwordAnswer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new NotSupportedException("This Membership Provider has not been configured to support password retrieval.");
            }
            CheckParameter(ref username, true, true, true, 256, "username");
            string encodedPasswordAnswer = GetEncodedPasswordAnswer(username, passwordAnswer);
            CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");

            string errText;
            int passwordFormat = 0;
            int status = 0;

            string pass = GetPasswordFromDB(username, encodedPasswordAnswer, RequiresQuestionAndAnswer, out passwordFormat, out status);

            if (pass == null)
            {
                errText = GetExceptionText(status);
                if (IsStatusDueToBadPassword(status))
                {
                    throw new MembershipPasswordException(errText);
                }
                else
                {
                    throw new ProviderException(errText);
                }
            }

            return UnEncodePassword(pass, passwordFormat);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            CheckParameter(ref username, true, true, true, 256, "username");
            CheckParameter(ref oldPassword, true, true, false, 128, "oldPassword");
            CheckParameter(ref newPassword, true, true, false, 128, "newPassword");

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
                throw new ArgumentException("Non alpha numeric characters in password needs to be greater than or equal to "+MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture)+".");
            }

            if (PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(newPassword, PasswordStrengthRegularExpression))
                {
                    throw new ArgumentException("The password does not match the regular expression specified in config file.");
                }
            }

            string pass = EncodePassword(newPassword, (int)passwordFormat, salt);
            if (pass.Length > 128)
            {
                throw new ArgumentException("The password is too long: it must not exceed 128 chars after encrypting.", "newPassword");
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
            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_SetPassword", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = username;
                    cmd.Parameters.Add("@NewPassword", FbDbType.VarChar,128).Value = pass;
                    cmd.Parameters.Add("@PasswordSalt", FbDbType.VarChar,128).Value = salt;
                    cmd.Parameters.Add("@PasswordFormat", FbDbType.Integer).Value = passwordFormat;
                    FbParameter p = new FbParameter("@ReturnValue", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    status = ((p.Value != null) ? ((int)p.Value) : -1);
                    if (status != 0)
                    {
                        string errText = GetExceptionText(status);

                        if (IsStatusDueToBadPassword(status))
                        {
                            throw new MembershipPasswordException(errText);
                        }
                        else
                        {
                            throw new ProviderException(errText);
                        }
                    }

                    return true;
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override string ResetPassword(string username, string passwordAnswer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("This provider is not configured to allow password resets. To enable password reset, set enablePasswordReset to \"true\" in the configuration file.");
            }

            CheckParameter(ref username, true, true, true, 256, "username");

            string salt;
            int passwordFormat;
            string passwdFromDB;
            int status;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            bool isApproved;
            DateTime lastLoginDate, lastActivityDate;

            GetPasswordWithFormat(username, false, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount,
                                  out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
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
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt);
            else
                encodedPasswordAnswer = passwordAnswer;
            CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");
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
            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_ResetPassword", con);
                    string errText;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = username;
                    cmd.Parameters.Add("@NewPassword", FbDbType.VarChar,128).Value = EncodePassword(newPassword, (int)passwordFormat, salt);
                    cmd.Parameters.Add("@MaxInvalidPasswordAttempts", FbDbType.Integer).Value = MaxInvalidPasswordAttempts;
                    cmd.Parameters.Add("@PasswordAttemptWindow", FbDbType.Integer).Value = PasswordAttemptWindow;
                    cmd.Parameters.Add("@PasswordSalt", FbDbType.VarChar,128).Value = salt;
                    cmd.Parameters.Add("@PasswordFormat", FbDbType.Integer).Value = (int)passwordFormat;
                    cmd.Parameters.Add("@RequiresQuestionAndAnswer", FbDbType.Integer).Value = RequiresQuestionAndAnswer ? 1 : 0; 
                    cmd.Parameters.Add("@PasswordAnswer", FbDbType.VarChar,255).Value = encodedPasswordAnswer;
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    status = ((p.Value != null) ? ((int)p.Value) : -1);

                    if (status != 0)
                    {
                        errText = GetExceptionText(status);

                        if (IsStatusDueToBadPassword(status))
                        {
                            throw new MembershipPasswordException(errText);
                        }
                        else
                        {
                            throw new ProviderException(errText);
                        }
                    }

                    return newPassword;
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            string temp = user.UserName;
            CheckParameter(ref temp, true, true, true, 256, "UserName");
            temp = user.Email;
            CheckParameter(ref temp,
                                       RequiresUniqueEmail,
                                       RequiresUniqueEmail,
                                       false,
                                       256,
                                       "Email");
            user.Email = temp;
            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_UpdateUser", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = user.UserName;
                    cmd.Parameters.Add("@Email", FbDbType.VarChar,255).Value = user.Email;
                    cmd.Parameters.Add("@Comment", FbDbType.VarChar,255).Value = user.Comment;
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
                        throw new ProviderException(GetExceptionText(status));
                    return;
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            if (ValidateParameter(ref username, true, true, true, 256) &&
                    ValidateParameter(ref password, true, true, false, 128) &&
                    CheckPassword(username, password, true, true))
                return true;
            else
                return false;
        }

        public override bool UnlockUser(string username)
        {
            CheckParameter(ref username, true, true, true, 256, "username");
            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_UnlockUser", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = username;
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();

                    int status = ((p.Value != null) ? ((int)p.Value) : -1);
                    if (status == 0)
                    {
                        return true;
                    }

                    return false;
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
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

            FbDataReader reader = null;

            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_GetUserByUserId", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserId", FbDbType.Guid).Value = providerUserKey;
                    cmd.Parameters.Add("@UpdateLastActivity", FbDbType.Integer).Value = userIsOnline;
                    reader = cmd.ExecuteReader();
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

                    return null;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader = null;
                    }

                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            CheckParameter(
                            ref username,
                            true,
                            false,
                            true,
                            256,
                            "username");

            FbDataReader reader = null;

            try
            {
                FbConnection con = null;
                try
                {
                    con  = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_GetUserByName", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = username;
                    cmd.Parameters.Add("@UpdateLastActivity", FbDbType.Integer).Value = userIsOnline;
                    reader = cmd.ExecuteReader();
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

                    return null;

                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader = null;
                    }

                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            CheckParameter(
                            ref email,
                            false,
                            false,
                            false,
                            256,
                            "email");


            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_GetUserByEmail", con);
                    string username = null;
                    FbDataReader reader = null;

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@Email", FbDbType.VarChar,255).Value = email;

                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    try
                    {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        if (reader.Read())
                        {
                            username = GetNullableString(reader, 0);
                            if (RequiresUniqueEmail && reader.Read())
                            {
                                throw new ProviderException("More than one user has the specified e-mail address.");
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                    return username;
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            CheckParameter(ref username, true, true, true, 256, "username");

            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_DeleteUser", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = username;
                    cmd.Parameters.Add("@deleteAllRelatedData",FbDbType.Integer).Value = deleteAllRelatedData?1:0;
                   
                    FbParameter p = new FbParameter("@RETURNCODE", FbDbType.Integer);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();

                    int status = ((p.Value != null) ? ((int)p.Value) : -1);

                    return (status > 0);
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
                throw new ArgumentException("The pageIndex must be greater than or equal to zero.","PageIndex");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize must be greater than zero.", "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
                throw new ArgumentException("The combination of pageIndex and pageSize cannot exceed the maximum value of System.Int32.", "pageIndex and pageSize");

            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = 0;
            try
            {
                FbConnection con = null;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_GetAllUsers", con);
                    FbDataReader reader = null;
                   
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@PageIndex", FbDbType.Integer).Value = pageIndex;
                    cmd.Parameters.Add("@PageSize", FbDbType.Integer).Value = pageSize;
                    try
                    {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
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
                            isApproved = GetNullableBool(reader,4);
                            dtCreate = GetNullableDateTime(reader,5).ToLocalTime();
                            dtLastLogin = GetNullableDateTime(reader, 6).ToLocalTime();
                            dtLastActivity = GetNullableDateTime(reader, 7).ToLocalTime();
                            dtLastPassChange = GetNullableDateTime(reader, 8).ToLocalTime();
                            userId = reader.GetValue(9);
                            isLockedOut = GetNullableBool(reader,10);
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
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return users;
        }

        public override int GetNumberOfUsersOnline()
        {

            try
            {
                FbConnection con = null;
                try
                {
                    TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
                    DateTime compareTime = DateTime.Now.Subtract(onlineSpan);
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_GetUsersOnline", con);
                    FbParameter p = new FbParameter("@NUMBERUSERS", FbDbType.Integer);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@SinceLastInActive", FbDbType.TimeStamp).Value = compareTime;
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    int num = ((p.Value != null) ? ((int)p.Value) : -1);
                    return num;
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameter(ref usernameToMatch, true, true, false, 256, "usernameToMatch");

            if (pageIndex < 0)
                throw new ArgumentException("The pageIndex must be greater than or equal to zero.", "PageIndex");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize must be greater than zero.", "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
                throw new ArgumentException("The combination of pageIndex and pageSize cannot exceed the maximum value of System.Int32.", "pageIndex and pageSize");

            try
            {
                FbConnection con = null;
                totalRecords = 0;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_FindUsersByName", con);
                    MembershipUserCollection users = new MembershipUserCollection();
                    FbDataReader reader = null;

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserNameToMatch", FbDbType.VarChar, 255).Value = usernameToMatch;
                    cmd.Parameters.Add("@PageIndex", FbDbType.Integer).Value = pageIndex;
                    cmd.Parameters.Add("@PageSize", FbDbType.Integer).Value = pageSize;
                    try
                    {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
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
                            isApproved = GetNullableBool(reader,4);
                            dtCreate = GetNullableDateTime(reader,5).ToLocalTime();
                            dtLastLogin = GetNullableDateTime(reader, 6).ToLocalTime();
                            dtLastActivity = GetNullableDateTime(reader, 7).ToLocalTime();
                            dtLastPassChange = GetNullableDateTime(reader, 8).ToLocalTime();
                            userId = reader.GetValue(9);
                            isLockedOut = GetNullableBool(reader,10);
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

                        return users;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
  
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameter(ref emailToMatch, false, false, false, 256, "emailToMatch");

            if (pageIndex < 0)
                throw new ArgumentException("The pageIndex must be greater than or equal to zero.", "PageIndex");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize must be greater than zero.", "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
                throw new ArgumentException("The combination of pageIndex and pageSize cannot exceed the maximum value of System.Int32.", "pageIndex and pageSize");

            try
            {
                FbConnection con = null;
                totalRecords = 0;
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_FindUsersByEmail", con);
                    MembershipUserCollection users = new MembershipUserCollection();
                    FbDataReader reader = null;

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@EmailToMatch", FbDbType.VarChar,128).Value = emailToMatch;
                    cmd.Parameters.Add("@PageIndex", FbDbType.Integer).Value = pageIndex;
                    cmd.Parameters.Add("@PageSize", FbDbType.Integer).Value = pageSize;
                    
                    try
                    {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
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

                        return users;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }


        #endregion

        #region � Private Methods �
        
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
            {
                return defaultValue;
            }

            return configValue;
        }

        private bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize) 
        {
            if (param == null) {
                return !checkForNull;
            }
            param = param.Trim();
            if ((checkIfEmpty && param.Length < 1) ||
                 (maxSize > 0 && param.Length > maxSize) ||
                 (checkForCommas && param.Contains(","))) {
                return false;
            }

            return true;
        }

        private string GenerateSalt()
        {
            byte[] buf = new byte[16];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        private string EncodePassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0)
                return pass;

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
                        return null;
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

                return;
            }

            param = param.Trim();
            if (checkIfEmpty && param.Length < 1)
            {
                throw new ArgumentException("A parameter  must not be empty.", paramName);
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

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved)
        {
            string salt;
            int passwordFormat;
            return CheckPassword(username, password, updateLastLoginActivityDate, failIfNotApproved, out salt, out passwordFormat);
        }

        private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out string salt, out int passwordFormat)
        {
            FbConnection con = null;
            string passwdFromDB;
            int status;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            bool isPasswordCorrect;
            bool isApproved;
            DateTime lastLoginDate, lastActivityDate;

            GetPasswordWithFormat(username, updateLastLoginActivityDate, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount,
                                  out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (status != 0)
                return false;
            if (!isApproved && failIfNotApproved)
                return false;

            string encodedPasswd = EncodePassword(password, passwordFormat, salt);

            isPasswordCorrect = passwdFromDB.Equals(encodedPasswd);

            if (isPasswordCorrect && failedPasswordAttemptCount == 0 && failedPasswordAnswerAttemptCount == 0)
                return true;

            try
            {
                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_UpdateUserInfo", con);
                    DateTime dtNow = DateTime.UtcNow;
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = username;
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
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
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
            try
            {
                FbConnection con = null;
                FbDataReader reader = null;

                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_GetPasswordandFormat", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar, 255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar, 255).Value = username;
                    cmd.Parameters.Add("@UpdateLastLoginActivityDate", FbDbType.Integer).Value = updateLastLoginActivityDate;
                    reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    status = -1;
                    if (reader.Read())
                    {
                        password = reader.GetString(0);
                        passwordFormat = GetNullableInt(reader,1);
                        passwordSalt = GetNullableString(reader,2);
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
                finally
                {
                    if (reader != null)
                    {
                        status = GetNullableInt(reader, 8);
                        reader.Close();
                        reader = null;
                    }

                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        private string GetPasswordFromDB(string username,
                                          string passwordAnswer,
                                          bool requiresQuestionAndAnswer,
                                          out int passwordFormat,
                                          out int status)
        {
            try
            {
                FbConnection con = null;
                FbDataReader reader = null;

                try
                {
                    con = new FbConnection(_fbConnectionString);
                    con.Open();
                    FbCommand cmd = new FbCommand("Membership_GetPassword", con);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ApplicationName", FbDbType.VarChar,255).Value = ApplicationName;
                    cmd.Parameters.Add("@UserName", FbDbType.VarChar,255).Value = username;
                    cmd.Parameters.Add("@MaxInvalidPasswordAttempts", FbDbType.Integer).Value = MaxInvalidPasswordAttempts;
                    cmd.Parameters.Add("@PasswordAttemptWindow", FbDbType.Integer).Value = PasswordAttemptWindow;
                    cmd.Parameters.Add("@requiresQuestionAndAnswer", FbDbType.Integer).Value = requiresQuestionAndAnswer ? 1 : 0;
                    cmd.Parameters.Add("@PasswordAnswer", FbDbType.VarChar,255).Value = passwordAnswer;
                    reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    string password = null;

                    status = -1;

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

                    return password;
                }
                finally
                {
                    if (reader != null)
                    {
                        status = GetNullableInt(reader, 2);
                        reader.Close();
                        reader = null;
                    }

                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        private string GetEncodedPasswordAnswer(string username, string passwordAnswer)
        {
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (string.IsNullOrEmpty(passwordAnswer))
                return passwordAnswer;
            int status, passwordFormat, failedPasswordAttemptCount, failedPasswordAnswerAttemptCount;
            string password, passwordSalt;
            bool isApproved;
            DateTime lastLoginDate, lastActivityDate;
            GetPasswordWithFormat(username, false, out status, out password, out passwordFormat, out passwordSalt,
                                  out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (status == 0)
                return EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, passwordSalt);
            else
                throw new ProviderException(GetExceptionText(status));
        }

        public virtual string GeneratePassword()
        {
            return Membership.GeneratePassword(
                      MinRequiredPasswordLength < PASSWORD_SIZE ? PASSWORD_SIZE : MinRequiredPasswordLength,
                      MinRequiredNonAlphanumericCharacters);
        }

        
        private string GetNullableString(FbDataReader reader, int col)
        {
            if (reader.IsDBNull(col) == false)
            {
                return reader.GetString(col);
            }

            return null;
        }

        public  int GetNullableInt(FbDataReader reader, int col)
        {

            if (reader.IsDBNull(col) == false)
            {
                return reader.GetInt32(col);
            }

            return 0;
        }

        public  bool GetNullableBool(FbDataReader reader, int col)
        {
            if (reader.IsDBNull(col) == false)
            {
                return reader.GetBoolean(col);
            }

            return false;
        }

        public  DateTime GetNullableDateTime(FbDataReader reader, int col)
        {
            if (reader.IsDBNull(col) == false)
            {
                return reader.GetDateTime(col);
            }
            return DateTime.Parse("1/1/1900");
        }
        
        private string GetExceptionText(int status)
        {
            string key;
            switch (status)
            {
                case 0:
                    return String.Empty;
                case 1:
                    key = "The user was not found.";
                    break;
                case 2:
                    key = "The password supplied is wrong.";
                    break;
                case 3:
                    key = "The password-answer supplied is wrong.";
                    break;
                case 4:
                    key = "The password supplied is invalid.  Passwords must conform to the password strength requirements configured for the default provider.";
                    break;
                case 5:
                    key = "The password-question supplied is invalid.  Note that the current provider configuration requires a valid password question and answer.  As a result, a CreateUser overload that accepts question and answer parameters must also be used.";
                    break;
                case 6:
                    key = "The password-answer supplied is invalid.";
                    break;
                case 7:
                    key = "The E-mail supplied is invalid.";
                    break;
                case 99:
                    key = "The user account has been locked out.";
                    break;
                default:
                    key = "The Provider encountered an unknown error.";
                    break;
            }
            return key;
        }

        private bool IsStatusDueToBadPassword(int status)
        {
            return (status >= 2 && status <= 6 || status == 99);
        }

        private DateTime RoundToSeconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        #endregion
    }
}