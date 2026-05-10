using System;
using ConnectHub.Media.Application.Services;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace ConnectHub.Media.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        var containerName = configuration["AzureStorage:ContainerName"];
        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException("Azure storage configuration is missing.");
        }
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name must be provided", nameof(fileName));

        await _containerClient.CreateIfNotExistsAsync();

        var blobClient = _containerClient.GetBlobClient(Guid.NewGuid().ToString() + "_" + fileName);
        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

        if (!blobClient.CanGenerateSasUri)
        {
            return blobClient.Uri.ToString();
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }
}
