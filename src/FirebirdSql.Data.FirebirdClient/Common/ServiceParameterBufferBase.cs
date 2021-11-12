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

namespace FirebirdSql.Data.Common
{
	internal abstract class ServiceParameterBufferBase : ParameterBuffer
	{
		private sealed class EmptyServiceParameterBuffer : ServiceParameterBufferBase
		{
			public override void AppendPreamble() { }

			public override void Append1(int type, byte[] value) => throw new NotImplementedException();
			public override void Append2(int type, byte[] value) => throw new NotImplementedException();
		}

		public static readonly ServiceParameterBufferBase Empty = new EmptyServiceParameterBuffer();

		public abstract void AppendPreamble();

		public abstract void Append1(int type, byte[] value);
		public abstract void Append2(int type, byte[] value);

		public void Append1(int type, string value) => Append1(type, value, Encoding2.Default);
		public void Append1(int type, string value, Encoding encoding) => Append1(type, encoding.GetBytes(value));

		public void Append2(int type, string value) => Append2(type, value, Encoding2.Default);
		public void Append2(int type, string value, Encoding encoding) => Append2(type, encoding.GetBytes(value));
	}
}
