﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="SyncLock.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.Utility
{
    /// <summary>
    /// This class is used by scanners when they are closing.
    /// The Status is set to Closing or Closed in the event
    /// handlers for OnFormCLosing or FormClosed. This is used
    /// by timers or threads owned by the form to know that they
    /// have to quit. This is cleaner than having to use a
    /// volatile bool for instance.
    /// Can also be used to see if a window has closed.
    /// </summary>
    public class SyncLock
    {
        /// <summary>
        /// Initialzies an instance of the class
        /// </summary>
        public SyncLock()
        {
            Status = StatusValues.None;
        }

        /// <summary>
        /// Indicates the scanner status
        /// </summary>
        public enum StatusValues
        {
            None = 0,
            Closing = 1,
            Closed = 2
        }

        /// <summary>
        /// Gets or sets the status value
        /// </summary>
        public StatusValues Status { get; set; }

        /// <summary>
        /// Returns whether the window is in the process of closing
        /// </summary>
        /// <returns>true if it is</returns>
        public bool IsClosing()
        {
            return (Status == StatusValues.Closed) || (Status == StatusValues.Closing);
        }
    }
}