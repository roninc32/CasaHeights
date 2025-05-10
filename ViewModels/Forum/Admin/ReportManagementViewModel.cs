using CasaHeights.Models.Forum;
using CasaHeights.Models.Forum.Enums;

namespace CasaHeights.ViewModels.Forum.Admin
{
    public class ReportManagementViewModel
    {
        public List<ForumReport> Reports { get; set; } = new();
        public Dictionary<ReportStatus, int> ReportsByStatus { get; set; } = new();
        
        // For handling reports
        public int ReportId { get; set; }
        public ReportStatus Status { get; set; }
        public string ModeratorNotes { get; set; }
    }
}