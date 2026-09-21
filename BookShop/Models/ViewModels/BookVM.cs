using BookShop.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BookShop.Models.ViewModels
{
    public class BookVM
    {
        public Book Book { get; set; } = new Book();

        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> AuthorList { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> PublisherList { get; set; } = new List<SelectListItem>();
    }
}
