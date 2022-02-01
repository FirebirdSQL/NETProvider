/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;

namespace FirebirdSql.Data.Logging;

public interface IFbLogger
{
	bool IsEnabled(FbLogLevel level);
	void Log(FbLogLevel level, string msg, Exception exception = null);
}

public static class IFbLoggerExtensions
{
	public static void Trace(this IFbLogger logger, string msg) => logger.Log(FbLogLevel.Trace, msg);
	public static void Debug(this IFbLogger logger, string msg) => logger.Log(FbLogLevel.Debug, msg);
	public static void Info(this IFbLogger logger, string msg) => logger.Log(FbLogLevel.Info, msg);
	public static void Warn(this IFbLogger logger, string msg) => logger.Log(FbLogLevel.Warn, msg);
	public static void Error(this IFbLogger logger, string msg) => logger.Log(FbLogLevel.Error, msg);
	public static void Fatal(this IFbLogger logger, string msg) => logger.Log(FbLogLevel.Fatal, msg);

	public static void Trace(this IFbLogger logger, string msg, Exception ex) => logger.Log(FbLogLevel.Trace, msg, ex);
	public static void Debug(this IFbLogger logger, string msg, Exception ex) => logger.Log(FbLogLevel.Debug, msg, ex);
	public static void Info(this IFbLogger logger, string msg, Exception ex) => logger.Log(FbLogLevel.Info, msg, ex);
	public static void Warn(this IFbLogger logger, string msg, Exception ex) => logger.Log(FbLogLevel.Warn, msg, ex);
	public static void Error(this IFbLogger logger, string msg, Exception ex) => logger.Log(FbLogLevel.Error, msg, ex);
	public static void Fatal(this IFbLogger logger, string msg, Exception ex) => logger.Log(FbLogLevel.Fatal, msg, ex);
}
