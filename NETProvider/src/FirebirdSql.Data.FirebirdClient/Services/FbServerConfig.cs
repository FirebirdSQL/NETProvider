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
 */

using System;

namespace FirebirdSql.Data.Services
{
	public struct FbServerConfig
	{
		#region Fields

		private int _lockMemSize;
		private int _lockSemCount;
		private int _lockSignal;
		private int _eventMemorySize;
		private int _prioritySwitchDelay;
		private int _minMemory;
		private int _maxMemory;
		private int _lockGrantOrder;
		private int _anyLockMemory;
		private int _anyLockSemaphore;
		private int _anyLockSignal;
		private int _anyEventMemory;
		private int _lockHashSlots;
		private int _deadlockTimeout;
		private int _lockRequireSpins;
		private int _connectionTimeout;
		private int _dummyPacketInterval;
		private int _ipcMapSize;
		private int _defaultDbCachePages;

		#endregion

		#region Properties

		public int LockMemSize
		{
			get { return _lockMemSize; }
			set { _lockMemSize = value; }
		}

		public int LockSemCount
		{
			get { return _lockSemCount; }
			set { _lockSemCount = value; }
		}

		public int LockSignal
		{
			get { return _lockSignal; }
			set { _lockSignal = value; }
		}

		public int EventMemorySize
		{
			get { return _eventMemorySize; }
			set { _eventMemorySize = value; }
		}

		public int PrioritySwitchDelay
		{
			get { return _prioritySwitchDelay; }
			set { _prioritySwitchDelay = value; }
		}

		public int MinMemory
		{
			get { return _minMemory; }
			set { _minMemory = value; }
		}

		public int MaxMemory
		{
			get { return _maxMemory; }
			set { _maxMemory = value; }
		}

		public int LockGrantOrder
		{
			get { return _lockGrantOrder; }
			set { _lockGrantOrder = value; }
		}

		public int AnyLockMemory
		{
			get { return _anyLockMemory; }
			set { _anyLockMemory = value; }
		}

		public int AnyLockSemaphore
		{
			get { return _anyLockSemaphore; }
			set { _anyLockSemaphore = value; }
		}

		public int AnyLockSignal
		{
			get { return _anyLockSignal; }
			set { _anyLockSignal = value; }
		}

		public int AnyEventMemory
		{
			get { return _anyEventMemory; }
			set { _anyEventMemory = value; }
		}

		public int LockHashSlots
		{
			get { return _lockHashSlots; }
			set { _lockHashSlots = value; }
		}

		public int DeadlockTimeout
		{
			get { return _deadlockTimeout; }
			set { _deadlockTimeout = value; }
		}

		public int LockRequireSpins
		{
			get { return _lockRequireSpins; }
			set { _lockRequireSpins = value; }
		}

		public int ConnectionTimeout
		{
			get { return _connectionTimeout; }
			set { _connectionTimeout = value; }
		}

		public int DummyPacketInterval
		{
			get { return _dummyPacketInterval; }
			set { _dummyPacketInterval = value; }
		}

		public int IpcMapSize
		{
			get { return _ipcMapSize; }
			set { _ipcMapSize = value; }
		}

		public int DefaultDbCachePages
		{
			get { return _defaultDbCachePages; }
			set { _defaultDbCachePages = value; }
		}

		#endregion
	}
}
