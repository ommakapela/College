using ibhayiPharmacy.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace ibhayiPharmacy.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "First name is required")]
            [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
            [Display(Name = "First Name")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Surname is required")]
            [StringLength(100, ErrorMessage = "Surname cannot exceed 100 characters")]
            [Display(Name = "Surname")]
            public string Surname { get; set; }

            [Required(ErrorMessage = "ID Number is required")]
            [StringLength(13, MinimumLength = 13, ErrorMessage = "ID Number must be exactly 13 digits")]
            [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be exactly 13 digits")]
            [Display(Name = "ID Number")]
            public string IdNumber { get; set; }

            [Required(ErrorMessage = "Cellphone number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            [RegularExpression(@"^0\d{9}$", ErrorMessage = "Cellphone must be 10 digits starting with 0")]
            [Display(Name = "Cellphone Number")]
            public string Cellphone { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [StringLength(1000, ErrorMessage = "Allergies list cannot exceed 1000 characters")]
            [Display(Name = "Allergies (comma-separated)")]
            public string? Allergies { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "You must agree to the terms and conditions")]
            [Display(Name = "I agree to the Terms and Conditions")]
            public bool AgreeToTerms { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Manual validation for Terms and Conditions checkbox
            if (!Input.AgreeToTerms)
            {
                ModelState.AddModelError("Input.AgreeToTerms", "You must agree to the terms and conditions to register.");
            }

            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                    _logger.LogWarning($"Registration attempt with existing email: {Input.Email}");
                    return Page();
                }

                var user = CreateUser();

                // Set custom properties
                user.Name = Input.Name;
                user.Surname = Input.Surname;
                user.IdNumber = Input.IdNumber;
                user.Cellphone = Input.Cellphone;
                user.PhoneNumber = Input.Cellphone; // Also set PhoneNumber from Identity
                user.Allergies = string.IsNullOrWhiteSpace(Input.Allergies) ? null : Input.Allergies;
                user.MustChangePassword = false; // Customers don't need to change password
                user.EmailConfirmed = true; // Auto-confirm email for easier testing/development

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User created a new account with password. Email: {Input.Email}");

                    // Assign Customer role
                    var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"Customer role assigned to user: {Input.Email}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to assign Customer role to user: {Input.Email}");
                        foreach (var error in roleResult.Errors)
                        {
                            _logger.LogError($"Role assignment error: {error.Description}");
                        }
                    }

                    var userId = await _userManager.GetUserIdAsync(user);

                    // For development/testing: Auto sign in without email confirmation
                    // Comment out these lines if you want to require email confirmation
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation($"User automatically signed in: {Input.Email}");
                    return LocalRedirect(returnUrl);

                    /* UNCOMMENT THIS SECTION IF YOU WANT EMAIL CONFIRMATION
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                    */
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogError($"Registration error: {error.Description}");
                }
            }
            else
            {
                // Log validation errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"Model validation error: {error.ErrorMessage}");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}