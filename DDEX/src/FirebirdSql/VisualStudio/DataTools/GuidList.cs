/*
 *  Visual Studio DDEX Provider for FirebirdClient
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
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

// Guids.cs
// MUST match guids.h

using System;

namespace FirebirdSql.VisualStudio.DataTools
{
    static class GuidList
    {
        public const string GuidDataToolsPkgString = "8d9358ba-ccc9-4169-9fd6-a52b8aee2d50";
        public const string GuidObjectFactoryServiceString = "AEF32AEC-2167-4438-81FF-AE6603341536";

        public static readonly Guid GuidDataToolsPkg = new Guid(GuidDataToolsPkgString);
        public static readonly Guid GuidObjectFactoryService = new Guid(GuidObjectFactoryServiceString);
    };
}