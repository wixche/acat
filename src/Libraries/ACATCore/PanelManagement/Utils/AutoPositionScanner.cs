﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AutoPostionScanner.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.ActuatorManagement;
using ACAT.Lib.Core.InputActuators;
using ACAT.Lib.Core.Utility;
using System;
using System.Windows.Forms;
using ACAT.ACATResources;

namespace ACAT.Lib.Core.PanelManagement
{
    /// <summary>
    /// Repositions the scanner to one of the pre-defined spots on
    /// the display.  The position is changed on on a timer tick.
    /// This allows the user to easily select
    /// the preferred scanner position.  Timer stops either by
    /// a mouse click or by an actuation switch trigger
    /// </summary>
    public class AutoPositionScanner : IDisposable
    {
        /// <summary>
        /// Timer used to reposition the scanner
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Has this been disposed?
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The scanner form
        /// </summary>
        private Form _form;

        /// <summary>
        /// The keyboard actuator object
        /// </summary>
        private KeyboardActuator _keyboardActuator;

        /// <summary>
        /// The toast form that prompts the user to click to stop
        /// </summary>
        private ToastForm _toastForm;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="form">name of the scanner</param>
        public AutoPositionScanner(Form form)
        {
            _form = form;
            _timer = new Timer { Interval = CoreGlobals.AppPreferences.MenuDialogScanTime };
            _timer.Tick += _timer_Tick;
        }

        /// <summary>
        /// Raised when the scanner is repositioned
        /// </summary>
        public event EventHandler EvtAutoPostionScannerStopped;

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
        /// Returns true if timer is running?
        /// </summary>
        /// <returns>true if it is</returns>
        public bool IsRunning()
        {
            return _timer.Enabled;
        }

        /// <summary>
        /// Starts the timer that repositions the scanner on
        /// a timer tick
        /// </summary>
        public void Start()
        {
            _form.Invoke(new MethodInvoker(delegate
            {
                _toastForm = new ToastForm(R.GetString("TriggerToStop"), -1);
                Windows.SetWindowPosition(_toastForm, Windows.WindowPosition.CenterScreen);
                _toastForm.Show();

                if (!_timer.Enabled)
                {
                    subscribeToHookEvents();
                    startTimer();
                }
            }));
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            _form.Invoke(new MethodInvoker(delegate
            {
                if (_timer.Enabled)
                {
                    stopTimer();
                }
            }));
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
                Log.Debug();

                if (disposing)
                {
                    // dispose all managed resources.
                    if (_timer != null)
                    {
                        stopTimer();
                        _timer.Dispose();
                    }

                    _form = null;
                }

                // Release unmanaged resources.
            }

            _disposed = true;
        }

        /// <summary>
        /// Timer tick function. Positions the scanner at
        /// the next corner
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void _timer_Tick(object sender, EventArgs e)
        {
            var nextPosition = Windows.WindowPosition.TopRight;

            _form.Invoke(new MethodInvoker(delegate
            {
                switch (Context.AppWindowPosition)
                {
                    case Windows.WindowPosition.TopRight:
                        nextPosition = Windows.WindowPosition.MiddleRight;
                        break;

                    case Windows.WindowPosition.MiddleRight:
                        nextPosition = Windows.WindowPosition.BottomRight;
                        break;

                    case Windows.WindowPosition.BottomRight:
                        nextPosition = Windows.WindowPosition.BottomLeft;
                        break;

                    case Windows.WindowPosition.BottomLeft:
                        nextPosition = Windows.WindowPosition.MiddleLeft;
                        break;

                    case Windows.WindowPosition.MiddleLeft:
                        nextPosition = Windows.WindowPosition.TopLeft;
                        break;

                    case Windows.WindowPosition.TopLeft:
                        nextPosition = Windows.WindowPosition.TopRight;
                        break;
                }

                Windows.SetWindowPositionAndNotify(_form, nextPosition);
            }));
        }

        /// <summary>
        /// An actuator switch trigger event was detected.  Stops
        /// the timer
        /// </summary>
        /// <param name="switchObj">switch that actuated</param>
        /// <param name="handled">set to true</param>
        private void AppActuatorManager_EvtSwitchHook(IActuatorSwitch switchObj, ref bool handled)
        {
            bool h = false;
            _form.Invoke(new MethodInvoker(delegate
            {
                if (_timer.Enabled)
                {
                    stopTimer();
                    h = true;
                }
            }));
            handled = h;
        }

        /// <summary>
        /// A mouse click was detected.  Stops the timer
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="mouseEventArgs">event args</param>
        private void KeyboardActuator_EvtMouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            _form.Invoke(new MethodInvoker(stopTimer));
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        private void startTimer()
        {
            _timer.Start();
        }

        /// <summary>
        /// Stops the timer, disposes off the toast messasge
        /// </summary>
        private void stopTimer()
        {
            try
            {
                unsubscribeFromHookEvents();

                if (_form != null)
                {
                    _form.Invoke(new MethodInvoker(delegate
                    {
                        if (_toastForm != null)
                        {
                            _toastForm.Close();
                            _toastForm = null;
                        }

                        if (_timer != null)
                        {
                            _timer.Stop();
                            if (EvtAutoPostionScannerStopped != null)
                            {
                                EvtAutoPostionScannerStopped(this, EventArgs.Empty);
                            }
                        }
                    }));
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Subscribes the events
        /// </summary>
        private void subscribeToHookEvents()
        {
            Context.AppActuatorManager.EvtSwitchHook += AppActuatorManager_EvtSwitchHook;

            var actuator = Context.AppActuatorManager.GetActuator(typeof(KeyboardActuator));
            if (actuator is KeyboardActuator)
            {
                _keyboardActuator = actuator as KeyboardActuator;
                _keyboardActuator.EvtMouseDown += KeyboardActuator_EvtMouseDown;
            }
        }

        /// <summary>
        /// Unsubscribes from events
        /// </summary>
        private void unsubscribeFromHookEvents()
        {
            Context.AppActuatorManager.EvtSwitchHook -= AppActuatorManager_EvtSwitchHook;

            if (_keyboardActuator != null)
            {
                _keyboardActuator.EvtMouseDown -= KeyboardActuator_EvtMouseDown;
            }
        }
    }
}