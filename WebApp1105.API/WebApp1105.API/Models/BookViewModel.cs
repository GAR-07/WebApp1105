namespace WebApp1105.API.Models
{
    public class BookViewModel
    {
        public int id { get; set; }
        public string userId { get; set; }
        public string genre { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string? annotation { get; set; }
        public string bookFilePath { get; set; }
        public int writingDate { get; set; }
        public int? publicationDate { get; set; }
        public string coverPath { get; set; }
        public int? numberOfPages { get; set; }
        public string? publisher { get; set; }
    }
}