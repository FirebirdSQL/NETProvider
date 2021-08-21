/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Services
{
	public abstract class FbService
	{
		public event EventHandler<ServiceOutputEventArgs> ServiceOutput;

		public event EventHandler<FbInfoMessageEventArgs> InfoMessage;

		private const string ServiceName = "service_mgr";

		private ServiceManagerBase _svc;
		private ConnectionString _options;

		private protected Encoding SpbFilenameEncoding;

		private protected string Database => _options.Database;

		public FbServiceState State { get; private set; }
		public int QueryBufferSize { get; set; }

		private string _connectionString;
		public string ConnectionString
		{
			get { return _connectionString; }
			set
			{
				if (_svc != null && State == FbServiceState.Open)
				{
					throw new InvalidOperationException("ConnectionString cannot be modified on open instances.");
				}

				_options = new ConnectionString(value);

				if (value == null)
				{
					_connectionString = string.Empty;
				}
				else
				{
					_connectionString = value;
				}
			}
		}

		private protected FbService(string connectionString = null)
		{
			State = FbServiceState.Closed;
			QueryBufferSize = IscCodes.DEFAULT_MAX_BUFFER_SIZE;
			ConnectionString = connectionString;
		}

		private ServiceParameterBufferBase BuildSpb()
		{
			SpbFilenameEncoding = Encoding2.Default;
			var spb = _svc.CreateServiceParameterBuffer();
			spb.AppendPreamble();
			if (_svc is Client.Managed.Version13.GdsServiceManager gdsSvc)
			{
				if (!gdsSvc.Connection.AuthBlock.HasClientData)
				{
					spb.Append1(IscCodes.isc_spb_auth_plugin_name, gdsSvc.Connection.AuthBlock.AcceptPluginName);
					spb.Append1(IscCodes.isc_spb_specific_auth_data, gdsSvc.Connection.AuthBlock.PublicClientData);
				}
				else
				{
					spb.Append1(IscCodes.isc_spb_specific_auth_data, gdsSvc.Connection.AuthBlock.ClientData);
				}
			}
			else
			{
				spb.Append1(IscCodes.isc_spb_user_name, _options.UserID);
				spb.Append1(IscCodes.isc_spb_password, _options.Password);
			}
			spb.Append1(IscCodes.isc_spb_dummy_packet_interval, new byte[] { 120, 10, 0, 0 });
			if ((_options?.Role.Length ?? 0) != 0)
			{
				spb.Append1(IscCodes.isc_spb_sql_role_name, _options.Role);
			}
			if (_svc is Client.Managed.Version13.GdsServiceManager)
			{
				spb.Append1(IscCodes.isc_spb_utf8_filename, new byte[] { 0 });
				SpbFilenameEncoding = Encoding.UTF8;
			}
			spb.Append1(IscCodes.isc_spb_expected_db, _options.Database, SpbFilenameEncoding);
			return spb;
		}

		private protected void Open()
		{
			if (State != FbServiceState.Closed)
				throw new InvalidOperationException("Service already Open.");
			if (string.IsNullOrEmpty(_options.UserID))
				throw new InvalidOperationException("No user name was specified.");
			if (string.IsNullOrEmpty(_options.Password))
				throw new InvalidOperationException("No user password was specified.");

			try
			{
				if (_svc == null)
				{
					_svc = ClientFactory.CreateServiceManager(_options);
				}
				_svc.Attach(BuildSpb(), _options.DataSource, _options.Port, ServiceName, _options.CryptKey);
				_svc.WarningMessage = OnWarningMessage;
				State = FbServiceState.Open;
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
		}
		private protected async Task OpenAsync(CancellationToken cancellationToken = default)
		{
			if (State != FbServiceState.Closed)
				throw new InvalidOperationException("Service already Open.");
			if (string.IsNullOrEmpty(_options.UserID))
				throw new InvalidOperationException("No user name was specified.");
			if (string.IsNullOrEmpty(_options.Password))
				throw new InvalidOperationException("No user password was specified.");

			try
			{
				if (_svc == null)
				{
					_svc = await ClientFactory.CreateServiceManagerAsync(_options, cancellationToken).ConfigureAwait(false);
				}
				await _svc.AttachAsync(BuildSpb(), _options.DataSource, _options.Port, ServiceName, _options.CryptKey, cancellationToken).ConfigureAwait(false);
				_svc.WarningMessage = OnWarningMessage;
				State = FbServiceState.Open;
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
		}

		private protected void Close()
		{
			if (State != FbServiceState.Open)
			{
				return;
			}
			try
			{
				_svc.Detach();
				_svc = null;
				State = FbServiceState.Closed;
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
		}
		private protected async Task CloseAsync(CancellationToken cancellationToken = default)
		{
			if (State != FbServiceState.Open)
			{
				return;
			}
			try
			{
				await _svc.DetachAsync(cancellationToken).ConfigureAwait(false);
				_svc = null;
				State = FbServiceState.Closed;
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
		}

		private protected void StartTask(ServiceParameterBufferBase spb)
		{
			if (State == FbServiceState.Closed)
				throw new InvalidOperationException("Service is Closed.");

			try
			{
				_svc.Start(spb);
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
		}
		private protected async Task StartTaskAsync(ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
		{
			if (State == FbServiceState.Closed)
				throw new InvalidOperationException("Service is Closed.");

			try
			{
				await _svc.StartAsync(spb, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw FbException.Create(ex);
			}
		}

		private protected List<object> Query(byte[] items, ServiceParameterBufferBase spb)
		{
			var result = new List<object>();
			Query(items, spb, (truncated, item) =>
			{
				if (item is string stringItem)
				{
					if (!truncated)
					{
						result.Add(stringItem);
					}
					else
					{
						var lastValue = result[result.Count - 1] as string;
						result[result.Count - 1] = lastValue + stringItem;
					}
					return;
				}

				if (item is byte[] byteArrayItem)
				{
					if (!truncated)
					{
						result.Add(byteArrayItem);
					}
					else
					{
						var lastValue = result[result.Count - 1] as byte[];
						var lastValueLength = lastValue.Length;
						Array.Resize(ref lastValue, lastValue.Length + byteArrayItem.Length);
						Array.Copy(byteArrayItem, 0, lastValue, lastValueLength, byteArrayItem.Length);
					}
					return;
				}

				result.Add(item);
			});
			return result;
		}
		private protected async Task<List<object>> QueryAsync(byte[] items, ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
		{
			var result = new List<object>();
			await QueryAsync(items, spb, (truncated, item) =>
			{
				if (item is string stringItem)
				{
					if (!truncated)
					{
						result.Add(stringItem);
					}
					else
					{
						var lastValue = result[result.Count - 1] as string;
						result[result.Count - 1] = lastValue + stringItem;
					}
					return Task.CompletedTask;
				}

				if (item is byte[] byteArrayItem)
				{
					if (!truncated)
					{
						result.Add(byteArrayItem);
					}
					else
					{
						var lastValue = result[result.Count - 1] as byte[];
						var lastValueLength = lastValue.Length;
						Array.Resize(ref lastValue, lastValue.Length + byteArrayItem.Length);
						Array.Copy(byteArrayItem, 0, lastValue, lastValueLength, byteArrayItem.Length);
					}
					return Task.CompletedTask;
				}

				result.Add(item);

				return Task.CompletedTask;
			}, cancellationToken).ConfigureAwait(false);
			return result;
		}

		private protected void Query(byte[] items, ServiceParameterBufferBase spb, Action<bool, object> queryResponseAction)
		{
			var pos = 0;
			var truncated = false;
			var type = default(int);

			var buffer = QueryService(items, spb);

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				if (type == IscCodes.isc_info_truncated)
				{
					buffer = QueryService(items, spb);
					pos = 0;
					truncated = true;
					continue;
				}

				switch (type)
				{
					case IscCodes.isc_info_svc_version:
					case IscCodes.isc_info_svc_get_license_mask:
					case IscCodes.isc_info_svc_capabilities:
					case IscCodes.isc_info_svc_get_licensed_users:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							queryResponseAction(truncated, (int)IscHelper.VaxInteger(buffer, pos, 4));
							pos += length;
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_server_version:
					case IscCodes.isc_info_svc_implementation:
					case IscCodes.isc_info_svc_get_env:
					case IscCodes.isc_info_svc_get_env_lock:
					case IscCodes.isc_info_svc_get_env_msg:
					case IscCodes.isc_info_svc_user_dbpath:
					case IscCodes.isc_info_svc_line:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							queryResponseAction(truncated, Encoding2.Default.GetString(buffer, pos, length));
							pos += length;
							truncated = false;
							break;
						}
					case IscCodes.isc_info_svc_to_eof:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							var block = new byte[length];
							Array.Copy(buffer, pos, block, 0, length);
							queryResponseAction(truncated, block);
							pos += length;
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_svr_db_info:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							queryResponseAction(truncated, ParseDatabasesInfo(buffer, ref pos));
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_get_users:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							queryResponseAction(truncated, ParseUserData(buffer, ref pos));
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_get_config:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							queryResponseAction(truncated, ParseServerConfig(buffer, ref pos));
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_stdin:
						{
							var length = GetLength(buffer, 4, ref pos);
							queryResponseAction(truncated, length);
							truncated = false;
							break;
						}

					case IscCodes.isc_info_data_not_ready:
						{
							queryResponseAction(truncated, typeof(void));
							truncated = false;
							break;
						}
				}
			}
		}
		private protected async Task QueryAsync(byte[] items, ServiceParameterBufferBase spb, Func<bool, object, Task> queryResponseAction, CancellationToken cancellationToken = default)
		{
			var pos = 0;
			var truncated = false;
			var type = default(int);

			var buffer = await QueryServiceAsync(items, spb, cancellationToken).ConfigureAwait(false);

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				if (type == IscCodes.isc_info_truncated)
				{
					buffer = await QueryServiceAsync(items, spb, cancellationToken).ConfigureAwait(false);
					pos = 0;
					truncated = true;
					continue;
				}

				switch (type)
				{
					case IscCodes.isc_info_svc_version:
					case IscCodes.isc_info_svc_get_license_mask:
					case IscCodes.isc_info_svc_capabilities:
					case IscCodes.isc_info_svc_get_licensed_users:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							await queryResponseAction(truncated, (int)IscHelper.VaxInteger(buffer, pos, 4)).ConfigureAwait(false);
							pos += length;
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_server_version:
					case IscCodes.isc_info_svc_implementation:
					case IscCodes.isc_info_svc_get_env:
					case IscCodes.isc_info_svc_get_env_lock:
					case IscCodes.isc_info_svc_get_env_msg:
					case IscCodes.isc_info_svc_user_dbpath:
					case IscCodes.isc_info_svc_line:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							await queryResponseAction(truncated, Encoding2.Default.GetString(buffer, pos, length)).ConfigureAwait(false);
							pos += length;
							truncated = false;
							break;
						}
					case IscCodes.isc_info_svc_to_eof:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							var block = new byte[length];
							Array.Copy(buffer, pos, block, 0, length);
							await queryResponseAction(truncated, block).ConfigureAwait(false);
							pos += length;
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_svr_db_info:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							await queryResponseAction(truncated, ParseDatabasesInfo(buffer, ref pos)).ConfigureAwait(false);
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_get_users:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							await queryResponseAction(truncated, ParseUserData(buffer, ref pos)).ConfigureAwait(false);
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_get_config:
						{
							var length = GetLength(buffer, 2, ref pos);
							if (length == 0)
								continue;
							await queryResponseAction(truncated, ParseServerConfig(buffer, ref pos)).ConfigureAwait(false);
							truncated = false;
							break;
						}

					case IscCodes.isc_info_svc_stdin:
						{
							var length = GetLength(buffer, 4, ref pos);
							await queryResponseAction(truncated, length).ConfigureAwait(false);
							truncated = false;
							break;
						}

					case IscCodes.isc_info_data_not_ready:
						{
							await queryResponseAction(truncated, typeof(void)).ConfigureAwait(false);
							truncated = false;
							break;
						}
				}
			}
		}

		private protected void ProcessServiceOutput(ServiceParameterBufferBase spb)
		{
			string line;
			while ((line = GetNextLine(spb)) != null)
			{
				OnServiceOutput(line);
			}
		}
		private protected async Task ProcessServiceOutputAsync(ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
		{
			string line;
			while ((line = await GetNextLineAsync(spb, cancellationToken).ConfigureAwait(false)) != null)
			{
				OnServiceOutput(line);
			}
		}

		private protected string GetNextLine(ServiceParameterBufferBase spb)
		{
			var info = Query(new byte[] { IscCodes.isc_info_svc_line }, spb);
			if (info.Count == 0)
				return null;
			return info[0] as string;
		}
		private protected async Task<string> GetNextLineAsync(ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
		{
			var info = await QueryAsync(new byte[] { IscCodes.isc_info_svc_line }, spb, cancellationToken).ConfigureAwait(false);
			if (info.Count == 0)
				return null;
			return info[0] as string;
		}

		private protected void OnServiceOutput(string message)
		{
			ServiceOutput?.Invoke(this, new ServiceOutputEventArgs(message));
		}

		private protected void EnsureDatabase()
		{
			if (string.IsNullOrEmpty(Database))
				throw FbException.Create("Action should be executed against a specific database.");
		}

		private byte[] QueryService(byte[] items, ServiceParameterBufferBase spb)
		{
			var shouldClose = false;
			if (State == FbServiceState.Closed)
			{
				Open();
				shouldClose = true;
			}
			try
			{
				var buffer = new byte[QueryBufferSize];
				_svc.Query(spb, items.Length, items, buffer.Length, buffer);
				return buffer;
			}
			finally
			{
				if (shouldClose)
				{
					Close();
				}
			}
		}
		private async Task<byte[]> QueryServiceAsync(byte[] items, ServiceParameterBufferBase spb, CancellationToken cancellationToken = default)
		{
			var shouldClose = false;
			if (State == FbServiceState.Closed)
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);
				shouldClose = true;
			}
			try
			{
				var buffer = new byte[QueryBufferSize];
				await _svc.QueryAsync(spb, items.Length, items, buffer.Length, buffer, cancellationToken).ConfigureAwait(false);
				return buffer;
			}
			finally
			{
				if (shouldClose)
				{
					await CloseAsync(cancellationToken).ConfigureAwait(false);
				}
			}
		}

		private void OnWarningMessage(IscException warning)
		{
			InfoMessage?.Invoke(this, new FbInfoMessageEventArgs(warning));
		}

		private static FbServerConfig ParseServerConfig(byte[] buffer, ref int pos)
		{
			var config = new FbServerConfig();

			pos = 1;
			while (buffer[pos] != IscCodes.isc_info_flag_end)
			{
				pos++;

				int key = buffer[pos - 1];
				var keyValue = (int)IscHelper.VaxInteger(buffer, pos, 4);

				pos += 4;

				switch (key)
				{
					case IscCodes.ISCCFG_LOCKMEM_KEY:
						config.LockMemSize = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKSEM_KEY:
						config.LockSemCount = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKSIG_KEY:
						config.LockSignal = keyValue;
						break;

					case IscCodes.ISCCFG_EVNTMEM_KEY:
						config.EventMemorySize = keyValue;
						break;

					case IscCodes.ISCCFG_PRIORITY_KEY:
						config.PrioritySwitchDelay = keyValue;
						break;

					case IscCodes.ISCCFG_MEMMIN_KEY:
						config.MinMemory = keyValue;
						break;

					case IscCodes.ISCCFG_MEMMAX_KEY:
						config.MaxMemory = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKORDER_KEY:
						config.LockGrantOrder = keyValue;
						break;

					case IscCodes.ISCCFG_ANYLOCKMEM_KEY:
						config.AnyLockMemory = keyValue;
						break;

					case IscCodes.ISCCFG_ANYLOCKSEM_KEY:
						config.AnyLockSemaphore = keyValue;
						break;

					case IscCodes.ISCCFG_ANYLOCKSIG_KEY:
						config.AnyLockSignal = keyValue;
						break;

					case IscCodes.ISCCFG_ANYEVNTMEM_KEY:
						config.AnyEventMemory = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKHASH_KEY:
						config.LockHashSlots = keyValue;
						break;

					case IscCodes.ISCCFG_DEADLOCK_KEY:
						config.DeadlockTimeout = keyValue;
						break;

					case IscCodes.ISCCFG_LOCKSPIN_KEY:
						config.LockRequireSpins = keyValue;
						break;

					case IscCodes.ISCCFG_CONN_TIMEOUT_KEY:
						config.ConnectionTimeout = keyValue;
						break;

					case IscCodes.ISCCFG_DUMMY_INTRVL_KEY:
						config.DummyPacketInterval = keyValue;
						break;

					case IscCodes.ISCCFG_IPCMAP_KEY:
						config.IpcMapSize = keyValue;
						break;

					case IscCodes.ISCCFG_DBCACHE_KEY:
						config.DefaultDbCachePages = keyValue;
						break;
				}
			}

			pos++;

			return config;
		}

		private static FbDatabasesInfo ParseDatabasesInfo(byte[] buffer, ref int pos)
		{
			var dbInfo = new FbDatabasesInfo();
			var type = 0;
			var length = 0;

			pos = 1;

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				switch (type)
				{
					case IscCodes.isc_spb_num_att:
						dbInfo.ConnectionCount = (int)IscHelper.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;

					case IscCodes.isc_spb_num_db:
						pos += 4;
						break;

					case IscCodes.isc_spb_dbname:
						length = (int)IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						dbInfo.AddDatabase(Encoding2.Default.GetString(buffer, pos, length));
						pos += length;
						break;
				}
			}

			pos--;

			return dbInfo;
		}

		private static FbUserData[] ParseUserData(byte[] buffer, ref int pos)
		{
			var users = new List<FbUserData>();
			FbUserData currentUser = null;
			var type = 0;
			var length = 0;

			while ((type = buffer[pos++]) != IscCodes.isc_info_end)
			{
				switch (type)
				{
					case IscCodes.isc_spb_sec_username:
						{
							length = (int)IscHelper.VaxInteger(buffer, pos, 2);
							pos += 2;
							currentUser = new FbUserData();
							currentUser.UserName = Encoding2.Default.GetString(buffer, pos, length);
							pos += length;

							users.Add(currentUser);
						}
						break;

					case IscCodes.isc_spb_sec_firstname:
						length = (int)IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						currentUser.FirstName = Encoding2.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_middlename:
						length = (int)IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						currentUser.MiddleName = Encoding2.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_lastname:
						length = (int)IscHelper.VaxInteger(buffer, pos, 2);
						pos += 2;
						currentUser.LastName = Encoding2.Default.GetString(buffer, pos, length);
						pos += length;
						break;

					case IscCodes.isc_spb_sec_userid:
						currentUser.UserID = (int)IscHelper.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;

					case IscCodes.isc_spb_sec_groupid:
						currentUser.GroupID = (int)IscHelper.VaxInteger(buffer, pos, 4);
						pos += 4;
						break;
				}
			}

			pos--;

			return users.ToArray();
		}

		private static int GetLength(byte[] buffer, int size, ref int pos)
		{
			var result = (int)IscHelper.VaxInteger(buffer, pos, size);
			pos += size;
			return result;
		}
	}
}
