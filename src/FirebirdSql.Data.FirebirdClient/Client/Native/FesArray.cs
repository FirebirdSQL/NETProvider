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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Native.Marshalers;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native;

internal sealed class FesArray : ArrayBase
{
	#region Fields

	private long _handle;
	private FesDatabase _database;
	private FesTransaction _transaction;
	private IntPtr[] _statusVector;

	#endregion

	#region Properties

	public override long Handle
	{
		get { return _handle; }
		set { _handle = value; }
	}

	public override DatabaseBase Database
	{
		get { return _database; }
		set { _database = (FesDatabase)value; }
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

	public FesArray(FesDatabase database, FesTransaction transaction, string tableName, string fieldName)
		: this(database, transaction, -1, tableName, fieldName)
	{ }

	public FesArray(FesDatabase database, FesTransaction transaction, long handle, string tableName, string fieldName)
		: base(tableName, fieldName)
	{
		_database = database;
		_transaction = transaction;
		_handle = handle;
		_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
	}

	#endregion

	#region Methods

	public override byte[] GetSlice(int sliceLength)
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;
		var trHandle = _transaction.HandlePtr;

		var arrayDesc = ArrayDescMarshaler.MarshalManagedToNative(Descriptor);

		var buffer = new byte[sliceLength];

		_database.FbClient.isc_array_get_slice(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _handle,
			arrayDesc,
			buffer,
			ref sliceLength);

		ArrayDescMarshaler.CleanUpNativeData(ref arrayDesc);

		_database.ProcessStatusVector(_statusVector);

		return buffer;
	}
	public override ValueTask<byte[]> GetSliceAsync(int sliceLength, CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;
		var trHandle = _transaction.HandlePtr;

		var arrayDesc = ArrayDescMarshaler.MarshalManagedToNative(Descriptor);

		var buffer = new byte[sliceLength];

		_database.FbClient.isc_array_get_slice(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _handle,
			arrayDesc,
			buffer,
			ref sliceLength);

		ArrayDescMarshaler.CleanUpNativeData(ref arrayDesc);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask.FromResult(buffer);
	}

	public override void PutSlice(Array sourceArray, int sliceLength)
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;
		var trHandle = _transaction.HandlePtr;

		var arrayDesc = ArrayDescMarshaler.MarshalManagedToNative(Descriptor);

		var systemType = GetSystemType();

		var buffer = new byte[sliceLength];
		if (systemType.GetTypeInfo().IsPrimitive)
		{
			Buffer.BlockCopy(sourceArray, 0, buffer, 0, buffer.Length);
		}
		else
		{
			buffer = EncodeSlice(Descriptor, sourceArray, sliceLength);
		}

		_database.FbClient.isc_array_put_slice(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _handle,
			arrayDesc,
			buffer,
			ref sliceLength);

		ArrayDescMarshaler.CleanUpNativeData(ref arrayDesc);

		_database.ProcessStatusVector(_statusVector);
	}
	public override ValueTask PutSliceAsync(Array sourceArray, int sliceLength, CancellationToken cancellationToken = default)
	{
		ClearStatusVector();

		var dbHandle = _database.HandlePtr;
		var trHandle = _transaction.HandlePtr;

		var arrayDesc = ArrayDescMarshaler.MarshalManagedToNative(Descriptor);

		var systemType = GetSystemType();

		var buffer = new byte[sliceLength];
		if (systemType.GetTypeInfo().IsPrimitive)
		{
			Buffer.BlockCopy(sourceArray, 0, buffer, 0, buffer.Length);
		}
		else
		{
			buffer = EncodeSlice(Descriptor, sourceArray, sliceLength);
		}

		_database.FbClient.isc_array_put_slice(
			_statusVector,
			ref dbHandle,
			ref trHandle,
			ref _handle,
			arrayDesc,
			buffer,
			ref sliceLength);

		ArrayDescMarshaler.CleanUpNativeData(ref arrayDesc);

		_database.ProcessStatusVector(_statusVector);

		return ValueTask.CompletedTask;
	}

	#endregion

	#region Protected Methods

	protected override Array DecodeSlice(byte[] slice)
	{
		Array sliceData = null;
		var slicePosition = 0;
		var type = 0;
		var dbType = DbDataType.Array;
		var systemType = GetSystemType();
		var charset = _database.Charset;
		var lengths = new int[Descriptor.Dimensions];
		var lowerBounds = new int[Descriptor.Dimensions];

		for (var i = 0; i < Descriptor.Dimensions; i++)
		{
			lowerBounds[i] = Descriptor.Bounds[i].LowerBound;
			lengths[i] = Descriptor.Bounds[i].UpperBound;

			if (lowerBounds[i] == 0)
			{
				lengths[i]++;
			}
		}

		sliceData = Array.CreateInstance(systemType, lengths, lowerBounds);

		var tempData = Array.CreateInstance(systemType, sliceData.Length);

		type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
		dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, 0, Descriptor.Scale);

		int itemLength = Descriptor.Length;

		for (var i = 0; i < tempData.Length; i++)
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
						var index = slicePosition;
						var count = 0;
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

						var dvalue = TypeDecoder.DecodeDecimal(evalue, Descriptor.Scale, type);

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
						var idate = BitConverter.ToInt32(slice, slicePosition);

						var date = TypeDecoder.DecodeDate(idate);

						tempData.SetValue(date, i);
					}
					break;

				case DbDataType.Time:
					{
						var itime = BitConverter.ToInt32(slice, slicePosition);

						var time = TypeDecoder.DecodeTime(itime);

						tempData.SetValue(time, i);
					}
					break;

				case DbDataType.TimeStamp:
					{
						var idate = BitConverter.ToInt32(slice, slicePosition);
						var itime = BitConverter.ToInt32(slice, slicePosition + 4);

						var date = TypeDecoder.DecodeDate(idate);
						var time = TypeDecoder.DecodeTime(itime);

						var timestamp = date.Add(time);

						tempData.SetValue(timestamp, i);
					}
					break;
			}

			slicePosition += itemLength;
		}

		if (systemType.GetTypeInfo().IsPrimitive)
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
	protected override ValueTask<Array> DecodeSliceAsync(byte[] slice, CancellationToken cancellationToken = default)
	{
		Array sliceData = null;
		var slicePosition = 0;
		var type = 0;
		var dbType = DbDataType.Array;
		var systemType = GetSystemType();
		var charset = _database.Charset;
		var lengths = new int[Descriptor.Dimensions];
		var lowerBounds = new int[Descriptor.Dimensions];

		for (var i = 0; i < Descriptor.Dimensions; i++)
		{
			lowerBounds[i] = Descriptor.Bounds[i].LowerBound;
			lengths[i] = Descriptor.Bounds[i].UpperBound;

			if (lowerBounds[i] == 0)
			{
				lengths[i]++;
			}
		}

		sliceData = Array.CreateInstance(systemType, lengths, lowerBounds);

		var tempData = Array.CreateInstance(systemType, sliceData.Length);

		type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
		dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, 0, Descriptor.Scale);

		int itemLength = Descriptor.Length;

		for (var i = 0; i < tempData.Length; i++)
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
						var index = slicePosition;
						var count = 0;
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

						var dvalue = TypeDecoder.DecodeDecimal(evalue, Descriptor.Scale, type);

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
						var idate = BitConverter.ToInt32(slice, slicePosition);

						var date = TypeDecoder.DecodeDate(idate);

						tempData.SetValue(date, i);
					}
					break;

				case DbDataType.Time:
					{
						var itime = BitConverter.ToInt32(slice, slicePosition);

						var time = TypeDecoder.DecodeTime(itime);

						tempData.SetValue(time, i);
					}
					break;

				case DbDataType.TimeStamp:
					{
						var idate = BitConverter.ToInt32(slice, slicePosition);
						var itime = BitConverter.ToInt32(slice, slicePosition + 4);

						var date = TypeDecoder.DecodeDate(idate);
						var time = TypeDecoder.DecodeTime(itime);

						var timestamp = date.Add(time);

						tempData.SetValue(timestamp, i);
					}
					break;
			}

			slicePosition += itemLength;
		}

		if (systemType.GetTypeInfo().IsPrimitive)
		{
			// For primitive types we can use System.Buffer	to copy	generated data to destination array
			Buffer.BlockCopy(tempData, 0, sliceData, 0, Buffer.ByteLength(tempData));
		}
		else
		{
			sliceData = tempData;
		}

		return ValueTask.FromResult(sliceData);
	}

	#endregion

	#region Private Metods

	private void ClearStatusVector()
	{
		Array.Clear(_statusVector, 0, _statusVector.Length);
	}

	private byte[] EncodeSlice(ArrayDesc desc, Array sourceArray, int length)
	{
		using (var ms = new MemoryStream())
		{
			using (var writer = new BinaryWriter(ms))
			{
				var charset = _database.Charset;
				var dbType = DbDataType.Array;
				var subType = (Descriptor.Scale < 0) ? 2 : 0;
				var type = 0;

				type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
				dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, subType, Descriptor.Scale);

				foreach (var source in sourceArray)
				{
					switch (dbType)
					{
						case DbDataType.Char:
							{
								var value = source != null ? (string)source : string.Empty;
								var buffer = charset.GetBytes(value);

								writer.Write(buffer);

								if (desc.Length > buffer.Length)
								{
									for (var j = buffer.Length; j < desc.Length; j++)
									{
										writer.Write((byte)32);
									}
								}
							}
							break;

						case DbDataType.VarChar:
							{
								var value = source != null ? (string)source : string.Empty;

								var buffer = charset.GetBytes(value);
								writer.Write(buffer);

								if (desc.Length > buffer.Length)
								{
									for (var j = buffer.Length; j < desc.Length; j++)
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
								var numeric = TypeEncoder.EncodeDecimal((decimal)source, desc.Scale, type);

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
							writer.Write(TypeEncoder.EncodeTime(TypeHelper.DateTimeTimeToTimeSpan(dt)));
							break;

						default:
							throw TypeHelper.InvalidDataType((int)dbType);
					}
				}

				writer.Flush();
				return ms.ToArray();
			}
		}
	}

	#endregion
}
