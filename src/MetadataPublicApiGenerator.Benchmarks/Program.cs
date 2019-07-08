// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace MetadataPublicApiGenerator.Benchmarks
{
    /// <summary>
    /// Class which hosts the main entry point into the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point into the benchmarking application.
        /// </summary>
        /// <param name="args">Arguments from the command line.</param>
        public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
    }
}
