using ConnectHub.Media.API.Controllers;
using ConnectHub.Media.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConnectHub.Media.API.Tests;

[TestClass]
public class MediaEdgeTests
{
    private Mock<IBlobStorageService> _mockBlob = null!;
    private MediaController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockBlob = new Mock<IBlobStorageService>();
        _controller = new MediaController(_mockBlob.Object);
    }

    [TestMethod]
    public async Task Upload_HandlesVariousContentTypes()
    {
        var contentTypes = new[] { "image/png", "video/mp4", "application/pdf" };
        
        foreach (var contentType in contentTypes)
        {
            var file = CreateMockFile("test", contentType);
            _mockBlob.Setup(b => b.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), contentType))
                     .ReturnsAsync($"http://blob/{contentType}");

            var result = await _controller.Upload(file);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsTrue(ReadProperty<string>(okResult.Value!, "url").Contains(contentType));
        }
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task Upload_Throws_WhenBlobServiceFails()
    {
        var file = CreateMockFile("fail.txt", "text/plain");
        _mockBlob.Setup(b => b.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ThrowsAsync(new Exception("Upload failed"));

        await _controller.Upload(file);
    }

    private static IFormFile CreateMockFile(string fileName, string contentType)
    {
        var content = "fake content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static T ReadProperty<T>(object value, string propertyName)
    {
        return (T)value.GetType().GetProperty(propertyName)!.GetValue(value)!;
    }
}
