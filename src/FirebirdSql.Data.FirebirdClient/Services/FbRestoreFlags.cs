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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Services;

[Flags]
public enum FbRestoreFlags
{
	DeactivateIndexes = IscCodes.isc_spb_res_deactivate_idx,
	NoShadow = IscCodes.isc_spb_res_no_shadow,
	NoValidity = IscCodes.isc_spb_res_no_validity,
	IndividualCommit = IscCodes.isc_spb_res_one_at_a_time,
	Replace = IscCodes.isc_spb_res_replace,
	Create = IscCodes.isc_spb_res_create,
	UseAllSpace = IscCodes.isc_spb_res_use_all_space,
	MetaDataOnly = IscCodes.isc_spb_res_metadata_only,
}
