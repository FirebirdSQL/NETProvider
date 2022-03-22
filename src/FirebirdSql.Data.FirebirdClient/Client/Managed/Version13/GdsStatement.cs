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
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13;

internal class GdsStatement : Version12.GdsStatement
{
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

				var bits = new BitArray(_parameters.Count);
				for (var i = 0; i < _parameters.Count; i++)
				{
					var field = _parameters[i];
					bits.Set(i, field.DbValue.IsDBNull());
				}
				var buffer = new byte[(int)Math.Ceiling(_parameters.Count / 8d)];
				for (var i = 0; i < buffer.Length * 8; i++)
				{
					var index = i / 8;
					// LSB
					buffer[index] = (byte)((buffer[index] >> 1) | (bits.Length > i && bits[i] ? 1 << 7 : 0));
				}
				xdr.WriteOpaque(buffer);

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

				var bits = new BitArray(_parameters.Count);
				for (var i = 0; i < _parameters.Count; i++)
				{
					var field = _parameters[i];
					bits.Set(i, field.DbValue.IsDBNull());
				}
				var buffer = new byte[(int)Math.Ceiling(_parameters.Count / 8d)];
				for (var i = 0; i < buffer.Length * 8; i++)
				{
					var index = i / 8;
					// LSB
					buffer[index] = (byte)((buffer[index] >> 1) | (bits.Length > i && bits[i] ? 1 << 7 : 0));
				}
				await xdr.WriteOpaqueAsync(buffer, cancellationToken).ConfigureAwait(false);

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
		var row = new DbValue[_fields.Count];
		try
		{
			if (_fields.Count > 0)
			{
				var nullBytes = _database.Xdr.ReadOpaque((int)Math.Ceiling(_fields.Count / 8d));
				var nullBits = new BitArray(nullBytes);
				for (var i = 0; i < _fields.Count; i++)
				{
					if (nullBits.Get(i))
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
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		return row;
	}
	protected override async ValueTask<DbValue[]> ReadRowAsync(CancellationToken cancellationToken = default)
	{
		var row = new DbValue[_fields.Count];
		try
		{
			if (_fields.Count > 0)
			{
				var nullBytes = await _database.Xdr.ReadOpaqueAsync((int)Math.Ceiling(_fields.Count / 8d), cancellationToken).ConfigureAwait(false);
				var nullBits = new BitArray(nullBytes);
				for (var i = 0; i < _fields.Count; i++)
				{
					if (nullBits.Get(i))
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
		}
		catch (IOException ex)
		{
			throw IscException.ForIOException(ex);
		}
		return row;
	}

	#endregion
}
