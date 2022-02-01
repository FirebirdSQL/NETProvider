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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebirdSql.Data.Common;

internal sealed class Charset
{
	#region Constants

	internal const string Octets = "OCTETS";
	internal const string None = "NONE";

	#endregion

	#region Statics

	private readonly static Dictionary<int, Charset> charsetsById;
	private readonly static Dictionary<string, Charset> charsetsByName;

	static Charset()
	{
		var charsets = GetSupportedCharsets();
		charsetsById = charsets.ToDictionary(x => x.Identifier);
		charsetsByName = charsets.ToDictionary(x => x.Name, StringComparer.CurrentCultureIgnoreCase);
	}

	public static Charset DefaultCharset
	{
		get { return charsetsById.First().Value; }
	}

	public static Charset GetCharset(int charsetId)
	{
		return charsetsById.TryGetValue(charsetId, out var value) ? value : null;
	}

	public static Charset GetCharset(string charsetName)
	{
		return charsetsByName.TryGetValue(charsetName, out var value) ? value : null;
	}

	private static List<Charset> GetSupportedCharsets()
	{
		var charsets = new List<Charset>();

		charsets.Add(new Charset(0, None, 1, None));
		charsets.Add(new Charset(1, Octets, 1, Octets));
		charsets.Add(new Charset(2, "ASCII", 1, "ascii"));
		charsets.Add(new Charset(3, "UNICODE_FSS", 3, "UTF-8"));
		charsets.Add(new Charset(4, "UTF8", 4, "UTF-8"));

		TryAddCharset(charsets, () => new Charset(5, "SJIS_0208", 2, "shift_jis"));
		TryAddCharset(charsets, () => new Charset(6, "EUCJ_0208", 2, "euc-jp"));
		TryAddCharset(charsets, () => new Charset(7, "ISO2022-JP", 2, "iso-2022-jp"));
		TryAddCharset(charsets, () => new Charset(10, "DOS437", 1, "IBM437"));
		TryAddCharset(charsets, () => new Charset(11, "DOS850", 1, "ibm850"));
		TryAddCharset(charsets, () => new Charset(12, "DOS865", 1, "IBM865"));
		TryAddCharset(charsets, () => new Charset(13, "DOS860", 1, "IBM860"));
		TryAddCharset(charsets, () => new Charset(14, "DOS863", 1, "IBM863"));
		TryAddCharset(charsets, () => new Charset(21, "ISO8859_1", 1, "iso-8859-1"));
		TryAddCharset(charsets, () => new Charset(22, "ISO8859_2", 1, "iso-8859-2"));
		TryAddCharset(charsets, () => new Charset(44, "KSC_5601", 2, "ks_c_5601-1987"));
		TryAddCharset(charsets, () => new Charset(47, "DOS861", 1, "ibm861"));
		TryAddCharset(charsets, () => new Charset(51, "WIN1250", 1, "windows-1250"));
		TryAddCharset(charsets, () => new Charset(52, "WIN1251", 1, "windows-1251"));
		TryAddCharset(charsets, () => new Charset(53, "WIN1252", 1, "windows-1252"));
		TryAddCharset(charsets, () => new Charset(54, "WIN1253", 1, "windows-1253"));
		TryAddCharset(charsets, () => new Charset(55, "WIN1254", 1, "windows-1254"));
		TryAddCharset(charsets, () => new Charset(56, "BIG_5", 2, "big5"));
		TryAddCharset(charsets, () => new Charset(57, "GB_2312", 2, "gb2312"));
		TryAddCharset(charsets, () => new Charset(58, "WIN1255", 1, "windows-1255"));
		TryAddCharset(charsets, () => new Charset(59, "WIN1256", 1, "windows-1256"));
		TryAddCharset(charsets, () => new Charset(60, "WIN1257", 1, "windows-1257"));
		//TryAddCharset(charsets, () => new Charset(61, "UTF16", 4, "utf-16"));
		//TryAddCharset(charsets, () => new Charset(62, "UTF32", 4, "utf-32"));
		TryAddCharset(charsets, () => new Charset(63, "KOI8R", 2, "koi8-r"));
		TryAddCharset(charsets, () => new Charset(64, "KOI8U", 2, "koi8-u"));
		TryAddCharset(charsets, () => new Charset(65, "TIS620", 1, "tis-620"));

		return charsets;
	}

	private static void TryAddCharset(List<Charset> charsets, Func<Charset> charsetCreator)
	{
		try
		{
			charsets.Add(charsetCreator());
		}
		catch
		{ }
	}

	#endregion

	#region Fields

	private int _id;
	private int _bytesPerCharacter;
	private string _name;
	private string _systemName;
	private Encoding _encoding;
	private bool _isNone;
	private bool _isOctets;

	#endregion

	#region Properties

	public int Identifier
	{
		get { return _id; }
	}

	public string Name
	{
		get { return _name; }
	}

	public int BytesPerCharacter
	{
		get { return _bytesPerCharacter; }
	}

	public bool IsOctetsCharset
	{
		get { return _isOctets; }
	}

	public bool IsNoneCharset
	{
		get { return _isNone; }
	}

	#endregion

	#region Constructors

	public Charset(int id, string name, int bytesPerCharacter, string systemName)
	{
		_id = id;
		_name = name;
		_bytesPerCharacter = bytesPerCharacter;
		_systemName = systemName;
		_isNone = false;
		_isOctets = false;
		switch (_systemName)
		{
			case None:
				_encoding = Encoding2.Default;
				_isNone = true;
				break;
			case Octets:
				_encoding = new BinaryEncoding();
				_isOctets = true;
				break;
			default:
				_encoding = Encoding.GetEncoding(_systemName);
				break;
		}
	}

	#endregion

	#region Methods

	public byte[] GetBytes(string s)
	{
		return _encoding.GetBytes(s);
	}

	public int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		return _encoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);
	}

	public string GetString(byte[] buffer)
	{
		return GetString(buffer, 0, buffer.Length);
	}

	public string GetString(byte[] buffer, int index, int count)
	{
		return _encoding.GetString(buffer, index, count);
	}

	#endregion
}
