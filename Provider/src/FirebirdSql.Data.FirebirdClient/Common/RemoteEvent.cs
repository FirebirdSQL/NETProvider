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
 *
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections.Generic;

namespace FirebirdSql.Data.Common
{
	internal class RemoteEvent
	{
		#region Callbacks

		public RemoteEventCountsCallback EventCountsCallback { get; set; }

		#endregion

		#region Fields

		private List<string> _events;
		private IDatabase _db;
		private int _localId;
		private int _remoteId;
		private bool _initialCounts;
		private int[] _previousCounts;
		private int[] _actualCounts;

		#endregion

		#region Properties

		public int LocalId
		{
			get { return _localId; }
			set { _localId = value; }
		}

		public int RemoteId
		{
			get { return _remoteId; }
			set { _remoteId = value; }
		}

		public List<string> Events
		{
			get { return _events; }
		}

		public int[] PreviousCounts
		{
			get { return _previousCounts; }
		}

		public int[] ActualCounts
		{
			get { return _actualCounts; }
		}

		#endregion

		#region Constructors

		public RemoteEvent(IDatabase db)
		{
			_localId = 0;
			_remoteId = 0;
			_events = new List<string>();
			_db = db;
		}

		#endregion

		#region Methods

		public void QueueEvents()
		{
			_db.QueueEvents(this);
		}

		public void CancelEvents()
		{
			_db.CancelEvents(this);
			ResetCounts();
		}

		public void ResetCounts()
		{
			_initialCounts = false;
			_actualCounts = null;
			_previousCounts = null;
		}

		public void EventCounts(byte[] buffer)
		{
			int pos = 1;
			Charset charset = _db.Charset;

			if (buffer != null)
			{
				if (_initialCounts)
				{
					_previousCounts = _actualCounts;
				}

				_actualCounts = new int[_events.Count];

				while (pos < buffer.Length)
				{
					int length = buffer[pos++];
					string eventName = charset.GetString(buffer, pos, length);

					pos += length;

					int index = _events.IndexOf(eventName);
					if (index != -1)
					{
						_actualCounts[index] = BitConverter.ToInt32(buffer, pos) - 1;
					}

					pos += 4;
				}

				if (!_initialCounts)
				{
					QueueEvents();
					_initialCounts = true;
				}
				else
				{
					EventCountsCallback?.Invoke();
				}
			}
		}

		public EventParameterBuffer ToEpb()
		{
			EventParameterBuffer epb = new EventParameterBuffer();
			epb.Append(IscCodes.EPB_version1);
			for (int i = 0; i < _events.Count; i++)
			{
				if (_actualCounts != null)
				{
					epb.Append(_events[i], _actualCounts[i] + 1);
				}
				else
				{
					epb.Append(_events[i], 0);
				}
			}
			return epb;
		}

		#endregion
	}
}
