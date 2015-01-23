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
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Runtime.InteropServices;
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Common
{
	internal sealed class XsqldaMarshaler
	{
		#region Static Fields

		private static readonly XsqldaMarshaler instance = new XsqldaMarshaler();

		#endregion

		#region Static Properties

		public static XsqldaMarshaler Instance
		{
			get { return XsqldaMarshaler.instance; }
		}

		#endregion

		#region Constructors

		private XsqldaMarshaler()
		{
		}

		#endregion

		#region Methods

		public void CleanUpNativeData(ref IntPtr pNativeData)
		{
			if (pNativeData != IntPtr.Zero)
			{
				// Obtain XSQLDA information
				XSQLDA xsqlda = new XSQLDA();

				xsqlda = (XSQLDA)Marshal.PtrToStructure(pNativeData, typeof(XSQLDA));

				// Destroy XSQLDA structure
				Marshal.DestroyStructure(pNativeData, typeof(XSQLDA));

				// Destroy XSQLVAR structures
				for (int i = 0; i < xsqlda.sqln; i++)
				{
					IntPtr ptr1 = this.GetIntPtr(pNativeData, this.ComputeLength(i));

					// Free	sqldata	and	sqlind pointers	if needed
					XSQLVAR sqlvar = (XSQLVAR)Marshal.PtrToStructure(ptr1, typeof(XSQLVAR));

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

					IntPtr ptr2 = this.GetIntPtr(pNativeData, this.ComputeLength(i));
					Marshal.DestroyStructure(ptr2, typeof(XSQLVAR));
				}

				// Free	pointer	memory
				Marshal.FreeHGlobal(pNativeData);

				pNativeData = IntPtr.Zero;
			}
		}

		public IntPtr MarshalManagedToNative(Charset charset, Descriptor descriptor)
		{
			// Set up XSQLDA structure
			XSQLDA xsqlda = new XSQLDA();

			xsqlda.version  = descriptor.Version;
			xsqlda.sqln     = descriptor.Count;
			xsqlda.sqld     = descriptor.ActualCount;

			XSQLVAR[] xsqlvar = new XSQLVAR[descriptor.Count];

			for (int i = 0; i < xsqlvar.Length; i++)
			{
				// Create a	new	XSQLVAR	structure and fill it
				xsqlvar[i] = new XSQLVAR();

				xsqlvar[i].sqltype      = descriptor[i].DataType;
				xsqlvar[i].sqlscale     = descriptor[i].NumericScale;
				xsqlvar[i].sqlsubtype   = descriptor[i].SubType;
				xsqlvar[i].sqllen       = descriptor[i].Length;

				// Create a	new	pointer	for	the	xsqlvar	data
				if (descriptor[i].HasDataType() && descriptor[i].DbDataType != DbDataType.Null)
				{
					byte[] buffer = descriptor[i].DbValue.GetBytes();
					xsqlvar[i].sqldata = Marshal.AllocHGlobal(buffer.Length);
					Marshal.Copy(buffer, 0, xsqlvar[i].sqldata, buffer.Length);
				}
				else
				{
					xsqlvar[i].sqldata = Marshal.AllocHGlobal(0);
				}

				// Create a	new	pointer	for	the	sqlind value
				xsqlvar[i].sqlind = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int16)));
				Marshal.WriteInt16(xsqlvar[i].sqlind, descriptor[i].NullFlag);

				// Name
				xsqlvar[i].sqlname = this.GetStringBuffer(charset, descriptor[i].Name);
				xsqlvar[i].sqlname_length = (short)descriptor[i].Name.Length;

				// Relation	Name
				xsqlvar[i].relname = this.GetStringBuffer(charset, descriptor[i].Relation);
				xsqlvar[i].relname_length = (short)descriptor[i].Relation.Length;

				// Owner name
				xsqlvar[i].ownername = this.GetStringBuffer(charset, descriptor[i].Owner);
				xsqlvar[i].ownername_length = (short)descriptor[i].Owner.Length;

				// Alias name
				xsqlvar[i].aliasname = this.GetStringBuffer(charset, descriptor[i].Alias);
				xsqlvar[i].aliasname_length = (short)descriptor[i].Alias.Length;
			}

			return this.MarshalManagedToNative(xsqlda, xsqlvar);
		}

		public IntPtr MarshalManagedToNative(XSQLDA xsqlda, XSQLVAR[] xsqlvar)
		{
			int size = this.ComputeLength(xsqlda.sqln);
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(xsqlda, ptr, true);

			for (int i = 0; i < xsqlvar.Length; i++)
			{
				int offset = this.ComputeLength(i);
				Marshal.StructureToPtr(xsqlvar[i], this.GetIntPtr(ptr, offset), true);
			}

			return ptr;
		}

		public Descriptor MarshalNativeToManaged(Charset charset, IntPtr pNativeData)
		{
			return this.MarshalNativeToManaged(charset, pNativeData, false);
		}

		public Descriptor MarshalNativeToManaged(Charset charset, IntPtr pNativeData, bool fetching)
		{
			// Obtain XSQLDA information
			XSQLDA xsqlda = new XSQLDA();

			xsqlda = (XSQLDA)Marshal.PtrToStructure(pNativeData, typeof(XSQLDA));

			// Create a	new	Descriptor
			Descriptor descriptor   = new Descriptor(xsqlda.sqln);
			descriptor.ActualCount  = xsqlda.sqld;

			// Obtain XSQLVAR members information
			XSQLVAR[] xsqlvar = new XSQLVAR[xsqlda.sqln];

			for (int i = 0; i < xsqlvar.Length; i++)
			{
				IntPtr ptr = this.GetIntPtr(pNativeData, this.ComputeLength(i));
				xsqlvar[i] = (XSQLVAR)Marshal.PtrToStructure(ptr, typeof(XSQLVAR));

				// Map XSQLVAR information to Descriptor
				descriptor[i].DataType      = xsqlvar[i].sqltype;
				descriptor[i].NumericScale  = xsqlvar[i].sqlscale;
				descriptor[i].SubType       = xsqlvar[i].sqlsubtype;
				descriptor[i].Length        = xsqlvar[i].sqllen;

				// Decode sqlind value
				if (xsqlvar[i].sqlind == IntPtr.Zero)
				{
					descriptor[i].NullFlag = 0;
				}
				else
				{
					descriptor[i].NullFlag = Marshal.ReadInt16(xsqlvar[i].sqlind);
				}

				// Set value
				if (fetching)
				{
					if (descriptor[i].NullFlag != -1)
					{
						descriptor[i].SetValue(this.GetBytes(xsqlvar[i]));
					}
				}

				descriptor[i].Name      = this.GetString(charset, xsqlvar[i].sqlname);
				descriptor[i].Relation  = this.GetString(charset, xsqlvar[i].relname);
				descriptor[i].Owner     = this.GetString(charset, xsqlvar[i].ownername);
				descriptor[i].Alias     = this.GetString(charset, xsqlvar[i].aliasname);
			}

			return descriptor;
		}

		#endregion

		#region Private Methods

		private IntPtr GetIntPtr(IntPtr ptr, int offset)
		{
			return new IntPtr(ptr.ToInt64() + offset);
		}

		private int ComputeLength(int n)
		{
			int length = (Marshal.SizeOf(typeof(XSQLDA)) + n * Marshal.SizeOf(typeof(XSQLVAR)));
			if (IntPtr.Size == 8)
				length += 4;
			return length;
		}

		private byte[] GetBytes(XSQLVAR xsqlvar)
		{
			byte[] buffer   = null;
			IntPtr tmp      = IntPtr.Zero;

			if (xsqlvar.sqllen == 0 || xsqlvar.sqldata == IntPtr.Zero)
			{
				return null;
			}

			switch (xsqlvar.sqltype & ~1)
			{
				case IscCodes.SQL_VARYING:
					buffer  = new byte[Marshal.ReadInt16(xsqlvar.sqldata)];
					tmp     = this.GetIntPtr(xsqlvar.sqldata, 2);

					Marshal.Copy(tmp, buffer, 0, buffer.Length);

					return buffer;

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
					buffer = new byte[xsqlvar.sqllen];
					Marshal.Copy(xsqlvar.sqldata, buffer, 0, buffer.Length);

					return buffer;

				default:
					throw new NotSupportedException("Unknown data type");
			}
		}

		private byte[] GetStringBuffer(Charset charset, string value)
		{
			byte[] buffer = new byte[32];

			charset.GetBytes(value, 0, value.Length, buffer, 0);

			return buffer;
		}

		private string GetString(Charset charset, byte[] buffer)
		{
			string value = charset.GetString(buffer);

			return value.TrimEnd('\0', ' ');
		}

		#endregion
	}
}
