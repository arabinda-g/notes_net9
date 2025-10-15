using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Notes
{
    public partial class frmAbout : Form
    {
        private Color _buttonHoverColor = Color.FromArgb(41, 128, 185);
        private Color _buttonNormalColor = Color.FromArgb(52, 152, 219);

        public frmAbout()
        {
            InitializeComponent();
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            // Get version from assembly
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                lblVersion.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
            }

            // Apply theme if dark mode is active
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // Check if dark mode is enabled
            bool isDarkMode = false;
            try
            {
                // Try to get theme from NotesLibrary if available
                var themeField = typeof(NotesLibrary).GetField("Theme", BindingFlags.Public | BindingFlags.Static);
                if (themeField != null)
                {
                    var theme = themeField.GetValue(null);
                    if (theme != null && theme.ToString() == "Dark")
                    {
                        isDarkMode = true;
                    }
                }
            }
            catch
            {
                // If we can't get the theme, default to light mode
            }

            if (isDarkMode)
            {
                // Dark theme colors
                this.BackColor = Color.FromArgb(45, 45, 48);
                panelBody.BackColor = Color.FromArgb(45, 45, 48);
                panelFooter.BackColor = Color.FromArgb(37, 37, 38);
                
                lblDescription.ForeColor = Color.FromArgb(220, 220, 220);
                lblCopyright.ForeColor = Color.FromArgb(180, 180, 180);
                lblFramework.ForeColor = Color.FromArgb(180, 180, 180);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_MouseEnter(object sender, EventArgs e)
        {
            btnOK.BackColor = _buttonHoverColor;
        }

        private void btnOK_MouseLeave(object sender, EventArgs e)
        {
            btnOK.BackColor = _buttonNormalColor;
        }

        private void linkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open GitHub repository (update this URL if you have one)
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/yourusername/notes",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Unable to open the link.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

