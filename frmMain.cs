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
                SaveStateForUndo();
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
            Button newButton = new DoubleClickButton();
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
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    Button btn = owner.SourceControl as Button;
                    if (btn != null)
                    {
                        return new ContextMenuInfo { Button = btn, Id = (string)btn.Tag };
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
                    selectedUnitModified = false;

                    Units[id] = selectedUnit;
                    btn.Text = selectedUnit.Title;
                    btn.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                    btn.ForeColor = Color.FromArgb(selectedUnit.TextColor);
                    btn.Font = selectedUnit.Font;
                    btn.Location = new Point(selectedUnit.X, selectedUnit.Y);

                    configModified = true;
                    status = "Updated successfully";
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
                selectedUnitModified = false;
                var id = getNewId();

                Units.Add(id, selectedUnit);
                addButton(id, selectedUnit);
                selectedUnit = new UnitStruct();

                configModified = true;
                status = "New button added";
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
                panelContainer.Controls.Clear();
                Units.Clear();

                configModified = true;
                status = "All buttons deleted successfully";
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

                        foreach (var keyValuePair in newUnits)
                        {
                            var id = getNewId();
                            Units.Add(id, keyValuePair.Value);
                            addButton(id, keyValuePair.Value);
                        }

                        configModified = true;
                        status = string.Format("{0} notes imported successfully", newUnits.Count());
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
                        selectedUnitModified = false;

                        Units[id] = selectedUnit;
                        btn.Text = selectedUnit.Title;
                        btn.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                        btn.ForeColor = Color.FromArgb(selectedUnit.TextColor);
                        btn.Font = selectedUnit.Font;
                        btn.Location = new Point(selectedUnit.X, selectedUnit.Y);

                        configModified = true;
                        status = "Updated successfully";
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
                    Units.Remove(id);
                    panelContainer.Controls.Remove(btn);

                    configModified = true;
                    status = "Deleted successfully";
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
                        selectedUnitModified = false;
                        id = getNewId();

                        Units.Add(id, selectedUnit);
                        addButton(id, selectedUnit);

                        configModified = true;
                        status = "New button added";
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
                    var unit = Units[id];
                    unit.BackgroundColor = copiedUnit.Value.BackgroundColor;
                    unit.TextColor = copiedUnit.Value.TextColor;
                    unit.Font = copiedUnit.Value.Font;
                    Units[id] = unit;

                    btn.BackColor = Color.FromArgb(copiedUnit.Value.BackgroundColor);
                    btn.ForeColor = Color.FromArgb(copiedUnit.Value.TextColor);
                    btn.Font = copiedUnit.Value.Font;

                    configModified = true;
                    status = "Copy successful";
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
    }
}

public class DoubleClickButton : Button
{
    public DoubleClickButton()
    {
        SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
    }
}

