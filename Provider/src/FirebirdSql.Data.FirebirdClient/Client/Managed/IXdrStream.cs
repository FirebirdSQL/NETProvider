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
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
{
	internal interface IXdrStream : IDisposable
	{
		long Position { get; }
		long Length { get; }
		bool IOFailed { get; }

		void SetCompression(Ionic.Zlib.ZlibCodec compressor, Ionic.Zlib.ZlibCodec decompressor);
		void SetEncryption(Org.BouncyCastle.Crypto.Engines.RC4Engine encryptor, Org.BouncyCastle.Crypto.Engines.RC4Engine decryptor);

		void Flush();

		byte[] ReadBytes(byte[] buffer, int count);
		Task<byte[]> ReadBytesAsync(byte[] buffer, int count);
		byte[] ReadOpaque(int length);
		byte[] ReadBuffer();
		string ReadString();
		string ReadString(int length);
		string ReadString(Charset charset);
		string ReadString(Charset charset, int length);
		short ReadInt16();
		int ReadInt32();
		Task<int> ReadInt32Async();
		long ReadInt64();
		Guid ReadGuid();
		float ReadSingle();
		double ReadDouble();
		DateTime ReadDateTime();
		DateTime ReadDate();
		TimeSpan ReadTime();
		decimal ReadDecimal(int type, int scale);
		bool ReadBoolean();
		IscException ReadStatusVector();

		void WriteBytes(byte[] buffer, int count);
		//Task WriteBytesAsync(byte[] buffer, int count);
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
		void WriteDate(DateTime value);
		void WriteTime(TimeSpan value);

		int ReadOperation();
		int ReadNextOperation();
		Task<int> ReadNextOperationAsync();
		void SetOperation(int operation);
	}
}
