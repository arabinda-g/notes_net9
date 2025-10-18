using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Notes
{
    // Base class for custom group boxes
    public class CustomGroupBoxBase : GroupBox
    {
        private Panel resizeHandlePanel;
        private bool isResizing = false;
        private Point resizeStart;
        private Size originalSize;
        private const int RESIZE_HANDLE_SIZE = 16;

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
            animationTimer.Start();
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
            starTimer.Start();
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
            colorTimer.Start();
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
}

