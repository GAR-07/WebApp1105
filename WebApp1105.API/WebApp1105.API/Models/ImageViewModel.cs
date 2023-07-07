namespace WebApp1105.API.Models
{
    public class ImageViewModel
    {
        public int[] id { get; set; }
        public string userId { get; set; }
        public string imgName { get; set; }
        public string[] imgPath { get; set; }
        public int[] imgWidth { get; set; }
        public int[] imgHeight { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
    }
}