using BookShop.Models;
using System.Collections.Generic;

namespace BookShop.Models.ViewModels
{
    public class CustomerBookDetailsVM
    {
        public Book Book { get; set; } = null!;
        public IEnumerable<Book> RelatedBooks { get; set; } = new List<Book>();
        public int DefaultQuantity { get; set; } = 1;
    }
}
