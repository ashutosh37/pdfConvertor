using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using MsgReader.Outlook;
using PdfSharpCore.Drawing.Layout; // Added for XTextFormatter
using System.Text.RegularExpressions; // Added for basic HTML stripping
using System.Diagnostics;

// Add using statement for the DOCX conversion library you choose
// e.g., using Syncfusion.DocIO.DLS; using Syncfusion.DocIORenderer;

namespace risys.legal365
{
    public class FileConverterService
    {
        public async Task<MemoryStream> ConvertToPdfAsync(Stream inputStream, string ext)
        {
            var pdfStream = new MemoryStream();

            if (ext == ".msg")
            {
                var msg = new Storage.Message(inputStream);
                var doc = new PdfDocument();
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                // Use a font more likely to be available on Linux, like DejaVu Sans
                var font = new XFont("DejaVu Sans", 10);
                var formatter = new XTextFormatter(gfx); // Create a text formatter

                double currentY = 20; // Starting Y position

                // Use XPoint for drawing single lines at specific coordinates
                gfx.DrawString($"Subject: {msg.Subject}", font, XBrushes.Black, new XPoint(10, currentY));
                currentY += 20; // Move down for the next line
                gfx.DrawString($"From: {msg.Sender?.Email}", font, XBrushes.Black, new XPoint(10, currentY));
                currentY += 20; // Move down for the body

                // Attempt to get body text: prioritize BodyText, then try BodyHtml (with basic tag stripping)
                string bodyToDraw = msg.BodyText ?? string.Empty;

                if (string.IsNullOrWhiteSpace(bodyToDraw) && !string.IsNullOrWhiteSpace(msg.BodyHtml))
                {
                    // Basic HTML stripping as a fallback
                    // Replace <br> and <p> tags with newlines, then strip all other tags
                    bodyToDraw = Regex.Replace(msg.BodyHtml, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
                    bodyToDraw = Regex.Replace(bodyToDraw, @"<p\s*/?>", "\n", RegexOptions.IgnoreCase);
                    bodyToDraw = Regex.Replace(bodyToDraw, @"<[^>]+>", string.Empty).Trim();
                    // Decode HTML entities like &nbsp; &amp; etc.
                    bodyToDraw = System.Net.WebUtility.HtmlDecode(bodyToDraw);
                }

                // Define the rectangle for the body text
                var bodyRect = new XRect(10, currentY, page.Width - 20, page.Height - currentY - 10); // Use remaining page space with margins

                // Draw the body text using the formatter for better layout
                formatter.DrawString(text: bodyToDraw, // bodyToDraw is guaranteed non-null here
                                     font: font,
                                     brush: XBrushes.Black,
                                     layoutRectangle: bodyRect);

                doc.Save(pdfStream);
            }
            else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var image = XImage.FromStream(() => inputStream);
                gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                doc.Save(pdfStream);
            }
            else if (ext == ".docx")
            {
                // Convert DOCX to PDF using LibreOffice in headless mode
                var tempDir = Path.GetTempPath();
                var inputPath = Path.Combine(tempDir, Guid.NewGuid() + ".docx");
                var outputPath = Path.ChangeExtension(inputPath, ".pdf");

                // Save the DOCX input stream to a temporary file
                await using (var fs = File.Create(inputPath))
                {
                    await inputStream.CopyToAsync(fs);
                }

                // Convert DOCX to PDF using LibreOffice (headless mode)
                var startInfo = new ProcessStartInfo
                {
                    FileName = "soffice", // Ensure LibreOffice is installed and in PATH
                    Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{inputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using var process = Process.Start(startInfo);
                await process.WaitForExitAsync();

                if (!File.Exists(outputPath))
                    throw new Exception("PDF conversion failed for DOCX.");

                // Read the resulting PDF into the MemoryStream
                var result = new MemoryStream(await File.ReadAllBytesAsync(outputPath));

                // Clean up temporary files
                File.Delete(inputPath);
                File.Delete(outputPath);

                result.Position = 0;
                result.CopyTo(pdfStream);
            }
            else
            {
                throw new NotSupportedException($"File extension {ext} is not supported.");
            }

            pdfStream.Position = 0;
            return pdfStream;
        }

            // Convert .docx to .pdf using LibreOffice
        private async Task<MemoryStream> ConvertDocxToPdfAsync(Stream inputStream)
        {
            var tempDir = Path.GetTempPath();
            var inputPath = Path.Combine(tempDir, Guid.NewGuid() + ".docx");
            var outputPath = Path.ChangeExtension(inputPath, ".pdf");

            // Save the .docx input stream to a temporary file
            await using (var fs = File.Create(inputPath))
            {
                await inputStream.CopyToAsync(fs);
            }

            // Convert the file using LibreOffice (headless mode)
            var startInfo = new ProcessStartInfo
            {
                FileName = "soffice",
                Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{inputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(startInfo);
            await process.WaitForExitAsync();

            if (!File.Exists(outputPath))
                throw new Exception("PDF conversion failed.");

            // Read the resulting PDF into a memory stream
            var result = new MemoryStream(await File.ReadAllBytesAsync(outputPath));

            // Clean up temporary files
            File.Delete(inputPath);
            File.Delete(outputPath);

            result.Position = 0;
            return result;
        }
    }

}
