// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;

namespace MetadataPublicApiGenerator.Tests
{
    internal static class TestHelpers
    {
        private static readonly Regex _spaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public static void CheckEquals(string publicApi, string expectedApi, string receivedFileName, string approvedFile)
        {
            string fixedStringOne = _spaceRegex.Replace(publicApi, string.Empty);
            string fixedStringTwo = _spaceRegex.Replace(expectedApi, string.Empty);

            if (!string.Equals(fixedStringOne, fixedStringTwo, StringComparison.InvariantCulture))
            {
                try
                {
                    ShouldlyConfiguration.DiffTools.GetDiffTool().Open(receivedFileName, approvedFile, true);
                }
                catch (ShouldAssertException)
                {
                }

                publicApi.ShouldBe(expectedApi);
            }
        }

        public static string RemoveWhitespace(this string stringValue)
        {
            string[] lines = stringValue.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None);

            var output = new StringBuilder(stringValue.Length);

            bool contentSeen = false;

            foreach (var line in lines)
            {
                if (!contentSeen && string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!contentSeen && !string.IsNullOrWhiteSpace(line))
                {
                    contentSeen = true;
                }

                output.AppendLine(line.TrimEnd());
            }

            return output.ToString();
        }
    }
}
