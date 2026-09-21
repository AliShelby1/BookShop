using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BookShop.Models.ViewModels
{
    public class RoleManagementVM
    {
        public string UserId { get; set; } = default!;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? CurrentRole { get; set; }
        public string SelectedRole { get; set; } = default!;

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
    }
}
