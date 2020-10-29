/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using System.Numerics;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Client.Managed
{
	interface IXdrWriter
	{
		void Flush();
		void WriteBytes(byte[] buffer, int count);
		void WriteOpaque(byte[] buffer);
		void WriteOpaque(byte[] buffer, int length);
		void WriteBuffer(byte[] buffer);
		void WriteBuffer(byte[] buffer, int length);
		void WriteBlobBuffer(byte[] buffer);
		void WriteTyped(int type, byte[] buffer);
		void Write(string value);
		void Write(short value);
		void Write(int value);
		void Write(long value);
		void Write(float value);
		void Write(double value);
		void Write(decimal value, int type, int scale);
		void Write(bool value);
		void Write(DateTime value);
		void Write(Guid value);
		void Write(FbDecFloat value, int size);
		void Write(BigInteger value);
		void WriteDate(DateTime value);
		void WriteTime(TimeSpan value);
	}
}
