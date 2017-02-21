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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Client.Native.Handle;
using FirebirdSql.Data.Client.Native.Marshalers;
using System.Reflection;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesArray : ArrayBase
	{
		#region Fields

		private long _handle;
		private FesDatabase _db;
		private FesTransaction _transaction;
		private IntPtr[] _statusVector;

		#endregion

		#region Properties

		public override long Handle
		{
			get { return _handle; }
			set { _handle = value; }
		}

		public override IDatabase DB
		{
			get { return _db; }
			set { _db = (FesDatabase)value; }
		}

		public override TransactionBase Transaction
		{
			get { return _transaction; }
			set { _transaction = (FesTransaction)value; }
		}

		#endregion

		#region Constructors

		public FesArray(ArrayDesc descriptor)
			: base(descriptor)
		{
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		public FesArray(
			IDatabase db,
			TransactionBase transaction,
			string tableName,
			string fieldName)
			: this(db, transaction, -1, tableName, fieldName)
		{
		}

		public FesArray(
			IDatabase db,
			TransactionBase transaction,
			long handle,
			string tableName,
			string fieldName)
			: base(tableName, fieldName)
		{
			if (!(db is FesDatabase))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(FesDatabase)} type.");
			}
			if (!(transaction is FesTransaction))
			{
				throw new ArgumentException($"Specified argument is not of {nameof(FesTransaction)} type.");
			}
			_db = (FesDatabase)db;
			_transaction = (FesTransaction)transaction;
			_handle = handle;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

			LookupBounds();
		}

		#endregion

		#region Methods

		public override byte[] GetSlice(int sliceLength)
		{
			ClearStatusVector();

			DatabaseHandle dbHandle = _db.HandlePtr;
			TransactionHandle trHandle = _transaction.HandlePtr;

			IntPtr arrayDesc = ArrayDescMarshaler.MarshalManagedToNative(Descriptor);

			byte[] buffer = new byte[sliceLength];

			_db.FbClient.isc_array_get_slice(
				_statusVector,
				ref dbHandle,
				ref trHandle,
				ref _handle,
				arrayDesc,
				buffer,
				ref sliceLength);

			ArrayDescMarshaler.CleanUpNativeData(ref arrayDesc);

			_db.ProcessStatusVector(_statusVector);

			return buffer;
		}

		public override void PutSlice(Array sourceArray, int sliceLength)
		{
			ClearStatusVector();

			DatabaseHandle dbHandle = _db.HandlePtr;
			TransactionHandle trHandle = _transaction.HandlePtr;

			IntPtr arrayDesc = ArrayDescMarshaler.MarshalManagedToNative(Descriptor);

			Type systemType = GetSystemType();

			byte[] buffer = new byte[sliceLength];
#if NET40
			if (systemType.IsPrimitive)
#else
			if (systemType.GetTypeInfo().IsPrimitive)
#endif
			{
				Buffer.BlockCopy(sourceArray, 0, buffer, 0, buffer.Length);
			}
			else
			{
				buffer = EncodeSlice(Descriptor, sourceArray, sliceLength);
			}

			_db.FbClient.isc_array_put_slice(
				_statusVector,
				ref dbHandle,
				ref trHandle,
				ref _handle,
				arrayDesc,
				buffer,
				ref sliceLength);

			ArrayDescMarshaler.CleanUpNativeData(ref arrayDesc);

			_db.ProcessStatusVector(_statusVector);
		}

		#endregion

		#region Protected Methods

		protected override Array DecodeSlice(byte[] slice)
		{
			Array sliceData = null;
			int slicePosition = 0;
			int type = 0;
			DbDataType dbType = DbDataType.Array;
			Type systemType = GetSystemType();
			Charset charset = _db.Charset;
			int[] lengths = new int[Descriptor.Dimensions];
			int[] lowerBounds = new int[Descriptor.Dimensions];

			for (int i = 0; i < Descriptor.Dimensions; i++)
			{
				lowerBounds[i] = Descriptor.Bounds[i].LowerBound;
				lengths[i] = Descriptor.Bounds[i].UpperBound;

				if (lowerBounds[i] == 0)
				{
					lengths[i]++;
				}
			}

			sliceData = Array.CreateInstance(systemType, lengths, lowerBounds);

			Array tempData = Array.CreateInstance(systemType, sliceData.Length);

			type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
			dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, 0, Descriptor.Scale);

			int itemLength = Descriptor.Length;

			for (int i = 0; i < tempData.Length; i++)
			{
				if (slicePosition >= slice.Length)
				{
					break;
				}

				switch (dbType)
				{
					case DbDataType.Char:
						tempData.SetValue(charset.GetString(slice, slicePosition, itemLength), i);
						break;

					case DbDataType.VarChar:
						{
							int index = slicePosition;
							int count = 0;
							while (slice[index++] != 0)
							{
								count++;
							}
							tempData.SetValue(charset.GetString(slice, slicePosition, count), i);

							slicePosition += 2;
						}
						break;

					case DbDataType.SmallInt:
						tempData.SetValue(BitConverter.ToInt16(slice, slicePosition), i);
						break;

					case DbDataType.Integer:
						tempData.SetValue(BitConverter.ToInt32(slice, slicePosition), i);
						break;

					case DbDataType.BigInt:
						tempData.SetValue(BitConverter.ToInt64(slice, slicePosition), i);
						break;

					case DbDataType.Decimal:
					case DbDataType.Numeric:
						{
							object evalue = null;

							switch (type)
							{
								case IscCodes.SQL_SHORT:
									evalue = BitConverter.ToInt16(slice, slicePosition);
									break;

								case IscCodes.SQL_LONG:
									evalue = BitConverter.ToInt32(slice, slicePosition);
									break;

								case IscCodes.SQL_QUAD:
								case IscCodes.SQL_INT64:
									evalue = BitConverter.ToInt64(slice, slicePosition);
									break;
							}

							decimal dvalue = TypeDecoder.DecodeDecimal(evalue, Descriptor.Scale, type);

							tempData.SetValue(dvalue, i);
						}
						break;

					case DbDataType.Double:
						tempData.SetValue(BitConverter.ToDouble(slice, slicePosition), i);
						break;

					case DbDataType.Float:
						tempData.SetValue(BitConverter.ToSingle(slice, slicePosition), i);
						break;

					case DbDataType.Date:
						{
							int idate = BitConverter.ToInt32(slice, slicePosition);

							DateTime date = TypeDecoder.DecodeDate(idate);

							tempData.SetValue(date, i);
						}
						break;

					case DbDataType.Time:
						{
							int itime = BitConverter.ToInt32(slice, slicePosition);

							TimeSpan time = TypeDecoder.DecodeTime(itime);

							tempData.SetValue(time, i);
						}
						break;

					case DbDataType.TimeStamp:
						{
							int idate = BitConverter.ToInt32(slice, slicePosition);
							int itime = BitConverter.ToInt32(slice, slicePosition + 4);

							DateTime date = TypeDecoder.DecodeDate(idate);
							TimeSpan time = TypeDecoder.DecodeTime(itime);

							DateTime timestamp = date.Add(time);

							tempData.SetValue(timestamp, i);
						}
						break;
				}

				slicePosition += itemLength;
			}

#if NET40
			if (systemType.IsPrimitive)
#else
			if (systemType.GetTypeInfo().IsPrimitive)
#endif
			{
				// For primitive types we can use System.Buffer	to copy	generated data to destination array
				Buffer.BlockCopy(tempData, 0, sliceData, 0, Buffer.ByteLength(tempData));
			}
			else
			{
				sliceData = tempData;
			}

			return sliceData;
		}

		#endregion

		#region Private Metods

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		private byte[] EncodeSlice(ArrayDesc desc, Array sourceArray, int length)
		{
			BinaryWriter writer = new BinaryWriter(new MemoryStream());
			Charset charset = _db.Charset;
			DbDataType dbType = DbDataType.Array;
			int subType = (Descriptor.Scale < 0) ? 2 : 0;
			int type = 0;

			type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
			dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, subType, Descriptor.Scale);

			foreach (object source in sourceArray)
			{
				switch (dbType)
				{
					case DbDataType.Char:
						{
							string value = source != null ? (string)source : string.Empty;
							byte[] buffer = charset.GetBytes(value);

							writer.Write(buffer);

							if (desc.Length > buffer.Length)
							{
								for (int j = buffer.Length; j < desc.Length; j++)
								{
									writer.Write((byte)32);
								}
							}
						}
						break;

					case DbDataType.VarChar:
						{
							string value = source != null ? (string)source : string.Empty;

							byte[] buffer = charset.GetBytes(value);
							writer.Write(buffer);

							if (desc.Length > buffer.Length)
							{
								for (int j = buffer.Length; j < desc.Length; j++)
								{
									writer.Write((byte)0);
								}
							}
							writer.Write((short)0);
						}
						break;

					case DbDataType.SmallInt:
						writer.Write((short)source);
						break;

					case DbDataType.Integer:
						writer.Write((int)source);
						break;

					case DbDataType.BigInt:
						writer.Write((long)source);
						break;

					case DbDataType.Float:
						writer.Write((float)source);
						break;

					case DbDataType.Double:
						writer.Write((double)source);
						break;

					case DbDataType.Numeric:
					case DbDataType.Decimal:
						{
							object numeric = TypeEncoder.EncodeDecimal((decimal)source, desc.Scale, type);

							switch (type)
							{
								case IscCodes.SQL_SHORT:
									writer.Write((short)numeric);
									break;

								case IscCodes.SQL_LONG:
									writer.Write((int)numeric);
									break;

								case IscCodes.SQL_QUAD:
								case IscCodes.SQL_INT64:
									writer.Write((long)numeric);
									break;
							}
						}
						break;

					case DbDataType.Date:
						writer.Write(TypeEncoder.EncodeDate(Convert.ToDateTime(source, CultureInfo.CurrentCulture.DateTimeFormat)));
						break;

					case DbDataType.Time:
						writer.Write(TypeEncoder.EncodeTime((TimeSpan)source));
						break;

					case DbDataType.TimeStamp:
						var dt = Convert.ToDateTime(source, CultureInfo.CurrentCulture.DateTimeFormat);
						writer.Write(TypeEncoder.EncodeDate(dt));
						writer.Write(TypeEncoder.EncodeTime(TypeHelper.DateTimeToTimeSpan(dt)));
						break;

					default:
						throw TypeHelper.InvalidDataType((int)dbType);
				}
			}

			return ((MemoryStream)writer.BaseStream).ToArray();
		}

		#endregion
	}
}
