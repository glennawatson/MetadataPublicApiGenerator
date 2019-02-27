// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
        private Assembly _assembly;

        /// <summary>
        /// Setup for all benchmark instances being run.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _assembly = typeof(JsonConvert).Assembly;
        }

        /// <summary>
        /// Benchmark for when adding a item to a ReactiveList.
        /// </summary>
        [Benchmark]
        public void MetadataApiGenerator() => ApiGenerator.GeneratePublicApi(_assembly);

        /// <summary>
        /// Benchmark for when adding a item to a ReactiveList.
        /// </summary>
        [Benchmark]
        public void PublicApiGenerator() => global::PublicApiGenerator.ApiGenerator.GeneratePublicApi(_assembly);
    }
}
