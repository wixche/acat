﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="CurrentWordWidget" company="Intel Corporation">
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
using System.Timers;
using System.Windows.Forms;

namespace ACAT.Lib.Core.Widgets
{
    /// <summary>
    /// The Alphabet scanner has a UI control to display the
    /// current word being typed.  This widget provides
    /// functionality to the UI control. Simulates a blinking cursor
    /// at the end of the text in the Control.
    /// </summary>
    public class CurrentWordWidget : LabelWidget
    {
        /// <summary>
        /// Timer to simulate a blinking cursor
        /// </summary>
        private readonly System.Timers.Timer _cursorTimer;

        /// <summary>
        /// Has this been disposed off?
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Whether to display the cursor or not
        /// </summary>
        private bool _onOff;

        /// <summary>
        /// The text to display
        /// </summary>
        private String _text;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="control">the inner .NET Control for the widget</param>
        public CurrentWordWidget(Control control)
            : base(control)
        {
            if (control is Label)
            {
                var label = (Label)control;
                label.AutoEllipsis = true;
            }

            _cursorTimer = new System.Timers.Timer();
            _onOff = false;
            _text = String.Empty;
            _cursorTimer.Interval = 600;
            _cursorTimer.Elapsed += _cursorTimer_Elapsed;
            _cursorTimer.Start();
        }

        /// <summary>
        /// Set the text in the current word box
        /// </summary>
        /// <param name="text">Text to set</param>
        public void SetCurrentWord(String text)
        {
            SetText(text);
            _text = text;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">true to dispose managed resources</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    Log.Debug();

                    if (disposing)
                    {
                        // release managed resources
                        unInit();
                    }

                    // Release the native unmanaged resources

                    _disposed = true;
                }
                finally
                {
                    // Call Dispose on your base class.
                    base.Dispose(disposing);
                }
            }
        }

        /// <summary>
        /// Timer tick function to simulate a blinking cursor
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void _cursorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_onOff)
                {
                    SetText(_text + "_");
                    _onOff = false;
                }
                else
                {
                    SetText(_text);
                    _onOff = true;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        private void unInit()
        {
            _cursorTimer.Stop();
            _cursorTimer.Dispose();
        }
    }
}