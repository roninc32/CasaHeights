using CasaHeights.Models.Forum;

namespace CasaHeights.ViewModels.Forum.Components
{
    public class ForumPostCardViewModel
    {
        public ForumPost Post { get; set; }
        public bool ShowModeratorActions { get; set; }
        public bool ShowFullContent { get; set; }
        public int MaxContentLength { get; set; } = 200;
        
        public string TruncatedContent => Post.Content.Length > MaxContentLength 
            ? Post.Content.Substring(0, MaxContentLength) + "..." 
            : Post.Content;
    }
}