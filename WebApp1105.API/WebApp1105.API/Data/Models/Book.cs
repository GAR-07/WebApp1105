using System.ComponentModel.DataAnnotations;

namespace WebApp1105.API.Data.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Genre { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string? Annotation { get; set; }
        public string BookFilePath { get; set; }
        public byte[] BookFileData { get; set; }
        public int WritingDate { get; set; }
        public int? PublicationDate { get; set; }
        public string CoverPath { get; set; }
        public byte[] CoverData { get; set; }
        public int? NumberOfPages { get; set; }
        public string? Publisher { get; set; }
    }
}