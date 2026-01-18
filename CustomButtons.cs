using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Notes
{
    // Base custom button with double-click support
    public class CustomButtonBase : Button
    {
        public CustomButtonBase()
        {
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
    }

    // 1. Gradient Button with Shadow
    public class GradientButton : CustomButtonBase
    {
        private Color _gradientTop = Color.FromArgb(100, 149, 237);
        private Color _gradientBottom = Color.FromArgb(65, 105, 225);
        private Color _borderColor = Color.FromArgb(25, 25, 112);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GradientTop
        {
            get => _gradientTop;
            set { _gradientTop = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GradientBottom
        {
            get => _gradientBottom;
            set { _gradientBottom = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush clearBrush = new SolidBrush(Parent?.BackColor ?? BackColor))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Draw shadow
            using (GraphicsPath shadowPath = CreateRoundedRectangle(rect, 8))
            {
                Rectangle shadowRect = new Rectangle(3, 3, Width - 1, Height - 1);
                using (GraphicsPath shadow = CreateRoundedRectangle(shadowRect, 8))
                {
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    {
                        g.FillPath(shadowBrush, shadow);
                    }
                }
            }

            // Draw gradient background
            using (GraphicsPath path = CreateRoundedRectangle(rect, 8))
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, _gradientTop, _gradientBottom, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }

                // Draw glossy overlay
                Rectangle glossRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2);
                using (LinearGradientBrush glossBrush = new LinearGradientBrush(
                    glossRect, 
                    Color.FromArgb(80, 255, 255, 255), 
                    Color.FromArgb(0, 255, 255, 255), 
                    LinearGradientMode.Vertical))
                {
                    using (GraphicsPath glossPath = CreateRoundedRectangle(glossRect, 8))
                    {
                        g.FillPath(glossBrush, glossPath);
                    }
                }

                // Draw border
                using (Pen pen = new Pen(_borderColor, 2))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, rect, ForeColor, 
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top left
            path.AddArc(arc, 180, 90);
            // Top right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            // Bottom right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // Bottom left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }

    // 2. Neon Glow Button
    public class NeonGlowButton : CustomButtonBase
    {
        private Color _glowColor = Color.FromArgb(0, 255, 127);
        private bool _isHovered = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color GlowColor
        {
            get => _glowColor;
            set { _glowColor = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(4, 4, Width - 9, Height - 9);

            // Draw outer glow
            int glowSize = _isHovered ? 8 : 5;
            for (int i = glowSize; i > 0; i--)
            {
                int alpha = (int)(30 * (1 - (float)i / glowSize));
                using (Pen pen = new Pen(Color.FromArgb(alpha, _glowColor), i * 2))
                {
                    Rectangle glowRect = new Rectangle(
                        rect.X - i, rect.Y - i,
                        rect.Width + i * 2, rect.Height + i * 2);
                    g.DrawRectangle(pen, glowRect);
                }
            }

            // Draw background
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, rect);
            }

            // Draw border
            using (Pen pen = new Pen(_glowColor, _isHovered ? 3 : 2))
            {
                g.DrawRectangle(pen, rect);
            }

            // Draw inner glow line
            Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
            using (Pen pen = new Pen(Color.FromArgb(100, _glowColor), 1))
            {
                g.DrawRectangle(pen, innerRect);
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, rect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }
    }

    // 3. Modern Flat Material Button
    public class MaterialButton : CustomButtonBase
    {
        private bool _isPressed = false;
        private Point _ripplePoint;
        private float _rippleSize = 0;
        private System.Windows.Forms.Timer _rippleTimer;

        public MaterialButton()
        {
            _rippleTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _rippleTimer.Tick += RippleTimer_Tick;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _isPressed = true;
            _ripplePoint = e.Location;
            _rippleSize = 0;
            _rippleTimer.Start();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        private void RippleTimer_Tick(object sender, EventArgs e)
        {
            _rippleSize += 10;
            if (_rippleSize > Math.Max(Width, Height) * 2)
            {
                _rippleTimer.Stop();
                _rippleSize = 0;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Draw shadow
            if (!_isPressed)
            {
                using (GraphicsPath shadowPath = CreateRoundedRectangle(new Rectangle(2, 2, Width - 1, Height - 1), 4))
                {
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                    {
                        g.FillPath(shadowBrush, shadowPath);
                    }
                }
            }

            // Draw background
            Rectangle bgRect = _isPressed ? new Rectangle(1, 1, Width - 3, Height - 3) : rect;
            using (GraphicsPath path = CreateRoundedRectangle(bgRect, 4))
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw ripple effect
            if (_rippleSize > 0)
            {
                using (GraphicsPath ripplePath = new GraphicsPath())
                {
                    ripplePath.AddEllipse(
                        _ripplePoint.X - _rippleSize / 2,
                        _ripplePoint.Y - _rippleSize / 2,
                        _rippleSize, _rippleSize);

                    using (PathGradientBrush rippleBrush = new PathGradientBrush(ripplePath))
                    {
                        rippleBrush.CenterColor = Color.FromArgb(50, 255, 255, 255);
                        rippleBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                        g.FillPath(rippleBrush, ripplePath);
                    }
                }
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, bgRect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rippleTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // 4. Glass Morphism Button
    public class GlassMorphismButton : CustomButtonBase
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Draw frosted glass effect
            using (GraphicsPath path = CreateRoundedRectangle(rect, 12))
            {
                // Semi-transparent background
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(100, BackColor)))
                {
                    g.FillPath(bgBrush, path);
                }

                // Light reflection on top
                Rectangle topRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 3);
                using (LinearGradientBrush topBrush = new LinearGradientBrush(
                    topRect,
                    Color.FromArgb(60, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    using (GraphicsPath topPath = CreateRoundedRectangle(topRect, 12))
                    {
                        g.FillPath(topBrush, topPath);
                    }
                }

                // Border with slight blur effect
                using (Pen borderPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1.5f))
                {
                    g.DrawPath(borderPen, path);
                }

                // Inner highlight
                Rectangle innerRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                using (GraphicsPath innerPath = CreateRoundedRectangle(innerRect, 11))
                {
                    using (Pen innerPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1))
                    {
                        g.DrawPath(innerPen, innerPath);
                    }
                }
            }

            // Draw text with slight shadow for depth
            using (SolidBrush textShadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
            {
                Rectangle shadowRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width, rect.Height);
                TextRenderer.DrawText(g, Text, Font, shadowRect, Color.FromArgb(100, 0, 0, 0),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }

            TextRenderer.DrawText(g, Text, Font, rect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }

    // 5. Neumorphism Button (Soft UI)
    public class NeumorphismButton : CustomButtonBase
    {
        private bool _isPressed = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(5, 5, Width - 11, Height - 11);

            using (GraphicsPath path = CreateRoundedRectangle(rect, 15))
            {
                // Draw shadows for depth
                if (!_isPressed)
                {
                    // Dark shadow (bottom-right)
                    using (GraphicsPath darkShadow = CreateRoundedRectangle(
                        new Rectangle(rect.X + 6, rect.Y + 6, rect.Width, rect.Height), 15))
                    {
                        using (SolidBrush darkBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                        {
                            g.FillPath(darkBrush, darkShadow);
                        }
                    }

                    // Light shadow (top-left)
                    using (GraphicsPath lightShadow = CreateRoundedRectangle(
                        new Rectangle(rect.X - 4, rect.Y - 4, rect.Width, rect.Height), 15))
                    {
                        using (SolidBrush lightBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                        {
                            g.FillPath(lightBrush, lightShadow);
                        }
                    }
                }

                // Draw background
                using (SolidBrush bgBrush = new SolidBrush(BackColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Inner shadow when pressed
                if (_isPressed)
                {
                    Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                    using (GraphicsPath innerPath = CreateRoundedRectangle(innerRect, 13))
                    {
                        using (Pen innerPen = new Pen(Color.FromArgb(40, 0, 0, 0), 2))
                        {
                            g.DrawPath(innerPen, innerPath);
                        }
                    }
                }
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, rect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }

    // 6. Retro 3D Button
    public class Retro3DButton : CustomButtonBase
    {
        private bool _isPressed = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None; // Pixel-perfect for retro look
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            int offset = _isPressed ? 4 : 0;
            int depth = 6;

            Rectangle mainRect = new Rectangle(offset, offset, Width - depth - 1, Height - depth - 1);

            // Draw 3D depth layers
            if (!_isPressed)
            {
                for (int i = depth; i > 0; i--)
                {
                    int brightness = 255 - (i * 30);
                    Color layerColor = Color.FromArgb(
                        Math.Max(0, BackColor.R - brightness / 2),
                        Math.Max(0, BackColor.G - brightness / 2),
                        Math.Max(0, BackColor.B - brightness / 2));

                    using (SolidBrush layerBrush = new SolidBrush(layerColor))
                    {
                        Rectangle layerRect = new Rectangle(i, i, mainRect.Width, mainRect.Height);
                        g.FillRectangle(layerBrush, layerRect);
                    }
                }
            }

            // Draw main face
            using (SolidBrush faceBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(faceBrush, mainRect);
            }

            // Draw highlight (top and left)
            Color highlightColor = ControlPaint.Light(BackColor);
            using (Pen highlightPen = new Pen(highlightColor, 2))
            {
                g.DrawLine(highlightPen, mainRect.Left, mainRect.Top, mainRect.Right, mainRect.Top);
                g.DrawLine(highlightPen, mainRect.Left, mainRect.Top, mainRect.Left, mainRect.Bottom);
            }

            // Draw shadow (bottom and right)
            Color shadowColor = ControlPaint.Dark(BackColor);
            using (Pen shadowPen = new Pen(shadowColor, 2))
            {
                g.DrawLine(shadowPen, mainRect.Right, mainRect.Top, mainRect.Right, mainRect.Bottom);
                g.DrawLine(shadowPen, mainRect.Left, mainRect.Bottom, mainRect.Right, mainRect.Bottom);
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, mainRect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }
    }

    // 7. Premium Card Button
    public class PremiumCardButton : CustomButtonBase
    {
        private bool _isHovered = false;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush clearBrush = new SolidBrush(Parent?.BackColor ?? BackColor))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            int shadowOffset = _isHovered ? 8 : 4;
            Rectangle rect = new Rectangle(2, 2, Width - shadowOffset - 2, Height - shadowOffset - 2);

            // Draw deep shadow
            using (GraphicsPath shadowPath = CreateRoundedRectangle(
                new Rectangle(shadowOffset, shadowOffset, rect.Width, rect.Height), 10))
            {
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Draw card background
            using (GraphicsPath path = CreateRoundedRectangle(rect, 10))
            {
                using (SolidBrush bgBrush = new SolidBrush(BackColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Draw border
                using (Pen borderPen = new Pen(ControlPaint.Light(BackColor), 2))
                {
                    g.DrawPath(borderPen, path);
                }

                // Accent line on top
                using (Pen accentPen = new Pen(ForeColor, 3))
                {
                    g.DrawLine(accentPen, rect.Left + 10, rect.Top, rect.Right - 10, rect.Top);
                }
            }

            // Draw text
            Rectangle textRect = new Rectangle(rect.X, rect.Y + 5, rect.Width, rect.Height - 5);
            TextRenderer.DrawText(g, Text, Font, textRect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }

    // 8. Outline Button (Modern Minimal)
    public class OutlineButton : CustomButtonBase
    {
        private bool _isHovered = false;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(1, 1, Width - 3, Height - 3);

            // Fill background
            Color bgColor = _isHovered ? BackColor : (Parent?.BackColor ?? Color.White);
            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // Draw border
            using (Pen borderPen = new Pen(ForeColor, _isHovered ? 3 : 2))
            {
                g.DrawRectangle(borderPen, rect);
            }

            // Draw text
            Color textColor = _isHovered ? ForeColor : ForeColor;
            TextRenderer.DrawText(g, Text, Font, rect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }
    }

    // 9. Pill Button (Rounded Capsule)
    public class PillButton : CustomButtonBase
    {
        private bool _isPressed = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush clearBrush = new SolidBrush(Parent?.BackColor ?? BackColor))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            int yOffset = _isPressed ? 2 : 0;
            Rectangle rect = new Rectangle(1, 1 + yOffset, Width - 3, Height - 3);
            int radius = Math.Min(rect.Width, rect.Height) / 2;

            // Draw shadow
            if (!_isPressed)
            {
                using (GraphicsPath shadowPath = CreatePillShape(new Rectangle(3, 3, rect.Width, rect.Height), radius))
                {
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                    {
                        g.FillPath(shadowBrush, shadowPath);
                    }
                }
            }

            // Draw gradient background
            using (GraphicsPath path = CreatePillShape(rect, radius))
            {
                Color lightColor = ControlPaint.Light(BackColor);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect, lightColor, BackColor, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }

                // Draw border
                using (Pen pen = new Pen(ControlPaint.Dark(BackColor), 2))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, rect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private GraphicsPath CreatePillShape(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            // Left semicircle
            path.AddArc(rect.Left, rect.Top, diameter, rect.Height, 90, 180);
            // Right semicircle
            path.AddArc(rect.Right - diameter, rect.Top, diameter, rect.Height, 270, 180);

            path.CloseFigure();
            return path;
        }
    }

    // 10. Skeuomorphic Button (Super 3D)
    public class SkeuomorphicButton : CustomButtonBase
    {
        private bool _isPressed = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Parent?.BackColor ?? BackColor);

            Rectangle rect = new Rectangle(3, 3, Width - 7, Height - 7);

            using (GraphicsPath path = CreateRoundedRectangle(rect, 8))
            {
                // Draw outer shadow
                if (!_isPressed)
                {
                    using (GraphicsPath outerShadow = CreateRoundedRectangle(
                        new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height), 8))
                    {
                        using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                        {
                            g.FillPath(shadowBrush, outerShadow);
                        }
                    }
                }

                // Draw gradient background
                Color topColor = _isPressed ? ControlPaint.Dark(BackColor) : ControlPaint.Light(BackColor);
                Color bottomColor = _isPressed ? BackColor : ControlPaint.Dark(BackColor);

                using (LinearGradientBrush gradBrush = new LinearGradientBrush(
                    rect, topColor, bottomColor, LinearGradientMode.Vertical))
                {
                    g.FillPath(gradBrush, path);
                }

                // Draw glossy highlight
                if (!_isPressed)
                {
                    Rectangle highlightRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 3);
                    using (LinearGradientBrush highlightBrush = new LinearGradientBrush(
                        highlightRect,
                        Color.FromArgb(100, 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255),
                        LinearGradientMode.Vertical))
                    {
                        using (GraphicsPath highlightPath = CreateRoundedRectangle(highlightRect, 8))
                        {
                            g.FillPath(highlightBrush, highlightPath);
                        }
                    }
                }

                // Draw border
                Color borderColor = _isPressed ? ControlPaint.DarkDark(BackColor) : ControlPaint.Dark(BackColor);
                using (Pen borderPen = new Pen(borderColor, 2))
                {
                    g.DrawPath(borderPen, path);
                }

                // Inner highlight
                if (!_isPressed)
                {
                    Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                    using (GraphicsPath innerPath = CreateRoundedRectangle(innerRect, 6))
                    {
                        using (Pen innerPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
                        {
                            g.DrawPath(innerPen, innerPath);
                        }
                    }
                }
            }

            // Draw text
            int textOffset = _isPressed ? 1 : 0;
            Rectangle textRect = new Rectangle(rect.X + textOffset, rect.Y + textOffset, rect.Width, rect.Height);
            TextRenderer.DrawText(g, Text, Font, textRect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}


