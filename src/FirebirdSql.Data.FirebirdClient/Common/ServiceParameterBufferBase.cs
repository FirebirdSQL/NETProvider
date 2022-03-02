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
using System.Text;

namespace FirebirdSql.Data.Common;

internal abstract class ServiceParameterBufferBase : ParameterBuffer
{
	public ServiceParameterBufferBase(Encoding encoding)
	{
		Encoding = encoding;
	}

	public abstract void AppendPreamble();

	public abstract void Append1(int type, byte[] value);
	public abstract void Append2(int type, byte[] value);
	public abstract void Append(int type, byte value);
	public abstract void Append(int type, int value);

	public void Append1(int type, string value) => Append1(type, Encoding.GetBytes(value));

	public void Append2(int type, string value) => Append2(type, Encoding.GetBytes(value));

	public Encoding Encoding { get; }
}
