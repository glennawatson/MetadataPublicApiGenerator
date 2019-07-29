// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private static readonly string _rootDirectory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.FullName, "MetadataPublicApiGenerator.IntegrationTestData");

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
        /// <param name="filePrefix">The prefix to the code..</param>
        /// <param name="filePath">The file path to the calling method.</param>
        public static void CheckApi(string filePrefix, [CallerFilePath] string filePath = null)
        {
            string assemblyFilePath = null;
            try
            {
                var codeFilePath = Path.Combine(_rootDirectory, filePrefix + ".cs");
                var apiFilePath = Path.Combine(_rootDirectory, filePrefix + ".txt");

                var code = File.ReadAllText(codeFilePath);
                assemblyFilePath = CreateAssembly(code);

                var publicApi = MetadataApi.GeneratePublicApi(assemblyFilePath).Trim();
                var expectedApi = File.ReadAllText(apiFilePath);
                publicApi.Should().Be(expectedApi);
            }
            finally
            {
                if (assemblyFilePath != null)
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
