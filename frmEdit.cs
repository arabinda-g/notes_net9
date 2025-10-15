using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notes
{
    public partial class frmEdit : Form
    {
        private static frmMain.UnitStruct selectedUnit = new frmMain.UnitStruct();
        private frmMain.UnitStruct originalUnit;
        private bool hasChanges = false;

        public frmEdit()
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
            
            // Add change tracking
            this.FormClosing += FrmEdit_FormClosing;
        }

        private void frmEdit_Load(object sender, EventArgs e)
        {
            selectedUnit = frmMain.selectedUnit;
            originalUnit = selectedUnit; // Store original for comparison
            
            // Load data
            tbTitle.Text = selectedUnit.Title;
            tbContent.Text = selectedUnit.Content;
            numX.Value = selectedUnit.X;
            numY.Value = selectedUnit.Y;
            
            // Initialize styling
            UpdateColorButtons();
            
            // Set focus
            tbTitle.Focus();
            tbTitle.SelectAll();
            
            // Update window title
            this.Text = string.Format("Edit Note - {0}", selectedUnit.Title);
            
            // Add change tracking events
            tbTitle.TextChanged += (s, args) => MarkAsChanged();
            tbContent.TextChanged += (s, args) => MarkAsChanged();
            numX.ValueChanged += (s, args) => MarkAsChanged();
            numY.ValueChanged += (s, args) => MarkAsChanged();
        }

        private void MarkAsChanged()
        {
            if (!hasChanges)
            {
                hasChanges = true;
                this.Text = this.Text.TrimEnd('*') + "*"; // Add asterisk to indicate changes
            }
        }

        private void UpdateColorButtons()
        {
            try
            {
                btnBackgroundColor.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                btnTextColor.BackColor = Color.FromArgb(selectedUnit.TextColor);
                btnFont.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                btnFont.ForeColor = Color.FromArgb(selectedUnit.TextColor);
                
                if (selectedUnit.Font != null)
                {
                    btnFont.Text = string.Format("{0}, {1}pt", selectedUnit.Font.Name, selectedUnit.Font.SizeInPoints);
                }
                else
                {
                    selectedUnit.Font = NotesLibrary.Instance.GetDefaultFont();
                    btnFont.Text = string.Format("{0}, {1}pt", selectedUnit.Font.Name, selectedUnit.Font.SizeInPoints);
                }
            }
            catch (Exception)
            {
                // Fallback to defaults if color conversion fails
                var config = NotesLibrary.Instance.Config;
                selectedUnit.BackgroundColor = config.DefaultUnitStyle.BackgroundColor;
                selectedUnit.TextColor = config.DefaultUnitStyle.TextColor;
                selectedUnit.Font = NotesLibrary.Instance.GetDefaultFont();
                UpdateColorButtons();
            }
        }

        private void btnBackgroundColor_Click(object sender, EventArgs e)
        {
            try
            {
                colorDialog.Color = Color.FromArgb(selectedUnit.BackgroundColor);
                colorDialog.FullOpen = true;
                DialogResult result = colorDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    selectedUnit.BackgroundColor = colorDialog.Color.ToArgb();
                    UpdateColorButtons();
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting background color: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnTextColor_Click(object sender, EventArgs e)
        {
            try
            {
                colorDialog.Color = Color.FromArgb(selectedUnit.TextColor);
                colorDialog.FullOpen = true;
                DialogResult result = colorDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    selectedUnit.TextColor = colorDialog.Color.ToArgb();
                    UpdateColorButtons();
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting text color: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            try
            {
                fontDialog.Font = selectedUnit.Font;
                fontDialog.ShowColor = false;
                DialogResult result = fontDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    selectedUnit.Font = fontDialog.Font;
                    UpdateColorButtons();
                    MarkAsChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting font: " + ex.Message, 
                    NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateInput()
        {
            // Clear any previous error styling
            tbTitle.BackColor = SystemColors.Window;
            
            if (string.IsNullOrWhiteSpace(tbTitle.Text))
            {
                tbTitle.BackColor = Color.LightPink;
                MessageBox.Show("Please enter a title for the note.", NotesLibrary.AppName, 
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
                selectedUnit.Title = tbTitle.Text.Trim();
                selectedUnit.Content = tbContent.Text;
                selectedUnit.X = (int)numX.Value;
                selectedUnit.Y = (int)numY.Value;
                selectedUnit.ModifiedDate = DateTime.Now;

                frmMain.selectedUnit = selectedUnit;
                frmMain.selectedUnitModified = true;
                hasChanges = false;
                
                this.DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving note: " + ex.Message, NotesLibrary.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void FrmEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                        btnSave_Click(null, null);
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
                case Keys.F2:
                    tbTitle.Focus();
                    tbTitle.SelectAll();
                    return true;
                case Keys.F3:
                    tbContent.Focus();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Enhanced text editing features
        private void tbTitle_TextChanged(object sender, EventArgs e)
        {
            tbTitle.BackColor = SystemColors.Window; // Clear error styling
            
            // Update window title with new note title
            if (!string.IsNullOrWhiteSpace(tbTitle.Text))
            {
                string windowTitle = string.Format("Edit Note - {0}", tbTitle.Text.Trim());
                if (hasChanges) windowTitle += "*";
                this.Text = windowTitle;
            }
        }

        private void tbContent_TextChanged(object sender, EventArgs e)
        {
            // Could add word count, character count, etc.
            // Example: statusLabel.Text = string.Format("Characters: {0}", tbContent.Text.Length);
        }

        // Preview functionality - show how the note will look
        private void ShowPreview()
        {
            if (string.IsNullOrWhiteSpace(tbTitle.Text))
                return;
                
            Form preview = new Form();
            preview.Text = "Preview";
            preview.Size = new Size(300, 200);
            preview.StartPosition = FormStartPosition.CenterParent;
            preview.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            
            Button previewButton = new Button();
            previewButton.Text = tbTitle.Text;
            previewButton.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
            previewButton.ForeColor = Color.FromArgb(selectedUnit.TextColor);
            previewButton.Font = selectedUnit.Font;
            previewButton.AutoSize = true;
            previewButton.Location = new Point(10, 10);
            previewButton.FlatStyle = FlatStyle.Flat;
            
            preview.Controls.Add(previewButton);
            preview.ShowDialog();
        }
    }
}
