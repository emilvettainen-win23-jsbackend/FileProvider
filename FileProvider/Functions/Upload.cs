using Data.Entities;
using FileProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProvider.Functions;

public class Upload
{
    private readonly ILogger<Upload> _logger;
    private readonly FileService _fileService;

    public Upload(ILogger<Upload> logger, FileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    [Function("Upload")]
    public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            if (req.Form.Files["file"] is IFormFile file)
            {
                var maxAllowedFiles = 1;
                long maxFileSize = 2500 * 1500;

                if (file.Length > maxFileSize)
                {
                   return new BadRequestObjectResult("File size exceeds the limit.");
                }

                if (maxAllowedFiles > 1)
                {
                   return new BadRequestObjectResult("Exceeded the maximum number of files allowed.");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".svg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return new BadRequestObjectResult("Invalid file extension. Please use jpg, jpeg, svg or png.");
                }

                var containerName = !string.IsNullOrEmpty(req.Query["containerName"]) ? req.Query["containerName"].ToString() : "files";

                var fileEntity = new FileEntity
                {
                    FileName = _fileService.SetFileName(file),
                    ContentType = file.ContentType,
                    ContainerName = containerName
                };

                await _fileService.SetBlobContainerAsync(fileEntity.ContainerName);
                var filePath = await _fileService.UploadFileAsync(file, fileEntity);
                fileEntity.FilePath = filePath;

                await _fileService.SaveToDatabaseAsync(fileEntity);
                return new OkObjectResult(fileEntity);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : FileProvider.UploadFunction.cs :: {ex.Message}");
            return new StatusCodeResult(500);
        }

        return new BadRequestResult();
    }
}
