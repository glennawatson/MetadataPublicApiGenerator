// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LightweightMetadata
{
    internal static class AssemblyLoadingHelper
    {
        private static readonly string[] Extensions = new string[] { ".winmd", ".dll", ".exe" };

        private static readonly string DefaultNuGetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        private static readonly ConcurrentDictionary<string, AssemblyMetadata> _fileNameToModule = new ConcurrentDictionary<string, AssemblyMetadata>(StringComparer.InvariantCultureIgnoreCase);

        private static readonly ConcurrentDictionary<string?, AssemblyMetadata?> _nameToModule
            = new ConcurrentDictionary<string?, AssemblyMetadata?>();

        public static AssemblyMetadata? ResolveCompilationModule(string name, AssemblyMetadata parent, Version? version = null, bool isWindowsRuntime = false, bool isRetargetable = false, string? publicKey = null)
        {
            return _nameToModule.GetOrAdd(
                name,
                _ =>
                {
                    var searchDirectories = parent.MetadataRepository.SearchDirectories;

                    var fileName = GetFileName(name, version, parent, searchDirectories, isWindowsRuntime, isRetargetable, publicKey);

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        return null;
                    }

                    return _fileNameToModule.GetOrAdd(fileName!, __ => new AssemblyMetadata(fileName!, parent.MetadataRepository, parent.TypeProvider));
                });
        }

        private static string? GetFileName(string name, Version? version, AssemblyMetadata baseReader, IEnumerable<string> searchDirectories, bool isWindowsRuntime, bool isRetargetable, string? publicKey)
        {
            if (isWindowsRuntime)
            {
                return FindWindowsMetadataFile(name, version);
            }

            string? file;

            if (name == "mscorlib" && version != null && publicKey != null)
            {
                file = GetCorlib(version, isRetargetable, publicKey);
                if (!string.IsNullOrWhiteSpace(file))
                {
                    return file;
                }
            }

            file = FindInParentDirectory(name, baseReader, Extensions);

            if (!string.IsNullOrWhiteSpace(file))
            {
                return file;
            }

            file = SearchDirectories(name, Extensions, searchDirectories);

            if (!string.IsNullOrWhiteSpace(file))
            {
                return file;
            }

            file = FindInNuGetDirectory(name, Extensions);

            if (!string.IsNullOrWhiteSpace(file))
            {
                return file;
            }

            return null;
        }

        private static string? FindInParentDirectory(string name, AssemblyMetadata? parent, IEnumerable<string> extensions)
        {
            if (parent is null)
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
                string moduleFileName = Path.Combine(baseDirectory, name + extension);
                if (!File.Exists(moduleFileName))
                {
                    continue;
                }

                return moduleFileName;
            }

            return null;
        }

        private static string? SearchDirectories(string name, IReadOnlyList<string> extensions, IEnumerable<string> directories)
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

        private static string? GetCorlib(Version? version, bool isRetargetable, string? publicKey)
        {
            var corlib = typeof(object).Assembly.GetName();

            if (corlib.Version == version || (IsZeroOrAllOnes(version) || isRetargetable))
            {
                return typeof(object).Module.FullyQualifiedName;
            }

            string? path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = GetMscorlibBasePath(version, publicKey);
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
            return File.Exists(file) ? file : null;
        }

        private static string? GetMscorlibBasePath(Version? version, string? publicKeyToken)
        {
            if (publicKeyToken == "969db8053d3322ac" && version != null)
            {
                string programFiles = Environment.Is64BitOperatingSystem ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
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

                string? folder = null;
                if (version != null)
                {
                    folder = GetSubFolderForVersion(version!);
                }

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

        private static string? GetSubFolderForVersion(Version version)
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

        private static string? GetMonoMscorlibBasePath(Version? version)
        {
            var path = Directory.GetParent(typeof(object).Module.FullyQualifiedName).Parent?.FullName;

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (version == null)
            {
                return null;
            }

            switch (version.Major)
            {
                case 1:
                    path = Path.Combine(path, "1.0");
                    break;
                case 2:
                    path = Path.Combine(path, version.MajorRevision == 5 ? "2.1" : "2.0");
                    break;
                case 4:
                    path = Path.Combine(path, "4.0");
                    break;
            }

            return Directory.Exists(path) ? path : null;
        }

        private static bool IsZeroOrAllOnes(Version? version)
        {
            return version == null
                   || (version.Major == 0 && version.Minor == 0 && version.Build == 0 && version.Revision == 0)
                   || (version.Major == 65535 && version.Minor == 65535 && version.Build == 65535 && version.Revision == 65535);
        }

        private static string? FindWindowsMetadataFile(string name, Version? version)
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

        private static string? FindWindowsMetadataInSystemDirectory(string name)
        {
            string file = Path.Combine(Environment.SystemDirectory, "WinMetadata", name + ".winmd");
            if (File.Exists(file))
            {
                return file;
            }

            return null;
        }

        private static string? FindInNuGetDirectory(string name, IEnumerable<string> extensions)
        {
            if (!Directory.Exists(DefaultNuGetDirectory))
            {
                return null;
            }

            var extensionsArray = extensions as string[] ?? extensions.ToArray();

            foreach (var folder in Directory.EnumerateDirectories(DefaultNuGetDirectory))
            {
                var subFolder = new DirectoryInfo(folder)
                    .EnumerateDirectories()
                    .Select(d => (version: new Version(RemoveTrailingVersionInfo(d.Name)), fullPath: d))
                    .Where(v => v.version != null)
                    .OrderByDescending(v => v.version)
                    .FirstOrDefault();

                var file = subFolder.fullPath?.EnumerateFiles(name + ".*", SearchOption.AllDirectories).FirstOrDefault(x => extensionsArray.Contains(x.Extension));

                if (file != null)
                {
                    return file.FullName;
                }
            }

            return null;
        }

        private static string? FindClosestVersionDirectory(string basePath, Version? version)
        {
            if (version == null)
            {
                return null;
            }

            string? path = null;
            foreach (var folder in new DirectoryInfo(basePath)
                .EnumerateDirectories()
                .Select(d => ConvertToVersion(d.Name))
                .Where(v => v.Version != null)
                .OrderByDescending(v => v.Version))
            {
                if (path == null || folder.Version >= version)
                {
                    path = folder.Name;
                }
            }

            return path ?? version?.ToString();
        }

        [SuppressMessage("Design", "CA1031: Modify to catch a more specific exception type, or rethrow the exception.", Justification = "Deliberate usage.")]
        private static (Version? Version, string? Name) ConvertToVersion(string name)
        {
            try
            {
                return (new Version(name.RemoveTrailingVersionInfo()), name);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        private static string RemoveTrailingVersionInfo(this string name)
        {
            string shortName = name;
            int dashIndex = shortName.IndexOf('-');
            if (dashIndex > 0)
            {
                shortName = shortName.Remove(dashIndex);
            }

            return shortName;
        }
    }
}