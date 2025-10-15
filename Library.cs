using Newtonsoft.Json;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Notes
{
    public class NotesLibrary
    {
        public static string AppName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Notes";
        private static NotesLibrary? _instance;
        private Configuration? _config;

        public static NotesLibrary Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NotesLibrary();
                return _instance;
            }
        }

        public Configuration Config
        {
            get
            {
                if (_config == null)
                    _config = new Configuration();
                return _config;
            }
        }

        public class Configuration
        {
            public HotkeySettings Hotkey { get; set; }
            public UnitStyleSettings DefaultUnitStyle { get; set; }
            public WindowSettings Window { get; set; }
            public GeneralSettings General { get; set; }

            public Configuration()
            {
                Hotkey = new HotkeySettings();
                DefaultUnitStyle = new UnitStyleSettings();
                Window = new WindowSettings();
                General = new GeneralSettings();
            }
        }

        public class HotkeySettings
        {
            public bool Enabled { get; set; }
            public Keys Key { get; set; }
            public Keys[] Modifiers { get; set; }

            public HotkeySettings()
            {
                Enabled = false;
                Key = Keys.N;
                Modifiers = new Keys[] { Keys.Control, Keys.Alt };
            }
        }

        public class UnitStyleSettings
        {
            public int BackgroundColor { get; set; }
            public int TextColor { get; set; }
            public string FontFamily { get; set; }
            public float FontSize { get; set; }
            public FontStyle FontStyle { get; set; }

            public UnitStyleSettings()
            {
                BackgroundColor = Color.LightYellow.ToArgb();
                TextColor = Color.Black.ToArgb();
                FontFamily = "Segoe UI";
                FontSize = 9f;
                FontStyle = FontStyle.Regular;
            }
        }

        public class WindowSettings
        {
            public FormWindowState State { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool RememberPosition { get; set; }
            public bool RememberSize { get; set; }
            public bool AlwaysOnTop { get; set; }

            public WindowSettings()
            {
                State = FormWindowState.Normal;
                X = 100;
                Y = 100;
                Width = 800;
                Height = 600;
                RememberPosition = true;
                RememberSize = true;
                AlwaysOnTop = false;
            }
        }

        public enum ThemeMode
        {
            SystemDefault,
            Light,
            Dark
        }

        public class GeneralSettings
        {
            public bool AutoSave { get; set; }
            public int AutoSaveInterval { get; set; }
            public bool ConfirmDelete { get; set; }
            public bool ConfirmReset { get; set; }
            public bool ConfirmExit { get; set; }
            public bool ShowTrayIcon { get; set; }
            public bool MinimizeToTray { get; set; }
            public bool StartMinimized { get; set; }
            public bool StartWithWindows { get; set; }
            public bool AutoBackup { get; set; }
            public int BackupCount { get; set; }
            public int UndoLevels { get; set; }
            public bool DoubleClickToEdit { get; set; }
            public bool SingleClickToCopy { get; set; }
            public bool OptimizeForLargeFiles { get; set; }
            public bool EnableAnimations { get; set; }
            public ThemeMode Theme { get; set; }

            public GeneralSettings()
            {
                AutoSave = true;
                AutoSaveInterval = 30; // seconds
                ConfirmDelete = true;
                ConfirmReset = true;
                ConfirmExit = false;
                ShowTrayIcon = true;
                MinimizeToTray = false;
                StartMinimized = false;
                StartWithWindows = false;
                AutoBackup = true;
                BackupCount = 10;
                UndoLevels = 20;
                DoubleClickToEdit = true;
                SingleClickToCopy = true;
                OptimizeForLargeFiles = false;
                EnableAnimations = true;
                Theme = ThemeMode.SystemDefault;
            }
        }

        public bool LoadConfiguration()
        {
            try
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.ConfigJson))
                {
                    _config = JsonConvert.DeserializeObject<Configuration>(Properties.Settings.Default.ConfigJson);
                    return _config != null;
                }
                else
                {
                    _config = new Configuration();
                    return false;
                }
            }
            catch (Exception)
            {
                _config = new Configuration();
                return false;
            }
        }

        public void SaveConfiguration()
        {
            try
            {
                Properties.Settings.Default.ConfigJson = JsonConvert.SerializeObject(_config, Formatting.Indented);
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save configuration: " + ex.Message, AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public Font GetDefaultFont()
        {
            try
            {
                return new Font(_config?.DefaultUnitStyle?.FontFamily ?? "Segoe UI", 
                    _config?.DefaultUnitStyle?.FontSize ?? 9f, 
                    _config?.DefaultUnitStyle?.FontStyle ?? FontStyle.Regular);
            }
            catch
            {
                return new Font("Segoe UI", 9f, FontStyle.Regular);
            }
        }

        public string GenerateId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 11).ToUpper();
        }

        public void CreateBackup()
        {
            try
            {
                string backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName, "Backups");
                Directory.CreateDirectory(backupDir);
                
                string fileName = string.Format("backup_{0:yyyyMMdd_HHmmss}.json", DateTime.Now);
                string filePath = Path.Combine(backupDir, fileName);
                
                File.WriteAllText(filePath, Properties.Settings.Default.JsonData);
                
                // Keep only last 10 backups
                var files = Directory.GetFiles(backupDir, "backup_*.json")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .Skip(10);
                
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create backup: " + ex.Message, AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public bool SetStartupEntry(bool enable)
        {
            try
            {
                const string startupKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                string? appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(startupKeyPath, true))
                {
                    if (key != null && appPath != null)
                    {
                        if (enable)
                        {
                            key.SetValue(AppName, $"\"{appPath}\"");
                        }
                        else
                        {
                            key.DeleteValue(AppName, false);
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to {(enable ? "enable" : "disable")} startup with Windows: {ex.Message}", 
                    AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return false;
        }

        public bool IsStartupEntrySet()
        {
            try
            {
                const string startupKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(startupKeyPath, false))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(AppName);
                        return value != null;
                    }
                }
            }
            catch
            {
                // If we can't read the registry, assume it's not set
            }
            return false;
        }
    }

    public static class ThemeManager
    {
        public static event EventHandler? SystemThemeChanged;
        private static bool _isSystemDarkTheme = false;
        private static bool _initialized = false;

        public static class LightTheme
        {
            public static Color BackgroundColor = Color.FromArgb(248, 249, 250);
            public static Color SurfaceColor = Color.White;
            public static Color TextColor = Color.Black;
            public static Color SecondaryTextColor = Color.FromArgb(108, 117, 125);
            public static Color BorderColor = Color.FromArgb(222, 226, 230);
            public static Color ButtonBackgroundColor = Color.FromArgb(240, 244, 248);
            public static Color ButtonHoverColor = Color.FromArgb(230, 236, 241);
            public static Color AccentColor = Color.FromArgb(13, 110, 253);
        }

        public static class DarkTheme
        {
            public static Color BackgroundColor = Color.FromArgb(33, 37, 41);
            public static Color SurfaceColor = Color.FromArgb(52, 58, 64);
            public static Color TextColor = Color.FromArgb(248, 249, 250);
            public static Color SecondaryTextColor = Color.FromArgb(173, 181, 189);
            public static Color BorderColor = Color.FromArgb(73, 80, 87);
            public static Color ButtonBackgroundColor = Color.FromArgb(64, 70, 76);
            public static Color ButtonHoverColor = Color.FromArgb(74, 82, 90);
            public static Color AccentColor = Color.FromArgb(13, 110, 253);
        }

        public static bool IsSystemDarkTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key?.GetValue("AppsUseLightTheme");
                    if (value is int intValue)
                    {
                        bool isDark = intValue == 0; // 0 = dark, 1 = light
                        
                        // Check if theme actually changed
                        if (_initialized && isDark != _isSystemDarkTheme)
                        {
                            _isSystemDarkTheme = isDark;
                            SystemThemeChanged?.Invoke(null, EventArgs.Empty);
                        }
                        else if (!_initialized)
                        {
                            _isSystemDarkTheme = isDark;
                            _initialized = true;
                        }
                        
                        return isDark;
                    }
                }
            }
            catch
            {
                // If we can't read the registry, default to light theme
            }
            return false;
        }

        public static void StartSystemThemeMonitoring()
        {
            // Initialize the current theme state
            IsSystemDarkTheme();
            
            // Start monitoring registry changes
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        public static void StopSystemThemeMonitoring()
        {
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                // Check if system theme changed
                IsSystemDarkTheme();
            }
        }

        public static bool IsDarkThemeActive(NotesLibrary.ThemeMode themeMode)
        {
            switch (themeMode)
            {
                case NotesLibrary.ThemeMode.Dark:
                    return true;
                case NotesLibrary.ThemeMode.Light:
                    return false;
                case NotesLibrary.ThemeMode.SystemDefault:
                default:
                    return IsSystemDarkTheme();
            }
        }

        public static void ApplyTheme(Control control, NotesLibrary.ThemeMode themeMode)
        {
            bool isDark = IsDarkThemeActive(themeMode);
            
            if (isDark)
            {
                ApplyDarkTheme(control);
            }
            else
            {
                ApplyLightTheme(control);
            }
        }

        private static void ApplyLightTheme(Control control)
        {
            if (control is Form form)
            {
                form.BackColor = LightTheme.BackgroundColor;
                form.ForeColor = LightTheme.TextColor;
            }
            else if (control is Panel || control is GroupBox)
            {
                control.BackColor = LightTheme.SurfaceColor;
                control.ForeColor = LightTheme.TextColor;
            }
            else if (control is Button button)
            {
                button.BackColor = LightTheme.ButtonBackgroundColor;
                button.ForeColor = LightTheme.TextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = LightTheme.BorderColor;
            }
            else if (control is TextBox || control is ComboBox || control is NumericUpDown)
            {
                control.BackColor = LightTheme.SurfaceColor;
                control.ForeColor = LightTheme.TextColor;
            }
            else if (control is Label)
            {
                control.ForeColor = LightTheme.TextColor;
            }
            else if (control is CheckBox || control is RadioButton)
            {
                control.ForeColor = LightTheme.TextColor;
            }
            else if (control is MenuStrip || control is StatusStrip)
            {
                control.BackColor = LightTheme.SurfaceColor;
                control.ForeColor = LightTheme.TextColor;
            }
            else if (control is TabControl tabControl)
            {
                tabControl.BackColor = LightTheme.BackgroundColor;
                tabControl.ForeColor = LightTheme.TextColor;
            }
            
            // Apply to all child controls recursively
            foreach (Control child in control.Controls)
            {
                ApplyLightTheme(child);
            }
        }

        private static void ApplyDarkTheme(Control control)
        {
            if (control is Form form)
            {
                form.BackColor = DarkTheme.BackgroundColor;
                form.ForeColor = DarkTheme.TextColor;
            }
            else if (control is Panel || control is GroupBox)
            {
                control.BackColor = DarkTheme.SurfaceColor;
                control.ForeColor = DarkTheme.TextColor;
            }
            else if (control is Button button)
            {
                button.BackColor = DarkTheme.ButtonBackgroundColor;
                button.ForeColor = DarkTheme.TextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = DarkTheme.BorderColor;
            }
            else if (control is TextBox || control is ComboBox || control is NumericUpDown)
            {
                control.BackColor = DarkTheme.SurfaceColor;
                control.ForeColor = DarkTheme.TextColor;
            }
            else if (control is Label)
            {
                control.ForeColor = DarkTheme.TextColor;
            }
            else if (control is CheckBox || control is RadioButton)
            {
                control.ForeColor = DarkTheme.TextColor;
            }
            else if (control is MenuStrip || control is StatusStrip)
            {
                control.BackColor = DarkTheme.SurfaceColor;
                control.ForeColor = DarkTheme.TextColor;
            }
            else if (control is TabControl tabControl)
            {
                tabControl.BackColor = DarkTheme.BackgroundColor;
                tabControl.ForeColor = DarkTheme.TextColor;
            }
            
            // Apply to all child controls recursively
            foreach (Control child in control.Controls)
            {
                ApplyDarkTheme(child);
            }
        }

        public static Color GetCurrentBackgroundColor(NotesLibrary.ThemeMode themeMode)
        {
            return IsDarkThemeActive(themeMode) ? DarkTheme.BackgroundColor : LightTheme.BackgroundColor;
        }

        public static Color GetCurrentSurfaceColor(NotesLibrary.ThemeMode themeMode)
        {
            return IsDarkThemeActive(themeMode) ? DarkTheme.SurfaceColor : LightTheme.SurfaceColor;
        }

        public static Color GetCurrentTextColor(NotesLibrary.ThemeMode themeMode)
        {
            return IsDarkThemeActive(themeMode) ? DarkTheme.TextColor : LightTheme.TextColor;
        }
    }
}

