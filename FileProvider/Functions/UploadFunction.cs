using Data.Contexts;
using Data.Entities;
using FileProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProvider.Functions;

public class UploadFunction
{
    private readonly ILogger<UploadFunction> _logger;
    private readonly FileService _fileService;

    public UploadFunction(ILogger<UploadFunction> logger, FileService fileService)
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
