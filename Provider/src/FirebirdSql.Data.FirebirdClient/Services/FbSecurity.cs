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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services
{
	public sealed class FbSecurity : FbService
	{
		public FbSecurity(string connectionString = null)
			: base(connectionString)
		{ }

		public void AddUser(FbUserData user) => AddUserImpl(user, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task AddUserAsync(FbUserData user, CancellationToken cancellationToken = default) => AddUserImpl(user, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task AddUserImpl(FbUserData user, AsyncWrappingCommonArgs async)
		{
			if (string.IsNullOrEmpty(user.UserName))
				throw new InvalidOperationException("Invalid user name.");

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_add_user);
			startSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);
			startSpb.Append(IscCodes.isc_spb_sec_password, user.UserPassword);
			if ((user.FirstName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_firstname, user.FirstName);
			if ((user.MiddleName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_middlename, user.MiddleName);
			if ((user.LastName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_lastname, user.LastName);
			if (user.UserID != 0)
				startSpb.Append(IscCodes.isc_spb_sec_userid, user.UserID);
			if (user.GroupID != 0)
				startSpb.Append(IscCodes.isc_spb_sec_groupid, user.GroupID);
			if ((user.GroupName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_groupname, user.GroupName);
			if ((user.RoleName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void DeleteUser(FbUserData user) => DeleteUserImpl(user, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task DeleteUserAsync(FbUserData user, CancellationToken cancellationToken = default) => DeleteUserImpl(user, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task DeleteUserImpl(FbUserData user, AsyncWrappingCommonArgs async)
		{
			if (string.IsNullOrEmpty(user.UserName))
				throw new InvalidOperationException("Invalid user name.");

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_delete_user);
			startSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);
			if ((user.RoleName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public void ModifyUser(FbUserData user) => ModifyUserImpl(user, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task ModifyUserAsync(FbUserData user, CancellationToken cancellationToken = default) => ModifyUserImpl(user, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task ModifyUserImpl(FbUserData user, AsyncWrappingCommonArgs async)
		{
			if (string.IsNullOrEmpty(user.UserName))
				throw new InvalidOperationException("Invalid user name.");

			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_modify_user);
			startSpb.Append(IscCodes.isc_spb_sec_username, user.UserName);
			if ((user.UserPassword?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_password, user.UserPassword);
			if ((user.FirstName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_firstname, user.FirstName);
			if ((user.MiddleName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_middlename, user.MiddleName);
			if ((user.LastName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_lastname, user.LastName);
			startSpb.Append(IscCodes.isc_spb_sec_userid, user.UserID);
			startSpb.Append(IscCodes.isc_spb_sec_groupid, user.GroupID);
			if ((user.GroupName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sec_groupname, user.GroupName);
			if ((user.RoleName?.Length ?? 0) != 0)
				startSpb.Append(IscCodes.isc_spb_sql_role_name, user.RoleName);
			await StartTask(startSpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
		}

		public FbUserData DisplayUser(string userName) => DisplayUserImpl(userName, new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<FbUserData> DisplayUserAsync(string userName, CancellationToken cancellationToken = default) => DisplayUserImpl(userName, new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task<FbUserData> DisplayUserImpl(string userName, AsyncWrappingCommonArgs async)
		{
			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_display_user);
			startSpb.Append(IscCodes.isc_spb_sec_username, userName);
			await StartTask(startSpb, async).ConfigureAwait(false);
			var info = await Query(new byte[] { IscCodes.isc_info_svc_get_users }, EmptySpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
			return ((FbUserData[])info.FirstOrDefault())?.FirstOrDefault();
		}

		public FbUserData[] DisplayUsers() => DisplayUsersImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<FbUserData[]> DisplayUsersAsync(CancellationToken cancellationToken = default) => DisplayUsersImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task<FbUserData[]> DisplayUsersImpl(AsyncWrappingCommonArgs async)
		{
			await Open(async).ConfigureAwait(false);
			var startSpb = new ServiceParameterBuffer();
			startSpb.Append(IscCodes.isc_action_svc_display_user);
			await StartTask(startSpb, async).ConfigureAwait(false);
			var info = await Query(new byte[] { IscCodes.isc_info_svc_get_users }, EmptySpb, async).ConfigureAwait(false);
			await Close(async).ConfigureAwait(false);
			return (FbUserData[])info.FirstOrDefault();
		}

		public string GetUsersDbPath() => GetUsersDbPathImpl(new AsyncWrappingCommonArgs(false)).GetAwaiter().GetResult();
		public Task<string> GetUsersDbPathAsync(CancellationToken cancellationToken = default) => GetUsersDbPathImpl(new AsyncWrappingCommonArgs(true, cancellationToken));
		private async Task<string> GetUsersDbPathImpl(AsyncWrappingCommonArgs async)
		{
			var info = await Query(new byte[] { IscCodes.isc_info_svc_user_dbpath }, EmptySpb, async).ConfigureAwait(false);
			return (string)info.FirstOrDefault();
		}
	}
}
