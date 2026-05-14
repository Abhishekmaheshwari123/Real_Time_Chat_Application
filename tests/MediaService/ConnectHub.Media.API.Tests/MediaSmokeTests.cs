using ConnectHub.Media.API.Controllers;
using ConnectHub.Media.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectHub.Media.API.Tests;

[TestClass]
public class MediaControllerTests
{
    [TestMethod]
    public async Task Upload_ReturnsBadRequest_WhenFileIsNull()
    {
        var fakeBlobStorage = new FakeBlobStorageService();
        var controller = new MediaController(fakeBlobStorage);

        var result = await controller.Upload(null!);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual("No file uploaded.", badRequest.Value);
    }

    [TestMethod]
    public async Task Upload_ReturnsBadRequest_WhenFileIsEmpty()
    {
        var fakeBlobStorage = new FakeBlobStorageService();
        var controller = new MediaController(fakeBlobStorage);
        using var stream = new MemoryStream(Array.Empty<byte>());
        IFormFile file = new FormFile(stream, 0, 0, "file", "empty.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var result = await controller.Upload(file);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual("No file uploaded.", badRequest.Value);
    }

    [TestMethod]
    public async Task Upload_ReturnsUrl_WhenUploadSucceeds()
    {
        var fakeBlobStorage = new FakeBlobStorageService
        {
            UploadResult = "https://blob.local/file.png"
        };
        var controller = new MediaController(fakeBlobStorage);
        var payload = new byte[] { 1, 2, 3, 4 };
        using var stream = new MemoryStream(payload);
        IFormFile file = new FormFile(stream, 0, payload.Length, "file", "file.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var result = await controller.Upload(file);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual("https://blob.local/file.png", ReadProperty<string>(okResult.Value!, "url"));
        Assert.AreEqual(1, fakeBlobStorage.CallCount);
        Assert.AreEqual("file.png", fakeBlobStorage.LastFileName);
        Assert.AreEqual("image/png", fakeBlobStorage.LastContentType);
        Assert.AreEqual(payload.Length, fakeBlobStorage.LastStreamLength);
    }

    private static T ReadProperty<T>(object value, string propertyName)
    {
        return (T)value.GetType().GetProperty(propertyName)!.GetValue(value)!;
    }

    private sealed class FakeBlobStorageService : IBlobStorageService
    {
        public int CallCount { get; private set; }
        public string LastFileName { get; private set; } = string.Empty;
        public string LastContentType { get; private set; } = string.Empty;
        public long LastStreamLength { get; private set; }
        public string UploadResult { get; set; } = "https://blob.local/default";

        public Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            CallCount++;
            LastFileName = fileName;
            LastContentType = contentType;
            LastStreamLength = fileStream.Length;

            return Task.FromResult(UploadResult);
        }
    }
}