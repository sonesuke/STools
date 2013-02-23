// Guids.cs
// MUST match guids.h
using System;

namespace S2.STools
{
    static class GuidList
    {
        public const string guidSToolsPkgString = "df12694e-50e9-4174-b556-ff34cae42223";
        public const string guidSToolsCmdSetString = "096616e7-26a9-482d-a91b-d1389e0c0bd9";

        public static readonly Guid guidSToolsCmdSet = new Guid(guidSToolsCmdSetString);
    };
}