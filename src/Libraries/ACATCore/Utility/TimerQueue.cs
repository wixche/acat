﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="TimerQueue.cs" company="Intel Corporation">
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
using System.Runtime.InteropServices;

namespace ACAT.Lib.Core.Utility
{
    /// <summary>
    /// A wrapper class for Windows timer queue.  .NET timers
    /// are inherently inaccurate.  Their fidelity is on the
    /// order of 15 ms.  This timer gives us almost millisecond
    /// level accuracy
    /// </summary>
    public class TimerQueue : IDisposable
    {
        /// <summary>
        /// Windows constant
        /// </summary>
        private const int ErrorIOPending = 997;

        /// <summary>
        /// Callback function for the timer
        /// </summary>
        private readonly WaitOrTimerDelegate _callback;

        /// <summary>
        /// When will the timer expire
        /// </summary>
        private readonly uint _dueTime;

        /// <summary>
        /// The timer period, how often to fire after
        /// the first time
        /// </summary>
        private readonly uint _period;

        /// <summary>
        /// Disposed yet?
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Is the timer running?
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Handle to the timer
        /// </summary>
        private IntPtr _timerHandle;

        /// <summary>
        /// Constructor.  Use this for a oneshot timer that will
        /// expire at dueTime.
        /// </summary>
        /// <param name="dueTime">When to trigger</param>
        /// <param name="callback">callback to invoke</param>
        public TimerQueue(int dueTime, WaitOrTimerDelegate callback)
        {
            _dueTime = (uint)dueTime;
            _period = 0;
            _callback = callback;
        }

        /// <summary>
        /// Use this for periodic timer.  DueTime is when it's
        /// fired first (can be 0 for immediate) and period is
        /// how often to fire subsequently
        /// </summary>
        /// <param name="dueTime">Time of first firing</param>
        /// <param name="period">Periodicity</param>
        /// <param name="callback">callback function</param>
        public TimerQueue(int dueTime, int period, WaitOrTimerDelegate callback)
        {
            _dueTime = (uint)dueTime;
            _period = (uint)period;
            _callback = callback;
        }

        private enum Flag
        {
            WT_EXECUTEDEFAULT = 0x00000000,
            WT_EXECUTEINIOTHREAD = 0x00000001,
            WT_EXECUTEONLYONCE = 0x00000008,
            WT_EXECUTELONGFUNCTION = 0x00000010,
            WT_EXECUTEINTIMERTHREAD = 0x00000020,
            WT_EXECUTEINPERSISTENTTHREAD = 0x00000080,
        }

        /// <summary>
        /// Dispose off the timer
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            return Start(IntPtr.Zero);
        }

        /// <summary>
        /// Start the timer. Optional param will be sent
        /// to the callback function
        /// </summary>
        /// <param name="param">parameter to send to callback</param>
        /// <returns>true on success</returns>
        public bool Start(IntPtr param)
        {
            int getLastError;

            if (_isRunning == true)
            {
                // already running
                Log.Debug("Timer is already running. returning");
                return true;
            }

            bool retVal = Kernel32Interop.CreateTimerQueueTimer(
                            out _timerHandle,
                            IntPtr.Zero,
                            _callback,
                            param,
                            _dueTime,
                            _period,
                            (uint)Flag.WT_EXECUTEINIOTHREAD);

            if (retVal)
            {
                _isRunning = true;
            }
            else
            {
                getLastError = Marshal.GetLastWin32Error();
                Log.Debug("Error while starting timer.  getLastError=" + getLastError);
            }

            return retVal;
        }

        /// <summary>
        /// Stops the timer and deletes it
        /// </summary>
        public bool Stop()
        {
            if (_isRunning == false)
            {
                Log.Debug("Not running. returning");
                return true;
            }

            bool retVal = true;
            try
            {
                retVal = Kernel32Interop.DeleteTimerQueueTimer(IntPtr.Zero, _timerHandle, IntPtr.Zero);

                _isRunning = false;

                if (!retVal)
                {
                    var getLastError = Marshal.GetLastWin32Error();
                    if (getLastError == ErrorIOPending)
                    {
                        retVal = true;
                    }
                }
            }
            catch
            {
            }

            return retVal;
        }
    }
}