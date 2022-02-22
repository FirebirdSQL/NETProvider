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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace FirebirdSql.Data.FirebirdClient;

[ListBindable(false)]
public sealed class FbBatchParameterCollection : IList<FbParameterCollection>
{
	readonly List<FbParameterCollection> _inner;

	internal FbBatchParameterCollection()
	{
		_inner = new List<FbParameterCollection>();
	}

	public FbParameterCollection this[int index]
	{
		get => _inner[index];
		set => _inner[index] = value;
	}

	public int Count => _inner.Count;

	public bool IsReadOnly => false;

	public void Add(FbParameterCollection item) => _inner.Add(item);

	public void Clear() => _inner.Clear();

	public bool Contains(FbParameterCollection item) => _inner.Contains(item);

	public void CopyTo(FbParameterCollection[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

	public IEnumerator<FbParameterCollection> GetEnumerator() => _inner.GetEnumerator();

	public int IndexOf(FbParameterCollection item) => _inner.IndexOf(item);

	public void Insert(int index, FbParameterCollection item) => _inner.Insert(index, item);

	public bool Remove(FbParameterCollection item) => _inner.Remove(item);

	public void RemoveAt(int index) => _inner.RemoveAt(index);

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
