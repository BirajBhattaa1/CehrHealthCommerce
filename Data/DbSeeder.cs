using NepalMediHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NepalMediHub.Data;

/// <summary>
/// Applies migrations and seeds baseline data: roles, a demo admin and a demo customer.
/// Catalog (categories + products) seeding is added in <see cref="SeedCatalogAsync"/>.
/// All accounts and data are fictional/sample.
/// </summary>
public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Customer";

    public const string AdminEmail = "admin@nepalmedihub.local";
    public const string AdminPassword = "Admin@123";

    public const string CustomerEmail = "customer@nepalmedihub.local";
    public const string CustomerPassword = "Customer@123";

    // Login addresses earlier versions of this seeder used, so an existing development
    // database can be brought up to date without colliding on the unique NID.
    private static readonly HashSet<string> LegacySeededEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin@cehrhealth.local",
        "customer@cehrhealth.local"
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply any pending migrations (DB is created on first run).
        await db.Database.MigrateAsync();

        await EnsureRolesAsync(roleManager);
        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, "System Administrator", "1000000001", AdminRole);
        await EnsureUserAsync(userManager, CustomerEmail, CustomerPassword, "Demo Customer", "2000000002", CustomerRole);

        await SeedCatalogAsync(db);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { AdminRole, CustomerRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string fullName, string nid, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        // NID is the unique identity, the email is only a login handle. If the NID is already
        // taken, a previous version of the seeder created this account under an older address,
        // so rename it in place instead of inserting a second row that would violate the unique
        // index on NID. A NID held by an account we did not seed is left alone.
        var existing = await userManager.Users.FirstOrDefaultAsync(u => u.NID == nid);
        if (existing is not null)
        {
            if (LegacySeededEmails.Contains(existing.Email ?? string.Empty))
            {
                existing.Email = email;
                existing.UserName = email;
                await userManager.UpdateAsync(existing);
            }

            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            NID = nid,
            PhoneNumber = "9800000000"
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    /// <summary>
    /// Seeds the category tree and a Nepal-focused sample catalogue. All data is
    /// fictional; prices are in NPR. Runs only once (skipped if data already exists).
    /// </summary>
    private static async Task SeedCatalogAsync(ApplicationDbContext db)
    {
        if (await db.Categories.AnyAsync() || await db.Products.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // --- Top-level categories -----------------------------------------
        var medicines = new Category { Name = "Medicines", Slug = "medicines", Description = "Prescription and over-the-counter medicines." };
        var equipment = new Category { Name = "Medical Equipment", Slug = "medical-equipment", Description = "Devices for diagnosis, monitoring and home care." };
        var health = new Category { Name = "Health Products", Slug = "health-products", Description = "Supplements, personal care and everyday wellness." };
        db.Categories.AddRange(medicines, equipment, health);
        await db.SaveChangesAsync();

        // --- Sub-categories -----------------------------------------------
        Category Sub(string name, string slug, int parentId, string desc)
            => new() { Name = name, Slug = slug, ParentCategoryId = parentId, Description = desc };

        var painRelief = Sub("Pain Relief", "pain-relief", medicines.CategoryId, "Analgesics and anti-inflammatory medicines.");
        var coldFlu = Sub("Cold & Flu", "cold-flu", medicines.CategoryId, "Relief for cough, cold and seasonal flu.");
        var antibiotics = Sub("Antibiotics", "antibiotics", medicines.CategoryId, "Prescription antibiotics.");
        var digestive = Sub("Digestive Health", "digestive-health", medicines.CategoryId, "Acidity, indigestion and rehydration.");

        var diagnostic = Sub("Diagnostic Devices", "diagnostic-devices", equipment.CategoryId, "Monitors and measurement devices.");
        var mobility = Sub("Mobility & Support", "mobility-support", equipment.CategoryId, "Supports, braces and mobility aids.");
        var firstAid = Sub("First Aid", "first-aid", equipment.CategoryId, "First-aid kits and wound care.");

        var vitamins = Sub("Vitamins & Supplements", "vitamins-supplements", health.CategoryId, "Daily vitamins and dietary supplements.");
        var personalCare = Sub("Personal Care", "personal-care", health.CategoryId, "Hygiene and personal-care essentials.");
        var motherBaby = Sub("Mother & Baby", "mother-baby", health.CategoryId, "Products for mothers and babies.");

        db.Categories.AddRange(painRelief, coldFlu, antibiotics, digestive,
            diagnostic, mobility, firstAid, vitamins, personalCare, motherBaby);
        await db.SaveChangesAsync();

        // --- Products ------------------------------------------------------
        var seq = 0;
        Product P(string name, string slug, decimal price, int stock, ProductType type,
            int categoryId, string manufacturer, string description,
            bool prescription = false, int? expiryMonths = null)
        {
            seq++;
            var code = type switch
            {
                ProductType.Medicine => "MED",
                ProductType.MedicalEquipment => "EQP",
                _ => "HLT"
            };
            return new Product
            {
                Name = name,
                Slug = slug,
                SKU = $"CEHR-{code}-{seq:D3}",
                Price = price,
                StockQuantity = stock,
                ProductType = type,
                CategoryId = categoryId,
                Manufacturer = manufacturer,
                Description = description,
                PrescriptionRequired = prescription,
                ExpiryDate = expiryMonths.HasValue ? now.AddMonths(expiryMonths.Value) : null,
                CreatedAt = now.AddMinutes(seq),
                UpdatedAt = now.AddMinutes(seq)
            };
        }

        var products = new List<Product>
        {
            // Pain Relief (Medicine)
            P("Paracetamol 500mg Tablets", "paracetamol-500mg-tablets", 25m, 500, ProductType.Medicine, painRelief.CategoryId, "Annapurna Pharma", "Fast-acting fever and pain relief. Strip of 10 tablets.", expiryMonths: 24),
            P("Ibuprofen 400mg Tablets", "ibuprofen-400mg-tablets", 45m, 300, ProductType.Medicine, painRelief.CategoryId, "Himalaya Remedies Nepal", "Anti-inflammatory pain relief for headaches and body aches.", expiryMonths: 24),
            P("Diclofenac Pain Relief Gel 30g", "diclofenac-pain-relief-gel-30g", 180m, 150, ProductType.Medicine, painRelief.CategoryId, "Sagarmatha Medical", "Topical gel for muscle and joint pain.", expiryMonths: 18),
            P("Aspirin 75mg Tablets", "aspirin-75mg-tablets", 30m, 400, ProductType.Medicine, painRelief.CategoryId, "Annapurna Pharma", "Low-dose aspirin. Strip of 14 tablets.", expiryMonths: 24),

            // Cold & Flu (Medicine)
            P("Cetirizine 10mg Tablets", "cetirizine-10mg-tablets", 35m, 350, ProductType.Medicine, coldFlu.CategoryId, "Lumbini Lifesciences", "Antihistamine for allergies, runny nose and sneezing.", expiryMonths: 24),
            P("Herbal Cough Syrup 100ml", "herbal-cough-syrup-100ml", 120m, 200, ProductType.Medicine, coldFlu.CategoryId, "Gorkha Wellness", "Soothing syrup for dry and productive cough.", expiryMonths: 18),
            P("Cold Relief Tablets", "cold-relief-tablets", 65m, 250, ProductType.Medicine, coldFlu.CategoryId, "Himalaya Remedies Nepal", "Paracetamol with a decongestant for cold and flu symptoms.", expiryMonths: 24),

            // Antibiotics (Medicine, prescription)
            P("Amoxicillin 500mg Capsules", "amoxicillin-500mg-capsules", 150m, 180, ProductType.Medicine, antibiotics.CategoryId, "Kathmandu Care Labs", "Broad-spectrum antibiotic. Prescription required.", prescription: true, expiryMonths: 18),
            P("Azithromycin 500mg Tablets", "azithromycin-500mg-tablets", 220m, 120, ProductType.Medicine, antibiotics.CategoryId, "Sagarmatha Medical", "Antibiotic for respiratory and skin infections. Prescription required.", prescription: true, expiryMonths: 18),
            P("Ciprofloxacin 500mg Tablets", "ciprofloxacin-500mg-tablets", 95m, 160, ProductType.Medicine, antibiotics.CategoryId, "Annapurna Pharma", "Antibiotic for bacterial infections. Prescription required.", prescription: true, expiryMonths: 18),

            // Digestive Health (Medicine)
            P("Omeprazole 20mg Capsules", "omeprazole-20mg-capsules", 85m, 300, ProductType.Medicine, digestive.CategoryId, "Lumbini Lifesciences", "Reduces stomach acid for heartburn and acidity.", expiryMonths: 24),
            P("Oral Rehydration Salts (ORS)", "oral-rehydration-salts-ors", 15m, 600, ProductType.Medicine, digestive.CategoryId, "Gorkha Wellness", "Electrolyte replacement for dehydration. WHO-formula sachet.", expiryMonths: 30),
            P("Antacid Suspension 170ml", "antacid-suspension-170ml", 110m, 220, ProductType.Medicine, digestive.CategoryId, "Himalaya Remedies Nepal", "Fast relief from acidity and indigestion.", expiryMonths: 18),

            // Diagnostic Devices (Equipment)
            P("Digital Blood Pressure Monitor", "digital-blood-pressure-monitor", 2850m, 60, ProductType.MedicalEquipment, diagnostic.CategoryId, "MediTech Nepal", "Automatic upper-arm BP monitor with large display and memory."),
            P("Digital Thermometer", "digital-thermometer", 350m, 200, ProductType.MedicalEquipment, diagnostic.CategoryId, "MediTech Nepal", "Fast, accurate oral/underarm digital thermometer."),
            P("Fingertip Pulse Oximeter", "fingertip-pulse-oximeter", 1450m, 90, ProductType.MedicalEquipment, diagnostic.CategoryId, "CareSense", "Measures blood oxygen (SpO2) and pulse rate."),
            P("Blood Glucose Monitor Kit", "blood-glucose-monitor-kit", 1950m, 70, ProductType.MedicalEquipment, diagnostic.CategoryId, "CareSense", "Glucometer with lancets and 10 test strips."),
            P("Compressor Nebulizer", "compressor-nebulizer", 3200m, 40, ProductType.MedicalEquipment, diagnostic.CategoryId, "MediTech Nepal", "Nebulizer machine for respiratory therapy at home."),
            P("Infrared Forehead Thermometer", "infrared-forehead-thermometer", 1650m, 80, ProductType.MedicalEquipment, diagnostic.CategoryId, "CareSense", "Non-contact infrared thermometer with instant reading."),

            // Mobility & Support (Equipment)
            P("Folding Walking Stick", "folding-walking-stick", 1250m, 50, ProductType.MedicalEquipment, mobility.CategoryId, "SupportWell", "Adjustable aluminium folding cane with ergonomic handle."),
            P("Elastic Knee Support (Medium)", "elastic-knee-support-medium", 550m, 120, ProductType.MedicalEquipment, mobility.CategoryId, "SupportWell", "Breathable compression knee support."),
            P("Lumbar Support Belt", "lumbar-support-belt", 850m, 75, ProductType.MedicalEquipment, mobility.CategoryId, "SupportWell", "Lower-back support belt for posture and pain relief."),

            // First Aid (Equipment)
            P("Home First Aid Kit", "home-first-aid-kit", 950m, 100, ProductType.MedicalEquipment, firstAid.CategoryId, "SafeGuard", "Compact first-aid kit with essentials for home and travel."),
            P("Adhesive Bandages (Box of 100)", "adhesive-bandages-box-100", 120m, 400, ProductType.MedicalEquipment, firstAid.CategoryId, "SafeGuard", "Assorted waterproof adhesive bandages."),
            P("Antiseptic Liquid 100ml", "antiseptic-liquid-100ml", 95m, 300, ProductType.MedicalEquipment, firstAid.CategoryId, "SafeGuard", "Antiseptic solution for cuts, wounds and disinfection.", expiryMonths: 36),

            // Vitamins & Supplements (Health)
            P("Vitamin C 1000mg Tablets", "vitamin-c-1000mg-tablets", 480m, 250, ProductType.HealthProduct, vitamins.CategoryId, "VitaLife", "Immune-support vitamin C. Bottle of 30 tablets.", expiryMonths: 24),
            P("Vitamin D3 Softgels", "vitamin-d3-softgels", 620m, 180, ProductType.HealthProduct, vitamins.CategoryId, "VitaLife", "Supports bone and immune health. 60 softgels.", expiryMonths: 24),
            P("Calcium + Vitamin D3 Tablets", "calcium-vitamin-d3-tablets", 390m, 200, ProductType.HealthProduct, vitamins.CategoryId, "VitaLife", "Daily calcium supplement for bone strength.", expiryMonths: 24),
            P("Daily Multivitamin", "daily-multivitamin", 750m, 150, ProductType.HealthProduct, vitamins.CategoryId, "VitaLife", "Complete multivitamin and mineral formula. 30 tablets.", expiryMonths: 24),
            P("Zinc 50mg Tablets", "zinc-50mg-tablets", 260m, 220, ProductType.HealthProduct, vitamins.CategoryId, "VitaLife", "Zinc supplement for immunity and skin health.", expiryMonths: 24),

            // Personal Care (Health)
            P("Hand Sanitizer 500ml", "hand-sanitizer-500ml", 250m, 400, ProductType.HealthProduct, personalCare.CategoryId, "PureCare", "70% alcohol hand-sanitizer gel with pump."),
            P("N95 Face Mask (Pack of 5)", "n95-face-mask-pack-5", 320m, 500, ProductType.HealthProduct, personalCare.CategoryId, "PureCare", "Reusable N95 respirator masks with adjustable straps."),
            P("Antibacterial Hand Wash 250ml", "antibacterial-hand-wash-250ml", 180m, 350, ProductType.HealthProduct, personalCare.CategoryId, "PureCare", "Gentle antibacterial liquid hand wash."),

            // Mother & Baby (Health)
            P("Baby Diapers Medium (Pack of 40)", "baby-diapers-medium-pack-40", 890m, 200, ProductType.HealthProduct, motherBaby.CategoryId, "TinySteps", "Soft, absorbent diapers for babies 6-11 kg."),
            P("Baby Wipes (72 Sheets)", "baby-wipes-72-sheets", 210m, 300, ProductType.HealthProduct, motherBaby.CategoryId, "TinySteps", "Fragrance-free gentle baby wipes."),
            P("Prenatal Multivitamin", "prenatal-multivitamin", 680m, 130, ProductType.HealthProduct, motherBaby.CategoryId, "VitaLife", "Folic acid and iron supplement for expecting mothers.", expiryMonths: 24),
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}
