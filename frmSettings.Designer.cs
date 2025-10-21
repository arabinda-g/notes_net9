
namespace Notes
{
    partial class frmSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.groupBoxAutoSave = new System.Windows.Forms.GroupBox();
            this.lblSeconds = new System.Windows.Forms.Label();
            this.numAutoSaveInterval = new System.Windows.Forms.NumericUpDown();
            this.lblAutoSaveInterval = new System.Windows.Forms.Label();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            this.groupBoxConfirmation = new System.Windows.Forms.GroupBox();
            this.chkConfirmDelete = new System.Windows.Forms.CheckBox();
            this.chkConfirmReset = new System.Windows.Forms.CheckBox();
            this.chkConfirmExit = new System.Windows.Forms.CheckBox();
            this.groupBoxSystemTray = new System.Windows.Forms.GroupBox();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.chkCloseToTray = new System.Windows.Forms.CheckBox();
            this.chkShowTrayIcon = new System.Windows.Forms.CheckBox();
            this.chkStartMinimized = new System.Windows.Forms.CheckBox();
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
            this.groupBoxTheme = new System.Windows.Forms.GroupBox();
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.lblTheme = new System.Windows.Forms.Label();
            this.tabHotkeys = new System.Windows.Forms.TabPage();
            this.groupBoxGlobalHotkeys = new System.Windows.Forms.GroupBox();
            this.clbHotkeyModifier = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbHotkey = new System.Windows.Forms.ComboBox();
            this.cbHotkeyEnabled = new System.Windows.Forms.CheckBox();
            this.tabWindow = new System.Windows.Forms.TabPage();
            this.groupBoxWindowBehavior = new System.Windows.Forms.GroupBox();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.chkRememberPosition = new System.Windows.Forms.CheckBox();
            this.chkRememberSize = new System.Windows.Forms.CheckBox();
            this.groupBoxStartupWindow = new System.Windows.Forms.GroupBox();
            this.lblStartupWindowState = new System.Windows.Forms.Label();
            this.cmbWindowState = new System.Windows.Forms.ComboBox();
            this.tabDefaultStyle = new System.Windows.Forms.TabPage();
            this.groupBoxDefaultStyle = new System.Windows.Forms.GroupBox();
            this.btnUnitStyleApply = new System.Windows.Forms.Button();
            this.btnUnitStyleFont = new System.Windows.Forms.Button();
            this.btnUnitStyleTextColor = new System.Windows.Forms.Button();
            this.btnUnitStyleBackgroundColor = new System.Windows.Forms.Button();
            this.lblFont = new System.Windows.Forms.Label();
            this.lblTextColor = new System.Windows.Forms.Label();
            this.lblBackgroundColor = new System.Windows.Forms.Label();
            this.tabBackup = new System.Windows.Forms.TabPage();
            this.groupBoxBackupSettings = new System.Windows.Forms.GroupBox();
            this.btnOpenBackupFolder = new System.Windows.Forms.Button();
            this.btnRestoreBackup = new System.Windows.Forms.Button();
            this.lblBackupCount = new System.Windows.Forms.Label();
            this.numBackupCount = new System.Windows.Forms.NumericUpDown();
            this.chkAutoBackup = new System.Windows.Forms.CheckBox();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.groupBoxBehavior = new System.Windows.Forms.GroupBox();
            this.numUndoLevels = new System.Windows.Forms.NumericUpDown();
            this.lblUndoLevels = new System.Windows.Forms.Label();
            this.chkDoubleClickToEdit = new System.Windows.Forms.CheckBox();
            this.chkSingleClickToCopy = new System.Windows.Forms.CheckBox();
            this.groupBoxPerformance = new System.Windows.Forms.GroupBox();
            this.chkOptimizeForLargeFiles = new System.Windows.Forms.CheckBox();
            this.chkEnableAnimations = new System.Windows.Forms.CheckBox();
            this.groupBoxLogging = new System.Windows.Forms.GroupBox();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.btnOpenLogFile = new System.Windows.Forms.Button();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.groupBoxAutoSave.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoSaveInterval)).BeginInit();
            this.groupBoxConfirmation.SuspendLayout();
            this.groupBoxSystemTray.SuspendLayout();
            this.groupBoxTheme.SuspendLayout();
            this.tabHotkeys.SuspendLayout();
            this.groupBoxGlobalHotkeys.SuspendLayout();
            this.tabWindow.SuspendLayout();
            this.groupBoxWindowBehavior.SuspendLayout();
            this.groupBoxStartupWindow.SuspendLayout();
            this.groupBoxDefaultStyle.SuspendLayout();
            this.tabBackup.SuspendLayout();
            this.groupBoxBackupSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBackupCount)).BeginInit();
            this.tabAdvanced.SuspendLayout();
            this.groupBoxBehavior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUndoLevels)).BeginInit();
            this.groupBoxPerformance.SuspendLayout();
            this.groupBoxLogging.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(515, 415);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(611, 415);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(12, 415);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(120, 30);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "&Reset to Defaults";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // colorDialog
            // 
            this.colorDialog.FullOpen = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabHotkeys);
            this.tabControl1.Controls.Add(this.tabWindow);
            this.tabControl1.Controls.Add(this.tabDefaultStyle);
            this.tabControl1.Controls.Add(this.tabBackup);
            this.tabControl1.Controls.Add(this.tabAdvanced);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(689, 397);
            this.tabControl1.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.groupBoxTheme);
            this.tabGeneral.Controls.Add(this.groupBoxSystemTray);
            this.tabGeneral.Controls.Add(this.groupBoxConfirmation);
            this.tabGeneral.Controls.Add(this.groupBoxAutoSave);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(8);
            this.tabGeneral.Size = new System.Drawing.Size(681, 371);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBoxAutoSave
            // 
            this.groupBoxAutoSave.Controls.Add(this.lblSeconds);
            this.groupBoxAutoSave.Controls.Add(this.numAutoSaveInterval);
            this.groupBoxAutoSave.Controls.Add(this.lblAutoSaveInterval);
            this.groupBoxAutoSave.Controls.Add(this.chkAutoSave);
            this.groupBoxAutoSave.Location = new System.Drawing.Point(11, 11);
            this.groupBoxAutoSave.Name = "groupBoxAutoSave";
            this.groupBoxAutoSave.Size = new System.Drawing.Size(320, 80);
            this.groupBoxAutoSave.TabIndex = 0;
            this.groupBoxAutoSave.TabStop = false;
            this.groupBoxAutoSave.Text = "Auto Save";
            // 
            // lblSeconds
            // 
            this.lblSeconds.AutoSize = true;
            this.lblSeconds.Location = new System.Drawing.Point(245, 50);
            this.lblSeconds.Name = "lblSeconds";
            this.lblSeconds.Size = new System.Drawing.Size(47, 13);
            this.lblSeconds.TabIndex = 3;
            this.lblSeconds.Text = "seconds";
            // 
            // numAutoSaveInterval
            // 
            this.numAutoSaveInterval.Location = new System.Drawing.Point(165, 48);
            this.numAutoSaveInterval.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numAutoSaveInterval.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numAutoSaveInterval.Name = "numAutoSaveInterval";
            this.numAutoSaveInterval.Size = new System.Drawing.Size(74, 20);
            this.numAutoSaveInterval.TabIndex = 2;
            this.numAutoSaveInterval.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // lblAutoSaveInterval
            // 
            this.lblAutoSaveInterval.AutoSize = true;
            this.lblAutoSaveInterval.Location = new System.Drawing.Point(25, 50);
            this.lblAutoSaveInterval.Name = "lblAutoSaveInterval";
            this.lblAutoSaveInterval.Size = new System.Drawing.Size(134, 13);
            this.lblAutoSaveInterval.TabIndex = 1;
            this.lblAutoSaveInterval.Text = "Auto-save interval (5-300):";
            // 
            // chkAutoSave
            // 
            this.chkAutoSave.AutoSize = true;
            this.chkAutoSave.Location = new System.Drawing.Point(15, 24);
            this.chkAutoSave.Name = "chkAutoSave";
            this.chkAutoSave.Size = new System.Drawing.Size(158, 17);
            this.chkAutoSave.TabIndex = 0;
            this.chkAutoSave.Text = "Enable automatic saving";
            this.chkAutoSave.UseVisualStyleBackColor = true;
            // 
            // groupBoxConfirmation
            // 
            this.groupBoxConfirmation.Controls.Add(this.chkConfirmExit);
            this.groupBoxConfirmation.Controls.Add(this.chkConfirmReset);
            this.groupBoxConfirmation.Controls.Add(this.chkConfirmDelete);
            this.groupBoxConfirmation.Location = new System.Drawing.Point(350, 11);
            this.groupBoxConfirmation.Name = "groupBoxConfirmation";
            this.groupBoxConfirmation.Size = new System.Drawing.Size(320, 100);
            this.groupBoxConfirmation.TabIndex = 1;
            this.groupBoxConfirmation.TabStop = false;
            this.groupBoxConfirmation.Text = "Confirmation Dialogs";
            // 
            // chkConfirmDelete
            // 
            this.chkConfirmDelete.AutoSize = true;
            this.chkConfirmDelete.Location = new System.Drawing.Point(15, 24);
            this.chkConfirmDelete.Name = "chkConfirmDelete";
            this.chkConfirmDelete.Size = new System.Drawing.Size(137, 17);
            this.chkConfirmDelete.TabIndex = 0;
            this.chkConfirmDelete.Text = "Confirm before delete";
            this.chkConfirmDelete.UseVisualStyleBackColor = true;
            // 
            // chkConfirmReset
            // 
            this.chkConfirmReset.AutoSize = true;
            this.chkConfirmReset.Location = new System.Drawing.Point(15, 47);
            this.chkConfirmReset.Name = "chkConfirmReset";
            this.chkConfirmReset.Size = new System.Drawing.Size(129, 17);
            this.chkConfirmReset.TabIndex = 1;
            this.chkConfirmReset.Text = "Confirm before reset";
            this.chkConfirmReset.UseVisualStyleBackColor = true;
            // 
            // chkConfirmExit
            // 
            this.chkConfirmExit.AutoSize = true;
            this.chkConfirmExit.Location = new System.Drawing.Point(15, 70);
            this.chkConfirmExit.Name = "chkConfirmExit";
            this.chkConfirmExit.Size = new System.Drawing.Size(120, 17);
            this.chkConfirmExit.TabIndex = 2;
            this.chkConfirmExit.Text = "Confirm before exit";
            this.chkConfirmExit.UseVisualStyleBackColor = true;
            // 
            // groupBoxSystemTray
            // 
            this.groupBoxSystemTray.Controls.Add(this.chkStartWithWindows);
            this.groupBoxSystemTray.Controls.Add(this.chkStartMinimized);
            this.groupBoxSystemTray.Controls.Add(this.chkCloseToTray);
            this.groupBoxSystemTray.Controls.Add(this.chkMinimizeToTray);
            this.groupBoxSystemTray.Controls.Add(this.chkShowTrayIcon);
            this.groupBoxSystemTray.Location = new System.Drawing.Point(11, 107);
            this.groupBoxSystemTray.Name = "groupBoxSystemTray";
            this.groupBoxSystemTray.Size = new System.Drawing.Size(320, 143);
            this.groupBoxSystemTray.TabIndex = 2;
            this.groupBoxSystemTray.TabStop = false;
            this.groupBoxSystemTray.Text = "System Tray";
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(15, 47);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(130, 17);
            this.chkMinimizeToTray.TabIndex = 1;
            this.chkMinimizeToTray.Text = "Minimize to system tray";
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // chkCloseToTray
            // 
            this.chkCloseToTray.AutoSize = true;
            this.chkCloseToTray.Location = new System.Drawing.Point(15, 70);
            this.chkCloseToTray.Name = "chkCloseToTray";
            this.chkCloseToTray.Size = new System.Drawing.Size(184, 17);
            this.chkCloseToTray.TabIndex = 2;
            this.chkCloseToTray.Text = "Close to tray (keep hotkeys active)";
            this.chkCloseToTray.UseVisualStyleBackColor = true;
            // 
            // chkShowTrayIcon
            // 
            this.chkShowTrayIcon.AutoSize = true;
            this.chkShowTrayIcon.Location = new System.Drawing.Point(15, 24);
            this.chkShowTrayIcon.Name = "chkShowTrayIcon";
            this.chkShowTrayIcon.Size = new System.Drawing.Size(127, 17);
            this.chkShowTrayIcon.TabIndex = 0;
            this.chkShowTrayIcon.Text = "Show system tray icon";
            this.chkShowTrayIcon.UseVisualStyleBackColor = true;
            // 
            // chkStartMinimized
            // 
            this.chkStartMinimized.AutoSize = true;
            this.chkStartMinimized.Location = new System.Drawing.Point(15, 93);
            this.chkStartMinimized.Name = "chkStartMinimized";
            this.chkStartMinimized.Size = new System.Drawing.Size(96, 17);
            this.chkStartMinimized.TabIndex = 3;
            this.chkStartMinimized.Text = "Start minimized";
            this.chkStartMinimized.UseVisualStyleBackColor = true;
            // 
            // chkStartWithWindows
            // 
            this.chkStartWithWindows.AutoSize = true;
            this.chkStartWithWindows.Location = new System.Drawing.Point(15, 116);
            this.chkStartWithWindows.Name = "chkStartWithWindows";
            this.chkStartWithWindows.Size = new System.Drawing.Size(137, 17);
            this.chkStartWithWindows.TabIndex = 4;
            this.chkStartWithWindows.Text = "Start with Windows";
            this.chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // groupBoxTheme
            // 
            this.groupBoxTheme.Controls.Add(this.cmbTheme);
            this.groupBoxTheme.Controls.Add(this.lblTheme);
            this.groupBoxTheme.Location = new System.Drawing.Point(350, 145);
            this.groupBoxTheme.Name = "groupBoxTheme";
            this.groupBoxTheme.Size = new System.Drawing.Size(320, 70);
            this.groupBoxTheme.TabIndex = 3;
            this.groupBoxTheme.TabStop = false;
            this.groupBoxTheme.Text = "Theme";
            // 
            // cmbTheme
            // 
            this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Location = new System.Drawing.Point(120, 30);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(185, 21);
            this.cmbTheme.TabIndex = 1;
            // 
            // lblTheme
            // 
            this.lblTheme.AutoSize = true;
            this.lblTheme.Location = new System.Drawing.Point(15, 33);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(88, 13);
            this.lblTheme.TabIndex = 0;
            this.lblTheme.Text = "Application theme:";
            // 
            // tabHotkeys
            // 
            this.tabHotkeys.Controls.Add(this.groupBoxGlobalHotkeys);
            this.tabHotkeys.Location = new System.Drawing.Point(4, 22);
            this.tabHotkeys.Name = "tabHotkeys";
            this.tabHotkeys.Padding = new System.Windows.Forms.Padding(8);
            this.tabHotkeys.Size = new System.Drawing.Size(681, 371);
            this.tabHotkeys.TabIndex = 1;
            this.tabHotkeys.Text = "Hotkeys";
            this.tabHotkeys.UseVisualStyleBackColor = true;
            // 
            // groupBoxGlobalHotkeys
            // 
            this.groupBoxGlobalHotkeys.Controls.Add(this.clbHotkeyModifier);
            this.groupBoxGlobalHotkeys.Controls.Add(this.label2);
            this.groupBoxGlobalHotkeys.Controls.Add(this.label1);
            this.groupBoxGlobalHotkeys.Controls.Add(this.cmbHotkey);
            this.groupBoxGlobalHotkeys.Controls.Add(this.cbHotkeyEnabled);
            this.groupBoxGlobalHotkeys.Location = new System.Drawing.Point(11, 11);
            this.groupBoxGlobalHotkeys.Name = "groupBoxGlobalHotkeys";
            this.groupBoxGlobalHotkeys.Size = new System.Drawing.Size(320, 140);
            this.groupBoxGlobalHotkeys.TabIndex = 0;
            this.groupBoxGlobalHotkeys.TabStop = false;
            this.groupBoxGlobalHotkeys.Text = "Global Hotkeys";
            // 
            // clbHotkeyModifier
            // 
            this.clbHotkeyModifier.CheckOnClick = true;
            this.clbHotkeyModifier.FormattingEnabled = true;
            this.clbHotkeyModifier.Location = new System.Drawing.Point(120, 50);
            this.clbHotkeyModifier.Name = "clbHotkeyModifier";
            this.clbHotkeyModifier.Size = new System.Drawing.Size(185, 34);
            this.clbHotkeyModifier.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Hotkey";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Modifier keys";
            // 
            // cmbHotkey
            // 
            this.cmbHotkey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHotkey.FormattingEnabled = true;
            this.cmbHotkey.Location = new System.Drawing.Point(120, 102);
            this.cmbHotkey.Name = "cmbHotkey";
            this.cmbHotkey.Size = new System.Drawing.Size(185, 21);
            this.cmbHotkey.TabIndex = 3;
            // 
            // cbHotkeyEnabled
            // 
            this.cbHotkeyEnabled.AutoSize = true;
            this.cbHotkeyEnabled.Location = new System.Drawing.Point(15, 24);
            this.cbHotkeyEnabled.Name = "cbHotkeyEnabled";
            this.cbHotkeyEnabled.Size = new System.Drawing.Size(131, 17);
            this.cbHotkeyEnabled.TabIndex = 0;
            this.cbHotkeyEnabled.Text = "Enable global hotkey";
            this.cbHotkeyEnabled.UseVisualStyleBackColor = true;
            this.cbHotkeyEnabled.CheckedChanged += new System.EventHandler(this.cbHotkeyEnabled_CheckedChanged);
            // 
            // tabWindow
            // 
            this.tabWindow.Controls.Add(this.groupBoxWindowBehavior);
            this.tabWindow.Controls.Add(this.groupBoxStartupWindow);
            this.tabWindow.Location = new System.Drawing.Point(4, 22);
            this.tabWindow.Name = "tabWindow";
            this.tabWindow.Padding = new System.Windows.Forms.Padding(8);
            this.tabWindow.Size = new System.Drawing.Size(681, 371);
            this.tabWindow.TabIndex = 2;
            this.tabWindow.Text = "Window";
            this.tabWindow.UseVisualStyleBackColor = true;
            // 
            // groupBoxStartupWindow
            // 
            this.groupBoxStartupWindow.Controls.Add(this.cmbWindowState);
            this.groupBoxStartupWindow.Controls.Add(this.lblStartupWindowState);
            this.groupBoxStartupWindow.Location = new System.Drawing.Point(11, 11);
            this.groupBoxStartupWindow.Name = "groupBoxStartupWindow";
            this.groupBoxStartupWindow.Size = new System.Drawing.Size(320, 80);
            this.groupBoxStartupWindow.TabIndex = 0;
            this.groupBoxStartupWindow.TabStop = false;
            this.groupBoxStartupWindow.Text = "Startup Window";
            // 
            // lblStartupWindowState
            // 
            this.lblStartupWindowState.AutoSize = true;
            this.lblStartupWindowState.Location = new System.Drawing.Point(15, 35);
            this.lblStartupWindowState.Name = "lblStartupWindowState";
            this.lblStartupWindowState.Size = new System.Drawing.Size(74, 13);
            this.lblStartupWindowState.TabIndex = 0;
            this.lblStartupWindowState.Text = "Window state:";
            // 
            // cmbWindowState
            // 
            this.cmbWindowState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWindowState.FormattingEnabled = true;
            this.cmbWindowState.Location = new System.Drawing.Point(120, 32);
            this.cmbWindowState.Name = "cmbWindowState";
            this.cmbWindowState.Size = new System.Drawing.Size(185, 21);
            this.cmbWindowState.TabIndex = 1;
            // 
            // groupBoxWindowBehavior
            // 
            this.groupBoxWindowBehavior.Controls.Add(this.chkAlwaysOnTop);
            this.groupBoxWindowBehavior.Controls.Add(this.chkRememberPosition);
            this.groupBoxWindowBehavior.Controls.Add(this.chkRememberSize);
            this.groupBoxWindowBehavior.Location = new System.Drawing.Point(350, 11);
            this.groupBoxWindowBehavior.Name = "groupBoxWindowBehavior";
            this.groupBoxWindowBehavior.Size = new System.Drawing.Size(320, 100);
            this.groupBoxWindowBehavior.TabIndex = 1;
            this.groupBoxWindowBehavior.TabStop = false;
            this.groupBoxWindowBehavior.Text = "Window Behavior";
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(15, 70);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(98, 17);
            this.chkAlwaysOnTop.TabIndex = 2;
            this.chkAlwaysOnTop.Text = "Always on top";
            this.chkAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // chkRememberPosition
            // 
            this.chkRememberPosition.AutoSize = true;
            this.chkRememberPosition.Location = new System.Drawing.Point(15, 47);
            this.chkRememberPosition.Name = "chkRememberPosition";
            this.chkRememberPosition.Size = new System.Drawing.Size(145, 17);
            this.chkRememberPosition.TabIndex = 1;
            this.chkRememberPosition.Text = "Remember window position";
            this.chkRememberPosition.UseVisualStyleBackColor = true;
            // 
            // chkRememberSize
            // 
            this.chkRememberSize.AutoSize = true;
            this.chkRememberSize.Location = new System.Drawing.Point(15, 24);
            this.chkRememberSize.Name = "chkRememberSize";
            this.chkRememberSize.Size = new System.Drawing.Size(131, 17);
            this.chkRememberSize.TabIndex = 0;
            this.chkRememberSize.Text = "Remember window size";
            this.chkRememberSize.UseVisualStyleBackColor = true;
            // 
            // tabDefaultStyle
            // 
            this.tabDefaultStyle.Controls.Add(this.groupBoxDefaultStyle);
            this.tabDefaultStyle.Location = new System.Drawing.Point(4, 22);
            this.tabDefaultStyle.Name = "tabDefaultStyle";
            this.tabDefaultStyle.Padding = new System.Windows.Forms.Padding(8);
            this.tabDefaultStyle.Size = new System.Drawing.Size(681, 371);
            this.tabDefaultStyle.TabIndex = 3;
            this.tabDefaultStyle.Text = "Default Style";
            this.tabDefaultStyle.UseVisualStyleBackColor = true;
            // 
            // groupBoxDefaultStyle
            // 
            this.groupBoxDefaultStyle.Controls.Add(this.btnUnitStyleApply);
            this.groupBoxDefaultStyle.Controls.Add(this.btnUnitStyleFont);
            this.groupBoxDefaultStyle.Controls.Add(this.btnUnitStyleTextColor);
            this.groupBoxDefaultStyle.Controls.Add(this.btnUnitStyleBackgroundColor);
            this.groupBoxDefaultStyle.Controls.Add(this.lblFont);
            this.groupBoxDefaultStyle.Controls.Add(this.lblTextColor);
            this.groupBoxDefaultStyle.Controls.Add(this.lblBackgroundColor);
            this.groupBoxDefaultStyle.Location = new System.Drawing.Point(11, 11);
            this.groupBoxDefaultStyle.Name = "groupBoxDefaultStyle";
            this.groupBoxDefaultStyle.Size = new System.Drawing.Size(400, 180);
            this.groupBoxDefaultStyle.TabIndex = 0;
            this.groupBoxDefaultStyle.TabStop = false;
            this.groupBoxDefaultStyle.Text = "Default Note Style";
            // 
            // btnUnitStyleApply
            // 
            this.btnUnitStyleApply.Location = new System.Drawing.Point(180, 135);
            this.btnUnitStyleApply.Name = "btnUnitStyleApply";
            this.btnUnitStyleApply.Size = new System.Drawing.Size(200, 30);
            this.btnUnitStyleApply.TabIndex = 6;
            this.btnUnitStyleApply.Text = "Apply Style to All Existing Notes";
            this.btnUnitStyleApply.UseVisualStyleBackColor = true;
            this.btnUnitStyleApply.Click += new System.EventHandler(this.btnUnitStyleApply_Click);
            // 
            // btnUnitStyleFont
            // 
            this.btnUnitStyleFont.Location = new System.Drawing.Point(180, 90);
            this.btnUnitStyleFont.Name = "btnUnitStyleFont";
            this.btnUnitStyleFont.Size = new System.Drawing.Size(200, 30);
            this.btnUnitStyleFont.TabIndex = 5;
            this.btnUnitStyleFont.Text = "Segoe UI, 9pt";
            this.btnUnitStyleFont.UseVisualStyleBackColor = true;
            this.btnUnitStyleFont.Click += new System.EventHandler(this.btnUnitStyleFont_Click);
            // 
            // btnUnitStyleTextColor
            // 
            this.btnUnitStyleTextColor.Location = new System.Drawing.Point(180, 55);
            this.btnUnitStyleTextColor.Name = "btnUnitStyleTextColor";
            this.btnUnitStyleTextColor.Size = new System.Drawing.Size(200, 30);
            this.btnUnitStyleTextColor.TabIndex = 4;
            this.btnUnitStyleTextColor.Text = "Text Color";
            this.btnUnitStyleTextColor.UseVisualStyleBackColor = true;
            this.btnUnitStyleTextColor.Click += new System.EventHandler(this.btnUnitStyleTextColor_Click);
            // 
            // btnUnitStyleBackgroundColor
            // 
            this.btnUnitStyleBackgroundColor.Location = new System.Drawing.Point(180, 20);
            this.btnUnitStyleBackgroundColor.Name = "btnUnitStyleBackgroundColor";
            this.btnUnitStyleBackgroundColor.Size = new System.Drawing.Size(200, 30);
            this.btnUnitStyleBackgroundColor.TabIndex = 3;
            this.btnUnitStyleBackgroundColor.Text = "Background Color";
            this.btnUnitStyleBackgroundColor.UseVisualStyleBackColor = true;
            this.btnUnitStyleBackgroundColor.Click += new System.EventHandler(this.btnUnitStyleBackgroundColor_Click);
            // 
            // lblFont
            // 
            this.lblFont.AutoSize = true;
            this.lblFont.Location = new System.Drawing.Point(15, 100);
            this.lblFont.Name = "lblFont";
            this.lblFont.Size = new System.Drawing.Size(28, 13);
            this.lblFont.TabIndex = 2;
            this.lblFont.Text = "Font";
            // 
            // lblTextColor
            // 
            this.lblTextColor.AutoSize = true;
            this.lblTextColor.Location = new System.Drawing.Point(15, 65);
            this.lblTextColor.Name = "lblTextColor";
            this.lblTextColor.Size = new System.Drawing.Size(55, 13);
            this.lblTextColor.TabIndex = 1;
            this.lblTextColor.Text = "Text Color";
            // 
            // lblBackgroundColor
            // 
            this.lblBackgroundColor.AutoSize = true;
            this.lblBackgroundColor.Location = new System.Drawing.Point(15, 30);
            this.lblBackgroundColor.Name = "lblBackgroundColor";
            this.lblBackgroundColor.Size = new System.Drawing.Size(92, 13);
            this.lblBackgroundColor.TabIndex = 0;
            this.lblBackgroundColor.Text = "Background Color";
            // 
            // tabBackup
            // 
            this.tabBackup.Controls.Add(this.groupBoxBackupSettings);
            this.tabBackup.Location = new System.Drawing.Point(4, 22);
            this.tabBackup.Name = "tabBackup";
            this.tabBackup.Padding = new System.Windows.Forms.Padding(8);
            this.tabBackup.Size = new System.Drawing.Size(681, 371);
            this.tabBackup.TabIndex = 4;
            this.tabBackup.Text = "Backup";
            this.tabBackup.UseVisualStyleBackColor = true;
            // 
            // groupBoxBackupSettings
            // 
            this.groupBoxBackupSettings.Controls.Add(this.btnOpenBackupFolder);
            this.groupBoxBackupSettings.Controls.Add(this.btnRestoreBackup);
            this.groupBoxBackupSettings.Controls.Add(this.numBackupCount);
            this.groupBoxBackupSettings.Controls.Add(this.lblBackupCount);
            this.groupBoxBackupSettings.Controls.Add(this.chkAutoBackup);
            this.groupBoxBackupSettings.Location = new System.Drawing.Point(11, 11);
            this.groupBoxBackupSettings.Name = "groupBoxBackupSettings";
            this.groupBoxBackupSettings.Size = new System.Drawing.Size(400, 160);
            this.groupBoxBackupSettings.TabIndex = 0;
            this.groupBoxBackupSettings.TabStop = false;
            this.groupBoxBackupSettings.Text = "Backup Management";
            // 
            // btnOpenBackupFolder
            // 
            this.btnOpenBackupFolder.Location = new System.Drawing.Point(200, 110);
            this.btnOpenBackupFolder.Name = "btnOpenBackupFolder";
            this.btnOpenBackupFolder.Size = new System.Drawing.Size(180, 30);
            this.btnOpenBackupFolder.TabIndex = 4;
            this.btnOpenBackupFolder.Text = "Open Backup Folder";
            this.btnOpenBackupFolder.UseVisualStyleBackColor = true;
            this.btnOpenBackupFolder.Click += new System.EventHandler(this.btnOpenBackupFolder_Click);
            // 
            // btnRestoreBackup
            // 
            this.btnRestoreBackup.Location = new System.Drawing.Point(15, 110);
            this.btnRestoreBackup.Name = "btnRestoreBackup";
            this.btnRestoreBackup.Size = new System.Drawing.Size(180, 30);
            this.btnRestoreBackup.TabIndex = 3;
            this.btnRestoreBackup.Text = "Restore from Backup...";
            this.btnRestoreBackup.UseVisualStyleBackColor = true;
            this.btnRestoreBackup.Click += new System.EventHandler(this.btnRestoreBackup_Click);
            // 
            // lblBackupCount
            // 
            this.lblBackupCount.AutoSize = true;
            this.lblBackupCount.Location = new System.Drawing.Point(15, 75);
            this.lblBackupCount.Name = "lblBackupCount";
            this.lblBackupCount.Size = new System.Drawing.Size(149, 13);
            this.lblBackupCount.TabIndex = 1;
            this.lblBackupCount.Text = "Maximum number of backups:";
            // 
            // numBackupCount
            // 
            this.numBackupCount.Location = new System.Drawing.Point(180, 73);
            this.numBackupCount.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numBackupCount.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numBackupCount.Name = "numBackupCount";
            this.numBackupCount.Size = new System.Drawing.Size(80, 20);
            this.numBackupCount.TabIndex = 2;
            this.numBackupCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // chkAutoBackup
            // 
            this.chkAutoBackup.AutoSize = true;
            this.chkAutoBackup.Location = new System.Drawing.Point(15, 30);
            this.chkAutoBackup.Name = "chkAutoBackup";
            this.chkAutoBackup.Size = new System.Drawing.Size(172, 17);
            this.chkAutoBackup.TabIndex = 0;
            this.chkAutoBackup.Text = "Create automatic backups";
            this.chkAutoBackup.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.groupBoxLogging);
            this.tabAdvanced.Controls.Add(this.groupBoxPerformance);
            this.tabAdvanced.Controls.Add(this.groupBoxBehavior);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(8);
            this.tabAdvanced.Size = new System.Drawing.Size(681, 371);
            this.tabAdvanced.TabIndex = 5;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // groupBoxBehavior
            // 
            this.groupBoxBehavior.Controls.Add(this.chkSingleClickToCopy);
            this.groupBoxBehavior.Controls.Add(this.chkDoubleClickToEdit);
            this.groupBoxBehavior.Controls.Add(this.lblUndoLevels);
            this.groupBoxBehavior.Controls.Add(this.numUndoLevels);
            this.groupBoxBehavior.Location = new System.Drawing.Point(11, 11);
            this.groupBoxBehavior.Name = "groupBoxBehavior";
            this.groupBoxBehavior.Size = new System.Drawing.Size(320, 120);
            this.groupBoxBehavior.TabIndex = 0;
            this.groupBoxBehavior.TabStop = false;
            this.groupBoxBehavior.Text = "Application Behavior";
            // 
            // numUndoLevels
            // 
            this.numUndoLevels.Location = new System.Drawing.Point(180, 28);
            this.numUndoLevels.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numUndoLevels.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numUndoLevels.Name = "numUndoLevels";
            this.numUndoLevels.Size = new System.Drawing.Size(80, 20);
            this.numUndoLevels.TabIndex = 1;
            this.numUndoLevels.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // lblUndoLevels
            // 
            this.lblUndoLevels.AutoSize = true;
            this.lblUndoLevels.Location = new System.Drawing.Point(15, 30);
            this.lblUndoLevels.Name = "lblUndoLevels";
            this.lblUndoLevels.Size = new System.Drawing.Size(90, 13);
            this.lblUndoLevels.TabIndex = 0;
            this.lblUndoLevels.Text = "Undo levels (5-50):";
            // 
            // chkDoubleClickToEdit
            // 
            this.chkDoubleClickToEdit.AutoSize = true;
            this.chkDoubleClickToEdit.Location = new System.Drawing.Point(15, 60);
            this.chkDoubleClickToEdit.Name = "chkDoubleClickToEdit";
            this.chkDoubleClickToEdit.Size = new System.Drawing.Size(112, 17);
            this.chkDoubleClickToEdit.TabIndex = 2;
            this.chkDoubleClickToEdit.Text = "Double-click to edit";
            this.chkDoubleClickToEdit.UseVisualStyleBackColor = true;
            // 
            // chkSingleClickToCopy
            // 
            this.chkSingleClickToCopy.AutoSize = true;
            this.chkSingleClickToCopy.Location = new System.Drawing.Point(15, 83);
            this.chkSingleClickToCopy.Name = "chkSingleClickToCopy";
            this.chkSingleClickToCopy.Size = new System.Drawing.Size(109, 17);
            this.chkSingleClickToCopy.TabIndex = 3;
            this.chkSingleClickToCopy.Text = "Single-click to copy";
            this.chkSingleClickToCopy.UseVisualStyleBackColor = true;
            // 
            // groupBoxPerformance
            // 
            this.groupBoxPerformance.Controls.Add(this.chkEnableAnimations);
            this.groupBoxPerformance.Controls.Add(this.chkOptimizeForLargeFiles);
            this.groupBoxPerformance.Location = new System.Drawing.Point(350, 11);
            this.groupBoxPerformance.Name = "groupBoxPerformance";
            this.groupBoxPerformance.Size = new System.Drawing.Size(320, 80);
            this.groupBoxPerformance.TabIndex = 1;
            this.groupBoxPerformance.TabStop = false;
            this.groupBoxPerformance.Text = "Performance";
            // 
            // chkOptimizeForLargeFiles
            // 
            this.chkOptimizeForLargeFiles.AutoSize = true;
            this.chkOptimizeForLargeFiles.Location = new System.Drawing.Point(15, 24);
            this.chkOptimizeForLargeFiles.Name = "chkOptimizeForLargeFiles";
            this.chkOptimizeForLargeFiles.Size = new System.Drawing.Size(199, 17);
            this.chkOptimizeForLargeFiles.TabIndex = 0;
            this.chkOptimizeForLargeFiles.Text = "Optimize for large number of notes";
            this.chkOptimizeForLargeFiles.UseVisualStyleBackColor = true;
            // 
            // chkEnableAnimations
            // 
            this.chkEnableAnimations.AutoSize = true;
            this.chkEnableAnimations.Location = new System.Drawing.Point(15, 47);
            this.chkEnableAnimations.Name = "chkEnableAnimations";
            this.chkEnableAnimations.Size = new System.Drawing.Size(109, 17);
            this.chkEnableAnimations.TabIndex = 1;
            this.chkEnableAnimations.Text = "Enable animations";
            this.chkEnableAnimations.UseVisualStyleBackColor = true;
            // 
            // groupBoxLogging
            // 
            this.groupBoxLogging.Controls.Add(this.btnClearLogs);
            this.groupBoxLogging.Controls.Add(this.btnOpenLogFile);
            this.groupBoxLogging.Controls.Add(this.cmbLogLevel);
            this.groupBoxLogging.Controls.Add(this.lblLogLevel);
            this.groupBoxLogging.Location = new System.Drawing.Point(11, 216);
            this.groupBoxLogging.Name = "groupBoxLogging";
            this.groupBoxLogging.Size = new System.Drawing.Size(659, 90);
            this.groupBoxLogging.TabIndex = 2;
            this.groupBoxLogging.TabStop = false;
            this.groupBoxLogging.Text = "Logging / Debugging";
            // 
            // lblLogLevel
            // 
            this.lblLogLevel.AutoSize = true;
            this.lblLogLevel.Location = new System.Drawing.Point(15, 28);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(57, 13);
            this.lblLogLevel.TabIndex = 0;
            this.lblLogLevel.Text = "Log Level:";
            // 
            // cmbLogLevel
            // 
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Location = new System.Drawing.Point(78, 25);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(150, 21);
            this.cmbLogLevel.TabIndex = 1;
            // 
            // btnOpenLogFile
            // 
            this.btnOpenLogFile.Location = new System.Drawing.Point(250, 23);
            this.btnOpenLogFile.Name = "btnOpenLogFile";
            this.btnOpenLogFile.Size = new System.Drawing.Size(120, 25);
            this.btnOpenLogFile.TabIndex = 2;
            this.btnOpenLogFile.Text = "Open Log File";
            this.btnOpenLogFile.UseVisualStyleBackColor = true;
            this.btnOpenLogFile.Click += new System.EventHandler(this.btnOpenLogFile_Click);
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.Location = new System.Drawing.Point(380, 23);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(120, 25);
            this.btnClearLogs.TabIndex = 3;
            this.btnClearLogs.Text = "Clear Old Logs";
            this.btnClearLogs.UseVisualStyleBackColor = true;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // 
            // frmSettings
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(713, 457);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.groupBoxAutoSave.ResumeLayout(false);
            this.groupBoxAutoSave.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoSaveInterval)).EndInit();
            this.groupBoxConfirmation.ResumeLayout(false);
            this.groupBoxConfirmation.PerformLayout();
            this.groupBoxSystemTray.ResumeLayout(false);
            this.groupBoxSystemTray.PerformLayout();
            this.groupBoxTheme.ResumeLayout(false);
            this.groupBoxTheme.PerformLayout();
            this.tabHotkeys.ResumeLayout(false);
            this.groupBoxGlobalHotkeys.ResumeLayout(false);
            this.groupBoxGlobalHotkeys.PerformLayout();
            this.tabWindow.ResumeLayout(false);
            this.groupBoxWindowBehavior.ResumeLayout(false);
            this.groupBoxWindowBehavior.PerformLayout();
            this.groupBoxStartupWindow.ResumeLayout(false);
            this.groupBoxStartupWindow.PerformLayout();
            this.groupBoxDefaultStyle.ResumeLayout(false);
            this.groupBoxDefaultStyle.PerformLayout();
            this.tabBackup.ResumeLayout(false);
            this.groupBoxBackupSettings.ResumeLayout(false);
            this.groupBoxBackupSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numBackupCount)).EndInit();
            this.tabAdvanced.ResumeLayout(false);
            this.groupBoxBehavior.ResumeLayout(false);
            this.groupBoxBehavior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUndoLevels)).EndInit();
            this.groupBoxPerformance.ResumeLayout(false);
            this.groupBoxPerformance.PerformLayout();
            this.groupBoxLogging.ResumeLayout(false);
            this.groupBoxLogging.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.TabControl tabControl1;
        
        // General Tab
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox groupBoxAutoSave;
        private System.Windows.Forms.CheckBox chkAutoSave;
        private System.Windows.Forms.Label lblAutoSaveInterval;
        private System.Windows.Forms.NumericUpDown numAutoSaveInterval;
        private System.Windows.Forms.Label lblSeconds;
        private System.Windows.Forms.GroupBox groupBoxConfirmation;
        private System.Windows.Forms.CheckBox chkConfirmDelete;
        private System.Windows.Forms.CheckBox chkConfirmReset;
        private System.Windows.Forms.CheckBox chkConfirmExit;
        private System.Windows.Forms.GroupBox groupBoxSystemTray;
        private System.Windows.Forms.CheckBox chkShowTrayIcon;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.CheckBox chkCloseToTray;
        private System.Windows.Forms.CheckBox chkStartMinimized;
        private System.Windows.Forms.CheckBox chkStartWithWindows;
        private System.Windows.Forms.GroupBox groupBoxTheme;
        private System.Windows.Forms.ComboBox cmbTheme;
        private System.Windows.Forms.Label lblTheme;
        
        // Hotkeys Tab
        private System.Windows.Forms.TabPage tabHotkeys;
        private System.Windows.Forms.GroupBox groupBoxGlobalHotkeys;
        private System.Windows.Forms.CheckBox cbHotkeyEnabled;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox clbHotkeyModifier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbHotkey;
        
        // Window Tab
        private System.Windows.Forms.TabPage tabWindow;
        private System.Windows.Forms.GroupBox groupBoxWindowBehavior;
        private System.Windows.Forms.CheckBox chkAlwaysOnTop;
        private System.Windows.Forms.CheckBox chkRememberPosition;
        private System.Windows.Forms.CheckBox chkRememberSize;
        private System.Windows.Forms.GroupBox groupBoxStartupWindow;
        private System.Windows.Forms.Label lblStartupWindowState;
        private System.Windows.Forms.ComboBox cmbWindowState;
        
        // Default Style Tab
        private System.Windows.Forms.TabPage tabDefaultStyle;
        private System.Windows.Forms.GroupBox groupBoxDefaultStyle;
        private System.Windows.Forms.Label lblBackgroundColor;
        private System.Windows.Forms.Label lblTextColor;
        private System.Windows.Forms.Label lblFont;
        private System.Windows.Forms.Button btnUnitStyleBackgroundColor;
        private System.Windows.Forms.Button btnUnitStyleTextColor;
        private System.Windows.Forms.Button btnUnitStyleFont;
        private System.Windows.Forms.Button btnUnitStyleApply;
        
        // Backup Tab
        private System.Windows.Forms.TabPage tabBackup;
        private System.Windows.Forms.GroupBox groupBoxBackupSettings;
        private System.Windows.Forms.CheckBox chkAutoBackup;
        private System.Windows.Forms.Label lblBackupCount;
        private System.Windows.Forms.NumericUpDown numBackupCount;
        private System.Windows.Forms.Button btnRestoreBackup;
        private System.Windows.Forms.Button btnOpenBackupFolder;
        
        // Advanced Tab
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.GroupBox groupBoxBehavior;
        private System.Windows.Forms.NumericUpDown numUndoLevels;
        private System.Windows.Forms.Label lblUndoLevels;
        private System.Windows.Forms.CheckBox chkDoubleClickToEdit;
        private System.Windows.Forms.CheckBox chkSingleClickToCopy;
        private System.Windows.Forms.GroupBox groupBoxPerformance;
        private System.Windows.Forms.CheckBox chkOptimizeForLargeFiles;
        private System.Windows.Forms.CheckBox chkEnableAnimations;
        private System.Windows.Forms.GroupBox groupBoxLogging;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.Button btnOpenLogFile;
        private System.Windows.Forms.Button btnClearLogs;
    }
}