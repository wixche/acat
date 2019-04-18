﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="MenuPanelBase.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.PanelManagement.CommandDispatcher;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Core.WidgetManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;

namespace ACAT.Lib.Core.PanelManagement
{
    /// <summary>
    /// Base class for all contextual menu scanners that display
    /// icons and text.  The height of the scanner is dynamically
    /// computed depending on how many menu items are there
    /// </summary>
    public partial class MenuPanelBase : Form, IScannerPanel
    {
        /// <summary>
        /// Title of the scanner
        /// </summary>
        public String Title = String.Empty;

        /// <summary>
        /// The root widget representing this scanner form
        /// </summary>
        protected Widget rootWidget;

        /// <summary>
        /// ScannerCommon object for all the heavy lifting
        /// </summary>
        protected ScannerCommon scannerCommon;

        /// <summary>
        /// Startup args passed by the application
        /// </summary>
        protected StartupArg startupArg;

        /// <summary>
        /// Startup arguments containing contextual information
        /// </summary>
        protected object startupCommandArg;

        /// <summary>
        /// The command dispatcher to execute commands
        /// </summary>
        private readonly RunCommandDispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        public MenuPanelBase()
        {
            InitializeComponent();
            _dispatcher = new RunCommandDispatcher(this);
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="panelClass">Scanner class</param>
        /// <param name="panelTitle">Title of the scanner</param>
        public MenuPanelBase(String panelClass, String panelTitle)
        {
            InitializeComponent();
            Load += ContextMenu_Load;
            FormClosing += ContextMenu_FormClosing;

            Title = panelTitle;
            PanelClass = panelClass;
            _dispatcher = new RunCommandDispatcher(this);
        }

        /// <summary>
        /// Gets the command dispatcher object
        /// </summary>
        public virtual RunCommandDispatcher CommandDispatcher
        {
            get { return _dispatcher; }
        }

        /// <summary>
        /// Gets the descriptor for this class
        /// </summary>
        public IDescriptor Descriptor
        {
            get { return DescriptorAttribute.GetDescriptor(GetType()); }
        }

        /// <summary>
        /// Gets this form
        /// </summary>
        public Form Form
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the scanner class
        /// </summary>
        public String PanelClass { get; protected set; }

        /// <summary>
        /// Gets the PanelCommon object
        /// </summary>
        public IPanelCommon PanelCommon { get { return scannerCommon; } }

        /// <summary>
        /// Gets the scannerCommon object
        /// </summary>
        public ScannerCommon ScannerCommon
        {
            get { return scannerCommon; }
        }

        /// <summary>
        /// Gets the synchronization object for this scanner
        /// </summary>
        public SyncLock SyncObj
        {
            get { return scannerCommon.SyncObj; }
        }

        /// <summary>
        /// Gets the TextController object
        /// </summary>
        public ITextController TextController
        {
            get { return scannerCommon.TextController; }
        }

        /// <summary>
        /// Set the form style
        /// </summary>
        protected override CreateParams CreateParams
        {
            get { return Windows.SetFormStyles(base.CreateParams); }
        }

        /// <summary>
        /// Tell windows not to set focus to this form when
        /// user clicks on it
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>
        /// Called to check if the specified widget in arg should
        /// be enabled or not.  This function is called perfiodically
        /// because application context may change any time. Set
        /// the handled property in arg to true if this is handled.
        /// </summary>
        /// <param name="arg">argument</param>
        /// <returns>true on success</returns>
        public bool CheckCommandEnabled(CommandEnabledArg arg)
        {
            return false;
        }

        /// <summary>
        /// Performs initialization
        /// </summary>
        /// <param name="startupArg">Startup arguments</param>
        /// <returns>true on success</returns>
        public bool Initialize(StartupArg arg)
        {
            PanelClass = arg.PanelClass;
            startupCommandArg = arg.Arg;
            startupArg = arg;

            scannerCommon = new ScannerCommon(this);

            if (!scannerCommon.Initialize(startupArg))
            {
                Log.Debug("Could not initialize form " + Name);
                return false;
            }

            rootWidget = PanelCommon.RootWidget;

            onInitialize();

            return true;
        }

        /// <summary>
        /// Notification to indicate there was a focus switch
        /// in the application window
        /// </summary>
        /// <param name="monitorInfo"></param>
        public void OnFocusChanged(WindowActivityMonitorInfo monitorInfo)
        {
            scannerCommon.OnFocusChanged(monitorInfo);
        }

        /// <summary>
        /// Called to pause the scanner
        /// </summary>
        public virtual void OnPause()
        {
            scannerCommon.OnPause();
        }

        /// <summary>
        /// Should we allow ACAT to switch this scanner out when
        /// there is a application context switch.
        /// </summary>
        /// <param name="arg">contextual info</param>
        /// <returns>true</returns>
        public virtual bool OnQueryPanelChange(PanelRequestEventArgs arg)
        {
            return true;
        }

        /// <summary>
        /// Called to resume the scanner
        /// </summary>
        public virtual void OnResume()
        {
            scannerCommon.OnResume();
        }

        /// <summary>
        /// Invoked when the user actuates a button in
        /// the scanner form
        /// </summary>
        /// <param name="widget">widget actuated</param>
        /// <param name="handled">was this handled here?</param>
        public virtual void OnWidgetActuated(Widget widget, ref bool handled)
        {
            handled = false;
        }

        /// <summary>
        /// Unused
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="widget"></param>
        public void SetTargetControl(Form parent, Widget widget)
        {
        }

        /// <summary>
        /// Sets the title caption of the menu
        /// </summary>
        /// <param name="title">caption to set</param>
        public void SetTitle(String title)
        {
            Title = title;

            if (scannerCommon != null)
            {
                Widget widget = PanelCommon.RootWidget.Finder.FindChild("ContextMenuTitle");
                if (widget != null)
                {
                    widget.SetText(title);
                }
            }
        }

        /// <summary>
        /// Override this to perform cleanup.  Called when the
        /// form has closed
        /// </summary>
        protected virtual void OnClose()
        {
        }

        /// <summary>
        /// Override this to perform cleanup logic. Make sure
        /// this function is called from the derived class
        /// </summary>
        /// <param name="e">event arg</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            scannerCommon.OnFormClosing(e);
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Override this to perform additional initialization
        /// in the derived class
        /// </summary>
        protected virtual void onInitialize()
        {
        }

        /// <summary>
        /// Called when the form is loaded.  Override this in
        /// the derived class to perform additional initialization
        /// </summary>
        protected virtual void OnLoad()
        {
        }

        /// <summary>
        /// Window proc
        /// </summary>
        /// <param name="m">Windows message</param>
        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        protected override void WndProc(ref Message m)
        {
            if (scannerCommon != null)
            {
                if (scannerCommon.HandleWndProc(m))
                {
                    return;
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Form has closed.  Uninitialize
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event argument</param>
        private void ContextMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnClose();
            scannerCommon.OnClosing();
            scannerCommon.Dispose();
        }

        /// <summary>
        /// Load handler. Perform initialization
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event argument</param>
        private void ContextMenu_Load(object sender, EventArgs e)
        {
            setFormHeight();

            scannerCommon.OnLoad(false);

            SetTitle(Title);

            PanelCommon.AnimationManager.Start(rootWidget);

            OnLoad();
        }

        /// <summary>
        /// Removes a row from the scanner form table
        /// </summary>
        /// <param name="panel">The table control</param>
        /// <param name="rowIndex">which row to remove</param>
        private void removeRow(TableLayoutPanel panel, int rowIndex)
        {
            panel.RowStyles.RemoveAt(rowIndex);

            for (int columnIndex = 0; columnIndex < panel.ColumnCount; columnIndex++)
            {
                var control = panel.GetControlFromPosition(columnIndex, rowIndex);
                panel.Controls.Remove(control);
                control.Dispose();
            }

            for (int i = rowIndex + 1; i < panel.RowCount; i++)
            {
                for (int columnIndex = 0; columnIndex < panel.ColumnCount; columnIndex++)
                {
                    var control = panel.GetControlFromPosition(columnIndex, i);
                    panel.SetRow(control, i - 1);
                }
            }

            panel.RowCount--;
        }

        /// <summary>
        /// Sets the height of the scanner.  The scanner form contains a
        /// table that can hold a certain maximum number of rows.  Depending
        /// on how many menu items are actually in the animation file,
        /// delete the unused rows and then set the height of the scanner
        /// </summary>
        private void setFormHeight()
        {
            var children = new List<Widget>();
            rootWidget.Finder.FindAllChildren(typeof(IRowWidget), children);
            int count = children.Count();
            int rowHeight = Row1.Height;

            if (count > 0 && count < tableLayoutPanel1.RowCount)
            {
                int rowsRemoved = 0;
                for (int row = tableLayoutPanel1.RowCount - 1; row >= count; row--)
                {
                    removeRow(tableLayoutPanel1, count);
                    rowsRemoved++;
                }

                Height -= rowsRemoved * rowHeight;
            }
        }
    }
}