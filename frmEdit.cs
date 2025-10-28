﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;

namespace Notes
{
    public partial class frmEdit : Form
    {
        private static frmMain.UnitStruct selectedUnit = new frmMain.UnitStruct();
        private frmMain.UnitStruct originalUnit;
        private bool hasChanges = false;

        private enum ContentKind
        {
            Text,
            Image,
            Object
        }

        private ContentKind currentContentKind = ContentKind.Text;
        private string? imageBase64;
        private string? imageFormat;
        private string? objectBinaryData;

        public frmEdit()
        {
            InitializeComponent();
            InitializeModernUI();
            InitializeContentControls();
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

        private void InitializeContentControls()
        {
            UpdateContentPanels();
        }

        private void frmEdit_Load(object sender, EventArgs e)
        {
            selectedUnit = frmMain.selectedUnit;
            originalUnit = selectedUnit; // Store original for comparison

            tbTitle.Text = selectedUnit.Title;

            LoadGroups();

            UpdateColorButtons();

            LoadContentFromUnit(selectedUnit);

            tbTitle.Focus();
            tbTitle.SelectAll();

            this.Text = string.Format("Edit Note - {0}", selectedUnit.Title);

            tbTitle.TextChanged += (s, args) => MarkAsChanged();
            tbContent.TextChanged += (s, args) => MarkAsChanged();
            cmbGroup.SelectedIndexChanged += (s, args) => MarkAsChanged();
            cmbContentType.SelectedIndexChanged += (s, args) => MarkAsChanged();
        }

        private void LoadGroups()
        {
            cmbGroup.Items.Clear();
            cmbGroup.Items.Add("(None)");
            
            int selectedIndex = 0;
            int currentIndex = 1;
            
            foreach (var group in frmMain.GetGroups())
            {
                cmbGroup.Items.Add(new GroupComboItem { Id = group.Key, Title = group.Value.Title });
                
                if (!string.IsNullOrEmpty(selectedUnit.GroupId) && group.Key == selectedUnit.GroupId)
                {
                    selectedIndex = currentIndex;
                }
                currentIndex++;
            }
            
            cmbGroup.SelectedIndex = selectedIndex;
        }

        private class GroupComboItem
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public override string ToString() => Title;
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

                switch (currentContentKind)
                {
                    case ContentKind.Image:
                        if (string.IsNullOrEmpty(imageBase64))
                        {
                            MessageBox.Show("Please paste an image from the clipboard.", NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        selectedUnit.ContentType = "Image";
                        selectedUnit.ContentFormat = imageFormat ?? "png";
                        selectedUnit.ContentData = imageBase64;
                        break;
                    case ContentKind.Object:
                        if (string.IsNullOrEmpty(objectBinaryData))
                        {
                            MessageBox.Show("Please paste data from the clipboard.", NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        selectedUnit.ContentType = "Object";
                        selectedUnit.ContentFormat = "clipboard-binary";
                        selectedUnit.ContentData = objectBinaryData;
                        break;
                    default:
                        selectedUnit.ContentType = "Text";
                        selectedUnit.ContentFormat = "plain";
                        selectedUnit.ContentData = tbContent.Text;
                        objectBinaryData = null;
                        break;
                }

                selectedUnit.ModifiedDate = DateTime.Now;

                // Set GroupId from combobox
                if (cmbGroup.SelectedItem is GroupComboItem groupItem)
                {
                    selectedUnit.GroupId = groupItem.Id;
                }
                else
                {
                    selectedUnit.GroupId = null;
                }

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

        private void LoadContentFromUnit(frmMain.UnitStruct unit)
        {
            switch ((unit.ContentType ?? "Text").ToLowerInvariant())
            {
                case "image":
                    currentContentKind = ContentKind.Image;
                    cmbContentType.SelectedIndex = 1;
                    imageBase64 = unit.ContentData;
                    imageFormat = unit.ContentFormat;
                    if (!string.IsNullOrEmpty(unit.ContentData))
                    {
                        var bmp = ClipboardHelper.DecodeImage(unit.ContentData);
                        if (bmp != null)
                        {
                            if (picImagePreview.Image != null)
                                picImagePreview.Image.Dispose();
                            picImagePreview.Image = bmp;
                        }
                    }
                    tbContent.Text = string.Empty;
                    objectBinaryData = null;
                    lblObjectSummary.Text = string.Empty;
                    break;
                case "object":
                    currentContentKind = ContentKind.Object;
                    cmbContentType.SelectedIndex = 2;
                    objectBinaryData = unit.ContentData;
                    string summary;
                    if (!ClipboardHelper.TryDescribeObject(objectBinaryData, out summary))
                        summary = "Clipboard formats will appear here.";
                    lblObjectSummary.Text = summary;
                    imageBase64 = null;
                    imageFormat = null;
                    if (picImagePreview.Image != null)
                    {
                        picImagePreview.Image.Dispose();
                        picImagePreview.Image = null;
                    }
                    break;
                default:
                    currentContentKind = ContentKind.Text;
                    cmbContentType.SelectedIndex = 0;
                    tbContent.Text = unit.Content;
                    imageBase64 = null;
                    imageFormat = null;
                    objectBinaryData = null;
                    if (picImagePreview.Image != null)
                    {
                        picImagePreview.Image.Dispose();
                        picImagePreview.Image = null;
                    }
                    lblObjectSummary.Text = "Clipboard formats will appear here.";
                    break;
            }

            UpdateContentPanels();
        }

        private void UpdateContentPanels()
        {
            pnlTextContent.Visible = currentContentKind == ContentKind.Text;
            pnlImageContent.Visible = currentContentKind == ContentKind.Image;
            pnlObjectContent.Visible = currentContentKind == ContentKind.Object;

            if (currentContentKind != ContentKind.Object)
            {
                lblObjectSummary.Text = "Clipboard formats will appear here.";
                if (currentContentKind != ContentKind.Image && picImagePreview.Image != null)
                {
                    picImagePreview.Image.Dispose();
                    picImagePreview.Image = null;
                }
            }
            else if (string.IsNullOrEmpty(objectBinaryData))
            {
                lblObjectSummary.Text = "Clipboard formats will appear here.";
            }
        }

        private void cmbContentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var previousKind = currentContentKind;
            switch (cmbContentType.SelectedIndex)
            {
                case 1:
                    currentContentKind = ContentKind.Image;
                    break;
                case 2:
                    currentContentKind = ContentKind.Object;
                    break;
                default:
                    currentContentKind = ContentKind.Text;
                    break;
            }

            if (currentContentKind != previousKind)
            {
                MarkAsChanged();
            }

            UpdateContentPanels();
        }

        private void btnPasteImage_Click(object sender, EventArgs e)
        {
            string base64;
            string format;
            Bitmap? preview;
            if (ClipboardHelper.TryCaptureImageFromClipboard(out base64, out format, out preview))
            {
                imageBase64 = base64;
                imageFormat = format;
                if (picImagePreview.Image != null)
                {
                    picImagePreview.Image.Dispose();
                }
                picImagePreview.Image = preview;
                objectBinaryData = null;
                lblObjectSummary.Text = "";
                MarkAsChanged();
            }
            else
            {
                MessageBox.Show("Clipboard does not contain an image.", NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnPasteObject_Click(object sender, EventArgs e)
        {
            string base64;
            string summary;
            if (ClipboardHelper.TryCaptureClipboardObject(out base64, out summary))
            {
                objectBinaryData = base64;
                lblObjectSummary.Text = summary;
                imageBase64 = null;
                imageFormat = null;
                if (picImagePreview.Image != null)
                {
                    picImagePreview.Image.Dispose();
                    picImagePreview.Image = null;
                }
                MarkAsChanged();
            }
            else
            {
                MessageBox.Show("Unable to capture clipboard object.", NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
