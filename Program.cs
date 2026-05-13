using ibhayiPharmacy.Areas.Identity.Data;
using ibhayiPharmacy.Data;
using ibhayiPharmacy.Models;
using ibhayiPharmacy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();


var app = builder.Build();

// Seed Roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Customer", "Pharmacist", "PharmacyManager" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    var adminEmail = "Zipho@mandela.ac.za";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Name = "Zipho",
            Surname = "Mdliva",
            IdNumber = "9812105274087",
            Cellphone = "0674592885",
            Allergies = "",
            HealthCouncilRegNumber = ""
        };

        var result = await userManager.CreateAsync(admin, "Admin@123"); // Password should meet policy
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "PharmacyManager");
        }
    } 
    var pharmacistEmail = "Amanda@mandela.ac.za";
    var pharmacistUser = await userManager.FindByEmailAsync(pharmacistEmail);

    if (pharmacistUser == null)
    {
        var pharmacist = new ApplicationUser
        {
            UserName = pharmacistEmail,
            Email = pharmacistEmail,
            EmailConfirmed = true,
            Name = "Amanda",
            Surname = "Silo",
            IdNumber = "9910405274081",
            Cellphone = "0655844128",
            Allergies = "",
            HealthCouncilRegNumber = ""
        };

        var result = await userManager.CreateAsync(pharmacist, "Pharmacist@456"); // Password should meet policy
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(pharmacist, "Pharmacist");
        }
    }

    // ----------- BULK PASSWORD RESET FOR TEST USERS -----------
    var excludeEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "Zipho@mandela.ac.za",
    "s223125768@mandela.ac.za", // Ovayo
    "Amanda@mandela.ac.za"
};

    var allUsers = userManager.Users.ToList();
    foreach (var user in allUsers)
    {
        if (excludeEmails.Contains(user.Email))
            continue;

        // Use Surname + 123! as the password
        string surname = user.Surname?.Trim();
        if (string.IsNullOrEmpty(surname))
        {
            Console.WriteLine($"Skipping {user.Email} - no surname found.");
            continue;
        }
        string password = surname + "123!";

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, resetToken, password);

        if (resetResult.Succeeded)
        {
            Console.WriteLine($"Password reset for {user.Email} to {password}");
        }
        else
        {
            Console.WriteLine($"Failed to reset password for {user.Email}: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
        }
    }
    // ----------- END BULK PASSWORD RESET -----------
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
