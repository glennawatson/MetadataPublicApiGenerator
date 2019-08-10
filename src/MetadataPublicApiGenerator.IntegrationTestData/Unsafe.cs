﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public class Unsafe
    {
        public unsafe void* GenerateData() => default;

        public unsafe void GenerateData(int* data)
        {
        }

        public unsafe int* member;

        public unsafe int* MemberProperty { get; }

        public unsafe delegate void* DoStuff(void* buffer);
    }
}
