using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace Notes
{
    public partial class frmAddGroup : Form
    {
        public static frmMain.GroupStruct selectedGroup = new frmMain.GroupStruct();
        public static bool selectedGroupModified = false;
        
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsEditMode { get; set; } = false;

        private Color borderColor = Color.Black;
        private Color backgroundColor = Color.WhiteSmoke;
        private Color textColor = Color.Black;

        public frmAddGroup()
        {
            InitializeComponent();
            InitializeModernUI();
        }

        private void InitializeModernUI()
        {
            Icon = Properties.Resources.Notes;
            
            // Set window properties
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9f);
            
            // Apply theme styling
            var config = NotesLibrary.Instance.Config;
            ThemeManager.ApplyTheme(this, config.General.Theme);
        }

        private void frmAddGroup_Load(object sender, EventArgs e)
        {
            if (IsEditMode && !string.IsNullOrEmpty(selectedGroup.Id))
            {
                // Load existing group data
                tbTitle.Text = selectedGroup.Title;
                numX.Value = selectedGroup.X;
                numY.Value = selectedGroup.Y;
                numWidth.Value = selectedGroup.Width;
                numHeight.Value = selectedGroup.Height;
                
                borderColor = selectedGroup.BorderColor != 0 ? Color.FromArgb(selectedGroup.BorderColor) : Color.Black;
                backgroundColor = selectedGroup.BackgroundColor != 0 ? Color.FromArgb(selectedGroup.BackgroundColor) : Color.WhiteSmoke;
                textColor = selectedGroup.TextColor != 0 ? Color.FromArgb(selectedGroup.TextColor) : Color.Black;
                
                this.Text = "Edit Group";
            }
            else
            {
                // Set default values for new group
                numX.Value = 50;
                numY.Value = 50;
                numWidth.Value = 300;
                numHeight.Value = 200;
                this.Text = "Add New Group";
            }
            
            UpdateColorButtons();
            
            // Set focus to title
            tbTitle.Focus();
        }

        private void UpdateColorButtons()
        {
            btnBorderColor.BackColor = borderColor;
            btnBorderColor.ForeColor = GetContrastColor(borderColor);
            
            btnBackgroundColor.BackColor = backgroundColor;
            btnBackgroundColor.ForeColor = GetContrastColor(backgroundColor);
            
            btnTextColor.BackColor = textColor;
            btnTextColor.ForeColor = GetContrastColor(textColor);
        }

        private Color GetContrastColor(Color color)
        {
            // Calculate relative luminance
            double luminance = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
            return luminance > 128 ? Color.Black : Color.White;
        }

        private bool ValidateInput()
        {
            // Clear any previous error styling
            tbTitle.BackColor = SystemColors.Window;
            
            if (string.IsNullOrWhiteSpace(tbTitle.Text))
            {
                tbTitle.BackColor = Color.LightPink;
                MessageBox.Show("Please enter a title for the group.", NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbTitle.Focus();
                return false;
            }

            if (tbTitle.Text.Length > 100)
            {
                tbTitle.BackColor = Color.LightPink;
                MessageBox.Show("Title cannot exceed 100 characters.", NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbTitle.Focus();
                return false;
            }

            if (numWidth.Value < 100)
            {
                MessageBox.Show("Group width must be at least 100 pixels.", NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numWidth.Focus();
                return false;
            }

            if (numHeight.Value < 80)
            {
                MessageBox.Show("Group height must be at least 80 pixels.", NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numHeight.Focus();
                return false;
            }

            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            try
            {
                // Generate new ID if creating a new group
                if (!IsEditMode || string.IsNullOrEmpty(selectedGroup.Id))
                {
                    selectedGroup.Id = NotesLibrary.Instance.GenerateId();
                }

                var main = frmMain.Instance;
                Size panelSize = main?.PanelSize ?? Size.Empty;
                if (panelSize.Width <= 0 || panelSize.Height <= 0)
                {
                    panelSize = (main != null ? Screen.FromControl(main) : Screen.PrimaryScreen)?.WorkingArea.Size ?? new Size(800, 600);
                }
                int clampedWidth = Math.Max(100, Math.Min((int)numWidth.Value, panelSize.Width));
                int clampedHeight = Math.Max(80, Math.Min((int)numHeight.Value, panelSize.Height));
                numWidth.Value = clampedWidth;
                numHeight.Value = clampedHeight;

                int maxX = Math.Max(0, panelSize.Width - clampedWidth);
                int maxY = Math.Max(0, panelSize.Height - clampedHeight);
                numX.Value = Math.Min(Math.Max(numX.Value, 0), maxX);
                numY.Value = Math.Min(Math.Max(numY.Value, 0), maxY);

                string title = tbTitle.Text.Trim();
                var existing = frmMain.GetGroups();
                if (!string.IsNullOrEmpty(title))
                {
                    var used = existing
                        .Where(g => g.Key != selectedGroup.Id)
                        .Select(g => g.Value.Title ?? string.Empty)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    if (used.Contains(title))
                    {
                        string baseTitle = title;
                        int i = 2;
                        while (used.Contains(title))
                        {
                            title = $"{baseTitle} ({i})";
                            i++;
                        }
                    }
                }
                selectedGroup.Title = title;
                selectedGroup.X = (int)numX.Value;
                selectedGroup.Y = (int)numY.Value;
                selectedGroup.Width = (int)numWidth.Value;
                selectedGroup.Height = (int)numHeight.Value;
                selectedGroup.BorderColor = borderColor.ToArgb();
                selectedGroup.BackgroundColor = backgroundColor.ToArgb();
                selectedGroup.TextColor = textColor.ToArgb();
                
                // Set default GroupBoxType if not already set
                if (string.IsNullOrEmpty(selectedGroup.GroupBoxType))
                {
                    selectedGroup.GroupBoxType = "ResizableGroupBox";
                }

                selectedGroupModified = true;
                
                this.DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving group: " + ex.Message, NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        // Add keyboard shortcuts
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
                case Keys.Control | Keys.Enter:
                    btnSave_Click(null, null);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnBorderColor_Click(object sender, EventArgs e)
        {
            colorDialog.Color = borderColor;
            colorDialog.FullOpen = true;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                borderColor = colorDialog.Color;
                UpdateColorButtons();
            }
        }

        private void btnBackgroundColor_Click(object sender, EventArgs e)
        {
            colorDialog.Color = backgroundColor;
            colorDialog.FullOpen = true;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                backgroundColor = colorDialog.Color;
                UpdateColorButtons();
            }
        }

        private void btnTextColor_Click(object sender, EventArgs e)
        {
            colorDialog.Color = textColor;
            colorDialog.FullOpen = true;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                textColor = colorDialog.Color;
                UpdateColorButtons();
            }
        }
    }
}

