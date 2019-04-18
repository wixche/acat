﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AbbreviationReplaceEventArgs.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.AbbreviationsManagement
{
    /// <summary>
    /// Represents argument used by the event that is raised to indicate that
    /// an abbreviation needs to be handled.
    /// </summary>
    public class AbbreviationReplaceEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="startPos">Offset in the text stream</param>
        /// <param name="wordLength">Length of the word in the stream that needs replacing</param>
        /// <param name="replaceString">What to replace the word with</param>
        public AbbreviationReplaceEventArgs(int startPos, int wordLength, String replaceString)
        {
            StartPos = startPos;
            WordLength = wordLength;
            ReplaceString = replaceString;
        }

        /// <summary>
        /// The replacement string
        /// </summary>
        public String ReplaceString { get; private set; }

        /// <summary>
        /// Gets the starting pos in the text stream where the abbr was detected
        /// </summary>
        public int StartPos { get; private set; }

        /// <summary>
        /// Gets the length of the word to replace
        /// </summary>
        public int WordLength { get; private set; }
    }
}