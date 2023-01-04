namespace rembg_api
{
    public class ImageModel
    {
        public string? ModelType { get; set; } = "u2net_cloth_seg";
        public string? FileName { get; set; }
        public IFormFile? Image { get; set; }
    }
}