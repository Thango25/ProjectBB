namespace Project.Models
{
    // This model will be used specifically for the Lost Items Claimed Report view
    public class ItemClaimReportEntry
    {
        public int ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string PostedByUserName { get; set; } = string.Empty; // Assuming you can link back to a user
        public System.DateTime DateLost { get; set; }
        // Note: You might need a DateClaimed property if you track that separately
    }
}