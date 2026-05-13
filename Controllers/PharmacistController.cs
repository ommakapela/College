using ibhayiPharmacy.Areas.Identity.Data;
using ibhayiPharmacy.Data;
using ibhayiPharmacy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ibhayiPharmacy.Controllers
{
    [Authorize(Roles = "Pharmacist")]
    public class PharmacistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PharmacistController> _logger;
        public PharmacistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<PharmacistController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ==================== DASHBOARD ====================
        public async Task<IActionResult> Dashboard()
        {
            // Get statistics
            var pendingCount = await _context.Prescriptions
                .Where(p => p.Status == "Pending Processing")
                .CountAsync();

            var processedCount = await _context.Prescriptions
                .Where(p => p.Status == "Processed")
                .CountAsync();

            var readyForCollectionCount = await _context.Prescriptions
                .Where(p => p.ReadyForCollection && !p.IsCollected)
                .CountAsync();

            ViewBag.PendingCount = pendingCount;
            ViewBag.ProcessedCount = processedCount;
            ViewBag.ReadyForCollectionCount = readyForCollectionCount;

            return View();
        }

        // ==================== PENDING PRESCRIPTIONS (STEP 2 - LIST) ====================\
        // ==================== PENDING PRESCRIPTIONS (STEP 2 - LIST) ====================
        [HttpGet]
        public async Task<IActionResult> PendingPrescriptions()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Items)
                .Where(p => p.Status == "Pending Processing")
                .OrderBy(p => p.UploadDate)
                .ToListAsync();

            return View(prescriptions);
        }
        // Add this GET action to your PharmacistController

        [HttpGet]
        public async Task<IActionResult> ProcessPrescription(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
            {
                TempData["Error"] = "Prescription not found.";
                return RedirectToAction("PendingPrescriptions");
            }

            // Get customer to check for allergies
            var customer = await _userManager.FindByIdAsync(prescription.CustomerId);
            if (customer != null)
            {
                ViewBag.CustomerAllergies = customer.Allergies;
                ViewBag.CustomerName = $"{customer.Name} {customer.Surname}";
                ViewBag.HasAllergies = customer.HasAllergies;
            }

            // Load doctors for dropdown
            ViewBag.Doctors = await _context.Doctors.OrderBy(d => d.Name).ToListAsync();

            // Load medications with active ingredients
            ViewBag.Medications = await _context.Medications
                .Include(m => m.MedicationActiveIngredients)
                    .ThenInclude(mai => mai.ActiveIngredient)
                .OrderBy(m => m.Name)
                .ToListAsync();

            // Get PDF data if exists
            if (prescription.PrescriptionFileData != null && prescription.PrescriptionFileData.Length > 0)
            {
                ViewBag.PdfData = prescription.PrescriptionFileData;
            }

            return View(prescription);
        }

        // POST: Save Processed Prescription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPrescription(
    int id,
    DateTime prescriptionDate,
    int doctorId,
    string[] medicationIds,
    int[] quantities,
    string[] instructions,
    int[] repeats,
    decimal[] unitPrices)
        {
            try
            {
                // Get the prescription
                var prescription = await _context.Prescriptions
                    .Include(p => p.Customer) // Include customer to check allergies
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (prescription == null)
                {
                    TempData["Error"] = "Prescription not found.";
                    return RedirectToAction("PendingPrescriptions");
                }

                // Get customer details
                var customer = await _userManager.FindByIdAsync(prescription.CustomerId);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction("PendingPrescriptions");
                }

                // ALLERGY CHECKING - Check each medication against customer allergies
                var allergyWarnings = new List<string>();

                if (customer.HasAllergies)
                {
                    for (int i = 0; i < medicationIds.Length; i++)
                    {
                        if (int.TryParse(medicationIds[i], out int medId))
                        {
                            // Get medication with active ingredients
                            var medication = await _context.Medications
                                .Include(m => m.MedicationActiveIngredients)
                                    .ThenInclude(mai => mai.ActiveIngredient)
                                .FirstOrDefaultAsync(m => m.Id == medId);

                            if (medication != null && medication.MedicationActiveIngredients != null)
                            {
                                // Get all active ingredients in this medication
                                var medicationIngredients = medication.MedicationActiveIngredients
                                    .Select(mai => mai.ActiveIngredient.Name)
                                    .ToList();

                                // Check if customer is allergic to any of these ingredients
                                var allergicIngredients = customer.AllergyList
                                    .Where(allergy => medicationIngredients.Any(ing =>
                                        ing.Equals(allergy, StringComparison.OrdinalIgnoreCase)))
                                    .ToList();

                                if (allergicIngredients.Any())
                                {
                                    allergyWarnings.Add($"⚠️ ALLERGY ALERT: {customer.Name} {customer.Surname} is allergic to {string.Join(", ", allergicIngredients)} found in {medication.Name}!");
                                }
                            }
                        }
                    }
                }

                // If there are allergy warnings, store them and redirect back with warning
                if (allergyWarnings.Any())
                {
                    TempData["AllergyWarning"] = string.Join("<br/>", allergyWarnings);
                    TempData["WarningCount"] = allergyWarnings.Count;

                    // Log the warning
                    _logger.LogWarning($"ALLERGY WARNING: Prescription {id} contains medications that customer {customer.Email} is allergic to: {string.Join("; ", allergyWarnings)}");

                    // Redirect back to show warnings - pharmacist must acknowledge
                    return RedirectToAction("ProcessPrescriptionWithWarning", new { id });
                }

                // Validate doctor
                var doctor = await _context.Doctors.FindAsync(doctorId);
                if (doctor == null)
                {
                    TempData["Error"] = "Selected doctor not found.";
                    return RedirectToAction("ProcessPrescription", new { id });
                }

                // Update prescription
                prescription.PrescriptionDate = prescriptionDate;
                prescription.DoctorId = doctorId;
                prescription.Status = "Processed";

                // Clear existing items if any
                var existingItems = _context.PrescriptionItems.Where(pi => pi.PrescriptionId == id);
                _context.PrescriptionItems.RemoveRange(existingItems);

                // Add new medication items
                for (int i = 0; i < medicationIds.Length; i++)
                {
                    if (int.TryParse(medicationIds[i], out int medId))
                    {
                        var item = new PrescriptionItem
                        {
                            PrescriptionId = prescription.Id,
                            MedicationId = medId,
                            Quantity = quantities[i],
                            Instructions = instructions[i] ?? "No instructions provided",
                            Repeats = repeats[i]
                        };

                        _context.PrescriptionItems.Add(item);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Prescription RX{prescription.Id:D6} processed successfully!";
                return RedirectToAction("PendingPrescriptions");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing prescription: {ex.Message}");
                TempData["Error"] = "Failed to process prescription. Please try again.";
                return RedirectToAction("ProcessPrescription", new { id });
            }
        }

        // Action to show prescription with allergy warnings and require acknowledgment
        [HttpGet]
        public async Task<IActionResult> ProcessPrescriptionWithWarning(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
            {
                TempData["Error"] = "Prescription not found.";
                return RedirectToAction("PendingPrescriptions");
            }

            // Get customer
            var customer = await _userManager.FindByIdAsync(prescription.CustomerId);
            if (customer != null)
            {
                ViewBag.CustomerAllergies = customer.Allergies;
                ViewBag.CustomerName = $"{customer.Name} {customer.Surname}";
            }

            // Load doctors and medications for form
            ViewBag.Doctors = await _context.Doctors.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Medications = await _context.Medications
                .Include(m => m.MedicationActiveIngredients)
                    .ThenInclude(mai => mai.ActiveIngredient)
                .OrderBy(m => m.Name)
                .ToListAsync();

            // Get PDF data if exists
            if (prescription.PrescriptionFileData != null && prescription.PrescriptionFileData.Length > 0)
            {
                ViewBag.PdfData = prescription.PrescriptionFileData;
            }

            return View("ProcessPrescriptionWithWarning", prescription);
        }

        // Action to acknowledge warnings and proceed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeAllergyAndProcess(
            int id,
            DateTime prescriptionDate,
            int doctorId,
            string[] medicationIds,
            int[] quantities,
            string[] instructions,
            int[] repeats,
            decimal[] unitPrices,
            bool acknowledged)
        {
            if (!acknowledged)
            {
                TempData["Error"] = "You must acknowledge the allergy warning before proceeding.";
                return RedirectToAction("ProcessPrescriptionWithWarning", new { id });
            }

            // Log that pharmacist acknowledged the allergy warning
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning($"Pharmacist {userId} acknowledged allergy warning and proceeded with prescription {id}");

            // Get the prescription
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                TempData["Error"] = "Prescription not found.";
                return RedirectToAction("PendingPrescriptions");
            }

            // Validate doctor
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
            {
                TempData["Error"] = "Selected doctor not found.";
                return RedirectToAction("ProcessPrescriptionWithWarning", new { id });
            }

            // Update prescription
            prescription.PrescriptionDate = prescriptionDate;
            prescription.DoctorId = doctorId;
            prescription.Status = "Processed - Allergy Warning Acknowledged";

            // Clear existing items
            var existingItems = _context.PrescriptionItems.Where(pi => pi.PrescriptionId == id);
            _context.PrescriptionItems.RemoveRange(existingItems);

            // Add medication items
            for (int i = 0; i < medicationIds.Length; i++)
            {
                if (int.TryParse(medicationIds[i], out int medId))
                {
                    var item = new PrescriptionItem
                    {
                        PrescriptionId = prescription.Id,
                        MedicationId = medId,
                        Quantity = quantities[i],
                        Instructions = instructions[i] ?? "No instructions provided",
                        Repeats = repeats[i]
                    };

                    _context.PrescriptionItems.Add(item);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Warning"] = $"Prescription RX{prescription.Id:D6} processed with ALLERGY WARNING acknowledged.";
            return RedirectToAction("PendingPrescriptions");
        }

        // ==================== VIEW PRESCRIPTION PDF ====================
        [HttpGet]
        public async Task<IActionResult> ViewPrescriptionPdf(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null || prescription.PrescriptionFileData == null)
            {
                return NotFound("PDF not found.");
            }

            return File(prescription.PrescriptionFileData, "application/pdf");
        }

        // ==================== DISPENSE PRESCRIPTION ====================
        [HttpGet]
        public async Task<IActionResult> Dispense()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .Where(p => p.Status == "Processed" && !p.IsDispensed)
                .OrderBy(p => p.ProcessedDate)
                .ToListAsync();

            return View(prescriptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispensePrescription(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                var prescription = await _context.Prescriptions
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (prescription == null)
                {
                    TempData["Error"] = "Prescription not found.";
                    return RedirectToAction("Dispense");
                }

                prescription.IsDispensed = true;
                prescription.DispensedById = userId;
                prescription.DispensedDate = DateTime.Now;
                prescription.Status = "Dispensed";
                prescription.ReadyForCollection = true;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✓ Prescription RX{prescription.Id:D6} dispensed successfully!";
                return RedirectToAction("Dispense");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error dispensing prescription: {ex.Message}");
                TempData["Error"] = "Failed to dispense prescription.";
                return RedirectToAction("Dispense");
            }
        }

        // ==================== MARK AS COLLECTED ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCollected(int id)
        {
            try
            {
                var prescription = await _context.Prescriptions.FindAsync(id);

                if (prescription == null)
                {
                    TempData["Error"] = "Prescription not found.";
                    return RedirectToAction("ReadyForCollection");
                }

                prescription.IsCollected = true;
                prescription.CollectionDate = DateTime.Now;
                prescription.Status = "Collected";

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✓ Prescription RX{prescription.Id:D6} marked as collected!";
                return RedirectToAction("ReadyForCollection");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking as collected: {ex.Message}");
                TempData["Error"] = "Failed to mark prescription as collected.";
                return RedirectToAction("ReadyForCollection");
            }
        }

        // ==================== READY FOR COLLECTION ====================
        [HttpGet]
        public async Task<IActionResult> ReadyForCollection()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .Where(p => p.ReadyForCollection && !p.IsCollected)
                .OrderBy(p => p.DispensedDate)
                .ToListAsync();

            return View(prescriptions);
        }

        // ==================== ALL PRESCRIPTIONS ====================
        [HttpGet]
        public async Task<IActionResult> AllPrescriptions()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .OrderByDescending(p => p.UploadDate)
                .ToListAsync();

            return View(prescriptions);
        }

        // ==================== VIEW CUSTOMERS ====================
        [HttpGet]
        public async Task<IActionResult> ViewCustomers()
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            return View(customers.OrderBy(c => c.Name).ThenBy(c => c.Surname).ToList());
        }

        // ==================== VIEW CUSTOMER DETAILS ====================
        [HttpGet]
        public async Task<IActionResult> ViewCustomer(string id)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("ViewCustomers");
            }

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medication)
                .Where(p => p.CustomerId == id)
                .OrderByDescending(p => p.UploadDate)
                .ToListAsync();

            ViewBag.Prescriptions = prescriptions;

            return View(customer);
        }

        // ==================== PLACEHOLDER ACTIONS ====================
        public IActionResult Appointments() => View();
        public IActionResult Stock() => View();
        public IActionResult ViewStock() => View();
        public IActionResult Report() => View();
    }
}