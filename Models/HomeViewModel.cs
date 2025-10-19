namespace Project.Models
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Item> RecentLostItems { get; set; } = new();
        public List<Item> RecentFoundItems { get; set; } = new();
        public int TotalItems { get; set; }

        public int TotalLostItems { get; set; }

        public int TotalFoundItems { get; set; }
    }
}
