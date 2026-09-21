using BookShop.Models;
using System.Collections.Generic;

namespace BookShop.Models.ViewModels
{
    public class StorefrontCuratorVM
    {
        public Book? CurrentVolumeOfTheMonth { get; set; }
        public int? SelectedVolumeId { get; set; }

        public IEnumerable<Book> AllActiveBooks { get; set; } = new List<Book>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public List<int> FeaturedBookIds { get; set; } = new List<int>();

        public StoreSetting StoreSettings { get; set; } = new StoreSetting();
    }
}
