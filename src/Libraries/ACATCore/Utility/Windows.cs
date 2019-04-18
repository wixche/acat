﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="Windows.cs" company="Intel Corporation">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Automation;

namespace ACAT.Lib.Core.Utility
{
    /// <summary>
    /// Contains numerous functions for window manipulation,
    /// window sizing, window positioning, etc.
    /// Has cross-thread invoke-required functions for most
    /// of the commonly used windows functions
    /// </summary>
    public class Windows
    {
        private const int GCL_HICON = -14;
        private const int GCL_HICONSM = -34;
        private const int GWL_STYLE = (-16);
        private const int ICON_BIG = 1;
        private const int ICON_SMALL = 0;
        private const int ICON_SMALL2 = 2;
        private const int WM_GETICON = 0x7F;
        private static String _taskbarWinClass = "Shell_TrayWnd";
        private static float _dpiX = 0.0f;
        private static int _widestScannerWidth = 0; // width of the widest scanner

        private static WindowsVersion _windowsVersion = WindowsVersion.Unknown;

        public delegate void FadeInComplete(Form form);

        public delegate void FadeOutComplete(Form form);

        public delegate void WindowPositionChanged(Form form, WindowPosition position);

        private delegate void activate(Form form);

        private delegate void activateForm(Form form);

        private delegate void closeForm(Form form);

        private delegate int getCaretPosition(TextBoxBase textbox);

        private delegate double getOpacity(Form form);

        private delegate String getSelectedText(TextBoxBase control);

        private delegate String getText(Control control);

        private delegate int getTrackBarValueInt(TrackBar trackBar);

        private delegate bool getVisible(Control control);

        private delegate void setBackgroundColor(Control control, Color color);

        private delegate void setCaretPosition(TextBoxBase textbox, int pos);

        private delegate bool setFocus(Control control);

        private delegate void setForegroundColor(Control control, Color color);

        private delegate void setImage(PictureBox box, Image image);

        private delegate void setOpacity(Form form, double opacity);

        private delegate void setRegion(Control control, Region region);

        private delegate void setText(Control control, string text);

        private delegate void setTopMost(Form form, bool value);

        private delegate void setTrackBarValue(TrackBar trackBar, int positionValue);

        private delegate void setVisible(Control control, bool flag);

        private delegate void show(Form parent, Form child);

        private delegate void showDialog(Form parent, Form child);

        private delegate void showForm(Form form);

        private delegate void showInTaskbar(Form control, bool flag);

        private delegate void showWIndowWithoutActivation(Form form);

        private delegate void unselectText(TextBoxBase control);

        public static event FadeInComplete EvtFadeInComplete;

        public static event FadeOutComplete EvtFadeOutComplete;

        public static event WindowPositionChanged EvtWindowPositionChanged;

        public enum WindowAlign
        {
            CenterHorizontal = 0
        }

        public enum WindowPosition
        {
            TopRight,
            TopLeft,
            BottomRight,
            BottomLeft,
            CenterScreen,
            MiddleRight,
            MiddleLeft
        }

        public enum WindowRelative
        {
            Below = 0,
            Above = 1,
            Right = 2,
            Left = 3
        }

        public enum WindowsVersion
        {
            Unknown,
            Win7,
            Win8,
            Win10
        }

        /// <summary>
        /// Enum used by SetProssDpiAwareness
        /// </summary>
        private enum _Process_DPI_Awareness
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        /// <summary>
        /// Enum used by DwmGetWindowAttribute
        /// </summary>
        private enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExceludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }

        /// <summary>
        /// Gets or set the width of the widest scanner
        /// </summary>
        public static int WidestScannerWidth
        {
            get { return _widestScannerWidth; }
            set
            {
                if (value > _widestScannerWidth)
                {
                    _widestScannerWidth = value;
                }
            }
        }

        /// <summary>
        /// Activates the specified form
        /// </summary>
        /// <param name="form">form to activate</param>
        public static void Activate(Form form)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new activate(Activate), form);
            }
            else
            {
                form.Activate();
            }
        }

        /// <summary>
        /// Activates the specified form
        /// </summary>
        /// <param name="form">form to activate</param>
        public static void ActivateForm(Form form)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new activateForm(ActivateForm), form);
            }
            else
            {
                form.Activate();
            }
        }

        /// <summary>
        /// Shows the specified window and brings it to top
        /// </summary>
        /// <param name="handle">window handle</param>
        /// <returns>true on success</returns>
        public static bool ActivateWindow(IntPtr handle)
        {
            int style = User32Interop.GetWindowLong(handle, User32Interop.GWL_STYLE);
            if ((style & User32Interop.WS_MAXIMIZE) == User32Interop.WS_MAXIMIZE)
            {
                //It's maximized
                User32Interop.SetForegroundWindow(handle);
                User32Interop.ShowWindow(handle.ToInt32(), User32Interop.SW_SHOW);
                User32Interop.BringWindowToTop(handle);
            }
            else if ((style & User32Interop.WS_MINIMIZE) == User32Interop.WS_MINIMIZE)
            {
                //It's minimized
                User32Interop.SetForegroundWindow(handle);
                User32Interop.ShowWindow(handle.ToInt32(), User32Interop.SW_RESTORE);
                User32Interop.BringWindowToTop(handle);
            }
            else
            {
                // don't give up just yet!
                User32Interop.SetForegroundWindow(handle);
                User32Interop.ShowWindow(handle.ToInt32(), User32Interop.SW_SHOW);
                User32Interop.BringWindowToTop(handle);
            }
            return true;
        }

        /// <summary>
        /// Simulates a mouse click on the specified control
        /// </summary>
        /// <param name="control">the control</param>
        public static void ClickOnWindow(Control control)
        {
            if (control.Visible)
            {
                int xpos = control.Left + 5;
                int ypos = control.Top + 5;

                Point oldPos = Cursor.Position;
                MouseUtils.ClickLeftMouseButton(xpos, ypos);
                Cursor.Position = oldPos;
            }
        }

        /// <summary>
        /// Closes the form asynchronously in a different thread
        /// </summary>
        /// <param name="form">form to close</param>
        public static void CloseAsync(Form form)
        {
            var thread = new Thread(() => closeFormThreadProc(form));
            thread.Start();
        }

        /// <summary>
        /// Closes the specified form
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="form">Form to close</param>
        public static void CloseForm(Form form)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new closeForm(CloseForm), form);
            }
            else
            {
                form.Close();
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /// <summary>
        /// Docks the form to the scanner form in the relative
        /// position specified
        /// </summary>
        /// <param name="form">form to dock</param>
        /// <param name="scanner">scanner form to dock to</param>
        /// <param name="scannerPosition">relative position</param>
        public static void DockWithScanner(Form form, Form scanner, WindowPosition scannerPosition, bool repositionTop = true)
        {
            if (form == scanner)
            {
                return;
            }

            switch (scannerPosition)
            {
                case WindowPosition.TopRight:
                    form.Location = new Point(scanner.Left - form.Width, (repositionTop) ? scanner.Top : form.Top);
                    break;

                case WindowPosition.TopLeft:
                    form.Location = new Point(scanner.Left + scanner.Width, (repositionTop) ? scanner.Top : form.Top);
                    break;

                case WindowPosition.BottomLeft:
                    form.Location = new Point(scanner.Left + scanner.Width,
                                        (repositionTop) ? Screen.PrimaryScreen.WorkingArea.Height - form.Height : form.Top);
                    break;

                case WindowPosition.BottomRight:
                    form.Location = new Point(scanner.Left - form.Width,
                                        (repositionTop) ? Screen.PrimaryScreen.WorkingArea.Height - form.Height : form.Top);
                    break;

                case WindowPosition.MiddleRight:
                    form.Location = new Point(scanner.Left - form.Width,
                                        (repositionTop) ? (Screen.PrimaryScreen.WorkingArea.Height - form.Height)/2 : form.Top);
                    break;

                case WindowPosition.MiddleLeft:
                    form.Location = new Point(scanner.Width, 
                                        (repositionTop) ? (Screen.PrimaryScreen.WorkingArea.Height - form.Height)/2 : form.Top);
                    break;
            }
        }

        /// <summary>
        /// Docks the current foreground application window with the scanner
        /// </summary>
        /// <param name="scannerPosition">Position of the scanner</param>
        /// <param name="handle">Handle to the window</param>
        /// <param name="scannerForm">The scanner form</param>
        public static void DockAppWindowWithScanner(WindowPosition scannerPosition, IntPtr handle, Form scannerForm)
        {
            int screenOffset = 0;
            int moveX = 0;
            int moveY = 0;  // not really using Y-axis yet but something to keep in mind for the future

            if (handle != IntPtr.Zero)
            {
                Rectangle r = Screen.PrimaryScreen.WorkingArea;

                if (r.Width > 0 && r.Height > 0)
                {
                    switch (scannerPosition)
                    {
                        case WindowPosition.BottomRight:
                        case WindowPosition.TopRight:
                            moveX = 0;
                            moveY = 0;
                            break;

                        case WindowPosition.BottomLeft:
                        case WindowPosition.TopLeft:
                            moveX = scannerForm.Width;
                            moveY = 0;
                            break;
                    }

                    Log.Debug("screenOffset=" + screenOffset + " moveX=" + moveX.ToString() + " moveY=" + moveY.ToString());
                    User32Interop.SetWindowPos(handle.ToInt32(), 0, moveX, moveY, (r.Width - scannerForm.Width), r.Height, 0x0040 | 0x0004);
                }
            }
            else
            {
                Log.Debug("fgWnd is zero");
            }
        }


        public static void DockWindow(IntPtr fgWindow, Form panel, Windows.WindowPosition scannerPosition)
        {
            Log.Debug("fgWindow is null is : " + (fgWindow == IntPtr.Zero));

            if (fgWindow == IntPtr.Zero)
            {
                return;
            }

            Process process = WindowActivityMonitor.GetProcessForWindow(fgWindow);
            if (process.ProcessName == Process.GetCurrentProcess().ProcessName)
            {
                Log.Debug("#$#$#$#$   This is an ACAT Window!!!!!");
                return;
            }

            if (panel != null)
            {
                Log.Debug("#$#$#$#$  Calling IsDialog");

                if (!IsDialog(fgWindow))
                {
                    Log.Debug("#$#$#$#$  Returned from IsDialog");

                    Log.Debug("#$#$#$#$  Calling DockAppWindowWithScanner for form " + (panel as Form).Name);
                    DockAppWindowWithScanner(scannerPosition, fgWindow, panel);
                }
                else
                {
                    Log.Debug("#$#$#$#$  Window is a dialog. will not dock");
                }
            }
        }
        public static void DockWindowWithLargestScanner(IntPtr fgWindow, Form panel, Windows.WindowPosition scannerPosition)
        {
            Log.Debug("#$#$#$#$  fgWindow is null is : " + (fgWindow == IntPtr.Zero));

            if (fgWindow == IntPtr.Zero)
            {
                return;
            }

            if (WidestScannerWidth == 0)
            {
                DockWindow(fgWindow, panel, scannerPosition);
                return;
            }

            Process process = WindowActivityMonitor.GetProcessForWindow(fgWindow);
            if (process.ProcessName == Process.GetCurrentProcess().ProcessName)
            {
                Log.Debug("#$#$#$#$   This is an ACAT Window!!!!!");
                return;
            }

            if (panel != null)
            {
                if (!IsDialog(fgWindow))
                {
                    User32Interop.RECT rect;
                    User32Interop.GetWindowRect(fgWindow, out rect);

                    int moveX = 0;
                    int moveY = 0;

                    Rectangle r = Screen.PrimaryScreen.WorkingArea;

                    if (r.Width > 0 && r.Height > 0)
                    {
                        switch (scannerPosition)
                        {
                            case WindowPosition.BottomRight:
                            case WindowPosition.TopRight:
                                moveX = 0;
                                moveY = 0;
                                break;

                            case WindowPosition.BottomLeft:
                            case WindowPosition.TopLeft:
                                moveX = WidestScannerWidth;
                                moveY = 0;
                                break;
                        }

                        User32Interop.SetWindowPos(fgWindow.ToInt32(), 0, moveX, moveY, (r.Width - WidestScannerWidth), r.Height, 0x0040 | 0x0004);
                    }
                }
                else
                {
                    Log.Debug("Window is a dialog. will not dock");
                }
            }
        }

        public static bool IsDialog(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            bool retVal = false;

            AutomationElement window = AutomationElement.FromHandle(handle);
            object objPattern;
            if (window.Current.ControlType == ControlType.Menu)
            {
                retVal = true;
            }
            else if (window.TryGetCurrentPattern(WindowPattern.Pattern, out objPattern))
            {
                WindowPattern windowPattern = objPattern as WindowPattern;
                retVal = (!windowPattern.Current.CanMinimize && !windowPattern.Current.CanMaximize) || windowPattern.Current.IsModal;
            }

            Log.Debug("returning " + retVal);

            return retVal;
        }

        /// <summary>
        /// Fades in the specified form
        /// </summary>
        /// <param name="form">form to fade in</param>
        public static void FadeIn(Form form)
        {
            SetOpacity(form, 0.0);

            var fadeInThread = new Thread(delegate()
            {
                fadeInProc(form);
            });
            fadeInThread.Start();
        }

        /// <summary>
        /// Gradually fades out the form
        /// </summary>
        /// <param name="form">form to fade out</param>
        public static void FadeOut(Form form)
        {
            var fadeOutThread = new Thread(
                delegate()
                {
                    fadeOutProc(form);
                });
            fadeOutThread.Start();
        }

        /// <summary>
        /// Finds the control under the cursor
        /// </summary>
        /// <param name="form">form under the cursor</param>
        /// <returns>the control that's under the cursor . null if none</returns>
        public static Control FindControlAtCursor(Form form)
        {
            Point pos = Cursor.Position;
            if (form.Bounds.Contains(pos))
            {
                return FindControlAtPoint(form, form.PointToClient(Cursor.Position));
            }

            return null;
        }

        /// <summary>
        /// Finds which control of a window is at the specified point
        /// </summary>
        /// <param name="container">the container window</param>
        /// <param name="pos">point to examine</param>
        /// <returns>the control that's at the point. null if none</returns>
        public static Control FindControlAtPoint(Control container, Point pos)
        {
            foreach (Control c in container.Controls)
            {
                if (c.Visible && c.Bounds.Contains(pos))
                {
                    var child = FindControlAtPoint(c, new Point(pos.X - c.Left, pos.Y - c.Top));
                    if (child == null)
                    {
                        return c;
                    }
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the icon associated with the specified window
        /// </summary>
        /// <param name="winHandle">window handle</param>
        /// <returns>the icon, null if it can't find one</returns>
        public static Icon GetAppIcon(IntPtr winHandle)
        {
            Log.Debug("hWnd=" + winHandle);

            IntPtr hIcon = User32Interop.SendMessage(winHandle, WM_GETICON, ICON_BIG, 0);

            if (hIcon == IntPtr.Zero)
            {
                hIcon = User32Interop.SendMessage(winHandle, WM_GETICON, ICON_SMALL, 0);
                if (hIcon == IntPtr.Zero)
                {
                    hIcon = User32Interop.SendMessage(winHandle, WM_GETICON, ICON_SMALL2, 0);
                    if (hIcon == IntPtr.Zero)
                    {
                        hIcon = GetClassLongPtr(winHandle, GCL_HICON);
                        if (hIcon == IntPtr.Zero)
                        {
                            hIcon = GetClassLongPtr(winHandle, GCL_HICONSM);
                            if (hIcon == IntPtr.Zero)
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            Icon icon = null;
            if (hIcon != IntPtr.Zero)
            {
                icon = Icon.FromHandle(hIcon);
            }

            return icon;
        }

        /// <summary>
        /// Returns the caret position in a text box.
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        public static int GetCaretPosition(TextBoxBase textBox)
        {
            if (textBox.InvokeRequired)
            {
                return (int) textBox.Invoke(new getCaretPosition(GetCaretPosition), textBox);
            }
            else
            {
                return textBox.SelectionStart;
            }
        }

        /// <summary>
        /// Gets window class long pointer for hWnd
        /// </summary>
        /// <param name="hWnd">window handle</param>
        /// <param name="nIndex">index number</param>
        /// <returns>window class long</returns>
        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            try
            {
                if (IntPtr.Size > 4)
                {
                    return User32Interop.GetClassLongPtr64(hWnd, nIndex);
                }

                uint ret = User32Interop.GetClassLongPtr32(hWnd, nIndex);
                return new IntPtr((int) ret); // without the cast, it may result in an overflow
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns the currently active fg window
        /// </summary>
        /// <returns>window handle</returns>
        public static IntPtr GetForegroundWindow()
        {
            return User32Interop.GetForegroundWindow();
        }

        /// <summary>
        /// Gets the transparency of a form
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="form">Form object</param>
        /// <param name="arg">opacity value</param>
        public static double GetOpacity(Form form)
        {
            if (form.InvokeRequired)
            {
                return (double) form.Invoke(new getOpacity(GetOpacity), form);
            }

            return form.Opacity;
        }

        /// <summary>
        /// Returns the OS version
        /// </summary>
        /// <returns>os version</returns>
        public static WindowsVersion GetOSVersion()
        {
            if (_windowsVersion != WindowsVersion.Unknown)
            {
                return _windowsVersion;
            }

            WindowsVersion windowsVersion = WindowsVersion.Unknown;

            try
            {
                var osInfo = Environment.OSVersion;
                if (osInfo.Version.Major == 6 && osInfo.Version.Minor == 1)
                {
                    windowsVersion = WindowsVersion.Win7;
                }

                if (osInfo.Version.Major == 6 && osInfo.Version.Minor == 2)
                {
                    windowsVersion = WindowsVersion.Win8;
                }

                if (osInfo.Version.Major == 10)
                {
                    windowsVersion = WindowsVersion.Win10;
                }
            }
            catch
            {
                windowsVersion = WindowsVersion.Unknown;
            }

            _windowsVersion = windowsVersion;

            return _windowsVersion;
        }

        /// <summary>
        /// Gets the position of the specified form. which
        /// corner is the scanner at?
        /// </summary>
        /// <param name="form">the scanner form</param>
        /// <returns>the position</returns>
        public static WindowPosition GetScannerPosition(Form form)
        {
            WindowPosition retVal;

            if (form.Left == 0 && form.Top == 0)
            {
                retVal = WindowPosition.TopLeft;
            }
            else if (form.Left == 0 && form.Top == Screen.PrimaryScreen.WorkingArea.Height - form.Height)
            {
                retVal = WindowPosition.BottomLeft;
            }
            else if (form.Left == 0)
            {
                retVal = WindowPosition.MiddleLeft;
            }
            else if (form.Top == 0 && form.Left == Screen.PrimaryScreen.WorkingArea.Width - form.Width)
            {
                retVal = WindowPosition.TopRight;
            }
            else if (form.Top == Screen.PrimaryScreen.WorkingArea.Height - form.Height &&
                     form.Left == Screen.PrimaryScreen.WorkingArea.Width - form.Width)
            {
                retVal = WindowPosition.BottomRight;
            }
            else
            {
                retVal = WindowPosition.MiddleRight;
            }

            return retVal;
        }

        /// <summary>
        /// Returns selected text in the text box
        /// </summary>
        /// <param name="control">text box</param>
        /// <returns>selected text</returns>
        public static String GetSelectedText(TextBoxBase control)
        {
            if (control.InvokeRequired)
            {
                return (String) control.Invoke(new getSelectedText(GetSelectedText), control);
            }

            return control.SelectedText;
        }

        /// <summary>
        /// Returns the text value of the control.
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        public static String GetText(Control control)
        {
            if (control.InvokeRequired)
            {
                return (String) control.Invoke(new getText(GetText), control);
            }

            return control.Text;
        }

        /// <summary>
        /// Gets text from the speficied window
        /// </summary>
        /// <param name="hWnd">window handle</param>
        /// <returns>the text</returns>
        public static string GetText(IntPtr hWnd)
        {
            var title = new StringBuilder();

            // Get the size of the string required to hold the window title.
            Int32 size = User32Interop.SendMessageIntInt(hWnd, User32Interop.WM_GETTEXTLENGTH, 0, 0).ToInt32();

            // If the return is 0, there is no title.
            if (size > 0)
            {
                title = new StringBuilder(size + 1);

                User32Interop.SendMessageStringBuilder(hWnd, User32Interop.WM_GETTEXT, title.Capacity, title);
            }
            return title.ToString();
        }

        /// <summary>
        /// Returns trackbar value
        /// Takes care of cross-thread invokations that would result in
        /// .NET exceptions
        /// </summary>
        /// <param name="trackBar">the trackbar object</param>
        /// <returns>position</returns>
        public static int GetTrackBarValueInt(TrackBar trackBar)
        {
            if (trackBar.InvokeRequired)
            {
                return (int) trackBar.Invoke(new getTrackBarValueInt(GetTrackBarValueInt), trackBar);
            }

            return trackBar.Value;
        }

        /// <summary>
        /// Gets the visibility of a control.
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        public static bool GetVisible(Control control)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    return (bool) control.Invoke(new getVisible(GetVisible), control);
                }

                return control.Visible;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Returns the class name for the specified window handle
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <returns>class name</returns>
        public static string GetWindowClassName(IntPtr hWnd)
        {
            var sbClassName = new StringBuilder(512);
            var length = User32Interop.GetClassName(hWnd, sbClassName, sbClassName.Capacity);

            return sbClassName.ToString();
        }

        /// <summary>
        /// Returns the window title of the specified window
        /// </summary>
        /// <param name="hwnd">handle to the window</param>
        /// <returns>the title</returns>
        public static String GetWindowTitle(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
            {
                var sb = new StringBuilder(256);
                User32Interop.GetWindowText(hwnd, sb, 256);
                return sb.ToString();
            }
            return String.Empty;
        }

        /// <summary>
        /// Hide windows taskbar
        /// </summary>
        public static void HideTaskbar()
        {
            IntPtr hwnd = User32Interop.FindWindow(_taskbarWinClass, String.Empty);
            if (hwnd != IntPtr.Zero)
            {
                User32Interop.ShowWindow(hwnd.ToInt32(), ShowWindowFlags.SW_HIDE);
            }
        }

        /// <summary>
        /// Checks if the window has a caption and a title bar, if so
        /// returns true
        /// </summary>
        /// <param name="hWnd">window handle</param>
        /// <returns>true if it is</returns>
        public static bool IsApplicationWindow(IntPtr hWnd)
        {
            int style = User32Interop.GetWindowLong(hWnd, User32Interop.GWL_STYLE);

            // check for WS_VISIBLE and WS_CAPTION flags
            return (style & 0x10C00000) == 0x10C00000;
        }

        /// <summary>
        /// On Win 10, certain Store apps such as Calculator,
        /// Photos, Movies and TV are cloaked (hidden) even though
        /// their visibility is 'true'.  This function detects
        /// these windows
        /// </summary>
        /// <param name="hWnd">window handle to check</param>
        /// <returns>true if the window is a cloaked window</returns>
        public static bool IsCloakedWindow(IntPtr hWnd)
        {
            if (Windows.GetOSVersion() != Windows.WindowsVersion.Win10)
            {
                return false;
            }

            bool isCloaked;

            DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.Cloaked, out isCloaked, 8);

            return isCloaked;
        }

        /// <summary>
        /// returns if the specified handle is the desktop window
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsDesktopWindow(IntPtr handle)
        {
            return handle == User32Interop.GetDesktopWindow();
        }

        /// <summary>
        /// Checks if window is maximized or not
        /// </summary>
        /// <param name="handle">window handle</param>
        /// <returns>true if it is</returns>
        public static bool IsMaximized(IntPtr handle)
        {
            var placement = new User32Interop.WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            User32Interop.GetWindowPlacement(handle, out placement);
            return placement.showCmd == ShowWindowFlags.SW_MAXIMIZE;
        }

        /// <summary>
        /// Checks if window is minimzied or not
        /// </summary>
        /// <param name="handle">window handle</param>
        /// <returns>true if it is</returns>
        public static bool IsMinimized(IntPtr handle)
        {
            int style = User32Interop.GetWindowLong(handle, GWL_STYLE);

            return ((style & WindowStyleFlags.WS_MINIMIZE) == WindowStyleFlags.WS_MINIMIZE);
        }

        /// <summary>
        /// Checks if any part of the specified window is obscured by
        /// a window belonging to a process other than ours
        /// </summary>
        /// <param name="windowHandle">Window to check for</param>
        /// <returns>true if it is</returns>
        public static bool IsObscuredByNonACATWindows(IntPtr windowHandle)
        {
            if (windowHandle == null ||
                windowHandle == IntPtr.Zero ||
                !User32Interop.IsWindowVisible(windowHandle))
            {
                return false;
            }

            IntPtr hWnd = windowHandle;

            // store windows we have already visited
            var windowCache = new HashSet<IntPtr> {hWnd};

            User32Interop.RECT windowRect;
            User32Interop.GetWindowRect(hWnd, out windowRect);

            // check if any of the windows intersects with our window
            while ((hWnd = User32Interop.GetWindow(hWnd, User32Interop.GW_HWNDPREV)) != IntPtr.Zero &&
                   !windowCache.Contains(hWnd))
            {
                User32Interop.RECT rect;
                User32Interop.RECT intersection;

                windowCache.Add(hWnd);

                // is this a form created by our app?
                Control control = Form.FromHandle(hWnd);
                if (control != null)
                {
                    continue;
                }

                if (User32Interop.IsWindowVisible(hWnd) &&
                    !IsMinimized(hWnd) &&
                    User32Interop.GetWindowRect(hWnd, out rect) &&
                    User32Interop.IntersectRect(out intersection, ref windowRect, ref rect))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if any part of the specified window is obscured by
        /// another window. The other window could be one of this
        /// application's windows
        /// </summary>
        /// <param name="windowHandle">Window to check for</param>
        /// <returns>true if it is</returns>
        public static bool IsObscuredWindow(IntPtr windowHandle)
        {
            if (windowHandle == null ||
                windowHandle == IntPtr.Zero ||
                !User32Interop.IsWindowVisible(windowHandle))
            {
                return false;
            }

            IntPtr hWnd = windowHandle;

            // store windows we have already visited
            var windowCache = new HashSet<IntPtr> {hWnd};

            User32Interop.RECT windowRect;
            User32Interop.GetWindowRect(hWnd, out windowRect);

            // check if any of the windows intersects with our window
            while ((hWnd = User32Interop.GetWindow(hWnd, User32Interop.GW_HWNDPREV)) != IntPtr.Zero &&
                   !windowCache.Contains(hWnd))
            {
                User32Interop.RECT rect;
                User32Interop.RECT intersection;

                windowCache.Add(hWnd);

                if (User32Interop.IsWindowVisible(hWnd) &&
                    !IsMinimized(hWnd) &&
                    User32Interop.GetWindowRect(hWnd, out rect) &&
                    User32Interop.IntersectRect(out intersection, ref windowRect, ref rect))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Maximizes the window
        /// </summary>
        /// <param name="handle">window handle</param>
        public static void MaximizeWindow(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                User32Interop.ShowWindow(handle.ToInt32(), ShowWindowFlags.SW_SHOWMAXIMIZED);
            }
        }

        /// <summary>
        /// Minimizes a window
        /// </summary>
        /// <param name="handle">window handle</param>
        public static void MinimizeWindow(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                User32Interop.ShowWindow(handle.ToInt32(), ShowWindowFlags.SW_MINIMIZE);
            }
        }

        /// <summary>
        /// Windows PostMessage()
        /// </summary>
        public static bool PostMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, UInt32 lParam)
        {
            return User32Interop.PostMessage(hWnd, Msg, wParam, lParam);
        }

        /// <summary>
        /// Restores the window from the maximized position
        /// </summary>
        /// <param name="handle">window handle</param>
        public static void RestoreWindow(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                User32Interop.ShowWindow(handle.ToInt32(), ShowWindowFlags.SW_RESTORE);
            }
        }

        /// <summary>
        /// Windows SendMessage()
        /// </summary>
        public static IntPtr SendMessage(IntPtr hWnd, int Msg, int wparam, int lparam)
        {
            return User32Interop.SendMessage(hWnd, Msg, wparam, lparam);
        }

        /// <summary>
        /// Sets the window as the active window
        /// </summary>
        /// <param name="hwnd">window handle</param>
        /// <returns>true</returns>
        public static void SetActiveWindow(IntPtr hwnd)
        {
            try
            {
                User32Interop.SetActiveWindow(hwnd);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the background color of the widget.  Takes care
        /// of cross-thread invokations that would result in
        /// .NET exceptions
        /// </summary>
        /// <param name="color">Color to set</param>
        public static void SetBackgroundColor(Control control, Color color)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new setBackgroundColor(SetBackgroundColor), control, color);
                }
                else
                {
                    control.BackColor = color;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the caret position in a text box.
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        public static void SetCaretPosition(TextBoxBase textBox, int position)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new setCaretPosition(SetCaretPosition), textBox, position);
            }
            else
            {
                textBox.SelectionStart = position;
            }
        }

        /// <summary>
        /// Sets focus to the control
        /// </summary>
        /// <param name="control">control to set focus to</param>
        /// <returns>true on success</returns>
        public static bool SetFocus(Control control)
        {
            if (control.InvokeRequired)
            {
                return (bool) control.Invoke(new setFocus(SetFocus), control);
            }

            return control.Focus();
        }

        /// <summary>
        /// Sets current focus to the desktop window
        /// </summary>
        public static void SetFocusToDesktop()
        {
            IntPtr lHwnd = User32Interop.FindWindow("Shell_TrayWnd", null);
            if (lHwnd != IntPtr.Zero)
            {
                SetForegroundWindow(lHwnd);
            }
            else
            {
                Point oldPos = Cursor.Position;
                MouseUtils.ClickLeftMouseButton(0, 0);
                Cursor.Position = oldPos;
            }
        }

        /// <summary>
        /// Sets the foreground color of the widget.  Takes care
        /// of cross-thread invokations that would result in
        /// .NET exceptions
        /// </summary>
        /// <param name="color">Color to set</param>
        public static void SetForegroundColor(Control control, Color color)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new setForegroundColor(SetForegroundColor), control, color);
                }
                else
                {
                    control.ForeColor = color;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the fg window to the specified window
        /// </summary>
        /// <param name="hwnd">window handle</param>
        /// <returns>true on success</returns>
        public static bool SetForegroundWindow(IntPtr hwnd)
        {
            bool retVal = false;
            try
            {
                retVal = User32Interop.SetForegroundWindow(hwnd);
            }
            catch
            {
            }
            return retVal;
        }

        /// <summary>
        /// Sets the width of the foreground window to a certain
        /// percentage of the display monitor width.
        /// </summary>
        /// <param name="scannerPosition">where to position the window?</param>
        /// <param name="percent">the percent width</param>
        public static void SetForegroundWindowSizePercent(WindowPosition scannerPosition, int percent)
        {
            SetWindowSizePercent(User32Interop.GetForegroundWindow(), scannerPosition, percent);
        }

        /// <summary>
        /// Sets the form style to not activate and also composited
        /// for better viewing experience
        /// </summary>
        /// <param name="createParams">Windows create params</param>
        /// <returns>modified createparms object</returns>
        public static CreateParams SetFormStyles(CreateParams createParams)
        {
            createParams.ExStyle |= WindowStyleFlags.WS_EX_NOACTIVATE;
            createParams.ExStyle |= WindowStyleFlags.WS_EX_COMPOSITED;
            return createParams;
        }

        /// <summary>
        /// Sets the image in a picturebox and invalidates the picture box
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="image"></param>
        public static void SetImage(PictureBox box, Image image)
        {
            if (box.InvokeRequired)
            {
                box.Invoke(new setImage(SetImage), box, image);
            }
            else
            {
                box.Image = image;
                box.Invalidate();
            }
        }

        /// <summary>
        /// Sets the transparency of a form
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="form">Form object</param>
        /// <param name="arg">opacity value</param>
        public static void SetOpacity(Form form, double arg)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new setOpacity(SetOpacity), form, arg);
            }
            else
            {
                form.Opacity = arg;
            }
        }

        /// <summary>
        /// Set region for a control
        /// </summary>
        /// <param name="control">control to set region for</param>
        /// <param name="region">region to set</param>
        public static void SetRegion(Control control, Region region)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new setRegion(SetRegion), control, region);
                }
                else
                {
                    control.Region = region;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the text for the control.
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="Control">control to set text for</param>
        /// <param name="text">text to set</param>
        public static void SetText(Control control, String text)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new setText(SetText), control, text);
            }
            else
            {
                control.Text = text;
            }
        }

        /// <summary>
        /// Sets topmost to true
        /// </summary>
        /// <param name="form">which form?</param>
        /// <param name="topMost">set to true for topmost, false otherwise</param>
        public static void SetTopMost(Form form, bool topMost = true)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new setTopMost(SetTopMost), form, topMost);
            }
            else
            {
                // need to toggle to false and
                // then to true for this to take effect

                if (topMost)
                {
                    form.TopMost = false;
                    form.TopMost = true;
                }
                else
                {
                    form.TopMost = false;
                }
            }
        }

        /// <summary>
        /// Sets value of a tackbar as an integer
        /// Takes care of cross-thread invokations that would result in
        /// .NET exceptions
        /// </summary>
        /// <param name="trackBar"></param>
        /// <param name="positionValue"></param>
        public static void SetTrackBarValue(TrackBar trackBar, int positionValue)
        {
            if (trackBar.InvokeRequired)
            {
                trackBar.Invoke(new setTrackBarValue(SetTrackBarValue), trackBar, positionValue);
            }
            else
            {
                if ((positionValue <= trackBar.Maximum) && (positionValue >= trackBar.Minimum))
                {
                    trackBar.Value = positionValue;
                }
                else
                {
                    Log.Debug("Trying to set trackbar outside legal bounds!");
                }
            }
        }

        /// <summary>
        /// Sets the visibility of the control
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        public static void SetVisible(Control control, bool visible)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new setVisible(SetVisible), control, visible);
            }
            else
            {
                control.Visible = visible;
            }
        }

        /// <summary>
        /// Set the window position
        /// </summary>
        /// <param name="form">form to reposition</param>
        /// <param name="position">the new position</param>
        public static void SetWindowPosition(Form form, WindowPosition position)
        {
            form.StartPosition = FormStartPosition.Manual;

            Log.Debug("Before setposition " + position);

            switch (position)
            {
                case WindowPosition.TopRight:
                    form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - form.Width, 0);
                    break;

                case WindowPosition.TopLeft:
                    form.Location = new Point(0, 0);
                    break;

                case WindowPosition.BottomRight:
                    form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - form.Width,
                        Screen.PrimaryScreen.WorkingArea.Height - form.Height);
                    break;

                case WindowPosition.BottomLeft:
                    form.Location = new Point(0, Screen.PrimaryScreen.WorkingArea.Height - form.Height);
                    break;

                case WindowPosition.MiddleRight:
                    form.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - form.Width,
                        (Screen.PrimaryScreen.WorkingArea.Height - form.Height)/2);
                    break;

                case WindowPosition.MiddleLeft:
                    form.Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - form.Height)/2);
                    break;

                case WindowPosition.CenterScreen:
                    form.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - form.Width)/2,
                        (Screen.PrimaryScreen.WorkingArea.Height - form.Height)/2);
                    break;
            }
            Log.Debug("After setposition " + position);
        }

        /// <summary>
        /// Set the window position
        /// </summary>
        /// <param name="form">form to reposition</param>
        /// <param name="insertAfter">insert after this window</param>
        /// <param name="position">new position</param>
        public static void SetWindowPosition(Form form, IntPtr insertAfter, WindowPosition position)
        {
            form.StartPosition = FormStartPosition.Manual;
            var location = new Point(0, 0);

            switch (position)
            {
                case WindowPosition.TopRight:
                    location = new Point(Screen.PrimaryScreen.WorkingArea.Width - form.Width, 0);
                    break;

                case WindowPosition.TopLeft:
                    location = new Point(0, 0);
                    break;

                case WindowPosition.BottomRight:
                    location = new Point(Screen.PrimaryScreen.WorkingArea.Width - form.Width,
                        Screen.PrimaryScreen.WorkingArea.Height - form.Height);
                    break;

                case WindowPosition.BottomLeft:
                    location = new Point(0, Screen.PrimaryScreen.WorkingArea.Height - form.Height);
                    break;

                case WindowPosition.CenterScreen:
                    location = new Point((Screen.PrimaryScreen.WorkingArea.Width - form.Width)/2,
                        (Screen.PrimaryScreen.WorkingArea.Height - form.Height)/2);
                    break;
            }

            User32Interop.SetWindowPos(form.Handle.ToInt32(), insertAfter.ToInt32(), location.X, location.Y, 0, 0,
                0x0040 | 0x0001);
        }

        /// <summary>
        /// Set window pos to the specified position and notify when window position
        /// has changed
        /// </summary>
        /// <param name="form">The form</param>
        /// <param name="position">Where to set the position</param>
        public static void SetWindowPositionAndNotify(Form form, WindowPosition position)
        {
            Log.Debug("Setting position to " + position);
            SetWindowPosition(form, position);
            if (EvtWindowPositionChanged != null)
            {
                Log.Debug("Calling evtpositionchanged with " + position);

                EvtWindowPositionChanged(form, position);
            }
        }

        /// <summary>
        /// Set window pos to the specified position and notify when window position
        /// has changed
        /// </summary>
        /// <param name="form">The form</param>
        /// <param name="position">Where to set the position</param>
        public static void SetWindowPositionAndNotify(Form form, IntPtr insertAfter, WindowPosition position)
        {
            SetWindowPosition(form, insertAfter, position);
            if (EvtWindowPositionChanged != null)
            {
                EvtWindowPositionChanged(form, position);
            }
        }

        /// <summary>
        /// Partial maximize.  Sets the window width to
        /// a percent of the display width.
        /// </summary>
        /// <param name="handle">handle of the window</param>
        /// <param name="scannerPosition">which corner should the window be positioned at?</param>
        /// <param name="percent">percentage of display monitor width</param>
        public static void SetWindowSizePercent(IntPtr handle, WindowPosition scannerPosition, int percent)
        {
            Log.Debug("Entering...scannerPosition=" + scannerPosition.ToString() + " percent=" + percent.ToString());
            int screenOffset = 0;
            int moveX = 0;
            int moveY = 0; // not really using Y-axis yet but something to keep in mind for the future

            if (percent <= 10)
            {
                return;
            }

            if (percent > 100)
                percent = 100;

            screenOffset = 100 - percent;

            if (handle != IntPtr.Zero)
            {
                Rectangle r = Screen.PrimaryScreen.WorkingArea;

                if (r.Width > 0 && r.Height > 0)
                {
                    Log.Debug("Resize window to " + (r.Width*percent)/100 + ", " + r.Height);

                    switch (scannerPosition)
                    {
                        case WindowPosition.TopRight:
                            moveX = 0;
                            moveY = 0;
                            break;

                        case WindowPosition.TopLeft:
                            moveX = (r.Width*screenOffset)/100;
                            moveY = 0;
                            break;

                        case WindowPosition.BottomLeft:
                            moveX = (r.Width*screenOffset)/100;
                            moveY = 0;
                            break;

                        case WindowPosition.BottomRight:
                            moveX = 0;
                            moveY = 0;
                            break;
                    }

                    Log.Debug("screenOffset=" + screenOffset + " moveX=" + moveX.ToString() + " moveY=" +
                              moveY.ToString());
                    User32Interop.SetWindowPos(handle.ToInt32(), 0, moveX, moveY, (r.Width*percent)/100, r.Height,
                        0x0040 | 0x0004);
                }
            }
            else
            {
                Log.Debug("fgWnd is zero");
            }
        }

        /// <summary>
        /// Shows the child window, assigns parent as the owner
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public static void Show(Form parent, Form child)
        {
            if (parent.InvokeRequired)
            {
                parent.Invoke(new show(Show), parent, child);
            }
            else
            {
                child.Show(parent);
            }
        }

        /// <summary>
        /// Shows the child window as a dialog of the parent window
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public static void ShowDialog(Form parent, Form child)
        {
            if (parent.InvokeRequired)
            {
                parent.Invoke(new showDialog(ShowDialog), parent, child);
            }
            else
            {
                child.ShowDialog(parent);
                if (child.Owner == null)
                {
                    Log.Debug("child.parent is null");
                }
            }
        }

        /// <summary>
        /// Shows the specified form
        /// This is just a helper function that takes care of
        /// cross-thread invokations that would result in .NET
        /// exceptions.
        /// </summary>
        /// <param name="form"></param>
        public static void ShowForm(Form form)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new showForm(ShowForm), form);
            }
            else
            {
                form.Show();
            }
        }

        /// <summary>
        /// Show the window as topmost without activating it. Sets
        ///  the form's parent to parentForm
        /// </summary>
        /// <param name="parentForm">the parent of the form</param>
        /// <param name="form">the form</param>
        public static void ShowInactiveTopmost(Form parentForm, Form form)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new show(ShowInactiveTopmost), parentForm, form);
            }
            else
            {
                const int SW_SHOWNOACTIVATE = 4;
                const int HWND_TOPMOST = -1;
                const uint SWP_NOACTIVATE = 0x0010;

                form.Owner = parentForm;

                User32Interop.ShowWindow(form.Handle.ToInt32(), SW_SHOWNOACTIVATE);
                User32Interop.SetWindowPos(form.Handle.ToInt32(), HWND_TOPMOST,
                    form.Left, form.Top, form.Width, form.Height,
                    SWP_NOACTIVATE);
            }
        }

        /// <summary>
        /// Show or hide the specified form in the Windows taskbar
        /// </summary>
        /// <param name="control">form</param>
        /// <param name="show">set to true to show</param>
        public static void ShowInTaskbar(Form control, bool show)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new showInTaskbar(ShowInTaskbar), control, show);
            }
            else
            {
                control.ShowInTaskbar = show;
            }
        }

        /// <summary>
        /// Show windows taskbar
        /// </summary>
        public static void ShowTaskbar()
        {
            IntPtr hwnd = User32Interop.FindWindow(_taskbarWinClass, String.Empty);
            if (hwnd != IntPtr.Zero)
            {
                User32Interop.ShowWindow(hwnd.ToInt32(), ShowWindowFlags.SW_SHOWNORMAL);
            }
        }

        /// <summary>
        /// If show is set to true, shows the window borders and title
        /// </summary>
        /// <param name="form">the target form</param>
        /// <param name="show">true to show border/title</param>
        /// <param name="title">title to set</param>
        public static void ShowWindowBorder(Form form, bool show, String title = "")
        {
            if (show)
            {
                form.Text = title;
                form.ControlBox = false;
                form.FormBorderStyle = FormBorderStyle.FixedSingle;
            }
            else
            {
                form.FormBorderStyle = FormBorderStyle.None;
            }
        }

        /// <summary>
        /// Shows a window without setting focus to it
        /// </summary>
        /// <param name="form">the window form</param>
        public static void ShowWindowWithoutActivation(Form form)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new showWIndowWithoutActivation(ShowWindowWithoutActivation), form);
            }
            else
            {
                int handle = form.Handle.ToInt32();
                User32Interop.ShowWindow(handle, ShowWindowFlags.SW_SHOWNA);
            }
        }

        /// <summary>
        /// Sets the child's owner to parent and shows
        /// it without setting focus to it
        /// </summary>
        /// <param name="parent">parent form</param>
        /// <param name="child">child form</param>
        public static void ShowWithoutActivation(Form parent, Form child)
        {
            if (parent.InvokeRequired)
            {
                parent.Invoke(new show(ShowWithoutActivation), parent, child);
            }
            else
            {
                child.Owner = parent;
                ShowWindowWithoutActivation(child);
            }
        }

        /// <summary>
        /// Toggles size of the fg window between maximized to partially
        /// maximized
        /// </summary>
        /// <param name="scannerPosition">where to position the window</param>
        /// <param name="percent">the partial maximize percentage</param>
        public static void ToggleSnapForegroundWindow(WindowPosition scannerPosition, int percent)
        {
            IntPtr fgWindow = GetForegroundWindow();
            if (fgWindow != IntPtr.Zero)
            {
                if (IsMaximized(fgWindow))
                {
                    RestoreWindow(fgWindow);
                    SetForegroundWindowSizePercent(scannerPosition, percent);
                }
                else
                {
                    MaximizeWindow(fgWindow);
                }
            }
        }

        /// <summary>
        /// Toggles the window size of the foreground window between maximizing and
        /// docking with the scanner
        /// </summary>
        /// <param name="scanner">The scanner to dock to</param>
        /// <param name="scannerPosition">Position of the scanner</param>
        /// <param name="dockWithLargestScanner">Set to true if dock to the largest scanner</param>
        public static void ToggleForegroundWindowMaximizeDock(Form scanner, Windows.WindowPosition scannerPosition, bool dockWithLargestScanner = false)
        {
            IntPtr fgWindow = GetForegroundWindow();

            if (fgWindow != IntPtr.Zero)
            {
                Process process = WindowActivityMonitor.GetProcessForWindow(fgWindow);
                if (process.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    Log.Debug("This is an ACAT WIndow!");
                    return;
                }

                if (IsDialog(fgWindow))
                {
                    return;
                }

                if (IsMaximized(fgWindow))
                {
                    RestoreWindow(fgWindow);
                    if (dockWithLargestScanner)
                    {
                        DockWindowWithLargestScanner(fgWindow, scanner, scannerPosition);
                    }
                    else
                    {
                        DockWindow(fgWindow, scanner, scannerPosition);
                    }
                }
                else
                {
                    MaximizeWindow(fgWindow);
                }
            }
        }

        /// <summary>
        /// In Win8 and Win10, there are undesirable side-effects where the
        /// scanners are scaled imporperly.  Turn off DPI awareness to fix this
        /// </summary>
        public static void TurnOffDPIAwareness()
        {
            try
            {
                var osVersion = GetOSVersion();

                if (osVersion == WindowsVersion.Win8 || osVersion == WindowsVersion.Win10)
                {
                    SetProcessDpiAwareness(_Process_DPI_Awareness.Process_DPI_Unaware);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Unselect text in textbox
        /// </summary>
        /// <param name="control">the text box control</param>
        public static void UnselectText(TextBoxBase control)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new unselectText(UnselectText), control);
            }
            else
            {
                control.SelectionLength = 0;
                if (!String.IsNullOrEmpty(control.Text))
                {
                    control.Select(control.Text.Length, 0);
                }
            }
        }

        /// <summary>
        /// Returns the DPI of the monitor
        /// </summary>
        /// <returns>DPI</returns>
        public static float GetDPI()
        {
            float defaultDPI = 96.0f;

            if (_dpiX != 0.0f)
            {
                return _dpiX;
            }

            Form form = new Form();

            float retVal;

            Graphics g = form.CreateGraphics();
            try
            {
                retVal = g.DpiX;
            }
            catch
            {
                retVal = defaultDPI;
            }
            finally
            {
                g.Dispose();
            }

            _dpiX = retVal;

            return retVal;
        }

        /// <summary>
        /// Returns true if the dpi is 96.0f
        /// </summary>
        /// <returns>true if default dpi</returns>
        public static bool IsDefaultDPI()
        {
            float dpi = GetDPI();
            return (dpi <= 96.0f);
        }

        /// <summary>
        /// On Win10, checks if the window belongs to ApplicationFrameHost process
        /// </summary>
        /// <param name="handle">window handle</param>
        /// <returns>true if it is</returns>
        public static bool IsApplicationFrameHostProcessWindow(IntPtr handle)
        {
            try
            {
                int processId;

                if (GetOSVersion() != WindowsVersion.Win10)
                {
                    return false;
                }

                User32Interop.GetWindowThreadProcessId(handle, out processId);
                var process = Process.GetProcessById(processId);
                return (String.Compare(process.ProcessName, "ApplicationFrameHost", true) == 0);
            }
            catch
            {
                return false;

            }
        }

        /// <summary>
        /// Checks if the two forms overlap
        /// </summary>
        /// <param name="form1">first form</param>
        /// <param name="form2">second form</param>
        /// <returns>true if they do</returns>
        public static bool CheckOverlap(Form form1, Form form2)
        {
            User32Interop.RECT windowRect;
            User32Interop.GetWindowRect(form1.Handle, out windowRect);

            User32Interop.RECT rect;
            User32Interop.GetWindowRect(form2.Handle, out rect);

            User32Interop.RECT intersection;
            return User32Interop.IntersectRect(out intersection, ref windowRect, ref rect);
        }
        
        /// <summary>
        /// Thread proc for CloseAsync()
        /// </summary>
        /// <param name="form">form to close</param>
        private static void closeFormThreadProc(Form form)
        {
            Thread.Sleep(250);
            CloseForm(form);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);

        /// <summary>
        /// Threadproc for fade-in
        /// </summary>
        /// <param name="form">form to fade in</param>
        private static void fadeInProc(Form form)
        {
            while (true)
            {
                double opacity = GetOpacity(form);
                opacity += 0.05;
                SetOpacity(form, opacity);
                if (opacity >= 0.8)
                {
                    SetOpacity(form, 1.0);
                    if (EvtFadeInComplete != null)
                    {
                        EvtFadeInComplete(form);
                    }
                    return;
                }
                Thread.Sleep(30);
            }
        }

        /// <summary>
        /// Thread proc for fading out
        /// </summary>
        /// <param name="form">which form to fade out</param>
        private static void fadeOutProc(Form form)
        {
            while (true)
            {
                double opacity = GetOpacity(form);
                opacity -= 0.05;

                SetOpacity(form, opacity);
                if (opacity > 0.0)
                {
                    Thread.Sleep(30);
                }
                else
                {
                    if (EvtFadeOutComplete != null)
                    {
                        EvtFadeOutComplete(form);
                    }
                    return;
                }
            }
        }

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(_Process_DPI_Awareness value);

        public struct ShowWindowFlags
        {
            public const int SW_FORCEMINIMIZE = 11;
            public const int SW_HIDE = 0;
            public const int SW_MAXIMIZE = 3;
            public const int SW_MINIMIZE = 6;
            public const int SW_RESTORE = 9;
            public const int SW_SHOW = 5;
            public const int SW_SHOWDEFAULT = 10;
            public const int SW_SHOWMAXIMIZED = 3;
            public const int SW_SHOWMINIMIZED = 2;
            public const int SW_SHOWMINNOACTIVE = 7;
            public const int SW_SHOWNA = 8;
            public const int SW_SHOWNOACTIVATE = 4;
            public const int SW_SHOWNORMAL = 1;
        };

        public struct WindowStyleFlags
        {
            public const UInt32 WS_BORDER = 0x800000;
            public const UInt32 WS_CAPTION = 0xC00000;
            public const UInt32 WS_CHILD = 0x40000000;
            public const UInt32 WS_CLIPCHILDREN = 0x2000000;
            public const UInt32 WS_CLIPSIBLINGS = 0x4000000;
            public const UInt32 WS_DISABLED = 0x8000000;
            public const UInt32 WS_DLGFRAME = 0x400000;
            public const int WS_EX_COMPOSITED = 0x02000000;
            public const int WS_EX_NOACTIVATE = 0x8000000;
            public const UInt32 WS_GROUP = 0x20000;
            public const UInt32 WS_HSCROLL = 0x100000;
            public const UInt32 WS_MAXIMIZE = 0x1000000;
            public const UInt32 WS_MAXIMIZEBOX = 0x10000;
            public const UInt32 WS_MINIMIZE = 0x20000000;
            public const UInt32 WS_MINIMIZEBOX = 0x20000;
            public const UInt32 WS_OVERLAPPED = 0;
            public const UInt32 WS_POPUP = 0x80000000;
            public const UInt32 WS_SYSMENU = 0x80000;
            public const UInt32 WS_TABSTOP = 0x10000;
            public const UInt32 WS_THICKFRAME = 0x40000;
            public const UInt32 WS_VISIBLE = 0x10000000;
            public const UInt32 WS_VSCROLL = 0x200000;
        };
    }
}