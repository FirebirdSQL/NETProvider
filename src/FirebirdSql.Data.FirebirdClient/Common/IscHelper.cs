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
using System.Collections.Generic;
using System.Globalization;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Common;

internal static class IscHelper
{
	public static List<object> ParseDatabaseInfo(byte[] buffer, Charset charset)
	{
		var info = new List<object>();

		var pos = 0;
		var length = 0;
		var type = 0;

		while ((type = buffer[pos++]) != IscCodes.isc_info_end)
		{
			length = (int)VaxInteger(buffer, pos, 2);
			pos += 2;

			switch (type)
			{
				case IscCodes.isc_info_error:
					throw FbException.Create("Received error response.");

				case IscCodes.isc_info_allocation:
				case IscCodes.isc_info_ods_version:
				case IscCodes.isc_info_ods_minor_version:
				case IscCodes.isc_info_page_size:
				case IscCodes.isc_info_current_memory:
				case IscCodes.isc_info_max_memory:
				case IscCodes.isc_info_num_buffers:
				case IscCodes.isc_info_sweep_interval:
				case IscCodes.isc_info_fetches:
				case IscCodes.isc_info_marks:
				case IscCodes.isc_info_reads:
				case IscCodes.isc_info_writes:
				case IscCodes.isc_info_backout_count:
				case IscCodes.isc_info_delete_count:
				case IscCodes.isc_info_expunge_count:
				case IscCodes.isc_info_insert_count:
				case IscCodes.isc_info_purge_count:
				case IscCodes.isc_info_read_idx_count:
				case IscCodes.isc_info_read_seq_count:
				case IscCodes.isc_info_update_count:
				case IscCodes.isc_info_db_size_in_pages:
				case IscCodes.isc_info_oldest_transaction:
				case IscCodes.isc_info_oldest_active:
				case IscCodes.isc_info_oldest_snapshot:
				case IscCodes.isc_info_next_transaction:
				case IscCodes.isc_info_active_transactions:
				case IscCodes.isc_info_active_tran_count:
				case IscCodes.fb_info_next_attachment:
				case IscCodes.fb_info_next_statement:
				case IscCodes.fb_info_protocol_version:
				case IscCodes.fb_info_statement_timeout_db:
				case IscCodes.fb_info_statement_timeout_att:
					info.Add(VaxInteger(buffer, pos, length));
					break;

				case IscCodes.isc_info_no_reserve:
				case IscCodes.isc_info_forced_writes:
				case IscCodes.isc_info_db_read_only:
					info.Add(buffer[pos] == 1);
					break;

				case IscCodes.isc_info_user_names:
					info.Add(charset.GetString(buffer, pos + 1, buffer[pos]));
					break;

				case IscCodes.fb_info_wire_crypt:
				case IscCodes.fb_info_crypt_plugin:
				case IscCodes.fb_info_db_file_id:
					info.Add(charset.GetString(buffer, pos, length));
					break;

				case IscCodes.fb_info_db_guid:
					info.Add(Guid.ParseExact(charset.GetString(buffer, pos, length), "B"));
					break;

				case IscCodes.isc_info_base_level:
					info.Add(string.Format(CultureInfo.CurrentCulture, "{0}.{1}", buffer[pos], buffer[pos + 1]));
					break;

				case IscCodes.isc_info_db_id:
					{
						var dbFile = charset.GetString(buffer, pos + 2, buffer[pos + 1]);
						var sitePos = pos + 2 + buffer[pos + 1];
						int siteLength = buffer[sitePos];
						var siteName = charset.GetString(buffer, sitePos + 1, siteLength);

						sitePos += siteLength + 1;
						siteLength = buffer[sitePos];
						siteName += "." + charset.GetString(buffer, sitePos + 1, siteLength);

						info.Add(siteName + ":" + dbFile);
					}
					break;

				case IscCodes.isc_info_implementation:
					info.Add(string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}", buffer[pos], buffer[pos + 1], buffer[pos + 2]));
					break;

				case IscCodes.isc_info_isc_version:
				case IscCodes.isc_info_firebird_version:
					{
						var messagePosition = pos;
						var count = buffer[messagePosition];
						for (var i = 0; i < count; i++)
						{
							var messageLength = buffer[messagePosition + 1];
							info.Add(charset.GetString(buffer, messagePosition + 2, messageLength));
							messagePosition += 1 + messageLength;
						}
					}
					break;

				case IscCodes.isc_info_db_class:
					{
						var serverClass = VaxInteger(buffer, pos, length);
						info.Add(serverClass switch
						{
							IscCodes.isc_info_db_class_classic_access => "CLASSIC SERVER",
							IscCodes.isc_info_db_class_server_access => "SUPER SERVER",
							_ => throw new ArgumentOutOfRangeException(nameof(serverClass), $"{nameof(serverClass)}={serverClass}"),
						});
					}
					break;

				case IscCodes.isc_info_creation_date:
					{
						var date = TypeDecoder.DecodeDate((int)VaxInteger(buffer, pos, 4));
						var time = TypeDecoder.DecodeTime((int)VaxInteger(buffer, pos + 4, 4));
						info.Add(date.Add(time));
					}
					break;

				case IscCodes.fb_info_replica_mode:
					{
						var mode = VaxInteger(buffer, pos, length);
						info.Add(mode switch
						{
							0 => "NONE",
							1 => "READ ONLY",
							2 => "READ WRITE",
							_ => throw new ArgumentOutOfRangeException(nameof(mode), $"{nameof(mode)}={mode}"),
						});
					}
					break;

				case IscCodes.fb_info_creation_timestamp_tz:
					{
						var date = TypeDecoder.DecodeDate((int)VaxInteger(buffer, pos, 4));
						var time = TypeDecoder.DecodeTime((int)VaxInteger(buffer, pos + 4, 4));
						var tzId = (ushort)VaxInteger(buffer, pos + 4 + 4, 4);
						var dt = date.Add(time);
						dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
						info.Add(TypeHelper.CreateZonedDateTime(dt, tzId, null));
					}
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
			}

			pos += length;
		}

		return info;
	}

	public static List<object> ParseTransactionInfo(byte[] buffer, Charset charset)
	{
		var info = new List<object>();

		var pos = 0;
		var length = 0;
		var type = 0;

		while ((type = buffer[pos++]) != IscCodes.isc_info_end)
		{
			length = (int)VaxInteger(buffer, pos, 2);
			pos += 2;

			switch (type)
			{
				case IscCodes.isc_info_error:
					throw FbException.Create("Received error response.");

				case IscCodes.fb_info_tra_snapshot_number:
					info.Add(VaxInteger(buffer, pos, length));
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
			}

			pos += length;
		}

		return info;
	}

	public static long VaxInteger(byte[] buffer, int index, int length)
	{
		var value = 0L;
		var shift = 0;
		var i = index;
		while (--length >= 0)
		{
			value += (buffer[i++] & 0xffL) << shift;
			shift += 8;
		}
		return value;
	}
}
