namespace ST10296771_CLDV7311_POE.Config
{
    public class AzureStorageConfig
    {
        public static string ConnectionString { get; set; }
        public static string ContainerName { get; set; } = "eventimages";
        public static string ContainerUrl { get; set; }

        public static readonly string[] AllowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public static readonly long MaxFileSize = 5 * 1024 * 1024; 
    }
}
