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