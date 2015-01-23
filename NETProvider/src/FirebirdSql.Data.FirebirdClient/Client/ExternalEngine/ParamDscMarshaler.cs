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
 */

using System;
using System.Runtime.InteropServices;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.ExternalEngine
{
	internal sealed class ParamDscMarshaler
	{
		#region Constants

		public const int DSC_null           = 1;
		public const int DSC_no_subtype		= 2;	/* dsc has no sub type specified */
		public const int DSC_nullable       = 4;	/* not stored. instead, is derived
								                    from metadata primarily to flag
							 	                    SQLDA (in DSQL) */

		public const int dtype_null	        = 0;

		#endregion

		#region Static Fields

		private static readonly ParamDscMarshaler instance = new ParamDscMarshaler();

		#endregion

		#region Properties

		public static ParamDscMarshaler Instance
		{
			get { return ParamDscMarshaler.instance; }
		}

		#endregion

		#region Constructors

		private ParamDscMarshaler()
		{
		}

		#endregion

		#region Methods

		public void CleanUpNativeData(ref IntPtr pNativeData)
		{
			if (pNativeData != IntPtr.Zero)
			{
				// Destroy ParamDsc structure
				Marshal.DestroyStructure(pNativeData, typeof(ParamDsc));

				// Free	pointer	memory
				Marshal.FreeHGlobal(pNativeData);

				pNativeData = IntPtr.Zero;
			}
		}

		public IntPtr MarshalManagedToNative()
		{
			return this.MarshalManagedToNative(new ParamDsc());
		}

		public IntPtr MarshalManagedToNative(Charset charset, object value)
		{
			DbField field = new DbField();
			ParamDsc descriptor = this.BuildDescriptor(charset, value);
			DbDataType type = TypeHelper.GetTypeFromDsc(descriptor.Type, descriptor.Scale, descriptor.SubType);

			field.DataType          = (short)TypeHelper.GetFbType(type, true);
			field.SubType           = descriptor.SubType;
			field.DbValue.Value     = value;
			field.Length            = descriptor.Length;

			byte[] buffer = field.DbValue.GetBytes();

			descriptor.Data = Marshal.AllocHGlobal(buffer.Length);
			Marshal.Copy(buffer, 0, descriptor.Data, buffer.Length);

			return this.MarshalManagedToNative(descriptor);
		}

		public IntPtr MarshalManagedToNative(ParamDsc descriptor)
		{
			int size = Marshal.SizeOf(descriptor);
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(descriptor, ptr, true);

			return ptr;
		}

		public object MarshalNativeToManaged(Charset charset, IntPtr pNativeData)
		{
			// Obtain ParamDsc information
			ParamDsc descriptor = (ParamDsc)Marshal.PtrToStructure(pNativeData, typeof(ParamDsc));

			return this.GetValue(descriptor, charset);
		}

		#endregion

		#region Private Methods

		private object GetValue(ParamDsc descriptor, Charset charset)
		{
			DbField field = new DbField();

			if (descriptor.Type == dtype_null)
			{
				return null;
			}
			if (descriptor.Type == IscCodes.dtype_byte)
			{
				return null;
			}

			DbDataType dbType = TypeHelper.GetTypeFromDsc(descriptor.Type, descriptor.Scale, descriptor.SubType);

			field.DataType      = (short)TypeHelper.GetFbType(dbType, true);
			field.NumericScale  = descriptor.Scale;
			field.SubType       = descriptor.SubType;

			byte[] data = this.GetBytes(descriptor, field.DataType);

			field.SetValue(data);

			return field.Value;
		}

		private byte[] GetBytes(ParamDsc descriptor, int type)
		{
			if (descriptor.Length == 0 || descriptor.Data == IntPtr.Zero)
			{
				return null;
			}

			byte[] buffer = new byte[descriptor.Length];

			switch (type & ~1)
			{
				case IscCodes.SQL_VARYING:
					buffer = new byte[Marshal.ReadInt16(descriptor.Data)];
					IntPtr tmp = this.GetIntPtr(descriptor.Data, 2);
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
					Marshal.Copy(descriptor.Data, buffer, 0, buffer.Length);
					return buffer;

				default:
					throw new NotSupportedException("Unknown data type");
			}
		}

		private IntPtr GetIntPtr(IntPtr ptr, int offset)
		{
			return (IntPtr)(ptr.AsInt() + offset);
		}

		private ParamDsc BuildDescriptor(Charset charset, object value)
		{
			ParamDsc descriptor = new ParamDsc();

			if (value == null || value == DBNull.Value)
			{
				descriptor.Flags |= IscCodes.DSC_null;
			}
			else
			{
				this.SetDscType(ref descriptor, value);
			}

			if (descriptor.Type == IscCodes.dtype_cstring || descriptor.Type == IscCodes.dtype_varying)
			{
				descriptor.SubType = (short)charset.Identifier;
			}

			return descriptor;
		}

		private void SetDscType(ref ParamDsc descriptor, object value)
		{
			TypeCode code = Type.GetTypeCode(value.GetType());

			switch (code)
			{
				case TypeCode.Object:
					descriptor.Type = IscCodes.dtype_blob;
					descriptor.SubType = 0;
					break;

				case TypeCode.Char:
					descriptor.Type = IscCodes.dtype_cstring;
					descriptor.Length = (short)value.ToString().Length;
					break;

				case TypeCode.String:
					descriptor.Type = IscCodes.dtype_varying;
					descriptor.Length = (short)value.ToString().Length;
					break;

				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
					descriptor.Type = IscCodes.dtype_byte;
					descriptor.Length = 1;
					break;

				case TypeCode.Int16:
				case TypeCode.UInt16:
					descriptor.Type = IscCodes.dtype_short;
					descriptor.Length = 2;
					break;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					descriptor.Type = IscCodes.dtype_long;
					descriptor.Length = 4;
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					descriptor.Type = IscCodes.dtype_int64;
					descriptor.Length = 8;
					break;

				case TypeCode.Single:
					descriptor.Type = IscCodes.dtype_real;
					descriptor.Length = 4;
					break;

				case TypeCode.Double:
				case TypeCode.Decimal:
					descriptor.Type = IscCodes.dtype_double;
					descriptor.Length = 8;
					break;

				case TypeCode.DateTime:
					descriptor.Type = IscCodes.dtype_timestamp;
					descriptor.Length = 8;
					break;
			}
		}

		#endregion
	}
}
