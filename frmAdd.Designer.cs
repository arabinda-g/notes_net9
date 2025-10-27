
namespace Notes
{
    partial class frmAdd
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
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.btnBackgroundColor = new System.Windows.Forms.Button();
            this.btnTextColor = new System.Windows.Forms.Button();
            this.btnFont = new System.Windows.Forms.Button();
            this.cmbGroup = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbContentType = new System.Windows.Forms.ComboBox();
            this.pnlTextContent = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.tbContent = new System.Windows.Forms.TextBox();
            this.pnlImageContent = new System.Windows.Forms.Panel();
            this.picImagePreview = new System.Windows.Forms.PictureBox();
            this.btnPasteImage = new System.Windows.Forms.Button();
            this.pnlObjectContent = new System.Windows.Forms.Panel();
            this.lblObjectSummary = new System.Windows.Forms.Label();
            this.btnPasteObject = new System.Windows.Forms.Button();
            this.pnlTextContent.SuspendLayout();
            this.pnlImageContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picImagePreview)).BeginInit();
            this.pnlObjectContent.SuspendLayout();
            this.SuspendLayout();
            // label1
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(96, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Title";
            // label3
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 82);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Background Color";
            // label4
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 118);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Text Color";
            // label5
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(86, 50);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 16);
            this.label5.TabIndex = 5;
            this.label5.Text = "Group";
            // btnSave
            this.btnSave.Location = new System.Drawing.Point(294, 356);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 28);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // btnCancel
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(402, 356);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // label7
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(95, 153);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 16);
            this.label7.TabIndex = 9;
            this.label7.Text = "Font";
            // tbTitle
            this.tbTitle.Location = new System.Drawing.Point(140, 15);
            this.tbTitle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(351, 22);
            this.tbTitle.TabIndex = 1;
            // btnBackgroundColor
            this.btnBackgroundColor.Location = new System.Drawing.Point(140, 76);
            this.btnBackgroundColor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnBackgroundColor.Name = "btnBackgroundColor";
            this.btnBackgroundColor.Size = new System.Drawing.Size(352, 28);
            this.btnBackgroundColor.TabIndex = 3;
            this.btnBackgroundColor.Text = "Change";
            this.btnBackgroundColor.UseVisualStyleBackColor = true;
            this.btnBackgroundColor.Click += new System.EventHandler(this.btnBackgroundColor_Click);
            // btnTextColor
            this.btnTextColor.Location = new System.Drawing.Point(140, 111);
            this.btnTextColor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTextColor.Name = "btnTextColor";
            this.btnTextColor.Size = new System.Drawing.Size(352, 28);
            this.btnTextColor.TabIndex = 4;
            this.btnTextColor.Text = "Change";
            this.btnTextColor.UseVisualStyleBackColor = true;
            this.btnTextColor.Click += new System.EventHandler(this.btnTextColor_Click);
            // btnFont
            this.btnFont.Location = new System.Drawing.Point(140, 147);
            this.btnFont.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(352, 28);
            this.btnFont.TabIndex = 5;
            this.btnFont.Text = "Change";
            this.btnFont.UseVisualStyleBackColor = true;
            this.btnFont.Click += new System.EventHandler(this.btnFont_Click);
            // cmbGroup
            this.cmbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGroup.FormattingEnabled = true;
            this.cmbGroup.Location = new System.Drawing.Point(140, 47);
            this.cmbGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbGroup.Name = "cmbGroup";
            this.cmbGroup.Size = new System.Drawing.Size(351, 24);
            this.cmbGroup.TabIndex = 2;
            // label6
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(29, 187);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 16);
            this.label6.TabIndex = 10;
            this.label6.Text = "Content";
            // cmbContentType
            this.cmbContentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbContentType.FormattingEnabled = true;
            this.cmbContentType.Items.AddRange(new object[] {
            "Text",
            "Image",
            "Object"});
            this.cmbContentType.Location = new System.Drawing.Point(140, 184);
            this.cmbContentType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbContentType.Name = "cmbContentType";
            this.cmbContentType.Size = new System.Drawing.Size(351, 24);
            this.cmbContentType.TabIndex = 6;
            this.cmbContentType.SelectedIndexChanged += new System.EventHandler(this.cmbContentType_SelectedIndexChanged);
            // pnlTextContent
            this.pnlTextContent.Controls.Add(this.label2);
            this.pnlTextContent.Controls.Add(this.tbContent);
            this.pnlTextContent.Location = new System.Drawing.Point(140, 217);
            this.pnlTextContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlTextContent.Name = "pnlTextContent";
            this.pnlTextContent.Size = new System.Drawing.Size(352, 132);
            this.pnlTextContent.TabIndex = 7;
            // label2
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Content";
            // tbContent
            this.tbContent.AcceptsReturn = true;
            this.tbContent.AcceptsTab = true;
            this.tbContent.Location = new System.Drawing.Point(0, 22);
            this.tbContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbContent.Multiline = true;
            this.tbContent.Name = "tbContent";
            this.tbContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbContent.Size = new System.Drawing.Size(352, 110);
            this.tbContent.TabIndex = 0;
            // pnlImageContent
            this.pnlImageContent.Controls.Add(this.picImagePreview);
            this.pnlImageContent.Controls.Add(this.btnPasteImage);
            this.pnlImageContent.Location = new System.Drawing.Point(140, 217);
            this.pnlImageContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlImageContent.Name = "pnlImageContent";
            this.pnlImageContent.Size = new System.Drawing.Size(352, 132);
            this.pnlImageContent.TabIndex = 8;
            this.pnlImageContent.Visible = false;
            // picImagePreview
            this.picImagePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picImagePreview.Location = new System.Drawing.Point(4, 44);
            this.picImagePreview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.picImagePreview.Name = "picImagePreview";
            this.picImagePreview.Size = new System.Drawing.Size(216, 82);
            this.picImagePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picImagePreview.TabIndex = 1;
            this.picImagePreview.TabStop = false;
            // btnPasteImage
            this.btnPasteImage.Location = new System.Drawing.Point(4, 4);
            this.btnPasteImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPasteImage.Name = "btnPasteImage";
            this.btnPasteImage.Size = new System.Drawing.Size(144, 32);
            this.btnPasteImage.TabIndex = 0;
            this.btnPasteImage.Text = "Paste from Clipboard";
            this.btnPasteImage.UseVisualStyleBackColor = true;
            this.btnPasteImage.Click += new System.EventHandler(this.btnPasteImage_Click);
            // pnlObjectContent
            this.pnlObjectContent.Controls.Add(this.lblObjectSummary);
            this.pnlObjectContent.Controls.Add(this.btnPasteObject);
            this.pnlObjectContent.Location = new System.Drawing.Point(140, 217);
            this.pnlObjectContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlObjectContent.Name = "pnlObjectContent";
            this.pnlObjectContent.Size = new System.Drawing.Size(352, 132);
            this.pnlObjectContent.TabIndex = 9;
            this.pnlObjectContent.Visible = false;
            // lblObjectSummary
            this.lblObjectSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblObjectSummary.Location = new System.Drawing.Point(4, 44);
            this.lblObjectSummary.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblObjectSummary.Name = "lblObjectSummary";
            this.lblObjectSummary.Size = new System.Drawing.Size(344, 82);
            this.lblObjectSummary.TabIndex = 1;
            this.lblObjectSummary.Text = "Clipboard formats will appear here.";
            // btnPasteObject
            this.btnPasteObject.Location = new System.Drawing.Point(4, 4);
            this.btnPasteObject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPasteObject.Name = "btnPasteObject";
            this.btnPasteObject.Size = new System.Drawing.Size(144, 32);
            this.btnPasteObject.TabIndex = 0;
            this.btnPasteObject.Text = "Paste from Clipboard";
            this.btnPasteObject.UseVisualStyleBackColor = true;
            this.btnPasteObject.Click += new System.EventHandler(this.btnPasteObject_Click);
            // frmAdd
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(523, 397);
            this.Controls.Add(this.pnlObjectContent);
            this.Controls.Add(this.pnlImageContent);
            this.Controls.Add(this.pnlTextContent);
            this.Controls.Add(this.cmbContentType);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmbGroup);
            this.Controls.Add(this.btnFont);
            this.Controls.Add(this.btnTextColor);
            this.Controls.Add(this.btnBackgroundColor);
            this.Controls.Add(this.tbTitle);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAdd";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add";
            this.pnlTextContent.ResumeLayout(false);
            this.pnlTextContent.PerformLayout();
            this.pnlImageContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picImagePreview)).EndInit();
            this.pnlObjectContent.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button btnBackgroundColor;
        private System.Windows.Forms.Button btnTextColor;
        private System.Windows.Forms.Button btnFont;
        private System.Windows.Forms.ComboBox cmbGroup;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbContentType;
        private System.Windows.Forms.Panel pnlTextContent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbContent;
        private System.Windows.Forms.Panel pnlImageContent;
        private System.Windows.Forms.PictureBox picImagePreview;
        private System.Windows.Forms.Button btnPasteImage;
        private System.Windows.Forms.Panel pnlObjectContent;
        private System.Windows.Forms.Label lblObjectSummary;
        private System.Windows.Forms.Button btnPasteObject;
    }
}