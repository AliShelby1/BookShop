using BookShop.Models;
using BookShop.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace BookShop.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            // Migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {
                // In case migration fails or database already up-to-date
            }

            // 1. Seed Roles & Default Admin Account
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();

                var adminUser = new ApplicationUser
                {
                    UserName = "admin@bookshop.com",
                    Email = "admin@bookshop.com",
                    Name = "BookShop Admin",
                    PhoneNumber = "1112223333",
                    StreetAddress = "123 Admin Way",
                    State = "IL",
                    PostalCode = "60007",
                    City = "Chicago",
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(adminUser, SD.Role_Admin).GetAwaiter().GetResult();
                }
            }

            // 2. Seed Initial Catalog Data (Categories, Authors, Publishers, Books)
            if (!_db.Categories.Any())
            {
                var sciFi = new Category { Name = "Science Fiction", NameAr = "خيال علمي", DisplayOrder = 1, IconClass = "bi-rocket-takeoff" };
                var tech = new Category { Name = "Technology & Programming", NameAr = "تكنولوجيا وبرمجة", DisplayOrder = 2, IconClass = "bi-code-slash" };
                var business = new Category { Name = "Business & Finance", NameAr = "أعمال واقتصاد", DisplayOrder = 3, IconClass = "bi-briefcase" };
                var fiction = new Category { Name = "Fiction", NameAr = "روايات وقصص", DisplayOrder = 4, IconClass = "bi-journal-bookmark" };
                var selfHelp = new Category { Name = "Self-Help", NameAr = "تطوير الذات", DisplayOrder = 5, IconClass = "bi-lightbulb" };

                _db.Categories.AddRange(sciFi, tech, business, fiction, selfHelp);
                _db.SaveChanges();

                var martin = new Author
                {
                    Name = "Robert C. Martin",
                    Biography = "Known affectionately as 'Uncle Bob', he has been a programmer since 1970, is a co-founder of the Agile Alliance, and author of classic software craftsmanship books."
                };
                var hunt = new Author
                {
                    Name = "Andrew Hunt",
                    Biography = "Author of The Pragmatic Programmer and one of the original 17 authors and signatories of the Agile Manifesto."
                };
                var herbert = new Author
                {
                    Name = "Frank Herbert",
                    Biography = "Critically acclaimed American science fiction author best known for the masterpiece novel Dune and its five sequels."
                };
                var clear = new Author
                {
                    Name = "James Clear",
                    Biography = "Writer and speaker focused on habits, decision making, and continuous improvement. Author of the #1 New York Times bestseller Atomic Habits."
                };

                _db.Authors.AddRange(martin, hunt, herbert, clear);
                _db.SaveChanges();

                var prenticeHall = new Publisher
                {
                    Name = "Prentice Hall",
                    Description = "Major American educational publisher specializing in computer science and engineering textbooks."
                };
                var addisonWesley = new Publisher
                {
                    Name = "Addison-Wesley",
                    Description = "Publisher of leading professional software engineering and computer science books."
                };
                var chilton = new Publisher
                {
                    Name = "Chilton Books",
                    Description = "Classic American publisher renowned for automotive and science fiction publications."
                };
                var avery = new Publisher
                {
                    Name = "Avery / Penguin Random House",
                    Description = "Imprint of Penguin Publishing Group publishing bestselling self-help and non-fiction titles."
                };

                _db.Publishers.AddRange(prenticeHall, addisonWesley, chilton, avery);
                _db.SaveChanges();

                var cleanCode = new Book
                {
                    Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
                    TitleAr = "كود نظيف: دليل الحرفية البرمجية الرشيقة",
                    Language = "English",
                    Description = "Even bad code can function. But if code isn't clean, it can bring a development organization to its knees. Every year, countless hours and significant resources are lost because of poorly written code. This book is a must for any developer, software engineer, project manager, or systems analyst looking to produce cleaner code.",
                    ISBN = "978-0132350884",
                    Price = 42.99m,
                    DiscountPercentage = 10m,
                    StockQuantity = 25,
                    CoverImageUrl = "https://images.unsplash.com/photo-1532012164546-f432f2e3777a?auto=format&fit=crop&q=80&w=800",
                    IsActive = true,
                    IsFeatured = true,
                    IsVolumeOfTheMonth = false,
                    CategoryId = tech.Id,
                    AuthorId = martin.Id,
                    PublisherId = prenticeHall.Id
                };

                var pragmaticProg = new Book
                {
                    Title = "The Pragmatic Programmer: Your Journey To Mastery",
                    TitleAr = "المبرمج البراغماتي: رحلتك إلى الاحتراف",
                    Language = "English",
                    Description = "The Pragmatic Programmer is one of those rare tech books you'll read and re-read over the years. Whether you're new to the field or an experienced practitioner, you'll come away each time with fresh insights into code architecture, refactoring, and career progression.",
                    ISBN = "978-0135957059",
                    Price = 49.95m,
                    DiscountPercentage = 15m,
                    StockQuantity = 18,
                    CoverImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?auto=format&fit=crop&q=80&w=800",
                    IsActive = true,
                    IsFeatured = false,
                    IsVolumeOfTheMonth = true,
                    CategoryId = tech.Id,
                    AuthorId = hunt.Id,
                    PublisherId = addisonWesley.Id
                };

                var dune = new Book
                {
                    Title = "Dune",
                    TitleAr = "كثيب (ديون)",
                    Language = "English",
                    Description = "Set on the desert planet Arrakis, Dune tells the story of Paul Atreides, heir to a noble family tasked with ruling an inhospitable world where the only thing of value is the 'spice' melange, a drug capable of extending life and enhancing consciousness.",
                    ISBN = "978-0441172719",
                    Price = 18.99m,
                    DiscountPercentage = 0m,
                    StockQuantity = 40,
                    CoverImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?auto=format&fit=crop&q=80&w=800",
                    IsActive = true,
                    IsFeatured = true,
                    IsVolumeOfTheMonth = false,
                    CategoryId = sciFi.Id,
                    AuthorId = herbert.Id,
                    PublisherId = chilton.Id
                };

                var atomicHabits = new Book
                {
                    Title = "Atomic Habits: An Easy & Proven Way to Build Good Habits",
                    TitleAr = "العادات الذرية",
                    Language = "English",
                    Description = "No matter your goals, Atomic Habits offers a proven framework for improving every day. James Clear reveals practical strategies that will teach you exactly how to form good habits, break bad ones, and master the tiny behaviors that lead to remarkable results.",
                    ISBN = "978-0735211292",
                    Price = 27.00m,
                    DiscountPercentage = 20m,
                    StockQuantity = 50,
                    CoverImageUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&q=80&w=800",
                    IsActive = true,
                    IsFeatured = false,
                    IsVolumeOfTheMonth = false,
                    CategoryId = selfHelp.Id,
                    AuthorId = clear.Id,
                    PublisherId = avery.Id
                };

                _db.Books.AddRange(cleanCode, pragmaticProg, dune, atomicHabits);
                _db.SaveChanges();
            }
            else
            {
                // 3. Ensure existing database records have category icons, Arabic names, and home curation flags synced
                var categoryArabicMap = new Dictionary<string, (string NameAr, string Icon)>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Science Fiction"] = ("خيال علمي", "bi-rocket-takeoff"),
                    ["Technology & Programming"] = ("تكنولوجيا وبرمجة", "bi-code-slash"),
                    ["Business & Finance"] = ("أعمال واقتصاد", "bi-briefcase"),
                    ["Fiction"] = ("روايات وقصص", "bi-journal-bookmark"),
                    ["Self-Help"] = ("تطوير الذات", "bi-lightbulb"),
                    ["Philosophy"] = ("فلسفة وفكر", "bi-book"),
                    ["History"] = ("تاريخ وحضارة", "bi-hourglass-split"),
                    ["Poetry"] = ("شعر وأدب", "bi-feather"),
                    ["Science"] = ("علوم ومعرفة", "bi-compass"),
                    ["Biography"] = ("سير وتراجم", "bi-person-badge"),
                    ["Test Category"] = ("قسم تجريبي", "bi-bookmark")
                };

                foreach (var cat in _db.Categories.ToList())
                {
                    if (categoryArabicMap.TryGetValue(cat.Name, out var mapping))
                    {
                        cat.NameAr = mapping.NameAr;
                        if (!string.IsNullOrEmpty(mapping.Icon)) cat.IconClass = mapping.Icon;
                    }
                    else if (string.IsNullOrWhiteSpace(cat.NameAr))
                    {
                        cat.NameAr = cat.Name;
                    }
                }

                // Ensure books have a default Language
                foreach (var b in _db.Books.Where(b => string.IsNullOrEmpty(b.Language)))
                {
                    b.Language = "English";
                }

                // Ensure at least one Volume of the Month is chosen
                if (!_db.Books.Any(b => b.IsVolumeOfTheMonth))
                {
                    var volume = _db.Books.FirstOrDefault(b => b.Title.Contains("Pragmatic")) ?? _db.Books.FirstOrDefault();
                    if (volume != null) volume.IsVolumeOfTheMonth = true;
                }

                // Ensure featured books exist
                if (!_db.Books.Any(b => b.IsFeatured))
                {
                    var cleanCodeBook = _db.Books.FirstOrDefault(b => b.Title.Contains("Clean Code"));
                    if (cleanCodeBook != null) cleanCodeBook.IsFeatured = true;

                    var duneBook = _db.Books.FirstOrDefault(b => b.Title.Contains("Dune"));
                    if (duneBook != null) duneBook.IsFeatured = true;
                }

                // Ensure default StoreSetting exists
                if (!_db.StoreSettings.Any())
                {
                    _db.StoreSettings.Add(new StoreSetting
                    {
                        Id = 1,
                        FreeShippingThreshold = 50.00m,
                        FlatShippingRate = 4.99m,
                        MaxQuantityPerBook = 10,
                        CartAnnouncementBanner = "Complimentary literary bookmark & archival packaging on all orders over $50.",
                        CartAnnouncementBannerAr = "تغليف إهدائي فاخر وفاصل كتب مجاني مع كل طلب يتجاوز 50 دولاراً.",
                        DefaultLanguage = "en",
                        EnableLanguageSwitcher = true,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                _db.SaveChanges();
            }
        }
    }
}
