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

		private int lockMemSize;
		private int lockSemCount;
		private int lockSignal;
		private int eventMemorySize;
		private int prioritySwitchDelay;
		private int minMemory;
		private int maxMemory;
		private int lockGrantOrder;
		private int anyLockMemory;
		private int anyLockSemaphore;
		private int anyLockSignal;
		private int anyEventMemory;
		private int lockHashSlots;
		private int deadlockTimeout;
		private int lockRequireSpins;
		private int connectionTimeout;
		private int dummyPacketInterval;
		private int ipcMapSize;
		private int defaultDbCachePages;

		#endregion

		#region Properties

		public int LockMemSize
		{
			get { return lockMemSize; }
			set { lockMemSize = value; }
		}

		public int LockSemCount
		{
			get { return lockSemCount; }
			set { lockSemCount = value; }
		}

		public int LockSignal
		{
			get { return lockSignal; }
			set { lockSignal = value; }
		}

		public int EventMemorySize
		{
			get { return eventMemorySize; }
			set { eventMemorySize = value; }
		}

		public int PrioritySwitchDelay
		{
			get { return prioritySwitchDelay; }
			set { prioritySwitchDelay = value; }
		}

		public int MinMemory
		{
			get { return minMemory; }
			set { minMemory = value; }
		}

		public int MaxMemory
		{
			get { return maxMemory; }
			set { maxMemory = value; }
		}

		public int LockGrantOrder
		{
			get { return lockGrantOrder; }
			set { lockGrantOrder = value; }
		}

		public int AnyLockMemory
		{
			get { return anyLockMemory; }
			set { anyLockMemory = value; }
		}

		public int AnyLockSemaphore
		{
			get { return anyLockSemaphore; }
			set { anyLockSemaphore = value; }
		}

		public int AnyLockSignal
		{
			get { return anyLockSignal; }
			set { anyLockSignal = value; }
		}

		public int AnyEventMemory
		{
			get { return anyEventMemory; }
			set { anyEventMemory = value; }
		}

		public int LockHashSlots
		{
			get { return lockHashSlots; }
			set { lockHashSlots = value; }
		}

		public int DeadlockTimeout
		{
			get { return deadlockTimeout; }
			set { deadlockTimeout = value; }
		}

		public int LockRequireSpins
		{
			get { return lockRequireSpins; }
			set { lockRequireSpins = value; }
		}

		public int ConnectionTimeout
		{
			get { return connectionTimeout; }
			set { connectionTimeout = value; }
		}

		public int DummyPacketInterval
		{
			get { return dummyPacketInterval; }
			set { dummyPacketInterval = value; }
		}

		public int IpcMapSize
		{
			get { return ipcMapSize; }
			set { ipcMapSize = value; }
		}

		public int DefaultDbCachePages
		{
			get { return defaultDbCachePages; }
			set { defaultDbCachePages = value; }
		}

		#endregion
	}
}
