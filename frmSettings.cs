using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Notes
{
    public partial class frmSettings : Form
    {
        private NotesLibrary.Configuration config;
        private bool hasChanges = false;

        private class NotesData
        {
            public Dictionary<string, frmMain.UnitStruct> Units { get; set; } = new Dictionary<string, frmMain.UnitStruct>();
            public Dictionary<string, frmMain.GroupStruct> Groups { get; set; } = new Dictionary<string, frmMain.GroupStruct>();
        }

        public frmSettings()
        {
            InitializeComponent();
            InitializeModernUI();
            
            // Subscribe to system theme changes for live updates
            ThemeManager.SystemThemeChanged += OnSystemThemeChanged;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!hasChanges)
            {
                LoadConfiguration();
                LoadControlsFromConfig();
            }
        }

        public void ApplyDefaultStylePreview()
        {
            if (!hasChanges)
            {
                LoadConfiguration();
                LoadControlsFromConfig();
            }
        }

        private void InitializeModernUI()
        {
            Icon = Properties.Resources.Notes;
            
            // Modern styling
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.Font = new Font("Segoe UI", 9f);
            
            // Set window properties
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Settings";
            
            // Add change tracking
            this.FormClosing += FrmSettings_FormClosing;
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            LoadConfiguration();
            SetupUI();
            LoadControlsFromConfig();
            
            // Add change tracking to all controls
            AddChangeTracking();
            
            // Apply current theme
            ThemeManager.ApplyTheme(this, config.General.Theme);
        }

        private void LoadConfiguration()
        {
            NotesLibrary.Instance.LoadConfiguration();
            config = NotesLibrary.Instance.Config;
        }

        private void SetupUI()
        {
            // Setup hotkey combo boxes
            cmbHotkey.DataSource = Enum.GetValues(typeof(Keys)).Cast<Keys>()
                .Where(k => k >= Keys.A && k <= Keys.Z || k >= Keys.F1 && k <= Keys.F12)
                .ToList();
            cmbHotkey.SelectedItem = Keys.N;

            // Setup modifier checkboxes
            clbHotkeyModifier.Items.Clear();
            clbHotkeyModifier.Items.Add(Keys.Control, false);
            clbHotkeyModifier.Items.Add(Keys.Alt, false);
            clbHotkeyModifier.Items.Add(Keys.Shift, false);

            // Setup window state combo
            cmbWindowState.DataSource = Enum.GetValues(typeof(FormWindowState));
            cmbWindowState.SelectedItem = FormWindowState.Normal;

            // Setup numeric controls
            numAutoSaveInterval.Minimum = 5;
            numAutoSaveInterval.Maximum = 300;
            numAutoSaveInterval.Value = 30;

            numBackupCount.Minimum = 5;
            numBackupCount.Maximum = 50;
            numBackupCount.Value = 10;

            numUndoLevels.Minimum = 5;
            numUndoLevels.Maximum = 50;
            numUndoLevels.Value = 20;

            // Setup theme combo box
            cmbTheme.DataSource = Enum.GetValues(typeof(NotesLibrary.ThemeMode));
            cmbTheme.SelectedItem = NotesLibrary.ThemeMode.SystemDefault;

            // Setup log level combo box
            cmbLogLevel.Items.Clear();
            cmbLogLevel.Items.Add("None (Disabled)");
            cmbLogLevel.Items.Add("Error");
            cmbLogLevel.Items.Add("Warning");
            cmbLogLevel.Items.Add("Info");
            cmbLogLevel.Items.Add("Debug");
            cmbLogLevel.SelectedIndex = 0;

            // Add event handlers for dependent controls
            chkAutoSave.CheckedChanged += ChkAutoSave_CheckedChanged;
            chkShowTrayIcon.CheckedChanged += ChkShowTrayIcon_CheckedChanged;
            cmbTheme.SelectedIndexChanged += CmbTheme_SelectedIndexChanged;
        }

        private void ChkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            numAutoSaveInterval.Enabled = chkAutoSave.Checked;
            lblAutoSaveInterval.Enabled = chkAutoSave.Checked;
            lblSeconds.Enabled = chkAutoSave.Checked;
        }

        private void ChkShowTrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            chkMinimizeToTray.Enabled = chkShowTrayIcon.Checked;
            chkCloseToTray.Enabled = chkShowTrayIcon.Checked;
            chkStartMinimized.Enabled = chkShowTrayIcon.Checked;
        }

        private void CmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Apply theme immediately for preview
            if (cmbTheme.SelectedItem is NotesLibrary.ThemeMode selectedTheme)
            {
                ThemeManager.ApplyTheme(this, selectedTheme);
            }
        }

        private void LoadControlsFromConfig()
        {
            try
            {
                // General Settings
                chkAutoSave.Checked = config.General.AutoSave;
                numAutoSaveInterval.Value = config.General.AutoSaveInterval;
                chkConfirmDelete.Checked = config.General.ConfirmDelete;
                chkConfirmReset.Checked = config.General.ConfirmReset;
                chkConfirmExit.Checked = config.General.ConfirmExit;
                chkShowTrayIcon.Checked = config.General.ShowTrayIcon;
                chkMinimizeToTray.Checked = config.General.MinimizeToTray;
                chkCloseToTray.Checked = config.General.CloseToTray;
                chkStartMinimized.Checked = config.General.StartMinimized;
                
                // Check actual registry state for startup setting
                bool actualStartupState = NotesLibrary.Instance.IsStartupEntrySet();
                chkStartWithWindows.Checked = actualStartupState;
                
                // Update config if it differs from actual state
                if (config.General.StartWithWindows != actualStartupState)
                {
                    config.General.StartWithWindows = actualStartupState;
                }

                // Hotkey settings
                cbHotkeyEnabled.Checked = config.Hotkey.Enabled;
                cmbHotkey.SelectedItem = config.Hotkey.Key;
                
                // Set modifier keys
                for (int i = 0; i < clbHotkeyModifier.Items.Count; i++)
                {
                    Keys key = (Keys)clbHotkeyModifier.Items[i];
                    clbHotkeyModifier.SetItemChecked(i, config.Hotkey.Modifiers.Contains(key));
                }

                // Enable/disable hotkey controls
                clbHotkeyModifier.Enabled = config.Hotkey.Enabled;
                cmbHotkey.Enabled = config.Hotkey.Enabled;

                // Unit style settings
                btnUnitStyleBackgroundColor.BackColor = Color.FromArgb(config.DefaultUnitStyle.BackgroundColor);
                btnUnitStyleTextColor.BackColor = Color.FromArgb(config.DefaultUnitStyle.TextColor);
                UpdateFontButton();

                // Window settings
                cmbWindowState.SelectedItem = config.Window.State;
                chkRememberPosition.Checked = config.Window.RememberPosition;
                chkRememberSize.Checked = config.Window.RememberSize;
                chkAlwaysOnTop.Checked = config.Window.AlwaysOnTop;

                // Backup settings
                chkAutoBackup.Checked = config.General.AutoBackup;
                numBackupCount.Value = config.General.BackupCount;

                // Advanced settings
                numUndoLevels.Value = config.General.UndoLevels;
                chkDoubleClickToEdit.Checked = config.General.DoubleClickToEdit;
                chkSingleClickToCopy.Checked = config.General.SingleClickToCopy;
                chkOptimizeForLargeFiles.Checked = config.General.OptimizeForLargeFiles;
                chkEnableAnimations.Checked = config.General.EnableAnimations;
                cmbTheme.SelectedItem = config.General.Theme;

                // Logging settings
                cmbLogLevel.SelectedIndex = (int)config.General.LogLevel;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message, NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddChangeTracking()
        {
            // General tab controls
            chkAutoSave.CheckedChanged += (s, e) => MarkAsChanged();
            numAutoSaveInterval.ValueChanged += (s, e) => MarkAsChanged();
            chkConfirmDelete.CheckedChanged += (s, e) => MarkAsChanged();
            chkConfirmReset.CheckedChanged += (s, e) => MarkAsChanged();
            chkConfirmExit.CheckedChanged += (s, e) => MarkAsChanged();
            chkShowTrayIcon.CheckedChanged += (s, e) => MarkAsChanged();
            chkMinimizeToTray.CheckedChanged += (s, e) => MarkAsChanged();
            chkCloseToTray.CheckedChanged += (s, e) => MarkAsChanged();
            chkStartMinimized.CheckedChanged += (s, e) => MarkAsChanged();
            chkStartWithWindows.CheckedChanged += (s, e) => MarkAsChanged();

            // Hotkey tab controls
            cbHotkeyEnabled.CheckedChanged += (s, e) => MarkAsChanged();
            cmbHotkey.SelectedIndexChanged += (s, e) => MarkAsChanged();
            clbHotkeyModifier.ItemCheck += (s, e) => MarkAsChanged();

            // Window tab controls
            cmbWindowState.SelectedIndexChanged += (s, e) => MarkAsChanged();
            chkRememberPosition.CheckedChanged += (s, e) => MarkAsChanged();
            chkRememberSize.CheckedChanged += (s, e) => MarkAsChanged();
            chkAlwaysOnTop.CheckedChanged += (s, e) => MarkAsChanged();

            // Backup tab controls
            chkAutoBackup.CheckedChanged += (s, e) => MarkAsChanged();
            numBackupCount.ValueChanged += (s, e) => MarkAsChanged();

            // Advanced tab controls
            numUndoLevels.ValueChanged += (s, e) => MarkAsChanged();
            chkDoubleClickToEdit.CheckedChanged += (s, e) => MarkAsChanged();
            chkSingleClickToCopy.CheckedChanged += (s, e) => MarkAsChanged();
            chkOptimizeForLargeFiles.CheckedChanged += (s, e) => MarkAsChanged();
            chkEnableAnimations.CheckedChanged += (s, e) => MarkAsChanged();
            
            // Theme controls
            cmbTheme.SelectedIndexChanged += (s, e) => MarkAsChanged();
        }

        private void MarkAsChanged()
        {
            if (!hasChanges)
            {
                hasChanges = true;
                this.Text = "Settings*";
            }
        }

        private void UpdateFontButton()
        {
            try
            {
                var font = new Font(config.DefaultUnitStyle.FontFamily, config.DefaultUnitStyle.FontSize, config.DefaultUnitStyle.FontStyle);
                btnUnitStyleFont.Font = font;
                btnUnitStyleFont.BackColor = Color.FromArgb(config.DefaultUnitStyle.BackgroundColor);
                btnUnitStyleFont.ForeColor = Color.FromArgb(config.DefaultUnitStyle.TextColor);
                btnUnitStyleFont.Text = string.Format("{0}, {1}pt", font.Name, font.SizeInPoints);
            }
            catch
            {
                btnUnitStyleFont.Text = "Select Font";
            }
        }

        private void cbHotkeyEnabled_CheckedChanged(object sender, EventArgs e)
        {
            clbHotkeyModifier.Enabled = cbHotkeyEnabled.Checked;
            cmbHotkey.Enabled = cbHotkeyEnabled.Checked;
        }

        private void btnUnitStyleBackgroundColor_Click(object sender, EventArgs e)
        {
            try
            {
                colorDialog.Color = Color.FromArgb(config.DefaultUnitStyle.BackgroundColor);
                colorDialog.FullOpen = true;
                DialogResult result = colorDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    config.DefaultUnitStyle.BackgroundColor = colorDialog.Color.ToArgb();
                    btnUnitStyleBackgroundColor.BackColor = colorDialog.Color;
                    UpdateFontButton();
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting background color: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUnitStyleTextColor_Click(object sender, EventArgs e)
        {
            try
            {
                colorDialog.Color = Color.FromArgb(config.DefaultUnitStyle.TextColor);
                colorDialog.FullOpen = true;
                DialogResult result = colorDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    config.DefaultUnitStyle.TextColor = colorDialog.Color.ToArgb();
                    btnUnitStyleTextColor.BackColor = colorDialog.Color;
                    UpdateFontButton();
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting text color: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUnitStyleFont_Click(object sender, EventArgs e)
        {
            try
            {
                var currentFont = new Font(config.DefaultUnitStyle.FontFamily, config.DefaultUnitStyle.FontSize, config.DefaultUnitStyle.FontStyle);
                fontDialog.Font = currentFont;
                fontDialog.ShowColor = false;
                DialogResult result = fontDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    config.DefaultUnitStyle.FontFamily = fontDialog.Font.FontFamily.Name;
                    config.DefaultUnitStyle.FontSize = fontDialog.Font.SizeInPoints;
                    config.DefaultUnitStyle.FontStyle = fontDialog.Font.Style;
                    UpdateFontButton();
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting font: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUnitStyleApply_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "This will apply the default style to all existing notes. Are you sure?", 
                NotesLibrary.AppName, 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                frmMain.ApplyDefaultStyleToAllNotes();
                MessageBox.Show("Default style settings have been saved. New notes will use this style.", 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                // General settings
                config.General.AutoSave = chkAutoSave.Checked;
                config.General.AutoSaveInterval = (int)numAutoSaveInterval.Value;
                config.General.ConfirmDelete = chkConfirmDelete.Checked;
                config.General.ConfirmReset = chkConfirmReset.Checked;
                config.General.ConfirmExit = chkConfirmExit.Checked;
                config.General.ShowTrayIcon = chkShowTrayIcon.Checked;
                config.General.MinimizeToTray = chkMinimizeToTray.Checked;
                config.General.CloseToTray = chkCloseToTray.Checked;
                config.General.StartMinimized = chkStartMinimized.Checked;
                config.General.StartWithWindows = chkStartWithWindows.Checked;
                config.General.AutoBackup = chkAutoBackup.Checked;
                config.General.BackupCount = (int)numBackupCount.Value;
                config.General.UndoLevels = (int)numUndoLevels.Value;
                config.General.DoubleClickToEdit = chkDoubleClickToEdit.Checked;
                config.General.SingleClickToCopy = chkSingleClickToCopy.Checked;
                config.General.OptimizeForLargeFiles = chkOptimizeForLargeFiles.Checked;
                config.General.EnableAnimations = chkEnableAnimations.Checked;
                config.General.Theme = (NotesLibrary.ThemeMode)cmbTheme.SelectedItem;
                config.General.LogLevel = (LogLevel)cmbLogLevel.SelectedIndex;

                // Hotkey settings
                config.Hotkey.Enabled = cbHotkeyEnabled.Checked;
                config.Hotkey.Key = (Keys)cmbHotkey.SelectedItem;
                
                // Get selected modifier keys
                var modifiers = new List<Keys>();
                for (int i = 0; i < clbHotkeyModifier.Items.Count; i++)
                {
                    if (clbHotkeyModifier.GetItemChecked(i))
                    {
                        modifiers.Add((Keys)clbHotkeyModifier.Items[i]);
                    }
                }
                config.Hotkey.Modifiers = modifiers.ToArray();

                // Window settings
                config.Window.State = (FormWindowState)cmbWindowState.SelectedItem;
                config.Window.RememberPosition = chkRememberPosition.Checked;
                config.Window.RememberSize = chkRememberSize.Checked;
                config.Window.AlwaysOnTop = chkAlwaysOnTop.Checked;

                // Save configuration
                NotesLibrary.Instance.SaveConfiguration();
                
                // Apply startup setting
                bool startupApplied = NotesLibrary.Instance.SetStartupEntry(config.General.StartWithWindows);
                if (!startupApplied)
                {
                    config.General.StartWithWindows = NotesLibrary.Instance.IsStartupEntrySet();
                    chkStartWithWindows.Checked = config.General.StartWithWindows;
                    NotesLibrary.Instance.SaveConfiguration();
                }
                
                // Apply logging setting
                Logger.Initialize(config.General.LogLevel);
                Logger.Info("Settings updated - Log level changed to: " + config.General.LogLevel);
                
                hasChanges = false;
                
                if (!startupApplied)
                {
                    MessageBox.Show("Settings saved, but startup setting could not be applied.", NotesLibrary.AppName, 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Settings saved successfully!", NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: " + ex.Message, NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate settings
            if (numAutoSaveInterval.Value < 5)
            {
                MessageBox.Show("Auto-save interval must be at least 5 seconds.", NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl1.SelectedTab = tabGeneral;
                numAutoSaveInterval.Focus();
                return;
            }

            SaveConfiguration();
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (hasChanges)
            {
                DialogResult result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to cancel?", 
                    NotesLibrary.AppName, 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                    return;
            }
            
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "This will reset all settings to their default values. Are you sure?", 
                NotesLibrary.AppName, 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                config = new NotesLibrary.Configuration();
                LoadControlsFromConfig();
                MarkAsChanged();
                MessageBox.Show("Settings have been reset to defaults.", NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FrmSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe from theme change events
            ThemeManager.SystemThemeChanged -= OnSystemThemeChanged;
            
            if (hasChanges && this.DialogResult != DialogResult.OK)
            {
                DialogResult result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?", 
                    NotesLibrary.AppName, 
                    MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Question);
                
                switch (result)
                {
                    case DialogResult.Yes:
                        SaveConfiguration();
                        this.DialogResult = DialogResult.OK;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                    case DialogResult.No:
                        // Continue closing
                        break;
                }
            }
        }

        private void OnSystemThemeChanged(object sender, EventArgs e)
        {
            // If the theme combo box is set to "System Default", update the preview immediately
            if (cmbTheme.SelectedItem is NotesLibrary.ThemeMode selectedTheme && 
                selectedTheme == NotesLibrary.ThemeMode.SystemDefault)
            {
                ThemeManager.ApplyTheme(this, selectedTheme);
            }
        }

        private void btnOpenLogFile_Click(object sender, EventArgs e)
        {
            try
            {
                string logFilePath = Logger.GetLogFilePath();
                
                if (string.IsNullOrEmpty(logFilePath))
                {
                    MessageBox.Show("Logging is currently disabled. Please set a log level first and restart the application.", 
                        NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!File.Exists(logFilePath))
                {
                    MessageBox.Show("No log file exists yet. The log file will be created when the first log entry is written.", 
                        NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Open the log file with default text editor
                Process.Start(new ProcessStartInfo
                {
                    FileName = logFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening log file: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "This will delete log files older than 7 days. Continue?",
                    NotesLibrary.AppName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Logger.ClearOldLogs(7);
                    MessageBox.Show("Old log files have been cleared.", 
                        NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clearing logs: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Keyboard shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    btnSave_Click(null, null);
                    return true;
                case Keys.Escape:
                    btnCancel_Click(null, null);
                    return true;
                case Keys.F5:
                    btnReset_Click(null, null);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Backup functionality
        private void btnRestoreBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    NotesLibrary.AppName, "Backups");

                if (!Directory.Exists(backupDir))
                {
                    MessageBox.Show("No backup folder found.", NotesLibrary.AppName, 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var backupFiles = Directory.GetFiles(backupDir, "backup_*.json")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToArray();

                if (backupFiles.Length == 0)
                {
                    MessageBox.Show("No backup files found.", NotesLibrary.AppName, 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show backup selection dialog
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = backupDir;
                    openFileDialog.Filter = "Backup Files (*.json)|*.json";
                    openFileDialog.Title = "Select backup to restore";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        DialogResult confirmResult = MessageBox.Show(
                            "This will replace all current notes. Are you sure?", 
                            NotesLibrary.AppName, 
                            MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Warning);

                        if (confirmResult == DialogResult.Yes)
                        {
                            string backupContent = File.ReadAllText(openFileDialog.FileName);
                            var data = JsonConvert.DeserializeObject<NotesData>(backupContent);
                            if (data == null || data.Units == null)
                            {
                                MessageBox.Show("Backup file is invalid or corrupted.", NotesLibrary.AppName, 
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            foreach (var key in data.Groups.Keys.ToList())
                            {
                                var group = data.Groups[key];
                                if (string.IsNullOrEmpty(group.Id))
                                    group.Id = key;
                                data.Groups[key] = group;
                            }
                            foreach (var key in data.Units.Keys.ToList())
                            {
                                var unit = data.Units[key];
                                if (!string.IsNullOrEmpty(unit.GroupId) && !data.Groups.ContainsKey(unit.GroupId))
                                {
                                    unit.GroupId = null;
                                    data.Units[key] = unit;
                                }
                            }
                            Properties.Settings.Default.JsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                            Properties.Settings.Default.Save();

                            MessageBox.Show("Backup restored successfully! Please restart the application.", 
                                NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error restoring backup: " + ex.Message, NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenBackupFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    NotesLibrary.AppName, "Backups");

                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                Process.Start("explorer.exe", backupDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening backup folder: " + ex.Message, NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
