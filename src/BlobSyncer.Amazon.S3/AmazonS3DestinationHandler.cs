using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobSyncer.Azure.BlobStorage.Channels.Readers;
using Microsoft.Extensions.Logging;

namespace BlobSyncer.Amazon.S3
{
    public class AmazonS3DestinationHandler : IDestinationHandler
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;
        ILogger<AmazonS3DestinationHandler> logger;
        public AmazonS3DestinationHandler(string key, string secret, string bucketName, ILogger<AmazonS3DestinationHandler> logger)
        {   
            _s3Client = new AmazonS3Client(key, secret);
            _bucketName = bucketName;
            this.logger = logger;
        }

        public async Task WriteBlobAsync(BlobContainerClient blobSourceContainerClient, BlobItem blobItem)
        {
            BlobClient sourceBlobClient = blobSourceContainerClient.GetBlobClient(blobItem.Name);
            using (var stream = await sourceBlobClient.OpenReadAsync())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = blobItem.Name,
                    InputStream = stream
                };
                await _s3Client.PutObjectAsync(putRequest);
            }
        }
    }

}
