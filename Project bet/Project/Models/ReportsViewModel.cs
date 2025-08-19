// Project.Models/ReportsViewModel.cs

using System.Collections.Generic;

namespace Project.Models
{
    public class CategoryCount
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    public class LatestItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public System.DateTime DateLost { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        public ItemType Type { get; set; }
    }

    public class ReportsViewModel
    {
        public List<CategoryCount> CategoryItemCounts { get; set; } = new List<CategoryCount>();
        public List<LatestItem> NewlyAddedItems { get; set; } = new List<LatestItem>();
    }
}