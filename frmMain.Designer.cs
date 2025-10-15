
namespace Notes
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileImport = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileExport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileReload = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileReset = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.menuEditMovable = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditAutofocus = new System.Windows.Forms.ToolStripMenuItem();
            this.autoArrangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewArrange = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewArrangeGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewArrangeByDate = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewArrangeByColor = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewArrangeCompact = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.unitMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.unitMenuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.unitMenuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.unitMenuCopyInLowercase = new System.Windows.Forms.ToolStripMenuItem();
            this.unitMenuCopyInUppercase = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.unitMenuDuplicate = new System.Windows.Forms.ToolStripMenuItem();
            this.unitMenuCopyStyle = new System.Windows.Forms.ToolStripMenuItem();
            this.unitMenuPasteStyle = new System.Windows.Forms.ToolStripMenuItem();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tmrStatus = new System.Windows.Forms.Timer(this.components);
            this.tmrClickHandle = new System.Windows.Forms.Timer(this.components);
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.unitMenuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.trayMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1067, 28);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileNew,
            this.menuFileSave,
            this.toolStripSeparator2,
            this.menuFileImport,
            this.menuFileExport,
            this.toolStripSeparator5,
            this.menuFileReload,
            this.menuFileReset,
            this.toolStripSeparator1,
            this.menuFileExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // menuFileNew
            // 
            this.menuFileNew.Name = "menuFileNew";
            this.menuFileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.menuFileNew.Size = new System.Drawing.Size(190, 26);
            this.menuFileNew.Text = "&New...";
            this.menuFileNew.Click += new System.EventHandler(this.menuFileNew_Click);
            // 
            // menuFileSave
            // 
            this.menuFileSave.Name = "menuFileSave";
            this.menuFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuFileSave.Size = new System.Drawing.Size(190, 26);
            this.menuFileSave.Text = "&Save";
            this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(187, 6);
            // 
            // menuFileImport
            // 
            this.menuFileImport.Name = "menuFileImport";
            this.menuFileImport.Size = new System.Drawing.Size(190, 26);
            this.menuFileImport.Text = "&Import...";
            this.menuFileImport.Click += new System.EventHandler(this.menuFileImport_Click);
            // 
            // menuFileExport
            // 
            this.menuFileExport.Name = "menuFileExport";
            this.menuFileExport.Size = new System.Drawing.Size(190, 26);
            this.menuFileExport.Text = "&Export...";
            this.menuFileExport.Click += new System.EventHandler(this.menuFileExport_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(187, 6);
            // 
            // menuFileReload
            // 
            this.menuFileReload.Name = "menuFileReload";
            this.menuFileReload.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.menuFileReload.Size = new System.Drawing.Size(190, 26);
            this.menuFileReload.Text = "Re&load";
            this.menuFileReload.Click += new System.EventHandler(this.menuFileReload_Click);
            // 
            // menuFileReset
            // 
            this.menuFileReset.Name = "menuFileReset";
            this.menuFileReset.Size = new System.Drawing.Size(190, 26);
            this.menuFileReset.Text = "&Reset";
            this.menuFileReset.Click += new System.EventHandler(this.menuFileReset_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(187, 6);
            // 
            // menuFileExit
            // 
            this.menuFileExit.Name = "menuFileExit";
            this.menuFileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.menuFileExit.Size = new System.Drawing.Size(190, 26);
            this.menuFileExit.Text = "E&xit";
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripSeparator4,
            this.menuEditMovable,
            this.menuEditAutofocus,
            this.autoArrangeToolStripMenuItem,
            this.toolStripSeparator3,
            this.settingsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.toolStripMenuItem1.Size = new System.Drawing.Size(203, 26);
            this.toolStripMenuItem1.Text = "&Undo";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.toolStripMenuItem2.Size = new System.Drawing.Size(203, 26);
            this.toolStripMenuItem2.Text = "&Redo";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(200, 6);
            // 
            // menuEditMovable
            // 
            this.menuEditMovable.CheckOnClick = true;
            this.menuEditMovable.Name = "menuEditMovable";
            this.menuEditMovable.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.menuEditMovable.Size = new System.Drawing.Size(203, 26);
            this.menuEditMovable.Text = "&Movable";
            this.menuEditMovable.Click += new System.EventHandler(this.menuEditMovable_Click);
            // 
            // menuEditAutofocus
            // 
            this.menuEditAutofocus.CheckOnClick = true;
            this.menuEditAutofocus.Name = "menuEditAutofocus";
            this.menuEditAutofocus.Size = new System.Drawing.Size(203, 26);
            this.menuEditAutofocus.Text = "Autofocus";
            this.menuEditAutofocus.Click += new System.EventHandler(this.menuEditAutofocus_Click);
            // 
            // autoArrangeToolStripMenuItem
            // 
            this.autoArrangeToolStripMenuItem.Name = "autoArrangeToolStripMenuItem";
            this.autoArrangeToolStripMenuItem.Size = new System.Drawing.Size(203, 26);
            this.autoArrangeToolStripMenuItem.Text = "&Auto arrange";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(200, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(203, 26);
            this.settingsToolStripMenuItem.Text = "&Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewArrange});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // menuViewArrange
            // 
            this.menuViewArrange.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewArrangeGrid,
            this.menuViewArrangeByDate,
            this.menuViewArrangeByColor,
            this.menuViewArrangeCompact});
            this.menuViewArrange.Name = "menuViewArrange";
            this.menuViewArrange.Size = new System.Drawing.Size(150, 26);
            this.menuViewArrange.Text = "&Arrange";
            // 
            // menuViewArrangeGrid
            // 
            this.menuViewArrangeGrid.Name = "menuViewArrangeGrid";
            this.menuViewArrangeGrid.Size = new System.Drawing.Size(180, 26);
            this.menuViewArrangeGrid.Text = "&Grid Layout";
            this.menuViewArrangeGrid.Click += new System.EventHandler(this.menuViewArrangeGrid_Click);
            // 
            // menuViewArrangeByDate
            // 
            this.menuViewArrangeByDate.Name = "menuViewArrangeByDate";
            this.menuViewArrangeByDate.Size = new System.Drawing.Size(180, 26);
            this.menuViewArrangeByDate.Text = "By &Date";
            this.menuViewArrangeByDate.Click += new System.EventHandler(this.menuViewArrangeByDate_Click);
            // 
            // menuViewArrangeByColor
            // 
            this.menuViewArrangeByColor.Name = "menuViewArrangeByColor";
            this.menuViewArrangeByColor.Size = new System.Drawing.Size(180, 26);
            this.menuViewArrangeByColor.Text = "By &Color";
            this.menuViewArrangeByColor.Click += new System.EventHandler(this.menuViewArrangeByColor_Click);
            // 
            // menuViewArrangeCompact
            // 
            this.menuViewArrangeCompact.Name = "menuViewArrangeCompact";
            this.menuViewArrangeCompact.Size = new System.Drawing.Size(180, 26);
            this.menuViewArrangeCompact.Text = "C&ompact";
            this.menuViewArrangeCompact.Click += new System.EventHandler(this.menuViewArrangeCompact_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHelpAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Name = "menuHelpAbout";
            this.menuHelpAbout.Size = new System.Drawing.Size(133, 26);
            this.menuHelpAbout.Text = "&About";
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // unitMenuStrip
            // 
            this.unitMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.unitMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unitMenuEdit,
            this.unitMenuDelete,
            this.toolStripSeparator6,
            this.unitMenuCopyInLowercase,
            this.unitMenuCopyInUppercase,
            this.toolStripSeparator9,
            this.unitMenuDuplicate,
            this.unitMenuCopyStyle,
            this.unitMenuPasteStyle});
            this.unitMenuStrip.Name = "unitMenuStrip";
            this.unitMenuStrip.Size = new System.Drawing.Size(211, 212);
            // 
            // unitMenuEdit
            // 
            this.unitMenuEdit.Name = "unitMenuEdit";
            this.unitMenuEdit.Size = new System.Drawing.Size(202, 24);
            this.unitMenuEdit.Text = "&Edit";
            this.unitMenuEdit.Click += new System.EventHandler(this.unitMenuEdit_Click);
            // 
            // unitMenuDelete
            // 
            this.unitMenuDelete.Name = "unitMenuDelete";
            this.unitMenuDelete.Size = new System.Drawing.Size(202, 24);
            this.unitMenuDelete.Text = "&Delete";
            this.unitMenuDelete.Click += new System.EventHandler(this.unitMenuDelete_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(199, 6);
            // 
            // unitMenuCopyInLowercase
            // 
            this.unitMenuCopyInLowercase.Name = "unitMenuCopyInLowercase";
            this.unitMenuCopyInLowercase.Size = new System.Drawing.Size(210, 24);
            this.unitMenuCopyInLowercase.Text = "Copy in &Lowercase";
            this.unitMenuCopyInLowercase.Click += new System.EventHandler(this.unitMenuCopyInLowercase_Click);
            // 
            // unitMenuCopyInUppercase
            // 
            this.unitMenuCopyInUppercase.Name = "unitMenuCopyInUppercase";
            this.unitMenuCopyInUppercase.Size = new System.Drawing.Size(210, 24);
            this.unitMenuCopyInUppercase.Text = "Copy in &Uppercase";
            this.unitMenuCopyInUppercase.Click += new System.EventHandler(this.unitMenuCopyInUppercase_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(199, 6);
            // 
            // unitMenuDuplicate
            // 
            this.unitMenuDuplicate.Name = "unitMenuDuplicate";
            this.unitMenuDuplicate.Size = new System.Drawing.Size(202, 24);
            this.unitMenuDuplicate.Text = "Dup&licate";
            this.unitMenuDuplicate.Click += new System.EventHandler(this.unitMenuDuplicate_Click);
            // 
            // unitMenuCopyStyle
            // 
            this.unitMenuCopyStyle.Name = "unitMenuCopyStyle";
            this.unitMenuCopyStyle.Size = new System.Drawing.Size(202, 24);
            this.unitMenuCopyStyle.Text = "&Copy style";
            this.unitMenuCopyStyle.Click += new System.EventHandler(this.unitMenuCopyStyle_Click);
            // 
            // unitMenuPasteStyle
            // 
            this.unitMenuPasteStyle.Enabled = false;
            this.unitMenuPasteStyle.Name = "unitMenuPasteStyle";
            this.unitMenuPasteStyle.Size = new System.Drawing.Size(202, 24);
            this.unitMenuPasteStyle.Text = "&Paste style";
            this.unitMenuPasteStyle.Click += new System.EventHandler(this.unitMenuPasteStyle_Click);
            // 
            // panelContainer
            // 
            this.panelContainer.Location = new System.Drawing.Point(0, 30);
            this.panelContainer.Margin = new System.Windows.Forms.Padding(4);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(267, 123);
            this.panelContainer.TabIndex = 2;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 528);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip.Size = new System.Drawing.Size(1067, 26);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(70, 20);
            this.statusLabel.Text = "Starting...";
            // 
            // tmrStatus
            // 
            this.tmrStatus.Interval = 2000;
            this.tmrStatus.Tick += new System.EventHandler(this.tmrStatus_Tick);
            // 
            // tmrClickHandle
            // 
            this.tmrClickHandle.Tick += new System.EventHandler(this.tmrClickHandle_Tick);
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayMenuStrip;
            this.trayIcon.Text = "Notes";
            this.trayIcon.Visible = true;
            // 
            // trayMenuStrip
            // 
            this.trayMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.trayMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripSeparator7,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripSeparator8,
            this.toolStripMenuItem6});
            this.trayMenuStrip.Name = "trayMenuStrip";
            this.trayMenuStrip.Size = new System.Drawing.Size(171, 112);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(170, 24);
            this.toolStripMenuItem3.Text = "Open";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(167, 6);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(170, 24);
            this.toolStripMenuItem4.Text = "Reset Position";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(170, 24);
            this.toolStripMenuItem5.Text = "Reset Size";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(167, 6);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(170, 24);
            this.toolStripMenuItem6.Text = "Exit";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Notes";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.unitMenuStrip.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.trayMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuFileExport;
        private System.Windows.Forms.ToolStripMenuItem menuFileImport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripMenuItem menuFileNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuEditMovable;
        private System.Windows.Forms.ToolStripMenuItem autoArrangeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;
        private System.Windows.Forms.ToolStripMenuItem menuFileSave;
        private System.Windows.Forms.ContextMenuStrip unitMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem unitMenuEdit;
        private System.Windows.Forms.ToolStripMenuItem unitMenuDelete;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.ToolStripMenuItem menuFileReset;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Timer tmrStatus;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem menuFileReload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem unitMenuDuplicate;
        private System.Windows.Forms.ToolStripMenuItem unitMenuCopyStyle;
        private System.Windows.Forms.ToolStripMenuItem unitMenuPasteStyle;
        private System.Windows.Forms.Timer tmrClickHandle;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem menuEditAutofocus;
        private System.Windows.Forms.ToolStripMenuItem unitMenuCopyInLowercase;
        private System.Windows.Forms.ToolStripMenuItem unitMenuCopyInUppercase;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuViewArrange;
        private System.Windows.Forms.ToolStripMenuItem menuViewArrangeGrid;
        private System.Windows.Forms.ToolStripMenuItem menuViewArrangeByDate;
        private System.Windows.Forms.ToolStripMenuItem menuViewArrangeByColor;
        private System.Windows.Forms.ToolStripMenuItem menuViewArrangeCompact;
    }
}

