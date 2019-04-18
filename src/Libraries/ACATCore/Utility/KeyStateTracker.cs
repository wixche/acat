﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="KeyStateTracker.cs" company="Intel Corporation">
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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ACAT.Lib.Core.Utility
{
    /// <summary>
    /// Tracks the state of Shift, Ctrl and Alt keys.  On the UI, if
    /// the user selects Shift (for eg) twice in a row, it is sticky and
    /// Shift is applied to all the letters typed there after. If the
    /// user selects Shift the third time, it is unselected.
    /// This class keeps track of the key states of Shift, Ctrl and Alt -
    /// whether they are currently active or not.
    ///
    /// Note that everything in this class is static since we want
    /// these settings to be system-wide.
    /// </summary>
    public class KeyStateTracker
    {
        private const byte VK_CAPITAL = 0x14;
        private const byte VK_NUMLOCK = 0x90;

        private static int _alt = 0x2;
        private static int _ctrl = 0x1;
        private static int _extendedKeyPressedStatus = 0;
        private static int _extendedKeyTriggerStatus = 0;
        private static int _shift = 0x4;
        private static bool _stickyAlt = false;
        private static bool _stickyCtrl = false;
        private static bool _stickyShift = false;

        /// <summary>
        /// Delegate for the event that is raised when the keystate changes
        /// </summary>
        public delegate void KeyStateChanged();

        /// <summary>
        /// Raised when the keystate changes
        /// </summary>
        public static event KeyStateChanged EvtKeyStateChanged;

        private enum State
        {
            Up,
            Down
        }

        /// <summary>
        /// Clears all keys
        /// </summary>
        public static void ClearAll()
        {
            _extendedKeyTriggerStatus = 0;
            _extendedKeyPressedStatus = 0;

            _stickyShift = false;
            _stickyCtrl = false;
            _stickyAlt = false;
            setCapsLockState(false);
            notifyKeyStateChanged();
        }

        /// <summary>
        /// Clears alt key status
        /// </summary>
        public static void ClearAlt()
        {
            turnOff(_alt);
            _stickyAlt = false;
            notifyKeyStateChanged();
        }

        /// <summary>
        /// Clears ctrl key status
        /// </summary>
        public static void ClearCtrl()
        {
            _stickyCtrl = false;
            turnOff(_ctrl);
            notifyKeyStateChanged();
        }

        /// <summary>
        /// Clears shift key status
        /// </summary>
        public static void ClearShift()
        {
            turnOff(_shift);
            _stickyShift = false;
            notifyKeyStateChanged();
        }

        /// <summary>
        /// Returns an array of special keys that are currently pressed
        /// </summary>
        /// <returns></returns>
        public static List<Keys> GetExtendedKeys()
        {
            List<Keys> retVal = new List<Keys>();

            if (IsCtrlOn())
            {
                Log.Debug("Ctrl is on");
                retVal.Add(Keys.LControlKey);
            }

            if (IsAltOn())
            {
                Log.Debug("Alt is on");
                retVal.Add(Keys.LMenu);
            }

            if (IsShiftOn() && !IsCapsLockOn())
            {
                Log.Debug("Shift is on");
                retVal.Add(Keys.LShiftKey);
            }

            return retVal;
        }

        /// <summary>
        /// Returns a string with the special keys currently in the
        /// pressed state.  The keys are delimited with a plus.
        /// For eg, Ctrl+Alt
        /// </summary>
        /// <returns>string representing the key pressed status</returns>
        public static string GetKeyPressedStatus()
        {
            String keyString = String.Empty;
            if (isOn(_extendedKeyPressedStatus, _ctrl))
            {
                keyString += "Ctrl";
            }

            if (isOn(_extendedKeyPressedStatus, _alt))
            {
                keyString += (String.IsNullOrEmpty(keyString) ? String.Empty : "+") + "Alt";
            }

            if (isOn(_extendedKeyPressedStatus, _shift))
            {
                keyString += (String.IsNullOrEmpty(keyString) ? String.Empty : "+") + "Shift";
            }

            return keyString;
        }

        /// <summary>
        /// Is the alt key pressed?
        /// </summary>
        /// <returns></returns>
        public static bool IsAltOn()
        {
            return isOn(_extendedKeyTriggerStatus, _alt) || isOn(_extendedKeyPressedStatus, _alt);
        }

        /// <summary>
        /// Is capslock engaged?
        /// </summary>
        /// <returns></returns>
        public static bool IsCapsLockOn()
        {
            return User32Interop.GetKeyState(VK_CAPITAL) != 0;
        }

        /// <summary>
        /// Is the ctrl key pressed?
        /// </summary>
        /// <returns></returns>
        public static bool IsCtrlOn()
        {
            return isOn(_extendedKeyTriggerStatus, _ctrl) || isOn(_extendedKeyPressedStatus, _ctrl);
        }

        /// <summary>
        /// Is capslock engaged?
        /// </summary>
        /// <returns></returns>
        public static bool IsNumLockOn()
        {
            return User32Interop.GetKeyState(VK_NUMLOCK) != 0;
        }

        /// <summary>
        /// Is the SHIFT key currently pressed?
        /// </summary>
        /// <returns></returns>
        public static bool IsShiftKeyDown()
        {
            return isOn(_extendedKeyPressedStatus, _shift);
        }

        /// <summary>
        /// Is the shift key on
        /// </summary>
        /// <returns></returns>
        public static bool IsShiftOn()
        {
            return isOn(_extendedKeyTriggerStatus, _shift) || isOn(_extendedKeyPressedStatus, _shift);
        }

        /// <summary>
        /// Is alt key sticky?
        /// </summary>
        /// <returns></returns>
        public static bool IsStickyAltOn()
        {
            return _stickyAlt;
        }

        /// <summary>
        /// Is the Ctrl key sticky?
        /// </summary>
        /// <returns></returns>
        public static bool IsStickyCtrlOn()
        {
            return _stickyCtrl;
        }

        /// <summary>
        /// Is the sticky shift key on?
        /// </summary>
        /// <returns></returns>
        public static bool IsStickyShiftOn()
        {
            return _stickyShift;
        }

        /// <summary>
        /// A keydown event was detected from the physical keyboard
        /// </summary>
        /// <param name="key">key that is down</param>
        /// <returns>true if it is one of the modifier keys</returns>
        public static bool KeyDown(Keys key)
        {
            bool retVal = true;

            Log.Debug(key.ToString());

            switch (key)
            {
                case Keys.RShiftKey:
                case Keys.LShiftKey:
                    shiftState(State.Down);
                    break;

                case Keys.RControlKey:
                case Keys.LControlKey:
                    ctrlState(State.Down);
                    break;

                case Keys.RMenu:
                case Keys.LMenu:
                    altState(State.Down);
                    break;

                default:
                    retVal = false;
                    break;
            }

            return retVal;
        }

        /// <summary>
        /// A normal key was hit. Remove non-sticky keys.  i.e,
        /// if the user pressed Shift previously, remove Shift.
        /// </summary>
        /// <param name="ch"></param>
        public static void KeyTriggered(char ch)
        {
            removeNonStickySpecialKeys();
        }

        /// <summary>
        /// The specified key was trigged from the UI, not from
        /// the physical keyboard.  Here's where we have to keep
        /// track of stickiness etc.
        /// </summary>
        /// <param name="key"></param>
        public static void KeyTriggered(Keys key)
        {
            switch (key)
            {
                case Keys.LShiftKey:
                    if (IsShiftOn())
                    {
                        if (_stickyShift)
                        {
                            turnOff(_shift);
                            _stickyShift = false;
                            setCapsLockState(false);
                        }
                        else
                        {
                            _stickyShift = true;
                            setCapsLockState(true);
                        }
                    }
                    else
                    {
                        turnOn(_shift);
                    }

                    notifyKeyStateChanged();
                    break;

                case Keys.LControlKey:
                    if (IsCtrlOn())
                    {
                        turnOff(_ctrl);
                    }
                    else
                    {
                        turnOn(_ctrl);
                    }

                    notifyKeyStateChanged();
                    break;

                case Keys.LMenu:
                    if (IsAltOn())
                    {
                        turnOff(_alt);
                    }
                    else
                    {
                        turnOn(_alt);
                    }

                    notifyKeyStateChanged();
                    break;

                default:
                    removeNonStickySpecialKeys();
                    break;
            }
        }

        /// <summary>
        /// A key up event was detected from the physical keyboard
        /// </summary>
        /// <param name="key"></param>
        public static void KeyUp(Keys key)
        {
            Log.Debug(key.ToString());

            switch (key)
            {
                case Keys.RShiftKey:
                case Keys.LShiftKey:
                    shiftState(State.Up);
                    break;

                case Keys.RControlKey:
                case Keys.LControlKey:
                    ctrlState(State.Up);
                    break;

                case Keys.RMenu:
                case Keys.LMenu:
                    altState(State.Up);
                    break;

                case Keys.CapsLock:
                    if (Form.IsKeyLocked(Keys.CapsLock))
                    {
                        turnOn(_shift);
                        _stickyShift = true;
                    }
                    else
                    {
                        ClearShift();
                    }

                    notifyKeyStateChanged();
                    break;
            }
        }

        /// <summary>
        /// Track the state of the Alt key
        /// </summary>
        /// <param name="state">the Key state</param>
        private static void altState(State state)
        {
            if (state == State.Down)
            {
                _extendedKeyPressedStatus |= _alt;
            }
            else
            {
                _extendedKeyPressedStatus &= ~_alt;
            }

            notifyKeyStateChanged();
        }

        /// <summary>
        /// Track the state of the ctrl key
        /// </summary>
        /// <param name="state"></param>
        private static void ctrlState(State state)
        {
            if (state == State.Down)
            {
                _extendedKeyPressedStatus |= _ctrl;
            }
            else
            {
                _extendedKeyPressedStatus &= ~_ctrl;
            }

            notifyKeyStateChanged();
        }

        /// <summary>
        /// Checks if the 'bit' is on in 'status'
        /// </summary>
        /// <param name="status">status field</param>
        /// <param name="bit">bit to check</param>
        /// <returns>true if it is on</returns>
        private static bool isOn(int status, int bit)
        {
            return (status & bit) == bit;
        }

        /// <summary>
        /// Notifies subscribeers that keystate changed
        /// </summary>
        private static void notifyKeyStateChanged()
        {
            if (EvtKeyStateChanged != null)
            {
                EvtKeyStateChanged();
            }
        }

        /// <summary>
        /// Removes all keys that are currently not sticky, but
        /// currently pressed
        /// </summary>
        private static void removeNonStickySpecialKeys()
        {
            ArrayList removeList = new ArrayList();
            bool somethingChanged = false;

            if (!_stickyShift)
            {
                turnOff(_shift);
                somethingChanged = true;
            }

            if (!_stickyCtrl)
            {
                turnOff(_ctrl);
                somethingChanged = true;
            }

            if (!_stickyAlt)
            {
                turnOff(_alt);
                somethingChanged = true;
            }

            if (somethingChanged)
            {
                notifyKeyStateChanged();
            }
        }

        /// <summary>
        /// Turns caps lock on/off and sets the state of the
        /// shift key accordingly
        /// </summary>
        /// <param name="onOff">true to turn on</param>
        private static void setCapsLockState(bool onOff)
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;

            if (onOff)
            {
                if (!Form.IsKeyLocked(Keys.CapsLock))
                {
                    // turn on capslock
                    User32Interop.keybd_event((byte)Keys.CapsLock, 0xAA, 0, UIntPtr.Zero);
                    User32Interop.keybd_event((byte)Keys.CapsLock, 0xAA, (uint)KEYEVENTF_KEYUP, UIntPtr.Zero);
                    turnOn(_shift);
                    _stickyShift = true;
                }
            }
            else
            {
                if (Form.IsKeyLocked(Keys.CapsLock))
                {
                    // turn off capslock
                    User32Interop.keybd_event((byte)Keys.CapsLock, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                    User32Interop.keybd_event((byte)Keys.CapsLock, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
                    ClearShift();
                }
            }
        }

        /// <summary>
        /// Track the state of the shift key
        /// </summary>
        /// <param name="state"></param>
        private static void shiftState(State state)
        {
            if (state == State.Down)
            {
                _extendedKeyPressedStatus |= _shift;
            }
            else
            {
                _extendedKeyPressedStatus &= ~_shift;
            }

            notifyKeyStateChanged();
        }

        /// <summary>
        /// Turn off the specified bit in the extendedkeytriggerstatus
        /// field
        /// </summary>
        /// <param name="bit">which bit to turn off</param>
        private static void turnOff(int bit)
        {
            _extendedKeyTriggerStatus &= ~bit;
        }

        /// <summary>
        /// Turn on the specified bit in the extendedkeytriggerstatus
        /// </summary>
        /// <param name="bit">which bit to turn on</param>
        private static void turnOn(int bit)
        {
            _extendedKeyTriggerStatus |= bit;
        }
    }
}