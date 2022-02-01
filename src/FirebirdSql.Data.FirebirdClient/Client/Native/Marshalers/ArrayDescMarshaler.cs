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

using System;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native.Marshalers;

internal static class ArrayDescMarshaler
{
	public static void CleanUpNativeData(ref IntPtr pNativeData)
	{
		if (pNativeData != IntPtr.Zero)
		{
			Marshal.DestroyStructure<ArrayDescMarshal>(pNativeData);

			for (var i = 0; i < 16; i++)
			{
				Marshal.DestroyStructure<ArrayBoundMarshal>(pNativeData + ArrayDescMarshal.ComputeLength(i));
			}

			Marshal.FreeHGlobal(pNativeData);

			pNativeData = IntPtr.Zero;
		}
	}

	public static IntPtr MarshalManagedToNative(ArrayDesc descriptor)
	{
		var arrayDesc = new ArrayDescMarshal();

		arrayDesc.DataType = descriptor.DataType;
		arrayDesc.Scale = (byte)descriptor.Scale;
		arrayDesc.Length = descriptor.Length;
		arrayDesc.FieldName = descriptor.FieldName;
		arrayDesc.RelationName = descriptor.RelationName;
		arrayDesc.Dimensions = descriptor.Dimensions;
		arrayDesc.Flags = descriptor.Flags;

		var arrayBounds = new ArrayBoundMarshal[descriptor.Bounds.Length];

		for (var i = 0; i < descriptor.Dimensions; i++)
		{
			arrayBounds[i].LowerBound = (short)descriptor.Bounds[i].LowerBound;
			arrayBounds[i].UpperBound = (short)descriptor.Bounds[i].UpperBound;
		}

		return MarshalManagedToNative(arrayDesc, arrayBounds);
	}

	public static IntPtr MarshalManagedToNative(ArrayDescMarshal arrayDesc, ArrayBoundMarshal[] arrayBounds)
	{
		var size = ArrayDescMarshal.ComputeLength(arrayBounds.Length);
		var ptr = Marshal.AllocHGlobal(size);

		Marshal.StructureToPtr(arrayDesc, ptr, true);
		for (var i = 0; i < arrayBounds.Length; i++)
		{
			Marshal.StructureToPtr(arrayBounds[i], ptr + ArrayDescMarshal.ComputeLength(i), true);
		}

		return ptr;
	}
}
