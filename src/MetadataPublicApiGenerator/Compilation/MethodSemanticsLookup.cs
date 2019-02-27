// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// Portions of this code are from the following project https://github.com/icsharpcode/ILSpy
// Copyright (c) 2018 Daniel Grunwald
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace MetadataPublicApiGenerator.Compilation
{
    /// <summary>
    /// Lookup structure that, for an accessor, can find the associated property/event.
    /// </summary>
    internal class MethodSemanticsLookup
    {
        private const MethodSemanticsAttributes CsharpAccessors =
            MethodSemanticsAttributes.Getter | MethodSemanticsAttributes.Setter
            | MethodSemanticsAttributes.Adder | MethodSemanticsAttributes.Remover;

        // entries, sorted by MethodRowNumber
        private readonly List<Entry> _entries;

        public MethodSemanticsLookup(MetadataReader metadata, MethodSemanticsAttributes filter = CsharpAccessors)
        {
            if ((filter & MethodSemanticsAttributes.Other) != 0)
            {
                throw new NotSupportedException("SRM doesn't provide access to 'other' accessors");
            }

            _entries = new List<Entry>(metadata.GetTableRowCount(TableIndex.MethodSemantics));
            foreach (var propHandle in metadata.PropertyDefinitions)
            {
                var prop = metadata.GetPropertyDefinition(propHandle);
                var accessors = prop.GetAccessors();
                AddEntry(MethodSemanticsAttributes.Getter, accessors.Getter, propHandle);
                AddEntry(MethodSemanticsAttributes.Setter, accessors.Setter, propHandle);
            }

            foreach (var eventHandle in metadata.EventDefinitions)
            {
                var ev = metadata.GetEventDefinition(eventHandle);
                var accessors = ev.GetAccessors();
                AddEntry(MethodSemanticsAttributes.Adder, accessors.Adder, eventHandle);
                AddEntry(MethodSemanticsAttributes.Remover, accessors.Remover, eventHandle);
                AddEntry(MethodSemanticsAttributes.Raiser, accessors.Raiser, eventHandle);
            }

            _entries.Sort();

            void AddEntry(MethodSemanticsAttributes semantics, MethodDefinitionHandle method, EntityHandle association)
            {
                if ((semantics & filter) == 0 || method.IsNil)
                {
                    return;
                }

                _entries.Add(new Entry(semantics, method, association));
            }
        }

        public (EntityHandle, MethodSemanticsAttributes) GetSemantics(MethodDefinitionHandle method)
        {
            var pos = _entries.BinarySearch(new Entry(0, method, default));
            return pos >= 0 ? (_entries[pos].Association, _entries[pos].Semantics) : (default, 0);
        }

        private readonly struct Entry : IComparable<Entry>
        {
            private readonly int _methodRowNumber;

            public Entry(MethodSemanticsAttributes semantics, MethodDefinitionHandle method, EntityHandle association)
            {
                Semantics = semantics;
                _methodRowNumber = MetadataTokens.GetRowNumber(method);
                Association = association;
            }

            public MethodSemanticsAttributes Semantics { get; }

            public MethodDefinitionHandle Method => MetadataTokens.MethodDefinitionHandle(_methodRowNumber);

            public EntityHandle Association { get; }

            public int CompareTo(Entry other)
            {
                return _methodRowNumber.CompareTo(other._methodRowNumber);
            }
        }
    }
}
