using System.ComponentModel.DataAnnotations;

namespace WebApp1105.API.Data.Models
{
    public class Video
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string VideoName { get; set; }
        public string VideoPath { get; set; }
        public byte[] VideoData { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}