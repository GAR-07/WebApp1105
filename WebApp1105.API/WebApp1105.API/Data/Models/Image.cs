using System.ComponentModel.DataAnnotations;

namespace WebApp1105.API.Data.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string ImgName { get; set; }
        public string ImgPath { get; set; }
        public byte[] ImgData { get; set; }
        public int ImgWidth { get; set; }
        public int ImgHeight { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}