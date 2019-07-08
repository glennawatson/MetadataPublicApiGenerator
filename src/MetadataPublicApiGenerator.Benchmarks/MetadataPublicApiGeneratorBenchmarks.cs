﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace MetadataPublicApiGenerator.Benchmarks
{
    /// <summary>
    /// Benchmarks associated with the MetadataPublicApiGenerator object.
    /// </summary>
    [CoreJob]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class MetadataPublicApiGeneratorBenchmarks
    {
        /// <summary>
        /// Benchmark for when adding a item to a ReactiveList.
        /// </summary>
        [Benchmark]
        public void MetadataApiGenerator() => ApiGenerator.GeneratePublicApi(typeof(string).Assembly);

        /// <summary>
        /// Benchmark for when adding a item to a ReactiveList.
        /// </summary>
        [Benchmark]
        public void PublicApiGenerator() => global::PublicApiGenerator.ApiGenerator.GeneratePublicApi(typeof(string).Assembly);
    }
}
