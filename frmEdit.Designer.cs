
namespace Notes
{
    partial class frmEdit
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.tbContent = new System.Windows.Forms.TextBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.btnBackgroundColor = new System.Windows.Forms.Button();
            this.btnTextColor = new System.Windows.Forms.Button();
            this.btnFont = new System.Windows.Forms.Button();
            this.numX = new System.Windows.Forms.NumericUpDown();
            this.numY = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(96, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Title";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(73, 266);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Content";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 117);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Background Color";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 153);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Text Color";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(113, 50);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 16);
            this.label5.TabIndex = 5;
            this.label5.Text = "X";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(113, 82);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(16, 16);
            this.label6.TabIndex = 6;
            this.label6.Text = "Y";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(284, 347);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 28);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(392, 347);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(95, 188);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 16);
            this.label7.TabIndex = 9;
            this.label7.Text = "Font";
            // 
            // tbTitle
            // 
            this.tbTitle.Location = new System.Drawing.Point(140, 15);
            this.tbTitle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(351, 22);
            this.tbTitle.TabIndex = 1;
            // 
            // tbContent
            // 
            this.tbContent.AcceptsReturn = true;
            this.tbContent.AcceptsTab = true;
            this.tbContent.Location = new System.Drawing.Point(140, 218);
            this.tbContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbContent.Multiline = true;
            this.tbContent.Name = "tbContent";
            this.tbContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbContent.Size = new System.Drawing.Size(351, 110);
            this.tbContent.TabIndex = 7;
            // 
            // colorDialog
            // 
            this.colorDialog.FullOpen = true;
            // 
            // btnBackgroundColor
            // 
            this.btnBackgroundColor.Location = new System.Drawing.Point(140, 111);
            this.btnBackgroundColor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnBackgroundColor.Name = "btnBackgroundColor";
            this.btnBackgroundColor.Size = new System.Drawing.Size(352, 28);
            this.btnBackgroundColor.TabIndex = 4;
            this.btnBackgroundColor.Text = "Change";
            this.btnBackgroundColor.UseVisualStyleBackColor = true;
            this.btnBackgroundColor.Click += new System.EventHandler(this.btnBackgroundColor_Click);
            // 
            // btnTextColor
            // 
            this.btnTextColor.Location = new System.Drawing.Point(140, 146);
            this.btnTextColor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTextColor.Name = "btnTextColor";
            this.btnTextColor.Size = new System.Drawing.Size(352, 28);
            this.btnTextColor.TabIndex = 5;
            this.btnTextColor.Text = "Change";
            this.btnTextColor.UseVisualStyleBackColor = true;
            this.btnTextColor.Click += new System.EventHandler(this.btnTextColor_Click);
            // 
            // btnFont
            // 
            this.btnFont.Location = new System.Drawing.Point(140, 182);
            this.btnFont.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(352, 28);
            this.btnFont.TabIndex = 6;
            this.btnFont.Text = "Change";
            this.btnFont.UseVisualStyleBackColor = true;
            this.btnFont.Click += new System.EventHandler(this.btnFont_Click);
            // 
            // numX
            // 
            this.numX.Location = new System.Drawing.Point(140, 47);
            this.numX.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numX.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numX.Name = "numX";
            this.numX.Size = new System.Drawing.Size(352, 22);
            this.numX.TabIndex = 2;
            // 
            // numY
            // 
            this.numY.Location = new System.Drawing.Point(140, 79);
            this.numY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numY.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numY.Name = "numY";
            this.numY.Size = new System.Drawing.Size(352, 22);
            this.numY.TabIndex = 3;
            // 
            // frmEdit
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(504, 384);
            this.Controls.Add(this.numY);
            this.Controls.Add(this.numX);
            this.Controls.Add(this.btnFont);
            this.Controls.Add(this.btnTextColor);
            this.Controls.Add(this.btnBackgroundColor);
            this.Controls.Add(this.tbContent);
            this.Controls.Add(this.tbTitle);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmEdit";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit";
            this.Load += new System.EventHandler(this.frmEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.TextBox tbContent;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button btnBackgroundColor;
        private System.Windows.Forms.Button btnTextColor;
        private System.Windows.Forms.Button btnFont;
        private System.Windows.Forms.NumericUpDown numX;
        private System.Windows.Forms.NumericUpDown numY;
    }
}