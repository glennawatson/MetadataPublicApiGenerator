// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MetadataPublicApiGenerator.Tests
{
    /// <summary>
    /// A test helper for producing roslyn based test code.
    /// </summary>
    public static class RoslynTestHelper
    {
        private static readonly IList<MetadataReference> _assemblyPaths;

        static RoslynTestHelper()
        {
            var netstandardDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "netstandard.library", "2.0.0", "build", "netstandard2.0", "ref");

            var filePaths = Directory.EnumerateFiles(netstandardDirectory, "*.dll", SearchOption.AllDirectories);

            _assemblyPaths = filePaths
                .Distinct()
                .Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
                .ToList();
        }

        /// <summary>
        /// Checks to make sure tha the specified code produces the expected API.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <param name="expectedApi">The API expected to be produced.</param>
        public static void CheckApi(string code, string expectedApi)
        {
            string filePath = null;
            try
            {
                filePath = CreateAssembly(code);

                var publicApi = MetadataApi.GeneratePublicApi(filePath).Trim();

                publicApi.Should().Be(expectedApi);
            }
            finally
            {
                if (filePath != null)
                {
                    File.Delete(filePath);
                }
            }
        }

        /// <summary>
        /// Creates a assembly given the set of code. It will use the same assembly references as included in the tests.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <returns>The compiled assembly path.</returns>
        private static string CreateAssembly(string code)
        {
            // create the syntax tree
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(code);

            var sourceLanguage = CSharpCompilation.Create("Test", new[] { syntaxTree }, _assemblyPaths, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));

            var assemblyPath = Path.GetTempFileName();

            using (var writeStream = new FileStream(assemblyPath, FileMode.Create))
            {
                var result = sourceLanguage.Emit(writeStream);

                if (!result.Success)
                {
                    throw new Exception("Our compilation failed to compile: \r\n" + string.Join("\r\n", result.Diagnostics.Select(x => x.ToString())));
                }
            }

            return assemblyPath;
        }
    }
}
