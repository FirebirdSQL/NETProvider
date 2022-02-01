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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net), Hennadii Zabula

using System;
using System.Runtime.InteropServices;
using System.IO;
using FirebirdSql.Data.Common;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Native.Marshalers;

internal static class XsqldaMarshaler
{
	private static int SizeOfXSQLDA = Marshal.SizeOf<XSQLDA>();
	private static int SizeOfXSQLVAR = Marshal.SizeOf<XSQLVAR>();

	public static void CleanUpNativeData(ref IntPtr pNativeData)
	{
		if (pNativeData != IntPtr.Zero)
		{
			var xsqlda = Marshal.PtrToStructure<XSQLDA>(pNativeData);

			Marshal.DestroyStructure<XSQLDA>(pNativeData);

			for (var i = 0; i < xsqlda.sqln; i++)
			{
				var ptr = GetIntPtr(pNativeData, ComputeLength(i));

				var sqlvar = new XSQLVAR();
				MarshalXSQLVARNativeToManaged(ptr, sqlvar, true);

				if (sqlvar.sqldata != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(sqlvar.sqldata);
					sqlvar.sqldata = IntPtr.Zero;
				}

				if (sqlvar.sqlind != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(sqlvar.sqlind);
					sqlvar.sqlind = IntPtr.Zero;
				}

				Marshal.DestroyStructure<XSQLVAR>(ptr);
			}

			Marshal.FreeHGlobal(pNativeData);

			pNativeData = IntPtr.Zero;
		}
	}

	public static IntPtr MarshalManagedToNative(Charset charset, Descriptor descriptor)
	{
		var xsqlda = new XSQLDA
		{
			version = descriptor.Version,
			sqln = descriptor.Count,
			sqld = descriptor.ActualCount
		};

		var xsqlvar = new XSQLVAR[descriptor.Count];

		for (var i = 0; i < xsqlvar.Length; i++)
		{
			xsqlvar[i] = new XSQLVAR
			{
				sqltype = descriptor[i].DataType,
				sqlscale = descriptor[i].NumericScale,
				sqlsubtype = descriptor[i].SubType,
				sqllen = descriptor[i].Length
			};


			if (descriptor[i].HasDataType() && descriptor[i].DbDataType != DbDataType.Null)
			{
				var buffer = descriptor[i].DbValue.GetBytes();
				xsqlvar[i].sqldata = Marshal.AllocHGlobal(buffer.Length);
				Marshal.Copy(buffer, 0, xsqlvar[i].sqldata, buffer.Length);
			}
			else
			{
				xsqlvar[i].sqldata = Marshal.AllocHGlobal(0);
			}

			xsqlvar[i].sqlind = Marshal.AllocHGlobal(Marshal.SizeOf<short>());
			Marshal.WriteInt16(xsqlvar[i].sqlind, descriptor[i].NullFlag);

			xsqlvar[i].sqlname = GetStringBuffer(charset, descriptor[i].Name);
			xsqlvar[i].sqlname_length = (short)descriptor[i].Name.Length;

			xsqlvar[i].relname = GetStringBuffer(charset, descriptor[i].Relation);
			xsqlvar[i].relname_length = (short)descriptor[i].Relation.Length;

			xsqlvar[i].ownername = GetStringBuffer(charset, descriptor[i].Owner);
			xsqlvar[i].ownername_length = (short)descriptor[i].Owner.Length;

			xsqlvar[i].aliasname = GetStringBuffer(charset, descriptor[i].Alias);
			xsqlvar[i].aliasname_length = (short)descriptor[i].Alias.Length;
		}

		return MarshalManagedToNative(xsqlda, xsqlvar);
	}

	public static IntPtr MarshalManagedToNative(XSQLDA xsqlda, XSQLVAR[] xsqlvar)
	{
		var size = ComputeLength(xsqlda.sqln);
		var ptr = Marshal.AllocHGlobal(size);

		Marshal.StructureToPtr(xsqlda, ptr, true);

		for (var i = 0; i < xsqlvar.Length; i++)
		{
			var offset = ComputeLength(i);
			Marshal.StructureToPtr(xsqlvar[i], GetIntPtr(ptr, offset), true);
		}

		return ptr;
	}

	public static Descriptor MarshalNativeToManaged(Charset charset, IntPtr pNativeData)
	{
		return MarshalNativeToManaged(charset, pNativeData, false);
	}

	public static Descriptor MarshalNativeToManaged(Charset charset, IntPtr pNativeData, bool fetching)
	{
		var xsqlda = Marshal.PtrToStructure<XSQLDA>(pNativeData);

		var descriptor = new Descriptor(xsqlda.sqln) { ActualCount = xsqlda.sqld };

		var xsqlvar = new XSQLVAR();
		for (var i = 0; i < xsqlda.sqln; i++)
		{
			var ptr = GetIntPtr(pNativeData, ComputeLength(i));
			MarshalXSQLVARNativeToManaged(ptr, xsqlvar);

			descriptor[i].DataType = xsqlvar.sqltype;
			descriptor[i].NumericScale = xsqlvar.sqlscale;
			descriptor[i].SubType = xsqlvar.sqlsubtype;
			descriptor[i].Length = xsqlvar.sqllen;

			descriptor[i].NullFlag = xsqlvar.sqlind == IntPtr.Zero
				? (short)0
				: Marshal.ReadInt16(xsqlvar.sqlind);

			if (fetching)
			{
				if (descriptor[i].NullFlag != -1)
				{
					descriptor[i].SetValue(GetBytes(xsqlvar));
				}
			}

			descriptor[i].Name = GetString(charset, xsqlvar.sqlname, xsqlvar.sqlname_length);
			descriptor[i].Relation = GetString(charset, xsqlvar.relname, xsqlvar.relname_length);
			descriptor[i].Owner = GetString(charset, xsqlvar.ownername, xsqlvar.ownername_length);
			descriptor[i].Alias = GetString(charset, xsqlvar.aliasname, xsqlvar.aliasname_length);
		}

		return descriptor;
	}

	private static void MarshalXSQLVARNativeToManaged(IntPtr ptr, XSQLVAR xsqlvar, bool onlyPointers = false)
	{
		unsafe
		{
			using (var reader = new BinaryReader(new UnmanagedMemoryStream((byte*)ptr.ToPointer(), SizeOfXSQLVAR)))
			{
				if (!onlyPointers) xsqlvar.sqltype = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				if (!onlyPointers) xsqlvar.sqlscale = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				if (!onlyPointers) xsqlvar.sqlsubtype = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				if (!onlyPointers) xsqlvar.sqllen = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				xsqlvar.sqldata = reader.ReadIntPtr();
				xsqlvar.sqlind = reader.ReadIntPtr();
				if (!onlyPointers) xsqlvar.sqlname_length = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				if (!onlyPointers) xsqlvar.sqlname = reader.ReadBytes(32); else reader.BaseStream.Position += 32;
				if (!onlyPointers) xsqlvar.relname_length = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				if (!onlyPointers) xsqlvar.relname = reader.ReadBytes(32); else reader.BaseStream.Position += 32;
				if (!onlyPointers) xsqlvar.ownername_length = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				if (!onlyPointers) xsqlvar.ownername = reader.ReadBytes(32); else reader.BaseStream.Position += 32;
				if (!onlyPointers) xsqlvar.aliasname_length = reader.ReadInt16(); else reader.BaseStream.Position += sizeof(short);
				if (!onlyPointers) xsqlvar.aliasname = reader.ReadBytes(32); else reader.BaseStream.Position += 32;
			}
		}
	}

	private static IntPtr GetIntPtr(IntPtr ptr, int offset)
	{
		return new IntPtr(ptr.ToInt64() + offset);
	}

	private static int ComputeLength(int n)
	{
		var length = (SizeOfXSQLDA + n * SizeOfXSQLVAR);
		if (IntPtr.Size == 8)
		{
			length += 4;
		}
		return length;
	}

	private static byte[] GetBytes(XSQLVAR xsqlvar)
	{
		if (xsqlvar.sqllen == 0 || xsqlvar.sqldata == IntPtr.Zero)
		{
			return null;
		}

		var type = xsqlvar.sqltype & ~1;
		switch (type)
		{
			case IscCodes.SQL_VARYING:
				{
					var buffer = new byte[Marshal.ReadInt16(xsqlvar.sqldata)];
					var tmp = GetIntPtr(xsqlvar.sqldata, 2);
					Marshal.Copy(tmp, buffer, 0, buffer.Length);
					return buffer;
				}
			case IscCodes.SQL_TEXT:
			case IscCodes.SQL_SHORT:
			case IscCodes.SQL_LONG:
			case IscCodes.SQL_FLOAT:
			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
			case IscCodes.SQL_BLOB:
			case IscCodes.SQL_ARRAY:
			case IscCodes.SQL_TIMESTAMP:
			case IscCodes.SQL_TYPE_TIME:
			case IscCodes.SQL_TYPE_DATE:
			case IscCodes.SQL_BOOLEAN:
			case IscCodes.SQL_TIMESTAMP_TZ:
			case IscCodes.SQL_TIMESTAMP_TZ_EX:
			case IscCodes.SQL_TIME_TZ:
			case IscCodes.SQL_TIME_TZ_EX:
			case IscCodes.SQL_DEC16:
			case IscCodes.SQL_DEC34:
			case IscCodes.SQL_INT128:
				{
					var buffer = new byte[xsqlvar.sqllen];
					Marshal.Copy(xsqlvar.sqldata, buffer, 0, buffer.Length);
					return buffer;
				}
			default:
				throw TypeHelper.InvalidDataType(type);
		}
	}

	private static byte[] GetStringBuffer(Charset charset, string value)
	{
		var buffer = new byte[32];
		charset.GetBytes(value, 0, value.Length, buffer, 0);
		return buffer;
	}

	private static string GetString(Charset charset, byte[] buffer)
	{
		var value = charset.GetString(buffer);
		return value.TrimEnd('\0', ' ');
	}

	private static string GetString(Charset charset, byte[] buffer, short bufferLength)
	{
		return charset.GetString(buffer, 0, bufferLength);
	}
}
