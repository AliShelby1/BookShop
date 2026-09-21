using BookShop.Models;
using System.Collections.Generic;

namespace BookShop.Models.ViewModels
{
    public class HomeVM
    {
        public Book? VolumeOfTheMonth { get; set; }
        public IEnumerable<Book> FeaturedBooks { get; set; } = new List<Book>();
        public IEnumerable<Book> NewArrivals { get; set; } = new List<Book>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Author> FeaturedAuthors { get; set; } = new List<Author>();
    }
}
