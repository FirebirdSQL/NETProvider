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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System.Text;

namespace FirebirdSql.Data.Common;

internal sealed class EventParameterBuffer : ParameterBuffer
{
	public EventParameterBuffer(Encoding encoding)
	{
		Encoding = encoding;
	}

	public void Append(byte[] content, int actualCount)
	{
		WriteByte(content.Length);
		Write(content);
		Write(actualCount);
	}

	public void Append(string content, int actualCount)
	{
		Append(Encoding.GetBytes(content), actualCount);
	}

	public Encoding Encoding { get; }
}
