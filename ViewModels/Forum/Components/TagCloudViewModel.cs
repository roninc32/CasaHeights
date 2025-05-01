namespace CasaHeights.ViewModels.Forum.Components
{
    public class TagCloudViewModel
    {
        public List<(string Name, int Count)> Tags { get; set; } = new();
        public string ActiveTag { get; set; }
        public bool ShowCounts { get; set; }
    }
}