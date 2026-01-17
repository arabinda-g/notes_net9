using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Notes
{
    internal static class AnimationHelper
    {
        public static bool Enabled => NotesLibrary.Instance.Config.General.EnableAnimations;

        public static void StartTimer(System.Windows.Forms.Timer timer)
        {
            if (Enabled)
                timer.Start();
        }

        public static void ApplyToExisting(Control root)
        {
            foreach (var timer in root.Controls.OfType<Control>()
                .SelectMany(GetTimers))
            {
                if (Enabled)
                    timer.Start();
                else
                    timer.Stop();
            }
        }

        private static IEnumerable<System.Windows.Forms.Timer> GetTimers(Control control)
        {
            foreach (var field in control.GetType().GetFields(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            {
                if (field.FieldType == typeof(System.Windows.Forms.Timer))
                {
                    if (field.GetValue(control) is System.Windows.Forms.Timer timer)
                        yield return timer;
                }
            }

            foreach (Control child in control.Controls)
            {
                foreach (var timer in GetTimers(child))
                    yield return timer;
            }
        }
    }

    // Base class for custom group boxes
    public class CustomGroupBoxBase : GroupBox
    {
        private Panel resizeHandlePanel;
        private bool isResizing = false;
        private Point resizeStart;
        private Size originalSize;
        private const int RESIZE_HANDLE_SIZE = 16;
        private int borderColor = 0;
        private bool useCustomBorder = false;

        public int BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public bool UseCustomBorder
        {
            get => useCustomBorder;
            set
            {
                useCustomBorder = value;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllowResize { get; set; } = true;
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsResizing => isResizing;

        public CustomGroupBoxBase()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            CreateResizeHandle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x000F && UseCustomBorder && BorderColor != 0) // WM_PAINT
            {
                using var g = Graphics.FromHwnd(this.Handle);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using var pen = new Pen(Color.FromArgb(BorderColor));
                var rect = this.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                var text = this.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                {
                    g.DrawRectangle(pen, rect);
                    return;
                }

                var textSize = TextRenderer.MeasureText(text, this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                int textPadding = 8;
                if (this.RightToLeft == RightToLeft.Yes)
                    textPadding = Math.Max(textPadding, this.Padding.Right);
                else
                    textPadding = Math.Max(textPadding, this.Padding.Left);
                int textGap = 4;
                int textLeft;
                int textRight;
                int top = rect.Top + Math.Max(0, this.Padding.Top);
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    textRight = rect.Right - textPadding;
                    textLeft = textRight - textSize.Width - textGap;
                }
                else
                {
                    textLeft = rect.Left + textPadding;
                    textRight = textLeft + textSize.Width + textGap;
                }
                g.DrawLine(pen, rect.Left, top, textLeft - 2, top);
                g.DrawLine(pen, textRight, top, rect.Right, top);
                g.DrawLine(pen, rect.Left, top, rect.Left, rect.Bottom);
                g.DrawLine(pen, rect.Right, top, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
            }
        }

        private void CreateResizeHandle()
        {
            resizeHandlePanel = new Panel
            {
                Width = RESIZE_HANDLE_SIZE,
                Height = RESIZE_HANDLE_SIZE,
                BackColor = Color.Transparent,
                Cursor = Cursors.SizeNWSE,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            resizeHandlePanel.Paint += (s, e) =>
            {
                if (!AllowResize) return;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, this.ForeColor)))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE);
                }
                using (Pen pen = new Pen(Color.FromArgb(200, this.Parent?.BackColor ?? SystemColors.Control), 2))
                {
                    e.Graphics.DrawLine(pen, 4, 12, 12, 4);
                    e.Graphics.DrawLine(pen, 7, 12, 12, 7);
                    e.Graphics.DrawLine(pen, 10, 12, 12, 10);
                }
            };

            resizeHandlePanel.MouseDown += (s, e) =>
            {
                if (!AllowResize || e.Button != MouseButtons.Left) return;
                isResizing = true;
                resizeStart = this.PointToClient(Cursor.Position);
                originalSize = this.Size;
            };

            resizeHandlePanel.MouseMove += (s, e) =>
            {
                if (!isResizing) return;
                Point currentMouse = this.PointToClient(Cursor.Position);
                int newWidth = originalSize.Width + (currentMouse.X - resizeStart.X);
                int newHeight = originalSize.Height + (currentMouse.Y - resizeStart.Y);
                newWidth = Math.Max(150, newWidth);
                newHeight = Math.Max(100, newHeight);
                
                Rectangle oldBounds = this.Bounds;
                this.Size = new Size(newWidth, newHeight);
                
                if (this.Parent != null)
                {
                    this.Parent.Invalidate(oldBounds);
                    this.Parent.Invalidate(this.Bounds);
                    this.Parent.Update();
                }
            };

            resizeHandlePanel.MouseUp += (s, e) =>
            {
                if (!isResizing) return;
                isResizing = false;
                OnSizeChanged(EventArgs.Empty);
            };

            this.Controls.Add(resizeHandlePanel);
            PositionResizeHandle();
        }

        private void PositionResizeHandle()
        {
            if (resizeHandlePanel != null)
            {
                resizeHandlePanel.Location = new Point(
                    this.Width - RESIZE_HANDLE_SIZE - 2,
                    this.Height - RESIZE_HANDLE_SIZE - 2);
                resizeHandlePanel.Visible = AllowResize;
                if (AllowResize) resizeHandlePanel.BringToFront();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PositionResizeHandle();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            PositionResizeHandle();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (resizeHandlePanel != null && AllowResize && e.Control != resizeHandlePanel)
            {
                resizeHandlePanel.BringToFront();
            }
        }

        public void UpdateResizeHandleVisibility()
        {
            if (resizeHandlePanel != null)
            {
                resizeHandlePanel.Visible = AllowResize;
            }
        }

        protected virtual void DrawGroupBoxBorder(Graphics g, Rectangle rect) { }
        protected virtual void DrawGroupBoxTitle(Graphics g, Rectangle rect) { }
    }

    // 1. Gradient Glass GroupBox
    public class GradientGlassGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 15, Width - 1, Height - 16);

            // Gradient background
            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect, Color.FromArgb(200, this.BackColor), 
                Color.FromArgb(150, ControlPaint.Light(this.BackColor)), 
                LinearGradientMode.Vertical))
            {
                using (GraphicsPath path = GetRoundedRect(rect, 12))
                {
                    g.FillPath(brush, path);
                    using (Pen pen = new Pen(this.ForeColor, 2))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 10, 0, titleSize.Width + 10, titleSize.Height);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, this.Font, textBrush, 15, 0);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // 2. Neon Glow GroupBox
    public class NeonGlowGroupBox : CustomGroupBoxBase
    {
        private Color glowColor = Color.FromArgb(0, 255, 127);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GlowColor
        {
            get => glowColor;
            set { glowColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(6, 21, Width - 13, Height - 28);

            // Outer glow
            for (int i = 5; i > 0; i--)
            {
                using (Pen pen = new Pen(Color.FromArgb(30, this.ForeColor), i * 2))
                {
                    Rectangle glowRect = new Rectangle(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2);
                    g.DrawRectangle(pen, glowRect);
                }
            }

            // Background
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, rect);
            }

            // Border
            using (Pen pen = new Pen(this.ForeColor, 3))
            {
                g.DrawRectangle(pen, rect);
            }

            // Inner glow
            Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
            using (Pen pen = new Pen(Color.FromArgb(100, this.ForeColor), 1))
            {
                g.DrawRectangle(pen, innerRect);
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 10, 5, titleSize.Width + 10, titleSize.Height + 10);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, this.Font, textBrush, 15, 8);
            }
        }
    }

    // 3. 3D Embossed GroupBox
    public class EmbossedGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(2, 17, Width - 5, Height - 20);

            // Shadow
            using (GraphicsPath shadowPath = GetRoundedRect(new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height), 8))
            {
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Main surface
            using (GraphicsPath path = GetRoundedRect(rect, 8))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect, ControlPaint.Light(this.BackColor), this.BackColor, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }

                // Highlight edge
                Rectangle highlightRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2);
                using (LinearGradientBrush highlightBrush = new LinearGradientBrush(
                    highlightRect, Color.FromArgb(80, 255, 255, 255), 
                    Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
                {
                    using (GraphicsPath highlightPath = GetRoundedRect(highlightRect, 8))
                    {
                        g.FillPath(highlightBrush, highlightPath);
                    }
                }

                using (Pen pen = new Pen(this.ForeColor, 2))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 10, 2, titleSize.Width + 10, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 15, 5);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // 4. Retro Style GroupBox
    public class RetroGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            Rectangle rect = new Rectangle(2, 17, Width - 5, Height - 20);

            // 3D effect layers
            for (int i = 4; i > 0; i--)
            {
                Color layerColor = Color.FromArgb(
                    Math.Max(0, this.BackColor.R - i * 30),
                    Math.Max(0, this.BackColor.G - i * 30),
                    Math.Max(0, this.BackColor.B - i * 30));
                using (SolidBrush brush = new SolidBrush(layerColor))
                {
                    g.FillRectangle(brush, rect.X + i, rect.Y + i, rect.Width, rect.Height);
                }
            }

            // Main face
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, rect);
            }

            // Highlights
            using (Pen pen = new Pen(ControlPaint.Light(this.BackColor), 2))
            {
                g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
                g.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom);
            }

            // Shadows
            using (Pen pen = new Pen(ControlPaint.Dark(this.BackColor), 2))
            {
                g.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 10, 2, titleSize.Width + 10, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 15, 5);
            }
        }
    }

    // 5. Modern Card GroupBox
    public class CardGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 18, Width - 10, Height - 24);

            // Shadow
            using (GraphicsPath shadowPath = GetRoundedRect(new Rectangle(rect.X + 4, rect.Y + 4, rect.Width, rect.Height), 10))
            {
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Card background
            using (GraphicsPath path = GetRoundedRect(rect, 10))
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    g.FillPath(brush, path);
                }

                // Border
                using (Pen borderPen = new Pen(ControlPaint.Light(this.BackColor), 2))
                {
                    g.DrawPath(borderPen, path);
                }

                // Accent line on top
                using (Pen accentPen = new Pen(this.ForeColor, 4))
                {
                    g.DrawLine(accentPen, rect.Left + 15, rect.Top, rect.Right - 15, rect.Top);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 12, 3, titleSize.Width + 12, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 18, 6);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // 6. Minimal Flat GroupBox
    public class MinimalGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 18, Width - 1, Height - 19);

            // Background
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, rect);
            }

            // Simple border
            using (Pen pen = new Pen(this.ForeColor, 1))
            {
                g.DrawRectangle(pen, rect);
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 10, 8, titleSize.Width + 10, titleSize.Height);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, this.Font, textBrush, 15, 8);
            }
        }
    }

    // 7. Dashed Border GroupBox
    public class DashedGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(2, 18, Width - 5, Height - 21);

            // Background
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, rect);
            }

            // Dashed border
            using (Pen pen = new Pen(this.ForeColor, 2))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(pen, rect);
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 10, 8, titleSize.Width + 10, titleSize.Height);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, this.Font, textBrush, 15, 8);
            }
        }
    }

    // 8. Double Border GroupBox
    public class DoubleBorderGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle outerRect = new Rectangle(1, 18, Width - 3, Height - 20);
            Rectangle innerRect = new Rectangle(outerRect.X + 4, outerRect.Y + 4, outerRect.Width - 8, outerRect.Height - 8);

            // Background
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, outerRect);
            }

            // Outer border
            using (Pen pen = new Pen(this.ForeColor, 2))
            {
                g.DrawRectangle(pen, outerRect);
            }

            // Inner border
            using (Pen pen = new Pen(ControlPaint.Light(this.ForeColor), 1))
            {
                g.DrawRectangle(pen, innerRect);
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 10, 8, titleSize.Width + 10, titleSize.Height);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 15, 8);
            }
        }
    }

    // 9. Shadow Panel GroupBox
    public class ShadowPanelGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 12, Height - 28);

            // Multiple shadow layers
            for (int i = 8; i > 0; i--)
            {
                int alpha = 10 + (i * 5);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                {
                    g.FillRectangle(brush, rect.X + i, rect.Y + i, rect.Width, rect.Height);
                }
            }

            // Main panel
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, rect);
            }

            // Border
            using (Pen pen = new Pen(this.ForeColor, 2))
            {
                g.DrawRectangle(pen, rect);
            }

            // Title background bar
            Rectangle titleBar = new Rectangle(rect.X, rect.Y, rect.Width, 30);
            using (LinearGradientBrush brush = new LinearGradientBrush(
                titleBar, ControlPaint.Light(this.ForeColor), this.ForeColor, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, titleBar);
            }

            // Title
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, rect.X + 10, rect.Y + 7);
            }
        }
    }

    // 10. Rounded Neon GroupBox
    public class RoundedNeonGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(5, 20, Width - 11, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 20))
            {
                // Glow effect
                for (int i = 6; i > 0; i--)
                {
                    using (Pen pen = new Pen(Color.FromArgb(25, this.ForeColor), i * 2))
                    {
                        using (GraphicsPath glowPath = GetRoundedRect(
                            new Rectangle(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2), 20))
                        {
                            g.DrawPath(pen, glowPath);
                        }
                    }
                }

                // Background
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    g.FillPath(brush, path);
                }

                // Border
                using (Pen pen = new Pen(this.ForeColor, 3))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 15, 5, titleSize.Width + 10, titleSize.Height + 10);
            }
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 20, 8);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // 11. Holographic/Futuristic GroupBox
    public class HolographicGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer animationTimer;
        private float glowOffset = 0;

        public HolographicGroupBox()
        {
            animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            animationTimer.Tick += (s, e) => { glowOffset += 0.1f; if (glowOffset > 360) glowOffset = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(animationTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(4, 20, Width - 12, Height - 28);

            // Animated holographic border
            using (GraphicsPath path = GetRoundedRect(rect, 15))
            {
                // Dark background
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, this.BackColor)))
                {
                    g.FillPath(brush, path);
                }

                // Multiple colored glow layers
                Color[] holoColors = {
                    Color.FromArgb(100, 0, 255, 255),
                    Color.FromArgb(100, 138, 43, 226),
                    Color.FromArgb(100, 255, 0, 255),
                    Color.FromArgb(100, 0, 191, 255)
                };

                for (int i = 0; i < 4; i++)
                {
                    float angle = glowOffset + (i * 90);
                    int offsetX = (int)(Math.Sin(angle * Math.PI / 180) * 3);
                    int offsetY = (int)(Math.Cos(angle * Math.PI / 180) * 3);
                    
                    using (Pen pen = new Pen(holoColors[i], 2))
                    {
                        using (GraphicsPath glowPath = GetRoundedRect(
                            new Rectangle(rect.X + offsetX, rect.Y + offsetY, rect.Width, rect.Height), 15))
                        {
                            g.DrawPath(pen, glowPath);
                        }
                    }
                }

                // Main border
                using (Pen pen = new Pen(Color.FromArgb(200, 0, 255, 255), 2))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Scan line effect
            using (Pen scanPen = new Pen(Color.FromArgb(30, 0, 255, 255), 1))
            {
                int scanY = (int)(glowOffset * rect.Height / 360) + rect.Y;
                g.DrawLine(scanPen, rect.Left, scanY, rect.Right, scanY);
            }

            // Title with glow
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
            {
                g.FillRectangle(bgBrush, 15, 5, titleSize.Width + 14, titleSize.Height + 10);
            }
            
            // Glow effect on text
            using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(100, 0, 255, 255)))
            {
                g.DrawString(this.Text, this.Font, glowBrush, 21, 9);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(0, 255, 255)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 20, 8);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // 12. Vintage Paper GroupBox
    public class VintagePaperGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            // Paper texture background
            Color paperColor = Color.FromArgb(245, 235, 210);
            using (SolidBrush paperBrush = new SolidBrush(paperColor))
            {
                g.FillRectangle(paperBrush, rect);
            }

            // Add aged texture spots
            Random rand = new Random(this.GetHashCode());
            for (int i = 0; i < 20; i++)
            {
                int x = rand.Next(rect.Left, rect.Right);
                int y = rand.Next(rect.Top, rect.Bottom);
                int size = rand.Next(2, 5);
                using (SolidBrush spotBrush = new SolidBrush(Color.FromArgb(40, 101, 67, 33)))
                {
                    g.FillEllipse(spotBrush, x, y, size, size);
                }
            }

            // Torn edge effect
            using (Pen edgePen = new Pen(Color.FromArgb(139, 90, 43), 3))
            {
                edgePen.DashStyle = DashStyle.Custom;
                edgePen.DashPattern = new float[] { 2, 1, 1, 1 };
                g.DrawRectangle(edgePen, rect);
            }

            // Inner shadow
            using (Pen shadowPen = new Pen(Color.FromArgb(60, 101, 67, 33), 1))
            {
                Rectangle innerRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6);
                g.DrawRectangle(shadowPen, innerRect);
            }

            // Vintage stamp in corner
            Rectangle stampRect = new Rectangle(rect.Right - 35, rect.Top + 5, 28, 28);
            using (Pen stampPen = new Pen(Color.FromArgb(100, 139, 0, 0), 2))
            {
                stampPen.DashStyle = DashStyle.Dash;
                g.DrawEllipse(stampPen, stampRect);
            }

            // Title with vintage look
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(paperColor))
            {
                g.FillRectangle(bgBrush, 12, 8, titleSize.Width + 12, titleSize.Height + 4);
            }
            
            // Shadow text
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(80, 101, 67, 33)))
            {
                g.DrawString(this.Text, this.Font, shadowBrush, 19, 11);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(101, 67, 33)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Italic), textBrush, 18, 10);
            }
        }
    }

    // 13. Liquid Metal GroupBox
    public class LiquidMetalGroupBox : CustomGroupBoxBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 20))
            {
                // Metallic gradient background
                using (LinearGradientBrush metalBrush = new LinearGradientBrush(
                    rect, 
                    Color.FromArgb(192, 192, 192),
                    Color.FromArgb(105, 105, 105),
                    LinearGradientMode.Vertical))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Colors = new Color[] {
                        Color.FromArgb(220, 220, 220),
                        Color.FromArgb(180, 180, 180),
                        Color.FromArgb(140, 140, 140),
                        Color.FromArgb(180, 180, 180),
                        Color.FromArgb(220, 220, 220)
                    };
                    blend.Positions = new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
                    metalBrush.InterpolationColors = blend;
                    
                    g.FillPath(metalBrush, path);
                }

                // Highlight streak
                Rectangle highlightRect = new Rectangle(rect.X + 10, rect.Y + 10, rect.Width - 20, rect.Height / 3);
                using (LinearGradientBrush highlightBrush = new LinearGradientBrush(
                    highlightRect,
                    Color.FromArgb(150, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    using (GraphicsPath highlightPath = GetRoundedRect(highlightRect, 15))
                    {
                        g.FillPath(highlightBrush, highlightPath);
                    }
                }

                // Liquid droplet effect
                using (PathGradientBrush dropletBrush = new PathGradientBrush(path))
                {
                    dropletBrush.CenterColor = Color.FromArgb(80, 255, 255, 255);
                    dropletBrush.SurroundColors = new Color[] { Color.FromArgb(0, 255, 255, 255) };
                    g.FillPath(dropletBrush, path);
                }

                // Chrome border
                using (Pen chromePen = new Pen(Color.FromArgb(169, 169, 169), 3))
                {
                    g.DrawPath(chromePen, path);
                }
                using (Pen innerChromePen = new Pen(Color.FromArgb(211, 211, 211), 1))
                {
                    Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                    using (GraphicsPath innerPath = GetRoundedRect(innerRect, 18))
                    {
                        g.DrawPath(innerChromePen, innerPath);
                    }
                }
            }

            // Metallic title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            Rectangle titleRect = new Rectangle(18, 6, (int)titleSize.Width + 14, (int)titleSize.Height + 8);
            
            using (LinearGradientBrush titleBrush = new LinearGradientBrush(
                titleRect, Color.FromArgb(220, 220, 220), Color.FromArgb(160, 160, 160), LinearGradientMode.Vertical))
            {
                using (GraphicsPath titlePath = GetRoundedRect(titleRect, 6))
                {
                    g.FillPath(titleBrush, titlePath);
                }
            }
            
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(50, 50, 50)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 25, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // 14. Cosmic/Space GroupBox
    public class CosmicGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer starTimer;
        private int starPhase = 0;

        public CosmicGroupBox()
        {
            starTimer = new System.Windows.Forms.Timer { Interval = 100 };
            starTimer.Tick += (s, e) => { starPhase = (starPhase + 1) % 20; this.Invalidate(); };
            AnimationHelper.StartTimer(starTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Deep space background
                using (LinearGradientBrush spaceBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(10, 0, 20), Color.FromArgb(30, 10, 50), 45f))
                {
                    g.FillPath(spaceBrush, path);
                }

                // Nebula effect
                Random rand = new Random(42);
                for (int i = 0; i < 15; i++)
                {
                    int x = rect.X + rand.Next(0, rect.Width);
                    int y = rect.Y + rand.Next(0, rect.Height);
                    int size = rand.Next(30, 80);
                    
                    Color[] nebulaColors = {
                        Color.FromArgb(30, 138, 43, 226),
                        Color.FromArgb(30, 75, 0, 130),
                        Color.FromArgb(30, 255, 0, 255)
                    };
                    
                    using (GraphicsPath nebula = new GraphicsPath())
                    {
                        nebula.AddEllipse(x - size/2, y - size/2, size, size);
                        using (PathGradientBrush nebulaBrush = new PathGradientBrush(nebula))
                        {
                            nebulaBrush.CenterColor = nebulaColors[i % 3];
                            nebulaBrush.SurroundColors = new Color[] { Color.Transparent };
                            g.FillPath(nebulaBrush, nebula);
                        }
                    }
                }

                // Twinkling stars
                Random starRand = new Random(this.GetHashCode());
                for (int i = 0; i < 30; i++)
                {
                    int x = rect.X + starRand.Next(0, rect.Width);
                    int y = rect.Y + starRand.Next(0, rect.Height);
                    
                    int alpha = (i + starPhase) % 20 < 10 ? 
                        ((i + starPhase) % 10) * 25 : 
                        (10 - ((i + starPhase) % 10)) * 25;
                    
                    using (SolidBrush starBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                    {
                        g.FillEllipse(starBrush, x, y, 2, 2);
                    }
                }

                // Glowing border
                for (int i = 3; i > 0; i--)
                {
                    using (Pen glowPen = new Pen(Color.FromArgb(40, 138, 43, 226), i * 2))
                    {
                        using (GraphicsPath glowPath = GetRoundedRect(
                            new Rectangle(rect.X - i, rect.Y - i, rect.Width + i*2, rect.Height + i*2), 12))
                        {
                            g.DrawPath(glowPen, glowPath);
                        }
                    }
                }

                // Main border
                using (Pen borderPen = new Pen(Color.FromArgb(138, 43, 226), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Cosmic title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(20, 10, 40)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            
            // Glow text
            using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(150, 138, 43, 226)))
            {
                g.DrawString(this.Text, this.Font, glowBrush, 21, 9);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(200, 162, 255)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 20, 8);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && starTimer != null)
            {
                starTimer.Stop();
                starTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // 15. Rainbow Spectrum GroupBox
    public class RainbowSpectrumGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer colorTimer;
        private float hueOffset = 0;
        private Rectangle lastRect;

        public RainbowSpectrumGroupBox()
        {
            colorTimer = new System.Windows.Forms.Timer { Interval = 50 };
            colorTimer.Tick += (s, e) => 
            { 
                hueOffset += 3; 
                if (hueOffset >= 360) hueOffset = 0; 
                
                // Invalidate only the border area to reduce artifacts
                if (this.Parent != null)
                {
                    this.Parent.Invalidate(this.Bounds, false);
                }
                this.Invalidate();
            };
            AnimationHelper.StartTimer(colorTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            
            Rectangle rect = new Rectangle(2, 18, Width - 5, Height - 21);
            lastRect = rect;

            using (GraphicsPath path = GetRoundedRect(rect, 15))
            {
                // Clear background completely
                using (SolidBrush clearBrush = new SolidBrush(this.Parent?.BackColor ?? SystemColors.Control))
                {
                    g.FillRectangle(clearBrush, 0, 0, Width, Height);
                }

                // Background
                using (SolidBrush bgBrush = new SolidBrush(this.BackColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Subtle inner rainbow gradient
                using (LinearGradientBrush rainbowBrush = new LinearGradientBrush(
                    rect, Color.Red, Color.Violet, LinearGradientMode.Horizontal))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Colors = new Color[] {
                        ColorFromHSV((hueOffset + 0) % 360, 0.2, 1.0),
                        ColorFromHSV((hueOffset + 60) % 360, 0.2, 1.0),
                        ColorFromHSV((hueOffset + 120) % 360, 0.2, 1.0),
                        ColorFromHSV((hueOffset + 180) % 360, 0.2, 1.0),
                        ColorFromHSV((hueOffset + 240) % 360, 0.2, 1.0),
                        ColorFromHSV((hueOffset + 300) % 360, 0.2, 1.0)
                    };
                    blend.Positions = new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f };
                    rainbowBrush.InterpolationColors = blend;
                    
                    g.FillPath(rainbowBrush, path);
                }

                // White overlay
                using (SolidBrush whiteBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                {
                    g.FillPath(whiteBrush, path);
                }

                // Rainbow border
                using (LinearGradientBrush borderBrush = new LinearGradientBrush(
                    new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2), 
                    Color.Red, Color.Violet, LinearGradientMode.Horizontal))
                {
                    ColorBlend borderBlend = new ColorBlend();
                    borderBlend.Colors = new Color[] {
                        ColorFromHSV((hueOffset + 0) % 360, 1.0, 1.0),
                        ColorFromHSV((hueOffset + 60) % 360, 1.0, 1.0),
                        ColorFromHSV((hueOffset + 120) % 360, 1.0, 1.0),
                        ColorFromHSV((hueOffset + 180) % 360, 1.0, 1.0),
                        ColorFromHSV((hueOffset + 240) % 360, 1.0, 1.0),
                        ColorFromHSV((hueOffset + 300) % 360, 1.0, 1.0)
                    };
                    borderBlend.Positions = new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f };
                    borderBrush.InterpolationColors = borderBlend;
                    
                    using (Pen rainbowPen = new Pen(borderBrush, 3))
                    {
                        g.DrawPath(rainbowPen, path);
                    }
                }
            }

            // Rainbow title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            Rectangle titleBg = new Rectangle(15, 5, (int)titleSize.Width + 14, (int)titleSize.Height + 8);
            
            using (LinearGradientBrush titleBrush = new LinearGradientBrush(
                titleBg, 
                ColorFromHSV(hueOffset % 360, 0.7, 1.0),
                ColorFromHSV((hueOffset + 180) % 360, 0.7, 1.0),
                LinearGradientMode.Horizontal))
            {
                using (GraphicsPath titlePath = GetRoundedRect(titleBg, 5))
                {
                    g.FillPath(titleBrush, titlePath);
                }
            }
            
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 9);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Don't call base to prevent default background painting
        }

        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && colorTimer != null)
            {
                colorTimer.Stop();
                colorTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // 16. Aurora Borealis GroupBox - Animated northern lights effect
    public class AuroraBorealisGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer auroraTimer;
        private float waveOffset = 0;

        public AuroraBorealisGroupBox()
        {
            auroraTimer = new System.Windows.Forms.Timer { Interval = 50 };
            auroraTimer.Tick += (s, e) => { waveOffset += 2; if (waveOffset >= 360) waveOffset = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(auroraTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Dark night sky background
                using (LinearGradientBrush skyBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(10, 10, 35), Color.FromArgb(25, 25, 60), LinearGradientMode.Vertical))
                {
                    g.FillPath(skyBrush, path);
                }

                // Aurora layers
                Color[] auroraColors = {
                    Color.FromArgb(80, 0, 255, 127),
                    Color.FromArgb(60, 0, 200, 150),
                    Color.FromArgb(50, 100, 0, 255),
                    Color.FromArgb(40, 255, 0, 200)
                };

                for (int layer = 0; layer < 4; layer++)
                {
                    using (GraphicsPath auroraPath = new GraphicsPath())
                    {
                        PointF[] points = new PointF[rect.Width / 4 + 2];
                        points[0] = new PointF(rect.X, rect.Bottom);
                        
                        for (int i = 0; i < points.Length - 2; i++)
                        {
                            float x = rect.X + (i * 4);
                            float wave1 = (float)Math.Sin((waveOffset + i * 8 + layer * 30) * Math.PI / 180) * 20;
                            float wave2 = (float)Math.Sin((waveOffset * 1.5 + i * 12 + layer * 45) * Math.PI / 180) * 15;
                            float y = rect.Y + rect.Height * 0.3f + layer * 25 + wave1 + wave2;
                            points[i + 1] = new PointF(x, Math.Max(rect.Y, Math.Min(rect.Bottom, y)));
                        }
                        points[points.Length - 1] = new PointF(rect.Right, rect.Bottom);

                        auroraPath.AddPolygon(points);
                        using (PathGradientBrush auroraBrush = new PathGradientBrush(auroraPath))
                        {
                            auroraBrush.CenterColor = auroraColors[layer];
                            auroraBrush.SurroundColors = new Color[] { Color.Transparent };
                            g.FillPath(auroraBrush, auroraPath);
                        }
                    }
                }

                // Stars
                Random starRand = new Random(42);
                for (int i = 0; i < 20; i++)
                {
                    int x = rect.X + starRand.Next(rect.Width);
                    int y = rect.Y + starRand.Next(rect.Height / 3);
                    int alpha = 100 + starRand.Next(155);
                    using (SolidBrush starBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                    {
                        g.FillEllipse(starBrush, x, y, 2, 2);
                    }
                }

                // Glowing border
                using (Pen glowPen = new Pen(Color.FromArgb(100, 0, 255, 127), 4))
                {
                    g.DrawPath(glowPen, path);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(0, 200, 150), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 10, 10, 35)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(0, 255, 180)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && auroraTimer != null) { auroraTimer.Stop(); auroraTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 17. Cyber Circuit GroupBox - Circuit board pattern with glowing traces
    public class CyberCircuitGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer pulseTimer;
        private float pulsePhase = 0;

        public CyberCircuitGroupBox()
        {
            pulseTimer = new System.Windows.Forms.Timer { Interval = 60 };
            pulseTimer.Tick += (s, e) => { pulsePhase += 5; if (pulsePhase >= 100) pulsePhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(pulseTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            // Dark PCB background
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(15, 25, 20)))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // Grid pattern
            using (Pen gridPen = new Pen(Color.FromArgb(30, 0, 100, 80), 1))
            {
                for (int x = rect.X; x < rect.Right; x += 20)
                    g.DrawLine(gridPen, x, rect.Y, x, rect.Bottom);
                for (int y = rect.Y; y < rect.Bottom; y += 20)
                    g.DrawLine(gridPen, rect.X, y, rect.Right, y);
            }

            // Circuit traces
            Random rand = new Random(this.GetHashCode());
            Color traceColor = Color.FromArgb(0, 255, 180);
            
            for (int i = 0; i < 8; i++)
            {
                using (GraphicsPath tracePath = new GraphicsPath())
                {
                    int startX = rect.X + rand.Next(rect.Width);
                    int startY = rect.Y + rand.Next(rect.Height);
                    List<Point> points = new List<Point> { new Point(startX, startY) };
                    
                    for (int j = 0; j < 4; j++)
                    {
                        Point last = points[points.Count - 1];
                        bool horizontal = rand.Next(2) == 0;
                        int length = rand.Next(20, 60);
                        int direction = rand.Next(2) == 0 ? 1 : -1;
                        
                        Point next = horizontal ? 
                            new Point(Math.Max(rect.X, Math.Min(rect.Right, last.X + length * direction)), last.Y) :
                            new Point(last.X, Math.Max(rect.Y, Math.Min(rect.Bottom, last.Y + length * direction)));
                        points.Add(next);
                    }
                    
                    if (points.Count > 1)
                    {
                        tracePath.AddLines(points.ToArray());
                        
                        // Animated glow
                        float glowIntensity = (float)Math.Sin((pulsePhase + i * 15) * Math.PI / 50) * 0.5f + 0.5f;
                        int alpha = (int)(100 * glowIntensity);
                        
                        using (Pen glowPen = new Pen(Color.FromArgb(alpha, traceColor), 6))
                        {
                            g.DrawPath(glowPen, tracePath);
                        }
                        using (Pen tracePen = new Pen(Color.FromArgb(150 + (int)(105 * glowIntensity), traceColor), 2))
                        {
                            g.DrawPath(tracePen, tracePath);
                        }
                        
                        // Nodes at corners
                        foreach (Point p in points)
                        {
                            using (SolidBrush nodeBrush = new SolidBrush(Color.FromArgb(200, traceColor)))
                            {
                                g.FillEllipse(nodeBrush, p.X - 3, p.Y - 3, 6, 6);
                            }
                        }
                    }
                }
            }

            // Border with tech corners
            using (Pen borderPen = new Pen(traceColor, 2))
            {
                g.DrawRectangle(borderPen, rect);
                
                // Tech corners
                int cornerSize = 15;
                g.DrawLine(borderPen, rect.X, rect.Y + cornerSize, rect.X + cornerSize, rect.Y);
                g.DrawLine(borderPen, rect.Right - cornerSize, rect.Y, rect.Right, rect.Y + cornerSize);
                g.DrawLine(borderPen, rect.Right, rect.Bottom - cornerSize, rect.Right - cornerSize, rect.Bottom);
                g.DrawLine(borderPen, rect.X + cornerSize, rect.Bottom, rect.X, rect.Bottom - cornerSize);
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(15, 25, 20)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(traceColor))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && pulseTimer != null) { pulseTimer.Stop(); pulseTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 18. Fire Lava GroupBox - Animated flames/lava effect
    public class FireLavaGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer fireTimer;
        private float flamePhase = 0;

        public FireLavaGroupBox()
        {
            fireTimer = new System.Windows.Forms.Timer { Interval = 40 };
            fireTimer.Tick += (s, e) => { flamePhase += 4; if (flamePhase >= 360) flamePhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(fireTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 10))
            {
                // Lava background gradient
                using (LinearGradientBrush lavaBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(80, 0, 0), Color.FromArgb(30, 0, 0), LinearGradientMode.Vertical))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Colors = new Color[] {
                        Color.FromArgb(40, 0, 0),
                        Color.FromArgb(80, 20, 0),
                        Color.FromArgb(120, 40, 0),
                        Color.FromArgb(60, 10, 0)
                    };
                    blend.Positions = new float[] { 0f, 0.3f, 0.7f, 1f };
                    lavaBrush.InterpolationColors = blend;
                    g.FillPath(lavaBrush, path);
                }

                // Animated flames from bottom
                for (int layer = 0; layer < 3; layer++)
                {
                    using (GraphicsPath flamePath = new GraphicsPath())
                    {
                        List<PointF> points = new List<PointF>();
                        points.Add(new PointF(rect.X, rect.Bottom));
                        
                        for (int x = rect.X; x <= rect.Right; x += 8)
                        {
                            float wave1 = (float)Math.Sin((flamePhase + x * 3 + layer * 40) * Math.PI / 180) * 15;
                            float wave2 = (float)Math.Sin((flamePhase * 1.5 + x * 5 + layer * 60) * Math.PI / 180) * 10;
                            float baseHeight = rect.Bottom - 30 - layer * 20;
                            float y = baseHeight + wave1 + wave2;
                            points.Add(new PointF(x, y));
                        }
                        points.Add(new PointF(rect.Right, rect.Bottom));

                        flamePath.AddPolygon(points.ToArray());
                        
                        Color[] flameColors = {
                            Color.FromArgb(180, 255, 100, 0),
                            Color.FromArgb(150, 255, 180, 0),
                            Color.FromArgb(120, 255, 255, 100)
                        };
                        
                        using (PathGradientBrush flameBrush = new PathGradientBrush(flamePath))
                        {
                            flameBrush.CenterColor = flameColors[layer];
                            flameBrush.SurroundColors = new Color[] { Color.FromArgb(60, 255, 80, 0) };
                            g.FillPath(flameBrush, flamePath);
                        }
                    }
                }

                // Ember particles
                Random rand = new Random((int)(flamePhase * 10));
                for (int i = 0; i < 15; i++)
                {
                    float x = rect.X + rand.Next(rect.Width);
                    float baseY = rect.Bottom - 20;
                    float y = baseY - ((flamePhase + i * 37) % 100) * (rect.Height * 0.8f) / 100;
                    int alpha = (int)(200 * (1 - (baseY - y) / (rect.Height * 0.8f)));
                    
                    if (y > rect.Y)
                    {
                        using (SolidBrush emberBrush = new SolidBrush(Color.FromArgb(alpha, 255, 200, 50)))
                        {
                            g.FillEllipse(emberBrush, x, y, 3, 3);
                        }
                    }
                }

                // Glowing border
                using (Pen glowPen = new Pen(Color.FromArgb(100, 255, 100, 0), 6))
                {
                    g.DrawPath(glowPen, path);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(255, 150, 0), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title with fire glow
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(150, 255, 100, 0)))
            {
                g.DrawString(this.Text, this.Font, glowBrush, 23, 11);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(255, 220, 100)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && fireTimer != null) { fireTimer.Stop(); fireTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 19. Matrix Digital Rain GroupBox
    public class MatrixRainGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer matrixTimer;
        private List<MatrixDrop> drops = new List<MatrixDrop>();
        private Random rand = new Random();

        private class MatrixDrop
        {
            public float X, Y, Speed;
            public string Char;
            public int Brightness;
        }

        public MatrixRainGroupBox()
        {
            matrixTimer = new System.Windows.Forms.Timer { Interval = 50 };
            matrixTimer.Tick += (s, e) => { UpdateDrops(); this.Invalidate(); };
            AnimationHelper.StartTimer(matrixTimer);
        }

        private void UpdateDrops()
        {
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);
            
            // Add new drops
            if (rand.Next(100) < 30 && drops.Count < 50)
            {
                drops.Add(new MatrixDrop
                {
                    X = rect.X + rand.Next(rect.Width),
                    Y = rect.Y,
                    Speed = 2 + rand.Next(4),
                    Char = ((char)(rand.Next(33, 127))).ToString(),
                    Brightness = 255
                });
            }

            // Update existing drops
            for (int i = drops.Count - 1; i >= 0; i--)
            {
                drops[i].Y += drops[i].Speed;
                if (rand.Next(100) < 20) 
                    drops[i].Char = ((char)(rand.Next(33, 127))).ToString();
                
                if (drops[i].Y > rect.Bottom)
                    drops.RemoveAt(i);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            // Black background
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(5, 5, 5)))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // Matrix rain
            using (Font matrixFont = new Font("Consolas", 10, FontStyle.Bold))
            {
                foreach (var drop in drops)
                {
                    // Trail
                    for (int t = 0; t < 8; t++)
                    {
                        float trailY = drop.Y - t * 12;
                        if (trailY >= rect.Y && trailY < rect.Bottom)
                        {
                            int alpha = 255 - t * 30;
                            Color color = t == 0 ? 
                                Color.FromArgb(alpha, 200, 255, 200) : 
                                Color.FromArgb(alpha, 0, 180, 0);
                            
                            using (SolidBrush charBrush = new SolidBrush(color))
                            {
                                string c = ((char)(rand.Next(33, 127))).ToString();
                                g.DrawString(t == 0 ? drop.Char : c, matrixFont, charBrush, drop.X, trailY);
                            }
                        }
                    }
                }
            }

            // Glowing green border
            using (Pen glowPen = new Pen(Color.FromArgb(60, 0, 255, 0), 6))
            {
                g.DrawRectangle(glowPen, rect);
            }
            using (Pen borderPen = new Pen(Color.FromArgb(0, 200, 0), 2))
            {
                g.DrawRectangle(borderPen, rect);
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(240, 5, 5, 5)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(0, 255, 0)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && matrixTimer != null) { matrixTimer.Stop(); matrixTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 20. Crystal Ice GroupBox
    public class CrystalIceGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer shimmerTimer;
        private float shimmerPhase = 0;

        public CrystalIceGroupBox()
        {
            shimmerTimer = new System.Windows.Forms.Timer { Interval = 80 };
            shimmerTimer.Tick += (s, e) => { shimmerPhase += 3; if (shimmerPhase >= 360) shimmerPhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(shimmerTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 15))
            {
                // Ice gradient background
                using (LinearGradientBrush iceBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(220, 240, 255), Color.FromArgb(180, 220, 245), LinearGradientMode.Vertical))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Colors = new Color[] {
                        Color.FromArgb(240, 250, 255),
                        Color.FromArgb(200, 230, 250),
                        Color.FromArgb(180, 220, 245),
                        Color.FromArgb(160, 210, 240)
                    };
                    blend.Positions = new float[] { 0f, 0.3f, 0.7f, 1f };
                    iceBrush.InterpolationColors = blend;
                    g.FillPath(iceBrush, path);
                }

                // Frost crystal patterns
                Random rand = new Random(this.GetHashCode());
                for (int i = 0; i < 12; i++)
                {
                    int cx = rect.X + rand.Next(rect.Width);
                    int cy = rect.Y + rand.Next(rect.Height);
                    float size = rand.Next(20, 50);
                    float shimmer = (float)Math.Sin((shimmerPhase + i * 30) * Math.PI / 180) * 0.3f + 0.7f;
                    
                    using (Pen crystalPen = new Pen(Color.FromArgb((int)(80 * shimmer), 150, 200, 255), 1))
                    {
                        // Draw 6-pointed crystal
                        for (int a = 0; a < 6; a++)
                        {
                            float angle = a * 60 * (float)Math.PI / 180;
                            float endX = cx + (float)Math.Cos(angle) * size;
                            float endY = cy + (float)Math.Sin(angle) * size;
                            g.DrawLine(crystalPen, cx, cy, endX, endY);
                            
                            // Branches
                            float branchX = cx + (float)Math.Cos(angle) * size * 0.6f;
                            float branchY = cy + (float)Math.Sin(angle) * size * 0.6f;
                            for (int b = -1; b <= 1; b += 2)
                            {
                                float branchAngle = angle + b * 45 * (float)Math.PI / 180;
                                float branchEndX = branchX + (float)Math.Cos(branchAngle) * size * 0.3f;
                                float branchEndY = branchY + (float)Math.Sin(branchAngle) * size * 0.3f;
                                g.DrawLine(crystalPen, branchX, branchY, branchEndX, branchEndY);
                            }
                        }
                    }
                }

                // Sparkle highlights
                for (int i = 0; i < 8; i++)
                {
                    int sx = rect.X + rand.Next(rect.Width);
                    int sy = rect.Y + rand.Next(rect.Height);
                    float sparkle = (float)Math.Sin((shimmerPhase * 2 + i * 45) * Math.PI / 180);
                    
                    if (sparkle > 0.5f)
                    {
                        int alpha = (int)((sparkle - 0.5f) * 2 * 200);
                        using (SolidBrush sparkleBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                        {
                            g.FillEllipse(sparkleBrush, sx - 2, sy - 2, 4, 4);
                        }
                    }
                }

                // Icy border
                using (Pen outerPen = new Pen(Color.FromArgb(100, 130, 180, 220), 4))
                {
                    g.DrawPath(outerPen, path);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(150, 180, 220), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (LinearGradientBrush titleBgBrush = new LinearGradientBrush(
                new Rectangle(15, 6, (int)titleSize.Width + 14, (int)titleSize.Height + 8),
                Color.FromArgb(230, 240, 250), Color.FromArgb(200, 220, 240), LinearGradientMode.Vertical))
            {
                g.FillRectangle(titleBgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(50, 100, 150)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && shimmerTimer != null) { shimmerTimer.Stop(); shimmerTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 21. Plasma Energy GroupBox
    public class PlasmaEnergyGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer plasmaTimer;
        private float plasmaPhase = 0;

        public PlasmaEnergyGroupBox()
        {
            plasmaTimer = new System.Windows.Forms.Timer { Interval = 30 };
            plasmaTimer.Tick += (s, e) => { plasmaPhase += 2; if (plasmaPhase >= 360) plasmaPhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(plasmaTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 15))
            {
                // Dark background
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(10, 0, 20)))
                {
                    g.FillPath(bgBrush, path);
                }

                // Plasma blobs
                for (int i = 0; i < 6; i++)
                {
                    float angle = (plasmaPhase + i * 60) * (float)Math.PI / 180;
                    float radius = rect.Width * 0.25f;
                    float cx = rect.X + rect.Width / 2 + (float)Math.Cos(angle) * radius * 0.5f;
                    float cy = rect.Y + rect.Height / 2 + (float)Math.Sin(angle * 1.5f) * radius * 0.3f;
                    float blobSize = 40 + (float)Math.Sin(angle * 2) * 20;

                    Color[] plasmaColors = {
                        Color.FromArgb(150, 255, 0, 128),
                        Color.FromArgb(150, 128, 0, 255),
                        Color.FromArgb(150, 0, 128, 255),
                        Color.FromArgb(150, 255, 128, 0),
                        Color.FromArgb(150, 0, 255, 128),
                        Color.FromArgb(150, 255, 0, 255)
                    };

                    using (GraphicsPath blob = new GraphicsPath())
                    {
                        blob.AddEllipse(cx - blobSize/2, cy - blobSize/2, blobSize, blobSize);
                        using (PathGradientBrush blobBrush = new PathGradientBrush(blob))
                        {
                            blobBrush.CenterColor = plasmaColors[i];
                            blobBrush.SurroundColors = new Color[] { Color.Transparent };
                            g.FillPath(blobBrush, blob);
                        }
                    }
                }

                // Energy arc
                using (Pen arcPen = new Pen(Color.FromArgb(150, 200, 100, 255), 2))
                {
                    float startAngle = plasmaPhase;
                    float sweepAngle = 120 + (float)Math.Sin(plasmaPhase * Math.PI / 180) * 60;
                    Rectangle arcRect = new Rectangle(rect.X + 20, rect.Y + 20, rect.Width - 40, rect.Height - 40);
                    if (arcRect.Width > 0 && arcRect.Height > 0)
                        g.DrawArc(arcPen, arcRect, startAngle, sweepAngle);
                }

                // Glowing border
                for (int i = 3; i > 0; i--)
                {
                    Color glowColor = Color.FromArgb(40, 200, 100, 255);
                    using (Pen glowPen = new Pen(glowColor, i * 3))
                    {
                        g.DrawPath(glowPen, path);
                    }
                }
                using (Pen borderPen = new Pen(Color.FromArgb(200, 100, 255), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 10, 0, 20)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(220, 150, 255)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && plasmaTimer != null) { plasmaTimer.Stop(); plasmaTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 22. Ocean Wave GroupBox
    public class OceanWaveGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer waveTimer;
        private float wavePhase = 0;

        public OceanWaveGroupBox()
        {
            waveTimer = new System.Windows.Forms.Timer { Interval = 40 };
            waveTimer.Tick += (s, e) => { wavePhase += 3; if (wavePhase >= 360) wavePhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(waveTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Ocean gradient
                using (LinearGradientBrush oceanBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(0, 100, 150), Color.FromArgb(0, 50, 100), LinearGradientMode.Vertical))
                {
                    g.FillPath(oceanBrush, path);
                }

                // Multiple wave layers
                Color[] waveColors = {
                    Color.FromArgb(80, 100, 200, 255),
                    Color.FromArgb(100, 50, 150, 200),
                    Color.FromArgb(120, 0, 100, 150)
                };

                for (int layer = 0; layer < 3; layer++)
                {
                    using (GraphicsPath wavePath = new GraphicsPath())
                    {
                        List<PointF> points = new List<PointF>();
                        points.Add(new PointF(rect.X, rect.Bottom));

                        float baseY = rect.Y + rect.Height * 0.4f + layer * 25;
                        for (int x = rect.X; x <= rect.Right; x += 4)
                        {
                            float wave1 = (float)Math.Sin((wavePhase + x * 2 + layer * 30) * Math.PI / 180) * 12;
                            float wave2 = (float)Math.Sin((wavePhase * 0.7f + x * 4 + layer * 60) * Math.PI / 180) * 6;
                            float y = baseY + wave1 + wave2;
                            points.Add(new PointF(x, y));
                        }

                        points.Add(new PointF(rect.Right, rect.Bottom));
                        wavePath.AddPolygon(points.ToArray());

                        using (SolidBrush waveBrush = new SolidBrush(waveColors[layer]))
                        {
                            g.FillPath(waveBrush, wavePath);
                        }
                    }
                }

                // Foam highlights
                Random rand = new Random((int)(wavePhase / 10));
                for (int i = 0; i < 10; i++)
                {
                    float x = rect.X + rand.Next(rect.Width);
                    float baseY = rect.Y + rect.Height * 0.35f;
                    float wave = (float)Math.Sin((wavePhase + x * 2) * Math.PI / 180) * 12;
                    float y = baseY + wave + rand.Next(-5, 5);
                    int alpha = 100 + rand.Next(100);
                    
                    using (SolidBrush foamBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                    {
                        g.FillEllipse(foamBrush, x, y, rand.Next(3, 8), rand.Next(2, 5));
                    }
                }

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(0, 150, 200), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 0, 80, 120)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(180, 230, 255)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && waveTimer != null) { waveTimer.Stop(); waveTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 23. Electric Storm GroupBox
    public class ElectricStormGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer stormTimer;
        private float stormPhase = 0;
        private List<LightningBolt> bolts = new List<LightningBolt>();
        private Random rand = new Random();

        private class LightningBolt
        {
            public List<PointF> Points;
            public int Life;
        }

        public ElectricStormGroupBox()
        {
            stormTimer = new System.Windows.Forms.Timer { Interval = 50 };
            stormTimer.Tick += (s, e) => { UpdateStorm(); this.Invalidate(); };
            AnimationHelper.StartTimer(stormTimer);
        }

        private void UpdateStorm()
        {
            stormPhase += 2;
            if (stormPhase >= 360) stormPhase = 0;

            // Random lightning
            if (rand.Next(100) < 15 && bolts.Count < 3)
            {
                Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);
                var bolt = new LightningBolt { Points = new List<PointF>(), Life = 8 };
                
                float x = rect.X + rand.Next(rect.Width);
                float y = rect.Y;
                bolt.Points.Add(new PointF(x, y));

                while (y < rect.Bottom - 20)
                {
                    x += rand.Next(-30, 30);
                    y += rand.Next(10, 30);
                    x = Math.Max(rect.X, Math.Min(rect.Right, x));
                    bolt.Points.Add(new PointF(x, y));
                }
                bolts.Add(bolt);
            }

            // Decay bolts
            for (int i = bolts.Count - 1; i >= 0; i--)
            {
                bolts[i].Life--;
                if (bolts[i].Life <= 0) bolts.RemoveAt(i);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 10))
            {
                // Storm clouds gradient
                using (LinearGradientBrush stormBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(30, 30, 50), Color.FromArgb(60, 60, 80), LinearGradientMode.Vertical))
                {
                    g.FillPath(stormBrush, path);
                }

                // Cloud wisps
                Random cloudRand = new Random(42);
                for (int i = 0; i < 8; i++)
                {
                    float cx = rect.X + cloudRand.Next(rect.Width);
                    float cy = rect.Y + cloudRand.Next(rect.Height / 3);
                    float size = 30 + cloudRand.Next(40);
                    float pulse = (float)Math.Sin((stormPhase + i * 20) * Math.PI / 180) * 0.3f + 0.7f;
                    
                    using (GraphicsPath cloud = new GraphicsPath())
                    {
                        cloud.AddEllipse(cx - size/2, cy - size/3, size, size * 0.6f);
                        using (PathGradientBrush cloudBrush = new PathGradientBrush(cloud))
                        {
                            cloudBrush.CenterColor = Color.FromArgb((int)(60 * pulse), 80, 80, 100);
                            cloudBrush.SurroundColors = new Color[] { Color.Transparent };
                            g.FillPath(cloudBrush, cloud);
                        }
                    }
                }

                // Lightning bolts
                foreach (var bolt in bolts)
                {
                    if (bolt.Points.Count > 1)
                    {
                        int alpha = bolt.Life * 30;
                        
                        // Glow
                        using (Pen glowPen = new Pen(Color.FromArgb(alpha / 2, 100, 150, 255), 8))
                        {
                            g.DrawLines(glowPen, bolt.Points.ToArray());
                        }
                        
                        // Core
                        using (Pen boltPen = new Pen(Color.FromArgb(alpha, 200, 220, 255), 2))
                        {
                            g.DrawLines(boltPen, bolt.Points.ToArray());
                        }
                        
                        // Bright center
                        using (Pen corePen = new Pen(Color.FromArgb(alpha, 255, 255, 255), 1))
                        {
                            g.DrawLines(corePen, bolt.Points.ToArray());
                        }
                    }
                }

                // Electric border
                float borderPulse = (float)Math.Sin(stormPhase * Math.PI / 45) * 0.3f + 0.7f;
                using (Pen glowPen = new Pen(Color.FromArgb((int)(60 * borderPulse), 100, 150, 255), 4))
                {
                    g.DrawPath(glowPen, path);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(100, 150, 200), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 40, 40, 60)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(180, 200, 255)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && stormTimer != null) { stormTimer.Stop(); stormTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 24. Starfield Warp GroupBox
    public class StarfieldWarpGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer warpTimer;
        private List<Star> stars = new List<Star>();
        private Random rand = new Random();

        private class Star { public float X, Y, Z, Speed; }

        public StarfieldWarpGroupBox()
        {
            for (int i = 0; i < 80; i++)
            {
                stars.Add(new Star
                {
                    X = rand.Next(-200, 200),
                    Y = rand.Next(-150, 150),
                    Z = rand.Next(1, 200),
                    Speed = 2 + rand.Next(3)
                });
            }

            warpTimer = new System.Windows.Forms.Timer { Interval = 30 };
            warpTimer.Tick += (s, e) => { UpdateStars(); this.Invalidate(); };
            AnimationHelper.StartTimer(warpTimer);
        }

        private void UpdateStars()
        {
            foreach (var star in stars)
            {
                star.Z -= star.Speed;
                if (star.Z <= 0)
                {
                    star.X = rand.Next(-200, 200);
                    star.Y = rand.Next(-150, 150);
                    star.Z = 200;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Deep space
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(5, 5, 15)))
                {
                    g.FillPath(bgBrush, path);
                }

                float centerX = rect.X + rect.Width / 2;
                float centerY = rect.Y + rect.Height / 2;

                // Stars with warp streaks
                foreach (var star in stars)
                {
                    float scale = 200f / star.Z;
                    float screenX = centerX + star.X * scale;
                    float screenY = centerY + star.Y * scale;

                    if (screenX >= rect.X && screenX <= rect.Right && screenY >= rect.Y && screenY <= rect.Bottom)
                    {
                        float size = Math.Max(1, 4 - star.Z / 50);
                        int alpha = (int)(255 * (1 - star.Z / 200));

                        // Streak
                        float streakScale = 200f / (star.Z + star.Speed * 3);
                        float streakX = centerX + star.X * streakScale;
                        float streakY = centerY + star.Y * streakScale;

                        using (Pen streakPen = new Pen(Color.FromArgb(alpha / 2, 150, 180, 255), size / 2))
                        {
                            g.DrawLine(streakPen, screenX, screenY, streakX, streakY);
                        }

                        // Star
                        using (SolidBrush starBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                        {
                            g.FillEllipse(starBrush, screenX - size/2, screenY - size/2, size, size);
                        }
                    }
                }

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(100, 150, 200), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 10, 10, 30)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(200, 220, 255)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && warpTimer != null) { warpTimer.Stop(); warpTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 25. Heartbeat Pulse GroupBox
    public class HeartbeatPulseGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer pulseTimer;
        private float pulsePhase = 0;

        public HeartbeatPulseGroupBox()
        {
            pulseTimer = new System.Windows.Forms.Timer { Interval = 30 };
            pulseTimer.Tick += (s, e) => { pulsePhase += 3; if (pulsePhase >= 360) pulsePhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(pulseTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Dark background
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(15, 5, 10)))
                {
                    g.FillPath(bgBrush, path);
                }

                // Grid lines
                using (Pen gridPen = new Pen(Color.FromArgb(30, 255, 50, 100), 1))
                {
                    for (int x = rect.X; x < rect.Right; x += 20)
                        g.DrawLine(gridPen, x, rect.Y, x, rect.Bottom);
                    for (int y = rect.Y; y < rect.Bottom; y += 20)
                        g.DrawLine(gridPen, rect.X, y, rect.Right, y);
                }

                // ECG line
                float centerY = rect.Y + rect.Height / 2;
                using (GraphicsPath ecgPath = new GraphicsPath())
                {
                    List<PointF> points = new List<PointF>();
                    
                    for (int x = rect.X; x <= rect.Right; x += 2)
                    {
                        float phase = (pulsePhase + x) % 120;
                        float y = centerY;

                        if (phase < 10) y = centerY;
                        else if (phase < 15) y = centerY + 5;
                        else if (phase < 20) y = centerY - 30;
                        else if (phase < 25) y = centerY + 20;
                        else if (phase < 30) y = centerY - 5;
                        else if (phase < 40) y = centerY;
                        else if (phase < 50) y = centerY + (float)Math.Sin((phase - 40) * Math.PI / 10) * 8;
                        
                        points.Add(new PointF(x, y));
                    }

                    if (points.Count > 1)
                    {
                        ecgPath.AddLines(points.ToArray());
                        
                        // Glow
                        using (Pen glowPen = new Pen(Color.FromArgb(100, 255, 50, 100), 6))
                        {
                            g.DrawPath(glowPen, ecgPath);
                        }
                        // Line
                        using (Pen ecgPen = new Pen(Color.FromArgb(255, 80, 120), 2))
                        {
                            g.DrawPath(ecgPen, ecgPath);
                        }
                    }
                }

                // Pulsing center effect
                float pulse = (float)Math.Sin(pulsePhase * 2 * Math.PI / 120);
                if (pulse > 0.8f)
                {
                    float size = 30 + (pulse - 0.8f) * 50;
                    using (GraphicsPath pulsePath = new GraphicsPath())
                    {
                        pulsePath.AddEllipse(rect.X + rect.Width/2 - size, centerY - size/2, size*2, size);
                        using (PathGradientBrush pulseBrush = new PathGradientBrush(pulsePath))
                        {
                            pulseBrush.CenterColor = Color.FromArgb((int)((pulse - 0.8f) * 300), 255, 50, 100);
                            pulseBrush.SurroundColors = new Color[] { Color.Transparent };
                            g.FillPath(pulseBrush, pulsePath);
                        }
                    }
                }

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(255, 80, 120), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 15, 5, 10)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(255, 120, 150)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && pulseTimer != null) { pulseTimer.Stop(); pulseTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 26. Snowfall GroupBox - White background with falling snowflakes
    public class SnowfallGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer snowTimer;
        private List<Snowflake> snowflakes = new List<Snowflake>();
        private Random rand = new Random();

        private class Snowflake { public float X, Y, Speed, Size, Sway; }

        public SnowfallGroupBox()
        {
            for (int i = 0; i < 40; i++)
            {
                snowflakes.Add(new Snowflake
                {
                    X = rand.Next(0, 400),
                    Y = rand.Next(0, 300),
                    Speed = 1 + (float)rand.NextDouble() * 2,
                    Size = 2 + (float)rand.NextDouble() * 4,
                    Sway = (float)rand.NextDouble() * 2
                });
            }

            snowTimer = new System.Windows.Forms.Timer { Interval = 40 };
            snowTimer.Tick += (s, e) => { UpdateSnow(); this.Invalidate(); };
            AnimationHelper.StartTimer(snowTimer);
        }

        private void UpdateSnow()
        {
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);
            foreach (var flake in snowflakes)
            {
                flake.Y += flake.Speed;
                flake.X += (float)Math.Sin(flake.Y / 20 + flake.Sway) * 0.5f;
                
                if (flake.Y > rect.Bottom)
                {
                    flake.Y = rect.Y;
                    flake.X = rect.X + rand.Next(rect.Width);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // White/light blue gradient background
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(250, 252, 255), Color.FromArgb(230, 240, 250), LinearGradientMode.Vertical))
                {
                    g.FillPath(bgBrush, path);
                }

                // Snowflakes
                foreach (var flake in snowflakes)
                {
                    if (flake.X >= rect.X && flake.X <= rect.Right && flake.Y >= rect.Y && flake.Y <= rect.Bottom)
                    {
                        using (SolidBrush snowBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                        {
                            g.FillEllipse(snowBrush, flake.X - flake.Size/2, flake.Y - flake.Size/2, flake.Size, flake.Size);
                        }
                        // Sparkle effect
                        using (Pen sparklePen = new Pen(Color.FromArgb(100, 200, 220, 255), 1))
                        {
                            g.DrawLine(sparklePen, flake.X - flake.Size/2, flake.Y, flake.X + flake.Size/2, flake.Y);
                            g.DrawLine(sparklePen, flake.X, flake.Y - flake.Size/2, flake.X, flake.Y + flake.Size/2);
                        }
                    }
                }

                // Subtle border
                using (Pen borderPen = new Pen(Color.FromArgb(180, 200, 220), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(245, 250, 255)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(80, 120, 160)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && snowTimer != null) { snowTimer.Stop(); snowTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 27. Cloud Drift GroupBox - White/blue sky with drifting clouds
    public class CloudDriftGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer cloudTimer;
        private float cloudOffset = 0;

        public CloudDriftGroupBox()
        {
            cloudTimer = new System.Windows.Forms.Timer { Interval = 50 };
            cloudTimer.Tick += (s, e) => { cloudOffset += 0.5f; if (cloudOffset > 200) cloudOffset = -100; this.Invalidate(); };
            AnimationHelper.StartTimer(cloudTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Sky gradient
                using (LinearGradientBrush skyBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(220, 235, 255), Color.FromArgb(245, 250, 255), LinearGradientMode.Vertical))
                {
                    g.FillPath(skyBrush, path);
                }

                // Draw clouds
                DrawCloud(g, rect.X + cloudOffset, rect.Y + 30, 60);
                DrawCloud(g, rect.X + cloudOffset + 120, rect.Y + 60, 45);
                DrawCloud(g, rect.X + cloudOffset - 80, rect.Y + 90, 50);
                DrawCloud(g, rect.X + cloudOffset + 200, rect.Y + 40, 55);

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(180, 210, 240), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(240, 248, 255)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(100, 140, 180)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private void DrawCloud(Graphics g, float x, float y, float size)
        {
            using (SolidBrush cloudBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.FillEllipse(cloudBrush, x, y, size, size * 0.6f);
                g.FillEllipse(cloudBrush, x + size * 0.3f, y - size * 0.2f, size * 0.8f, size * 0.5f);
                g.FillEllipse(cloudBrush, x + size * 0.6f, y, size * 0.7f, size * 0.5f);
                g.FillEllipse(cloudBrush, x - size * 0.2f, y + size * 0.1f, size * 0.6f, size * 0.4f);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && cloudTimer != null) { cloudTimer.Stop(); cloudTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 28. Sparkle Shine GroupBox - White with animated sparkle effects
    public class SparkleShineGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer sparkleTimer;
        private float sparklePhase = 0;

        public SparkleShineGroupBox()
        {
            sparkleTimer = new System.Windows.Forms.Timer { Interval = 60 };
            sparkleTimer.Tick += (s, e) => { sparklePhase += 5; if (sparklePhase >= 360) sparklePhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(sparkleTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 15))
            {
                // Pearl white gradient
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(255, 255, 255), Color.FromArgb(248, 248, 252), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillPath(bgBrush, path);
                }

                // Animated sparkles
                Random rand = new Random(42);
                for (int i = 0; i < 15; i++)
                {
                    int sx = rect.X + rand.Next(rect.Width);
                    int sy = rect.Y + rand.Next(rect.Height);
                    float phase = (sparklePhase + i * 24) % 360;
                    float intensity = (float)Math.Sin(phase * Math.PI / 180);
                    
                    if (intensity > 0)
                    {
                        int alpha = (int)(intensity * 255);
                        float size = 3 + intensity * 4;
                        
                        // Star sparkle
                        using (Pen sparklePen = new Pen(Color.FromArgb(alpha, 255, 215, 0), 2))
                        {
                            g.DrawLine(sparklePen, sx - size, sy, sx + size, sy);
                            g.DrawLine(sparklePen, sx, sy - size, sx, sy + size);
                            g.DrawLine(sparklePen, sx - size*0.7f, sy - size*0.7f, sx + size*0.7f, sy + size*0.7f);
                            g.DrawLine(sparklePen, sx + size*0.7f, sy - size*0.7f, sx - size*0.7f, sy + size*0.7f);
                        }
                        
                        // Center glow
                        using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 200)))
                        {
                            g.FillEllipse(glowBrush, sx - 2, sy - 2, 4, 4);
                        }
                    }
                }

                // Elegant gold border
                using (Pen borderPen = new Pen(Color.FromArgb(200, 218, 165, 32), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title with gold accent
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (LinearGradientBrush titleBgBrush = new LinearGradientBrush(
                new Rectangle(15, 6, (int)titleSize.Width + 14, (int)titleSize.Height + 8),
                Color.FromArgb(255, 250, 240), Color.FromArgb(255, 248, 220), LinearGradientMode.Vertical))
            {
                g.FillRectangle(titleBgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(180, 140, 50)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && sparkleTimer != null) { sparkleTimer.Stop(); sparkleTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 29. Ripple Water GroupBox - Light with water ripple effect
    public class RippleWaterGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer rippleTimer;
        private float ripplePhase = 0;

        public RippleWaterGroupBox()
        {
            rippleTimer = new System.Windows.Forms.Timer { Interval = 40 };
            rippleTimer.Tick += (s, e) => { ripplePhase += 2; if (ripplePhase >= 100) ripplePhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(rippleTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Light aqua gradient
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(240, 255, 255), Color.FromArgb(224, 255, 255), LinearGradientMode.Vertical))
                {
                    g.FillPath(bgBrush, path);
                }

                // Ripple circles
                float centerX = rect.X + rect.Width / 2;
                float centerY = rect.Y + rect.Height / 2;
                
                for (int i = 0; i < 4; i++)
                {
                    float rippleRadius = (ripplePhase + i * 25) % 100 * (Math.Max(rect.Width, rect.Height) / 100f);
                    int alpha = (int)(80 * (1 - (ripplePhase + i * 25) % 100 / 100f));
                    
                    if (rippleRadius > 0 && alpha > 0)
                    {
                        using (Pen ripplePen = new Pen(Color.FromArgb(alpha, 0, 180, 200), 2))
                        {
                            float rx = centerX - rippleRadius;
                            float ry = centerY - rippleRadius;
                            if (rippleRadius * 2 > 0)
                            {
                                g.DrawEllipse(ripplePen, rx, ry, rippleRadius * 2, rippleRadius * 2);
                            }
                        }
                    }
                }

                // Water shimmer lines
                using (Pen shimmerPen = new Pen(Color.FromArgb(40, 0, 150, 180), 1))
                {
                    for (int y = rect.Y + 20; y < rect.Bottom - 10; y += 15)
                    {
                        float wave = (float)Math.Sin((ripplePhase + y) * Math.PI / 30) * 5;
                        g.DrawLine(shimmerPen, rect.X + 10 + wave, y, rect.Right - 10 + wave, y);
                    }
                }

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(0, 180, 200), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(240, 255, 255)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(0, 140, 160)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && rippleTimer != null) { rippleTimer.Stop(); rippleTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 30. Bubbles Float GroupBox - Light with floating soap bubbles
    public class BubblesFloatGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer bubbleTimer;
        private List<Bubble> bubbles = new List<Bubble>();
        private Random rand = new Random();

        private class Bubble { public float X, Y, Size, Speed, Wobble; }

        public BubblesFloatGroupBox()
        {
            for (int i = 0; i < 20; i++)
            {
                bubbles.Add(new Bubble
                {
                    X = rand.Next(0, 400),
                    Y = rand.Next(0, 300),
                    Size = 8 + rand.Next(20),
                    Speed = 0.5f + (float)rand.NextDouble() * 1.5f,
                    Wobble = (float)rand.NextDouble() * 10
                });
            }

            bubbleTimer = new System.Windows.Forms.Timer { Interval = 50 };
            bubbleTimer.Tick += (s, e) => { UpdateBubbles(); this.Invalidate(); };
            AnimationHelper.StartTimer(bubbleTimer);
        }

        private void UpdateBubbles()
        {
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);
            foreach (var bubble in bubbles)
            {
                bubble.Y -= bubble.Speed;
                bubble.X += (float)Math.Sin(bubble.Y / 15 + bubble.Wobble) * 0.5f;
                
                if (bubble.Y < rect.Y - bubble.Size)
                {
                    bubble.Y = rect.Bottom + bubble.Size;
                    bubble.X = rect.X + rand.Next(rect.Width);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Light gradient background
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(255, 253, 250), Color.FromArgb(250, 248, 255), LinearGradientMode.Vertical))
                {
                    g.FillPath(bgBrush, path);
                }

                // Bubbles
                foreach (var bubble in bubbles)
                {
                    if (bubble.X >= rect.X && bubble.X <= rect.Right && bubble.Y >= rect.Y && bubble.Y <= rect.Bottom)
                    {
                        RectangleF bubbleRect = new RectangleF(bubble.X - bubble.Size/2, bubble.Y - bubble.Size/2, bubble.Size, bubble.Size);
                        
                        // Bubble gradient
                        using (GraphicsPath bubblePath = new GraphicsPath())
                        {
                            bubblePath.AddEllipse(bubbleRect);
                            using (PathGradientBrush bubbleBrush = new PathGradientBrush(bubblePath))
                            {
                                bubbleBrush.CenterColor = Color.FromArgb(60, 200, 230, 255);
                                bubbleBrush.SurroundColors = new Color[] { Color.FromArgb(100, 150, 200, 255) };
                                g.FillPath(bubbleBrush, bubblePath);
                            }
                        }
                        
                        // Bubble outline
                        using (Pen outlinePen = new Pen(Color.FromArgb(80, 100, 180, 220), 1))
                        {
                            g.DrawEllipse(outlinePen, bubbleRect);
                        }
                        
                        // Highlight
                        using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
                        {
                            g.FillEllipse(highlightBrush, bubble.X - bubble.Size * 0.25f, bubble.Y - bubble.Size * 0.3f, bubble.Size * 0.3f, bubble.Size * 0.2f);
                        }
                    }
                }

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(180, 200, 220), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(252, 250, 255)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(120, 140, 180)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && bubbleTimer != null) { bubbleTimer.Stop(); bubbleTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 31. Confetti Party GroupBox - White with colorful falling confetti
    public class ConfettiPartyGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer confettiTimer;
        private List<Confetti> confettiList = new List<Confetti>();
        private Random rand = new Random();

        private class Confetti { public float X, Y, Speed, Rotation, RotSpeed; public Color Color; public int Shape; }

        public ConfettiPartyGroupBox()
        {
            Color[] colors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple, Color.Pink, Color.Cyan };
            
            for (int i = 0; i < 30; i++)
            {
                confettiList.Add(new Confetti
                {
                    X = rand.Next(0, 400),
                    Y = rand.Next(0, 300),
                    Speed = 1 + (float)rand.NextDouble() * 2,
                    Rotation = rand.Next(360),
                    RotSpeed = -3 + rand.Next(6),
                    Color = colors[rand.Next(colors.Length)],
                    Shape = rand.Next(3)
                });
            }

            confettiTimer = new System.Windows.Forms.Timer { Interval = 40 };
            confettiTimer.Tick += (s, e) => { UpdateConfetti(); this.Invalidate(); };
            AnimationHelper.StartTimer(confettiTimer);
        }

        private void UpdateConfetti()
        {
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);
            Color[] colors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple, Color.Pink, Color.Cyan };
            
            foreach (var c in confettiList)
            {
                c.Y += c.Speed;
                c.X += (float)Math.Sin(c.Y / 20) * 1.5f;
                c.Rotation += c.RotSpeed;
                
                if (c.Y > rect.Bottom + 10)
                {
                    c.Y = rect.Y - 10;
                    c.X = rect.X + rand.Next(rect.Width);
                    c.Color = colors[rand.Next(colors.Length)];
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // White background
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(255, 255, 255)))
                {
                    g.FillPath(bgBrush, path);
                }

                // Confetti
                foreach (var c in confettiList)
                {
                    if (c.X >= rect.X && c.X <= rect.Right && c.Y >= rect.Y && c.Y <= rect.Bottom)
                    {
                        var state = g.Save();
                        g.TranslateTransform(c.X, c.Y);
                        g.RotateTransform(c.Rotation);
                        
                        using (SolidBrush confettiBrush = new SolidBrush(c.Color))
                        {
                            switch (c.Shape)
                            {
                                case 0: // Rectangle
                                    g.FillRectangle(confettiBrush, -4, -2, 8, 4);
                                    break;
                                case 1: // Circle
                                    g.FillEllipse(confettiBrush, -3, -3, 6, 6);
                                    break;
                                case 2: // Triangle
                                    g.FillPolygon(confettiBrush, new PointF[] { new PointF(0, -4), new PointF(-4, 3), new PointF(4, 3) });
                                    break;
                            }
                        }
                        
                        g.Restore(state);
                    }
                }

                // Festive border
                using (Pen borderPen = new Pen(Color.FromArgb(255, 180, 200), 2))
                {
                    borderPen.DashStyle = DashStyle.Dash;
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (LinearGradientBrush titleBgBrush = new LinearGradientBrush(
                new Rectangle(15, 6, (int)titleSize.Width + 14, (int)titleSize.Height + 8),
                Color.FromArgb(255, 230, 240), Color.FromArgb(230, 240, 255), LinearGradientMode.Horizontal))
            {
                g.FillRectangle(titleBgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(200, 100, 150)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && confettiTimer != null) { confettiTimer.Stop(); confettiTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 32. Sunburst Rays GroupBox - White/yellow with animated sun rays
    public class SunburstRaysGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer sunTimer;
        private float rayPhase = 0;

        public SunburstRaysGroupBox()
        {
            sunTimer = new System.Windows.Forms.Timer { Interval = 50 };
            sunTimer.Tick += (s, e) => { rayPhase += 1; if (rayPhase >= 360) rayPhase = 0; this.Invalidate(); };
            AnimationHelper.StartTimer(sunTimer);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Warm gradient background
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(255, 255, 250), Color.FromArgb(255, 250, 235), LinearGradientMode.Vertical))
                {
                    g.FillPath(bgBrush, path);
                }

                // Sun rays from corner
                float sunX = rect.X + 30;
                float sunY = rect.Y + 30;
                
                for (int i = 0; i < 12; i++)
                {
                    float angle = (rayPhase + i * 30) * (float)Math.PI / 180;
                    float rayLength = 150 + (float)Math.Sin((rayPhase + i * 15) * Math.PI / 90) * 30;
                    float endX = sunX + (float)Math.Cos(angle) * rayLength;
                    float endY = sunY + (float)Math.Sin(angle) * rayLength;
                    
                    int alpha = 40 + (int)((float)Math.Sin((rayPhase + i * 15) * Math.PI / 90) * 30);
                    using (Pen rayPen = new Pen(Color.FromArgb(alpha, 255, 200, 50), 15))
                    {
                        rayPen.StartCap = LineCap.Round;
                        rayPen.EndCap = LineCap.Round;
                        g.DrawLine(rayPen, sunX, sunY, endX, endY);
                    }
                }

                // Sun center
                using (GraphicsPath sunPath = new GraphicsPath())
                {
                    sunPath.AddEllipse(sunX - 25, sunY - 25, 50, 50);
                    using (PathGradientBrush sunBrush = new PathGradientBrush(sunPath))
                    {
                        sunBrush.CenterColor = Color.FromArgb(200, 255, 220, 100);
                        sunBrush.SurroundColors = new Color[] { Color.FromArgb(100, 255, 200, 50) };
                        g.FillPath(sunBrush, sunPath);
                    }
                }

                // Border
                using (Pen borderPen = new Pen(Color.FromArgb(255, 200, 100), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(255, 252, 240)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(200, 150, 50)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && sunTimer != null) { sunTimer.Stop(); sunTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 33. Cherry Blossom GroupBox - White/pink with falling petals
    public class CherryBlossomGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer petalTimer;
        private List<Petal> petals = new List<Petal>();
        private Random rand = new Random();

        private class Petal { public float X, Y, Speed, Rotation, RotSpeed, Size; }

        public CherryBlossomGroupBox()
        {
            for (int i = 0; i < 25; i++)
            {
                petals.Add(new Petal
                {
                    X = rand.Next(0, 400),
                    Y = rand.Next(0, 300),
                    Speed = 0.5f + (float)rand.NextDouble() * 1.5f,
                    Rotation = rand.Next(360),
                    RotSpeed = -2 + rand.Next(4),
                    Size = 6 + rand.Next(8)
                });
            }

            petalTimer = new System.Windows.Forms.Timer { Interval = 45 };
            petalTimer.Tick += (s, e) => { UpdatePetals(); this.Invalidate(); };
            AnimationHelper.StartTimer(petalTimer);
        }

        private void UpdatePetals()
        {
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);
            foreach (var p in petals)
            {
                p.Y += p.Speed;
                p.X += (float)Math.Sin(p.Y / 25) * 1f + 0.3f;
                p.Rotation += p.RotSpeed;
                
                if (p.Y > rect.Bottom + 10)
                {
                    p.Y = rect.Y - 10;
                    p.X = rect.X + rand.Next(rect.Width);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Soft pink/white gradient
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(255, 250, 252), Color.FromArgb(255, 245, 248), LinearGradientMode.Vertical))
                {
                    g.FillPath(bgBrush, path);
                }

                // Petals
                foreach (var p in petals)
                {
                    if (p.X >= rect.X - 10 && p.X <= rect.Right + 10 && p.Y >= rect.Y && p.Y <= rect.Bottom)
                    {
                        var state = g.Save();
                        g.TranslateTransform(p.X, p.Y);
                        g.RotateTransform(p.Rotation);
                        
                        // Petal shape (5 ellipses in a flower pattern)
                        Color[] petalColors = { 
                            Color.FromArgb(180, 255, 182, 193),
                            Color.FromArgb(160, 255, 192, 203),
                            Color.FromArgb(170, 255, 175, 185)
                        };
                        
                        using (SolidBrush petalBrush = new SolidBrush(petalColors[rand.Next(3)]))
                        {
                            g.FillEllipse(petalBrush, -p.Size/2, -p.Size/4, p.Size, p.Size/2);
                        }
                        
                        g.Restore(state);
                    }
                }

                // Branch accent in corner
                using (Pen branchPen = new Pen(Color.FromArgb(100, 139, 90, 43), 2))
                {
                    g.DrawBezier(branchPen, rect.Right - 50, rect.Y, rect.Right - 30, rect.Y + 40, rect.Right - 20, rect.Y + 60, rect.Right - 10, rect.Y + 80);
                }

                // Soft pink border
                using (Pen borderPen = new Pen(Color.FromArgb(255, 182, 193), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(255, 248, 250)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(200, 130, 150)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && petalTimer != null) { petalTimer.Stop(); petalTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // 34. Floating Hearts GroupBox - White/pink with floating hearts
    public class FloatingHeartsGroupBox : CustomGroupBoxBase
    {
        private System.Windows.Forms.Timer heartTimer;
        private List<Heart> hearts = new List<Heart>();
        private Random rand = new Random();

        private class Heart { public float X, Y, Speed, Size, Wobble; public Color Color; }

        public FloatingHeartsGroupBox()
        {
            Color[] colors = { 
                Color.FromArgb(255, 105, 180), Color.FromArgb(255, 20, 147), 
                Color.FromArgb(255, 182, 193), Color.FromArgb(220, 20, 60) 
            };
            
            for (int i = 0; i < 15; i++)
            {
                hearts.Add(new Heart
                {
                    X = rand.Next(0, 400),
                    Y = rand.Next(0, 300),
                    Speed = 0.5f + (float)rand.NextDouble() * 1f,
                    Size = 8 + rand.Next(12),
                    Wobble = (float)rand.NextDouble() * 5,
                    Color = colors[rand.Next(colors.Length)]
                });
            }

            heartTimer = new System.Windows.Forms.Timer { Interval = 50 };
            heartTimer.Tick += (s, e) => { UpdateHearts(); this.Invalidate(); };
            AnimationHelper.StartTimer(heartTimer);
        }

        private void UpdateHearts()
        {
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);
            Color[] colors = { 
                Color.FromArgb(255, 105, 180), Color.FromArgb(255, 20, 147), 
                Color.FromArgb(255, 182, 193), Color.FromArgb(220, 20, 60) 
            };
            
            foreach (var h in hearts)
            {
                h.Y -= h.Speed;
                h.X += (float)Math.Sin(h.Y / 20 + h.Wobble) * 0.8f;
                
                if (h.Y < rect.Y - h.Size)
                {
                    h.Y = rect.Bottom + h.Size;
                    h.X = rect.X + rand.Next(rect.Width);
                    h.Color = colors[rand.Next(colors.Length)];
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(3, 20, Width - 10, Height - 26);

            using (GraphicsPath path = GetRoundedRect(rect, 12))
            {
                // Soft white/pink gradient
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    rect, Color.FromArgb(255, 255, 255), Color.FromArgb(255, 245, 250), LinearGradientMode.Vertical))
                {
                    g.FillPath(bgBrush, path);
                }

                // Hearts
                foreach (var h in hearts)
                {
                    if (h.X >= rect.X && h.X <= rect.Right && h.Y >= rect.Y && h.Y <= rect.Bottom)
                    {
                        DrawHeart(g, h.X, h.Y, h.Size, h.Color);
                    }
                }

                // Pink border
                using (Pen borderPen = new Pen(Color.FromArgb(255, 150, 180), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Title
            SizeF titleSize = g.MeasureString(this.Text, this.Font);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(255, 250, 252)))
            {
                g.FillRectangle(bgBrush, 15, 6, titleSize.Width + 14, titleSize.Height + 8);
            }
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(220, 100, 140)))
            {
                g.DrawString(this.Text, new Font(this.Font, FontStyle.Bold), textBrush, 22, 10);
            }
        }

        private void DrawHeart(Graphics g, float x, float y, float size, Color color)
        {
            using (GraphicsPath heartPath = new GraphicsPath())
            {
                float w = size;
                float h = size;
                heartPath.AddBezier(x, y + h * 0.3f, x - w * 0.5f, y - h * 0.3f, x - w * 0.5f, y + h * 0.3f, x, y + h);
                heartPath.AddBezier(x, y + h, x + w * 0.5f, y + h * 0.3f, x + w * 0.5f, y - h * 0.3f, x, y + h * 0.3f);
                
                using (SolidBrush heartBrush = new SolidBrush(Color.FromArgb(180, color)))
                {
                    g.FillPath(heartBrush, heartPath);
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && heartTimer != null) { heartTimer.Stop(); heartTimer.Dispose(); }
            base.Dispose(disposing);
        }
    }
}

