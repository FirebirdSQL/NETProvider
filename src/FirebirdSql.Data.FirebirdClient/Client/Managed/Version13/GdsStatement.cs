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
using System.Collections;
using System.IO;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13;

internal class GdsStatement : Version12.GdsStatement
{
	const int STACKALLOC_LIMIT = 1024;

	#region Constructors

	public GdsStatement(GdsDatabase database)
		: base(database)
	{ }

	public GdsStatement(GdsDatabase database, Version10.GdsTransaction transaction)
		: base(database, transaction)
	{ }

	#endregion

	#region Overriden Methods

	protected override byte[] WriteParameters()
	{
		if (_parameters == null)
			return null;

		using (var ms = new MemoryStream(256))
		{
			try
			{
				var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);

				var count = _parameters.Count;
				var bytesLen = (int)Math.Ceiling(count / 8d);
				byte[] rented = null;
				Span<byte> buffer = bytesLen > STACKALLOC_LIMIT
					? (rented = ArrayPool<byte>.Shared.Rent(bytesLen)).AsSpan(0, bytesLen)
					: stackalloc byte[bytesLen];
				buffer.Clear();
				for (var i = 0; i < count; i++)
				{
					if (_parameters[i].DbValue.IsDBNull())
					{
						buffer[i / 8] |= (byte)(1 << (i % 8));
					}
				}
				xdr.WriteOpaque(buffer);
				if (rented != null)
				{
					ArrayPool<byte>.Shared.Return(rented);
				}

				for (var i = 0; i < _parameters.Count; i++)
				{
					var field = _parameters[i];
					if (field.DbValue.IsDBNull())
					{
						continue;
					}
					WriteRawParameter(xdr, field);
				}

				xdr.Flush();
				return ms.ToArray();
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}
	}
	protected override async ValueTask<byte[]> WriteParametersAsync(CancellationToken cancellationToken = default)
	{
		if (_parameters == null)
			return null;

		using (var ms = new MemoryStream(256))
		{
			try
			{
				var xdr = new XdrReaderWriter(new DataProviderStreamWrapper(ms), _database.Charset);

            var count = _parameters.Count;
            var len = (int)Math.Ceiling(count / 8d);
            var buffer = ArrayPool<byte>.Shared.Rent(len);
            Array.Clear(buffer, 0, len);
            for (var i = 0; i < count; i++)
            {
                if (_parameters[i].DbValue.IsDBNull())
                {
                    buffer[i / 8] |= (byte)(1 << (i % 8));
                }
            }
            try
            {
                await xdr.WriteOpaqueAsync(buffer, len, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

				for (var i = 0; i < _parameters.Count; i++)
				{
					var field = _parameters[i];
					if (field.DbValue.IsDBNull())
					{
						continue;
					}
					await WriteRawParameterAsync(xdr, field, cancellationToken).ConfigureAwait(false);
				}

				await xdr.FlushAsync(cancellationToken).ConfigureAwait(false);
				return ms.ToArray();
			}
			catch (IOException ex)
			{
				throw IscException.ForIOException(ex);
			}
		}
	}

	protected override DbValue[] ReadRow()
	{
		var row = _fields.Count > 0 ? new DbValue[_fields.Count] : Array.Empty<DbValue>();
		try
		{
			if (_fields.Count > 0)
			{
				var len = (int)Math.Ceiling(_fields.Count / 8d);
				var rented = ArrayPool<byte>.Shared.Rent(len);
				try
				{
					_database.Xdr.ReadOpaque(rented.AsSpan(0, len), len);
					for (var i = 0; i < _fields.Count; i++)
					{
						var isNull = (rented[i / 8] & (1 << (i % 8))) != 0;
						if (isNull)
						{
							row[i] = new DbValue(this, _fields[i], null);
						}
						else
						{
							var value = ReadRawValue(_database.Xdr, _fields[i]);
							row[i] = new DbValue(this, _fields[i], value);
						}
					}
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(rented);
				}
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		return row;
	}
	protected override async ValueTask<DbValue[]> ReadRowAsync(CancellationToken cancellationToken = default)
	{
		var row = _fields.Count > 0 ? new DbValue[_fields.Count] : Array.Empty<DbValue>();
		try
		{
			if (_fields.Count > 0)
			{
				var len = (int)Math.Ceiling(_fields.Count / 8d);
				var rented = ArrayPool<byte>.Shared.Rent(len);
				try
				{
					await _database.Xdr.ReadOpaqueAsync(rented.AsMemory(0, len), len, cancellationToken).ConfigureAwait(false);
					for (var i = 0; i < _fields.Count; i++)
					{
						var isNull = (rented[i / 8] & (1 << (i % 8))) != 0;
						if (isNull)
						{
							row[i] = new DbValue(this, _fields[i], null);
						}
						else
						{
							var value = await ReadRawValueAsync(_database.Xdr, _fields[i], cancellationToken).ConfigureAwait(false);
							row[i] = new DbValue(this, _fields[i], value);
						}
					}
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(rented);
				}
			}
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		return row;
	}

	#endregion
}
