﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public unsafe struct ArrayTypes
    {
        public static readonly int[] Standard = new int[64];

        public unsafe fixed int FixedSize[64];

        public int[][] MultipleDimensions2;
        public int[][][] MultipleDimensions3;

        public int[,] Jagged;

        public int[][,] SemiJagged;

        public int[,][] SemiJagged2;

        public const int[] InitializedField = null;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public static void Test(int[] normal, int[,] jagged, int[][] multiple2, int[][][] multiple3, int[,][] semiJagged1, int[][,] semiJagged2, int[] x = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
        }
    }
}
