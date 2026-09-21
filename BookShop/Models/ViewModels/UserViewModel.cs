using System;

namespace BookShop.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = default!;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }
}
