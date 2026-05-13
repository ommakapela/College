//using ibhayiPharmacy.Areas.Identity.Data;
//using ibhayiPharmacy.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace ibhayiPharmacy.Controllers
//{
//    [Authorize(Roles = "PharmacyManager")]
//    public class PharmacyManagerController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly IEmailSender _emailSender;

//        public PharmacyManagerController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
//        {
//            _userManager = userManager;
//            _emailSender = emailSender;
//        }

//        public IActionResult Dashboard()
//        {
//            return View();
//        }

//        // GET: Register Pharmacist View
//        public IActionResult RegisterPharmacist()
//        {
//            return View();
//        }

//        // POST: Register Pharmacist
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> RegisterPharmacist(ApplicationUser model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Generate required components
//                var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
//                var lowercase = "abcdefghijklmnopqrstuvwxyz";
//                var digits = "0123456789";
//                var symbols = "!@#$%^&*";

//                var random = new Random();

//                // Pick at least one character from each required set
//                var upper = uppercase[random.Next(uppercase.Length)];
//                var lower = lowercase[random.Next(lowercase.Length)];
//                var digit = digits[random.Next(digits.Length)];
//                var symbol = symbols[random.Next(symbols.Length)];

//                // Fill the rest of the password with random characters
//                var allChars = uppercase + lowercase + digits + symbols;
//                var remainingChars = new char[4]; // for a total of 8 chars
//                for (int i = 0; i < remainingChars.Length; i++)
//                {
//                    remainingChars[i] = allChars[random.Next(allChars.Length)];
//                }

//                // Combine all characters and shuffle
//                var passwordChars = new List<char> { upper, lower, digit, symbol };
//                passwordChars.AddRange(remainingChars);
//                passwordChars = passwordChars.OrderBy(c => random.Next()).ToList();

//                string generatedPassword = new string(passwordChars.ToArray()); // ✅ Renamed here

//                var pharmacist = new ApplicationUser
//                {
//                    UserName = model.Email,
//                    Email = model.Email,
//                    Name = model.Name,
//                    Surname = model.Surname,
//                    IdNumber = model.IdNumber,
//                    Cellphone = model.Cellphone,
//                    HealthCouncilRegNumber = model.HealthCouncilRegNumber,
//                    MustChangePassword = true
//                };

//                var result = await _userManager.CreateAsync(pharmacist, generatedPassword);
//                if (result.Succeeded)
//                {
//                    await _userManager.AddToRoleAsync(pharmacist, "Pharmacist");

//                    await _emailSender.SendEmailAsync(
//                        model.Email,
//                        "Account Created",
//                        $"<p>Your temporary password is: <strong>{generatedPassword}</strong>.</p><p>Please log in and change it.</p>"
//                    );

//                    TempData["Success"] = $"Pharmacist registered successfully.";
//                    return RedirectToAction("ManagePharmacists", "PharmacyManager");
//                }
//                foreach (var error in result.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//            }
//            return View(model);
//        }
       
//        public async Task<IActionResult> ManagePharmacists()
//        {
//            var usersInRole = await _userManager.GetUsersInRoleAsync("Pharmacist");
//            return View(usersInRole);
//        }

//        // GET: Edit Pharmacist
//        public async Task<IActionResult> EditPharmacist(string id)
//        {
//            if (string.IsNullOrEmpty(id))
//                return NotFound();

//            var pharmacist = await _userManager.FindByIdAsync(id);
//            if (pharmacist == null || !await _userManager.IsInRoleAsync(pharmacist, "Pharmacist"))
//                return NotFound();

//            return View(pharmacist);
//        }

//        // POST: Edit Pharmacist
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> EditPharmacist(ApplicationUser model)
//        {
//            var pharmacist = await _userManager.FindByIdAsync(model.Id);
//            if (pharmacist == null)
//                return NotFound();

//            pharmacist.Name = model.Name;
//            pharmacist.Surname = model.Surname;
//            pharmacist.Cellphone = model.Cellphone;
//            pharmacist.IDNumber = model.IDNumber;
//            pharmacist.HealthCouncilRegNumber = model.HealthCouncilRegNumber;

//            var result = await _userManager.UpdateAsync(pharmacist);
//            if (result.Succeeded)
//            {
//                TempData["Success"] = "Pharmacist updated successfully.";
//                return RedirectToAction("ManagePharmacists");
//            }

//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }

//            return View(model);
//        }

//        // POST: Delete Pharmacist
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeletePharmacist(string id)
//        {
//            var pharmacist = await _userManager.FindByIdAsync(id);
//            if (pharmacist == null || !await _userManager.IsInRoleAsync(pharmacist, "Pharmacist"))
//            {
//                TempData["Error"] = "Pharmacist not found or invalid role.";
//                return RedirectToAction("ManagePharmacists");
//            }

//            var result = await _userManager.DeleteAsync(pharmacist);
//            if (result.Succeeded)
//            {
//                TempData["Success"] = "Pharmacist deleted successfully.";
//            }
//            else
//            {
//                TempData["Error"] = "An error occurred while deleting the pharmacist.";
//            }

//            return RedirectToAction("ManagePharmacists");
//        }
//    }
//}

    

