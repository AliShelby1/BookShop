using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace BookShop.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CurrentCulture
        {
            get
            {
                var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
                return culture == "ar" ? "ar" : "en";
            }
        }

        public bool IsRtl => CurrentCulture == "ar";

        public string this[string key] => T(key);

        public string T(string key, string? fallback = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            var culture = CurrentCulture;
            if (Translations.TryGetValue(culture, out var dict) && dict.TryGetValue(key, out var translation))
            {
                return translation;
            }

            // Fallback to English if Arabic translation is missing
            if (culture != "en" && Translations.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out var enTranslation))
            {
                return enTranslation;
            }

            return fallback ?? key;
        }

        public IReadOnlyDictionary<string, string> GetSupportedLanguages()
        {
            return new Dictionary<string, string>
            {
                { "en", "English" },
                { "ar", "العربية" }
            };
        }

        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
        {
            ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Top Announcement & Navbar
                ["Top_Announcement"] = "Curated literary editions & independent presses · Complimentary delivery on orders over $50",
                ["Nav_Home"] = "Home",
                ["Nav_Catalog"] = "Shop Catalog",
                ["Nav_Privacy"] = "Privacy",
                ["Nav_AdminPortal"] = "Admin Portal",
                ["Nav_Curator"] = "Homepage Curator",
                ["Nav_Policies"] = "Store & Shipping Policies",
                ["Nav_BooksInventory"] = "Books Inventory",
                ["Nav_Categories"] = "Categories",
                ["Nav_Authors"] = "Authors",
                ["Nav_Publishers"] = "Publishers",
                ["Nav_ManageUsers"] = "Manage Users",
                ["Nav_CreateStaff"] = "Create New Staff",
                ["Nav_ReadingBag"] = "Reading Bag",
                ["Nav_Register"] = "Register",
                ["Nav_SignIn"] = "Sign In",
                ["Nav_SignOut"] = "Sign Out",
                ["Nav_Hello"] = "Hello",

                // Homepage Hero
                ["Hero_Badge"] = "Independent Bookseller · Est. 2026",
                ["Hero_Title_1"] = "A Quiet Sanctuary for",
                ["Hero_Title_2"] = "Devoted Readers",
                ["Hero_Subtitle"] = "Carefully curated editions, timeless literary classics, and modern technical volumes. Bound with craft, selected with care, and shipped to your personal library.",
                ["Hero_ExploreBtn"] = "Explore Collection",
                ["Hero_SpotlightBadge"] = "Selected Volume of the Month",
                ["Hero_OrderSpotlight"] = "Order This Edition",
                ["Hero_ViewDetails"] = "View Details",

                // Homepage Sections
                ["Genre_Section_Title"] = "Curated Shelves & Genres",
                ["Genre_Section_Sub"] = "Wander through our thoughtfully organized literary categories",
                ["Featured_Title"] = "Featured Literary Editions",
                ["Featured_Sub"] = "Distinguished volumes hand-selected for exceptional craft and thought",
                ["Recent_Title"] = "Recent Additions to the Shelves",
                ["Recent_Sub"] = "Fresh printings and newly archived volumes ready for discovery",
                ["View_All_Catalog"] = "Browse Complete Archive",

                // Commitments
                ["Commitments_Wrap"] = "Archival Wrapping",
                ["Commitments_Wrap_Desc"] = "Every edition is protected with classic kraft wrap and wax seal.",
                ["Commitments_Shipping"] = "Careful Delivery",
                ["Commitments_Shipping_Desc"] = "Dispatched within 24 hours in reinforced protective cardboard.",
                ["Commitments_Returns"] = "30-Day Fair Return",
                ["Commitments_Returns_Desc"] = "If a volume does not resonate, return it with our fair policy.",

                // Book Cards & Details
                ["Book_ViewEdition"] = "View Edition",
                ["Book_Available"] = "Available",
                ["Book_FewLeft"] = "Few Left",
                ["Book_SoldOut"] = "Sold Out",
                ["Book_InStock"] = "In Stock",
                ["Book_OnlyCopies"] = "Only {0} left in stock",
                ["Book_OutOfStock"] = "Currently Out of Stock",
                ["Book_By"] = "by",
                ["Book_PublishedBy"] = "Published by",
                ["Book_Quantity"] = "Quantity",
                ["Book_AddToBag"] = "Add to Bag",
                ["Book_SaveWishlist"] = "Save to Reading List",
                ["Book_SaveDiscount"] = "Save {0}",
                ["Book_ComplimentaryPackaging"] = "Complimentary packaging included. Shipping calculated at checkout.",
                ["Book_Guarantee_Wrap"] = "Protective Book Wrap",
                ["Book_Guarantee_Returns"] = "30-Day Fair Returns",
                ["Book_Guarantee_Secure"] = "Encrypted Checkout",
                ["Book_OutOfStockAlert"] = "This volume is currently out of stock. Check back soon or explore related titles below.",
                ["Book_RelatedTitles"] = "From the Same Category",

                // Shopping Bag / Cart
                ["Cart_Title"] = "Your Reading Bag",
                ["Cart_VolumesCount"] = "volumes",
                ["Cart_VolumeCount"] = "volume",
                ["Cart_VolumeEdition"] = "Book Edition",
                ["Cart_Price"] = "Price",
                ["Cart_Quantity"] = "Quantity",
                ["Cart_LineTotal"] = "Line Total",
                ["Cart_ContinueBrowsing"] = "Continue Browsing",
                ["Cart_ContinueShopping"] = "Continue Reading & Shopping",
                ["Cart_ClearBag"] = "Clear Bag",
                ["Cart_ClearBagConfirm"] = "Are you sure you want to clear your reading bag?",
                ["Cart_OrderSummary"] = "Order Summary",
                ["Cart_Subtotal"] = "Subtotal",
                ["Cart_Savings"] = "Literary Savings",
                ["Cart_EstimatedShipping"] = "Estimated Shipping",
                ["Cart_Complimentary"] = "Complimentary",
                ["Cart_TotalDue"] = "Total Due",
                ["Cart_ProceedCheckout"] = "Proceed to Checkout",
                ["Cart_TaxesNote"] = "Taxes and final shipping confirmed during checkout",
                ["Cart_FreeShippingUnlocked"] = "Complimentary shipping unlocked!",
                ["Cart_FreeShippingThreshold"] = "Threshold",
                ["Cart_EmptyTitle"] = "Your Reading Bag is Empty",
                ["Cart_EmptyDesc"] = "Our shelves are stocked with timeless classics, technical handbooks, and rare literary works waiting to find a home in your library.",
                ["Cart_ExploreBtn"] = "Explore Books Catalog",
                ["Cart_ReturnSanctuary"] = "Return to Sanctuary",

                // Footer
                ["Footer_Mission"] = "A peaceful sanctuary for readers, researchers, and book collectors. We celebrate the physical form of the printed word, offering curated editions from independent authors, classic literature, and contemporary voices.",
                ["Footer_Dispatched"] = "Independent Booksellers · Nationwide Shipping",
                ["Footer_ExploreCatalog"] = "Explore Catalog",
                ["Footer_AllVolumes"] = "All Volumes",
                ["Footer_RecentArrivals"] = "Recent Arrivals",
                ["Footer_InStock"] = "In Stock Titles",
                ["Footer_Policies"] = "Store Policies",
                ["Footer_Dispatch"] = "The Literary Dispatch",
                ["Footer_DispatchDesc"] = "Subscribe to receive monthly curated book lists and author spotlight essays.",
                ["Footer_EnterEmail"] = "Enter your email...",
                ["Footer_Subscribe"] = "Subscribe",
                ["Footer_Copyright"] = "All rights reserved. Built for readers and thinkers."
            },

            ["ar"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Top Announcement & Navbar
                ["Top_Announcement"] = "إصدارات أدبية منتقاة ومطابع مستقلة · توصيل مجاني للطلبات التي تتجاوز 50 دولاراً",
                ["Nav_Home"] = "الرئيسية",
                ["Nav_Catalog"] = "فهرس الكتب",
                ["Nav_Privacy"] = "الخصوصية",
                ["Nav_AdminPortal"] = "بوابة الإدارة",
                ["Nav_Curator"] = "أمين واجهة المتجر",
                ["Nav_Policies"] = "سياسات المتجر والشحن",
                ["Nav_BooksInventory"] = "مخزون الكتب",
                ["Nav_Categories"] = "التصنيفات",
                ["Nav_Authors"] = "المؤلفون",
                ["Nav_Publishers"] = "دور النشر",
                ["Nav_ManageUsers"] = "إدارة المستخدمين",
                ["Nav_CreateStaff"] = "إضافة موظف جديد",
                ["Nav_ReadingBag"] = "حقيبة القراءة",
                ["Nav_Register"] = "إنشاء حساب",
                ["Nav_SignIn"] = "تسجيل الدخول",
                ["Nav_SignOut"] = "تسجيل الخروج",
                ["Nav_Hello"] = "مرحباً",

                // Homepage Hero
                ["Hero_Badge"] = "مكتبة أدبية مستقلة · تأسست 2026",
                ["Hero_Title_1"] = "ملاذ هادئ لعشاق",
                ["Hero_Title_2"] = "القراءة والكتب النادرة",
                ["Hero_Subtitle"] = "إصدارات منتقاة بعناية، كلاسيكيات أدبية خالدة، ومراجع تقنية حديثة. مقتناة بعناية وحرفية لتصل مباشرة إلى مكتبتك الخاصة.",
                ["Hero_ExploreBtn"] = "استكشف المجموعة",
                ["Hero_SpotlightBadge"] = "مجلد الشهر المختار",
                ["Hero_OrderSpotlight"] = "اطلب هذا الإصدار",
                ["Hero_ViewDetails"] = "تفاصيل الإصدار",

                // Homepage Sections
                ["Genre_Section_Title"] = "أرفف وتصنيفات المكتبة",
                ["Genre_Section_Sub"] = "تجوّل بين أقسامنا الأدبية المصنفة بعناية وإتقان",
                ["Featured_Title"] = "إصدارات أدبية مميزة",
                ["Featured_Sub"] = "مجلدات متميزة تم اختيارها يدوياً لفرادة محتواها وأصالتها",
                ["Recent_Title"] = "أحدث الإضافات إلى الأرفف",
                ["Recent_Sub"] = "طبعات حديثة ومجلدات مضافة حديثاً بانتظار قراءتك",
                ["View_All_Catalog"] = "استعراض الأرشيف كاملاً",

                // Commitments
                ["Commitments_Wrap"] = "تغليف ورقي فاخر",
                ["Commitments_Wrap_Desc"] = "كل إصدار مغلف بعناية بورق الكرافت الطبيعي وختم شمعي كلاسيكي.",
                ["Commitments_Shipping"] = "شحن آمن وفائق العناية",
                ["Commitments_Shipping_Desc"] = "يتم التجهيز والشحن خلال 24 ساعة في عبوات كرتونية مقواة لحماية الكتاب.",
                ["Commitments_Returns"] = "إرجاع ميسر خلال 30 يوماً",
                ["Commitments_Returns_Desc"] = "إذا لم يلامس الكتاب ذائقتك، يمكنك إرجاعه بكل سلاسة وفق سياستنا.",

                // Book Cards & Details
                ["Book_ViewEdition"] = "عرض الإصدار",
                ["Book_Available"] = "متوفر",
                ["Book_FewLeft"] = "نسخ محدودة",
                ["Book_SoldOut"] = "نفدت الكمية",
                ["Book_InStock"] = "متوفر في المخزون",
                ["Book_OnlyCopies"] = "متبقي فقط {0} نسخ",
                ["Book_OutOfStock"] = "غير متوفر حالياً",
                ["Book_By"] = "بقلم",
                ["Book_PublishedBy"] = "دار النشر",
                ["Book_Quantity"] = "الكمية",
                ["Book_AddToBag"] = "أضف إلى حقيبة القراءة",
                ["Book_SaveWishlist"] = "حفظ في قائمة القراءة",
                ["Book_SaveDiscount"] = "وفر {0}",
                ["Book_ComplimentaryPackaging"] = "التغليف الإهدائي مشمول مجاناً. يحسب الشحن عند إتمام الطلب.",
                ["Book_Guarantee_Wrap"] = "تغليف كلاسيكي واقٍ",
                ["Book_Guarantee_Returns"] = "إرجاع عادل خلال 30 يوماً",
                ["Book_Guarantee_Secure"] = "دفع آمن ومشفر",
                ["Book_OutOfStockAlert"] = "هذا المجلد غير متوفر حالياً. تفقد الموقع قريباً أو تصفح العناوين المقترحة أدناه.",
                ["Book_RelatedTitles"] = "من نفس التصنيف الأدبي",

                // Shopping Bag / Cart
                ["Cart_Title"] = "حقيبة القراءة",
                ["Cart_VolumesCount"] = "مجلدات",
                ["Cart_VolumeCount"] = "مجلد",
                ["Cart_VolumeEdition"] = "إصدار الكتاب",
                ["Cart_Price"] = "السعر",
                ["Cart_Quantity"] = "الكمية",
                ["Cart_LineTotal"] = "المجموع",
                ["Cart_ContinueBrowsing"] = "متابعة التصفح",
                ["Cart_ContinueShopping"] = "متابعة القراءة والتسوق",
                ["Cart_ClearBag"] = "تفريغ الحقيبة",
                ["Cart_ClearBagConfirm"] = "هل أنت متأكد من رغبتك في تفريغ حقيبة القراءة؟",
                ["Cart_OrderSummary"] = "ملخص الطلب",
                ["Cart_Subtotal"] = "المجموع الفرعي",
                ["Cart_Savings"] = "وفر الخصم",
                ["Cart_EstimatedShipping"] = "الشحن التقديري",
                ["Cart_Complimentary"] = "مجاني",
                ["Cart_TotalDue"] = "المبلغ الإجمالي",
                ["Cart_ProceedCheckout"] = "المتابعة لإتمام الشراء",
                ["Cart_TaxesNote"] = "يتم تأكيد الضرائب ورسوم الشحن النهائية عند الدفع",
                ["Cart_FreeShippingUnlocked"] = "تم تفعيل التوصيل المجاني!",
                ["Cart_FreeShippingThreshold"] = "الحد الأدنى",
                ["Cart_EmptyTitle"] = "حقيبة القراءة فارغة حالياً",
                ["Cart_EmptyDesc"] = "أرففنا ممتلئة بأندر العناوين والكلاسيكيات الأدبية والمراجع العلمية بانتظار انضمامها لمكتبتك الخاصة.",
                ["Cart_ExploreBtn"] = "استكشف فهرس الكتب",
                ["Cart_ReturnSanctuary"] = "العودة للرئيسية",

                // Footer
                ["Footer_Mission"] = "ملاذ هادئ للقراء والباحثين وهواة جمع الكتب. نحتفي بالقيمة الحقيقية للكلمة المطبوعة، ونقدم إصدارات منتقاة بعناية من كبار الأدباء والمفكرين المعاصرين.",
                ["Footer_Dispatched"] = "مكتبة مستقلة · شحن آمن لكافة المناطق",
                ["Footer_ExploreCatalog"] = "استكشف الفهرس",
                ["Footer_AllVolumes"] = "جميع الإصدارات",
                ["Footer_RecentArrivals"] = "أحدث الواصلات",
                ["Footer_InStock"] = "الكتب المتوفرة",
                ["Footer_Policies"] = "سياسات المتجر",
                ["Footer_Dispatch"] = "النشرة الأدبية الدورية",
                ["Footer_DispatchDesc"] = "اشترك لتصلك ترشيحات شهرية حصرية وقراءات أدبية معمقة.",
                ["Footer_EnterEmail"] = "أدخل بريدك الإلكتروني...",
                ["Footer_Subscribe"] = "اشتراك",
                ["Footer_Copyright"] = "جميع الحقوق محفوظة. صُمم للقراء والمفكرين."
            }
        };
    }
}
