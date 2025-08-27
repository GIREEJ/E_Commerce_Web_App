using ECommerceWebApp.Services.Interfaces;

namespace ECommerceWebApp.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUploadService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            // Corrected path
            var relativeFolderPath = "Uploads/Images/User";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, relativeFolderPath);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return correct virtual path
            return "/" + relativeFolderPath.Replace("\\", "/") + "/" + uniqueFileName;
        }
    }
}
