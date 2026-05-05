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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FirebirdSql.Data.Common;

// Comma-separated list of SQL LIKE patterns used to match Firebird domain
// names (RDB$FIELD_SOURCE). '%' matches any sequence of characters; '_'
// matches a single character. Matching is case-insensitive. System
// domains (RDB$ prefix) are never matched.
internal sealed class DomainPatternList
{
	public static DomainPatternList Empty { get; } = new(Array.Empty<Regex>());

	private readonly Regex[] _patterns;

	private DomainPatternList(Regex[] patterns)
	{
		_patterns = patterns;
	}

	public bool HasAny => _patterns.Length > 0;

	public static DomainPatternList Parse(string spec)
	{
		if (string.IsNullOrWhiteSpace(spec))
			return Empty;

		var parts = spec.Split(',');
		var compiled = new List<Regex>(parts.Length);
		foreach (var raw in parts)
		{
			var token = raw.Trim();
			if (token.Length == 0)
				continue;
			compiled.Add(CompilePattern(token));
		}
		return compiled.Count == 0 ? Empty : new DomainPatternList(compiled.ToArray());
	}

	public bool Matches(string domainName)
	{
		if (_patterns.Length == 0)
			return false;
		if (string.IsNullOrEmpty(domainName))
			return false;
		var trimmed = domainName.Trim();
		if (trimmed.Length == 0)
			return false;
		if (trimmed.StartsWith("RDB$", StringComparison.OrdinalIgnoreCase))
			return false;
		foreach (var regex in _patterns)
		{
			if (regex.IsMatch(trimmed))
				return true;
		}
		return false;
	}

	private static Regex CompilePattern(string pattern)
	{
		var sb = new StringBuilder(pattern.Length + 8);
		sb.Append('^');
		for (var i = 0; i < pattern.Length; i++)
		{
			var c = pattern[i];
			switch (c)
			{
				case '%':
					sb.Append(".*");
					break;
				case '_':
					sb.Append('.');
					break;
				default:
					sb.Append(Regex.Escape(c.ToString()));
					break;
			}
		}
		sb.Append('$');
		return new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
