namespace WebApp1105.API.Models
{
    public class VideoViewModel
    {
        public int[] id { get; set; }
        public string userId { get; set; }
        public string videoName { get; set; }
        public string[] videoPath { get; set; }
        public int[] videoWidth { get; set; }
        public int[] videoHeight { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
    }
}