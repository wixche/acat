﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ToastForm.cs" company="Intel Corporation">
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
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace ACAT.Lib.Core.PanelManagement
{
    /// <summary>
    /// Form that displays a toast message (similar to the
    /// one in Android). The message fades out after brief
    /// timeout
    /// </summary>
    public partial class ToastForm : Form
    {
        /// <summary>
        /// Thread proc for fading out
        /// </summary>
        private Thread _fadeOutThread;

        /// <summary>
        /// Set to true to close the toast
        /// </summary>
        private bool _quit;

        /// <summary>
        /// How long should the toast stay up?
        /// </summary>
        private int _timeout = 2000;

        /// <summary>
        /// Timer used to faded out
        /// </summary>
        private System.Windows.Forms.Timer _timer;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="message">the Toast message</param>
        public ToastForm(String message, int timeOut = 2000)
        {
            InitializeComponent();
            ShowInTaskbar = false;
            TopMost = true;
            Load += ToastForm_Load;
            Closing += OnClosing;
            _timeout = timeOut;
            labelMessage.Text = message;
        }

        /// <summary>
        /// Starts the thread to fade out the form
        /// </summary>
        private void fadeOut()
        {
            _fadeOutThread = new Thread(fadeOutProc);
            _fadeOutThread.Start();
        }

        /// <summary>
        /// Thread proc to fade out the form
        /// </summary>
        private void fadeOutProc()
        {
            while (!_quit)
            {
                try
                {
                    double opacity = Windows.GetOpacity(this);
                    opacity -= 0.05;
                    Windows.SetOpacity(this, opacity);
                    if (opacity > 0.0)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        Windows.CloseForm(this);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        /// <summary>
        /// Called when the form is closing
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="cancelEventArgs">event args</param>
        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _quit = true;
        }

        /// <summary>
        /// Timer tick proc.  Fade out the form
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            fadeOut();
        }

        /// <summary>
        /// Performs initialization
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void ToastForm_Load(object sender, EventArgs e)
        {
            if (_timeout > 0)
            {
                _timer = new System.Windows.Forms.Timer();
                _timer.Tick += timer_Tick;
                _timer.Interval = _timeout;
                _timer.Start();
            }
        }
    }
}