using CasaHeights.Models.Forum.Enums;

namespace CasaHeights.ViewModels.Forum.Admin
{
    public class ForumStatistics
    {
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int ReportedPosts { get; set; }
        public int ActiveUsers { get; set; }
        
        public Dictionary<PostStatus, int> PostsByStatus { get; set; } = new();
        public Dictionary<string, int> PostsByCategory { get; set; } = new();
        
        public int DeletedPosts { get; set; }
        public int PinnedPosts { get; set; }
    }
}