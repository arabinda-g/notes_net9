using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Notes
{
    public static class ClipboardHelper
    {
        private const uint GMEM_MOVEABLE = 0x0002;
        private const int MAX_FORMAT_NAME = 512;

        private sealed class ClipboardBinaryPacket
        {
            public List<ClipboardBinaryEntry> Entries { get; } = new List<ClipboardBinaryEntry>();
        }

        private sealed class ClipboardBinaryEntry
        {
            public uint FormatId { get; set; }
            public string FormatName { get; set; } = string.Empty;
            public byte[] Data { get; set; } = Array.Empty<byte>();
        }

        public static bool TryCaptureTextFromClipboard(out string text)
        {
            try
            {
                    if (Clipboard.ContainsText())
                    {
                        text = Clipboard.GetText();
                        text = text.Replace("\0", string.Empty);
                        text = text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
                        return true;
                    }
                    if (Clipboard.ContainsText(TextDataFormat.Rtf))
                    {
                        var rtf = Clipboard.GetText(TextDataFormat.Rtf);
                        text = rtf;
                        text = text.Replace("\0", string.Empty);
                        text = text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
                        return true;
                    }
            }
            catch (ExternalException)
            {
                // Clipboard might be locked by another process
            }

            text = string.Empty;
            return false;
        }

        public static bool TryCaptureImageFromClipboard(out string base64Data, out string format, out Bitmap? preview)
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        using (image)
                        {
                            if (image.Width > 8000 || image.Height > 8000)
                                throw new InvalidOperationException("Image too large to capture.");
                            using var ms = new MemoryStream();
                            try
                            {
                                image.Save(ms, ImageFormat.Png);
                                base64Data = Convert.ToBase64String(ms.ToArray());
                                format = "png";
                                preview = new Bitmap(image);
                                return true;
                            }
                            catch
                            {
                                // fall through
                            }
                        }
                    }
                }
            }
            catch (ExternalException)
            {
                // Clipboard might be locked by another process
                format = "error:Clipboard busy";
            }
            catch (InvalidOperationException)
            {
                // Image too large or invalid
                format = "error:Image too large";
            }

            base64Data = string.Empty;
            format = string.Empty;
            preview = null;
            return false;
        }

        public static bool TryCaptureClipboardObject(out string base64, out string summary)
        {
            base64 = string.Empty;
            summary = string.Empty;

            if (!TryOpenClipboard())
            {
                summary = "Clipboard is busy. Try again.";
                return false;
            }

            try
            {
                var packet = CapturePacketFromClipboard();
                if (packet == null || packet.Entries.Count == 0)
                    return false;

                base64 = Convert.ToBase64String(SerializePacket(packet));
                summary = BuildSummary(packet);
                return true;
            }
            finally
            {
                CloseClipboard();
            }
        }

        public static bool TryDescribeObject(string? base64, out string summary)
        {
            summary = string.Empty;
            ClipboardBinaryPacket? packet;
            if (!TryDeserializePacket(base64, out packet) || packet == null || packet.Entries.Count == 0)
                return false;

            summary = BuildSummary(packet);
            return true;
        }

        public static string CopyUnitToClipboard(frmMain.UnitStruct unit)
        {
            var type = unit.ContentType?.ToLowerInvariant() ?? "text";

            try
            {
                switch (type)
                {
                    case "image":
                        if (string.IsNullOrEmpty(unit.ContentData))
                            return "Image content is empty";

                        var bytes = Convert.FromBase64String(unit.ContentData);
                        using (var ms = new MemoryStream(bytes))
                        {
                            using var image = Image.FromStream(ms);
                            using var bitmap = new Bitmap(image);
                            Clipboard.SetImage(bitmap);
                        }
                        return "Image copied to clipboard";

                    case "object":
                        if (TrySetClipboardObject(unit.ContentData, out var message))
                            return message;
                        return message;

                    default:
                        Clipboard.SetText(unit.ContentData ?? string.Empty);
                        return "Copied to clipboard";
                }
            }
            catch (Exception ex)
            {
                return "Clipboard operation failed: " + ex.Message;
            }
        }

        public static bool TrySetClipboardObject(string? base64, out string message)
        {
            try
            {
                ClipboardBinaryPacket? packet;
                if (TryDeserializePacket(base64, out packet) && packet != null && TrySetClipboardPacket(packet))
                {
                    message = "Object copied to clipboard";
                    return true;
                }
            }
            catch (ExternalException)
            {
                message = "Clipboard is busy. Try again.";
                return false;
            }

            message = "Object content unavailable";
            return false;
        }

        public static Bitmap? DecodeImage(string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return null;

            try
            {
                var bytes = Convert.FromBase64String(base64);
                if (bytes.Length > 10 * 1024 * 1024)
                    return null;
                using (var headerStream = new MemoryStream(bytes))
                using (var headerImage = Image.FromStream(headerStream, useEmbeddedColorManagement: false, validateImageData: false))
                {
                    if (headerImage.Width > 8000 || headerImage.Height > 8000)
                        return null;
                }
                using var ms = new MemoryStream(bytes);
                using var image = Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: true);
                if (image.Width > 8000 || image.Height > 8000)
                    return null;
                return new Bitmap(image);
            }
            catch
            {
                return null;
            }
        }

        private static ClipboardBinaryPacket? CapturePacketFromClipboard()
        {
            var packet = new ClipboardBinaryPacket();
            uint format = 0;

            while (true)
            {
                format = EnumClipboardFormats(format);
                if (format == 0)
                    break;

                var entry = CaptureEntry(format);
                if (entry != null)
                    packet.Entries.Add(entry);
            }

            return packet;
        }

        private static ClipboardBinaryEntry? CaptureEntry(uint format)
        {
            var handle = GetClipboardData(format);
            if (handle == IntPtr.Zero)
                return null;

            var sizePtr = GlobalSize(handle);
            if (sizePtr == IntPtr.Zero)
                return null;

            long size = sizePtr.ToInt64();
            if (size <= 0 || size > int.MaxValue)
                return null;

            var locked = GlobalLock(handle);
            if (locked == IntPtr.Zero)
                return null;

            try
            {
                int length = (int)size;
                var data = new byte[length];
                Marshal.Copy(locked, data, 0, length);

                return new ClipboardBinaryEntry
                {
                    FormatId = format,
                    FormatName = ResolveFormatName(format),
                    Data = data
                };
            }
            finally
            {
                GlobalUnlock(handle);
            }
        }

        private static string ResolveFormatName(uint format)
        {
            try
            {
                var df = DataFormats.GetFormat((int)format);
                if (!string.IsNullOrEmpty(df.Name))
                    return df.Name;
            }
            catch
            {
                // Ignore and fall back
            }

            if (format >= 0xC000)
            {
                var sb = new StringBuilder(MAX_FORMAT_NAME);
                var len = GetClipboardFormatName(format, sb, sb.Capacity);
                if (len > 0)
                    return sb.ToString();
            }

            return "Format_" + format;
        }

        private static string BuildSummary(ClipboardBinaryPacket packet)
        {
            if (packet.Entries.Count == 0)
                return string.Empty;

            return "Formats:" + Environment.NewLine + string.Join(Environment.NewLine,
                packet.Entries.Select(e => $"- {e.FormatName} ({e.Data.Length} bytes)"));
        }

        private static byte[] SerializePacket(ClipboardBinaryPacket packet)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            writer.Write(packet.Entries.Count);
            foreach (var entry in packet.Entries)
            {
                writer.Write(entry.FormatId);
                writer.Write(entry.FormatName ?? string.Empty);
                writer.Write(entry.Data.Length);
                writer.Write(entry.Data);
            }

            writer.Flush();
            return ms.ToArray();
        }

        private static bool TryDeserializePacket(string? base64, out ClipboardBinaryPacket? packet)
        {
            packet = null;
            if (string.IsNullOrWhiteSpace(base64))
                return false;

            try
            {
                var bytes = Convert.FromBase64String(base64);
                using var ms = new MemoryStream(bytes);
                using var reader = new BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

                var result = new ClipboardBinaryPacket();
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var entry = new ClipboardBinaryEntry
                    {
                        FormatId = reader.ReadUInt32(),
                        FormatName = reader.ReadString(),
                    };

                    int length = reader.ReadInt32();
                    if (length < 0 || length > bytes.Length)
                        return false;

                    entry.Data = reader.ReadBytes(length);
                    result.Entries.Add(entry);
                }

                packet = result;
                return true;
            }
            catch
            {
                packet = null;
                return false;
            }
        }

        private static bool TrySetClipboardPacket(ClipboardBinaryPacket packet)
        {
            if (!TryOpenClipboard())
                return false;

            try
            {
                if (!EmptyClipboard())
                    return false;

                foreach (var entry in packet.Entries)
                {
                    var formatId = ResolveFormatId(entry);
                    if (formatId == 0)
                        continue;

                    var data = entry.Data ?? Array.Empty<byte>();
                    var length = Math.Max(1, data.Length);
                    var hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)length);
                    if (hMem == IntPtr.Zero)
                    {
                        return false;
                    }

                    var target = GlobalLock(hMem);
                    if (target == IntPtr.Zero)
                    {
                        GlobalFree(hMem);
                        return false;
                    }

                    try
                    {
                        if (data.Length > 0)
                        {
                            Marshal.Copy(data, 0, target, data.Length);
                        }
                        else
                        {
                            Marshal.WriteByte(target, 0, 0);
                        }
                    }
                    finally
                    {
                        GlobalUnlock(hMem);
                    }

                    if (SetClipboardData(formatId, hMem) == IntPtr.Zero)
                    {
                        GlobalUnlock(hMem);
                        GlobalFree(hMem);
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                CloseClipboard();
            }
        }

        private static uint ResolveFormatId(ClipboardBinaryEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.FormatName))
            {
                try
                {
                    var dfByName = DataFormats.GetFormat(entry.FormatName);
                    if (dfByName != null)
                        return (uint)dfByName.Id;
                }
                catch
                {
                    // Ignore and fall back to stored id
                }
            }

            return entry.FormatId;
        }

        private static bool TryOpenClipboard()
        {
            return OpenClipboard(IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint EnumClipboardFormats(uint format);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetClipboardFormatName(uint format, StringBuilder lpszFormatName, int cchMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalSize(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalFree(IntPtr hMem);
    }
}

