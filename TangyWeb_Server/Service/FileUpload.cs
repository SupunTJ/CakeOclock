﻿using Microsoft.AspNetCore.Components.Forms;
using TangyWeb_Server.Service.IService;

namespace TangyWeb_Server.Service
{
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUpload(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public bool DeleteFile(string filepath)
        {
            if(File.Exists(_webHostEnvironment.WebRootPath+filepath)) 
            {
                File.Delete(_webHostEnvironment.WebRootPath+filepath);
                return true;
            }
            return false;
        }

        public async Task<string> UploadFile(IBrowserFile file)
        {
            FileInfo fileInfo = new(file.Name);
            var fileName = Guid.NewGuid().ToString()+fileInfo.Extension;
            var folderDirectory = $"{_webHostEnvironment.WebRootPath}\\images\\product";
            if(!Directory.Exists(folderDirectory))
            {
                Directory.CreateDirectory(folderDirectory);
            }
            var filePath = Path.Combine(folderDirectory, fileName);

            // Increase the buffer size to handle large files efficiently
            await using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            await file.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024).CopyToAsync(fs); // 2 MB

            //await using FileStream fs = new FileStream(filePath, FileMode.Create);
            //await file.OpenReadStream().CopyToAsync(fs);

            var fullpath = $"/images/product/{fileName}";
            return fullpath;

        }
    }
}
