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

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace FirebirdSql.Data.Logging;

public static class FbLogManager
{
	public static bool IsParameterLoggingEnabled { get; private set; } = false;

	private static ILoggerFactory LoggerFactory = NullLoggerFactory.Instance;

	public static void UseLoggerFactory(ILoggerFactory loggerFactory) =>
		LoggerFactory = loggerFactory;

	public static void EnableParameterLogging(bool enable = true) =>
		IsParameterLoggingEnabled = enable;

	internal static ILogger<T> CreateLogger<T>() =>
		LoggerFactory.CreateLogger<T>();
}
