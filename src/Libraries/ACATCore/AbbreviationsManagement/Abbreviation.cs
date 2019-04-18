﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="Abbreviation.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.Utility;
using System;
using System.Text.RegularExpressions;

namespace ACAT.Lib.Core.AbbreviationsManagement
{
    /// <summary>
    /// Represents an abbreviation.  The abbreviation has a mnemonic,
    /// the expansion and the mode of expansion - "Write" or "Speak".
    /// In the first case, the abbreviation is expanded to its full form
    /// in the textual form.  In the second case, the expansion is converted
    /// to speech.
    /// </summary>
    public class Abbreviation : IDisposable
    {
        /// <summary>
        /// Has this object been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="mnemonic">abbreviation</param>
        /// <param name="expansion">abbreviation expansion</param>
        /// <param name="mode">mode of expansion - speech or text</param>
        public Abbreviation(String mnemonic, String expansion, String mode)
        {
            Mode = Convert(mode);
            init(mnemonic, expansion, Mode);
        }

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="mnemonic">abbreviation</param>
        /// <param name="expansion">abbreviation expansion</param>
        /// <param name="mode">mode of expansion - speech or text</param>
        public Abbreviation(String mnemonic, String expansion, AbbreviationMode mode)
        {
            init(mnemonic, expansion, mode);
        }

        /// <summary>
        /// Mode of abbreviation expansion - expand as text, or render as
        /// text to speech
        /// </summary>
        public enum AbbreviationMode
        {
            None,
            Write,
            Speak,
        }

        /// <summary>
        /// Gets or sets the expansion
        /// </summary>
        public String Expansion { get; set; }

        /// <summary>
        /// Gets or sets the abbreviation
        /// </summary>
        public String Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the abbreviation mode
        /// </summary>
        public AbbreviationMode Mode { get; set; }

        /// <summary>
        /// Converts the string representation of the mode to the enum
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static AbbreviationMode Convert(String mode)
        {
            var retVal = AbbreviationMode.None;
            try
            {
                retVal = (AbbreviationMode)Enum.Parse(typeof(AbbreviationMode), mode);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return retVal;
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposer. Release resources and cleanup.
        /// </summary>
        /// <param name="disposing">true to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose all managed resources.
                }

                // Release unmanaged resources.
            }

            _disposed = true;
        }

        /// <summary>
        /// Initializes the properties of the abbreviation object
        /// </summary>
        /// <param name="mnemonic">abbreviation</param>
        /// <param name="expansion">abbreviation expansion</param>
        /// <param name="mode">mode of expansion - speech or text</param>
        private void init(String mnemonic, String expansion, AbbreviationMode mode)
        {
            Mnemonic = mnemonic.ToUpper();
            Expansion = Regex.Replace(expansion, "\r\n", "\n");
            Mode = mode;
        }
    }
}