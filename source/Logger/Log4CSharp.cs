/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 * 
 *  This file was originally ported from JayBird <http://firebird.sourceforge.net/>
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FirebirdSql.Logging
{
	#region ENUMS

	/// <summary>
	/// Messages priorities
	/// </summary>
	internal enum Priority
	{
		DEBUG,
		WARN,		
		ERROR,
		INFO,
		FATAL
	}

	/// <summary>
	/// Log modes
	/// </summary>
	internal enum Mode
	{
		OVERWRITE,
		APPEND	
	}

	#endregion

	/// <summary>
	/// Allow creation of log files for th eoperations of te code.
	/// </summary>
	internal class Log4CSharp
	{		
		#region FIELDS

		/// <summary>
		/// Log file stream		
		/// </summary>		
		private static Stream streamFile = null;
		
		/// <summary>
		/// Listener
		/// </summary>
		private static TextWriterTraceListener debugListener = null;

		/// <summary>
		/// Class type field
		/// </summary>
		private Type classType;

		/// <summary>
		/// Debug messages logging enabled 
		/// </summary>
		private bool debugEnabled  = true;
		/// <summary>
		/// Warn messages logging enabled
		/// </summary>
		private bool warnEnabled   = true;
		/// <summary>
		/// Error messages logging enabled
		/// </summary>
		private bool errorEnabled  = true;
		/// <summary>
		/// Info messages logging enabled
		/// </summary>
		private bool infoEnabled   = true;
		/// <summary>
		/// Fatal messages logging enabled
		/// </summary>
		private bool fatalEnabled  = true;

		
		private static bool initialized = false;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Debug messages enabled property
		/// </summary>
		public bool DebugEnabled
		{
			get { return debugEnabled; }
			set { debugEnabled = value; }
		}

		/// <summary>
		/// Warn messages enabled property
		/// </summary>
		public bool WarnEnabled
		{
			get { return warnEnabled; }
			set { warnEnabled = value; }
		}

		/// <summary>
		/// Error messages enabled property
		/// </summary>
		public bool ErrorEnabled
		{
			get { return errorEnabled; }
			set { errorEnabled = value; }
		}

		/// <summary>
		/// Info messages enabled property
		/// </summary>
		public bool InfoEnabled
		{
			get { return infoEnabled; }
			set { infoEnabled = value; }
		}

		/// <summary>
		/// Fatal messages enabled property
		/// </summary>
		public bool FatalEnabled
		{
			get { return fatalEnabled; }
			set { fatalEnabled = value; }
		}
		
		/// <summary>
		/// Class type property
		/// </summary>
		public Type ClassType
		{
			get { return classType; }
			set { classType = value; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <summary>
		/// Initializes a new instance of Log4CSharp class
		/// </summary>
		/// <param name="type">Class type</param>
		/// <param name="fileName">Path to log file</param>
		/// <param name="mode">Access mode to the log file</param>
		public Log4CSharp(Type type, string fileName, Mode mode)
		{
			System.Diagnostics.Debug.Assert(type != null, "Log4CSharp: You need to indicate a Class Type.");
			System.Diagnostics.Debug.Assert(fileName != null, "Log4CSharp : You need to indicate a log file.");

			classType = type;

			if (!initialized)
			{
				try
				{
					switch (mode)
					{
						case Mode.OVERWRITE:
							// Overwrite file
							streamFile = File.Create(fileName);
							streamFile.Close();
							// Reopen it in APPEND/SHARED mode
							streamFile = File.Open(fileName,FileMode.Append,FileAccess.Write,FileShare.ReadWrite);
							break;
					
						case Mode.APPEND:
							streamFile = File.Open(fileName,FileMode.Append,FileAccess.Write,FileShare.ReadWrite);
							break;

						default:
							streamFile = File.Create(fileName);
							break;
					}
					debugListener = new TextWriterTraceListener(streamFile);

					Trace.Listeners.Add(debugListener);
				}
				catch (IOException e)
				{
					throw e;
				}
				finally
				{
					initialized = true;
				}
			}
		}

		#endregion 

		#region METHODS

		/// <summary>
		/// Generic Log method
		/// </summary>
		/// <param name="logType">Message type</param>
		/// <param name="obj">Object</param>
		public virtual void Log(Priority logType, object obj)
		{
			switch (logType)
			{
				case Priority.DEBUG:
					Debug("{0}", obj);
					break;

				case Priority.WARN:
					Warn("{0}", obj);
					break;

				case Priority.ERROR:
					Error("{0}", obj);
					break;

				case Priority.INFO:
					Info("{0}", obj);
					break;

				case Priority.FATAL:
					Fatal("{0}", obj);
					break;

				default:
					break;
			}
		}
		
		/// <summary>
		/// Generic Log method - With exception
		/// </summary>
		/// <param name="logType">Message type</param>
		/// <param name="obj">Object</param>
		/// <param name="ex">Exception</param>
		public virtual void Log(Priority logType, object obj, Exception ex)
		{
			switch (logType)
			{
				case Priority.DEBUG:
					DebugEx(obj, ex);
					break;

				case Priority.WARN:
					WarnEx(obj, ex);
					break;

				case Priority.ERROR:
					ErrorEx(obj, ex);
					break;

				case Priority.INFO:
					InfoEx(obj, ex);
					break;

				case Priority.FATAL:
					FatalEx(obj, ex);
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Generic Log method - With format
		/// </summary>
		/// <param name="logType">Message type</param>
		/// <param name="strFormat">String format</param>
		/// <param name="args">Parameter list</param>
		public virtual void Log(Priority logType, string strFormat, params object[] args)
		{
			switch (logType)
			{
				case Priority.DEBUG:
					Debug(strFormat, args);
					break;

				case Priority.WARN:
					Warn(strFormat, args);
					break;

				case Priority.ERROR:
					Error(strFormat, args);
					break;

				case Priority.INFO:
					Info(strFormat, args);
					break;

				case Priority.FATAL:
					Fatal(strFormat, args);
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Debug message - with format
		/// </summary>
		/// <param name="strFormat">String format</param>
		/// <param name="args">Parameter list</param>
		public virtual void Debug(string strFormat, params object[] args)
		{
			StringBuilder msgDebug = new StringBuilder();
			
			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (debugEnabled)
						{
							if ( this.GetParamCount(strFormat) != 0 )
							{
								msgDebug.Append(GetMessage(Priority.DEBUG, String.Empty));
								msgDebug.AppendFormat(strFormat, args);
							}
							else
							{
								msgDebug.Append(GetMessage(Priority.DEBUG, strFormat));
							}
						
							debugListener.WriteLine(msgDebug.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Debug Message - With exception
		/// </summary>
		/// <param name="obj">Object</param>
		/// <param name="ex">Exception</param>
		public virtual void DebugEx(object obj, Exception ex)
		{
			StringBuilder msgDebug = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (debugEnabled)
						{
							msgDebug.Append(GetMessage(Priority.DEBUG, obj, ex));

							debugListener.WriteLine(msgDebug.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}

		}


		/// <summary>
		/// Error Message - With Format
		/// </summary>
		/// <param name="strFormat">String format</param>
		/// <param name="args">Parameter list</param>
		public virtual void Error(string strFormat, params object[] args)
		{
			StringBuilder msgError = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (errorEnabled)
						{
							if (this.GetParamCount(strFormat) != 0)
							{
								msgError.AppendFormat(strFormat, args);					
							}
							else
							{
								msgError.Append(GetMessage(Priority.ERROR, strFormat));
							}

							debugListener.WriteLine(msgError.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Error message - With Exception
		/// </summary>
		/// <param name="obj">Object</param>
		/// <param name="ex">Exception</param>
		public virtual void ErrorEx(object obj, Exception ex)
		{
			StringBuilder msgError = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (errorEnabled)
						{
							msgError.Append(GetMessage(Priority.DEBUG, obj, ex));
					
							debugListener.WriteLine(msgError.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Information Message - With Format
		/// </summary>
		/// <param name="strFormat">String format</param>
		/// <param name="args">Parameter list</param>
		public virtual void Info(string strFormat, params object[] args)
		{
			StringBuilder msgInfo = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (infoEnabled)
						{
							if ( this.GetParamCount(strFormat) != 0 )
							{
								msgInfo.AppendFormat(strFormat, args);					
							}
							else
							{
								msgInfo.Append(GetMessage(Priority.INFO, strFormat));
							}
							debugListener.WriteLine(msgInfo.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Information message - With exception
		/// </summary>
		/// <param name="obj">Object</param>
		/// <param name="ex">Exception</param>
		public virtual void InfoEx(object obj, Exception ex)
		{
			StringBuilder msgInfo = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (infoEnabled)
						{
							msgInfo.Append(GetMessage(Priority.DEBUG, obj, ex));
					
							debugListener.WriteLine(msgInfo.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Warning message - With format
		/// </summary>
		/// <param name="strFormat">String format</param>
		/// <param name="args">Parameter list</param>
		public virtual void Warn(string strFormat, params object[] args)
		{
			StringBuilder msgWarn = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (warnEnabled)
						{
							if ( this.GetParamCount(strFormat) != 0 )
							{
								msgWarn.AppendFormat(strFormat, args);					
							}
							else
							{
								msgWarn.Append(GetMessage(Priority.WARN, strFormat));
							}
							debugListener.WriteLine(msgWarn.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Warning message - With Exception
		/// </summary>
		/// <param name="obj">Object</param>
		/// <param name="ex">Exception</param>
		public virtual void WarnEx(object obj, Exception ex)
		{
			StringBuilder msgWarn = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (warnEnabled)
						{
							msgWarn.Append(GetMessage(Priority.DEBUG, obj, ex));
					
							debugListener.WriteLine(msgWarn.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Fatal message - With format
		/// </summary>
		/// <param name="strFormat">String format</param>
		/// <param name="args">Parameter list</param>
		public virtual void Fatal(string strFormat, params object[] args)
		{
			StringBuilder msgFatal = new StringBuilder();

			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (fatalEnabled)
						{
							if ( this.GetParamCount(strFormat) != 0 )
							{
								msgFatal.AppendFormat(strFormat, args);					
							}
							else
							{
								msgFatal.Append(GetMessage(Priority.FATAL, strFormat));
							}
							debugListener.WriteLine(msgFatal.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Fatal message - with exception
		/// </summary>
		/// <param name="obj">Object</param>
		/// <param name="ex">Exception</param>
		public virtual void FatalEx(object obj, Exception ex)
		{
			StringBuilder msgFatal = new StringBuilder();
			
			try
			{
				lock (debugListener)
				{
					if (debugListener != null)
					{
						if (fatalEnabled)
						{
							msgFatal.Append(GetMessage(Priority.DEBUG, obj, ex));
					
							debugListener.WriteLine(msgFatal.ToString());
							debugListener.Flush();
						}
					}
				}
			}
			catch (Exception)
			{
				debugListener = null;
			}
		}

		/// <summary>
		/// Gives information about the ability of log certain type of messages		
		/// </summary>
		/// <param name="priority">Message priority</param>
		/// <returns>True or false</returns>
		public bool IsEnabledFor(Priority priority)
		{
			switch (priority)
			{
				case Priority.DEBUG:
					return debugEnabled;

				case Priority.WARN:
					return warnEnabled;

				case Priority.ERROR:
					return errorEnabled;					

				case Priority.INFO:
					return infoEnabled;					

				case Priority.FATAL:
					return fatalEnabled;

				default:
					return false;					
			}
		}

		/// <summary>
		/// Returns a string with a formmated message - Normal
		/// </summary>
		/// <param name="priority">Message priority</param>
		/// <param name="obj"></param>
		/// <returns>a string</returns>
		private string GetMessage(Priority priority, object obj)
		{
			StringBuilder msg = new StringBuilder();

			switch (priority)
			{
				case Priority.DEBUG:
					msg.Append("DEBUG ");
					break;

				case Priority.WARN:
					msg.Append("WARN ");
					break;

				case Priority.ERROR:
					msg.Append("ERROR ");
					break;

				case Priority.INFO:
					msg.Append("INFO ");
					break;

				case Priority.FATAL:
					msg.Append("FATAL ");
					break;

				default:
					break;
			}
			msg.Append("[" + System.DateTime.Now.ToString() + "] ");
			msg.Append("(" + classType.Name + ") ");
			msg.Append(obj.ToString());

			return msg.ToString();
		}

		/// <summary>
		/// Returns a string with a formmated message - With exception
		/// </summary>
		/// <param name="priority">Message priority</param>
		/// <param name="obj">Object</param>
		/// <param name="ex">Exception</param>
		/// <returns>a string</returns>
		private string GetMessage(Priority priority, object obj, Exception ex)
		{
			StringBuilder msg = new StringBuilder();

			msg.Append(GetMessage(priority,obj) + "\n");
			msg.Append("\t\t");
			msg.Append("EXCEPTION [InnerException]: " + ex.InnerException + "\n");
			msg.Append("\t\t\t\t");
			msg.Append("[Source]: " + ex.Source + "\n");
			msg.Append("\t\t\t\t");
			msg.Append("[StackTrace]: " + ex.StackTrace + "\n");
			
			return msg.ToString();
		}

		/// <summary>
		/// Returns the number of parameters for the message template.
		/// </summary>
		/// <returns></returns>
		public int GetParamCount(string strFormat) 
		{
			int count = 0;

			for (int i = 0; i < strFormat.Length; i++)
			{
				if (strFormat[i] == '{') 
				{
					count++;
				}
			}

			return count;
		}

		#endregion
	}
}
