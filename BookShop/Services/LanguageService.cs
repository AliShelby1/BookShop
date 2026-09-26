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
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    var feature = httpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>();
                    if (feature != null)
                    {
                        var lang = feature.RequestCulture.UICulture.TwoLetterISOLanguageName.ToLowerInvariant();
                        if (lang == "ar") return "ar";
                        if (lang == "en") return "en";
                    }
                }
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
                ["Nav_Wishlist"] = "Reading List",
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

                // Wishlist / Reading List
                ["Wishlist_Title"] = "Your Reading List",
                ["Wishlist_Subtitle"] = "Curated literary volumes saved for future exploration and acquisition.",
                ["Wishlist_VolumesCount"] = "volumes saved",
                ["Wishlist_VolumeCount"] = "volume saved",
                ["Wishlist_EmptyTitle"] = "Your Reading List is Empty",
                ["Wishlist_EmptyDesc"] = "You haven't saved any books yet. Explore our shelves and click the heart icon on any edition to keep it close at hand.",
                ["Wishlist_ExploreBtn"] = "Explore Books Catalog",
                ["Wishlist_MoveToBag"] = "Move to Bag",
                ["Wishlist_Remove"] = "Remove",
                ["Wishlist_DateAdded"] = "Saved on",
                ["Wishlist_InStock"] = "In Stock",
                ["Wishlist_OutOfStock"] = "Sold Out",
                ["Wishlist_Save"] = "Save to Reading List",
                ["Wishlist_Saved"] = "In Reading List",

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
                ["Footer_Copyright"] = "All rights reserved. Built for readers and thinkers.",

                // Catalog & Shop
                ["Catalog_Title"] = "The Catalog",
                ["Catalog_Header"] = "The Bookstore Collection",
                ["Catalog_Subtitle"] = "Browse through our complete repository of printed works",
                ["Catalog_Showing"] = "Showing {0} of {1} titles",
                ["Catalog_Refine"] = "Refine Selection",
                ["Catalog_Reset"] = "Reset",
                ["Catalog_SearchKeywords"] = "Search Keywords",
                ["Catalog_SearchPlaceholder"] = "Title, author, ISBN...",
                ["Catalog_Genre"] = "Literary Genre",
                ["Catalog_AllGenres"] = "All Genres",
                ["Catalog_Author"] = "Author / Creator",
                ["Catalog_AllAuthors"] = "-- All Authors --",
                ["Catalog_PriceLimit"] = "Price Limit ($)",
                ["Catalog_Min"] = "Min",
                ["Catalog_Max"] = "Max",
                ["Catalog_InStockOnly"] = "In Stock Only",
                ["Catalog_FilterBtn"] = "Filter Collection",
                ["Catalog_ResultsFor"] = "Results for",
                ["Catalog_DisplayingCollection"] = "Displaying collection",
                ["Catalog_OrderBy"] = "Order By:",
                ["Catalog_SortNewest"] = "Newest Additions",
                ["Catalog_SortPriceAsc"] = "Price: Low to High",
                ["Catalog_SortPriceDesc"] = "Price: High to Low",
                ["Catalog_SortTitleAsc"] = "Title: A to Z",
                ["Catalog_NoVolumes"] = "No Volumes Found",
                ["Catalog_NoVolumesDesc"] = "We could not find any books matching your specific criteria.",
                ["Catalog_ClearFilters"] = "Clear Active Filters",
                ["Catalog_Previous"] = "Previous",
                ["Catalog_Next"] = "Next",
                ["Book_Language"] = "Language",

                // Checkout Flow
                ["Checkout_AddressTitle"] = "Shipping Address",
                ["Checkout_ReviewTitle"] = "Review Your Order",
                ["Checkout_ConfirmationTitle"] = "Order Confirmed",
                ["Checkout_StepAddress"] = "Address",
                ["Checkout_StepReview"] = "Review",
                ["Checkout_StepConfirmation"] = "Confirmation",
                ["Checkout_ShippingAddress"] = "Shipping Address",
                ["Checkout_OrderSummary"] = "Order Summary",
                ["Checkout_Email"] = "Email Address",
                ["Checkout_EmailPlaceholder"] = "you@example.com",
                ["Checkout_EmailHint"] = "Your receipt and order confirmation will be dispatched here.",
                ["Checkout_AlreadyHaveAccount"] = "Already have an account? Sign in for saved addresses.",
                ["Checkout_ReceiptDispatchedTo"] = "A receipt has been dispatched to {0}.",
                ["Checkout_CreateAccountHeading"] = "Save this order to your account",
                ["Checkout_CreateAccountDesc"] = "Create a password to access your reading history, track upcoming shipments, and maintain your personal library.",
                ["Checkout_FullName"] = "Full Name",
                ["Checkout_FullNamePlaceholder"] = "Your full name",
                ["Checkout_StreetAddress"] = "Street Address",
                ["Checkout_StreetAddressPlaceholder"] = "123 Reading Lane",
                ["Checkout_City"] = "City",
                ["Checkout_CityPlaceholder"] = "City",
                ["Checkout_State"] = "State / Province",
                ["Checkout_StatePlaceholder"] = "Optional",
                ["Checkout_PostalCode"] = "Postal Code",
                ["Checkout_PostalCodePlaceholder"] = "00000",
                ["Checkout_Country"] = "Country",
                ["Checkout_CountryPlaceholder"] = "Country",
                ["Checkout_Phone"] = "Phone Number (optional)",
                ["Checkout_PhonePlaceholder"] = "+1 (555) 000-0000",
                ["Checkout_Notes"] = "Order Notes (optional)",
                ["Checkout_NotesPlaceholder"] = "Special instructions for your order...",
                ["Checkout_ContinueToReview"] = "Continue to Review",
                ["Checkout_EditAddress"] = "Edit",
                ["Checkout_YourOrder"] = "Your Order",
                ["Checkout_PlaceOrder"] = "Place Order",
                ["Checkout_SecureCheckout"] = "Secure & encrypted checkout",
                ["Checkout_ThankYou"] = "Thank You for Your Order!",
                ["Checkout_ConfirmationSubtitle"] = "Your order has been received and is being prepared.",
                ["Checkout_OrderDate"] = "Order Date",
                ["Checkout_ItemsOrdered"] = "Items Ordered",
                ["Checkout_ContinueBrowsing"] = "Continue Browsing",

                // Order Management & History (Phase 7)
                ["Nav_MyOrders"] = "My Orders",
                ["Nav_OrdersFulfillment"] = "Orders & Fulfillment",
                ["Nav_AllOrders"] = "All Orders",
                ["Order_ManageAllTitle"] = "Orders Management",
                ["Order_MyOrdersTitle"] = "My Orders",
                ["Order_DetailsTitle"] = "Order Details",
                ["Order_SearchPlaceholder"] = "Search order #, customer, email, phone...",
                ["Order_TabAll"] = "All",
                ["Order_StatusPending"] = "Pending",
                ["Order_StatusConfirmed"] = "Confirmed",
                ["Order_StatusPreparing"] = "Preparing",
                ["Order_StatusShipped"] = "Dispatched",
                ["Order_StatusDelivered"] = "Delivered",
                ["Order_StatusCancelled"] = "Cancelled",
                ["Order_NoOrdersTitle"] = "No Orders Found",
                ["Order_NoOrdersDesc"] = "You haven't placed any orders yet, or no orders match the selected filter.",
                ["Order_TableNumber"] = "Order #",
                ["Order_TableDate"] = "Date",
                ["Order_TableCustomer"] = "Customer",
                ["Order_TableItems"] = "Items",
                ["Order_TableTotal"] = "Total",
                ["Order_TableStatus"] = "Status",
                ["Order_TableAction"] = "Action",
                ["Order_ViewDetails"] = "View Details",
                ["Order_BackToList"] = "Back to Orders",
                ["Order_CancelledDesc"] = "This order has been cancelled and any reserved items have been restored to our inventory.",
                ["Order_StaffControls"] = "Fulfillment Controls",
                ["Order_ActionConfirm"] = "Confirm Order",
                ["Order_ActionPrepare"] = "Start Preparing",
                ["Order_ActionShip"] = "Mark Dispatched",
                ["Order_ActionDeliver"] = "Mark Delivered",
                ["Order_ActionCancel"] = "Cancel & Restock",
                ["Order_ConfirmCancel"] = "Are you sure you want to cancel this order? This will restore books to inventory.",
                ["Order_CustomerCancelBtn"] = "Cancel This Order",
                ["Order_DispatchedOn"] = "Dispatched On",
                ["Order_DeliveredOn"] = "Delivered On",
                ["Order_PaymentStatus"] = "Payment Status"
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
                ["Nav_Wishlist"] = "قائمة القراءة",
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

                // Wishlist / Reading List
                ["Wishlist_Title"] = "قائمة قراءتك المحفوظة",
                ["Wishlist_Subtitle"] = "مجلدات وإصدارات أدبية قمت بحفظها للمطالعة واقتنائها لاحقاً.",
                ["Wishlist_VolumesCount"] = "إصدارات محفوظة",
                ["Wishlist_VolumeCount"] = "إصدار محفوظ",
                ["Wishlist_EmptyTitle"] = "قائمة قراءتك فارغة حالياً",
                ["Wishlist_EmptyDesc"] = "لم تقم بحفظ أي كتب بعد. تجول بين أرففنا واضغط على رمز القلب بجوار أي كتاب لحفظه في قائمتك الخاصة.",
                ["Wishlist_ExploreBtn"] = "استكشف فهرس الكتب",
                ["Wishlist_MoveToBag"] = "نقل إلى الحقيبة",
                ["Wishlist_Remove"] = "حذف من القائمة",
                ["Wishlist_DateAdded"] = "أضيف في",
                ["Wishlist_InStock"] = "متوفر في المخزون",
                ["Wishlist_OutOfStock"] = "نفد من المخزون",
                ["Wishlist_Save"] = "حفظ في قائمة القراءة",
                ["Wishlist_Saved"] = "محفوظ في القائمة",

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
                ["Footer_Copyright"] = "جميع الحقوق محفوظة. صُمم للقراء والمفكرين.",

                // Catalog & Shop
                ["Catalog_Title"] = "الكتالوج الأدبي",
                ["Catalog_Header"] = "مجموعة بوك شوب الأدبية",
                ["Catalog_Subtitle"] = "تصفح مجموعتنا الكاملة من الأعمال والمطبوعات الأدبية المتميزة",
                ["Catalog_Showing"] = "عرض {0} من أصل {1} عنواناً",
                ["Catalog_Refine"] = "تصفية النتائج",
                ["Catalog_Reset"] = "إعادة تعيين",
                ["Catalog_SearchKeywords"] = "كلمات البحث",
                ["Catalog_SearchPlaceholder"] = "العنوان، المؤلف، الرقم المعياري...",
                ["Catalog_Genre"] = "التصنيف الأدبي",
                ["Catalog_AllGenres"] = "جميع التصنيفات",
                ["Catalog_Author"] = "المؤلف / الكاتب",
                ["Catalog_AllAuthors"] = "-- جميع المؤلفين --",
                ["Catalog_PriceLimit"] = "الحد الأقصى للسعر ($)",
                ["Catalog_Min"] = "الأدنى",
                ["Catalog_Max"] = "الأقصى",
                ["Catalog_InStockOnly"] = "المتوفر في المخزن فقط",
                ["Catalog_FilterBtn"] = "تطبيق التصفية",
                ["Catalog_ResultsFor"] = "نتائج البحث عن",
                ["Catalog_DisplayingCollection"] = "عرض المجموعة الأدبية",
                ["Catalog_OrderBy"] = "الترتيب حسب:",
                ["Catalog_SortNewest"] = "أحدث الإصدارات",
                ["Catalog_SortPriceAsc"] = "السعر: من الأقل للأعلى",
                ["Catalog_SortPriceDesc"] = "السعر: من الأعلى للأقل",
                ["Catalog_SortTitleAsc"] = "العنوان: أبجدياً (A إلى Z)",
                ["Catalog_NoVolumes"] = "لم يتم العثور على أية كتب",
                ["Catalog_NoVolumesDesc"] = "لم نتمكن من العثور على أية كتب تطابق معايير البحث المحددة.",
                ["Catalog_ClearFilters"] = "مسح عوامل التصفية",
                ["Catalog_Previous"] = "السابق",
                ["Catalog_Next"] = "التالي",
                ["Book_Language"] = "لغة الإصدار",

                // Checkout Flow
                ["Checkout_AddressTitle"] = "عنوان الشحن",
                ["Checkout_ReviewTitle"] = "مراجعة طلبك",
                ["Checkout_ConfirmationTitle"] = "تم تأكيد الطلب",
                ["Checkout_StepAddress"] = "العنوان",
                ["Checkout_StepReview"] = "المراجعة",
                ["Checkout_StepConfirmation"] = "التأكيد",
                ["Checkout_ShippingAddress"] = "عنوان الشحن",
                ["Checkout_OrderSummary"] = "ملخص الطلب",
                ["Checkout_Email"] = "البريد الإلكتروني",
                ["Checkout_EmailPlaceholder"] = "name@example.com",
                ["Checkout_EmailHint"] = "سنرسل إشعار تأكيد الطلب وإيصال الشحن إلى هذا العنوان.",
                ["Checkout_AlreadyHaveAccount"] = "هل لديك حساب بالفعل؟ سجّل دخولك لتعبئة العنوان تلقائياً.",
                ["Checkout_ReceiptDispatchedTo"] = "تم إرسال نسخة من إيصال الشراء إلى {0}.",
                ["Checkout_CreateAccountHeading"] = "احفظ هذا الطلب في حسابك",
                ["Checkout_CreateAccountDesc"] = "أنشئ كلمة مرور لتتمكن من تتبع شحناتك، واستعراض سجل قراءتك، والاستفادة من مزايا القراء.",
                ["Checkout_FullName"] = "الاسم الكامل",
                ["Checkout_FullNamePlaceholder"] = "اسمك الكامل",
                ["Checkout_StreetAddress"] = "عنوان الشارع",
                ["Checkout_StreetAddressPlaceholder"] = "شارع القراءة 123",
                ["Checkout_City"] = "المدينة",
                ["Checkout_CityPlaceholder"] = "المدينة",
                ["Checkout_State"] = "الولاية / المنطقة",
                ["Checkout_StatePlaceholder"] = "اختياري",
                ["Checkout_PostalCode"] = "الرمز البريدي",
                ["Checkout_PostalCodePlaceholder"] = "00000",
                ["Checkout_Country"] = "الدولة",
                ["Checkout_CountryPlaceholder"] = "الدولة",
                ["Checkout_Phone"] = "رقم الهاتف (اختياري)",
                ["Checkout_PhonePlaceholder"] = "+966 5X XXX XXXX",
                ["Checkout_Notes"] = "ملاحظات الطلب (اختياري)",
                ["Checkout_NotesPlaceholder"] = "تعليمات خاصة لطلبك...",
                ["Checkout_ContinueToReview"] = "المتابعة إلى المراجعة",
                ["Checkout_EditAddress"] = "تعديل",
                ["Checkout_YourOrder"] = "طلبك",
                ["Checkout_PlaceOrder"] = "تأكيد الطلب",
                ["Checkout_SecureCheckout"] = "دفع آمن ومشفر",
                ["Checkout_ThankYou"] = "شكراً لطلبك!",
                ["Checkout_ConfirmationSubtitle"] = "تم استلام طلبك وهو قيد التجهيز.",
                ["Checkout_OrderDate"] = "تاريخ الطلب",
                ["Checkout_ItemsOrdered"] = "الكتب المطلوبة",
                ["Checkout_ContinueBrowsing"] = "مواصلة التصفح",

                // Order Management & History (Phase 7)
                ["Nav_MyOrders"] = "طلباتي",
                ["Nav_OrdersFulfillment"] = "إدارة الطلبات والشحن",
                ["Nav_AllOrders"] = "جميع الطلبات",
                ["Order_ManageAllTitle"] = "إدارة وتجهيز الطلبات",
                ["Order_MyOrdersTitle"] = "سجل طلباتي",
                ["Order_DetailsTitle"] = "تفاصيل الطلب",
                ["Order_SearchPlaceholder"] = "ابحث برقم الطلب، العميل، البريد، الهاتف...",
                ["Order_TabAll"] = "الكل",
                ["Order_StatusPending"] = "قيد الانتظار",
                ["Order_StatusConfirmed"] = "تم التأكيد",
                ["Order_StatusPreparing"] = "قيد التجهيز",
                ["Order_StatusShipped"] = "تم الشحن",
                ["Order_StatusDelivered"] = "تم التوصيل",
                ["Order_StatusCancelled"] = "ملغي",
                ["Order_NoOrdersTitle"] = "لا توجد أية طلبات",
                ["Order_NoOrdersDesc"] = "لم تقم بإنشاء أية طلبات بعد، أو لا توجد طلبات تطابق الفلتر المحدد.",
                ["Order_TableNumber"] = "رقم الطلب",
                ["Order_TableDate"] = "التاريخ",
                ["Order_TableCustomer"] = "العميل",
                ["Order_TableItems"] = "الكتب",
                ["Order_TableTotal"] = "الإجمالي",
                ["Order_TableStatus"] = "الحالة",
                ["Order_TableAction"] = "الإجراء",
                ["Order_ViewDetails"] = "عرض التفاصيل",
                ["Order_BackToList"] = "العودة للطلبات",
                ["Order_CancelledDesc"] = "تم إلغاء هذا الطلب وإعادة الكتب المحجوزة إلى المخزون تلقائياً.",
                ["Order_StaffControls"] = "إجراءات التجهيز والشحن",
                ["Order_ActionConfirm"] = "تأكيد الطلب",
                ["Order_ActionPrepare"] = "بدء تجهيز الطلب",
                ["Order_ActionShip"] = "تسليم لشركة الشحن",
                ["Order_ActionDeliver"] = "تم التوصيل للعميل",
                ["Order_ActionCancel"] = "إلغاء وإعادة للمخزون",
                ["Order_ConfirmCancel"] = "هل أنت متأكد من رغبتك في إلغاء هذا الطلب؟ سيتم إرجاع الكتب إلى المخزون فوراً.",
                ["Order_CustomerCancelBtn"] = "إلغاء هذا الطلب",
                ["Order_DispatchedOn"] = "تاريخ الشحن",
                ["Order_DeliveredOn"] = "تاريخ التوصيل",
                ["Order_PaymentStatus"] = "حالة الدفع"
            }
        };
    }
}
