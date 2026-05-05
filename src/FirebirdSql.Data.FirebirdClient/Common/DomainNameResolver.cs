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

//$Authors = Ebubekir Cagri Sen (ebubekircagrisen@gmail.com)

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Common;

// Resolves domain names (RDB$FIELD_SOURCE) for the (relation, field) pairs
// of a prepared statement and applies any configured DbDataType override
// to the matched DbField. Cache is per FbConnectionInternal; an
// _isResolving sentinel suppresses re-entry while the resolver itself
// runs an internal SELECT against RDB$RELATION_FIELDS. Fetch failures
// are swallowed (best-effort): a feature opted in via the connection
// string must never break the user's normal queries.
internal sealed class DomainNameResolver
{
	private readonly Dictionary<(string Relation, string Field), string> _cache = new();
	// volatile: the flag is read/written across async continuations that may resume
	// on different threads. Without volatile the compiler or JIT may cache the value
	// in a register and miss the reset in the finally block.
	private volatile bool _isResolving;
	private int _fetchCount;

	public bool IsResolving => _isResolving;
	public int FetchCount => _fetchCount;

	public void Resolve(FbConnection connection, IEnumerable<DbField> fields,
						IReadOnlyDictionary<DbDataType, DomainPatternList> mappings)
	{
		if (_isResolving)
			return;
		if (mappings == null || mappings.Count == 0)
			return;

		var collected = Collect(fields);
		if (collected.Count == 0)
			return;

		var needed = NotInCache(collected.Keys);
		if (needed.Count > 0)
		{
			_isResolving = true;
			try
			{
				FetchDomainNames(connection, needed);
			}
			finally
			{
				_isResolving = false;
			}
		}

		ApplyOverrides(collected, mappings);
	}

	public async ValueTask ResolveAsync(FbConnection connection, IEnumerable<DbField> fields,
										IReadOnlyDictionary<DbDataType, DomainPatternList> mappings,
										CancellationToken cancellationToken = default)
	{
		if (_isResolving)
			return;
		if (mappings == null || mappings.Count == 0)
			return;

		var collected = Collect(fields);
		if (collected.Count == 0)
			return;

		var needed = NotInCache(collected.Keys);
		if (needed.Count > 0)
		{
			_isResolving = true;
			try
			{
				await FetchDomainNamesAsync(connection, needed, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				_isResolving = false;
			}
		}

		ApplyOverrides(collected, mappings);
	}

	private static Dictionary<(string Relation, string Field), List<DbField>> Collect(IEnumerable<DbField> fields)
	{
		var result = new Dictionary<(string, string), List<DbField>>();
		if (fields == null)
			return result;
		foreach (var f in fields)
		{
			if (f == null)
				continue;
			var rel = f.Relation;
			var name = f.Name;
			if (string.IsNullOrEmpty(rel) || string.IsNullOrEmpty(name))
				continue;
			var key = (rel.Trim(), name.Trim());
			if (key.Item1.Length == 0 || key.Item2.Length == 0)
				continue;
			if (!result.TryGetValue(key, out var list))
			{
				list = new List<DbField>();
				result[key] = list;
			}
			list.Add(f);
		}
		return result;
	}

	private List<(string Relation, string Field)> NotInCache(IEnumerable<(string Relation, string Field)> keys)
	{
		var result = new List<(string, string)>();
		foreach (var k in keys)
		{
			if (!_cache.ContainsKey(k))
				result.Add(k);
		}
		return result;
	}

	private void ApplyOverrides(Dictionary<(string Relation, string Field), List<DbField>> collected,
								IReadOnlyDictionary<DbDataType, DomainPatternList> mappings)
	{
		foreach (var entry in collected)
		{
			if (!_cache.TryGetValue(entry.Key, out var domain) || string.IsNullOrEmpty(domain))
				continue;
			DbDataType? matched = null;
			foreach (var m in mappings)
			{
				if (m.Value.Matches(domain))
				{
					matched = m.Key;
					break;
				}
			}
			foreach (var field in entry.Value)
			{
				field.DomainName = domain;
				if (matched.HasValue)
					field.OverrideDataType = matched.Value;
			}
		}
	}

	private void FetchDomainNames(FbConnection connection, List<(string Relation, string Field)> needed)
	{
		// Negative cache: if the fetch fails or rows are missing, the entry stays null.
		foreach (var k in needed)
			_cache[k] = null;

		_fetchCount++;
		try
		{
			var sql = BuildQuery(needed.Count);
			var activeTransaction = connection.InnerConnection?.ActiveTransaction;
			using (var cmd = new FbCommand(sql, connection, activeTransaction))
			{
				AddParameters(cmd, needed);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var rel = reader.IsDBNull(0) ? null : reader.GetString(0);
						var fld = reader.IsDBNull(1) ? null : reader.GetString(1);
						var dom = reader.IsDBNull(2) ? null : reader.GetString(2);
						if (rel != null && fld != null)
							_cache[(rel.Trim(), fld.Trim())] = dom?.Trim();
					}
				}
			}
		}
		catch
		{
			// Best-effort: an opt-in feature must never break user's normal queries.
			// Cache is already populated with negative entries above, so subsequent
			// prepares for the same columns won't keep re-trying within this session.
		}
	}

	private async ValueTask FetchDomainNamesAsync(FbConnection connection, List<(string Relation, string Field)> needed,
												  CancellationToken cancellationToken)
	{
		foreach (var k in needed)
			_cache[k] = null;

		_fetchCount++;
		try
		{
			var sql = BuildQuery(needed.Count);
			var activeTransaction = connection.InnerConnection?.ActiveTransaction;
			var cmd = new FbCommand(sql, connection, activeTransaction);
			await using (cmd.ConfigureAwait(false))
			{
				AddParameters(cmd, needed);
				var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
				await using (reader.ConfigureAwait(false))
				{
					while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
					{
						var rel = await reader.IsDBNullAsync(0, cancellationToken).ConfigureAwait(false) ? null : reader.GetString(0);
						var fld = await reader.IsDBNullAsync(1, cancellationToken).ConfigureAwait(false) ? null : reader.GetString(1);
						var dom = await reader.IsDBNullAsync(2, cancellationToken).ConfigureAwait(false) ? null : reader.GetString(2);
						if (rel != null && fld != null)
							_cache[(rel.Trim(), fld.Trim())] = dom?.Trim();
					}
				}
			}
		}
		catch
		{
			// Best-effort. See sync counterpart.
		}
	}

	private static string BuildQuery(int count)
	{
		var sb = new StringBuilder();
		sb.Append("SELECT TRIM(rfr.RDB$RELATION_NAME), TRIM(rfr.RDB$FIELD_NAME), TRIM(rfr.RDB$FIELD_SOURCE) ");
		sb.Append("FROM RDB$RELATION_FIELDS rfr WHERE ");
		for (var i = 0; i < count; i++)
		{
			if (i > 0)
				sb.Append(" OR ");
			sb.Append("(rfr.RDB$RELATION_NAME = @r");
			sb.Append(i);
			sb.Append(" AND rfr.RDB$FIELD_NAME = @f");
			sb.Append(i);
			sb.Append(')');
		}
		return sb.ToString();
	}

	private static void AddParameters(FbCommand cmd, List<(string Relation, string Field)> needed)
	{
		for (var i = 0; i < needed.Count; i++)
		{
			cmd.Parameters.Add("@r" + i, needed[i].Relation);
			cmd.Parameters.Add("@f" + i, needed[i].Field);
		}
	}

	public void Clear()
	{
		_cache.Clear();
		_fetchCount = 0;
	}
}
