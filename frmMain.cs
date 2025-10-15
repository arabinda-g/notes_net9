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
            public string Content;
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
        }

        // Status bar management
        private string status
        {
            set
            {
                tmrStatus.Stop();
                statusLabel.Text = value;
                tmrStatus.Enabled = true;
                
                // Auto-hide status after 3 seconds
                System.Windows.Forms.Timer hideTimer = new System.Windows.Forms.Timer { Interval = 3000 };
                hideTimer.Tick += (s, e) => {
                    statusLabel.Text = string.Format("Ready - {0} notes", Units.Count);
                    hideTimer.Stop();
                    hideTimer.Dispose();
                };
                hideTimer.Start();
            }
        }

        private Dictionary<string, UnitStruct> Units = new Dictionary<string, UnitStruct>();
        private List<string> searchResults = new List<string>();
        private Stack<Dictionary<string, UnitStruct>> undoStack = new Stack<Dictionary<string, UnitStruct>>();
        private Stack<Dictionary<string, UnitStruct>> redoStack = new Stack<Dictionary<string, UnitStruct>>();

        public static UnitStruct selectedUnit = new UnitStruct();
        public static bool selectedUnitModified = false;
        private bool configModified = false;
        private bool isMovable = false;
        private bool isAutofocus = false;
        private bool autoSaveEnabled = true;

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
            InitializeComponent();
            InitializeCustomComponents();
            SetupAutoSave();
            LoadConfiguration();
            SetupSystemThemeMonitoring();
            RegisterGlobalHotkey();
            
            // Enable key preview to catch keyboard events
            this.KeyPreview = true;
            this.KeyDown += frmMain_KeyDown;
        }

        private void InitializeCustomComponents()
        {
            Icon = Properties.Resources.Notes;
            trayIcon.Icon = Icon;
            tmrClickHandle.Interval = SystemInformation.DoubleClickTime;

            // Apply theme styling
            ApplyCurrentTheme();
            
            // Setup events
            this.Resize += frmMain_Resize;
            this.FormClosing += frmMain_FormClosing;
            panelContainer.MouseUp += panelContainer_MouseUp;
            panelContainer.MouseDown += panelContainer_MouseDown;
            panelContainer.MouseMove += panelContainer_MouseMove;
            panelContainer.Paint += panelContainer_Paint;
            
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
            autoSaveTimer.Interval = config.General.AutoSaveInterval * 1000;
            
            // Update menu checkbox state
            menuEditAutoSave.Checked = autoSaveEnabled;
            
            if (autoSaveEnabled)
            {
                autoSaveTimer.Start();
            }
            
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
                var config = NotesLibrary.Instance.Config;
                
                if (!config.Hotkey.Enabled)
                    return;

                // Calculate modifier flags
                int modifiers = 0;
                foreach (var modifier in config.Hotkey.Modifiers)
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

                // Register the hotkey
                bool success = Win32.RegisterHotKey(this.Handle, HOTKEY_ID, modifiers, (int)config.Hotkey.Key);
                
                if (!success)
                {
                    // Hotkey registration failed (might be already in use)
                    System.Diagnostics.Debug.WriteLine("Failed to register global hotkey");
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
                Win32.UnregisterHotKey(this.Handle, HOTKEY_ID);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unregistering hotkey: {ex.Message}");
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                // Global hotkey was pressed - bring window to front
                ActivateAndBringToFront();
            }
        }

        private void ActivateAndBringToFront()
        {
            try
            {
                // If minimized, restore
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
                
                // Show the form if it's hidden
                if (!this.Visible)
                {
                    this.Show();
                }
                
                // Bring to front and activate
                this.Activate();
                this.BringToFront();
                this.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error activating window: {ex.Message}");
            }
        }

        private void RestoreWindowState()
        {
            try
            {
                var config = NotesLibrary.Instance.Config;
                
                // Restore window size
                int width = Properties.Settings.Default.WindowWidth;
                int height = Properties.Settings.Default.WindowHeight;
                
                // Ensure minimum window size
                if (width < 400) width = 800;
                if (height < 300) height = 600;
                
                this.Size = new Size(width, height);
                
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
                
                // Apply startup window state from configuration
                this.WindowState = config.Window.State;
                
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
                
                // Save maximized state
                Properties.Settings.Default.WindowMaximized = (this.WindowState == FormWindowState.Maximized);
                
                // If maximized, we need to get the restored bounds
                if (this.WindowState == FormWindowState.Maximized)
                {
                    Properties.Settings.Default.WindowWidth = this.RestoreBounds.Width;
                    Properties.Settings.Default.WindowHeight = this.RestoreBounds.Height;
                    Properties.Settings.Default.WindowX = this.RestoreBounds.X;
                    Properties.Settings.Default.WindowY = this.RestoreBounds.Y;
                }
                else
                {
                    // Window is in normal state
                    Properties.Settings.Default.WindowWidth = this.Width;
                    Properties.Settings.Default.WindowHeight = this.Height;
                    Properties.Settings.Default.WindowX = this.Left;
                    Properties.Settings.Default.WindowY = this.Top;
                }
                
                // Save the settings
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // Silently fail if we can't save window state
                // The application should continue to work normally
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
            // Restore window size and position
            RestoreWindowState();
            
            ResizePanel();
            
            if (string.IsNullOrEmpty(Properties.Settings.Default.JsonData))
            {
                status = "No saved notes found - Create your first note by right-clicking";
                CreateWelcomeNote();
            }
            else
            {
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
                Content = "Welcome to your improved Notes application!\n\n" +
                         "• Right-click to create new notes\n" +
                         "• Single-click to copy content\n" +
                         "• Double-click to edit\n" +
                         "• Drag notes around when movable mode is enabled\n" +
                         "• Use Ctrl+F to search your notes\n" +
                         "• Auto-save is enabled by default\n\n" +
                         "Enjoy your organized note-taking!",
                BackgroundColor = Color.LightSteelBlue.ToArgb(),
                TextColor = Color.DarkBlue.ToArgb(),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                X = 50,
                Y = 50,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Category = "System",
                Tags = new string[] { "welcome", "help" }
            };
            
            var id = NotesLibrary.Instance.GenerateId();
            Units.Add(id, welcomeNote);
            addButton(id, welcomeNote);
            
            configModified = true;
            status = "Welcome note created!";
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            ResizePanel();
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
            foreach (Control control in panelContainer.Controls)
            {
                if (control is Button btn)
                {
                    SelectButton(btn);
                }
            }
            status = $"Selected {selectedButtons.Count} buttons";
        }

        private void DeleteSelectedButtons()
        {
            if (selectedButtons.Count == 0)
                return;

            DialogResult result = MessageBox.Show(
                $"Do you want to delete {selectedButtons.Count} selected button(s)?", 
                AppName, 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SaveStateForUndo();
                var buttonsToDelete = selectedButtons.ToList();
                
                foreach (var btn in buttonsToDelete)
                {
                    string id = (string)btn.Tag;
                    if (Units.ContainsKey(id))
                    {
                        Units.Remove(id);
                        panelContainer.Controls.Remove(btn);
                    }
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
            if (autoSaveTimer != null)
                autoSaveTimer.Stop();
            
            // Unregister global hotkey
            UnregisterGlobalHotkey();
            
            // Stop monitoring system theme changes
            ThemeManager.StopSystemThemeMonitoring();
            
            // Save window state before closing
            if (!e.Cancel)
                SaveWindowState();
            
            if (configModified)
            {
                var config = NotesLibrary.Instance.Config;
                if (config.General.AutoSave)
                {
                    saveJson();
                    status = "Auto-saved before closing";
                }
                else
                {
                    DialogResult result = MessageBox.Show("Do you want to save changes?", AppName, 
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        saveJson();
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
                SaveWindowState();
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (configModified && autoSaveEnabled)
            {
                saveJson();
                status = "Auto-saved";
            }
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            // TODO: Implement search functionality
            searchTimer.Stop();
        }

        private string getNewId()
        {
            return NotesLibrary.Instance.GenerateId();
        }

        private void SaveStateForUndo()
        {
            // Deep clone current state for undo
            var currentState = JsonConvert.DeserializeObject<Dictionary<string, UnitStruct>>(
                JsonConvert.SerializeObject(Units));
            undoStack.Push(currentState);
            
            // Limit undo history to 20 actions
            if (undoStack.Count > 20)
            {
                var temp = undoStack.ToArray().Take(20).Reverse().ToArray();
                undoStack.Clear();
                foreach (var item in temp)
                    undoStack.Push(item);
            }
            
            redoStack.Clear(); // Clear redo stack when new action is performed
        }

        public void PerformUndo()
        {
            if (undoStack.Count > 0)
            {
                // Save current state to redo stack
                var currentState = JsonConvert.DeserializeObject<Dictionary<string, UnitStruct>>(
                    JsonConvert.SerializeObject(Units));
                redoStack.Push(currentState);
                
                // Restore previous state
                Units = undoStack.Pop();
                RefreshAllButtons();
                configModified = true;
                status = "Undo successful";
            }
        }

        public void PerformRedo()
        {
            if (redoStack.Count > 0)
            {
                // Save current state to undo stack WITHOUT clearing redo stack
                var currentState = JsonConvert.DeserializeObject<Dictionary<string, UnitStruct>>(
                    JsonConvert.SerializeObject(Units));
                undoStack.Push(currentState);
                
                // Restore state from redo stack
                Units = redoStack.Pop();
                RefreshAllButtons();
                configModified = true;
                status = "Redo successful";
            }
        }

        private void RefreshAllButtons()
        {
            ClearSelection();
            panelContainer.Controls.Clear();
            foreach (var kvp in Units)
            {
                addButton(kvp.Key, kvp.Value);
            }
            UpdateUndoRedoMenuState();
        }

        private bool loadJson(string json)
        {
            status = "Loading notes...";

            try
            {
                panelContainer.Controls.Clear();
                var newUnits = JsonConvert.DeserializeObject<Dictionary<string, UnitStruct>>(json);

                if (newUnits == null)
                {
                    status = "No notes found";
                    return false;
                }
                else
                {
                    Units = newUnits;

                    foreach (var keyValuePair in Units)
                    {
                        addButton(keyValuePair.Key, keyValuePair.Value);
                    }

                    status = string.Format("Loaded {0} notes successfully", Units.Count);
                    return true;
                }
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
            Button newButton = CreateButtonByType(unit.ButtonType);
            newButton.Tag = id;
            newButton.AutoSize = true;
            newButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            newButton.Text = unit.Title;
            newButton.BackColor = Color.FromArgb(unit.BackgroundColor);
            newButton.ForeColor = Color.FromArgb(unit.TextColor);
            newButton.Font = unit.Font ?? NotesLibrary.Instance.GetDefaultFont();
            newButton.Location = new Point(unit.X, unit.Y);
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

            panelContainer.Controls.Add(newButton);
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

        private void saveJson()
        {
            try
            {
                Properties.Settings.Default.JsonData = JsonConvert.SerializeObject(Units, Formatting.Indented);
                Properties.Settings.Default.configAutofocus = isAutofocus;
                
                // Also save window state when saving other data
                SaveWindowState();
                
                configModified = false;
                status = "Saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving notes: " + ex.Message, AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }











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

        private bool saveButtonLocation(Button btn)
        {
            string id = (string)btn.Tag;

            if (Units.ContainsKey(id))
            {
                var item = Units[id];
                item.X = btn.Location.X;
                item.Y = btn.Location.Y;
                Units[id] = item;
                configModified = true;
                status = "Moving successful";
                return true;
            }
            else
            {
                status = "Button not found";
            }

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
                    saveButtonLocation(btn);
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
                    saveButtonLocation(sender as Button);
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
                
                foreach (var selectedBtn in selectedButtons)
                {
                    if (groupOriginalPositions.ContainsKey(selectedBtn))
                    {
                        selectedBtn.Location = new Point(
                            groupOriginalPositions[selectedBtn].X + deltaX,
                            groupOriginalPositions[selectedBtn].Y + deltaY
                        );
                    }
                }
                status = "Moving selected buttons";
            }
            else if (isMovable && this.BtnDragging)
            {
                Button btn = sender as Button;
                btn.Left = this.Origin_Control.X - (this.Origin_Cursor.X - Cursor.Position.X);
                btn.Top = this.Origin_Control.Y - (this.Origin_Cursor.Y - Cursor.Position.Y);
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
                saveButtonLocation(sender as Button);
            }
        }









        private void unit_Click_Handle(object sender, EventArgs e)
        {
            Button ct = sender as Button;
            string id = (string)ct.Tag;

            //Check if key exists
            if (Units.ContainsKey(id))
            {
                Clipboard.SetText(Units[id].Content);
                status = "Copied to clipboard";
            }
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
                Form editForm = new frmEdit();
                editForm.ShowDialog();

                if (selectedUnitModified)
                {
                    SaveStateForUndo();
                    selectedUnitModified = false;

                    Units[id] = selectedUnit;
                    btn.Text = selectedUnit.Title;
                    btn.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                    btn.ForeColor = Color.FromArgb(selectedUnit.TextColor);
                    btn.Font = selectedUnit.Font;
                    btn.Location = new Point(selectedUnit.X, selectedUnit.Y);

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
                if (_unitDoubleClicked)
                {
                    unit_DoubleClick_Handle(_unitClickSender, _unitClickE);
                }
                else
                {
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
            }
            selectedButtons.Clear();
        }

        private void UpdateButtonSelectionVisual(Button btn, bool isSelected)
        {
            if (isSelected)
            {
                // Add visual indicator for selection
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.DodgerBlue;
                btn.FlatAppearance.BorderSize = 3;
            }
            else
            {
                // Remove selection visual
                btn.FlatStyle = FlatStyle.Standard;
            }
        }






        private void menuFileNew_Click(object sender, EventArgs e)
        {
            Form addForm = new frmAdd();
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

        private void menuFileSave_Click(object sender, EventArgs e)
        {
            saveJson();
            status = "Saved successfully";
        }

        private void menuFileReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to delete all buttons?", AppName, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                SaveStateForUndo();
                panelContainer.Controls.Clear();
                Units.Clear();

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
                loadJson(Properties.Settings.Default.JsonData);
                loadConfig();
                configModified = false;
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
                    string json = File.ReadAllText(openFileDialog.FileName);
                    //loadJson(json);

                    try
                    {
                        var newUnits = JsonConvert.DeserializeObject<Dictionary<string, UnitStruct>>(json);

                        SaveStateForUndo();
                        foreach (var keyValuePair in newUnits)
                        {
                            var id = getNewId();
                            Units.Add(id, keyValuePair.Value);
                            addButton(id, keyValuePair.Value);
                        }

                        configModified = true;
                        status = string.Format("{0} notes imported successfully", newUnits.Count());
                        UpdateUndoRedoMenuState();
                    }
                    catch { }
                }

            }
        }

        private void menuFileExport_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "Text Documents (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Stream stream;

                if ((stream = saveFileDialog.OpenFile()) != null)
                {
                    StreamWriter writer = new StreamWriter(stream);
                    //writer.Write(Properties.Settings.Default.JsonData);
                    writer.Write(JsonConvert.SerializeObject(Units));
                    writer.Close();

                    status = string.Format("{0} notes exported successfully", Units.Count());
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

            if (item.Checked)
            {
                status = "Movable buttons";
            }
            else
            {
                status = "Non-movable buttons";
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
                    Form editForm = new frmEdit();
                    editForm.ShowDialog();

                    if (selectedUnitModified)
                    {
                        SaveStateForUndo();
                        selectedUnitModified = false;

                        Units[id] = selectedUnit;
                        btn.Text = selectedUnit.Title;
                        btn.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                        btn.ForeColor = Color.FromArgb(selectedUnit.TextColor);
                        btn.Font = selectedUnit.Font;
                        btn.Location = new Point(selectedUnit.X, selectedUnit.Y);

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
                    SaveStateForUndo();
                    Units.Remove(id);
                    panelContainer.Controls.Remove(btn);

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
                    Form editForm = new frmEdit();
                    editForm.ShowDialog();

                    if (selectedUnitModified)
                    {
                        SaveStateForUndo();
                        selectedUnitModified = false;
                        id = getNewId();

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
                        panelContainer.Controls.Remove(btn);
                        btn.Dispose();
                        
                        // Create new button with correct type
                        Button newBtn = CreateButtonByType(copiedUnit.Value.ButtonType);
                        newBtn.Tag = id;
                        newBtn.Text = unit.Title;
                        newBtn.BackColor = Color.FromArgb(copiedUnit.Value.BackgroundColor);
                        newBtn.ForeColor = Color.FromArgb(copiedUnit.Value.TextColor);
                        newBtn.Font = copiedUnit.Value.Font;
                        newBtn.Location = location;
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
                        
                        panelContainer.Controls.Add(newBtn);
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
            //Get clicked item
            var item = getContextMenuInfo(sender);

            if (item != null)
            {
                var btn = item.Button;
                var id = item.Id;

                if (Units.ContainsKey(id))
                {
                    // Load the selected button
                    selectedUnit = Units[id];

                    Clipboard.SetText(selectedUnit.Content.ToLower());
                    status = "Copied to clipboard in lowercase";
                }
            }
        }

        private void unitMenuCopyInUppercase_Click(object sender, EventArgs e)
        {
            //Get clicked item
            var item = getContextMenuInfo(sender);

            if (item != null)
            {
                var btn = item.Button;
                var id = item.Id;

                if (Units.ContainsKey(id))
                {
                    // Load the selected button
                    selectedUnit = Units[id];

                    Clipboard.SetText(selectedUnit.Content.ToUpper());
                    status = "Copied to clipboard in uppercase";
                }
            }
        }








        private void tmrStatus_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = null;
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

            var buttons = panelContainer.Controls.OfType<Button>().ToList();
            
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
                var btn = panelContainer.Controls.OfType<Button>()
                    .FirstOrDefault(b => (string)b.Tag == id);
                    
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
                var btn = panelContainer.Controls.OfType<Button>()
                    .FirstOrDefault(b => (string)b.Tag == id);
                    
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
            
            var buttons = panelContainer.Controls.OfType<Button>().OrderBy(b => 
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

            var buttons = panelContainer.Controls.OfType<Button>().ToList();
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
                buttonsToStyle = panelContainer.Controls.OfType<Button>().ToList();
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
                        panelContainer.Controls.Remove(btn);
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

                panelContainer.Controls.Add(newBtn);
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
                buttonsToStyle = panelContainer.Controls.OfType<Button>().ToList();
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
                buttonsToReplace = panelContainer.Controls.OfType<Button>().ToList();
            }

            if (buttonsToReplace.Count == 0)
            {
                status = "No buttons to style";
                return;
            }

            SaveStateForUndo();

            // Suspend layout to prevent flickering
            panelContainer.SuspendLayout();

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

                    // Store old button's rectangle for invalidation
                    Rectangle oldBounds = oldBtn.Bounds;

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

                    // Remove old button from panel
                    panelContainer.Controls.Remove(oldBtn);
                    
                    // Dispose old button to free resources
                    oldBtn.Dispose();

                    // Add new button
                    panelContainer.Controls.Add(newBtn);
                    
                    // Add to selection if it was selected
                    if (buttonsToReplace.Contains(oldBtn))
                    {
                        selectedButtons.Add(newBtn);
                    }

                    newButtons.Add(newBtn);

                    // Invalidate the old area to clear artifacts
                    panelContainer.Invalidate(oldBounds);
                }
            }

            // Resume layout and refresh
            panelContainer.ResumeLayout(true);
            panelContainer.Refresh();

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
    }
}

public class DoubleClickButton : Button
{
    public DoubleClickButton()
    {
        SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
    }
}

