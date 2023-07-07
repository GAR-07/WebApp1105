namespace WebApp1105.API.Models
{
    public class ItemsRequestModel
    {
        public string? userId { get; set; }
        public int pageSize { get; set; }
        public int page { get; set; }
    }
}
