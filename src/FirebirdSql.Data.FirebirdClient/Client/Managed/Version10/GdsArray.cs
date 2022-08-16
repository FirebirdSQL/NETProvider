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
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10;

internal sealed class GdsArray : ArrayBase
{
	const long ArrayHandle = 0;

	#region Fields

	private long _handle;
	private GdsDatabase _database;
	private GdsTransaction _transaction;

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
		set { _database = (GdsDatabase)value; }
	}

	public override TransactionBase Transaction
	{
		get { return _transaction; }
		set { _transaction = (GdsTransaction)value; }
	}

	#endregion

	#region Constructors

	public GdsArray(ArrayDesc descriptor)
		: base(descriptor)
	{ }

	public GdsArray(GdsDatabase database, GdsTransaction transaction, string tableName, string fieldName)
		: this(database, transaction, -1, tableName, fieldName)
	{ }

	public GdsArray(GdsDatabase database, GdsTransaction transaction, long handle, string tableName, string fieldName)
		: base(tableName, fieldName)
	{
		_database = database;
		_transaction = transaction;
		_handle = handle;
	}

	#endregion

	#region Methods

	public override byte[] GetSlice(int sliceLength)
	{
		try
		{
			var sdl = GenerateSDL(Descriptor);

			_database.Xdr.Write(IscCodes.op_get_slice);
			_database.Xdr.Write(_transaction.Handle);
			_database.Xdr.Write(_handle);
			_database.Xdr.Write(sliceLength);
			_database.Xdr.WriteBuffer(sdl);
			_database.Xdr.Write(string.Empty);
			_database.Xdr.Write(0);
			_database.Xdr.Flush();

			return ReceiveSliceResponse(Descriptor);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask<byte[]> GetSliceAsync(int sliceLength, CancellationToken cancellationToken = default)
	{
		try
		{
			var sdl = GenerateSDL(Descriptor);

			await _database.Xdr.WriteAsync(IscCodes.op_get_slice, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_transaction.Handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(sliceLength, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(sdl, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(string.Empty, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(0, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			return await ReceiveSliceResponseAsync(Descriptor, cancellationToken).ConfigureAwait(false);
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	public override void PutSlice(Array sourceArray, int sliceLength)
	{
		try
		{
			var sdl = GenerateSDL(Descriptor);
			var slice = EncodeSliceArray(sourceArray);

			_database.Xdr.Write(IscCodes.op_put_slice);
			_database.Xdr.Write(_transaction.Handle);
			_database.Xdr.Write(ArrayHandle);
			_database.Xdr.Write(sliceLength);
			_database.Xdr.WriteBuffer(sdl);
			_database.Xdr.Write(string.Empty);
			_database.Xdr.Write(sliceLength);
			_database.Xdr.WriteBytes(slice, slice.Length);
			_database.Xdr.Flush();

			var response = (GenericResponse)_database.ReadResponse();

			_handle = response.BlobId;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	public override async ValueTask PutSliceAsync(Array sourceArray, int sliceLength, CancellationToken cancellationToken = default)
	{
		try
		{
			var sdl = GenerateSDL(Descriptor);
			var slice = await EncodeSliceArrayAsync(sourceArray, cancellationToken).ConfigureAwait(false);

			await _database.Xdr.WriteAsync(IscCodes.op_put_slice, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(_transaction.Handle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(ArrayHandle, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(sliceLength, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBufferAsync(sdl, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(string.Empty, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteAsync(sliceLength, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.WriteBytesAsync(slice, slice.Length, cancellationToken).ConfigureAwait(false);
			await _database.Xdr.FlushAsync(cancellationToken).ConfigureAwait(false);

			var response = (GenericResponse)await _database.ReadResponseAsync(cancellationToken).ConfigureAwait(false);

			_handle = response.BlobId;
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	#endregion

	#region Protected Methods

	protected override Array DecodeSlice(byte[] slice)
	{
		var dbType = DbDataType.Array;
		Array sliceData = null;
		Array tempData = null;
		var systemType = GetSystemType();
		var lengths = new int[Descriptor.Dimensions];
		var lowerBounds = new int[Descriptor.Dimensions];
		var type = 0;
		var index = 0;

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
		tempData = Array.CreateInstance(systemType, sliceData.Length);

		type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
		dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, 0, Descriptor.Scale);

		using (var ms = new MemoryStream(slice))
		{
			var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);
			while (ms.Position < ms.Length)
			{
				switch (dbType)
				{
					case DbDataType.Char:
						tempData.SetValue(xdr.ReadString(Descriptor.Length), index);
						break;

					case DbDataType.VarChar:
						tempData.SetValue(xdr.ReadString(), index);
						break;

					case DbDataType.SmallInt:
						tempData.SetValue(xdr.ReadInt16(), index);
						break;

					case DbDataType.Integer:
						tempData.SetValue(xdr.ReadInt32(), index);
						break;

					case DbDataType.BigInt:
						tempData.SetValue(xdr.ReadInt64(), index);
						break;

					case DbDataType.Numeric:
					case DbDataType.Decimal:
						tempData.SetValue(xdr.ReadDecimal(type, Descriptor.Scale), index);
						break;

					case DbDataType.Float:
						tempData.SetValue(xdr.ReadSingle(), index);
						break;

					case DbDataType.Double:
						tempData.SetValue(xdr.ReadDouble(), index);
						break;

					case DbDataType.Date:
						tempData.SetValue(xdr.ReadDate(), index);
						break;

					case DbDataType.Time:
						tempData.SetValue(xdr.ReadTime(), index);
						break;

					case DbDataType.TimeStamp:
						tempData.SetValue(xdr.ReadDateTime(), index);
						break;
				}

				index++;
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
		}

		return sliceData;
	}
	protected override async ValueTask<Array> DecodeSliceAsync(byte[] slice, CancellationToken cancellationToken = default)
	{
		var dbType = DbDataType.Array;
		Array sliceData = null;
		Array tempData = null;
		var systemType = GetSystemType();
		var lengths = new int[Descriptor.Dimensions];
		var lowerBounds = new int[Descriptor.Dimensions];
		var type = 0;
		var index = 0;

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
		tempData = Array.CreateInstance(systemType, sliceData.Length);

		type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
		dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, 0, Descriptor.Scale);

		using (var ms = new MemoryStream(slice))
		{
			var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);
			while (ms.Position < ms.Length)
			{
				switch (dbType)
				{
					case DbDataType.Char:
						tempData.SetValue(await xdr.ReadStringAsync(Descriptor.Length, cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.VarChar:
						tempData.SetValue(await xdr.ReadStringAsync(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.SmallInt:
						tempData.SetValue(await xdr.ReadInt16Async(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.Integer:
						tempData.SetValue(await xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.BigInt:
						tempData.SetValue(await xdr.ReadInt64Async(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.Numeric:
					case DbDataType.Decimal:
						tempData.SetValue(await xdr.ReadDecimalAsync(type, Descriptor.Scale, cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.Float:
						tempData.SetValue(await xdr.ReadSingleAsync(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.Double:
						tempData.SetValue(await xdr.ReadDoubleAsync(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.Date:
						tempData.SetValue(await xdr.ReadDateAsync(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.Time:
						tempData.SetValue(await xdr.ReadTimeAsync(cancellationToken).ConfigureAwait(false), index);
						break;

					case DbDataType.TimeStamp:
						tempData.SetValue(await xdr.ReadDateTimeAsync(cancellationToken).ConfigureAwait(false), index);
						break;
				}

				index++;
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
		}

		return sliceData;
	}

	#endregion

	#region Private Methods

	private byte[] ReceiveSliceResponse(ArrayDesc desc)
	{
		try
		{
			var operation = _database.ReadOperation();
			if (operation == IscCodes.op_slice)
			{
				var isVariying = false;
				var elements = 0;
				var length = _database.Xdr.ReadInt32();

				length = _database.Xdr.ReadInt32();

				switch (desc.DataType)
				{
					case IscCodes.blr_text:
					case IscCodes.blr_text2:
					case IscCodes.blr_cstring:
					case IscCodes.blr_cstring2:
						elements = length / desc.Length;
						length += elements * ((4 - desc.Length) & 3);
						break;

					case IscCodes.blr_varying:
					case IscCodes.blr_varying2:
						elements = length / desc.Length;
						isVariying = true;
						break;

					case IscCodes.blr_short:
						length = length * desc.Length;
						break;
				}

				if (isVariying)
				{
					using (var ms = new MemoryStream())
					{
						var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms));
						for (var i = 0; i < elements; i++)
						{
							var buffer = _database.Xdr.ReadOpaque(_database.Xdr.ReadInt32());
							xdr.WriteBuffer(buffer, buffer.Length);
						}
						xdr.Flush();
						return ms.ToArray();
					}
				}
				else
				{
					return _database.Xdr.ReadOpaque(length);
				}
			}
			else
			{
				_database.ReadResponse(operation);
				return null;
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}
	private async ValueTask<byte[]> ReceiveSliceResponseAsync(ArrayDesc desc, CancellationToken cancellationToken = default)
	{
		try
		{
			var operation = await _database.ReadOperationAsync(cancellationToken).ConfigureAwait(false);
			if (operation == IscCodes.op_slice)
			{
				var isVariying = false;
				var elements = 0;
				var length = await _database.Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);

				length = await _database.Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false);

				switch (desc.DataType)
				{
					case IscCodes.blr_text:
					case IscCodes.blr_text2:
					case IscCodes.blr_cstring:
					case IscCodes.blr_cstring2:
						elements = length / desc.Length;
						length += elements * ((4 - desc.Length) & 3);
						break;

					case IscCodes.blr_varying:
					case IscCodes.blr_varying2:
						elements = length / desc.Length;
						isVariying = true;
						break;

					case IscCodes.blr_short:
						length = length * desc.Length;
						break;
				}

				if (isVariying)
				{
					using (var ms = new MemoryStream())
					{
						var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms));
						for (var i = 0; i < elements; i++)
						{
							var buffer = await _database.Xdr.ReadOpaqueAsync(await _database.Xdr.ReadInt32Async(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
							await xdr.WriteBufferAsync(buffer, buffer.Length, cancellationToken).ConfigureAwait(false);
						}
						await xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
						return ms.ToArray();
					}
				}
				else
				{
					return await _database.Xdr.ReadOpaqueAsync(length, cancellationToken).ConfigureAwait(false);
				}
			}
			else
			{
				await _database.ReadResponseAsync(operation, cancellationToken).ConfigureAwait(false);
				return null;
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
	}

	private byte[] EncodeSliceArray(Array sourceArray)
	{
		var dbType = DbDataType.Array;
		var charset = _database.Charset;
		var subType = (Descriptor.Scale < 0) ? 2 : 0;
		var type = 0;

		using (var ms = new MemoryStream())
		{
			var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);

			type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
			dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, subType, Descriptor.Scale);

			foreach (var source in sourceArray)
			{
				switch (dbType)
				{
					case DbDataType.Char:
						var buffer = charset.GetBytes(source.ToString());
						xdr.WriteOpaque(buffer, Descriptor.Length);
						break;

					case DbDataType.VarChar:
						xdr.Write((string)source);
						break;

					case DbDataType.SmallInt:
						xdr.Write((short)source);
						break;

					case DbDataType.Integer:
						xdr.Write((int)source);
						break;

					case DbDataType.BigInt:
						xdr.Write((long)source);
						break;

					case DbDataType.Decimal:
					case DbDataType.Numeric:
						xdr.Write((decimal)source, type, Descriptor.Scale);
						break;

					case DbDataType.Float:
						xdr.Write((float)source);
						break;

					case DbDataType.Double:
						xdr.Write((double)source);
						break;

					case DbDataType.Date:
						xdr.WriteDate(Convert.ToDateTime(source, CultureInfo.CurrentCulture.DateTimeFormat));
						break;

					case DbDataType.Time:
						xdr.WriteTime((TimeSpan)source);
						break;

					case DbDataType.TimeStamp:
						xdr.Write(Convert.ToDateTime(source, CultureInfo.CurrentCulture.DateTimeFormat));
						break;

					default:
						throw TypeHelper.InvalidDataType((int)dbType);
				}
			}

			xdr.Flush();
			return ms.ToArray();
		}
	}
	private async ValueTask<byte[]> EncodeSliceArrayAsync(Array sourceArray, CancellationToken cancellationToken = default)
	{
		var dbType = DbDataType.Array;
		var charset = _database.Charset;
		var subType = (Descriptor.Scale < 0) ? 2 : 0;
		var type = 0;

		using (var ms = new MemoryStream())
		{
			var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);

			type = TypeHelper.GetSqlTypeFromBlrType(Descriptor.DataType);
			dbType = TypeHelper.GetDbDataTypeFromBlrType(Descriptor.DataType, subType, Descriptor.Scale);

			foreach (var source in sourceArray)
			{
				switch (dbType)
				{
					case DbDataType.Char:
						var buffer = charset.GetBytes(source.ToString());
						await xdr.WriteOpaqueAsync(buffer, Descriptor.Length, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.VarChar:
						await xdr.WriteAsync((string)source, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.SmallInt:
						await xdr.WriteAsync((short)source, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.Integer:
						await xdr.WriteAsync((int)source, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.BigInt:
						await xdr.WriteAsync((long)source, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.Decimal:
					case DbDataType.Numeric:
						await xdr.WriteAsync((decimal)source, type, Descriptor.Scale, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.Float:
						await xdr.WriteAsync((float)source, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.Double:
						await xdr.WriteAsync((double)source, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.Date:
						await xdr.WriteDateAsync(Convert.ToDateTime(source, CultureInfo.CurrentCulture.DateTimeFormat), cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.Time:
						await xdr.WriteTimeAsync((TimeSpan)source, cancellationToken).ConfigureAwait(false);
						break;

					case DbDataType.TimeStamp:
						await xdr.WriteAsync(Convert.ToDateTime(source, CultureInfo.CurrentCulture.DateTimeFormat), cancellationToken).ConfigureAwait(false);
						break;

					default:
						throw TypeHelper.InvalidDataType((int)dbType);
				}
			}

			await xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
			return ms.ToArray();
		}
	}

	private byte[] GenerateSDL(ArrayDesc desc)
	{
		int n;
		int from;
		int to;
		int increment;
		int dimensions;
		ArrayBound tail;
		BinaryWriter sdl;

		dimensions = desc.Dimensions;

		if (dimensions > 16)
		{
			throw IscException.ForErrorCode(IscCodes.isc_invalid_dimension);
		}

		sdl = new BinaryWriter(new MemoryStream());
		Stuff(
			sdl, 4, IscCodes.isc_sdl_version1,
			IscCodes.isc_sdl_struct, 1, desc.DataType);

		switch (desc.DataType)
		{
			case IscCodes.blr_short:
			case IscCodes.blr_long:
			case IscCodes.blr_int64:
			case IscCodes.blr_quad:
				StuffSdl(sdl, (byte)desc.Scale);
				break;

			case IscCodes.blr_text:
			case IscCodes.blr_cstring:
			case IscCodes.blr_varying:
				StuffWord(sdl, desc.Length);
				break;

			default:
				break;
		}

		StuffString(sdl, IscCodes.isc_sdl_relation, desc.RelationName);
		StuffString(sdl, IscCodes.isc_sdl_field, desc.FieldName);

		if ((desc.Flags & IscCodes.ARRAY_DESC_COLUMN_MAJOR) == IscCodes.ARRAY_DESC_COLUMN_MAJOR)
		{
			from = dimensions - 1;
			to = -1;
			increment = -1;
		}
		else
		{
			from = 0;
			to = dimensions;
			increment = 1;
		}

		for (n = from; n != to; n += increment)
		{
			tail = desc.Bounds[n];
			if (tail.LowerBound == 1)
			{
				Stuff(sdl, 2, IscCodes.isc_sdl_do1, n);
			}
			else
			{
				Stuff(sdl, 2, IscCodes.isc_sdl_do2, n);

				StuffLiteral(sdl, tail.LowerBound);
			}

			StuffLiteral(sdl, tail.UpperBound);
		}

		Stuff(sdl, 5, IscCodes.isc_sdl_element, 1, IscCodes.isc_sdl_scalar, 0, dimensions);

		for (n = 0; n < dimensions; n++)
		{
			Stuff(sdl, 2, IscCodes.isc_sdl_variable, n);
		}

		StuffSdl(sdl, IscCodes.isc_sdl_eoc);

		return ((MemoryStream)sdl.BaseStream).ToArray();
	}

	private void Stuff(BinaryWriter sdl, short count, params object[] args)
	{
		for (var i = 0; i < count; i++)
		{
			sdl.Write(Convert.ToByte(args[i], CultureInfo.InvariantCulture));
		}
	}

	private void Stuff(BinaryWriter sdl, byte[] args)
	{
		sdl.Write(args);
	}

	private void StuffSdl(BinaryWriter sdl, byte sdl_byte)
	{
		Stuff(sdl, 1, sdl_byte);
	}

	private void StuffWord(BinaryWriter sdl, short word)
	{
		Stuff(sdl, BitConverter.GetBytes(word));
	}

	private void StuffLong(BinaryWriter sdl, int word)
	{
		Stuff(sdl, BitConverter.GetBytes(word));
	}

	private void StuffLiteral(BinaryWriter sdl, int literal)
	{
		if (literal >= -128 && literal <= 127)
		{
			Stuff(sdl, 2, IscCodes.isc_sdl_tiny_integer, literal);

			return;
		}

		if (literal >= -32768 && literal <= 32767)
		{
			StuffSdl(sdl, IscCodes.isc_sdl_short_integer);
			StuffWord(sdl, (short)literal);

			return;
		}

		StuffSdl(sdl, IscCodes.isc_sdl_long_integer);
		StuffLong(sdl, literal);
	}

	private void StuffString(BinaryWriter sdl, int constant, string value)
	{
		StuffSdl(sdl, (byte)constant);
		StuffSdl(sdl, (byte)value.Length);

		for (var i = 0; i < value.Length; i++)
		{
			StuffSdl(sdl, (byte)value[i]);
		}
	}

	#endregion
}
