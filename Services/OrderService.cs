using ECommerceWebApp.Data;
using ECommerceWebApp.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using Document = iText.Layout.Document;
using Table = iText.Layout.Element.Table;
using LineSeparator = iText.Layout.Element.LineSeparator;

namespace ECommerceWebApp.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetOrderByIdAsync(string id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<byte[]> GenerateOrderPdfAsync(string orderId)
        {
            try
            {
                var order = await GetOrderByIdAsync(orderId);
                if (order == null)
                    throw new Exception("Order not found.");

                using MemoryStream ms = new MemoryStream();
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // ---------------- Header ----------------
                Table headerTable = new Table(2).UseAllAvailableWidth();
                headerTable.AddCell(new Cell()
                    .Add(new Paragraph("My ECommerce App")
                        .SetFont(boldFont).SetFontSize(26))
                    .Add(new Paragraph("1234 Business Rd, Suite 100\nCityville, ST 12345")
                        .SetFont(regularFont).SetFontSize(12))
                    .SetBorder(Border.NO_BORDER));
                headerTable.AddCell(new Cell()
                    .Add(new Paragraph($"Invoice #{order.OrderId}\nDate: {order.OrderDate:yyyy-MM-dd}")
                        .SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER));
                document.Add(headerTable);
                document.Add(new LineSeparator(new SolidLine()).SetMarginTop(10).SetMarginBottom(10));

                // ---------------- Customer & Payment ----------------
                var user = order.User!;
                Table detailsTable = new Table(2).UseAllAvailableWidth();
                detailsTable.AddCell(new Cell()
                    .Add(new Paragraph("Bill To:").SetFont(boldFont).SetFontSize(14))
                    .Add(new Paragraph($"{user.FirstName} {user.LastName}").SetFont(regularFont))
                    .Add(new Paragraph($"Email: {user.Email}").SetFont(regularFont))
                    .SetBorder(Border.NO_BORDER));
                detailsTable.AddCell(new Cell()
                    .Add(new Paragraph("Payment Details:").SetFont(boldFont).SetFontSize(14))
                    .Add(new Paragraph("Payment not recorded.").SetFont(regularFont))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(Border.NO_BORDER));
                document.Add(detailsTable);

                // ---------------- Order Items ----------------
                Table itemsTable = new Table(6).UseAllAvailableWidth();
                string[] headers = { "Description", "Qty", "Unit Price", "Tax %", "Tax Amt", "Total" };
                foreach (var h in headers)
                {
                    itemsTable.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFont(boldFont).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(ColorConstants.DARK_GRAY)
                        .SetTextAlignment(TextAlignment.RIGHT));
                }

                decimal subtotal = 0;
                decimal totalTax = 0;
                decimal grandTotal = 0;

                foreach (var item in order.OrderItems)
                {
                    decimal taxPercent = 0.1m; // example: 10%
                    decimal unitPrice = item.UnitPrice;
                    int qty = item.Quantity;
                    decimal totalPrice = unitPrice * qty;
                    decimal taxAmount = totalPrice * taxPercent;
                    decimal totalWithTax = totalPrice + taxAmount;

                    subtotal += totalPrice;
                    totalTax += taxAmount;
                    grandTotal += totalWithTax;

                    itemsTable.AddCell(new Cell().Add(new Paragraph(item.Product.Name + " - " + item.Product.Description)).SetFont(regularFont));
                    itemsTable.AddCell(new Cell().Add(new Paragraph(qty.ToString())).SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));
                    itemsTable.AddCell(new Cell().Add(new Paragraph(unitPrice.ToString("C"))).SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));
                    itemsTable.AddCell(new Cell().Add(new Paragraph((taxPercent * 100).ToString("F2") + "%")).SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));
                    itemsTable.AddCell(new Cell().Add(new Paragraph(taxAmount.ToString("C"))).SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));
                    itemsTable.AddCell(new Cell().Add(new Paragraph(totalWithTax.ToString("C"))).SetFont(regularFont).SetTextAlignment(TextAlignment.RIGHT));
                }

                document.Add(itemsTable.SetMarginTop(20));

                // ---------------- Totals ----------------
                Table totalsTable = new Table(2).UseAllAvailableWidth().SetHorizontalAlignment(HorizontalAlignment.RIGHT);

                void AddTotalRow(string label, decimal value)
                {
                    totalsTable.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
                    totalsTable.AddCell(new Cell().Add(new Paragraph(value.ToString("C")).SetFont(regularFont)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
                }

                AddTotalRow("Subtotal:", subtotal);
                AddTotalRow("Total Tax:", totalTax);
                AddTotalRow("Grand Total:", grandTotal);

                document.Add(totalsTable.SetMarginTop(20));

                // ---------------- Footer ----------------
                document.Add(new Paragraph("\n"));
                document.Add(new LineSeparator(new SolidLine()));
                document.Add(new Paragraph("Thank you for your business!")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(regularFont)
                    .SetFontSize(12)
                    .SetMarginTop(10));

                document.Close();
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                // Log or inspect the exact error message
                throw new Exception($"PDF generation failed: {ex.Message}", ex);
            }
        }
    }
}
