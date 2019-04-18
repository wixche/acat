﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="TTSBookmarkReachedEventArgs.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.TTSManagement
{
    /// <summary>
    /// Argument for the event raised when the TTS engine reaches
    /// a bookmark.  Bookmarks can be inserted into the text stream
    /// and during the conversion process, the bookmarks are returned
    /// through this event, telling how far the speech engine has progressed.
    /// </summary>
    public class TTSBookmarkReachedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="bookmark">bookmark</param>
        public TTSBookmarkReachedEventArgs(int bookmark)
        {
            Bookmark = bookmark;
        }

        /// <summary>
        /// Gets the bookmark value
        /// </summary>
        public int Bookmark { get; private set; }
    }
}