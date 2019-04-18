﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="Attributions.cs" company="Intel Corporation">
//
// Copyright (c) 2013-2017 Intel Corporation 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace ACAT.Lib.Core.Utility
{
    /// <summary>
    /// Manages third party attributions that could be displayed in an
    /// About box for example
    /// </summary>
    public static class Attributions
    {
        /// <summary>
        /// Stores mapping between the attribution source and the
        /// attribution string
        /// </summary>
        private static readonly Dictionary<String, String> _attributions = new Dictionary<string, string>();

        /// <summary>
        /// Adds an attribution. Token is a string to ensure if the
        /// same 3rd party adds attributions multiple times, they
        /// are not duplicated. Use the same token value to prevent this.
        /// </summary>
        /// <param name="source">source of the attribution</param>
        /// <param name="attribution">the attribution string</param>
        /// <returns></returns>
        public static bool Add(String source, String attribution)
        {
            bool retVal = !_attributions.ContainsKey(source);

            if (retVal)
            {
                _attributions.Add(source, attribution);
            }

            return retVal;
        }

        /// <summary>
        /// Gets the attribution for the specified source
        /// </summary>
        /// <param name="source">source</param>
        /// <returns>attribution value, empty string if not found</returns>
        public static String Get(String source)
        {
            return (_attributions.ContainsKey(source) ? _attributions[source] : String.Empty);
        }

        /// <summary>
        /// Gets all the attributions
        /// </summary>
        /// <returns>string array of all the attributions</returns>
        public static IEnumerable<String> GetAll()
        {
            return _attributions.Values;
        }
    }
}