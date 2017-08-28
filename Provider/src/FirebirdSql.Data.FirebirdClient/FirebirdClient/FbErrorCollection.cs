/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  Copyright (c) 2012, 2015 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	[Serializable]
#if !NETSTANDARD1_6
	[ListBindable(false)]
#endif
	public sealed class FbErrorCollection : ICollection<FbError>
	{
		#region Fields

		private List<FbError> _errors;

		#endregion

		#region Constructors

		internal FbErrorCollection()
		{
			_errors = new List<FbError>();
		}

		#endregion

		#region Properties

		public int Count
		{
			get { return _errors.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Methods

		internal int IndexOf(string errorMessage)
		{
			return _errors.FindIndex(x => string.Equals(x.Message, errorMessage, StringComparison.CurrentCultureIgnoreCase));
		}

		internal FbError Add(FbError error)
		{
			_errors.Add(error);

			return error;
		}

		internal FbError Add(string errorMessage, int number)
		{
			return Add(new FbError(errorMessage, number));
		}

		void ICollection<FbError>.Add(FbError item)
		{
			throw new NotSupportedException();
		}

		void ICollection<FbError>.Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(FbError item)
		{
			return _errors.Contains(item);
		}

		public void CopyTo(FbError[] array, int arrayIndex)
		{
			_errors.CopyTo(array, arrayIndex);
		}

		bool ICollection<FbError>.Remove(FbError item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<FbError> GetEnumerator()
		{
			return _errors.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
