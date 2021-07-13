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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common
{
	[StructLayout(LayoutKind.Auto)]
	internal struct AsyncWrappingCommonArgs
	{
		readonly bool _isAsync;
		readonly CancellationToken _cancellationTokenRaw;
		int _explicitCancel;
		public readonly bool IsAsync => _isAsync;
		public readonly CancellationToken CancellationToken => _explicitCancel > 0 ? CancellationToken.None : _cancellationTokenRaw;

		public AsyncWrappingCommonArgs(bool isAsync, CancellationToken cancellationToken = default)
		{
			_isAsync = isAsync;
			_cancellationTokenRaw = cancellationToken;
			_explicitCancel = 0;
		}

		public readonly Task<TResult> AsyncSyncCall<TResult>(Func<CancellationToken, Task<TResult>> asyncCall, Func<TResult> syncCall)
		{
			return _isAsync ? asyncCall(CancellationToken) : Task.FromResult(syncCall());
		}
		public readonly Task<TResult> AsyncSyncCallNoCancellation<TResult>(Func<Task<TResult>> asyncCall, Func<TResult> syncCall)
		{
			return _isAsync ? asyncCall() : Task.FromResult(syncCall());
		}
		public readonly Task<TResult> AsyncSyncCall<T1, TResult>(Func<T1, CancellationToken, Task<TResult>> asyncCall, Func<T1, TResult> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1, CancellationToken) : Task.FromResult(syncCall(arg1));
		}
		public readonly Task<TResult> AsyncSyncCallNoCancellation<T1, TResult>(Func<T1, Task<TResult>> asyncCall, Func<T1, TResult> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1) : Task.FromResult(syncCall(arg1));
		}
		public readonly Task<TResult> AsyncSyncCall<T1, T2, TResult>(Func<T1, T2, CancellationToken, Task<TResult>> asyncCall, Func<T1, T2, TResult> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2, CancellationToken) : Task.FromResult(syncCall(arg1, arg2));
		}
		public readonly Task<TResult> AsyncSyncCallNoCancellation<T1, T2, TResult>(Func<T1, T2, Task<TResult>> asyncCall, Func<T1, T2, TResult> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2) : Task.FromResult(syncCall(arg1, arg2));
		}
		public readonly Task<TResult> AsyncSyncCall<T1, T2, T3, TResult>(Func<T1, T2, T3, CancellationToken, Task<TResult>> asyncCall, Func<T1, T2, T3, TResult> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3, CancellationToken) : Task.FromResult(syncCall(arg1, arg2, arg3));
		}
		public readonly Task<TResult> AsyncSyncCallNoCancellation<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> asyncCall, Func<T1, T2, T3, TResult> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3) : Task.FromResult(syncCall(arg1, arg2, arg3));
		}

		public readonly ValueTask<TResult> AsyncSyncCall<TResult>(Func<CancellationToken, ValueTask<TResult>> asyncCall, Func<TResult> syncCall)
		{
			return _isAsync ? asyncCall(CancellationToken) : ValueTask2.FromResult(syncCall());
		}
		public readonly ValueTask<TResult> AsyncSyncCallNoCancellation<TResult>(Func<ValueTask<TResult>> asyncCall, Func<TResult> syncCall)
		{
			return _isAsync ? asyncCall() : ValueTask2.FromResult(syncCall());
		}
		public readonly ValueTask<TResult> AsyncSyncCall<T1, TResult>(Func<T1, CancellationToken, ValueTask<TResult>> asyncCall, Func<T1, TResult> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1, CancellationToken) : ValueTask2.FromResult(syncCall(arg1));
		}
		public readonly ValueTask<TResult> AsyncSyncCallNoCancellation<T1, TResult>(Func<T1, ValueTask<TResult>> asyncCall, Func<T1, TResult> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1) : ValueTask2.FromResult(syncCall(arg1));
		}
		public readonly ValueTask<TResult> AsyncSyncCall<T1, T2, TResult>(Func<T1, T2, CancellationToken, ValueTask<TResult>> asyncCall, Func<T1, T2, TResult> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2, CancellationToken) : ValueTask2.FromResult(syncCall(arg1, arg2));
		}
		public readonly ValueTask<TResult> AsyncSyncCallNoCancellation<T1, T2, TResult>(Func<T1, T2, ValueTask<TResult>> asyncCall, Func<T1, T2, TResult> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2) : ValueTask2.FromResult(syncCall(arg1, arg2));
		}
		public readonly ValueTask<TResult> AsyncSyncCall<T1, T2, T3, TResult>(Func<T1, T2, T3, CancellationToken, ValueTask<TResult>> asyncCall, Func<T1, T2, T3, TResult> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3, CancellationToken) : ValueTask2.FromResult(syncCall(arg1, arg2, arg3));
		}
		public readonly ValueTask<TResult> AsyncSyncCallNoCancellation<T1, T2, T3, TResult>(Func<T1, T2, T3, ValueTask<TResult>> asyncCall, Func<T1, T2, T3, TResult> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3) : ValueTask2.FromResult(syncCall(arg1, arg2, arg3));
		}

		public readonly Task AsyncSyncCall(Func<CancellationToken, Task> asyncCall, Action syncCall)
		{
			return _isAsync ? asyncCall(CancellationToken) : SyncTaskCompleted(Task.CompletedTask, syncCall);
		}
		public readonly Task AsyncSyncCallNoCancellation(Func<Task> asyncCall, Action syncCall)
		{
			return _isAsync ? asyncCall() : SyncTaskCompleted(Task.CompletedTask, syncCall);
		}
		public readonly Task AsyncSyncCall<T1>(Func<T1, CancellationToken, Task> asyncCall, Action<T1> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1, CancellationToken) : SyncTaskCompleted(Task.CompletedTask, syncCall, arg1);
		}
		public readonly Task AsyncSyncCallNoCancellation<T1>(Func<T1, Task> asyncCall, Action<T1> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1) : SyncTaskCompleted(Task.CompletedTask, syncCall, arg1);
		}
		public readonly Task AsyncSyncCall<T1, T2>(Func<T1, T2, CancellationToken, Task> asyncCall, Action<T1, T2> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2, CancellationToken) : SyncTaskCompleted(Task.CompletedTask, syncCall, arg1, arg2);
		}
		public readonly Task AsyncSyncCallNoCancellation<T1, T2>(Func<T1, T2, Task> asyncCall, Action<T1, T2> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2) : SyncTaskCompleted(Task.CompletedTask, syncCall, arg1, arg2);
		}
		public readonly Task AsyncSyncCall<T1, T2, T3>(Func<T1, T2, T3, CancellationToken, Task> asyncCall, Action<T1, T2, T3> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3, CancellationToken) : SyncTaskCompleted(Task.CompletedTask, syncCall, arg1, arg2, arg3);
		}
		public readonly Task AsyncSyncCallNoCancellation<T1, T2, T3>(Func<T1, T2, T3, Task> asyncCall, Action<T1, T2, T3> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3) : SyncTaskCompleted(Task.CompletedTask, syncCall, arg1, arg2, arg3);
		}

		public readonly ValueTask AsyncSyncCall(Func<CancellationToken, ValueTask> asyncCall, Action syncCall)
		{
			return _isAsync ? asyncCall(CancellationToken) : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall);
		}
		public readonly ValueTask AsyncSyncCallNoCancellation(Func<ValueTask> asyncCall, Action syncCall)
		{
			return _isAsync ? asyncCall() : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall);
		}
		public readonly ValueTask AsyncSyncCall<T1>(Func<T1, CancellationToken, ValueTask> asyncCall, Action<T1> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1, CancellationToken) : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall, arg1);
		}
		public readonly ValueTask AsyncSyncCallNoCancellation<T1>(Func<T1, ValueTask> asyncCall, Action<T1> syncCall, T1 arg1)
		{
			return _isAsync ? asyncCall(arg1) : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall, arg1);
		}
		public readonly ValueTask AsyncSyncCall<T1, T2>(Func<T1, T2, CancellationToken, ValueTask> asyncCall, Action<T1, T2> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2, CancellationToken) : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall, arg1, arg2);
		}
		public readonly ValueTask AsyncSyncCallNoCancellation<T1, T2>(Func<T1, T2, ValueTask> asyncCall, Action<T1, T2> syncCall, T1 arg1, T2 arg2)
		{
			return _isAsync ? asyncCall(arg1, arg2) : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall, arg1, arg2);
		}
		public readonly ValueTask AsyncSyncCall<T1, T2, T3>(Func<T1, T2, T3, CancellationToken, ValueTask> asyncCall, Action<T1, T2, T3> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3, CancellationToken) : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall, arg1, arg2, arg3);
		}
		public readonly ValueTask AsyncSyncCallNoCancellation<T1, T2, T3>(Func<T1, T2, T3, ValueTask> asyncCall, Action<T1, T2, T3> syncCall, T1 arg1, T2 arg2, T3 arg3)
		{
			return _isAsync ? asyncCall(arg1, arg2, arg3) : SyncTaskCompleted(ValueTask2.CompletedTask, syncCall, arg1, arg2, arg3);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TTask SyncTaskCompleted<TTask>(TTask completed, Action sync)
		{
			sync();
			return completed;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TTask SyncTaskCompleted<TTask, T1>(TTask completed, Action<T1> sync, T1 arg1)
		{
			sync(arg1);
			return completed;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TTask SyncTaskCompleted<TTask, T1, T2>(TTask completed, Action<T1, T2> sync, T1 arg1, T2 arg2)
		{
			sync(arg1, arg2);
			return completed;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TTask SyncTaskCompleted<TTask, T1, T2, T3>(TTask completed, Action<T1, T2, T3> sync, T1 arg1, T2 arg2, T3 arg3)
		{
			sync(arg1, arg2, arg3);
			return completed;
		}

		public IDisposable EnterExplicitCancel(Action explicitCancel)
		{
			if (_cancellationTokenRaw.IsCancellationRequested)
			{
				explicitCancel();
				throw new OperationCanceledException(_cancellationTokenRaw);
			}
			_explicitCancel += 1;
			var ctr = _cancellationTokenRaw.Register(explicitCancel);
			return new ExitHolder(this, ctr);
		}

		void ExitExplicitCancel(CancellationTokenRegistration cancellationTokenRegistration)
		{
			cancellationTokenRegistration.Dispose();
			_explicitCancel -= 1;
		}

		[StructLayout(LayoutKind.Auto)]
		readonly struct ExitHolder : IDisposable
		{
			readonly AsyncWrappingCommonArgs _async;
			readonly CancellationTokenRegistration _cancellationTokenRegistration;

			public ExitHolder(AsyncWrappingCommonArgs async, CancellationTokenRegistration cancellationTokenRegistration)
			{
				_async = async;
				_cancellationTokenRegistration = cancellationTokenRegistration;
			}

			public void Dispose()
			{
				_async.ExitExplicitCancel(_cancellationTokenRegistration);
			}
		}
	}
}
