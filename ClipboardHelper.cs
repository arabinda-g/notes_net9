using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Notes
{
    public static class ClipboardHelper
    {
        public class ClipboardPacket
        {
            public string Version { get; set; } = "1.0";
            public List<ClipboardEntry> Entries { get; set; } = new List<ClipboardEntry>();
        }

        public class ClipboardEntry
        {
            public string Format { get; set; } = string.Empty;
            public string Kind { get; set; } = string.Empty;
            public string Data { get; set; } = string.Empty;
        }

        public static bool TryCaptureTextFromClipboard(out string text)
        {
            if (Clipboard.ContainsText())
            {
                text = Clipboard.GetText();
                return true;
            }

            text = string.Empty;
            return false;
        }

        public static bool TryCaptureImageFromClipboard(out string base64Data, out string format, out Bitmap? preview)
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                if (image != null)
                {
                    using (image)
                    {
                        using var ms = new MemoryStream();
                        image.Save(ms, ImageFormat.Png);
                        base64Data = Convert.ToBase64String(ms.ToArray());
                        format = "png";
                        preview = new Bitmap(image);
                        return true;
                    }
                }
            }

            base64Data = string.Empty;
            format = string.Empty;
            preview = null;
            return false;
        }

        public static bool TryCaptureObjectFromClipboard(out ClipboardPacket? packet, out string summary)
        {
            packet = null;
            summary = string.Empty;

            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null)
            {
                return false;
            }

            var formats = dataObject.GetFormats();
            var result = new ClipboardPacket();

            foreach (var format in formats)
            {
                try
                {
                    var data = dataObject.GetData(format);
                    if (data == null)
                        continue;

                    switch (data)
                    {
                        case string s:
                            result.Entries.Add(new ClipboardEntry
                            {
                                Format = format,
                                Kind = "string",
                                Data = s
                            });
                            break;
                        case string[] array:
                            result.Entries.Add(new ClipboardEntry
                            {
                                Format = format,
                                Kind = "string-array",
                                Data = JsonConvert.SerializeObject(array)
                            });
                            break;
                        case MemoryStream ms:
                            result.Entries.Add(new ClipboardEntry
                            {
                                Format = format,
                                Kind = "binary",
                                Data = Convert.ToBase64String(ms.ToArray())
                            });
                            break;
                        case byte[] bytes:
                            result.Entries.Add(new ClipboardEntry
                            {
                                Format = format,
                                Kind = "binary",
                                Data = Convert.ToBase64String(bytes)
                            });
                            break;
                        case Image image:
                            using (image)
                            {
                                using var imageStream = new MemoryStream();
                                image.Save(imageStream, ImageFormat.Png);
                                result.Entries.Add(new ClipboardEntry
                                {
                                    Format = format,
                                    Kind = "image",
                                    Data = Convert.ToBase64String(imageStream.ToArray())
                                });
                            }
                            break;
                        default:
                            if (data.GetType().IsSerializable)
                            {
#pragma warning disable SYSLIB0011
                                using var serialized = new MemoryStream();
                                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                                formatter.Serialize(serialized, data);
#pragma warning restore SYSLIB0011
                                result.Entries.Add(new ClipboardEntry
                                {
                                    Format = format,
                                    Kind = "binary",
                                    Data = Convert.ToBase64String(serialized.ToArray())
                                });
                            }
                            break;
                    }
                }
                catch
                {
                    // Ignore formats we cannot capture
                }
            }

            if (result.Entries.Count == 0)
            {
                return false;
            }

            packet = result;
            summary = BuildSummary(result);
            return true;
        }

        public static string BuildSummary(ClipboardPacket packet)
        {
            if (packet.Entries.Count == 0)
                return string.Empty;

            return "Formats:" + Environment.NewLine + string.Join(Environment.NewLine,
                packet.Entries.Select(e => $"- {e.Format} ({e.Kind})"));
        }

        public static string SerializePacket(ClipboardPacket packet) => JsonConvert.SerializeObject(packet);

        public static bool TryDeserializePacket(string? data, out ClipboardPacket? packet, out string summary)
        {
            packet = null;
            summary = string.Empty;

            if (string.IsNullOrWhiteSpace(data))
                return false;

            try
            {
                var parsed = JsonConvert.DeserializeObject<ClipboardPacket>(data);
                if (parsed == null || parsed.Entries == null || parsed.Entries.Count == 0)
                    return false;

                packet = parsed;
                summary = BuildSummary(parsed);
                return true;
            }
            catch
            {
                return false;
            }
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
                            Clipboard.SetImage(new Bitmap(image));
                        }
                        return "Image copied to clipboard";

                    case "object":
                        if (!TryDeserializePacket(unit.ContentData, out var packet, out _))
                            return "Object content unavailable";

                        var dataObject = new DataObject();
                        foreach (var entry in packet!.Entries)
                        {
                            try
                            {
                                switch (entry.Kind)
                                {
                                    case "string":
                                        dataObject.SetData(entry.Format, entry.Data);
                                        break;
                                    case "string-array":
                                        var array = JsonConvert.DeserializeObject<string[]>(entry.Data) ?? Array.Empty<string>();
                                        dataObject.SetData(entry.Format, array);
                                        break;
                                    case "image":
                                        var imageBytes = Convert.FromBase64String(entry.Data);
                                        using (var ms = new MemoryStream(imageBytes))
                                        {
                                            using var image = Image.FromStream(ms);
                                            dataObject.SetData(entry.Format, new Bitmap(image));
                                        }
                                        break;
                                    case "binary":
                                        var binary = Convert.FromBase64String(entry.Data);
                                        dataObject.SetData(entry.Format, binary);
                                        break;
                                }
                            }
                            catch
                            {
                                // Ignore formats we cannot restore
                            }
                        }

                        Clipboard.SetDataObject(dataObject, true);
                        return "Object copied to clipboard";

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

        public static Bitmap? DecodeImage(string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return null;

            try
            {
                var bytes = Convert.FromBase64String(base64);
                using var ms = new MemoryStream(bytes);
                return new Bitmap(Image.FromStream(ms));
            }
            catch
            {
                return null;
            }
        }
    }
}

