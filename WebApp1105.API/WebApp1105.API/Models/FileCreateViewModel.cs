namespace WebApp1105.API.Models
{
    public class FileCreateViewModel
    {
        public string userId { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string fileName { get; set; }
        public string[] fileType { get; set; }
        public string filePath { get; set; }

    }
}