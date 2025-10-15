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
    public partial class frmAdd : Form
    {
        private static frmMain.UnitStruct selectedUnit = new frmMain.UnitStruct();

        public frmAdd()
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
            
            // Initialize with default values
            selectedUnit.BackgroundColor = config.DefaultUnitStyle.BackgroundColor;
            selectedUnit.TextColor = config.DefaultUnitStyle.TextColor;
            selectedUnit.Font = NotesLibrary.Instance.GetDefaultFont();
            selectedUnit.CreatedDate = DateTime.Now;
            selectedUnit.ModifiedDate = DateTime.Now;
            selectedUnit.Category = "";
            selectedUnit.Tags = new string[0];
        }

        private void frmAdd_Load(object sender, EventArgs e)
        {
            // Set position values from main form selection
            numX.Value = frmMain.selectedUnit.X;
            numY.Value = frmMain.selectedUnit.Y;
            
            // Initialize color buttons
            UpdateColorButtons();
            
            // Set focus to title
            tbTitle.Focus();
            
            // Add validation hints
            this.Text = "Add New Note";
        }

        private void UpdateColorButtons()
        {
            try
            {
                btnBackgroundColor.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                btnTextColor.BackColor = Color.FromArgb(selectedUnit.TextColor);
                btnFont.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                btnFont.ForeColor = Color.FromArgb(selectedUnit.TextColor);
                btnFont.Text = string.Format("{0}, {1}pt", selectedUnit.Font.Name, selectedUnit.Font.SizeInPoints);
            }
            catch (Exception)
            {
                // Fallback to defaults if color conversion fails
                var config = NotesLibrary.Instance.Config;
                selectedUnit.BackgroundColor = config.DefaultUnitStyle.BackgroundColor;
                selectedUnit.TextColor = config.DefaultUnitStyle.TextColor;
                selectedUnit.Font = NotesLibrary.Instance.GetDefaultFont();
                
                btnBackgroundColor.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                btnTextColor.BackColor = Color.FromArgb(selectedUnit.TextColor);
                btnFont.BackColor = Color.FromArgb(selectedUnit.BackgroundColor);
                btnFont.ForeColor = Color.FromArgb(selectedUnit.TextColor);
                btnFont.Text = string.Format("{0}, {1}pt", selectedUnit.Font.Name, selectedUnit.Font.SizeInPoints);
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
                selectedUnit.CreatedDate = DateTime.Now;
                selectedUnit.ModifiedDate = DateTime.Now;

                frmMain.selectedUnit = selectedUnit;
                frmMain.selectedUnitModified = true;
                
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

        // Preview functionality
        private void tbTitle_TextChanged(object sender, EventArgs e)
        {
            tbTitle.BackColor = SystemColors.Window; // Clear error styling
        }

        private void tbContent_TextChanged(object sender, EventArgs e)
        {
            // Update character count or word count if needed
            // Could add a status label showing character count
        }
    }
}
