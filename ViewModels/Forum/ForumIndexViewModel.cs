using CasaHeights.Models.Forum.Enums;
using CasaHeights.Models.Forum;
using System.Collections.Generic;

namespace CasaHeights.ViewModels.Forum
{
    public class ForumIndexViewModel
    {
        public List<ForumPost> Posts { get; set; } = new();
        public List<ForumCategory> Categories { get; set; } = new();
        public List<ForumTag> PopularTags { get; set; } = new();
        
        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        
        // Filters
        public string Category { get; set; }
        public string Tag { get; set; }
        public string SearchTerm { get; set; }
        
        // Stats
        public int TotalPosts { get; set; }
        public int ActiveDiscussions { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}