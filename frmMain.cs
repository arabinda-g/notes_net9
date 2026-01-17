using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notes
{
    public partial class frmMain : Form
    {
        public static string AppName = NotesLibrary.AppName;

        public struct UnitStruct
        {
            public string Title;
            public int BackgroundColor;
            public int TextColor;
            public Font Font;
            public int X;
            public int Y;
            public DateTime CreatedDate;
            public DateTime ModifiedDate;
            public string Category;
            public string[] Tags;
            public string ButtonType; // Store the custom button type name
            public string GroupId; // Optional group association
            public string ContentType;
            public string ContentData;
            public string ContentFormat;

            [JsonIgnore]
            public string Content
            {
                readonly get
                {
                    if (string.IsNullOrWhiteSpace(ContentType) || string.Equals(ContentType, "Text", StringComparison.OrdinalIgnoreCase))
                        return ContentData ?? string.Empty;
                    if (string.Equals(ContentType, "Object", StringComparison.OrdinalIgnoreCase))
                        return ContentData ?? string.Empty;
                    return string.Empty;
                }
                set
                {
                    ContentType = "Text";
                    ContentFormat ??= "plain";
                    ContentData = value ?? string.Empty;
                }
            }

            [JsonProperty("Content")]
            private string? LegacyContent
            {
                readonly get
                {
                    if (string.IsNullOrWhiteSpace(ContentType) || string.Equals(ContentType, "Text", StringComparison.OrdinalIgnoreCase))
                        return ContentData;
                    if (string.Equals(ContentType, "Object", StringComparison.OrdinalIgnoreCase))
                        return ContentData;
                    return null;
                }
                set
                {
                    if (value != null)
                    {
                        ContentType = "Text";
                        ContentFormat ??= "plain";
                        ContentData = value;
                    }
                }
            }
        }

        public struct GroupStruct
        {
            public string Id;
            public string Title;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int BorderColor;
            public int BackgroundColor;
            public int TextColor;
            public string GroupBoxType;
        }

        private class AppState
        {
            public Dictionary<string, UnitStruct> Units { get; set; } = new Dictionary<string, UnitStruct>();
            public Dictionary<string, GroupStruct> Groups { get; set; } = new Dictionary<string, GroupStruct>();
        }

        private class NotesData
        {
            public Dictionary<string, UnitStruct> Units { get; set; } = new Dictionary<string, UnitStruct>();
            public Dictionary<string, GroupStruct> Groups { get; set; } = new Dictionary<string, GroupStruct>();
        }

        // Status bar management
        private string status
        {
            set
            {
                tmrStatus.Stop();
                statusLabel.Text = value;
                tmrStatus.Interval = 3000;
                tmrStatus.Start();
            }
        }

        private Dictionary<string, UnitStruct> Units = new Dictionary<string, UnitStruct>();
        private Dictionary<string, GroupStruct> Groups = new Dictionary<string, GroupStruct>();
        private List<string> searchResults = new List<string>();
        private Stack<AppState> undoStack = new Stack<AppState>();
        private Stack<AppState> redoStack = new Stack<AppState>();

        public static UnitStruct selectedUnit = new UnitStruct();
        public static bool selectedUnitModified = false;
        private bool configModified = false;
        private static frmMain instance;
        private bool isMovable = false;
        private bool isAutofocus = false;
        private bool autoSaveEnabled = true;
        private bool isAutoSaving = false;
        private readonly object saveLock = new object();
        private bool forceExit = false;

        // Button movement variables
        private bool btnMovingArrow = false;
        private bool _unitDoubleClicked = false;
        private bool _unitMouseMoved = false;
        private object _unitClickSender;
        private EventArgs _unitClickE;

        // Drag and drop variables
        private Point Origin_Cursor;
        private Point Origin_Control;
        private bool BtnDragging = false;

        // Multi-selection variables
        private bool isSelecting = false;
        private Point selectionStart;
        private Point selectionEnd;
        private Rectangle selectionRectangle;
        private HashSet<Button> selectedButtons = new HashSet<Button>();
        private struct SelectionStyle
        {
            public FlatStyle FlatStyle;
            public Color BorderColor;
            public int BorderSize;
        }

        private Dictionary<Button, SelectionStyle> selectionOriginalStyles = new Dictionary<Button, SelectionStyle>();
        private HashSet<GroupBox> resizingGroups = new HashSet<GroupBox>();
        private static readonly Random random = new Random();
        private bool isMovingGroupBox = false;
        private Point groupBoxMoveStart;
        private GroupBox currentGroupBoxDrag;
        private Point currentGroupBoxOriginalLocation;
        private Dictionary<Button, Point> currentGroupBoxButtonOrigins = new Dictionary<Button, Point>();
        private bool isMovingGroup = false;
        private Point groupMoveStart;
        private Dictionary<Button, Point> groupOriginalPositions = new Dictionary<Button, Point>();

        // Copy functionality
        public static UnitStruct? copiedUnit = null;

        // Auto-save timer
        private System.Windows.Forms.Timer autoSaveTimer;
        private System.Windows.Forms.Timer searchTimer;

        // Global hotkey
        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;

        public frmMain()
        {
            instance = this;
            InitializeComponent();
            InitializeCustomComponents();
            SetupAutoSave();
            LoadConfiguration();
            SetupSystemThemeMonitoring();
            // Don't register hotkey here - handle isn't created yet
            // It will be registered in OnHandleCreated
            
            // Enable key preview to catch keyboard events
            this.KeyPreview = true;
            this.KeyDown += frmMain_KeyDown;
        }

        public static Dictionary<string, GroupStruct> GetGroups()
        {
            return instance?.Groups ?? new Dictionary<string, GroupStruct>();
        }

        public static void ApplyDefaultStyleToAllNotes()
        {
            instance?.ApplyDefaultStyleToAllNotesInternal();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            
            // Now that the handle is created, we can register the hotkey
            RegisterGlobalHotkey();
        }

        private void InitializeCustomComponents()
        {
            Icon = Properties.Resources.Notes;
            trayIcon.Icon = Icon;
            trayIcon.Text = AppName;
            tmrClickHandle.Interval = SystemInformation.DoubleClickTime;

            // Apply theme styling
            ApplyCurrentTheme();
            
            // Setup events
            this.Resize += frmMain_Resize;
            this.FormClosing += frmMain_FormClosing;
            trayIcon.MouseClick += TrayIcon_MouseClick;
            panelContainer.MouseUp += panelContainer_MouseUp;
            panelContainer.MouseDown += panelContainer_MouseDown;
            panelContainer.MouseMove += panelContainer_MouseMove;
            panelContainer.Paint += panelContainer_Paint;
            panelContainer.DragEnter += panelContainer_DragEnter;
            panelContainer.DragDrop += panelContainer_DragDrop;
            panelContainer.AllowDrop = true;
            
            // Enable double buffering to prevent visual artifacts when drawing selection rectangle
            typeof(Control).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(panelContainer, true);
            
            // Initialize status
            statusLabel.Text = "Ready";
        }

        private void SetupAutoSave()
        {
            autoSaveTimer = new System.Windows.Forms.Timer();
            autoSaveTimer.Interval = 30000; // 30 seconds default
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            
            searchTimer = new System.Windows.Forms.Timer();
            searchTimer.Interval = 500; // 500ms delay for search
            searchTimer.Tick += SearchTimer_Tick;
        }

        private void LoadConfiguration()
        {
            NotesLibrary.Instance.LoadConfiguration();
            var config = NotesLibrary.Instance.Config;
            
            autoSaveEnabled = config.General.AutoSave;
            int intervalSeconds = config.General.AutoSaveInterval;
            if (intervalSeconds < 5)
                intervalSeconds = 5;
            else if (intervalSeconds > 300)
                intervalSeconds = 300;
            autoSaveTimer.Interval = intervalSeconds * 1000;
            if (config.General.AutoSaveInterval != intervalSeconds)
            {
                config.General.AutoSaveInterval = intervalSeconds;
                NotesLibrary.Instance.SaveConfiguration();
            }
            
            // Update menu checkbox state
            menuEditAutoSave.Checked = autoSaveEnabled;
            
            if (autoSaveEnabled)
            {
                autoSaveTimer.Start();
            }
            else
            {
                autoSaveTimer.Stop();
            }
            
            // Apply tray icon settings
            trayIcon.Visible = config.General.ShowTrayIcon;
            
            // Apply window settings
            this.TopMost = config.Window.AlwaysOnTop;
            
            // Apply theme after loading configuration
            ApplyCurrentTheme();
            
            // Re-register hotkey if settings changed
            UnregisterGlobalHotkey();
            RegisterGlobalHotkey();
        }

        private void RegisterGlobalHotkey()
        {
            try
            {
                // Make sure handle is created
                if (!this.IsHandleCreated)
                {
                    Logger.Debug("Hotkey registration skipped - Handle not created yet");
                    return;
                }

                var config = NotesLibrary.Instance.Config;
                
                if (!config.Hotkey.Enabled)
                {
                    Logger.Debug("Hotkey registration skipped - Hotkey not enabled in settings");
                    return;
                }

                // Calculate modifier flags
                int modifiers = 0;
                foreach (var modifier in (config.Hotkey.Modifiers ?? Array.Empty<Keys>()))
                {
                    if (modifier == Keys.Control)
                        modifiers |= 0x0002; // MOD_CONTROL
                    else if (modifier == Keys.Alt)
                        modifiers |= 0x0001; // MOD_ALT
                    else if (modifier == Keys.Shift)
                        modifiers |= 0x0004; // MOD_SHIFT
                    else if (modifier == Keys.LWin || modifier == Keys.RWin)
                        modifiers |= 0x0008; // MOD_WIN
                }

                Logger.Debug($"Attempting to register hotkey: Modifiers={modifiers}, Key={config.Hotkey.Key} ({(int)config.Hotkey.Key})");

                // Register the hotkey
                bool success = Win32.RegisterHotKey(this.Handle, HOTKEY_ID, modifiers, (int)config.Hotkey.Key);
                
                if (!success)
                {
                    // Hotkey registration failed (might be already in use)
                    int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    Logger.Warning($"Failed to register global hotkey. Error code: {error}. The hotkey might be in use by another application.");
                    MessageBox.Show("Failed to register global hotkey. The hotkey combination might be in use by another application.", 
                        AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                Logger.Info($"Global hotkey registered successfully: {string.Join("+", (config.Hotkey.Modifiers ?? Array.Empty<Keys>()).Select(m => m.ToString()))}+{config.Hotkey.Key}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering hotkey: {ex.Message}");
            }
        }

        private void UnregisterGlobalHotkey()
        {
            try
            {
                if (this.IsHandleCreated)
                {
                    Win32.UnregisterHotKey(this.Handle, HOTKEY_ID);
                    Logger.Debug("Global hotkey unregistered");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error unregistering hotkey: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error unregistering hotkey: {ex.Message}");
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                Logger.Debug("Global hotkey pressed - activating window");
                // Global hotkey was pressed - bring window to front
                ActivateAndBringToFront();
                return; // Don't pass this message to base
            }

            base.WndProc(ref m);
        }

        private void ActivateAndBringToFront()
        {
            try
            {
                // Show the form if it's hidden (instant since it's kept in RAM)
                if (!this.Visible)
                {
                    this.ShowInTaskbar = true;
                    this.Show();
                    Logger.Debug("Window restored from tray");
                }
                
                // If minimized, restore using Win32 for speed
                if (this.WindowState == FormWindowState.Minimized)
                {
                    Win32.ShowWindow(this.Handle, Win32.SW_RESTORE);
                    this.WindowState = FormWindowState.Normal;
                }
                
                // Force window to top with Win32 APIs for instant response
                Win32.ShowWindow(this.Handle, Win32.SW_SHOW);
                Win32.SetWindowPos(this.Handle, Win32.HWND_TOPMOST, 0, 0, 0, 0, 
                    Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_SHOWWINDOW);
                Win32.SetWindowPos(this.Handle, Win32.HWND_NOTOPMOST, 0, 0, 0, 0, 
                    Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_SHOWWINDOW);
                Win32.SetForegroundWindow(this.Handle);
                
                // Also use managed methods for safety
                this.Activate();
                this.Focus();
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error activating window: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error activating window: {ex.Message}");
            }
        }

        private void RestoreWindowState()
        {
            try
            {
                var config = NotesLibrary.Instance.Config;
                
                if (config.Window.RememberSize)
                {
                    // Restore window size
                    int width = Properties.Settings.Default.WindowWidth;
                    int height = Properties.Settings.Default.WindowHeight;
                    
                    // Ensure minimum window size
                    if (width < 400) width = 800;
                    if (height < 300) height = 600;
                    
                    this.Size = new Size(width, height);
                }
                else
                {
                    this.StartPosition = FormStartPosition.CenterScreen;
                }

                if (config.Window.RememberPosition)
                {
                    // Restore window position if valid
                    int x = Properties.Settings.Default.WindowX;
                    int y = Properties.Settings.Default.WindowY;
                    
                    if (x >= 0 && y >= 0)
                    {
                        // Check if the position is within screen bounds
                        bool isOnScreen = false;
                        foreach (Screen screen in Screen.AllScreens)
                        {
                            if (screen.WorkingArea.Contains(x, y))
                            {
                                isOnScreen = true;
                                break;
                            }
                        }
                        
                        if (isOnScreen)
                        {
                            this.Location = new Point(x, y);
                            this.StartPosition = FormStartPosition.Manual;
                        }
                        else
                        {
                            // Position is off-screen, center the window
                            this.StartPosition = FormStartPosition.CenterScreen;
                        }
                    }
                    else
                    {
                        // First time running, center the window
                        this.StartPosition = FormStartPosition.CenterScreen;
                    }
                }
                else
                {
                    this.StartPosition = FormStartPosition.CenterScreen;
                }
                
                // Apply startup window state from configuration
                this.WindowState = config.Window.State;
                if (config.Window.RememberSize && this.WindowState == FormWindowState.Normal && Properties.Settings.Default.WindowMaximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                
                // If minimized and tray icon is not shown, start as normal instead
                if (config.Window.State == FormWindowState.Minimized && !config.General.ShowTrayIcon)
                {
                    this.WindowState = FormWindowState.Normal;
                    status = "Cannot start minimized without system tray icon enabled";
                }
            }
            catch (Exception)
            {
                // If anything goes wrong, use default window state
                this.Size = new Size(800, 600);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.WindowState = FormWindowState.Normal;
                
                status = "Failed to restore window state, using defaults";
            }
        }

        private void SaveWindowState()
        {
            try
            {
                // Only save if window is in normal state or maximized
                if (this.WindowState == FormWindowState.Minimized)
                    return;
                
                var config = NotesLibrary.Instance.Config;
                
                // Save maximized state only when size is remembered
                Properties.Settings.Default.WindowMaximized = config.Window.RememberSize && (this.WindowState == FormWindowState.Maximized);
                
                // If maximized, we need to get the restored bounds
                if (this.WindowState == FormWindowState.Maximized)
                {
                    if (config.Window.RememberSize)
                    {
                        Properties.Settings.Default.WindowWidth = this.RestoreBounds.Width;
                        Properties.Settings.Default.WindowHeight = this.RestoreBounds.Height;
                    }
                    if (config.Window.RememberPosition)
                    {
                        Properties.Settings.Default.WindowX = this.RestoreBounds.X;
                        Properties.Settings.Default.WindowY = this.RestoreBounds.Y;
                    }
                }
                else
                {
                    // Window is in normal state
                    if (config.Window.RememberSize)
                    {
                        Properties.Settings.Default.WindowWidth = this.Width;
                        Properties.Settings.Default.WindowHeight = this.Height;
                    }
                    if (config.Window.RememberPosition)
                    {
                        Properties.Settings.Default.WindowX = this.Left;
                        Properties.Settings.Default.WindowY = this.Top;
                    }
                }
                
                // Save the settings
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // Silently fail if we can't save window state
                // The application should continue to work normally
                Logger.Warning("Failed to save window state");
            }
        }

        private void ApplyCurrentTheme()
        {
            var config = NotesLibrary.Instance.Config;
            ThemeManager.ApplyTheme(this, config.General.Theme);
            
            // Update panel container specifically
            panelContainer.BackColor = ThemeManager.GetCurrentSurfaceColor(config.General.Theme);
        }

        private void SetupSystemThemeMonitoring()
        {
            // Start monitoring system theme changes
            ThemeManager.StartSystemThemeMonitoring();
            
            // Subscribe to theme change events
            ThemeManager.SystemThemeChanged += OnSystemThemeChanged;
        }

        private void OnSystemThemeChanged(object sender, EventArgs e)
        {
            var config = NotesLibrary.Instance.Config;
            
            // Only apply automatic theme changes if user selected "System Default"
            if (config.General.Theme == NotesLibrary.ThemeMode.SystemDefault)
            {
                // Apply the new theme to main form
                ApplyCurrentTheme();
                
                // Refresh all note buttons with the new theme
                RefreshAllButtons();
                
                status = "Theme updated automatically to follow system setting";
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Initialize logging system
            LoadConfiguration();
            Logger.Initialize(NotesLibrary.Instance.Config.General.LogLevel);
            Logger.Info("Application starting");
            Logger.Debug($"Log level: {NotesLibrary.Instance.Config.General.LogLevel}");
            
            // Restore window size and position
            RestoreWindowState();
            
            ResizePanel();

            var config = NotesLibrary.Instance.Config;
            if (config.General.StartMinimized && config.General.ShowTrayIcon)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                this.ShowInTaskbar = false;
            }
            
            if (string.IsNullOrEmpty(Properties.Settings.Default.JsonData))
            {
                status = "No saved notes found - Create your first note by right-clicking";
                Logger.Info("No existing data found, creating welcome note");
                CreateWelcomeNote();
            }
            else
            {
                Logger.Info("Loading existing notes from storage");
                loadJson(Properties.Settings.Default.JsonData);
                loadConfig();
            }
            
            // Create backup on startup
            if (!string.IsNullOrEmpty(Properties.Settings.Default.JsonData))
            {
                NotesLibrary.Instance.CreateBackup();
            }
            
            // Initialize undo/redo menu state
            UpdateUndoRedoMenuState();
        }

        private void CreateWelcomeNote()
        {
            var welcomeNote = new UnitStruct
            {
                Title = "Welcome to Notes!",
                ContentType = "Text",
                ContentData = "Welcome to your improved Notes application!\n\n" +
                               "• Right-click to create new notes\n" +
                               "• Single-click to copy content\n" +
                               "• Double-click to edit\n" +
                               "• Drag notes around when movable mode is enabled\n" +
                               "• Use Ctrl+F to search your notes\n" +
                               "• Auto-save is enabled by default\n\n" +
                               "Enjoy your organized note-taking!",
                ContentFormat = "plain",
                BackgroundColor = Color.LightSteelBlue.ToArgb(),
                TextColor = Color.DarkBlue.ToArgb(),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ButtonType = "DoubleClickButton",
                X = 50,
                Y = 50,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Category = "System",
                Tags = new string[] { "welcome", "help" }
            };

            welcomeNote.Content = welcomeNote.ContentData;
            
            var id = NotesLibrary.Instance.GenerateId();
            Units.Add(id, welcomeNote);
            addButton(id, welcomeNote);
            
            configModified = true;
            status = "Welcome note created!";
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            ResizePanel();
            
            // Handle minimize to tray
            var config = NotesLibrary.Instance.Config;
            if (this.WindowState == FormWindowState.Minimized && config.General.MinimizeToTray && config.General.ShowTrayIcon)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+A to select all buttons
            if (e.Control && e.KeyCode == Keys.A && isMovable)
            {
                SelectAllButtons();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Escape to clear selection
            else if (e.KeyCode == Keys.Escape && selectedButtons.Count > 0)
            {
                ClearSelection();
                status = "Selection cleared";
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Delete to delete selected buttons
            else if (e.KeyCode == Keys.Delete && selectedButtons.Count > 0)
            {
                DeleteSelectedButtons();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void SelectAllButtons()
        {
            ClearSelection();
            foreach (Control control in GetAllButtonsInPanel())
            {
                if (control is Button button)
                {
                    SelectButton(button);
                }
            }
            status = $"Selected {selectedButtons.Count} buttons";
        }

        private void DeleteSelectedButtons()
        {
            if (selectedButtons.Count == 0)
                return;

            var config = NotesLibrary.Instance.Config;
            DialogResult result = DialogResult.Yes;
            if (config.General.ConfirmDelete)
            {
                result = MessageBox.Show(
                    $"Do you want to delete {selectedButtons.Count} selected button(s)?", 
                    AppName, 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
            }

            if (result == DialogResult.Yes)
            {
                SaveStateForUndo();
                var buttonsToDelete = selectedButtons.ToList();

                foreach (var btn in buttonsToDelete)
                {
                    string id = (string)btn.Tag;
                    if (Units.ContainsKey(id))
                    {
                        var unit = Units[id];
                        Units.Remove(id);
                        RemoveButtonControl(btn);
                        if (unit.Font != null)
                            DisposeFontIfUnused(unit.Font);
                    }
                }

                foreach (var btn in buttonsToDelete)
                {
                    selectionOriginalStyles.Remove(btn);
                }
                selectedButtons.Clear();
                configModified = true;
                status = $"Deleted {buttonsToDelete.Count} button(s)";
                UpdateUndoRedoMenuState();
            }
        }

        private void ResizePanel()
        {
            panelContainer.Size = new Size(this.Width, this.Height - menuStrip.Height - statusStrip.Height);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            var config = NotesLibrary.Instance.Config;
            
            // Check if we should minimize to tray instead of closing
            if (!forceExit && config.General.CloseToTray && config.General.ShowTrayIcon && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                // Keep window in RAM for instant reopening - just hide it without minimizing
                this.Hide();
                this.ShowInTaskbar = false;
                Logger.Debug("Application hidden to tray (close to tray enabled) - keeping in RAM for instant reopening");
                return;
            }

            if (!forceExit && config.General.ConfirmExit && e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult exitResult = MessageBox.Show("Are you sure you want to exit?", AppName, 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (exitResult != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            
            // Only cleanup if actually closing (not just hiding to tray)
            if (autoSaveTimer != null)
                autoSaveTimer.Stop();
            
            // Unregister global hotkey only when actually closing
            UnregisterGlobalHotkey();
            Logger.Info("Unregistered global hotkey - application closing");
            
            // Stop monitoring system theme changes
            ThemeManager.StopSystemThemeMonitoring();
            
            if (configModified)
            {
                if (config.General.AutoSave)
                {
                    if (saveJson(false))
                    {
                        status = "Auto-saved before closing";
                    }
                    else
                    {
                        status = "Auto-save failed";
                    }
                    if (configModified)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("Do you want to save changes?", AppName, 
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (saveJson(true))
                        {
                            status = "Saved successfully";
                        }
                        else
                        {
                            status = "Save failed";
                        }
                        if (configModified)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return; // Don't save window state if user cancels
                    }
                }
            }
            
            // Save window state again if the operation wasn't cancelled
            if (!e.Cancel)
            {
                SaveWindowState();
                DisposeAllUnitFonts();
            }
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // Only handle left click
            if (e.Button == MouseButtons.Left)
            {
                // Instant show - window is kept in RAM
                ActivateAndBringToFront();
            }
        }

        private void trayMenuOpen_Click(object sender, EventArgs e)
        {
            // Show and activate the window
            this.Show();
            this.ShowInTaskbar = true;
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            ActivateAndBringToFront();
        }

        private void trayMenuResetPosition_Click(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.CenterToScreen();
            SaveWindowState();
            status = "Window position reset to center";
        }

        private void trayMenuResetSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.CenterToScreen();
            SaveWindowState();
            status = "Window size reset to 800x600";
        }

        private void trayMenuExit_Click(object sender, EventArgs e)
        {
            forceExit = true;
            Application.Exit();
        }

        private async void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (isAutoSaving)
                return;

            if (configModified && autoSaveEnabled)
            {
                try
                {
                    isAutoSaving = true;
                    var formatting = NotesLibrary.Instance.Config.General.OptimizeForLargeFiles
                        ? Formatting.None
                        : Formatting.Indented;
                    var snapshot = new NotesData
                    {
                        Units = CloneUnits(Units),
                        Groups = CloneGroups(Groups)
                    };
                    var json = await Task.Run(() => JsonConvert.SerializeObject(snapshot, formatting));
                    bool saved = SaveJsonSerialized(json, showErrors: false, includeWindowState: true);
                    status = saved ? "Auto-saved" : "Auto-save failed";
                }
                finally
                {
                    isAutoSaving = false;
                }
            }
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            // TODO: Implement search functionality
            searchTimer.Stop();
        }

        private string getNewId() => NotesLibrary.Instance.GenerateId();

        private void SaveStateForUndo()
        {
            var snapshot = CreateStateSnapshot();
            undoStack.Push(snapshot);
            
            int maxUndoLevels = NotesLibrary.Instance.Config.General.UndoLevels;
            if (maxUndoLevels < 1)
                maxUndoLevels = 1;

            if (undoStack.Count > maxUndoLevels)
            {
                var temp = undoStack.ToArray().Take(maxUndoLevels).Reverse().ToArray();
                undoStack.Clear();
                foreach (var item in temp)
                    undoStack.Push(item);
            }
            
            redoStack.Clear();
        }

        public void PerformUndo()
        {
            if (undoStack.Count > 0)
            {
                var currentState = CreateStateSnapshot();
                redoStack.Push(currentState);

                var previousState = undoStack.Pop();
                RestoreState(previousState);
                RefreshAllButtons();
                configModified = true;
                status = "Undo successful";
            }
        }

        public void PerformRedo()
        {
            if (redoStack.Count > 0)
            {
                var currentState = CreateStateSnapshot();
                undoStack.Push(currentState);

                var redoState = redoStack.Pop();
                RestoreState(redoState);
                RefreshAllButtons();
                configModified = true;
                status = "Redo successful";
            }
        }

        private void RefreshAllButtons()
        {
            ClearSelection();
            bool optimize = NotesLibrary.Instance.Config.General.OptimizeForLargeFiles;
            if (optimize)
                panelContainer.SuspendLayout();

            try
            {
                foreach (var control in panelContainer.Controls.OfType<Control>().ToList())
                {
                    control.Dispose();
                }
                panelContainer.Controls.Clear();

                foreach (var group in Groups.Values.OrderBy(g => g.X).ThenBy(g => g.Y))
                {
                    AddGroupBoxToPanel(group);
                }

                foreach (var kvp in Units)
                {
                    addButton(kvp.Key, kvp.Value);
                }

                UpdateUndoRedoMenuState();
            }
            finally
            {
                if (optimize)
                    panelContainer.ResumeLayout(true);
            }
        }

        private bool loadJson(string json)
        {
            status = "Loading notes...";

            try
            {
                var data = JsonConvert.DeserializeObject<NotesData>(json);

                if (data == null || data.Units == null)
                {
                    status = "No notes found";
                    return false;
                }

                Units = data.Units;
                foreach (var key in Units.Keys.ToList())
                {
                    var unit = Units[key];
                    unit.ButtonType = NormalizeButtonType(unit.ButtonType);
                    Units[key] = unit;
                }
                var loadedGroups = data.Groups ?? new Dictionary<string, GroupStruct>();
                foreach (var key in loadedGroups.Keys.ToList())
                {
                    var group = loadedGroups[key];
                    if (string.IsNullOrEmpty(group.Id) || !string.Equals(group.Id, key, StringComparison.Ordinal))
                        group.Id = key;
                    group.GroupBoxType = NormalizeGroupBoxTypeCaseInsensitive(group.GroupBoxType);
                    loadedGroups[key] = group;
                }
                Groups = loadedGroups;
                foreach (var key in Units.Keys.ToList())
                {
                    var unit = Units[key];
                    if (!string.IsNullOrEmpty(unit.GroupId) && !Groups.ContainsKey(unit.GroupId))
                    {
                        unit.GroupId = null;
                        Units[key] = unit;
                    }
                }

                RefreshAllButtons();
                undoStack.Clear();
                redoStack.Clear();
                UpdateUndoRedoMenuState();

                status = string.Format("Loaded {0} notes successfully", Units.Count);
                return true;
            }
            catch (Exception ex)
            {
                status = "Failed to load notes";
                MessageBox.Show("Error loading notes: " + ex.Message, AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool loadConfig()
        {
            try
            {
                isAutofocus = Properties.Settings.Default.configAutofocus;
                menuEditAutofocus.Checked = isAutofocus;
                return true;
            }
            catch (Exception ex)
            {
                status = "Failed to load configuration";
                MessageBox.Show("Error loading configuration: " + ex.Message, AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void addButton(string id, UnitStruct unit)
        {
            unit.ButtonType = NormalizeButtonType(unit.ButtonType);
            if (Units.ContainsKey(id))
                Units[id] = unit;

            Button newButton = CreateButtonByType(unit.ButtonType);
            newButton.Tag = id;
            newButton.AutoSize = true;
            newButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            newButton.Text = unit.Title;
            newButton.BackColor = Color.FromArgb(unit.BackgroundColor);
            newButton.ForeColor = Color.FromArgb(unit.TextColor);
            newButton.Font = unit.Font ?? NotesLibrary.Instance.GetDefaultFont();

            GroupBox targetGroupBox = null;
            if (!string.IsNullOrEmpty(unit.GroupId))
            {
                targetGroupBox = GetOrCreateGroupBox(unit.GroupId);
            }

            newButton.ContextMenuStrip = unitMenuStrip;
            newButton.Cursor = Cursors.Hand;
            
            // Add padding and prevent text wrapping for custom buttons
            if (!string.IsNullOrEmpty(unit.ButtonType) && unit.ButtonType != "DoubleClickButton")
            {
                newButton.Padding = new Padding(12, 8, 12, 8);
                newButton.MinimumSize = new Size(80, 40);
            }

            // Set up events
            newButton.Click += newButton_Click;
            newButton.DoubleClick += newButton_DoubleClick;
            newButton.MouseUp += newButton_MouseUp;
            newButton.MouseDown += newButton_MouseDown;
            newButton.MouseMove += newButton_MouseMove;
            newButton.PreviewKeyDown += newButton_PreviewKeyDown;
            newButton.KeyDown += newButton_KeyDown;
            newButton.KeyUp += newButton_KeyUp;

            // Add button to appropriate container
            if (targetGroupBox != null)
            {
                Point relativePos = new Point(
                    unit.X - targetGroupBox.Location.X,
                    unit.Y - targetGroupBox.Location.Y
                );
                int minX = 0;
                int minY = Math.Max(0, targetGroupBox.DisplayRectangle.Top);
                int maxX = Math.Max(minX, targetGroupBox.ClientSize.Width - newButton.Width);
                int maxY = Math.Max(minY, targetGroupBox.ClientSize.Height - newButton.Height);

                int clampedX = Math.Min(Math.Max(relativePos.X, minX), maxX);
                int clampedY = Math.Min(Math.Max(relativePos.Y, minY), maxY);
                if (clampedX != relativePos.X || clampedY != relativePos.Y)
                {
                    relativePos = new Point(clampedX, clampedY);
                    unit.X = targetGroupBox.Location.X + relativePos.X;
                    unit.Y = targetGroupBox.Location.Y + relativePos.Y;
                    if (Units.ContainsKey(id))
                        Units[id] = unit;
                }
                newButton.Location = relativePos;
                targetGroupBox.Controls.Add(newButton);
                newButton.Visible = true;
                newButton.BringToFront();
            }
            else
            {
                int maxX = Math.Max(0, panelContainer.ClientSize.Width - newButton.Width);
                int maxY = Math.Max(0, panelContainer.ClientSize.Height - newButton.Height);
                int clampedX = Math.Min(Math.Max(unit.X, 0), maxX);
                int clampedY = Math.Min(Math.Max(unit.Y, 0), maxY);
                if (clampedX != unit.X || clampedY != unit.Y)
                {
                    unit.X = clampedX;
                    unit.Y = clampedY;
                    if (Units.ContainsKey(id))
                        Units[id] = unit;
                }
                newButton.Location = new Point(unit.X, unit.Y);
                panelContainer.Controls.Add(newButton);
            }
        }

        private void ApplyUnitChangesToButton(Button btn, UnitStruct unit)
        {
            if (btn == null)
                return;

            btn.Text = unit.Title;
            btn.BackColor = Color.FromArgb(unit.BackgroundColor);
            btn.ForeColor = Color.FromArgb(unit.TextColor);
            btn.Font = unit.Font ?? NotesLibrary.Instance.GetDefaultFont();

            Control targetParent = panelContainer;
            Point targetLocation = new Point(unit.X, unit.Y);

            if (!string.IsNullOrEmpty(unit.GroupId))
            {
                GroupBox targetGroup = GetOrCreateGroupBox(unit.GroupId);
                if (targetGroup != null)
                {
                    targetParent = targetGroup;

                    int relativeX = unit.X - targetGroup.Location.X;
                    int relativeY = unit.Y - targetGroup.Location.Y;

                    int minX = 0;
                    int minY = Math.Max(0, targetGroup.DisplayRectangle.Top);
                    int maxX = Math.Max(minX, targetGroup.ClientSize.Width - btn.Width);
                    int maxY = Math.Max(minY, targetGroup.ClientSize.Height - btn.Height);

                    relativeX = Math.Min(Math.Max(relativeX, minX), maxX);
                    relativeY = Math.Min(Math.Max(relativeY, minY), maxY);

                    targetLocation = new Point(relativeX, relativeY);
                }
                else
                {
                    targetParent = panelContainer;
                    targetLocation = new Point(unit.X, unit.Y);
                }
            }

            if (btn.Parent != targetParent)
            {
                btn.Parent?.Controls.Remove(btn);
                targetParent.Controls.Add(btn);
            }

            btn.Location = targetLocation;
            btn.BringToFront();

            SaveButtonLocationAndGroup(btn);
        }

        private Button CreateButtonByType(string buttonType)
        {
            if (string.IsNullOrEmpty(buttonType))
                return new DoubleClickButton();

            switch (buttonType)
            {
                case "GradientButton":
                    return new GradientButton();
                case "NeonGlowButton":
                    return new NeonGlowButton();
                case "MaterialButton":
                    return new MaterialButton();
                case "GlassMorphismButton":
                    return new GlassMorphismButton();
                case "NeumorphismButton":
                    return new NeumorphismButton();
                case "Retro3DButton":
                    return new Retro3DButton();
                case "PremiumCardButton":
                    return new PremiumCardButton();
                case "OutlineButton":
                    return new OutlineButton();
                case "PillButton":
                    return new PillButton();
                case "SkeuomorphicButton":
                    return new SkeuomorphicButton();
                default:
                    return new DoubleClickButton();
            }
        }

        private class ContextMenuInfo
        {
            public Button Button { get; set; }
            public string Id { get; set; }
        }

        private ContextMenuInfo getContextMenuInfo(object sender)
        {
            //Get clicked item
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // For nested menus, we need to traverse up to find the root ContextMenuStrip
                ToolStripDropDown currentDropDown = menuItem.Owner as ToolStripDropDown;
                while (currentDropDown != null)
                {
                    // Check if this is the root ContextMenuStrip
                    ContextMenuStrip contextMenu = currentDropDown as ContextMenuStrip;
                    if (contextMenu != null)
                    {
                        Button btn = contextMenu.SourceControl as Button;
                        if (btn != null)
                        {
                            return new ContextMenuInfo { Button = btn, Id = (string)btn.Tag };
                        }
                        break;
                    }
                    
                    // Move up to the parent dropdown
                    if (currentDropDown.OwnerItem != null)
                    {
                        currentDropDown = currentDropDown.OwnerItem.Owner as ToolStripDropDown;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return null;
        }

        private bool SaveJsonSerialized(string json, bool showErrors, bool includeWindowState)
        {
            try
            {
                lock (saveLock)
                {
                    Properties.Settings.Default.JsonData = json;
                    Properties.Settings.Default.configAutofocus = isAutofocus;

                    if (includeWindowState)
                        SaveWindowState();
                    Properties.Settings.Default.Save();
                }

                configModified = false;
                return true;
            }
            catch (Exception ex)
            {
                if (showErrors)
                {
                    MessageBox.Show("Error saving notes: " + ex.Message, AppName, 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        private bool saveJson(bool showErrors)
        {
            var data = new NotesData
            {
                Units = Units,
                Groups = Groups
            };

            var formatting = NotesLibrary.Instance.Config.General.OptimizeForLargeFiles
                ? Formatting.None
                : Formatting.Indented;
            var json = JsonConvert.SerializeObject(data, formatting);
            return SaveJsonSerialized(json, showErrors, includeWindowState: true);
        }

        private async Task<bool> saveJsonAsync(bool showErrors)
        {
            var formatting = NotesLibrary.Instance.Config.General.OptimizeForLargeFiles
                ? Formatting.None
                : Formatting.Indented;
            var snapshot = new NotesData
            {
                Units = CloneUnits(Units),
                Groups = CloneGroups(Groups)
            };
            var json = await Task.Run(() => JsonConvert.SerializeObject(snapshot, formatting));
            return SaveJsonSerialized(json, showErrors, includeWindowState: true);
        }

        private bool saveJson() => saveJson(true);











        private void newButton_Click(object sender, EventArgs e)
        {
            _unitClickSender = sender;
            _unitClickE = e;
            tmrClickHandle.Start();
        }

        private void newButton_DoubleClick(object sender, EventArgs e)
        {
            _unitDoubleClicked = true;
        }

        private bool SaveButtonLocationAndGroup(Button btn)
        {
            string id = (string)btn.Tag;

            if (Units.ContainsKey(id))
            {
                var item = Units[id];

                if (btn.Parent is GroupBox groupBox)
                {
                    item.X = groupBox.Location.X + btn.Location.X;
                    item.Y = groupBox.Location.Y + btn.Location.Y;
                    item.GroupId = groupBox.Tag as string;
                }
                else
                {
                    item.X = btn.Location.X;
                    item.Y = btn.Location.Y;
                    item.GroupId = null;
                }

                Units[id] = item;
                configModified = true;
                return true;
            }

            status = "Button not found";
            return false;
        }

        private void newButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMovable && isMovingGroup && selectedButtons.Count > 1)
            {
                // End group movement
                isMovingGroup = false;
                
                // Save all button positions
                foreach (var btn in selectedButtons)
                {
                    SaveButtonLocationAndGroup(btn);
                }
                
                groupOriginalPositions.Clear();
                _unitMouseMoved = true; // Prevent click action
                status = "Group move completed";
            }
            else if (isMovable && this.BtnDragging)
            {
                this.BtnDragging = false;

                if (this.Origin_Cursor.X - Cursor.Position.X != 0 || this.Origin_Cursor.Y - Cursor.Position.Y != 0)
                {
                    _unitMouseMoved = true;
                    SaveButtonLocationAndGroup(sender as Button);
                }
            }
        }

        private void newButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (isMovable && e.Button == MouseButtons.Left)
            {
                Button ct = sender as Button;
                
                // If this button is already selected and we have multiple selections,
                // start group movement
                if (selectedButtons.Contains(ct) && selectedButtons.Count > 1)
                {
                    isMovingGroup = true;
                    groupMoveStart = ct.PointToScreen(e.Location);
                    groupMoveStart = panelContainer.PointToClient(groupMoveStart);
                    groupOriginalPositions.Clear();
                    foreach (var btn in selectedButtons)
                    {
                        groupOriginalPositions[btn] = btn.Location;
                    }
                    return;
                }
                
                // Single button drag
                ct.Capture = true;
                this.Origin_Cursor = System.Windows.Forms.Cursor.Position;
                this.Origin_Control = ct.Location;
                this.BtnDragging = true;
                
                // Clear selection when dragging a single button
                if (selectedButtons.Count > 0)
                {
                    ClearSelection();
                }
            }
        }

        private void newButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMovable && isMovingGroup && selectedButtons.Count > 1)
            {
                // Handle group movement
                Button btn = sender as Button;
                Point currentPos = btn.PointToScreen(e.Location);
                currentPos = panelContainer.PointToClient(currentPos);
                
                int deltaX = currentPos.X - groupMoveStart.X;
                int deltaY = currentPos.Y - groupMoveStart.Y;
                
                panelContainer.SuspendLayout();
                foreach (var selectedBtn in selectedButtons)
                {
                    if (groupOriginalPositions.ContainsKey(selectedBtn))
                    {
                        Rectangle oldBounds = selectedBtn.Bounds;
                        selectedBtn.Location = new Point(
                            groupOriginalPositions[selectedBtn].X + deltaX,
                            groupOriginalPositions[selectedBtn].Y + deltaY
                        );
                        
                        // Invalidate both old and new positions
                        if (selectedBtn.Parent != null)
                        {
                            selectedBtn.Parent.Invalidate(oldBounds);
                            selectedBtn.Parent.Invalidate(selectedBtn.Bounds);
                        }
                    }
                }
                panelContainer.ResumeLayout(false);
                panelContainer.Update();
                status = "Moving selected buttons";
            }
            else if (isMovable && this.BtnDragging)
            {
                Button btn = sender as Button;
                Control parent = btn.Parent;
                
                // Store old bounds for invalidation
                Rectangle oldBounds = btn.Bounds;
                
                // Calculate new position
                int newLeft = this.Origin_Control.X - (this.Origin_Cursor.X - Cursor.Position.X);
                int newTop = this.Origin_Control.Y - (this.Origin_Cursor.Y - Cursor.Position.Y);
                
                // Update position
                btn.Left = newLeft;
                btn.Top = newTop;
                
                // Invalidate old and new areas to prevent visual artifacts
                if (parent != null)
                {
                    parent.Invalidate(oldBounds);
                    parent.Invalidate(btn.Bounds);
                    parent.Update();
                }
                
                status = "Moving";
            }
            else if (isAutofocus)
            {
                Button btn = sender as Button;
                btn.Focus();
            }
        }

        private void newButton_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
        }

        private void newButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                Button btn = sender as Button;
                btn.Left -= 1;

                btnMovingArrow = true;
                status = "Moving";
            }
            else if (e.KeyCode == Keys.Right)
            {
                Button btn = sender as Button;
                btn.Left += 1;

                btnMovingArrow = true;
                status = "Moving";
            }
            else if (e.KeyCode == Keys.Up)
            {
                Button btn = sender as Button;
                btn.Top -= 1;

                btnMovingArrow = true;
                status = "Moving";
            }
            else if (e.KeyCode == Keys.Down)
            {
                Button btn = sender as Button;
                btn.Top += 1;

                btnMovingArrow = true;
                status = "Moving";
            }
        }

        private void newButton_KeyUp(object sender, KeyEventArgs e)
        {
            if (btnMovingArrow)
            {
                btnMovingArrow = false;
                SaveButtonLocationAndGroup(sender as Button);
            }
        }









        private void unit_Click_Handle(object sender, EventArgs e)
        {
            if (sender is Button ct && ct.Tag is string id)
            {
                UnitStruct unit;
                if (Units.TryGetValue(id, out unit))
                {
                    var message = ClipboardHelper.CopyUnitToClipboard(unit);
                    status = message;
                    return;
                }
            }

            status = "Unable to copy content";
        }

        private void unit_DoubleClick_Handle(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string id = (string)btn.Tag;

            //Check if key exists
            if (Units.ContainsKey(id))
            {
                //Load edit form
                selectedUnitModified = false;
                selectedUnit = Units[id];
                using var editForm = new frmEdit();
                editForm.ShowDialog();

                if (selectedUnitModified)
                {
                    SaveStateForUndo();
                    selectedUnitModified = false;

                    Units[id] = selectedUnit;
                    ApplyUnitChangesToButton(btn, selectedUnit);

                    configModified = true;
                    status = "Updated successfully";
                    UpdateUndoRedoMenuState();
                }
            }
        }

        private void tmrClickHandle_Tick(object sender, EventArgs e)
        {
            tmrClickHandle.Stop();

            if (_unitMouseMoved)
            {
                _unitMouseMoved = false;
            }
            else
            {
                var config = NotesLibrary.Instance.Config;
                if (_unitDoubleClicked)
                {
                    if (config.General.DoubleClickToEdit)
                        unit_DoubleClick_Handle(_unitClickSender, _unitClickE);
                    else if (config.General.SingleClickToCopy)
                        unit_Click_Handle(_unitClickSender, _unitClickE);
                }
                else
                {
                    if (config.General.SingleClickToCopy)
                        unit_Click_Handle(_unitClickSender, _unitClickE);
                }
            }

            _unitDoubleClicked = false;
        }










        private void panelContainer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isMovable)
            {
                // Check if clicking on empty space (not on a button)
                Control clickedControl = panelContainer.GetChildAtPoint(e.Location);
                if (clickedControl == null)
                {
                    // Start selection rectangle
                    isSelecting = true;
                    selectionStart = e.Location;
                    selectionEnd = e.Location;
                    
                    // Clear previous selection if not holding Ctrl
                    if (!ModifierKeys.HasFlag(Keys.Control))
                    {
                        ClearSelection();
                    }
                }
            }
        }

        private void panelContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                selectionEnd = e.Location;
                UpdateSelectionRectangle();
                panelContainer.Invalidate(); // Trigger repaint
            }
        }

        private void panelContainer_MouseUp(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                UpdateSelectionRectangle();
                SelectButtonsInRectangle();
                panelContainer.Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                selectedUnit = new UnitStruct();
                selectedUnit.X = e.X;
                selectedUnit.Y = e.Y;

                menuFileNew_Click(sender, e);
            }
        }

        private void panelContainer_Paint(object sender, PaintEventArgs e)
        {
            if (isSelecting && !selectionRectangle.IsEmpty)
            {
                // Draw selection rectangle
                using (Pen pen = new Pen(Color.DodgerBlue, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, selectionRectangle);
                }
                
                // Fill with semi-transparent color
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.DodgerBlue)))
                {
                    e.Graphics.FillRectangle(brush, selectionRectangle);
                }
            }
        }

        private void UpdateSelectionRectangle()
        {
            int x = Math.Min(selectionStart.X, selectionEnd.X);
            int y = Math.Min(selectionStart.Y, selectionEnd.Y);
            int width = Math.Abs(selectionEnd.X - selectionStart.X);
            int height = Math.Abs(selectionEnd.Y - selectionStart.Y);
            
            selectionRectangle = new Rectangle(x, y, width, height);
        }

        private void SelectButtonsInRectangle()
        {
            foreach (Control control in panelContainer.Controls)
            {
                if (control is Button btn)
                {
                    if (selectionRectangle.IntersectsWith(btn.Bounds))
                    {
                        SelectButton(btn);
                    }
                }
                else if (control is GroupBox groupBox)
                {
                    // Search for buttons inside the groupbox
                    foreach (Control childControl in groupBox.Controls)
                    {
                        if (childControl is Button childBtn)
                        {
                            // Convert child button bounds to panel coordinates
                            Point childLocation = groupBox.PointToScreen(childBtn.Location);
                            Point panelLocation = panelContainer.PointToClient(childLocation);
                            Rectangle childBounds = new Rectangle(panelLocation, childBtn.Size);
                            
                            if (selectionRectangle.IntersectsWith(childBounds))
                            {
                                SelectButton(childBtn);
                            }
                        }
                    }
                }
            }
        }

        private void SelectButton(Button btn)
        {
            if (!selectedButtons.Contains(btn))
            {
                selectedButtons.Add(btn);
                UpdateButtonSelectionVisual(btn, true);
            }
        }

        private void DeselectButton(Button btn)
        {
            if (selectedButtons.Contains(btn))
            {
                selectedButtons.Remove(btn);
                UpdateButtonSelectionVisual(btn, false);
            }
        }

        private void ClearSelection()
        {
            foreach (var btn in selectedButtons.ToList())
            {
                UpdateButtonSelectionVisual(btn, false);
                selectionOriginalStyles.Remove(btn);
            }
            selectedButtons.Clear();
        }

        private void UpdateButtonSelectionVisual(Button btn, bool isSelected)
        {
            if (isSelected)
            {
                if (!selectionOriginalStyles.ContainsKey(btn))
                    selectionOriginalStyles[btn] = new SelectionStyle
                    {
                        FlatStyle = btn.FlatStyle,
                        BorderColor = btn.FlatAppearance.BorderColor,
                        BorderSize = btn.FlatAppearance.BorderSize
                    };
                // Add visual indicator for selection
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.DodgerBlue;
                btn.FlatAppearance.BorderSize = 3;
            }
            else
            {
                // Remove selection visual
                if (selectionOriginalStyles.TryGetValue(btn, out var original))
                {
                    btn.FlatStyle = original.FlatStyle;
                    btn.FlatAppearance.BorderColor = original.BorderColor;
                    btn.FlatAppearance.BorderSize = original.BorderSize;
                    selectionOriginalStyles.Remove(btn);
                }
                else
                {
                    btn.FlatStyle = FlatStyle.Standard;
                }
            }
        }






        private void menuFileNew_Click(object sender, EventArgs e)
        {
            using var addForm = new frmAdd();
            selectedUnitModified = false;
            addForm.ShowDialog();

            if (selectedUnitModified)
            {
                SaveStateForUndo();
                selectedUnitModified = false;
                var id = getNewId();

                Units.Add(id, selectedUnit);
                addButton(id, selectedUnit);
                selectedUnit = new UnitStruct();

                configModified = true;
                status = "New button added";
                UpdateUndoRedoMenuState();
            }
        }

        private void menuFileNewGroup_Click(object sender, EventArgs e)
        {
            Logger.LogMethodEntry("menuFileNewGroup_Click");
            
            using var addGroupForm = new frmAddGroup { IsEditMode = false };
            frmAddGroup.selectedGroupModified = false;
            frmAddGroup.selectedGroup = new GroupStruct();

            addGroupForm.ShowDialog();

            if (frmAddGroup.selectedGroupModified)
            {
                SaveStateForUndo();
                frmAddGroup.selectedGroupModified = false;
                var group = frmAddGroup.selectedGroup;

                Logger.Info($"Creating new group: Id={group.Id}, Title={group.Title}, Location=({group.X},{group.Y}), Size=({group.Width}x{group.Height})");
                
                Groups.Add(group.Id, group);
                AddGroupBoxToPanel(group);

                configModified = true;
                status = "New group added";
                UpdateUndoRedoMenuState();
                
                Logger.Debug($"Group added successfully. Total groups: {Groups.Count}");
            }
            
            Logger.LogMethodExit("menuFileNewGroup_Click");
        }

        private async void menuFileSave_Click(object sender, EventArgs e)
        {
            if (await saveJsonAsync(true))
                status = "Saved successfully";
            else
                status = "Save failed";
        }

        private void menuFileReset_Click(object sender, EventArgs e)
        {
            var config = NotesLibrary.Instance.Config;
            DialogResult result = DialogResult.OK;
            if (config.General.ConfirmReset)
            {
                result = MessageBox.Show("Do you want to delete all buttons?", AppName, MessageBoxButtons.OKCancel);
            }

            if (result == DialogResult.OK)
            {
                SaveStateForUndo();
                ClearSelection();
                DisposeAllUnitFonts();
                foreach (var control in panelContainer.Controls.OfType<Control>().ToList())
                {
                    control.Dispose();
                }
                panelContainer.Controls.Clear();
                Units.Clear();
                Groups.Clear();
                undoStack.Clear();
                redoStack.Clear();

                configModified = true;
                status = "All buttons deleted successfully";
                UpdateUndoRedoMenuState();
            }
        }

        private void menuFileReload_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to reload config?", AppName, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                if (loadJson(Properties.Settings.Default.JsonData))
                {
                    loadConfig();
                    configModified = false;
                }
            }
        }

        private void menuFileImport_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Text Documents (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    var preImportState = CreateStateSnapshot();
                    bool previousConfigModified = configModified;
                    try
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var data = JsonConvert.DeserializeObject<NotesData>(json);
                        if (data == null || data.Units == null)
                            throw new InvalidDataException("Import file is invalid or corrupted.");
                        var newUnits = data?.Units ?? new Dictionary<string, UnitStruct>();
                        var newGroups = data?.Groups ?? new Dictionary<string, GroupStruct>();

                        SaveStateForUndo();
                        
                        // Map old group IDs to new group IDs
                        var groupIdMap = new Dictionary<string, string>();
                        
                        foreach (var keyValuePair in newGroups)
                        {
                            var oldGroupId = keyValuePair.Key;
                            var newGroupId = getNewId();
                            var group = keyValuePair.Value;
                            group.Id = newGroupId;
                            
                            groupIdMap[oldGroupId] = newGroupId;
                            Groups.Add(newGroupId, group);
                            AddGroupBoxToPanel(group);
                        }

                        foreach (var keyValuePair in newUnits)
                        {
                            var id = getNewId();
                            var unit = keyValuePair.Value;
                            
                            // Remap GroupId if the unit belongs to an imported group
                            if (!string.IsNullOrEmpty(unit.GroupId) && groupIdMap.ContainsKey(unit.GroupId))
                            {
                                unit.GroupId = groupIdMap[unit.GroupId];
                            }
                            else
                            {
                                unit.GroupId = null;
                            }
                            
                            Units.Add(id, unit);
                            addButton(id, unit);
                        }

                        configModified = true;
                        status = string.Format("{0} notes and {1} groups imported successfully", newUnits.Count(), newGroups.Count());
                        UpdateUndoRedoMenuState();
                    }
                    catch (Exception ex)
                    {
                        RestoreState(preImportState);
                        RefreshAllButtons();
                        configModified = previousConfigModified;
                        status = "Import failed";
                        MessageBox.Show("Import failed: " + ex.Message, AppName, 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Selected file no longer exists.", AppName, 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
        }

        private void menuFileExport_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.OverwritePrompt = true;
            if (string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                saveFileDialog.FileName = "notes_export.json";

            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    using (var stream = saveFileDialog.OpenFile())
                    {
                        if (stream != null)
                        {
                            using (var writer = new StreamWriter(stream))
                            {
                                var data = new NotesData
                                {
                                    Units = Units,
                                    Groups = Groups
                                };
                                var formatting = NotesLibrary.Instance.Config.General.OptimizeForLargeFiles
                                    ? Formatting.None
                                    : Formatting.Indented;
                                writer.Write(JsonConvert.SerializeObject(data, formatting));
                            }

                            status = string.Format("{0} notes and {1} groups exported successfully", Units.Count(), Groups.Count());
                            return;
                        }
                    }

                    MessageBox.Show("Export failed: unable to open the selected file.", AppName, 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting notes: " + ex.Message, AppName, 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Help Menu Handlers
        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            using (frmAbout aboutForm = new frmAbout())
            {
                aboutForm.ShowDialog(this);
            }
        }




        private void menuEditMovable_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            isMovable = item.Checked;

            // Update AllowResize for all group boxes
            foreach (var control in panelContainer.Controls.OfType<CustomGroupBoxBase>())
            {
                control.AllowResize = isMovable;
                control.UpdateResizeHandleVisibility();
            }
            foreach (var control in panelContainer.Controls.OfType<ResizableGroupBox>())
            {
                control.AllowResize = isMovable;
                control.UpdateResizeHandleVisibility();
            }

            if (item.Checked)
            {
                status = "Movable buttons / Resizable groups";
            }
            else
            {
                status = "Non-movable buttons / Non-resizable groups";
            }
        }


        private void menuEditAutofocus_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            isAutofocus = item.Checked;

            if (item.Checked)
            {
                status = "Autofocus buttons";
            }
            else
            {
                status = "Non-autofocus buttons";
            }
        }

        private void menuEditAutoSave_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            autoSaveEnabled = item.Checked;
            
            var config = NotesLibrary.Instance.Config;
            config.General.AutoSave = autoSaveEnabled;
            NotesLibrary.Instance.SaveConfiguration();

            if (autoSaveEnabled)
            {
                autoSaveTimer.Start();
                status = "Auto-save enabled";
            }
            else
            {
                autoSaveTimer.Stop();
                status = "Auto-save disabled";
            }
        }

        private void menuEditUndo_Click(object sender, EventArgs e)
        {
            PerformUndo();
            UpdateUndoRedoMenuState();
        }

        private void menuEditRedo_Click(object sender, EventArgs e)
        {
            PerformRedo();
            UpdateUndoRedoMenuState();
        }

        private void UpdateUndoRedoMenuState()
        {
            toolStripMenuItem1.Enabled = (undoStack.Count > 0);
            toolStripMenuItem2.Enabled = (redoStack.Count > 0);
        }











        private void unitMenuEdit_Click(object sender, EventArgs e)
        {
            //Get clicked item
            var item = getContextMenuInfo(sender);

            if (item != null)
            {
                var btn = item.Button;
                var id = item.Id;

                if (Units.ContainsKey(id))
                {
                    //Load edit form
                    selectedUnitModified = false;
                    selectedUnit = Units[id];
                    using var editForm = new frmEdit();
                    editForm.ShowDialog();

                    if (selectedUnitModified)
                    {
                        SaveStateForUndo();
                        selectedUnitModified = false;

                        Units[id] = selectedUnit;
                        ApplyUnitChangesToButton(btn, selectedUnit);

                        configModified = true;
                        status = "Updated successfully";
                        UpdateUndoRedoMenuState();
                    }
                }
            }
        }

        private void unitMenuDelete_Click(object sender, EventArgs e)
        {
            //Get clicked item
            var item = getContextMenuInfo(sender);

            if (item != null)
            {
                var btn = item.Button;
                var id = item.Id;

                if (Units.ContainsKey(id))
                {
                    var config = NotesLibrary.Instance.Config;
                    DialogResult result = DialogResult.Yes;
                    if (config.General.ConfirmDelete)
                    {
                        result = MessageBox.Show("Do you want to delete this button?", AppName, 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    }

                    if (result != DialogResult.Yes)
                        return;

                    var unit = Units[id];
                    SaveStateForUndo();
                    Units.Remove(id);
                    RemoveButtonControl(btn);
                    if (unit.Font != null)
                        DisposeFontIfUnused(unit.Font);

                    configModified = true;
                    status = "Deleted successfully";
                    UpdateUndoRedoMenuState();
                }
            }
        }

        private void unitMenuDuplicate_Click(object sender, EventArgs e)
        {
            //Get clicked item
            var item = getContextMenuInfo(sender);

            if (item != null)
            {
                var btn = item.Button;
                var id = item.Id;

                if (Units.ContainsKey(id))
                {
                    //Load edit form
                    selectedUnitModified = false;
                    selectedUnit = Units[id];
                    using var editForm = new frmEdit();
                    editForm.ShowDialog();

                    if (selectedUnitModified)
                    {
                        SaveStateForUndo();
                        selectedUnitModified = false;
                        id = getNewId();

                        selectedUnit.CreatedDate = DateTime.Now;
                        selectedUnit.ModifiedDate = DateTime.Now;
                        Units.Add(id, selectedUnit);
                        addButton(id, selectedUnit);

                        configModified = true;
                        status = "New button added";
                        UpdateUndoRedoMenuState();
                    }
                }
            }
        }

        private void unitMenuCopyStyle_Click(object sender, EventArgs e)
        {
            var item = getContextMenuInfo(sender);

            if (item != null)
            {
                var btn = item.Button;
                var id = item.Id;

                if (Units.ContainsKey(id))
                {
                    copiedUnit = Units[id];
                    unitMenuPasteStyle.Enabled = true;
                    status = "Style copied";
                }
            }
        }

        private void unitMenuPasteStyle_Click(object sender, EventArgs e)
        {
            var item = getContextMenuInfo(sender);

            if (item != null)
            {
                var btn = item.Button;
                var id = item.Id;

                if (Units.ContainsKey(id) && copiedUnit.HasValue)
                {
                    SaveStateForUndo();
                    var unit = Units[id];
                    unit.BackgroundColor = copiedUnit.Value.BackgroundColor;
                    unit.TextColor = copiedUnit.Value.TextColor;
                    unit.Font = copiedUnit.Value.Font;
                    unit.ButtonType = copiedUnit.Value.ButtonType; // Copy button type too
                    Units[id] = unit;

                    // If button type changed, recreate the button
                    if (btn.GetType().Name != copiedUnit.Value.ButtonType)
                    {
                        // Store location and remove old button
                        Point location = btn.Location;
                        Rectangle oldBounds = btn.Bounds;
                        Control parent = btn.Parent;
                        if (parent != null)
                        {
                            parent.Controls.Remove(btn);
                        }
                        btn.Dispose();
                        
                        // Create new button with correct type
                        Button newBtn = CreateButtonByType(copiedUnit.Value.ButtonType);
                        newBtn.Tag = id;
                        newBtn.Text = unit.Title;
                        newBtn.BackColor = Color.FromArgb(copiedUnit.Value.BackgroundColor);
                        newBtn.ForeColor = Color.FromArgb(copiedUnit.Value.TextColor);
                        newBtn.Font = copiedUnit.Value.Font;
                        if (parent is GroupBox parentGroup)
                        {
                            var parentLocation = parentGroup.PointToClient(panelContainer.PointToScreen(location));
                            newBtn.Location = parentLocation;
                        }
                        else
                        {
                            newBtn.Location = location;
                        }
                        newBtn.AutoSize = true;
                        newBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                        newBtn.ContextMenuStrip = unitMenuStrip;
                        newBtn.Cursor = Cursors.Hand;
                        
                        // Add padding for custom buttons
                        if (!string.IsNullOrEmpty(copiedUnit.Value.ButtonType) && copiedUnit.Value.ButtonType != "DoubleClickButton")
                        {
                            newBtn.Padding = new Padding(12, 8, 12, 8);
                            newBtn.MinimumSize = new Size(80, 40);
                        }
                        
                        // Set up events
                        newBtn.Click += newButton_Click;
                        newBtn.DoubleClick += newButton_DoubleClick;
                        newBtn.MouseUp += newButton_MouseUp;
                        newBtn.MouseDown += newButton_MouseDown;
                        newBtn.MouseMove += newButton_MouseMove;
                        newBtn.PreviewKeyDown += newButton_PreviewKeyDown;
                        newBtn.KeyDown += newButton_KeyDown;
                        newBtn.KeyUp += newButton_KeyUp;
                        
                        if (parent is GroupBox parentGroupBox)
                        {
                            parentGroupBox.Controls.Add(newBtn);
                        }
                        else
                        {
                            AddButtonControl(newBtn);
                        }
                        panelContainer.Invalidate(oldBounds);
                        panelContainer.Refresh();
                    }
                    else
                    {
                        // Same type, just update properties
                        btn.BackColor = Color.FromArgb(copiedUnit.Value.BackgroundColor);
                        btn.ForeColor = Color.FromArgb(copiedUnit.Value.TextColor);
                        btn.Font = copiedUnit.Value.Font;
                        btn.Invalidate();
                    }

                    configModified = true;
                    status = "Style pasted successfully";
                    UpdateUndoRedoMenuState();
                }
            }
        }

        private void unitMenuCopyInLowercase_Click(object sender, EventArgs e)
        {
            var item = getContextMenuInfo(sender);
            if (item == null)
                return;

            UnitStruct unit;
            if (!Units.TryGetValue(item.Id, out unit))
                return;

            selectedUnit = unit;
            var text = unit.Content;
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    Clipboard.SetText(text.Trim().ToLowerInvariant());
                    status = "Copied to clipboard in lowercase";
                }
                catch (ExternalException ex)
                {
                    status = "Clipboard is busy";
                    Logger.Warning("Failed to copy lowercase text: " + ex.Message);
                }
            }
            else
            {
                status = "Note does not contain text content";
            }
        }

        private void unitMenuCopyInUppercase_Click(object sender, EventArgs e)
        {
            var item = getContextMenuInfo(sender);
            if (item == null)
                return;

            UnitStruct unit;
            if (!Units.TryGetValue(item.Id, out unit))
                return;

            selectedUnit = unit;
            var text = unit.Content;
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    Clipboard.SetText(text.Trim().ToUpperInvariant());
                    status = "Copied to clipboard in uppercase";
                }
                catch (ExternalException ex)
                {
                    status = "Clipboard is busy";
                    Logger.Warning("Failed to copy uppercase text: " + ex.Message);
                }
            }
            else
            {
                status = "Note does not contain text content";
            }
        }

        private void unitMenuAddToGroup_Click(object sender, EventArgs e)
        {
            Logger.LogMethodEntry("unitMenuAddToGroup_Click");
            
            // Get buttons to add to group
            List<Button> buttonsToAdd = new List<Button>();
            List<string> buttonIds = new List<string>();

            if (selectedButtons.Count > 0)
            {
                // Multiple buttons selected
                Logger.Debug($"Adding {selectedButtons.Count} selected buttons to group");
                foreach (var btn in selectedButtons)
                {
                    string btnId = btn.Tag as string;
                    if (!string.IsNullOrEmpty(btnId) && Units.ContainsKey(btnId))
                    {
                        buttonsToAdd.Add(btn);
                        buttonIds.Add(btnId);
                    }
                }
            }
            else
            {
                // Single button from context menu
                var item = getContextMenuInfo(sender);
                if (item == null)
                {
                    Logger.Warning("No context menu info found");
                    return;
                }

                var btn = item.Button;
                var id = item.Id;

                Logger.Debug($"Adding single button to group - Button ID: {id}, Text: {btn.Text}");

                if (!Units.ContainsKey(id))
                {
                    Logger.Error($"Unit ID not found in Units dictionary: {id}");
                    return;
                }

                buttonsToAdd.Add(btn);
                buttonIds.Add(id);
            }

            if (buttonsToAdd.Count == 0)
            {
                Logger.Warning("No valid buttons to add to group");
                return;
            }

            // Show dialog to select group
            if (Groups.Count == 0)
            {
                Logger.Info("No groups available, prompting user to create one");
                MessageBox.Show("No groups available. Please create a group first.", AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            Logger.Debug($"Available groups: {Groups.Count}, Buttons to add: {buttonsToAdd.Count}");

            // Create selection dialog
            Form selectGroupForm = new Form
            {
                Text = "Select Group",
                Width = 400,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                Font = new Font("Segoe UI", 9f)
            };

            Label label = new Label
            {
                Text = "Select a group:",
                Left = 20,
                Top = 20,
                Width = 350,
                Font = new Font("Segoe UI", 9f)
            };

            ComboBox comboBox = new ComboBox
            {
                Left = 20,
                Top = 45,
                Width = 345,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };

            List<string> groupIds = new List<string>();
            foreach (var group in Groups.Values)
            {
                comboBox.Items.Add($"{group.Title}");
                groupIds.Add(group.Id);
            }

            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;

            Button okButton = new Button
            {
                Text = "OK",
                Left = 195,
                Top = 80,
                Width = 80,
                Height = 28,
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new Button
            {
                Text = "Cancel",
                Left = 285,
                Top = 80,
                Width = 80,
                Height = 28,
                DialogResult = DialogResult.Cancel
            };

            selectGroupForm.Controls.Add(label);
            selectGroupForm.Controls.Add(comboBox);
            selectGroupForm.Controls.Add(okButton);
            selectGroupForm.Controls.Add(cancelButton);
            selectGroupForm.AcceptButton = okButton;
            selectGroupForm.CancelButton = cancelButton;

            if (selectGroupForm.ShowDialog() == DialogResult.OK && comboBox.SelectedIndex >= 0)
            {
                SaveStateForUndo();

                var selectedGroupId = groupIds[comboBox.SelectedIndex];
                var newGroupBox = GetOrCreateGroupBox(selectedGroupId);
                
                if (newGroupBox == null)
                {
                    Logger.Error($"Failed to get or create group box for ID: {selectedGroupId}");
                    return;
                }

                Logger.Info($"Selected group: {selectedGroupId}, Adding {buttonsToAdd.Count} button(s)");

                int addedCount = 0;
                const int minPadding = 20;
                const int titleHeight = 25;
                int stackOffset = 0;

                for (int i = 0; i < buttonsToAdd.Count; i++)
                {
                    var btn = buttonsToAdd[i];
                    var id = buttonIds[i];
                    var unit = Units[id];

                    // Calculate absolute position
                    Point absolutePos;
                    if (btn.Parent == panelContainer)
                    {
                        absolutePos = btn.Location;
                    }
                    else if (btn.Parent is GroupBox oldGroupBox)
                    {
                        absolutePos = new Point(
                            oldGroupBox.Location.X + btn.Location.X,
                            oldGroupBox.Location.Y + btn.Location.Y
                        );
                    }
                    else
                    {
                        absolutePos = btn.Location;
                    }

                    // Remove from current parent
                    if (btn.Parent is GroupBox oldGroup)
                    {
                        oldGroup.Controls.Remove(btn);
                    }
                    else
                    {
                        panelContainer.Controls.Remove(btn);
                    }

                    unit.GroupId = selectedGroupId;
                    
                    Point relativePos = new Point(
                        absolutePos.X - newGroupBox.Location.X,
                        absolutePos.Y - newGroupBox.Location.Y
                    );
                    
                    // Check if outside visible area
                    if (relativePos.X < minPadding || relativePos.Y < titleHeight ||
                        relativePos.X > newGroupBox.Width - btn.Width - minPadding ||
                        relativePos.Y > newGroupBox.Height - btn.Height - minPadding)
                    {
                        // Stack buttons vertically if adding multiple
                        relativePos = new Point(minPadding, titleHeight + 10 + stackOffset);
                        stackOffset += btn.Height + 10;
                        
                        absolutePos = new Point(
                            newGroupBox.Location.X + relativePos.X,
                            newGroupBox.Location.Y + relativePos.Y
                        );
                    }
                    
                    unit.X = absolutePos.X;
                    unit.Y = absolutePos.Y;
                    
                    btn.Visible = true;
                    btn.Enabled = true;
                    newGroupBox.Controls.Add(btn);
                    btn.Location = relativePos;
                    btn.BringToFront();
                    
                    Units[id] = unit;
                    addedCount++;
                }

                newGroupBox.PerformLayout();
                newGroupBox.Refresh();
                
                Logger.Info($"Successfully added {addedCount} button(s) to group");

                configModified = true;
                status = $"{addedCount} button(s) added to group";
                UpdateUndoRedoMenuState();
            }
            
            Logger.LogMethodExit("unitMenuAddToGroup_Click");
        }

        private void unitMenuRemoveFromGroup_Click(object sender, EventArgs e)
        {
            var item = getContextMenuInfo(sender);
            if (item == null) return;

            var btn = item.Button;
            var id = item.Id;

            if (!Units.ContainsKey(id)) return;

            var unit = Units[id];
            if (string.IsNullOrEmpty(unit.GroupId)) return;

            SaveStateForUndo();

            // Get absolute position
            Point absolutePos = btn.Parent.PointToScreen(btn.Location);
            absolutePos = panelContainer.PointToClient(absolutePos);
            int maxX = panelContainer.ClientSize.Width - btn.Width;
            int maxY = panelContainer.ClientSize.Height - btn.Height;
            if (maxX < 0) maxX = 0;
            if (maxY < 0) maxY = 0;
            absolutePos.X = Math.Max(0, Math.Min(absolutePos.X, maxX));
            absolutePos.Y = Math.Max(0, Math.Min(absolutePos.Y, maxY));

            // Remove from group
            if (btn.Parent is GroupBox groupBox)
            {
                groupBox.Controls.Remove(btn);
            }

            // Add to panel
            unit.GroupId = null;
            unit.X = absolutePos.X;
            unit.Y = absolutePos.Y;
            btn.Location = absolutePos;
            btn.Parent = panelContainer;
            panelContainer.Controls.Add(btn);

            Units[id] = unit;

            configModified = true;
            status = "Button removed from group";
            UpdateUndoRedoMenuState();
        }

        private void groupMenuEdit_Click(object sender, EventArgs e)
        {
            ContextMenuStrip contextMenu = (sender as ToolStripMenuItem)?.Owner as ContextMenuStrip;
            if (contextMenu?.SourceControl is GroupBox groupBox)
            {
                string groupId = groupBox.Tag as string;
                if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                    return;

                frmAddGroup.selectedGroup = Groups[groupId];
                frmAddGroup.selectedGroupModified = false;

                using var editGroupForm = new frmAddGroup { IsEditMode = true };
                editGroupForm.ShowDialog();

                if (frmAddGroup.selectedGroupModified)
                {
                    SaveStateForUndo();

                    var updatedGroup = frmAddGroup.selectedGroup;
                    var clampedLocation = ClampGroupBoxLocation(groupBox, new Point(updatedGroup.X, updatedGroup.Y));
                    if (clampedLocation.X != updatedGroup.X || clampedLocation.Y != updatedGroup.Y)
                    {
                        updatedGroup.X = clampedLocation.X;
                        updatedGroup.Y = clampedLocation.Y;
                    }
                    Groups[groupId] = updatedGroup;

                    groupBox.Text = updatedGroup.Title;
                    groupBox.Location = clampedLocation;
                    groupBox.Size = new Size(updatedGroup.Width, updatedGroup.Height);
                    ApplyGroupBoxColors(groupBox, updatedGroup);
                    groupBox.Invalidate(); // Redraw to apply colors

                    configModified = true;
                    status = "Group updated";
                    UpdateUndoRedoMenuState();
                }
            }
        }

        private void groupMenuDelete_Click(object sender, EventArgs e)
        {
            ContextMenuStrip contextMenu = (sender as ToolStripMenuItem)?.Owner as ContextMenuStrip;
            if (contextMenu?.SourceControl is GroupBox groupBox)
            {
                string groupId = groupBox.Tag as string;
                if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                    return;

                DialogResult result = MessageBox.Show(
                    "Do you want to delete this group? Buttons inside will be moved to the main panel.",
                    AppName,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.OK)
                {
                    SaveStateForUndo();

                    // Move all buttons to main panel
                    var buttonsToMove = groupBox.Controls.OfType<Button>().ToList();
                    foreach (var btn in buttonsToMove)
                    {
                        string btnId = btn.Tag as string;
                        if (!string.IsNullOrEmpty(btnId) && Units.ContainsKey(btnId))
                        {
                            // Get absolute position
                            Point absolutePos = groupBox.PointToScreen(btn.Location);
                            absolutePos = panelContainer.PointToClient(absolutePos);
                            int maxX = panelContainer.ClientSize.Width - btn.Width;
                            int maxY = panelContainer.ClientSize.Height - btn.Height;
                            if (maxX < 0) maxX = 0;
                            if (maxY < 0) maxY = 0;
                            absolutePos.X = Math.Max(0, Math.Min(absolutePos.X, maxX));
                            absolutePos.Y = Math.Max(0, Math.Min(absolutePos.Y, maxY));

                            var unit = Units[btnId];
                            unit.GroupId = null;
                            unit.X = absolutePos.X;
                            unit.Y = absolutePos.Y;
                            Units[btnId] = unit;

                            groupBox.Controls.Remove(btn);
                            btn.Location = absolutePos;
                            btn.Parent = panelContainer;
                            panelContainer.Controls.Add(btn);
                        }
                    }

                    // Remove group
                    Groups.Remove(groupId);
                    panelContainer.Controls.Remove(groupBox);
                    resizingGroups.Remove(groupBox);
                    groupBox.Dispose();

                    configModified = true;
                    status = "Group deleted";
                    UpdateUndoRedoMenuState();
                }
            }
        }

        private void groupMenuAutoResize_Click(object sender, EventArgs e)
        {
            ContextMenuStrip contextMenu = (sender as ToolStripMenuItem)?.Owner as ContextMenuStrip;
            if (contextMenu?.SourceControl is GroupBox groupBox)
            {
                string groupId = groupBox.Tag as string;
                if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                    return;

                AutoResizeGroup(groupBox);

                configModified = true;
                status = "Group resized";
            }
        }

        private void groupMenuAlign_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            ContextMenuStrip contextMenu = menuItem?.GetCurrentParent() as ContextMenuStrip 
                ?? (menuItem?.OwnerItem as ToolStripMenuItem)?.GetCurrentParent() as ContextMenuStrip;
            if (contextMenu?.SourceControl is GroupBox groupBox)
            {
                string groupId = groupBox.Tag as string;
                if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                    return;

                var buttons = groupBox.Controls.OfType<Button>().ToList();
                if (buttons.Count == 0)
                    return;

                string alignType = menuItem.Name.Replace("groupMenuAlign", "");
                const int spacing = 10;

                switch (alignType)
                {
                    case "Left":
                        int leftX = buttons.Min(b => b.Left);
                        var sortedByTop = buttons.OrderBy(b => b.Top).ToList();
                        int currentY = sortedByTop[0].Top;
                        foreach (var btn in sortedByTop)
                        {
                            btn.Left = leftX;
                            btn.Top = currentY;
                            currentY += btn.Height + spacing;
                        }
                        break;

                    case "Center":
                        int centerX = buttons.Sum(b => b.Left + b.Width / 2) / buttons.Count;
                        var sortedByCenterTop = buttons.OrderBy(b => b.Top).ToList();
                        int currentCenterY = sortedByCenterTop[0].Top;
                        foreach (var btn in sortedByCenterTop)
                        {
                            btn.Left = centerX - btn.Width / 2;
                            btn.Top = currentCenterY;
                            currentCenterY += btn.Height + spacing;
                        }
                        break;

                    case "Right":
                        int rightX = buttons.Max(b => b.Right);
                        var sortedByRightTop = buttons.OrderBy(b => b.Top).ToList();
                        int currentRightY = sortedByRightTop[0].Top;
                        foreach (var btn in sortedByRightTop)
                        {
                            btn.Left = rightX - btn.Width;
                            btn.Top = currentRightY;
                            currentRightY += btn.Height + spacing;
                        }
                        break;

                    case "Top":
                        int topY = buttons.Min(b => b.Top);
                        var sortedByLeft = buttons.OrderBy(b => b.Left).ToList();
                        int currentX = sortedByLeft[0].Left;
                        foreach (var btn in sortedByLeft)
                        {
                            btn.Top = topY;
                            btn.Left = currentX;
                            currentX += btn.Width + spacing;
                        }
                        break;

                    case "Middle":
                        int middleY = buttons.Sum(b => b.Top + b.Height / 2) / buttons.Count;
                        var sortedByMiddleLeft = buttons.OrderBy(b => b.Left).ToList();
                        int currentMiddleX = sortedByMiddleLeft[0].Left;
                        foreach (var btn in sortedByMiddleLeft)
                        {
                            btn.Top = middleY - btn.Height / 2;
                            btn.Left = currentMiddleX;
                            currentMiddleX += btn.Width + spacing;
                        }
                        break;

                    case "Bottom":
                        int bottomY = buttons.Max(b => b.Bottom);
                        var sortedByBottomLeft = buttons.OrderBy(b => b.Left).ToList();
                        int currentBottomX = sortedByBottomLeft[0].Left;
                        foreach (var btn in sortedByBottomLeft)
                        {
                            btn.Top = bottomY - btn.Height;
                            btn.Left = currentBottomX;
                            currentBottomX += btn.Width + spacing;
                        }
                        break;
                }

                // Update button positions in the library
                foreach (var btn in buttons)
                {
                    string btnId = btn.Tag as string;
                    if (!string.IsNullOrEmpty(btnId) && Units.ContainsKey(btnId))
                    {
                        var unit = Units[btnId];
                        unit.X = btn.Left;
                        unit.Y = btn.Top;
                        Units[btnId] = unit;
                    }
                }

                configModified = true;
                status = $"Buttons aligned: {alignType}";
            }
        }

        private void AutoResizeGroup(GroupBox groupBox)
        {
            if (groupBox == null || groupBox.Controls.Count == 0)
                return;

            string groupId = groupBox.Tag as string;
            if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                return;

            // Calculate bounds of all buttons
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (Button btn in groupBox.Controls.OfType<Button>())
            {
                minX = Math.Min(minX, btn.Left);
                minY = Math.Min(minY, btn.Top);
                maxX = Math.Max(maxX, btn.Right);
                maxY = Math.Max(maxY, btn.Bottom);
            }

            if (minX == int.MaxValue) // No buttons found
                return;

            // Add padding
            const int padding = 20;
            const int titleHeight = 30;

            int newWidth = maxX - minX + padding * 2;
            int newHeight = maxY - minY + padding + titleHeight;

            // Ensure minimum size
            newWidth = Math.Max(newWidth, 150);
            newHeight = Math.Max(newHeight, 100);

            // Adjust button positions if needed
            if (minX < padding || minY < titleHeight)
            {
                int offsetX = Math.Max(0, padding - minX);
                int offsetY = Math.Max(0, titleHeight - minY);

                foreach (Button btn in groupBox.Controls.OfType<Button>())
                {
                    btn.Left += offsetX;
                    btn.Top += offsetY;

                    // Update unit data
                    string btnId = btn.Tag as string;
                    if (!string.IsNullOrEmpty(btnId) && Units.ContainsKey(btnId))
                    {
                        var unit = Units[btnId];
                        Point absolutePos = groupBox.PointToScreen(btn.Location);
                        absolutePos = panelContainer.PointToClient(absolutePos);
                        unit.X = absolutePos.X;
                        unit.Y = absolutePos.Y;
                        Units[btnId] = unit;
                    }
                }

                newWidth += offsetX;
                newHeight += offsetY;
            }

            groupBox.Size = new Size(newWidth, newHeight);

            var group = Groups[groupId];
            group.Width = newWidth;
            group.Height = newHeight;
            Groups[groupId] = group;
        }

        private void groupMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Get the group box that triggered the context menu
            if (groupMenuStrip.SourceControl is GroupBox groupBox)
            {
                string groupId = groupBox.Tag as string;
                if (!string.IsNullOrEmpty(groupId) && Groups.ContainsKey(groupId))
                {
                    var group = Groups[groupId];
                    string groupBoxType = group.GroupBoxType;

                    // Uncheck all style menu items first
                    groupMenuStyleDefault.Checked = false;
                    groupMenuStyleGradientGlass.Checked = false;
                    groupMenuStyleNeonGlow.Checked = false;
                    groupMenuStyleEmbossed.Checked = false;
                    groupMenuStyleRetro.Checked = false;
                    groupMenuStyleCard.Checked = false;
                    groupMenuStyleMinimal.Checked = false;
                    groupMenuStyleDashed.Checked = false;
                    groupMenuStyleDoubleBorder.Checked = false;
                    groupMenuStyleShadowPanel.Checked = false;
                    groupMenuStyleRoundedNeon.Checked = false;
                    groupMenuStyleHolographic.Checked = false;
                    groupMenuStyleVintagePaper.Checked = false;
                    groupMenuStyleLiquidMetal.Checked = false;
                    groupMenuStyleCosmic.Checked = false;
                    groupMenuStyleRainbow.Checked = false;
                    groupMenuStyleAuroraBorealis.Checked = false;
                    groupMenuStyleCyberCircuit.Checked = false;
                    groupMenuStyleFireLava.Checked = false;
                    groupMenuStyleMatrixRain.Checked = false;
                    groupMenuStyleCrystalIce.Checked = false;
                    groupMenuStylePlasmaEnergy.Checked = false;
                    groupMenuStyleOceanWave.Checked = false;
                    groupMenuStyleElectricStorm.Checked = false;
                    groupMenuStyleStarfieldWarp.Checked = false;
                    groupMenuStyleHeartbeatPulse.Checked = false;
                    groupMenuStyleSnowfall.Checked = false;
                    groupMenuStyleCloudDrift.Checked = false;
                    groupMenuStyleSparkleShine.Checked = false;
                    groupMenuStyleRippleWater.Checked = false;
                    groupMenuStyleBubblesFloat.Checked = false;
                    groupMenuStyleConfettiParty.Checked = false;
                    groupMenuStyleSunburstRays.Checked = false;
                    groupMenuStyleCherryBlossom.Checked = false;
                    groupMenuStyleFloatingHearts.Checked = false;

                    // Check the current style
                    switch (groupBoxType)
                    {
                        case "GradientGlassGroupBox":
                            groupMenuStyleGradientGlass.Checked = true;
                            break;
                        case "NeonGlowGroupBox":
                            groupMenuStyleNeonGlow.Checked = true;
                            break;
                        case "EmbossedGroupBox":
                            groupMenuStyleEmbossed.Checked = true;
                            break;
                        case "RetroGroupBox":
                            groupMenuStyleRetro.Checked = true;
                            break;
                        case "CardGroupBox":
                            groupMenuStyleCard.Checked = true;
                            break;
                        case "MinimalGroupBox":
                            groupMenuStyleMinimal.Checked = true;
                            break;
                        case "DashedGroupBox":
                            groupMenuStyleDashed.Checked = true;
                            break;
                        case "DoubleBorderGroupBox":
                            groupMenuStyleDoubleBorder.Checked = true;
                            break;
                        case "ShadowPanelGroupBox":
                            groupMenuStyleShadowPanel.Checked = true;
                            break;
                        case "RoundedNeonGroupBox":
                            groupMenuStyleRoundedNeon.Checked = true;
                            break;
                        case "HolographicGroupBox":
                            groupMenuStyleHolographic.Checked = true;
                            break;
                        case "VintagePaperGroupBox":
                            groupMenuStyleVintagePaper.Checked = true;
                            break;
                        case "LiquidMetalGroupBox":
                            groupMenuStyleLiquidMetal.Checked = true;
                            break;
                        case "CosmicGroupBox":
                            groupMenuStyleCosmic.Checked = true;
                            break;
                        case "RainbowSpectrumGroupBox":
                            groupMenuStyleRainbow.Checked = true;
                            break;
                        case "AuroraBorealisGroupBox":
                            groupMenuStyleAuroraBorealis.Checked = true;
                            break;
                        case "CyberCircuitGroupBox":
                            groupMenuStyleCyberCircuit.Checked = true;
                            break;
                        case "FireLavaGroupBox":
                            groupMenuStyleFireLava.Checked = true;
                            break;
                        case "MatrixRainGroupBox":
                            groupMenuStyleMatrixRain.Checked = true;
                            break;
                        case "CrystalIceGroupBox":
                            groupMenuStyleCrystalIce.Checked = true;
                            break;
                        case "PlasmaEnergyGroupBox":
                            groupMenuStylePlasmaEnergy.Checked = true;
                            break;
                        case "OceanWaveGroupBox":
                            groupMenuStyleOceanWave.Checked = true;
                            break;
                        case "ElectricStormGroupBox":
                            groupMenuStyleElectricStorm.Checked = true;
                            break;
                        case "StarfieldWarpGroupBox":
                            groupMenuStyleStarfieldWarp.Checked = true;
                            break;
                        case "HeartbeatPulseGroupBox":
                            groupMenuStyleHeartbeatPulse.Checked = true;
                            break;
                        case "SnowfallGroupBox":
                            groupMenuStyleSnowfall.Checked = true;
                            break;
                        case "CloudDriftGroupBox":
                            groupMenuStyleCloudDrift.Checked = true;
                            break;
                        case "SparkleShineGroupBox":
                            groupMenuStyleSparkleShine.Checked = true;
                            break;
                        case "RippleWaterGroupBox":
                            groupMenuStyleRippleWater.Checked = true;
                            break;
                        case "BubblesFloatGroupBox":
                            groupMenuStyleBubblesFloat.Checked = true;
                            break;
                        case "ConfettiPartyGroupBox":
                            groupMenuStyleConfettiParty.Checked = true;
                            break;
                        case "SunburstRaysGroupBox":
                            groupMenuStyleSunburstRays.Checked = true;
                            break;
                        case "CherryBlossomGroupBox":
                            groupMenuStyleCherryBlossom.Checked = true;
                            break;
                        case "FloatingHeartsGroupBox":
                            groupMenuStyleFloatingHearts.Checked = true;
                            break;
                        default: // ResizableGroupBox
                            groupMenuStyleDefault.Checked = true;
                            break;
                    }
                }
            }
        }

        private void unitMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Get the button that triggered the context menu
            if (unitMenuStrip.SourceControl is Button btn)
            {
                string id = btn.Tag as string;
                if (!string.IsNullOrEmpty(id) && Units.ContainsKey(id))
                {
                    var unit = Units[id];
                    // Enable "Remove from Group" only if button is in a group
                    unitMenuRemoveFromGroup.Enabled = !string.IsNullOrEmpty(unit.GroupId);

                    // Uncheck all style menu items
                    unitMenuStyleClassic.Checked = false;
                    unitMenuStylePastel.Checked = false;
                    unitMenuStyleDark.Checked = false;
                    unitMenuStyleNeon.Checked = false;
                    unitMenuStyleEarth.Checked = false;
                    unitMenuStyleOcean.Checked = false;
                    unitMenuStyleSunset.Checked = false;
                    unitMenuStyleMonochrome.Checked = false;
                    unitMenuStyleVibrant.Checked = false;
                    unitMenuStyleGradient.Checked = false;
                    unitMenuStyleGloss.Checked = false;
                    unitMenuStyleEmbossed.Checked = false;
                    unitMenuStyleRaised.Checked = false;
                    unitMenuStyleInset.Checked = false;
                    unitMenuStyleRetro.Checked = false;
                    unitMenuStyleCyber.Checked = false;
                    unitMenuStyleGlass.Checked = false;
                    unitMenuStyleNeonGlow.Checked = false;
                    unitMenuStyleGolden.Checked = false;
                    unitMenuStyleMinimal.Checked = false;
                    unitMenuStyleBold.Checked = false;
                    unitMenuStyleElegant.Checked = false;
                    unitMenuStylePlayful.Checked = false;
                    unitMenuStyleProfessional.Checked = false;

                    // Check the current button type
                    string buttonType = btn.GetType().Name;
                    
                    // Map button type to menu item
                    switch (buttonType)
                    {
                        case "GradientButton":
                            // Could be Classic, Gradient, or Vibrant - check by color
                            if (btn.BackColor == Color.LightSteelBlue)
                                unitMenuStyleClassic.Checked = true;
                            else if (btn.BackColor == Color.LightGreen)
                                unitMenuStyleEarth.Checked = true;
                            else if (btn.BackColor == Color.Orange)
                                unitMenuStyleSunset.Checked = true;
                            else if (btn.BackColor == Color.DeepPink)
                                unitMenuStyleVibrant.Checked = true;
                            else
                                unitMenuStyleGradient.Checked = true;
                            break;
                        case "Button":
                        case "DoubleClickButton":
                            // Check by color for standard buttons
                            if (btn.BackColor == Color.LightPink)
                                unitMenuStylePastel.Checked = true;
                            else if (btn.BackColor == Color.FromArgb(45, 45, 48))
                                unitMenuStyleDark.Checked = true;
                            else if (btn.BackColor == Color.Black)
                                unitMenuStyleNeon.Checked = true;
                            else if (btn.BackColor == Color.LightBlue)
                                unitMenuStyleOcean.Checked = true;
                            else if (btn.BackColor == Color.White)
                                unitMenuStyleMonochrome.Checked = true;
                            else if (btn.BackColor == Color.LightGray)
                                unitMenuStyleMinimal.Checked = true;
                            else if (btn.Font?.Bold == true)
                                unitMenuStyleBold.Checked = true;
                            else
                                unitMenuStyleClassic.Checked = true;
                            break;
                        case "GlossButton":
                            unitMenuStyleGloss.Checked = true;
                            break;
                        case "NeumorphismButton":
                            unitMenuStyleEmbossed.Checked = true;
                            break;
                        case "Retro3DButton":
                            if (btn.BackColor == Color.Crimson)
                                unitMenuStyleRetro.Checked = true;
                            else
                                unitMenuStyleRaised.Checked = true;
                            break;
                        case "NeonGlowButton":
                            if (btn.BackColor == Color.Purple)
                                unitMenuStyleCyber.Checked = true;
                            else if (btn.BackColor == Color.Cyan)
                                unitMenuStyleNeonGlow.Checked = true;
                            else
                                unitMenuStyleInset.Checked = true;
                            break;
                        case "GlassMorphismButton":
                            unitMenuStyleGlass.Checked = true;
                            break;
                        case "OutlineButton":
                            unitMenuStyleElegant.Checked = true;
                            break;
                        case "SkeuomorphicButton":
                            unitMenuStyleGolden.Checked = true;
                            break;
                        case "PremiumCardButton":
                            unitMenuStylePlayful.Checked = true;
                            break;
                        case "MaterialButton":
                            unitMenuStyleProfessional.Checked = true;
                            break;
                        case "PillButton":
                            unitMenuStyleElegant.Checked = true;
                            break;
                    }
                }
            }
        }

        private void groupMenuStyle_Click(object sender, EventArgs e)
        {
            Logger.Debug(">> groupMenuStyle_Click triggered");
            
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem == null)
            {
                Logger.Warning("menuItem is null");
                return;
            }
            
            Logger.Debug($"Menu item clicked: {menuItem.Name} - {menuItem.Text}");

            // Get the root ContextMenuStrip (not the parent ToolStripDropDownMenu)
            ContextMenuStrip contextMenu = null;
            ToolStripItem current = menuItem;
            while (current != null)
            {
                if (current.Owner is ContextMenuStrip cms)
                {
                    contextMenu = cms;
                    break;
                }
                current = current.OwnerItem;
            }
            
            Logger.Debug($"ContextMenu: {contextMenu?.Name}, SourceControl: {contextMenu?.SourceControl?.GetType().Name}");
            
            if (contextMenu?.SourceControl is GroupBox oldGroupBox)
            {
                string groupId = oldGroupBox.Tag as string;
                Logger.Debug($"GroupBox found - GroupId: {groupId}, Title: {oldGroupBox.Text}");
                
                if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                {
                    Logger.Warning($"Invalid groupId or not found in Groups dictionary");
                    return;
                }

                SaveStateForUndo();

                var group = Groups[groupId];
                Logger.Debug($"Current group type: {group.GroupBoxType}");
                
                // Determine group box type from menu item name
                string groupBoxType = menuItem.Name switch
                {
                    "groupMenuStyleGradientGlass" => "GradientGlassGroupBox",
                    "groupMenuStyleNeonGlow" => "NeonGlowGroupBox",
                    "groupMenuStyleEmbossed" => "EmbossedGroupBox",
                    "groupMenuStyleRetro" => "RetroGroupBox",
                    "groupMenuStyleCard" => "CardGroupBox",
                    "groupMenuStyleMinimal" => "MinimalGroupBox",
                    "groupMenuStyleDashed" => "DashedGroupBox",
                    "groupMenuStyleDoubleBorder" => "DoubleBorderGroupBox",
                    "groupMenuStyleShadowPanel" => "ShadowPanelGroupBox",
                    "groupMenuStyleRoundedNeon" => "RoundedNeonGroupBox",
                    "groupMenuStyleHolographic" => "HolographicGroupBox",
                    "groupMenuStyleVintagePaper" => "VintagePaperGroupBox",
                    "groupMenuStyleLiquidMetal" => "LiquidMetalGroupBox",
                    "groupMenuStyleCosmic" => "CosmicGroupBox",
                    "groupMenuStyleRainbow" => "RainbowSpectrumGroupBox",
                    "groupMenuStyleAuroraBorealis" => "AuroraBorealisGroupBox",
                    "groupMenuStyleCyberCircuit" => "CyberCircuitGroupBox",
                    "groupMenuStyleFireLava" => "FireLavaGroupBox",
                    "groupMenuStyleMatrixRain" => "MatrixRainGroupBox",
                    "groupMenuStyleCrystalIce" => "CrystalIceGroupBox",
                    "groupMenuStylePlasmaEnergy" => "PlasmaEnergyGroupBox",
                    "groupMenuStyleOceanWave" => "OceanWaveGroupBox",
                    "groupMenuStyleElectricStorm" => "ElectricStormGroupBox",
                    "groupMenuStyleStarfieldWarp" => "StarfieldWarpGroupBox",
                    "groupMenuStyleHeartbeatPulse" => "HeartbeatPulseGroupBox",
                    "groupMenuStyleSnowfall" => "SnowfallGroupBox",
                    "groupMenuStyleCloudDrift" => "CloudDriftGroupBox",
                    "groupMenuStyleSparkleShine" => "SparkleShineGroupBox",
                    "groupMenuStyleRippleWater" => "RippleWaterGroupBox",
                    "groupMenuStyleBubblesFloat" => "BubblesFloatGroupBox",
                    "groupMenuStyleConfettiParty" => "ConfettiPartyGroupBox",
                    "groupMenuStyleSunburstRays" => "SunburstRaysGroupBox",
                    "groupMenuStyleCherryBlossom" => "CherryBlossomGroupBox",
                    "groupMenuStyleFloatingHearts" => "FloatingHeartsGroupBox",
                    _ => "ResizableGroupBox"
                };

                Logger.Info($"Changing group box type from '{group.GroupBoxType}' to '{groupBoxType}'");
                
                group.GroupBoxType = groupBoxType;
                Groups[groupId] = group;

                // Get all buttons in the old group
                var buttons = oldGroupBox.Controls.OfType<Button>().ToList();
                var buttonData = new List<(Button btn, Point location)>();
                
                foreach (var btn in buttons)
                {
                    buttonData.Add((btn, btn.Location));
                }

                // Get properties from old group
                Point location = oldGroupBox.Location;
                Size size = oldGroupBox.Size;
                string title = oldGroupBox.Text;

                // Remove buttons from old group before disposing
                foreach (var btn in buttons)
                {
                    oldGroupBox.Controls.Remove(btn);
                }

                // Remove old group
                Logger.Debug($"Removing old GroupBox from panel");
                panelContainer.Controls.Remove(oldGroupBox);
                resizingGroups.Remove(oldGroupBox);
                oldGroupBox.Dispose();

                // Create new styled group directly
                Logger.Debug($"Creating new GroupBox with type: {groupBoxType}");
                var newGroupBox = CreateGroupBoxByType(groupBoxType);
                Logger.Debug($"New GroupBox created - Actual type: {newGroupBox.GetType().Name}");
                
                newGroupBox.Tag = groupId;
                newGroupBox.Text = title;
                newGroupBox.Location = location;
                newGroupBox.Size = size;
                ApplyGroupBoxColors(newGroupBox, group);

                newGroupBox.MouseDown += GroupBox_MouseDown;
                newGroupBox.MouseMove += GroupBox_MouseMove;
                newGroupBox.MouseUp += GroupBox_MouseUp;
                newGroupBox.ContextMenuStrip = groupMenuStrip;
                newGroupBox.SizeChanged += GroupBox_SizeChanged;
                ApplyGroupBoxBehavior(newGroupBox);

                panelContainer.Controls.Add(newGroupBox);
                
                // Re-add buttons
                foreach (var (btn, loc) in buttonData)
                {
                    newGroupBox.Controls.Add(btn);
                    btn.Location = loc;
                    btn.BringToFront();
                }

                newGroupBox.Refresh();
                
                Logger.Info($"<< Group style successfully changed to {menuItem.Text} - Type: {newGroupBox.GetType().Name}");

                configModified = true;
                status = $"Group style changed to {menuItem.Text}";
                UpdateUndoRedoMenuState();
            }
            else
            {
                Logger.Warning("ContextMenu or SourceControl is not a GroupBox");
            }
        }

        private void groupMenuStyleRandom_Click(object sender, EventArgs e)
        {
            Logger.Debug(">> groupMenuStyleRandom_Click triggered");
            
            // Get the root ContextMenuStrip
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem == null)
            {
                Logger.Warning("menuItem is null");
                return;
            }

            ContextMenuStrip contextMenu = null;
            ToolStripItem current = menuItem;
            while (current != null)
            {
                if (current.Owner is ContextMenuStrip cms)
                {
                    contextMenu = cms;
                    break;
                }
                current = current.OwnerItem;
            }
            
            if (contextMenu?.SourceControl is GroupBox oldGroupBox)
            {
                string groupId = oldGroupBox.Tag as string;
                Logger.Debug($"GroupBox found - GroupId: {groupId}, Title: {oldGroupBox.Text}");
                
                if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                {
                    Logger.Warning($"Invalid groupId or not found in Groups dictionary");
                    return;
                }

                SaveStateForUndo();

                var group = Groups[groupId];
                Logger.Debug($"Current group type: {group.GroupBoxType}");
                
                // Array of all available styles
                string[] styles = new[]
                {
                    "ResizableGroupBox",
                    "GradientGlassGroupBox",
                    "NeonGlowGroupBox",
                    "EmbossedGroupBox",
                    "RetroGroupBox",
                    "CardGroupBox",
                    "MinimalGroupBox",
                    "DashedGroupBox",
                    "DoubleBorderGroupBox",
                    "ShadowPanelGroupBox",
                    "RoundedNeonGroupBox",
                    "HolographicGroupBox",
                    "VintagePaperGroupBox",
                    "LiquidMetalGroupBox",
                    "CosmicGroupBox",
                    "RainbowSpectrumGroupBox",
                    "AuroraBorealisGroupBox",
                    "CyberCircuitGroupBox",
                    "FireLavaGroupBox",
                    "MatrixRainGroupBox",
                    "CrystalIceGroupBox",
                    "PlasmaEnergyGroupBox",
                    "OceanWaveGroupBox",
                    "ElectricStormGroupBox",
                    "StarfieldWarpGroupBox",
                    "HeartbeatPulseGroupBox",
                    "SnowfallGroupBox",
                    "CloudDriftGroupBox",
                    "SparkleShineGroupBox",
                    "RippleWaterGroupBox",
                    "BubblesFloatGroupBox",
                    "ConfettiPartyGroupBox",
                    "SunburstRaysGroupBox",
                    "CherryBlossomGroupBox",
                    "FloatingHeartsGroupBox"
                };

                // Pick a random style
                string groupBoxType = styles[random.Next(styles.Length)];

                Logger.Info($"Randomly changing group box type from '{group.GroupBoxType}' to '{groupBoxType}'");
                
                group.GroupBoxType = groupBoxType;
                Groups[groupId] = group;

                // Get all buttons in the old group
                var buttons = oldGroupBox.Controls.OfType<Button>().ToList();
                var buttonData = new List<(Button btn, Point location)>();
                
                foreach (var btn in buttons)
                {
                    buttonData.Add((btn, btn.Location));
                }

                // Get properties from old group
                Point location = oldGroupBox.Location;
                Size size = oldGroupBox.Size;
                string title = oldGroupBox.Text;

                // Remove buttons from old group before disposing
                foreach (var btn in buttons)
                {
                    oldGroupBox.Controls.Remove(btn);
                }

                // Remove old group
                Logger.Debug($"Removing old GroupBox from panel");
                panelContainer.Controls.Remove(oldGroupBox);
                resizingGroups.Remove(oldGroupBox);
                oldGroupBox.Dispose();

                // Create new styled group directly
                Logger.Debug($"Creating new GroupBox with type: {groupBoxType}");
                var newGroupBox = CreateGroupBoxByType(groupBoxType);
                Logger.Debug($"New GroupBox created - Actual type: {newGroupBox.GetType().Name}");
                
                newGroupBox.Tag = groupId;
                newGroupBox.Text = title;
                newGroupBox.Location = location;
                newGroupBox.Size = size;
                ApplyGroupBoxColors(newGroupBox, group);

                newGroupBox.MouseDown += GroupBox_MouseDown;
                newGroupBox.MouseMove += GroupBox_MouseMove;
                newGroupBox.MouseUp += GroupBox_MouseUp;
                newGroupBox.ContextMenuStrip = groupMenuStrip;
                newGroupBox.SizeChanged += GroupBox_SizeChanged;
                ApplyGroupBoxBehavior(newGroupBox);

                panelContainer.Controls.Add(newGroupBox);
                
                // Re-add buttons
                foreach (var (btn, loc) in buttonData)
                {
                    newGroupBox.Controls.Add(btn);
                    btn.Location = loc;
                    btn.BringToFront();
                }

                newGroupBox.Refresh();
                
                Logger.Info($"<< Group style successfully changed to Random ({groupBoxType}) - Type: {newGroupBox.GetType().Name}");

                configModified = true;
                status = $"Group style changed to Random ({groupBoxType.Replace("GroupBox", "")})";
                UpdateUndoRedoMenuState();
            }
            else
            {
                Logger.Warning("ContextMenu or SourceControl is not a GroupBox");
            }
        }






        private void tmrStatus_Tick(object sender, EventArgs e)
        {
            tmrStatus.Stop();
            if (!string.IsNullOrWhiteSpace(statusLabel.Text))
                statusLabel.Text = string.Format("Ready - {0} notes", Units.Count);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var settingsForm = new frmSettings())
                {
                    if (settingsForm.ShowDialog() == DialogResult.OK)
                    {
                        // Reload configuration after settings are saved
                        LoadConfiguration();
                        
                        // Refresh all note buttons with new theme
                        RefreshAllButtons();
                        AnimationHelper.ApplyToExisting(panelContainer);
                        
                        status = "Settings updated successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening settings: " + ex.Message, AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // View Menu - Arrange submenu handlers
        
        private void menuViewArrangeGrid_Click(object sender, EventArgs e)
        {
            ArrangeButtonsInGrid();
        }

        private void menuViewArrangeByDate_Click(object sender, EventArgs e)
        {
            ArrangeButtonsByDate();
        }

        private void menuViewArrangeByColor_Click(object sender, EventArgs e)
        {
            ArrangeButtonsByColor();
        }

        private void menuViewArrangeCompact_Click(object sender, EventArgs e)
        {
            ArrangeButtonsCompact();
        }

        private void menuViewFixOverlaps_Click(object sender, EventArgs e)
        {
            FixOverlappingButtons();
        }

        // Arrangement algorithms

        private void ArrangeButtonsInGrid()
        {
            if (Units.Count == 0)
            {
                status = "No notes to arrange";
                return;
            }

            SaveStateForUndo();

            const int padding = 10;
            const int startX = 10;
            const int startY = 10;
            const int maxColumns = 5;
            
            int currentX = startX;
            int currentY = startY;
            int column = 0;
            int maxHeightInRow = 0;

            var buttons = GetAllButtonsInPanel().Cast<Button>().ToList();
            
            foreach (var btn in buttons)
            {
                string id = (string)btn.Tag;
                if (Units.ContainsKey(id))
                {
                    var unit = Units[id];
                    unit.X = currentX;
                    unit.Y = currentY;
                    Units[id] = unit;
                    
                    btn.Location = new Point(currentX, currentY);
                    
                    // Track max height in this row
                    maxHeightInRow = Math.Max(maxHeightInRow, btn.Height);
                    
                    column++;
                    if (column >= maxColumns)
                    {
                        // Move to next row
                        column = 0;
                        currentX = startX;
                        currentY += maxHeightInRow + padding;
                        maxHeightInRow = 0;
                    }
                    else
                    {
                        // Move to next column
                        currentX += btn.Width + padding;
                    }
                }
            }

            configModified = true;
            status = $"Arranged {buttons.Count} notes in grid layout";
            UpdateUndoRedoMenuState();
        }

        private void ArrangeButtonsByDate()
        {
            if (Units.Count == 0)
            {
                status = "No notes to arrange";
                return;
            }

            SaveStateForUndo();

            const int padding = 10;
            const int startX = 10;
            const int startY = 10;
            
            // Sort buttons by creation date (newest first)
            var sortedUnits = Units.OrderByDescending(kvp => kvp.Value.CreatedDate).ToList();
            
            int currentX = startX;
            int currentY = startY;
            int maxHeightInRow = 0;
            int itemsInRow = 0;
            const int maxColumns = 4;

            foreach (var kvp in sortedUnits)
            {
                var id = kvp.Key;
                var unit = kvp.Value;
                
                // Find the button for this unit
                var btn = FindButtonById(id);
                    
                if (btn != null)
                {
                    unit.X = currentX;
                    unit.Y = currentY;
                    Units[id] = unit;
                    
                    btn.Location = new Point(currentX, currentY);
                    
                    maxHeightInRow = Math.Max(maxHeightInRow, btn.Height);
                    
                    itemsInRow++;
                    if (itemsInRow >= maxColumns)
                    {
                        // Move to next row
                        itemsInRow = 0;
                        currentX = startX;
                        currentY += maxHeightInRow + padding;
                        maxHeightInRow = 0;
                    }
                    else
                    {
                        // Move to next column
                        currentX += btn.Width + padding;
                    }
                }
            }

            configModified = true;
            status = $"Arranged {sortedUnits.Count} notes by date (newest first)";
            UpdateUndoRedoMenuState();
        }

        private void ArrangeButtonsByColor()
        {
            if (Units.Count == 0)
            {
                status = "No notes to arrange";
                return;
            }

            SaveStateForUndo();

            const int padding = 10;
            const int startX = 10;
            const int startY = 10;
            
            // Group by color and sort by hue
            var sortedUnits = Units.OrderBy(kvp => 
            {
                var color = Color.FromArgb(kvp.Value.BackgroundColor);
                return color.GetHue();
            }).ThenBy(kvp => 
            {
                var color = Color.FromArgb(kvp.Value.BackgroundColor);
                return color.GetBrightness();
            }).ToList();
            
            int currentX = startX;
            int currentY = startY;
            int maxHeightInRow = 0;
            int itemsInRow = 0;
            const int maxColumns = 4;

            foreach (var kvp in sortedUnits)
            {
                var id = kvp.Key;
                var unit = kvp.Value;
                
                // Find the button for this unit
                var btn = FindButtonById(id);
                    
                if (btn != null)
                {
                    unit.X = currentX;
                    unit.Y = currentY;
                    Units[id] = unit;
                    
                    btn.Location = new Point(currentX, currentY);
                    
                    maxHeightInRow = Math.Max(maxHeightInRow, btn.Height);
                    
                    itemsInRow++;
                    if (itemsInRow >= maxColumns)
                    {
                        // Move to next row
                        itemsInRow = 0;
                        currentX = startX;
                        currentY += maxHeightInRow + padding;
                        maxHeightInRow = 0;
                    }
                    else
                    {
                        // Move to next column
                        currentX += btn.Width + padding;
                    }
                }
            }

            configModified = true;
            status = $"Arranged {sortedUnits.Count} notes by color";
            UpdateUndoRedoMenuState();
        }

        private void ArrangeButtonsCompact()
        {
            if (Units.Count == 0)
            {
                status = "No notes to arrange";
                return;
            }

            SaveStateForUndo();

            const int padding = 5;
            const int startX = 5;
            const int startY = 5;
            
            var buttons = GetAllButtonsInPanel().Cast<Button>().OrderBy(b => 
            {
                // Sort by current position (top-to-bottom, left-to-right)
                return b.Location.Y * 10000 + b.Location.X;
            }).ToList();
            
            int currentX = startX;
            int currentY = startY;
            int maxHeightInRow = 0;
            int availableWidth = panelContainer.Width - 20;

            foreach (var btn in buttons)
            {
                string id = (string)btn.Tag;
                if (Units.ContainsKey(id))
                {
                    // Check if this button would exceed the available width
                    if (currentX + btn.Width > availableWidth && currentX > startX)
                    {
                        // Move to next row
                        currentX = startX;
                        currentY += maxHeightInRow + padding;
                        maxHeightInRow = 0;
                    }
                    
                    var unit = Units[id];
                    unit.X = currentX;
                    unit.Y = currentY;
                    Units[id] = unit;
                    
                    btn.Location = new Point(currentX, currentY);
                    
                    // Track max height in this row
                    maxHeightInRow = Math.Max(maxHeightInRow, btn.Height);
                    
                    // Move to next position
                    currentX += btn.Width + padding;
                }
            }

            configModified = true;
            status = $"Arranged {buttons.Count} notes in compact layout";
            UpdateUndoRedoMenuState();
        }

        private void FixOverlappingButtons()
        {
            if (Units.Count == 0)
            {
                status = "No notes to check";
                return;
            }

            SaveStateForUndo();

            var buttons = GetAllButtonsInPanel().Cast<Button>().ToList();
            if (buttons.Count == 0)
            {
                status = "No buttons to check";
                return;
            }

            const int minSpacing = 5; // Minimum spacing between buttons
            const int maxIterations = 100; // Maximum iterations to prevent infinite loops
            const double damping = 0.8; // Damping factor for smoother convergence
            
            // Track which buttons have been moved
            var movedButtons = new HashSet<Button>();
            
            // Stores displacement vectors for each button per iteration
            var displacements = new Dictionary<Button, (double dx, double dy)>();
            
            int iteration = 0;
            bool hasOverlaps = true;
            
            // Iterative physics-based separation algorithm
            while (hasOverlaps && iteration < maxIterations)
            {
                hasOverlaps = false;
                
                // Reset displacements for this iteration
                displacements.Clear();
                foreach (var btn in buttons)
                {
                    displacements[btn] = (0.0, 0.0);
                }
                
                // Check all pairs of buttons
                for (int i = 0; i < buttons.Count; i++)
                {
                    for (int j = i + 1; j < buttons.Count; j++)
                    {
                        var btn1 = buttons[i];
                        var btn2 = buttons[j];
                        
                        // Create expanded rectangles with spacing buffer
                        Rectangle rect1 = new Rectangle(
                            btn1.Left - minSpacing,
                            btn1.Top - minSpacing,
                            btn1.Width + (minSpacing * 2),
                            btn1.Height + (minSpacing * 2)
                        );
                        Rectangle rect2 = new Rectangle(
                            btn2.Left - minSpacing,
                            btn2.Top - minSpacing,
                            btn2.Width + (minSpacing * 2),
                            btn2.Height + (minSpacing * 2)
                        );
                        
                        // Check for overlap
                        if (rect1.IntersectsWith(rect2))
                        {
                            hasOverlaps = true;
                            
                            // Calculate centers
                            double center1X = btn1.Left + btn1.Width / 2.0;
                            double center1Y = btn1.Top + btn1.Height / 2.0;
                            double center2X = btn2.Left + btn2.Width / 2.0;
                            double center2Y = btn2.Top + btn2.Height / 2.0;
                            
                            // Calculate direction vector from btn1 to btn2
                            double dx = center2X - center1X;
                            double dy = center2Y - center1Y;
                            
                            // Calculate distance between centers
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            
                            // Handle exact overlap (same center)
                            if (distance < 0.1)
                            {
                                // Use a random direction to separate
                                Random rnd = new Random(btn1.GetHashCode() ^ btn2.GetHashCode());
                                double angle = rnd.NextDouble() * Math.PI * 2;
                                dx = Math.Cos(angle);
                                dy = Math.Sin(angle);
                                distance = 1.0;
                            }
                            
                            // Normalize direction
                            double normX = dx / distance;
                            double normY = dy / distance;
                            
                            // Calculate required separation distance
                            // We need to separate enough so rectangles don't overlap with spacing
                            Rectangle intersection = Rectangle.Intersect(rect1, rect2);
                            double overlapWidth = intersection.Width;
                            double overlapHeight = intersection.Height;
                            
                            // Calculate repulsion force based on overlap amount
                            // Larger overlap = stronger force
                            double overlapArea = overlapWidth * overlapHeight;
                            double forceMagnitude = Math.Sqrt(overlapArea) * 0.5;
                            
                            // Apply force in the direction of separation
                            // Both buttons move away from each other (symmetric)
                            double forceX = normX * forceMagnitude;
                            double forceY = normY * forceMagnitude;
                            
                            // Accumulate displacement for both buttons
                            var disp1 = displacements[btn1];
                            displacements[btn1] = (disp1.dx - forceX, disp1.dy - forceY);
                            
                            var disp2 = displacements[btn2];
                            displacements[btn2] = (disp2.dx + forceX, disp2.dy + forceY);
                        }
                    }
                }
                
                // Apply accumulated displacements with damping
                foreach (var btn in buttons)
                {
                    var (dx, dy) = displacements[btn];
                    
                    // Only move if there's actual displacement
                    if (Math.Abs(dx) > 0.1 || Math.Abs(dy) > 0.1)
                    {
                        // Apply damping for smoother convergence
                        double dampedDx = dx * damping;
                        double dampedDy = dy * damping;
                        
                        // Calculate new position
                        int newX = btn.Left + (int)Math.Round(dampedDx);
                        int newY = btn.Top + (int)Math.Round(dampedDy);
                        
                        // Keep button on screen (non-negative coordinates)
                        newX = Math.Max(0, newX);
                        newY = Math.Max(0, newY);
                        
                        // Update button position
                        btn.Left = newX;
                        btn.Top = newY;
                        
                        movedButtons.Add(btn);
                        
                        // Update the unit position in the dictionary
                        string btnId = (string)btn.Tag;
                        if (Units.ContainsKey(btnId))
                        {
                            var unit = Units[btnId];
                            unit.X = btn.Left;
                            unit.Y = btn.Top;
                            Units[btnId] = unit;
                        }
                    }
                }
                
                iteration++;
            }
            
            // Final verification with actual bounds (no spacing buffer)
            int remainingOverlaps = 0;
            for (int i = 0; i < buttons.Count; i++)
            {
                for (int j = i + 1; j < buttons.Count; j++)
                {
                    if (buttons[i].Bounds.IntersectsWith(buttons[j].Bounds))
                    {
                        remainingOverlaps++;
                    }
                }
            }
            
            if (movedButtons.Count > 0)
            {
                configModified = true;
            }
            
            if (remainingOverlaps > 0)
            {
                status = $"Moved {movedButtons.Count} button(s) in {iteration} iterations, {remainingOverlaps} overlap(s) remain";
            }
            else if (movedButtons.Count > 0)
            {
                status = $"Fixed all overlaps affecting {movedButtons.Count} button(s) in {iteration} iteration(s)";
            }
            else
            {
                status = "No overlapping buttons found";
            }
            
            UpdateUndoRedoMenuState();
        }

        // Style Application Methods

        private void ApplyStyleToSelectedButtons(Color backgroundColor, Color textColor, Font font = null, Button specificButton = null)
        {
            // Get buttons to style
            List<Button> buttonsToStyle;
            
            if (specificButton != null && selectedButtons.Count > 0 && selectedButtons.Contains(specificButton))
            {
                // Right-clicked on a selected button - apply to all selected buttons
                buttonsToStyle = selectedButtons.ToList();
            }
            else if (specificButton != null)
            {
                // Right-clicked on a non-selected button - apply only to that button
                buttonsToStyle = new List<Button> { specificButton };
            }
            else if (selectedButtons.Count > 0)
            {
                // Style selected buttons (from View menu)
                buttonsToStyle = selectedButtons.ToList();
            }
            else
            {
                // Style all buttons (from View menu when nothing selected)
                buttonsToStyle = GetAllButtonsInPanel().Cast<Button>().ToList();
            }

            if (buttonsToStyle.Count == 0)
            {
                status = "No buttons to style";
                return;
            }

            SaveStateForUndo();

            // Use default font if none provided
            if (font == null)
            {
                font = new Font("Segoe UI", 9f, FontStyle.Regular);
            }

            // Suspend layout to prevent flickering
            panelContainer.SuspendLayout();

            var buttonsToRecreate = new List<(string id, UnitStruct unit, Point location)>();

            foreach (var btn in buttonsToStyle)
            {
                string id = (string)btn.Tag;
                if (Units.ContainsKey(id))
                {
                    var unit = Units[id];
                    unit.BackgroundColor = backgroundColor.ToArgb();
                    unit.TextColor = textColor.ToArgb();
                    unit.Font = font;
                    unit.ButtonType = "DoubleClickButton"; // Clear custom button type
                    Units[id] = unit;

                    // If this is a custom button type, we need to replace it with standard button
                    if (btn.GetType() != typeof(DoubleClickButton))
                    {
                        buttonsToRecreate.Add((id, unit, btn.Location));
                        
                        // Remove from selection
                        if (selectedButtons.Contains(btn))
                        {
                            selectedButtons.Remove(btn);
                        }
                        
                        // Remove and dispose
                        Rectangle oldBounds = btn.Bounds;
                        RemoveButtonControl(btn);
                        btn.Dispose();
                        panelContainer.Invalidate(oldBounds);
                    }
                    else
                    {
                        // Standard button - just update properties
                        btn.BackColor = backgroundColor;
                        btn.ForeColor = textColor;
                        btn.Font = font;
                        btn.FlatStyle = FlatStyle.Standard;
                        btn.Invalidate();
                    }
                }
            }

            // Recreate custom buttons as standard buttons
            foreach (var (id, unit, location) in buttonsToRecreate)
            {
                Button newBtn = new DoubleClickButton();
                newBtn.Tag = id;
                newBtn.AutoSize = true;
                newBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                newBtn.Text = unit.Title;
                newBtn.BackColor = backgroundColor;
                newBtn.ForeColor = textColor;
                newBtn.Font = font;
                newBtn.Location = location;
                newBtn.ContextMenuStrip = unitMenuStrip;
                newBtn.Cursor = Cursors.Hand;
                newBtn.FlatStyle = FlatStyle.Standard;

                // Set up events
                newBtn.Click += newButton_Click;
                newBtn.DoubleClick += newButton_DoubleClick;
                newBtn.MouseUp += newButton_MouseUp;
                newBtn.MouseDown += newButton_MouseDown;
                newBtn.MouseMove += newButton_MouseMove;
                newBtn.PreviewKeyDown += newButton_PreviewKeyDown;
                newBtn.KeyDown += newButton_KeyDown;
                newBtn.KeyUp += newButton_KeyUp;

                AddButtonControl(newBtn);
            }

            // Resume layout and refresh
            panelContainer.ResumeLayout(true);
            panelContainer.Refresh();

            configModified = true;
            string target = buttonsToStyle.Count.ToString();
            status = $"Applied style to {target} button(s)";
            UpdateUndoRedoMenuState();
        }

        private void ApplyAdvancedStyleToSelectedButtons(Color backgroundColor, Color textColor, Font font, FlatStyle flatStyle, Color borderColor, int borderSize = 2, Button specificButton = null)
        {
            // Get buttons to style
            List<Button> buttonsToStyle;
            
            if (specificButton != null && selectedButtons.Count > 0 && selectedButtons.Contains(specificButton))
            {
                // Right-clicked on a selected button - apply to all selected buttons
                buttonsToStyle = selectedButtons.ToList();
            }
            else if (specificButton != null)
            {
                // Right-clicked on a non-selected button - apply only to that button
                buttonsToStyle = new List<Button> { specificButton };
            }
            else if (selectedButtons.Count > 0)
            {
                // Style selected buttons (from View menu)
                buttonsToStyle = selectedButtons.ToList();
            }
            else
            {
                // Style all buttons (from View menu when nothing selected)
                buttonsToStyle = GetAllButtonsInPanel().Cast<Button>().ToList();
            }

            if (buttonsToStyle.Count == 0)
            {
                status = "No buttons to style";
                return;
            }

            SaveStateForUndo();

            foreach (var btn in buttonsToStyle)
            {
                string id = (string)btn.Tag;
                if (Units.ContainsKey(id))
                {
                    var unit = Units[id];
                    unit.BackgroundColor = backgroundColor.ToArgb();
                    unit.TextColor = textColor.ToArgb();
                    unit.Font = font;
                    Units[id] = unit;

                    btn.BackColor = backgroundColor;
                    btn.ForeColor = textColor;
                    btn.Font = font;
                    btn.FlatStyle = flatStyle;
                    
                    if (flatStyle == FlatStyle.Flat)
                    {
                        btn.FlatAppearance.BorderColor = borderColor;
                        btn.FlatAppearance.BorderSize = borderSize;
                    }
                }
            }

            configModified = true;
            string target = selectedButtons.Count > 0 ? $"{selectedButtons.Count} selected button(s)" : $"all {buttonsToStyle.Count} button(s)";
            status = $"Applied advanced style to {target}";
            UpdateUndoRedoMenuState();
        }

        private void ApplyCustomButtonStyle<T>(Color backgroundColor, Color textColor, Font font, Action<T> customizer = null, Button specificButton = null) where T : Button, new()
        {
            // Get buttons to replace
            List<Button> buttonsToReplace;
            
            if (specificButton != null && selectedButtons.Count > 0 && selectedButtons.Contains(specificButton))
            {
                // Right-clicked on a selected button - apply to all selected buttons
                buttonsToReplace = selectedButtons.ToList();
            }
            else if (specificButton != null)
            {
                // Right-clicked on a non-selected button - apply only to that button
                buttonsToReplace = new List<Button> { specificButton };
            }
            else if (selectedButtons.Count > 0)
            {
                // Style selected buttons (from View menu)
                buttonsToReplace = selectedButtons.ToList();
            }
            else
            {
                // Style all buttons (from View menu when nothing selected)
                buttonsToReplace = GetAllButtonsInPanel().Cast<Button>().ToList();
            }

            if (buttonsToReplace.Count == 0)
            {
                status = "No buttons to style";
                return;
            }

            SaveStateForUndo();

            // Collect parent controls and suspend their layouts
            var parentControls = new HashSet<Control>();
            foreach (var btn in buttonsToReplace)
            {
                if (btn.Parent != null)
                {
                    parentControls.Add(btn.Parent);
                }
            }

            foreach (var parent in parentControls)
            {
                parent.SuspendLayout();
            }

            var newButtons = new List<Button>();

            foreach (var oldBtn in buttonsToReplace)
            {
                string id = (string)oldBtn.Tag;
                if (Units.ContainsKey(id))
                {
                    // Update unit data
                    var unit = Units[id];
                    unit.BackgroundColor = backgroundColor.ToArgb();
                    unit.TextColor = textColor.ToArgb();
                    unit.Font = font;
                    unit.ButtonType = typeof(T).Name; // Save the button type
                    Units[id] = unit;

                    // Store old button's rectangle and parent for invalidation
                    Rectangle oldBounds = oldBtn.Bounds;
                    Control parentControl = oldBtn.Parent;

                    // Create new custom button
                    T newBtn = new T();
                    newBtn.Tag = id;
                    newBtn.Text = oldBtn.Text;
                    newBtn.BackColor = backgroundColor;
                    newBtn.ForeColor = textColor;
                    newBtn.Font = font;
                    newBtn.Location = oldBtn.Location;
                    newBtn.AutoSize = true;
                    newBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    newBtn.ContextMenuStrip = unitMenuStrip;
                    newBtn.Cursor = Cursors.Hand;
                    
                    // Add padding and prevent text wrapping for custom buttons
                    newBtn.Padding = new Padding(12, 8, 12, 8);
                    newBtn.MinimumSize = new Size(80, 40);

                    // Apply custom properties
                    customizer?.Invoke(newBtn);

                    // Set up events
                    newBtn.Click += newButton_Click;
                    newBtn.DoubleClick += newButton_DoubleClick;
                    newBtn.MouseUp += newButton_MouseUp;
                    newBtn.MouseDown += newButton_MouseDown;
                    newBtn.MouseMove += newButton_MouseMove;
                    newBtn.PreviewKeyDown += newButton_PreviewKeyDown;
                    newBtn.KeyDown += newButton_KeyDown;
                    newBtn.KeyUp += newButton_KeyUp;

                    // Remove old button from selection first
                    if (selectedButtons.Contains(oldBtn))
                    {
                        selectedButtons.Remove(oldBtn);
                    }

                    // Remove old button from its parent
                    if (parentControl != null)
                    {
                        parentControl.Controls.Remove(oldBtn);
                    }
                    
                    // Dispose old button to free resources
                    oldBtn.Dispose();

                    // Add new button to the same parent
                    if (parentControl != null)
                    {
                        parentControl.Controls.Add(newBtn);
                    }
                    
                    // Add to selection if it was selected
                    if (buttonsToReplace.Contains(oldBtn))
                    {
                        selectedButtons.Add(newBtn);
                    }

                    newButtons.Add(newBtn);

                    // Invalidate the old area to clear artifacts on the correct parent
                    parentControl?.Invalidate(oldBounds);
                }
            }

            // Resume layout and refresh for all affected parent controls
            foreach (var parent in parentControls)
            {
                parent.ResumeLayout(true);
                parent.Refresh();
            }

            configModified = true;
            string target = newButtons.Count.ToString();
            status = $"Applied custom style to {target} button(s)";
            UpdateUndoRedoMenuState();
        }

        private void menuStyleClassic_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.LightSteelBlue,
                Color.DarkBlue,
                new Font("Segoe UI", 9f, FontStyle.Regular),
                contextInfo?.Button
            );
        }

        private void menuStylePastel_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.LightPink,
                Color.DarkRed,
                new Font("Segoe UI", 9f, FontStyle.Regular),
                contextInfo?.Button
            );
        }

        private void menuStyleDark_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.FromArgb(45, 45, 48),
                Color.WhiteSmoke,
                new Font("Consolas", 9f, FontStyle.Regular),
                contextInfo?.Button
            );
        }

        private void menuStyleNeon_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.FromArgb(57, 255, 20),
                Color.Black,
                new Font("Segoe UI", 9f, FontStyle.Bold),
                contextInfo?.Button
            );
        }

        private void menuStyleEarth_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.FromArgb(210, 180, 140),
                Color.FromArgb(101, 67, 33),
                new Font("Georgia", 9f, FontStyle.Regular),
                contextInfo?.Button
            );
        }

        private void menuStyleOcean_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.FromArgb(0, 119, 190),
                Color.White,
                new Font("Segoe UI", 9f, FontStyle.Regular),
                contextInfo?.Button
            );
        }

        private void menuStyleSunset_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.FromArgb(255, 140, 0),
                Color.White,
                new Font("Segoe UI", 9f, FontStyle.Bold),
                contextInfo?.Button
            );
        }

        private void menuStyleMonochrome_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.White,
                Color.Black,
                new Font("Arial", 9f, FontStyle.Regular),
                contextInfo?.Button
            );
        }

        private void menuStyleVibrant_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            ApplyStyleToSelectedButtons(
                Color.FromArgb(138, 43, 226),
                Color.White,
                new Font("Segoe UI", 9f, FontStyle.Bold),
                contextInfo?.Button
            );
        }

        // 3D and Advanced Effect Styles

        private void menuStyleGradient_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Custom Gradient Button with shadow
            ApplyCustomButtonStyle<GradientButton>(
                Color.FromArgb(100, 149, 237),
                Color.White,
                new Font("Segoe UI", 9.5f, FontStyle.Bold),
                btn =>
                {
                    btn.GradientTop = Color.FromArgb(100, 149, 237);
                    btn.GradientBottom = Color.FromArgb(65, 105, 225);
                },
                contextInfo?.Button
            );
        }

        private void menuStyleGloss_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Glossy gradient button
            ApplyCustomButtonStyle<GradientButton>(
                Color.FromArgb(70, 130, 180),
                Color.White,
                new Font("Segoe UI", 10f, FontStyle.Bold),
                btn =>
                {
                    btn.GradientTop = Color.FromArgb(135, 206, 250);
                    btn.GradientBottom = Color.FromArgb(25, 25, 112);
                },
                contextInfo?.Button
            );
        }

        private void menuStyleEmbossed_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Neumorphism soft UI
            ApplyCustomButtonStyle<NeumorphismButton>(
                Color.FromArgb(230, 230, 230),
                Color.FromArgb(60, 60, 60),
                new Font("Segoe UI", 9f, FontStyle.Bold),
                null,
                contextInfo?.Button
            );
        }

        private void menuStyleRaised_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Material Design button with ripple
            ApplyCustomButtonStyle<MaterialButton>(
                Color.FromArgb(33, 150, 243),
                Color.White,
                new Font("Segoe UI", 9.5f, FontStyle.Regular),
                null,
                contextInfo?.Button
            );
        }

        private void menuStyleInset_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Retro 3D button
            ApplyCustomButtonStyle<Retro3DButton>(
                Color.FromArgb(255, 20, 147),
                Color.White,
                new Font("Impact", 10f, FontStyle.Bold),
                null,
                contextInfo?.Button
            );
        }

        // Creative Theme Styles

        private void menuStyleRetro_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Retro 3D style - hot pink
            ApplyCustomButtonStyle<Retro3DButton>(
                Color.FromArgb(255, 20, 147),
                Color.FromArgb(255, 255, 0),
                new Font("Impact", 10f, FontStyle.Bold),
                null,
                contextInfo?.Button
            );
        }

        private void menuStyleCyber_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Cyberpunk neon glow
            ApplyCustomButtonStyle<NeonGlowButton>(
                Color.FromArgb(20, 20, 35),
                Color.FromArgb(0, 255, 255),
                new Font("Consolas", 9.5f, FontStyle.Bold),
                btn =>
                {
                    btn.GlowColor = Color.FromArgb(255, 0, 255);
                },
                contextInfo?.Button
            );
        }

        private void menuStyleGlass_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Glassmorphism frosted glass
            ApplyCustomButtonStyle<GlassMorphismButton>(
                Color.FromArgb(240, 248, 255),
                Color.FromArgb(70, 130, 180),
                new Font("Segoe UI", 9f, FontStyle.Regular),
                null,
                contextInfo?.Button
            );
        }

        private void menuStyleNeonGlow_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Neon glow effect
            ApplyCustomButtonStyle<NeonGlowButton>(
                Color.FromArgb(10, 10, 20),
                Color.FromArgb(0, 255, 127),
                new Font("Arial", 10f, FontStyle.Bold),
                btn =>
                {
                    btn.GlowColor = Color.FromArgb(0, 255, 127);
                },
                contextInfo?.Button
            );
        }

        private void menuStyleGolden_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Golden gradient premium
            ApplyCustomButtonStyle<GradientButton>(
                Color.FromArgb(255, 215, 0),
                Color.FromArgb(139, 69, 19),
                new Font("Georgia", 10f, FontStyle.Bold),
                btn =>
                {
                    btn.GradientTop = Color.FromArgb(255, 223, 0);
                    btn.GradientBottom = Color.FromArgb(218, 165, 32);
                },
                contextInfo?.Button
            );
        }

        // Typography-focused Styles

        private void menuStyleMinimal_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Minimal outline style
            ApplyCustomButtonStyle<OutlineButton>(
                Color.White,
                Color.FromArgb(100, 100, 100),
                new Font("Segoe UI", 9f, FontStyle.Regular),
                null,
                contextInfo?.Button
            );
        }

        private void menuStyleBold_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Bold skeuomorphic 3D
            ApplyCustomButtonStyle<SkeuomorphicButton>(
                Color.FromArgb(220, 20, 60),
                Color.White,
                new Font("Arial Black", 10f, FontStyle.Bold),
                null,
                contextInfo?.Button
            );
        }

        private void menuStyleElegant_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Elegant premium card
            ApplyCustomButtonStyle<PremiumCardButton>(
                Color.FromArgb(245, 245, 220),
                Color.FromArgb(75, 0, 130),
                new Font("Times New Roman", 10f, FontStyle.Italic),
                null,
                contextInfo?.Button
            );
        }

        private void menuStylePlayful_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Playful pill button
            ApplyCustomButtonStyle<PillButton>(
                Color.FromArgb(255, 182, 193),
                Color.FromArgb(255, 20, 147),
                new Font("Comic Sans MS", 10f, FontStyle.Bold),
                null,
                contextInfo?.Button
            );
        }

        private void menuStyleProfessional_Click(object sender, EventArgs e)
        {
            var contextInfo = getContextMenuInfo(sender);
            // Professional material design
            ApplyCustomButtonStyle<MaterialButton>(
                Color.FromArgb(96, 125, 139),
                Color.White,
                new Font("Calibri", 9.5f, FontStyle.Regular),
                null,
                contextInfo?.Button
            );
        }

        private void unitMenuStyleRandom_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            int styleIndex = rand.Next(24);

            // Call one of the 24 style handlers randomly
            switch (styleIndex)
            {
                case 0: menuStyleClassic_Click(sender, e); break;
                case 1: menuStylePastel_Click(sender, e); break;
                case 2: menuStyleDark_Click(sender, e); break;
                case 3: menuStyleNeon_Click(sender, e); break;
                case 4: menuStyleEarth_Click(sender, e); break;
                case 5: menuStyleOcean_Click(sender, e); break;
                case 6: menuStyleSunset_Click(sender, e); break;
                case 7: menuStyleMonochrome_Click(sender, e); break;
                case 8: menuStyleVibrant_Click(sender, e); break;
                case 9: menuStyleGradient_Click(sender, e); break;
                case 10: menuStyleGloss_Click(sender, e); break;
                case 11: menuStyleEmbossed_Click(sender, e); break;
                case 12: menuStyleRaised_Click(sender, e); break;
                case 13: menuStyleInset_Click(sender, e); break;
                case 14: menuStyleRetro_Click(sender, e); break;
                case 15: menuStyleCyber_Click(sender, e); break;
                case 16: menuStyleGlass_Click(sender, e); break;
                case 17: menuStyleNeonGlow_Click(sender, e); break;
                case 18: menuStyleGolden_Click(sender, e); break;
                case 19: menuStyleMinimal_Click(sender, e); break;
                case 20: menuStyleBold_Click(sender, e); break;
                case 21: menuStyleElegant_Click(sender, e); break;
                case 22: menuStylePlayful_Click(sender, e); break;
                case 23: menuStyleProfessional_Click(sender, e); break;
            }
        }

        private IEnumerable<Button> GetAllButtonsInPanel()
        {
            foreach (var button in EnumerateButtons(panelContainer))
                yield return button;
        }

        private IEnumerable<Button> EnumerateButtons(Control root)
        {
            foreach (Control control in root.Controls)
            {
                if (control is Button button)
                {
                    if (button.Tag is string id && Units.ContainsKey(id))
                        yield return button;
                }

                foreach (var childButton in EnumerateButtons(control))
                    yield return childButton;
            }
        }

        private void DisposeFontIfUnused(Font font)
        {
            if (font == null)
                return;
            if (Units.Values.Any(u => u.Font == font))
                return;
            if (GetAllButtonsInPanel().Any(b => b.Font == font))
                return;
            font.Dispose();
        }

        private void DisposeAllUnitFonts()
        {
            var fonts = new HashSet<Font>();
            foreach (var unit in Units.Values)
            {
                if (unit.Font != null)
                    fonts.Add(unit.Font);
            }
            foreach (var font in fonts)
                font.Dispose();
        }

        private Button FindButtonById(string id)
        {
            foreach (Button button in GetAllButtonsInPanel())
            {
                if ((string)button.Tag == id)
                {
                    return button;
                }
            }

            return null;
        }

        private void RemoveButtonControl(Button button)
        {
            if (selectedButtons.Contains(button))
            {
                selectedButtons.Remove(button);
            }
            if (selectionOriginalStyles.ContainsKey(button))
            {
                selectionOriginalStyles.Remove(button);
            }
            if (button.Parent is GroupBox parentGroup)
            {
                resizingGroups.Remove(parentGroup);
            }
            if (button.Parent is GroupBox groupBox)
            {
                groupBox.Controls.Remove(button);

                if (groupBox.Controls.OfType<Button>().Count() == 0)
                {
                    groupBox.Text = string.IsNullOrWhiteSpace(groupBox.Text) ? "Group" : groupBox.Text;
                }
                button.Dispose();
            }
            else
            {
                panelContainer.Controls.Remove(button);
                button.Dispose();
            }
        }

        private void AddButtonControl(Button button)
        {
            // This method is no longer needed since we add buttons directly in addButton
            // Kept for backward compatibility with other parts of code
            if (button.Parent == null)
            {
                panelContainer.Controls.Add(button);
            }
            else if (button.Parent is GroupBox groupBox && !groupBox.Controls.Contains(button))
            {
                groupBox.Controls.Add(button);
            }
        }

        private GroupBox CreateGroupBoxByType(string groupBoxType)
        {
            GroupBox groupBox;
            var normalizedType = NormalizeGroupBoxType(groupBoxType);
            
            switch (normalizedType)
            {
                case "GradientGlassGroupBox":
                    groupBox = new GradientGlassGroupBox();
                    break;
                case "NeonGlowGroupBox":
                    groupBox = new NeonGlowGroupBox();
                    break;
                case "EmbossedGroupBox":
                    groupBox = new EmbossedGroupBox();
                    break;
                case "RetroGroupBox":
                    groupBox = new RetroGroupBox();
                    break;
                case "CardGroupBox":
                    groupBox = new CardGroupBox();
                    break;
                case "MinimalGroupBox":
                    groupBox = new MinimalGroupBox();
                    break;
                case "DashedGroupBox":
                    groupBox = new DashedGroupBox();
                    break;
                case "DoubleBorderGroupBox":
                    groupBox = new DoubleBorderGroupBox();
                    break;
                case "ShadowPanelGroupBox":
                    groupBox = new ShadowPanelGroupBox();
                    break;
                case "RoundedNeonGroupBox":
                    groupBox = new RoundedNeonGroupBox();
                    break;
                case "HolographicGroupBox":
                    groupBox = new HolographicGroupBox();
                    break;
                case "VintagePaperGroupBox":
                    groupBox = new VintagePaperGroupBox();
                    break;
                case "LiquidMetalGroupBox":
                    groupBox = new LiquidMetalGroupBox();
                    break;
                case "CosmicGroupBox":
                    groupBox = new CosmicGroupBox();
                    break;
                case "RainbowSpectrumGroupBox":
                    groupBox = new RainbowSpectrumGroupBox();
                    break;
                case "AuroraBorealisGroupBox":
                    groupBox = new AuroraBorealisGroupBox();
                    break;
                case "CyberCircuitGroupBox":
                    groupBox = new CyberCircuitGroupBox();
                    break;
                case "FireLavaGroupBox":
                    groupBox = new FireLavaGroupBox();
                    break;
                case "MatrixRainGroupBox":
                    groupBox = new MatrixRainGroupBox();
                    break;
                case "CrystalIceGroupBox":
                    groupBox = new CrystalIceGroupBox();
                    break;
                case "PlasmaEnergyGroupBox":
                    groupBox = new PlasmaEnergyGroupBox();
                    break;
                case "OceanWaveGroupBox":
                    groupBox = new OceanWaveGroupBox();
                    break;
                case "ElectricStormGroupBox":
                    groupBox = new ElectricStormGroupBox();
                    break;
                case "StarfieldWarpGroupBox":
                    groupBox = new StarfieldWarpGroupBox();
                    break;
                case "HeartbeatPulseGroupBox":
                    groupBox = new HeartbeatPulseGroupBox();
                    break;
                case "SnowfallGroupBox":
                    groupBox = new SnowfallGroupBox();
                    break;
                case "CloudDriftGroupBox":
                    groupBox = new CloudDriftGroupBox();
                    break;
                case "SparkleShineGroupBox":
                    groupBox = new SparkleShineGroupBox();
                    break;
                case "RippleWaterGroupBox":
                    groupBox = new RippleWaterGroupBox();
                    break;
                case "BubblesFloatGroupBox":
                    groupBox = new BubblesFloatGroupBox();
                    break;
                case "ConfettiPartyGroupBox":
                    groupBox = new ConfettiPartyGroupBox();
                    break;
                case "SunburstRaysGroupBox":
                    groupBox = new SunburstRaysGroupBox();
                    break;
                case "CherryBlossomGroupBox":
                    groupBox = new CherryBlossomGroupBox();
                    break;
                case "FloatingHeartsGroupBox":
                    groupBox = new FloatingHeartsGroupBox();
                    break;
                default:
                    groupBox = new ResizableGroupBox();
                    break;
            }

            if (groupBox is CustomGroupBoxBase customGroupBox)
            {
                customGroupBox.AllowResize = isMovable;
            }

            return groupBox;
        }

        private void ApplyGroupBoxBehavior(GroupBox groupBox)
        {
            if (groupBox is ResizableGroupBox resizableGroupBox)
            {
                resizableGroupBox.AllowResize = isMovable;
                resizableGroupBox.UpdateResizeHandleVisibility();
            }
            else if (groupBox is CustomGroupBoxBase customGroupBox)
            {
                customGroupBox.AllowResize = isMovable;
                customGroupBox.UpdateResizeHandleVisibility();
            }
        }

        private void ApplyGroupBoxColors(GroupBox groupBox, GroupStruct group)
        {
            groupBox.BackColor = group.BackgroundColor != 0 ? Color.FromArgb(group.BackgroundColor) : Color.WhiteSmoke;
            groupBox.ForeColor = group.TextColor != 0 ? Color.FromArgb(group.TextColor) : Color.Black;

            if (group.BorderColor != 0)
            {
                groupBox.Paint -= GroupBox_CustomBorder_Paint;
                groupBox.Paint += GroupBox_CustomBorder_Paint;
            }
            else
            {
                groupBox.Paint -= GroupBox_CustomBorder_Paint;
            }

            groupBox.Invalidate();
        }

        private void GroupBox_CustomBorder_Paint(object sender, PaintEventArgs e)
        {
            if (sender is GroupBox groupBox)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                string groupId = groupBox.Tag as string;
                if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                    return;

                var group = Groups[groupId];
                if (group.BorderColor == 0)
                    return;

                using var pen = new Pen(Color.FromArgb(group.BorderColor));
                var rect = groupBox.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;

                var text = groupBox.Text ?? string.Empty;
                var textSize = TextRenderer.MeasureText(text, groupBox.Font);
                int textPadding = 8;
                int textGap = 4;
                int textLeft = rect.Left + textPadding;
                int textRight = textLeft + textSize.Width + textGap;

                // Top border split around text
                e.Graphics.DrawLine(pen, rect.Left, rect.Top, textLeft - 2, rect.Top);
                e.Graphics.DrawLine(pen, textRight, rect.Top, rect.Right, rect.Top);

                // Other borders
                e.Graphics.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom);
                e.Graphics.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom);
                e.Graphics.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
            }
        }

        private static string NormalizeGroupBoxType(string groupBoxType)
        {
            if (string.IsNullOrWhiteSpace(groupBoxType))
                return "Default";

            switch (groupBoxType.Trim())
            {
                case "ResizableGroupBox":
                case "GradientGlassGroupBox":
                case "NeonGlowGroupBox":
                case "EmbossedGroupBox":
                case "RetroGroupBox":
                case "CardGroupBox":
                case "MinimalGroupBox":
                case "DashedGroupBox":
                case "DoubleBorderGroupBox":
                case "ShadowPanelGroupBox":
                case "RoundedNeonGroupBox":
                case "HolographicGroupBox":
                case "VintagePaperGroupBox":
                case "LiquidMetalGroupBox":
                case "CosmicGroupBox":
                case "RainbowSpectrumGroupBox":
                case "AuroraBorealisGroupBox":
                case "CyberCircuitGroupBox":
                case "FireLavaGroupBox":
                case "MatrixRainGroupBox":
                case "CrystalIceGroupBox":
                case "PlasmaEnergyGroupBox":
                case "OceanWaveGroupBox":
                case "ElectricStormGroupBox":
                case "StarfieldWarpGroupBox":
                case "HeartbeatPulseGroupBox":
                case "SnowfallGroupBox":
                case "CloudDriftGroupBox":
                case "SparkleShineGroupBox":
                case "RippleWaterGroupBox":
                case "BubblesFloatGroupBox":
                case "ConfettiPartyGroupBox":
                case "SunburstRaysGroupBox":
                case "CherryBlossomGroupBox":
                case "FloatingHeartsGroupBox":
                    return groupBoxType;
                default:
                    return "Default";
            }
        }

        private static string NormalizeGroupBoxTypeCaseInsensitive(string groupBoxType)
        {
            if (string.IsNullOrWhiteSpace(groupBoxType))
                return "Default";

            var normalized = NormalizeGroupBoxType(groupBoxType);
            if (normalized != "Default")
                return normalized;

            var candidate = groupBoxType.Trim();
            foreach (var name in new[]
            {
                "ResizableGroupBox",
                "GradientGlassGroupBox",
                "NeonGlowGroupBox",
                "EmbossedGroupBox",
                "RetroGroupBox",
                "CardGroupBox",
                "MinimalGroupBox",
                "DashedGroupBox",
                "DoubleBorderGroupBox",
                "ShadowPanelGroupBox",
                "RoundedNeonGroupBox",
                "HolographicGroupBox",
                "VintagePaperGroupBox",
                "LiquidMetalGroupBox",
                "CosmicGroupBox",
                "RainbowSpectrumGroupBox",
                "AuroraBorealisGroupBox",
                "CyberCircuitGroupBox",
                "FireLavaGroupBox",
                "MatrixRainGroupBox",
                "CrystalIceGroupBox",
                "PlasmaEnergyGroupBox",
                "OceanWaveGroupBox",
                "ElectricStormGroupBox",
                "StarfieldWarpGroupBox",
                "HeartbeatPulseGroupBox",
                "SnowfallGroupBox",
                "CloudDriftGroupBox",
                "SparkleShineGroupBox",
                "RippleWaterGroupBox",
                "BubblesFloatGroupBox",
                "ConfettiPartyGroupBox",
                "SunburstRaysGroupBox",
                "CherryBlossomGroupBox",
                "FloatingHeartsGroupBox"
            })
            {
                if (string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase))
                    return name;
            }

            return "Default";
        }

        private static string NormalizeButtonType(string buttonType)
        {
            if (string.IsNullOrWhiteSpace(buttonType))
                return "DoubleClickButton";

            switch (buttonType.Trim())
            {
                case "Button":
                case "GradientButton":
                case "NeonGlowButton":
                case "MaterialButton":
                case "GlassMorphismButton":
                case "NeumorphismButton":
                case "Retro3DButton":
                case "PremiumCardButton":
                case "OutlineButton":
                case "PillButton":
                case "SkeuomorphicButton":
                case "DoubleClickButton":
                    return buttonType;
                default:
                    // case-insensitive fallback
                    if (string.Equals(buttonType, "Button", StringComparison.OrdinalIgnoreCase))
                        return "Button";
                    if (string.Equals(buttonType, "DoubleClickButton", StringComparison.OrdinalIgnoreCase))
                        return "DoubleClickButton";
                    if (string.Equals(buttonType, "GradientButton", StringComparison.OrdinalIgnoreCase))
                        return "GradientButton";
                    if (string.Equals(buttonType, "NeonGlowButton", StringComparison.OrdinalIgnoreCase))
                        return "NeonGlowButton";
                    if (string.Equals(buttonType, "MaterialButton", StringComparison.OrdinalIgnoreCase))
                        return "MaterialButton";
                    if (string.Equals(buttonType, "GlassMorphismButton", StringComparison.OrdinalIgnoreCase))
                        return "GlassMorphismButton";
                    if (string.Equals(buttonType, "NeumorphismButton", StringComparison.OrdinalIgnoreCase))
                        return "NeumorphismButton";
                    if (string.Equals(buttonType, "Retro3DButton", StringComparison.OrdinalIgnoreCase))
                        return "Retro3DButton";
                    if (string.Equals(buttonType, "PremiumCardButton", StringComparison.OrdinalIgnoreCase))
                        return "PremiumCardButton";
                    if (string.Equals(buttonType, "OutlineButton", StringComparison.OrdinalIgnoreCase))
                        return "OutlineButton";
                    if (string.Equals(buttonType, "PillButton", StringComparison.OrdinalIgnoreCase))
                        return "PillButton";
                    if (string.Equals(buttonType, "SkeuomorphicButton", StringComparison.OrdinalIgnoreCase))
                        return "SkeuomorphicButton";
                    return "DoubleClickButton";
            }
        }

        private GroupBox GetOrCreateGroupBox(string groupId)
        {
            Logger.Debug($"GetOrCreateGroupBox called for: {groupId}");
            
            if (string.IsNullOrEmpty(groupId))
            {
                Logger.Warning("GetOrCreateGroupBox called with null/empty groupId");
                return null;
            }

            var groupBox = panelContainer.Controls
                .OfType<GroupBox>()
                .FirstOrDefault(gb => string.Equals(gb.Tag as string, groupId, StringComparison.Ordinal));

            if (groupBox != null)
            {
                Logger.Debug($"Existing group box found: {groupBox.Text}, Controls count: {groupBox.Controls.Count}");
                return groupBox;
            }

            if (!Groups.ContainsKey(groupId))
            {
                Logger.Error($"Group ID not found in Groups dictionary: {groupId}");
                return null;
            }

            var group = Groups[groupId];
            Logger.Info($"Creating new GroupBox for group: {group.Title}, Type: {group.GroupBoxType}");

            var newGroupBox = CreateGroupBoxByType(group.GroupBoxType);
            newGroupBox.Tag = group.Id;
            newGroupBox.Text = group.Title;
            newGroupBox.Location = new Point(group.X, group.Y);
            newGroupBox.Size = new Size(group.Width, group.Height);
            newGroupBox.BackColor = group.BackgroundColor != 0 ? Color.FromArgb(group.BackgroundColor) : Color.WhiteSmoke;
            newGroupBox.ForeColor = group.TextColor != 0 ? Color.FromArgb(group.TextColor) : Color.Black;

            Logger.Debug($"GroupBox created - Location: {newGroupBox.Location}, Size: {newGroupBox.Size}");

            newGroupBox.MouseDown += GroupBox_MouseDown;
            newGroupBox.MouseMove += GroupBox_MouseMove;
            newGroupBox.MouseUp += GroupBox_MouseUp;
            newGroupBox.ContextMenuStrip = groupMenuStrip;
            newGroupBox.SizeChanged += GroupBox_SizeChanged;

            panelContainer.Controls.Add(newGroupBox);
            Logger.Debug($"GroupBox added to panel. Panel controls count: {panelContainer.Controls.Count}");

            return newGroupBox;
        }

        private void GroupBox_SizeChanged(object sender, EventArgs e)
        {
            if (sender is GroupBox groupBox)
            {
                string groupId = groupBox.Tag as string;
                if (!string.IsNullOrEmpty(groupId) && Groups.ContainsKey(groupId))
                {
                    bool isResizing = (groupBox is ResizableGroupBox resizable && resizable.IsResizing) ||
                                      (groupBox is CustomGroupBoxBase custom && custom.IsResizing);
                    if (isResizing && !resizingGroups.Contains(groupBox))
                    {
                        SaveStateForUndo();
                        resizingGroups.Add(groupBox);
                    }
                    else if (!isResizing && resizingGroups.Contains(groupBox))
                    {
                        resizingGroups.Remove(groupBox);
                        UpdateUndoRedoMenuState();
                    }
                    var group = Groups[groupId];
                    group.Width = groupBox.Width;
                    group.Height = groupBox.Height;
                    Groups[groupId] = group;
                    configModified = true;

                    int minY = Math.Max(0, groupBox.DisplayRectangle.Top);
                    int maxX = Math.Max(0, groupBox.ClientSize.Width);
                    int maxY = Math.Max(minY, groupBox.ClientSize.Height);
                    foreach (Button button in groupBox.Controls.OfType<Button>())
                    {
                        int clampedX = Math.Min(Math.Max(button.Location.X, 0), maxX - button.Width);
                        int clampedY = Math.Min(Math.Max(button.Location.Y, minY), maxY - button.Height);
                        if (clampedX != button.Location.X || clampedY != button.Location.Y)
                        {
                            button.Location = new Point(clampedX, clampedY);
                            string btnId = button.Tag as string;
                            if (!string.IsNullOrEmpty(btnId) && Units.ContainsKey(btnId))
                            {
                                var unit = Units[btnId];
                                unit.X = groupBox.Location.X + clampedX;
                                unit.Y = groupBox.Location.Y + clampedY;
                                Units[btnId] = unit;
                            }
                        }
                    }
                }
            }
        }

        private void panelContainer_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(GroupBox)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void panelContainer_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(GroupBox)))
                return;

            GroupBox groupBox = e.Data.GetData(typeof(GroupBox)) as GroupBox;
            if (groupBox == null)
                return;

            Point dropPoint = panelContainer.PointToClient(new Point(e.X, e.Y));
            MoveGroupBox(groupBox, dropPoint);
        }

        private void MoveGroupBox(GroupBox groupBox, Point newLocation)
        {
            string groupId = groupBox.Tag as string;
            if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                return;

            var clampedLocation = ClampGroupBoxLocation(groupBox, newLocation);
            if (clampedLocation != groupBox.Location)
            {
                SaveStateForUndo();
                MoveGroupBoxTo(groupBox, clampedLocation);
                configModified = true;
                status = "Group moved";
                UpdateUndoRedoMenuState();
            }

        }

        private void AddGroupBoxToPanel(GroupStruct group)
        {
            Logger.Debug($"AddGroupBoxToPanel - Group: {group.Id}, Title: {group.Title}, Location: ({group.X},{group.Y}), Size: ({group.Width}x{group.Height})");
            var normalizedType = NormalizeGroupBoxTypeCaseInsensitive(group.GroupBoxType);
            if (normalizedType == "Default")
                normalizedType = "ResizableGroupBox";
            if (!string.Equals(group.GroupBoxType, normalizedType, StringComparison.Ordinal))
            {
                group.GroupBoxType = normalizedType;
                if (!string.IsNullOrEmpty(group.Id) && Groups.ContainsKey(group.Id))
                    Groups[group.Id] = group;
            }

            var groupBox = GetOrCreateGroupBox(group.Id);

            if (groupBox == null)
            {
                Logger.Error($"Failed to get or create group box for: {group.Id}");
                return;
            }

            groupBox.Text = group.Title;
            var clampedLocation = ClampGroupBoxLocation(groupBox, new Point(group.X, group.Y));
            groupBox.Location = clampedLocation;
            if (clampedLocation.X != group.X || clampedLocation.Y != group.Y)
            {
                group.X = clampedLocation.X;
                group.Y = clampedLocation.Y;
                if (!string.IsNullOrEmpty(group.Id) && Groups.ContainsKey(group.Id))
                    Groups[group.Id] = group;
                configModified = true;
            }
            int width = Math.Max(100, group.Width);
            int height = Math.Max(80, group.Height);
            if (width != group.Width || height != group.Height)
            {
                group.Width = width;
                group.Height = height;
                if (!string.IsNullOrEmpty(group.Id) && Groups.ContainsKey(group.Id))
                    Groups[group.Id] = group;
                configModified = true;
            }
            groupBox.Size = new Size(width, height);
            ApplyGroupBoxColors(groupBox, group);
            
            Logger.Debug($"Group box configured - Actual location: {groupBox.Location}, Size: {groupBox.Size}, BackColor: {groupBox.BackColor}");
            
            if (groupBox is ResizableGroupBox resizableGroupBox)
            {
                resizableGroupBox.AllowResize = isMovable;
                resizableGroupBox.UpdateResizeHandleVisibility();
                Logger.Debug($"ResizableGroupBox AllowResize set to: {isMovable}");
            }
            else if (groupBox is CustomGroupBoxBase customGroupBox)
            {
                customGroupBox.AllowResize = isMovable;
                customGroupBox.UpdateResizeHandleVisibility();
                Logger.Debug($"CustomGroupBoxBase AllowResize set to: {isMovable}");
            }
        }

        private void GroupBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isMovable || e.Button != MouseButtons.Left)
                return;

            // Don't start drag if clicking on resize handle
            if (sender is ResizableGroupBox resizable && resizable.IsResizing)
                return;
            
            if (sender is CustomGroupBoxBase customResizable && customResizable.IsResizing)
                return;

            currentGroupBoxDrag = sender as GroupBox;
            if (currentGroupBoxDrag == null)
                return;

            groupBoxMoveStart = e.Location;
            currentGroupBoxOriginalLocation = currentGroupBoxDrag.Location;

            isMovingGroupBox = true;
        }

        private void GroupBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMovingGroupBox || currentGroupBoxDrag == null)
                return;

            // Check if we're in resize mode
            if (sender is ResizableGroupBox resizable && resizable.IsResizing)
            {
                isMovingGroupBox = false;
                return;
            }
            
            if (sender is CustomGroupBoxBase customResizable && customResizable.IsResizing)
            {
                isMovingGroupBox = false;
                return;
            }

            // Calculate new location based on current mouse position relative to panel
            Point mouseInPanel = panelContainer.PointToClient(currentGroupBoxDrag.PointToScreen(e.Location));
            Point newLocation = new Point(
                mouseInPanel.X - groupBoxMoveStart.X,
                mouseInPanel.Y - groupBoxMoveStart.Y);

            newLocation = ClampGroupBoxLocation(currentGroupBoxDrag, newLocation);

            currentGroupBoxDrag.Location = newLocation;
        }

        private void GroupBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isMovingGroupBox || currentGroupBoxDrag == null)
                return;

            isMovingGroupBox = false;

            if (currentGroupBoxDrag.Location != currentGroupBoxOriginalLocation)
            {
                SaveStateForUndo();
            }

            MoveGroupBoxTo(currentGroupBoxDrag, currentGroupBoxDrag.Location);
            if (currentGroupBoxDrag.Location != currentGroupBoxOriginalLocation)
            {
                configModified = true;
                status = "Group moved";
                UpdateUndoRedoMenuState();
            }

            currentGroupBoxDrag = null;
            currentGroupBoxButtonOrigins.Clear();
        }

        private void MoveGroupBoxTo(GroupBox groupBox, Point newLocation)
        {
            string groupId = groupBox.Tag as string;
            if (string.IsNullOrEmpty(groupId) || !Groups.ContainsKey(groupId))
                return;

            newLocation = ClampGroupBoxLocation(groupBox, newLocation);

            groupBox.Location = newLocation;

            // Update button absolute positions in the data model
            foreach (Button button in groupBox.Controls.OfType<Button>())
            {
                string btnId = button.Tag as string;
                if (!string.IsNullOrEmpty(btnId) && Units.ContainsKey(btnId))
                {
                    var unit = Units[btnId];
                    // Calculate absolute position: group location + button's relative position
                    Point absolutePos = new Point(
                        groupBox.Location.X + button.Location.X,
                        groupBox.Location.Y + button.Location.Y);
                    unit.X = absolutePos.X;
                    unit.Y = absolutePos.Y;
                    Units[btnId] = unit;
                }
            }

            var groupStruct = Groups[groupId];
            groupStruct.X = newLocation.X;
            groupStruct.Y = newLocation.Y;
            groupStruct.Width = groupBox.Width;
            groupStruct.Height = groupBox.Height;
            Groups[groupId] = groupStruct;
        }

        private Point ClampGroupBoxLocation(GroupBox groupBox, Point newLocation)
        {
            int maxX = panelContainer.ClientSize.Width - groupBox.Width;
            int maxY = panelContainer.ClientSize.Height - groupBox.Height;
            if (maxX < 0) maxX = 0;
            if (maxY < 0) maxY = 0;
            newLocation.X = Math.Max(0, Math.Min(newLocation.X, maxX));
            newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, maxY));
            return newLocation;
        }

        private AppState CreateStateSnapshot()
        {
            return new AppState
            {
                Units = CloneUnits(Units),
                Groups = CloneGroups(Groups)
            };
        }

        private void RestoreState(AppState state)
        {
            Units = CloneUnits(state.Units);
            Groups = CloneGroups(state.Groups);
        }

        private static Dictionary<string, UnitStruct> CloneUnits(Dictionary<string, UnitStruct> source)
        {
            var result = new Dictionary<string, UnitStruct>(source.Count);
            foreach (var kvp in source)
            {
                var unit = kvp.Value;
                unit.Tags = unit.Tags?.ToArray();
                result[kvp.Key] = unit;
            }
            return result;
        }

        private static Dictionary<string, GroupStruct> CloneGroups(Dictionary<string, GroupStruct> source)
        {
            var result = new Dictionary<string, GroupStruct>(source.Count);
            foreach (var kvp in source)
                result[kvp.Key] = kvp.Value;
            return result;
        }

        private static string GetContentSummary(UnitStruct unit)
        {
            switch ((unit.ContentType ?? "Text").ToLowerInvariant())
            {
                case "image":
                    return "Image";
                case "object":
                    string summary;
                    if (ClipboardHelper.TryDescribeObject(unit.ContentData, out summary))
                    {
                        return string.IsNullOrWhiteSpace(summary) ? "Object" : summary;
                    }
                    return "Object";
                default:
                    var text = unit.Content;
                    if (string.IsNullOrWhiteSpace(text))
                        return string.Empty;
                    return text.Length > 200 ? text.Substring(0, 200) + "…" : text;
            }
        }

        public static string GetContentSummaryFor(UnitStruct unit) => GetContentSummary(unit);

        private void ApplyDefaultStyleToAllNotesInternal()
        {
            SaveStateForUndo();
            var style = NotesLibrary.Instance.Config.DefaultUnitStyle;
            var oldFonts = new HashSet<Font>();
            foreach (var unit in Units.Values)
            {
                if (unit.Font != null)
                    oldFonts.Add(unit.Font);
            }

            foreach (var key in Units.Keys.ToList())
            {
                var unit = Units[key];
                var oldFont = unit.Font;
                Font newFont;
                try
                {
                    newFont = new Font(style.FontFamily, style.FontSize, style.FontStyle);
                }
                catch
                {
                    newFont = NotesLibrary.Instance.GetDefaultFont();
                }
                unit.BackgroundColor = style.BackgroundColor;
                unit.TextColor = style.TextColor;
                unit.Font = newFont;
                Units[key] = unit;

                var btn = FindButtonById(key);
                if (btn != null)
                {
                    ApplyUnitChangesToButton(btn, unit);
                }
            }

            var usedFonts = new HashSet<Font>();
            foreach (var unit in Units.Values)
            {
                if (unit.Font != null)
                    usedFonts.Add(unit.Font);
            }
            foreach (var btn in GetAllButtonsInPanel())
            {
                if (btn.Font != null)
                    usedFonts.Add(btn.Font);
            }
            foreach (var oldFont in oldFonts)
            {
                if (!usedFonts.Contains(oldFont))
                    oldFont.Dispose();
            }

            configModified = true;
            ClearSelection();
            status = "Default style applied to all notes";
            UpdateUndoRedoMenuState();
        }
    }
}

public class DoubleClickButton : Button
{
    public DoubleClickButton()
    {
        SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
    }
}

public class ResizableGroupBox : GroupBox
{
    private bool isResizing = false;
    private Point resizeStart;
    private Size originalSize;
    private const int RESIZE_HANDLE_SIZE = 16;
    private Panel resizeHandlePanel;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool AllowResize { get; set; } = true;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool IsResizing => isResizing;

    public ResizableGroupBox()
    {
        this.DoubleBuffered = true;
        CreateResizeHandle();
    }

    private void CreateResizeHandle()
    {
        resizeHandlePanel = new Panel
        {
            Width = RESIZE_HANDLE_SIZE,
            Height = RESIZE_HANDLE_SIZE,
            BackColor = Color.Transparent,
            Cursor = Cursors.SizeNWSE,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

        resizeHandlePanel.Paint += ResizeHandlePanel_Paint;
        resizeHandlePanel.MouseDown += ResizeHandlePanel_MouseDown;
        resizeHandlePanel.MouseMove += ResizeHandlePanel_MouseMove;
        resizeHandlePanel.MouseUp += ResizeHandlePanel_MouseUp;

        this.Controls.Add(resizeHandlePanel);
        PositionResizeHandle();
    }
    
    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        // Ensure resize handle stays on top
        if (resizeHandlePanel != null && AllowResize && e.Control != resizeHandlePanel)
        {
            resizeHandlePanel.BringToFront();
        }
    }

    private void PositionResizeHandle()
    {
        if (resizeHandlePanel != null)
        {
            resizeHandlePanel.Location = new Point(
                this.Width - RESIZE_HANDLE_SIZE - 2,
                this.Height - RESIZE_HANDLE_SIZE - 2);
            resizeHandlePanel.Visible = AllowResize;
            if (AllowResize)
            {
                resizeHandlePanel.BringToFront();
            }
        }
    }

    private void ResizeHandlePanel_Paint(object sender, PaintEventArgs e)
    {
        if (!AllowResize) return;

        // Draw resize handle background
        using (SolidBrush handleBrush = new SolidBrush(Color.FromArgb(150, this.ForeColor)))
        {
            e.Graphics.FillRectangle(handleBrush, 0, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE);
        }

        // Draw grip lines
        using (Pen gripPen = new Pen(Color.FromArgb(200, this.Parent?.BackColor ?? SystemColors.Control), 2))
        {
            e.Graphics.DrawLine(gripPen, 4, 12, 12, 4);
            e.Graphics.DrawLine(gripPen, 7, 12, 12, 7);
            e.Graphics.DrawLine(gripPen, 10, 12, 12, 10);
        }
    }

    private void ResizeHandlePanel_MouseDown(object sender, MouseEventArgs e)
    {
        if (!AllowResize || e.Button != MouseButtons.Left)
            return;

        isResizing = true;
        resizeStart = this.PointToClient(Cursor.Position);
        originalSize = this.Size;
    }

    private void ResizeHandlePanel_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isResizing)
            return;

        Point currentMouse = this.PointToClient(Cursor.Position);
        int newWidth = originalSize.Width + (currentMouse.X - resizeStart.X);
        int newHeight = originalSize.Height + (currentMouse.Y - resizeStart.Y);

        newWidth = Math.Max(150, newWidth);
        newHeight = Math.Max(100, newHeight);

        Rectangle oldBounds = this.Bounds;
        this.Size = new Size(newWidth, newHeight);
        
        // Invalidate old and new areas to prevent artifacts
        if (this.Parent != null)
        {
            this.Parent.Invalidate(oldBounds);
            this.Parent.Invalidate(this.Bounds);
            this.Parent.Update();
        }
    }

    private void ResizeHandlePanel_MouseUp(object sender, MouseEventArgs e)
    {
        if (!isResizing)
            return;

        isResizing = false;
        
        // Notify parent that resize is complete
        OnSizeChanged(EventArgs.Empty);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        PositionResizeHandle();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        PositionResizeHandle();
    }

    public void UpdateResizeHandleVisibility()
    {
        if (resizeHandlePanel != null)
        {
            resizeHandlePanel.Visible = AllowResize;
        }
    }
}

