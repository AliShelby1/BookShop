using BookShop.Models;
using System;
using System.Collections.Generic;

namespace BookShop.Models.ViewModels
{
    public class ShopVM
    {
        // Books for current page
        public IEnumerable<Book> Books { get; set; } = new List<Book>();

        // Filter Inputs
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
        public int? AuthorId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStockOnly { get; set; }
        public string SortBy { get; set; } = "newest";

        // Reference Data for Filters
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Author> Authors { get; set; } = new List<Author>();

        // Pagination Properties
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 8;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 8));
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
