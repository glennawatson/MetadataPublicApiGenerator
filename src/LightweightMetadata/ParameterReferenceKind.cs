// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace LightweightMetadata
{
    /// <summary>
    /// The type of parameter reference kind we have.
    /// </summary>
    public enum ParameterReferenceKind
    {
        /// <summary>
        /// It is a ordinary reference.
        /// </summary>
        None,

        /// <summary>
        /// This is a output parameter type.
        /// </summary>
        Out,

        /// <summary>
        /// This is a input value.
        /// </summary>
        In,

        /// <summary>
        /// This is a reference type.
        /// </summary>
        Ref
    }
}
