using ibhayiPharmacy.Areas.Identity.Data;
using ibhayiPharmacy.Data;
using ibhayiPharmacy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Security.Claims;

namespace ibhayiPharmacy.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== DASHBOARD ====================
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Get prescription statistics
            var totalPrescriptions = await _context.Prescriptions
                .Where(p => p.CustomerId == userId)
                .CountAsync();

            var pendingPrescriptions = await _context.Prescriptions
                .Where(p => p.CustomerId == userId && p.Status == "Pending Processing")
                .CountAsync();

            var readyForCollection = await _context.Prescriptions
                .Where(p => p.CustomerId == userId && p.ReadyForCollection && !p.IsCollected)
                .CountAsync();

            ViewBag.TotalPrescriptions = totalPrescriptions;
            ViewBag.PendingPrescriptions = pendingPrescriptions;
            ViewBag.ReadyForCollection = readyForCollection;

            return View();
        }

        // ==================== UPLOAD PRESCRIPTION (CUSTOMER - STEP 1) ====================
        [HttpGet]
        public async Task<IActionResult> UploadPrescription()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            ViewBag.CurrentCustomer = new
            {
                FullName = user.Name + " " + user.Surname,
                IdNumber = user.IdNumber,
                Cellphone = user.Cellphone,
                Allergies = user.Allergies
            };

            return View();
        }

        // ==================== UPLOAD PRESCRIPTION PDF (CUSTOMER - STEP 1 POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPrescription(IFormFile PrescriptionFile, string? PatientIDNumber)
        {
            try
            {
                if (PrescriptionFile == null || PrescriptionFile.Length == 0)
                {
                    TempData["Error"] = "Please select a PDF file to upload.";
                    return RedirectToAction("UploadPrescription");
                }

                // Validate file type
                if (PrescriptionFile.ContentType != "application/pdf")
                {
                    TempData["Error"] = "Only PDF files are accepted.";
                    return RedirectToAction("UploadPrescription");
                }

                // Validate file size (5MB max)
                if (PrescriptionFile.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "File size must be less than 5MB.";
                    return RedirectToAction("UploadPrescription");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return Unauthorized();

                // Convert file to byte array
                byte[] fileData;
                using (var ms = new MemoryStream())
                {
                    await PrescriptionFile.CopyToAsync(ms);
                    fileData = ms.ToArray();
                }

                // Create prescription with minimal data (STEP 1)
                var prescription = new Prescription
                {
                    CustomerId = userId,
                    PatientName = $"{user.Name} {user.Surname}",
                    PatientIDNumber = PatientIDNumber,
                    UploadDate = DateTime.Now,
                    UploadedById = userId,
                    PrescriptionFileData = fileData,
                    FileName = PrescriptionFile.FileName,
                    ContentType = PrescriptionFile.ContentType,
                    Status = "Pending Processing",
                    PrescriptionDate = DateTime.Now, // Temporary, will be updated by pharmacist
                    DoctorId = null, // Will be filled by pharmacist
                    Items = new List<PrescriptionItem>() // Empty, will be filled by pharmacist
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✓ Prescription uploaded successfully! Reference: RX{prescription.Id:D6}. A pharmacist will process your prescription soon.";
                return RedirectToAction("MyPrescriptions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading prescription: {ex.Message}");
                TempData["Error"] = "Failed to upload prescription. Please try again.";
                return RedirectToAction("UploadPrescription");
            }
        }

        // ==================== MY PRESCRIPTIONS ====================
        [HttpGet]
        public async Task<IActionResult> MyPrescriptions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .Where(p => p.CustomerId == userId)
                .OrderByDescending(p => p.UploadDate)
                .ToListAsync();

            return View(prescriptions);
        }

        // ==================== VIEW PRESCRIPTION DETAILS ====================
        [HttpGet]
        public async Task<IActionResult> ViewPrescriptionDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var prescription = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                        .ThenInclude(m => m.MedicationActiveIngredients)
                            .ThenInclude(mai => mai.ActiveIngredient)
                .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == userId);

            if (prescription == null)
            {
                TempData["Error"] = "Prescription not found.";
                return RedirectToAction("MyPrescriptions");
            }

            return View(prescription);
        }

        // ==================== DOWNLOAD PRESCRIPTION PDF ====================
        [HttpGet]
        public async Task<IActionResult> DownloadPrescription(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == userId);

            if (prescription == null)
            {
                TempData["Error"] = "Prescription not found or access denied.";
                return RedirectToAction("MyPrescriptions");
            }

            if (prescription.PrescriptionFileData == null || prescription.PrescriptionFileData.Length == 0)
            {
                TempData["Error"] = "Prescription file not available.";
                return RedirectToAction("MyPrescriptions");
            }

            var fileName = prescription.FileName ?? $"Prescription_RX{prescription.Id:D6}.pdf";
            var contentType = prescription.ContentType ?? "application/pdf";

            return File(prescription.PrescriptionFileData, contentType, fileName);
        }

        // ==================== MANAGE REPEATS ====================
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ManageRepeats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .Where(p => p.CustomerId == userId && p.Items.Any(i => i.Repeats > 0))
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            return View(prescriptions);
        }

        // ==================== REQUEST REPEAT (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestRepeat(int prescriptionId, int itemId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                // Get the prescription and verify ownership
                var prescription = await _context.Prescriptions
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Medication)
                    .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.CustomerId == userId);

                if (prescription == null)
                {
                    TempData["Error"] = "Prescription not found or access denied.";
                    return RedirectToAction("ManageRepeats");
                }

                // Get the specific prescription item
                var item = prescription.Items.FirstOrDefault(i => i.Id == itemId);

                if (item == null)
                {
                    TempData["Error"] = "Medication item not found.";
                    return RedirectToAction("ManageRepeats");
                }

                // Check if repeats are available
                if (item.Repeats <= 0)
                {
                    TempData["Error"] = $"No repeats remaining for {item.Medication?.Name}.";
                    return RedirectToAction("ManageRepeats");
                }

                // Check for allergies
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && user.HasAllergies && item.Medication != null)
                {
                    // Get medication's active ingredients
                    var medicationIngredients = await _context.MedicationActiveIngredient
                        .Where(mai => mai.MedicationId == item.MedicationId)
                        .Include(mai => mai.ActiveIngredient)
                        .Select(mai => mai.ActiveIngredient.Name)
                        .ToListAsync();

                    // Check if user is allergic to any ingredient
                    var allergicIngredients = user.AllergyList
                        .Where(allergy => medicationIngredients.Any(ing =>
                            ing.Equals(allergy, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    if (allergicIngredients.Any())
                    {
                        TempData["Error"] = $"⚠️ WARNING: You are allergic to {string.Join(", ", allergicIngredients)} " +
                                          $"which is in {item.Medication.Name}. Please consult with a pharmacist before requesting this repeat.";
                        return RedirectToAction("ManageRepeats");
                    }
                }

                // Reduce repeat count
                item.Repeats--;
                item.RepeatsRemaining = item.Repeats;

                // Save changes
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✓ Repeat requested successfully for {item.Medication?.Name}! " +
                                    $"Repeats remaining: {item.Repeats}. A pharmacist will process your request.";

                // Optional: Create an order or notification for pharmacist
                // You can add code here to notify pharmacists about the repeat request

                return RedirectToAction("ManageRepeats");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error requesting repeat: {ex.Message}");
                TempData["Error"] = "Failed to request repeat. Please try again.";
                return RedirectToAction("ManageRepeats");
            }
        }

        // ==================== VIEW REPEAT HISTORY ====================
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RepeatHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .Where(p => p.CustomerId == userId)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            // Filter to show only items that had repeats (original repeats > 0)
            var prescriptionsWithRepeats = prescriptions
                .Where(p => p.Items.Any(i => i.Repeats >= 0)) // Show all items with repeat tracking
                .ToList();

            return View(prescriptionsWithRepeats);
        }

        // ==================== GENERATE REPORT ====================
        [HttpGet]
        public IActionResult GenerateReport() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(DateTime FromDate, DateTime ToDate, string GroupBy)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                if (FromDate > ToDate)
                {
                    TempData["Error"] = "From Date cannot be after To Date.";
                    return RedirectToAction("GenerateReport");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return Unauthorized();

                var prescriptions = await _context.Prescriptions
                    .Include(p => p.Doctor)
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Medication)
                    .Where(p => p.CustomerId == userId &&
                               p.IsDispensed &&
                               p.DispensedDate >= FromDate &&
                               p.DispensedDate <= ToDate)
                    .OrderByDescending(p => p.DispensedDate)
                    .ToListAsync();

                if (!prescriptions.Any())
                {
                    TempData["Error"] = "No dispensed prescriptions found for the selected date range.";
                    return RedirectToAction("GenerateReport");
                }

                byte[] pdfBytes;
                if (GroupBy == "Doctor")
                {
                    pdfBytes = GenerateReportByDoctor(prescriptions, user, FromDate, ToDate);
                }
                else
                {
                    pdfBytes = GenerateReportByMedication(prescriptions, user, FromDate, ToDate);
                }

                var fileName = $"Prescription_Report_{FromDate:yyyyMMdd}_to_{ToDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating report: {ex.Message}");
                TempData["Error"] = "Failed to generate report. Please try again.";
                return RedirectToAction("GenerateReport");
            }
        }

        // ==================== HELPER - GENERATE REPORT BY DOCTOR ====================
        // Replace the GenerateReportByDoctor and GenerateReportByMedication methods with these enhanced versions:

        // ==================== HELPER - GENERATE REPORT BY DOCTOR ====================
        private byte[] GenerateReportByDoctor(List<Prescription> prescriptions, ApplicationUser user, DateTime fromDate, DateTime toDate)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            var normalFont = new XFont("Arial", 10, XFontStyle.Regular);
            var smallFont = new XFont("Arial", 9, XFontStyle.Regular);

            double yPos = 40;
            double leftMargin = 50;
            double rightMargin = page.Width - 50;

            // Header with light green background
            var headerRect = new XRect(0, yPos, page.Width, 150);
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(200, 255, 200)), headerRect);

            yPos += 30;
            gfx.DrawString("DISPENSED PRESCRIPTIONS BY DOCTOR", titleFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 30), XStringFormats.TopCenter);

            yPos += 35;
            gfx.DrawString($"{user.Name} {user.Surname}", headerFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 20), XStringFormats.TopCenter);

            yPos += 25;
            gfx.DrawString($"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}", normalFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 15), XStringFormats.TopCenter);

            yPos += 20;
            gfx.DrawString($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}", smallFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 15), XStringFormats.TopCenter);

            yPos += 40;

            var groupedByDoctor = prescriptions
                .GroupBy(p => p.Doctor?.FullName ?? "Unknown")
                .OrderBy(g => g.Key);

            int grandTotal = 0;

            foreach (var group in groupedByDoctor)
            {
                // Check if we need a new page
                if (yPos > page.Height - 150)
                {
                    page = document.AddPage();
                    page.Size = PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = 50;
                }

                // Doctor heading
                gfx.DrawString($"DOCTOR: {group.Key}", headerFont, XBrushes.Black,
                    new XRect(leftMargin, yPos, rightMargin - leftMargin, 20), XStringFormats.TopLeft);
                yPos += 30;

                // Table header
                var tableTop = yPos;
                var colDate = leftMargin + 20;
                var colMed = colDate + 100;
                var colQty = colMed + 200;
                var colRepeats = colQty + 80;

                // Draw header row with gray background
                var headerRowRect = new XRect(leftMargin, yPos, rightMargin - leftMargin, 20);
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(220, 220, 220)), headerRowRect);

                gfx.DrawString("Date", normalFont, XBrushes.Black,
                    new XRect(colDate, yPos + 5, 90, 15), XStringFormats.TopLeft);
                gfx.DrawString("Medication", normalFont, XBrushes.Black,
                    new XRect(colMed, yPos + 5, 190, 15), XStringFormats.TopLeft);
                gfx.DrawString("Qty", normalFont, XBrushes.Black,
                    new XRect(colQty, yPos + 5, 70, 15), XStringFormats.TopLeft);
                gfx.DrawString("Repeats", normalFont, XBrushes.Black,
                    new XRect(colRepeats, yPos + 5, 70, 15), XStringFormats.TopLeft);

                yPos += 20;

                int subtotal = 0;

                // Draw table rows
                foreach (var prescription in group.OrderBy(p => p.PrescriptionDate))
                {
                    foreach (var item in prescription.Items)
                    {
                        if (yPos > page.Height - 100)
                        {
                            page = document.AddPage();
                            page.Size = PageSize.A4;
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = 50;
                        }

                        // Alternating row colors
                        var rowRect = new XRect(leftMargin, yPos, rightMargin - leftMargin, 18);
                        gfx.DrawRectangle(XBrushes.White, rowRect);

                        gfx.DrawString(prescription.PrescriptionDate.ToString("dd/MM/yyyy") ?? "N/A",
                            normalFont, XBrushes.Black,
                            new XRect(colDate, yPos + 3, 90, 15), XStringFormats.TopLeft);

                        gfx.DrawString(item.Medication?.Name ?? "Unknown",
                            normalFont, XBrushes.Black,
                            new XRect(colMed, yPos + 3, 190, 15), XStringFormats.TopLeft);

                        gfx.DrawString(item.Quantity.ToString(),
                            normalFont, XBrushes.Black,
                            new XRect(colQty, yPos + 3, 70, 15), XStringFormats.TopLeft);

                        gfx.DrawString(item.Repeats.ToString(),
                            normalFont, XBrushes.Black,
                            new XRect(colRepeats, yPos + 3, 70, 15), XStringFormats.TopLeft);

                        subtotal += item.Quantity;
                        yPos += 18;
                    }
                }

                // Draw table border
                gfx.DrawRectangle(XPens.Black, new XRect(leftMargin, tableTop, rightMargin - leftMargin, yPos - tableTop));

                // Subtotal row
                yPos += 5;
                gfx.DrawString($"Sub-total: {subtotal} units", normalFont, XBrushes.Black,
                    new XRect(rightMargin - 150, yPos, 140, 15), XStringFormats.TopRight);

                grandTotal += subtotal;
                yPos += 30;
            }

            // Grand total with green background
            var grandTotalRect = new XRect(leftMargin, yPos, rightMargin - leftMargin, 25);
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(200, 255, 200)), grandTotalRect);
            gfx.DrawString($"GRAND TOTAL: {grandTotal} units", headerFont, XBrushes.Black,
                new XRect(0, yPos + 5, page.Width, 20), XStringFormats.TopCenter);

            using (var stream = new MemoryStream())
            {
                document.Save(stream, false);
                return stream.ToArray();
            }
        }

        // ==================== HELPER - GENERATE REPORT BY MEDICATION ====================
        private byte[] GenerateReportByMedication(List<Prescription> prescriptions, ApplicationUser user, DateTime fromDate, DateTime toDate)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            var normalFont = new XFont("Arial", 10, XFontStyle.Regular);
            var smallFont = new XFont("Arial", 9, XFontStyle.Regular);

            double yPos = 40;
            double leftMargin = 50;
            double rightMargin = page.Width - 50;

            // Header with light green background
            var headerRect = new XRect(0, yPos, page.Width, 150);
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(200, 255, 200)), headerRect);

            yPos += 30;
            gfx.DrawString("DISPENSED PRESCRIPTIONS BY MEDICATION", titleFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 30), XStringFormats.TopCenter);

            yPos += 35;
            gfx.DrawString($"{user.Name} {user.Surname}", headerFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 20), XStringFormats.TopCenter);

            yPos += 25;
            gfx.DrawString($"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}", normalFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 15), XStringFormats.TopCenter);

            yPos += 20;
            gfx.DrawString($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}", smallFont, XBrushes.Black,
                new XRect(0, yPos, page.Width, 15), XStringFormats.TopCenter);

            yPos += 40;

            var allItems = prescriptions.SelectMany(p => p.Items).ToList();
            var groupedByMedication = allItems
                .GroupBy(i => i.Medication?.Name ?? "Unknown")
                .OrderBy(g => g.Key);

            int grandTotal = 0;

            foreach (var group in groupedByMedication)
            {
                if (yPos > page.Height - 150)
                {
                    page = document.AddPage();
                    page.Size = PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = 50;
                }

                var totalQty = group.Sum(i => i.Quantity);

                // Medication heading
                gfx.DrawString($"MEDICATION: {group.Key} (Total: {totalQty} units)", headerFont, XBrushes.Black,
                    new XRect(leftMargin, yPos, rightMargin - leftMargin, 20), XStringFormats.TopLeft);
                yPos += 30;

                // Table header
                var tableTop = yPos;
                var colDate = leftMargin + 20;
                var colDoctor = colDate + 100;
                var colQty = colDoctor + 200;
                var colRepeats = colQty + 80;

                var headerRowRect = new XRect(leftMargin, yPos, rightMargin - leftMargin, 20);
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(220, 220, 220)), headerRowRect);

                gfx.DrawString("Date", normalFont, XBrushes.Black,
                    new XRect(colDate, yPos + 5, 90, 15), XStringFormats.TopLeft);
                gfx.DrawString("Doctor", normalFont, XBrushes.Black,
                    new XRect(colDoctor, yPos + 5, 190, 15), XStringFormats.TopLeft);
                gfx.DrawString("Qty", normalFont, XBrushes.Black,
                    new XRect(colQty, yPos + 5, 70, 15), XStringFormats.TopLeft);
                gfx.DrawString("Repeats", normalFont, XBrushes.Black,
                    new XRect(colRepeats, yPos + 5, 70, 15), XStringFormats.TopLeft);

                yPos += 20;

                // Get prescriptions for this medication
                var medicationPrescriptions = prescriptions
                    .Where(p => p.Items.Any(i => i.Medication?.Name == group.Key))
                    .OrderBy(p => p.PrescriptionDate);

                foreach (var prescription in medicationPrescriptions)
                {
                    var item = prescription.Items.FirstOrDefault(i => i.Medication?.Name == group.Key);
                    if (item == null) continue;

                    if (yPos > page.Height - 100)
                    {
                        page = document.AddPage();
                        page.Size = PageSize.A4;
                        gfx = XGraphics.FromPdfPage(page);
                        yPos = 50;
                    }

                    var rowRect = new XRect(leftMargin, yPos, rightMargin - leftMargin, 18);
                    gfx.DrawRectangle(XBrushes.White, rowRect);

                    gfx.DrawString(prescription.PrescriptionDate.ToString("dd/MM/yyyy") ?? "N/A",
                        normalFont, XBrushes.Black,
                        new XRect(colDate, yPos + 3, 90, 15), XStringFormats.TopLeft);

                    gfx.DrawString(prescription.Doctor?.FullName ?? "Unknown",
                        normalFont, XBrushes.Black,
                        new XRect(colDoctor, yPos + 3, 190, 15), XStringFormats.TopLeft);

                    gfx.DrawString(item.Quantity.ToString(),
                        normalFont, XBrushes.Black,
                        new XRect(colQty, yPos + 3, 70, 15), XStringFormats.TopLeft);

                    gfx.DrawString(item.Repeats.ToString(),
                        normalFont, XBrushes.Black,
                        new XRect(colRepeats, yPos + 3, 70, 15), XStringFormats.TopLeft);

                    yPos += 18;
                }

                // Draw table border
                gfx.DrawRectangle(XPens.Black, new XRect(leftMargin, tableTop, rightMargin - leftMargin, yPos - tableTop));

                // Subtotal
                yPos += 5;
                gfx.DrawString($"Sub-total: {totalQty} units", normalFont, XBrushes.Black,
                    new XRect(rightMargin - 150, yPos, 140, 15), XStringFormats.TopRight);

                grandTotal += totalQty;
                yPos += 30;
            }

            // Grand total with green background
            var grandTotalRect = new XRect(leftMargin, yPos, rightMargin - leftMargin, 25);
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(200, 255, 200)), grandTotalRect);
            gfx.DrawString($"GRAND TOTAL: {grandTotal} units", headerFont, XBrushes.Black,
                new XRect(0, yPos + 5, page.Width, 20), XStringFormats.TopCenter);

            using (var stream = new MemoryStream())
            {
                document.Save(stream, false);
                return stream.ToArray();
            }
        }

        // ==================== HELPER - DRAW FOOTER ====================
        private void DrawFooter(XGraphics gfx, PdfPage page, int pageNumber)
        {
            var footerFont = new XFont("Arial", 9, XFontStyle.Regular);
            var footerY = page.Height - 30;
            gfx.DrawString($"Page {pageNumber}", footerFont, XBrushes.Gray,
                new XRect(0, footerY, page.Width, 20), XStringFormats.TopCenter);
        }
        public async Task<IActionResult> ViewOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .Where(p => p.CustomerId == userId)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            return View(prescriptions);
        }
    }



}