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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 * Contributors:
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace FirebirdSql.Data.Common
{
	[Serializable]
	internal sealed class IscException : Exception
	{
		#region Fields
		private string _message;
		#endregion

		#region Properties

		public List<IscError> Errors { get; private set; }

		public override string Message
		{
			get { return _message; }
		}

		public int ErrorCode { get; private set; }

		public string SQLSTATE { get; private set; }

		public bool IsWarning
		{
			get
			{
				if (this.Errors.Count > 0)
				{
					return this.Errors[0].IsWarning;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Constructors

		public IscException()
			: base()
		{
			this.Errors = new List<IscError>();
		}

		public IscException(int errorCode)
			: this()
		{
			this.Errors.Add(new IscError(IscCodes.isc_arg_gds, errorCode));

			this.BuildExceptionData();
		}

		public IscException(IEnumerable<int> errorCodes)
			: this()
		{
			foreach (int errorCode in errorCodes)
			{
				this.Errors.Add(new IscError(IscCodes.isc_arg_gds, errorCode));
			}

			this.BuildExceptionData();
		}

		/// <param name="dummy">This parameter is here only to differentiate sqlState and strParam.</param>
		public IscException(string sqlState, int dummy)
			: this()
		{
			this.Errors.Add(new IscError(IscCodes.isc_arg_sql_state, sqlState));

			this.BuildExceptionData();
		}

		public IscException(string strParam)
			: this()
		{
			this.Errors.Add(new IscError(IscCodes.isc_arg_string, strParam));

			this.BuildExceptionData();
		}

		public IscException(int errorCode, int intParam)
			: this()
		{
			this.Errors.Add(new IscError(IscCodes.isc_arg_gds, errorCode));
			this.Errors.Add(new IscError(IscCodes.isc_arg_number, intParam));

			this.BuildExceptionData();
		}

		public IscException(int type, int errorCode, string strParam)
			: this()
		{
			this.Errors.Add(new IscError(type, errorCode));
			this.Errors.Add(new IscError(IscCodes.isc_arg_string, strParam));

			this.BuildExceptionData();
		}

		public IscException(int type, int errorCode, int intParam, string strParam)
			: this()
		{
			this.Errors.Add(new IscError(type, errorCode));
			this.Errors.Add(new IscError(IscCodes.isc_arg_number, intParam));
			this.Errors.Add(new IscError(IscCodes.isc_arg_string, strParam));

			this.BuildExceptionData();
		}

		internal IscException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			this.Errors = (List<IscError>)info.GetValue("errors", typeof(List<IscError>));
			this.ErrorCode = info.GetInt32("errorCode");
		}

		#endregion

		#region Public Methods

		public void BuildExceptionData()
		{
			this.BuildErrorCode();
			this.BuildSqlState();
			this.BuildExceptionMessage();
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("errors", this.Errors);
			info.AddValue("errorCode", this.ErrorCode);
		}

		public override string ToString()
		{
			return _message;
		}

		#endregion

		#region Private Methods

		private void BuildErrorCode()
		{
			this.ErrorCode = (this.Errors.Count != 0 ? this.Errors[0].ErrorCode : 0);
		}

		private void BuildSqlState()
		{
			IscError error = this.Errors.Find(e => e.Type == IscCodes.isc_arg_sql_state);
			// step #1, maybe we already have a SQLSTATE stuffed in the status vector
			if (error != null)
			{
				this.SQLSTATE = error.StrParam;
			}
			// step #2, see if we can find a mapping.
			else
			{
				this.SQLSTATE = GetValueOrDefault(SqlStateMapping.Values, this.ErrorCode, _ => string.Empty);
			}
		}

		private void BuildExceptionMessage()
		{
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < this.Errors.Count; i++)
			{
				if (this.Errors[i].Type == IscCodes.isc_arg_gds ||
					this.Errors[i].Type == IscCodes.isc_arg_warning)
				{
					int code = this.Errors[i].ErrorCode;
					string message = GetValueOrDefault(IscErrorMessages.Values, code, BuildDefaultErrorMessage);

					ArrayList param = new ArrayList();

					int index = i + 1;

					while (index < this.Errors.Count && this.Errors[index].IsArgument)
					{
						param.Add(this.Errors[index++].StrParam);
						i++;
					}

					object[] args = (object[])param.ToArray(typeof(object));

					try
					{
						if (code == IscCodes.isc_except)
						{
							// Custom exception	add	the	first argument as error	code
							this.ErrorCode = Convert.ToInt32(args[0], CultureInfo.InvariantCulture);
						}
						else if (code == IscCodes.isc_except2)
						{
							// Custom exception. Next Error should be the exception name.
							// And the next one the Exception message
						}
						else if (code == IscCodes.isc_stack_trace)
						{
							// The next error contains the PSQL Stack Trace
							if (builder.Length > 0)
							{
								builder.Append(Environment.NewLine);
							}
							builder.AppendFormat(CultureInfo.CurrentCulture, "{0}", args);
						}
						else
						{
							if (builder.Length > 0)
							{
								builder.Append(Environment.NewLine);
							}

							builder.AppendFormat(CultureInfo.CurrentCulture, message, args);
						}
					}
					catch
					{
						message = BuildDefaultErrorMessage(code);

						builder.AppendFormat(CultureInfo.CurrentCulture, message, args);
					}
				}
			}

			// Update error	collection only	with the main error
			IscError mainError = new IscError(this.ErrorCode);
			mainError.Message = builder.ToString();

			this.Errors.Add(mainError);

			// Update exception	message
			_message = builder.ToString();
		}

		private string BuildDefaultErrorMessage(int code)
		{
			return string.Format(CultureInfo.CurrentCulture, "No message for error code {0} found.", code);
		}

		#endregion

		#region Static Methods

		private static string GetValueOrDefault(IDictionary<int, string> dictionary, int key, Func<int, string> defaultValueFactory)
		{
			string result;
			if (!dictionary.TryGetValue(key, out result))
			{
				result = defaultValueFactory(key);
			}
			return result;
		}

		#endregion
	}
}
