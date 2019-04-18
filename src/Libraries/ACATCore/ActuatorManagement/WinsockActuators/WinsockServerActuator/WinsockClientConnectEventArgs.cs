﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="WinsockClientConnectEventArgs.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.InputActuators
{
    /// <summary>
    /// Argument for the event raised when a TCP/IP client connects
    /// </summary>
    public class WinsockClientConnectEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="ipAddress">IP address of the client</param>
        public WinsockClientConnectEventArgs(String id, String ipAddress)
        {
            Id = id;
            IPAddress = ipAddress;
        }

        /// <summary>
        /// Gets or sets the client id
        /// </summary>
        public String Id { get; private set; }

        /// <summary>
        /// Gets or sets the ip address
        /// </summary>
        public String IPAddress { get; private set; }
    }
}