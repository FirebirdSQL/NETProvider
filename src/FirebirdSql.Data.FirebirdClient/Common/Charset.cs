﻿/*
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
using FbCharsetEnum = FirebirdSql.Data.FirebirdClient.FbCharset;

namespace FirebirdSql.Data.Common;

internal sealed class Charset
{
	internal const string Octets = "OCTETS";
	internal const string None = "NONE";

	private readonly static Dictionary<int, Charset> charsetsById;
	private readonly static Dictionary<string, Charset> charsetsByName;
	private readonly static Dictionary<FbCharsetEnum, Charset> charsetsByFbCharset;

	static Charset()
	{
		var charsets = GetSupportedCharsets();
		charsetsById = charsets.ToDictionary(x => x.Identifier);
		charsetsByName = charsets.ToDictionary(x => x.Name, StringComparer.CurrentCultureIgnoreCase);
		charsetsByFbCharset = charsets.Where(x => x.FbCharset != null).ToDictionary(x => x.FbCharset.Value);
	}

	public static Charset DefaultCharset => charsetsByName[None];

	public static bool TryGetById(int id, out Charset charset) => charsetsById.TryGetValue(id, out charset);

	public static bool TryGetByName(string name, out Charset charset) => charsetsByName.TryGetValue(name, out charset);

	public static bool TryGetByType(FbCharsetEnum fbCharset, out Charset charset) => charsetsByFbCharset.TryGetValue(fbCharset, out charset);

	private static List<Charset> GetSupportedCharsets()
	{
		var charsets = new List<Charset>();

		charsets.Add(new Charset(0, None, 1, None, FbCharsetEnum.None));
		charsets.Add(new Charset(1, Octets, 1, Octets, FbCharsetEnum.Octets));
		charsets.Add(new Charset(2, "ASCII", 1, "ascii", FbCharsetEnum.Ascii));
		charsets.Add(new Charset(3, "UNICODE_FSS", 3, "UTF-8")); //Don't set FbCharSet, UTF8 is preferred above UNICODE_FSS
		charsets.Add(new Charset(4, "UTF8", 4, "UTF-8", FbCharsetEnum.Utf8));

		TryAddCharset(charsets, () => new Charset(5, "SJIS_0208", 2, "shift_jis", FbCharsetEnum.ShiftJis0208));
		TryAddCharset(charsets, () => new Charset(6, "EUCJ_0208", 2, "euc-jp", FbCharsetEnum.EucJapanese0208));
		TryAddCharset(charsets, () => new Charset(7, "ISO2022-JP", 2, "iso-2022-jp", FbCharsetEnum.Iso2022Japanese));
		TryAddCharset(charsets, () => new Charset(10, "DOS437", 1, "IBM437", FbCharsetEnum.Dos437));
		TryAddCharset(charsets, () => new Charset(11, "DOS850", 1, "ibm850", FbCharsetEnum.Dos850));
		TryAddCharset(charsets, () => new Charset(12, "DOS865", 1, "IBM865", FbCharsetEnum.Dos865));
		TryAddCharset(charsets, () => new Charset(13, "DOS860", 1, "IBM860", FbCharsetEnum.Dos860));
		TryAddCharset(charsets, () => new Charset(14, "DOS863", 1, "IBM863", FbCharsetEnum.Dos863));
		TryAddCharset(charsets, () => new Charset(21, "ISO8859_1", 1, "iso-8859-1", FbCharsetEnum.Iso8859_1));
		TryAddCharset(charsets, () => new Charset(22, "ISO8859_2", 1, "iso-8859-2", FbCharsetEnum.Iso8859_2));
		TryAddCharset(charsets, () => new Charset(44, "KSC_5601", 2, "ks_c_5601-1987", FbCharsetEnum.Ksc5601));
		TryAddCharset(charsets, () => new Charset(47, "DOS861", 1, "ibm861", FbCharsetEnum.Dos861));
		TryAddCharset(charsets, () => new Charset(51, "WIN1250", 1, "windows-1250", FbCharsetEnum.Windows1250));
		TryAddCharset(charsets, () => new Charset(52, "WIN1251", 1, "windows-1251", FbCharsetEnum.Windows1251));
		TryAddCharset(charsets, () => new Charset(53, "WIN1252", 1, "windows-1252", FbCharsetEnum.Windows1252));
		TryAddCharset(charsets, () => new Charset(54, "WIN1253", 1, "windows-1253", FbCharsetEnum.Windows1253));
		TryAddCharset(charsets, () => new Charset(55, "WIN1254", 1, "windows-1254", FbCharsetEnum.Windows1254));
		TryAddCharset(charsets, () => new Charset(56, "BIG_5", 2, "big5", FbCharsetEnum.Big5));
		TryAddCharset(charsets, () => new Charset(57, "GB_2312", 2, "gb2312", FbCharsetEnum.Gb2312));
		TryAddCharset(charsets, () => new Charset(58, "WIN1255", 1, "windows-1255", FbCharsetEnum.Windows1255));
		TryAddCharset(charsets, () => new Charset(59, "WIN1256", 1, "windows-1256", FbCharsetEnum.Windows1256));
		TryAddCharset(charsets, () => new Charset(60, "WIN1257", 1, "windows-1257", FbCharsetEnum.Windows1257));
		//TryAddCharset(charsets, () => new Charset(61, "UTF16", 4, "utf-16"));
		//TryAddCharset(charsets, () => new Charset(62, "UTF32", 4, "utf-32"));
		TryAddCharset(charsets, () => new Charset(63, "KOI8R", 2, "koi8-r", FbCharsetEnum.Koi8R));
		TryAddCharset(charsets, () => new Charset(64, "KOI8U", 2, "koi8-u", FbCharsetEnum.Koi8U));
		TryAddCharset(charsets, () => new Charset(65, "TIS620", 1, "tis-620", FbCharsetEnum.TIS620));

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

	public int Identifier { get; }
	public string Name { get; }
	public string SystemName { get; private set; }
	public int BytesPerCharacter { get; }
	public Encoding Encoding { get; }
	public bool IsOctetsCharset { get; }
	public bool IsNoneCharset { get; }
	public FbCharsetEnum? FbCharset { get; }

	public Charset(int id, string name, int bytesPerCharacter, string systemName, FbCharsetEnum? fbCharset = null)
	{
		Identifier = id;
		Name = name;
		BytesPerCharacter = bytesPerCharacter;
		SystemName = systemName;
		IsNoneCharset = false;
		IsOctetsCharset = false;
		FbCharset = fbCharset;
		switch (SystemName)
		{
			case None:
				Encoding = Encoding2.Default;
				IsNoneCharset = true;
				break;
			case Octets:
				Encoding = new BinaryEncoding();
				IsOctetsCharset = true;
				break;
			default:
				Encoding = Encoding.GetEncoding(SystemName);
				break;
		}
	}

	public byte[] GetBytes(string s)
	{
		return Encoding.GetBytes(s);
	}

	public int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		return Encoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);
	}

	public string GetString(byte[] buffer)
	{
		return Encoding.GetString(buffer);
	}

	public string GetString(byte[] buffer, int index, int count)
	{
		return Encoding.GetString(buffer, index, count);
	}
}
