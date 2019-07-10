// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace LightweightMetadata.Extensions
{
    internal static class PathSearchExtensions
    {
        [SuppressMessage("Design", "CA5350: Sha1 uses a weak cryptographic algorithm SHA1", Justification = "Deliberate usage.")]
        private static readonly SHA1 Sha1 = SHA1.Create();

        public static AssemblyReference Resolve(this AssemblyReferenceHandle reference, CompilationModule baseReader)
        {
            return baseReader.MetadataReader.GetAssemblyReference(reference);
        }

        public static CompilationModule Resolve(this AssemblyReference reference, ICompilation compilation, CompilationModule baseReader, TypeProvider typeProvider, IReadOnlyCollection<string> targetAssemblyDirectories)
        {
            var name = baseReader.MetadataReader.GetString(reference.Name);

            var fileName = GetFileName(reference, baseReader, targetAssemblyDirectories);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            return new CompilationModule(fileName, compilation, typeProvider);
        }

        public static string GetFileName(this AssemblyReference reference, CompilationModule baseReader, IEnumerable<string> searchDirectories)
        {
            var extensions = new[] { ".winmd", ".dll", ".exe" };

            var name = baseReader.MetadataReader.GetString(reference.Name);

            bool isWindowsRuntime = (reference.Flags & AssemblyFlags.WindowsRuntime) != 0;

            if (isWindowsRuntime)
            {
                return FindWindowsMetadataFile(name, reference.Version);
            }

            string file;

            if (name == "mscorlib")
            {
                file = GetCorlib(reference, baseReader);
                if (!string.IsNullOrWhiteSpace(file))
                {
                    return file;
                }
            }

            file = FindInParentDirectory(reference, baseReader, extensions);

            if (!string.IsNullOrWhiteSpace(file))
            {
                return file;
            }

            file = SearchDirectories(name, extensions, searchDirectories);

            if (!string.IsNullOrWhiteSpace(file))
            {
                return file;
            }

            return null;
        }

        private static string FindInParentDirectory(AssemblyReference reference, CompilationModule parent, IEnumerable<string> extensions)
        {
            if (parent == null)
            {
                return null;
            }

            string baseDirectory = Path.GetDirectoryName(parent.FileName);

            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                return null;
            }

            foreach (var extension in extensions)
            {
                string moduleFileName = Path.Combine(baseDirectory, reference.Name + extension);
                if (!File.Exists(moduleFileName))
                {
                    continue;
                }

                return moduleFileName;
            }

            return null;
        }

        private static string SearchDirectories(string name, IReadOnlyList<string> extensions, IEnumerable<string> directories)
        {
            foreach (var searchDirectory in directories)
            {
                foreach (var extension in extensions)
                {
                    var testName = Path.Combine(searchDirectory, name + extension);
                    if (string.IsNullOrWhiteSpace(testName))
                    {
                        continue;
                    }

                    if (!File.Exists(testName))
                    {
                        continue;
                    }

                    return testName;
                }
            }

            return null;
        }

        private static byte[] GetPublicKey(this AssemblyReference reference, CompilationModule compilation)
        {
            if (reference.PublicKeyOrToken.IsNil)
            {
                return Array.Empty<byte>();
            }

            var bytes = compilation.MetadataReader.GetBlobBytes(reference.PublicKeyOrToken);

            if ((reference.Flags & AssemblyFlags.PublicKey) != 0)
            {
                return Sha1.ComputeHash(bytes).Skip(12).ToArray();
            }

            return bytes;
        }

        private static string GetCorlib(AssemblyReference reference, CompilationModule compilation)
        {
            var version = reference.Version;
            var corlib = typeof(object).Assembly.GetName();

            if (corlib.Version == version || IsSpecialVersionOrRetargetable(reference))
            {
                return typeof(object).Module.FullyQualifiedName;
            }

            string path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = GetMscorlibBasePath(version, reference.GetPublicKey(compilation).ToHexString(8));
            }
            else
            {
                path = GetMonoMscorlibBasePath(version);
            }

            if (path == null)
            {
                return null;
            }

            var file = Path.Combine(path, "mscorlib.dll");
            if (File.Exists(file))
            {
                return file;
            }

            return null;
        }

        private static bool IsSpecialVersionOrRetargetable(AssemblyReference reference)
        {
            return IsZeroOrAllOnes(reference.Version) || (reference.Flags & AssemblyFlags.Retargetable) != 0;
        }

        private static string GetMscorlibBasePath(Version version, string publicKeyToken)
        {
            string GetSubFolderForVersion()
            {
                switch (version.Major)
                {
                    case 1:
                        if (version.MajorRevision == 3300)
                        {
                            return "v1.0.3705";
                        }

                        return "v1.1.4322";
                    case 2:
                        return "v2.0.50727";
                    case 4:
                        return "v4.0.30319";
                    default:
                        return null;
                }
            }

            if (publicKeyToken == "969db8053d3322ac")
            {
                string programFiles = Environment.Is64BitOperatingSystem ?
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) :
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string windowsCeDirectoryPath = $@"Microsoft.NET\SDK\CompactFramework\v{version.Major}.{version.Minor}\WindowsCE\";
                string fullDirectoryPath = Path.Combine(programFiles, windowsCeDirectoryPath);
                if (Directory.Exists(fullDirectoryPath))
                {
                    return fullDirectoryPath;
                }
            }
            else
            {
                string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET");
                string[] frameworkPaths =
                {
                    Path.Combine(rootPath, "Framework"),
                    Path.Combine(rootPath, "Framework64")
                };

                string folder = GetSubFolderForVersion();

                if (folder != null)
                {
                    foreach (var path in frameworkPaths)
                    {
                        var basePath = Path.Combine(path, folder);
                        if (Directory.Exists(basePath))
                        {
                            return basePath;
                        }
                    }
                }
            }

            return null;
        }

        private static string GetMonoMscorlibBasePath(Version version)
        {
            var path = Directory.GetParent(typeof(object).Module.FullyQualifiedName).Parent?.FullName;

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (version.Major == 1)
            {
                path = Path.Combine(path, "1.0");
            }
            else if (version.Major == 2)
            {
                if (version.MajorRevision == 5)
                {
                    path = Path.Combine(path, "2.1");
                }
                else
                {
                    path = Path.Combine(path, "2.0");
                }
            }
            else if (version.Major == 4)
            {
                path = Path.Combine(path, "4.0");
            }

            if (Directory.Exists(path))
            {
                return path;
            }

            return null;
        }

        private static bool IsZeroOrAllOnes(Version version)
        {
            return version == null
                   || (version.Major == 0 && version.Minor == 0 && version.Build == 0 && version.Revision == 0)
                   || (version.Major == 65535 && version.Minor == 65535 && version.Build == 65535 && version.Revision == 65535);
        }

        private static string FindWindowsMetadataFile(string name, Version version)
        {
            // This is only supported on windows at the moment.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "References");

            if (!Directory.Exists(basePath))
            {
                return FindWindowsMetadataInSystemDirectory(name);
            }

            basePath = Path.Combine(basePath, FindClosestVersionDirectory(basePath, version));

            if (!Directory.Exists(basePath))
            {
                return FindWindowsMetadataInSystemDirectory(name);
            }

            string file = Path.Combine(basePath, name + ".winmd");

            if (!File.Exists(file))
            {
                return FindWindowsMetadataInSystemDirectory(name);
            }

            return file;
        }

        private static string FindWindowsMetadataInSystemDirectory(string name)
        {
            string file = Path.Combine(Environment.SystemDirectory, "WinMetadata", name + ".winmd");
            if (File.Exists(file))
            {
                return file;
            }

            return null;
        }

        private static string FindClosestVersionDirectory(string basePath, Version version)
        {
            string path = null;
            foreach (var folder in new DirectoryInfo(basePath)
                .EnumerateDirectories()
                .Select(d => ConvertToVersion(d.Name))
                .Where(v => v.Item1 != null)
                .OrderByDescending(v => v.Item1))
            {
                if (path == null || folder.Item1 >= version)
                {
                    path = folder.Item2;
                }
            }

            return path ?? version.ToString();
        }

        private static string ToHexString(this IEnumerable<byte> bytes, int estimatedLength)
        {
            StringBuilder sb = new StringBuilder(estimatedLength * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return sb.ToString();
        }

        [SuppressMessage("Design", "CA1031: Modify to catch a more specific exception type, or rethrow the exception.", Justification = "Deliberate usage.")]
        private static (Version, string) ConvertToVersion(string name)
        {
            string RemoveTrailingVersionInfo()
            {
                string shortName = name;
                int dashIndex = shortName.IndexOf('-');
                if (dashIndex > 0)
                {
                    shortName = shortName.Remove(dashIndex);
                }

                return shortName;
            }

            try
            {
                return (new Version(RemoveTrailingVersionInfo()), name);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }
    }
}
