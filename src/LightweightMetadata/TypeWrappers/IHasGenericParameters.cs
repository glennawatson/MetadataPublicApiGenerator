﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace LightweightMetadata
{
    /// <summary>
    /// Indicates that this class has generic parameters.
    /// </summary>
    public interface IHasGenericParameters
    {
        /// <summary>
        /// Gets a list of generic parameters.
        /// </summary>
        IReadOnlyList<GenericParameterWrapper> GenericParameters { get; }
    }
}
