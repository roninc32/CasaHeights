using CasaHeights.Models.Forum;

namespace CasaHeights.ViewModels.Forum.Admin
{
    public class ForumAdminIndexViewModel
    {
        public List<ForumPost> Posts { get; set; } = new();
        public string Filter { get; set; }
        public ForumStatistics Statistics { get; set; }

        // Quick actions
        public bool ShowDeleted { get; set; }
        public bool ShowReported { get; set; }
    }
}