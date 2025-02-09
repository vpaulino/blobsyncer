using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage.Channels.Readers.AzureBlob
{
 

    public class AzureBlobDestinationHandler : IDestinationHandler
    {
        private readonly string _destinationConnectionString;
        ILogger<AzureBlobDestinationHandler> logger;
        public AzureBlobDestinationHandler(string destinationConnectionString, ILogger<AzureBlobDestinationHandler> logger)
        {
            _destinationConnectionString = destinationConnectionString ?? throw new ArgumentNullException(nameof(destinationConnectionString));
            this.logger = logger;
        }

        public async Task WriteBlobAsync(BlobContainerClient blobSourceContainerClient, BlobItem blobItem)
        {
            if (blobSourceContainerClient == null) throw new ArgumentNullException(nameof(blobSourceContainerClient));
            if (blobItem == null) throw new ArgumentNullException(nameof(blobItem));

            string containerName = blobSourceContainerClient.Name;  // ✅ Keep the same container name
            string blobName = blobItem.Name;  // ✅ Keep the same blob name

            // Create source blob client
            BlobClient sourceBlobClient = blobSourceContainerClient.GetBlobClient(blobName);

            // Get destination container client
            BlobContainerClient destinationContainerClient = new BlobContainerClient(_destinationConnectionString, containerName);
            await destinationContainerClient.CreateIfNotExistsAsync(PublicAccessType.None); // Ensure container exists

            // Create destination blob client
            BlobClient destinationBlobClient = destinationContainerClient.GetBlobClient(blobName);

            // ✅ Copy blob from source to destination
            using (var stream = await sourceBlobClient.OpenReadAsync())
            {
                await destinationBlobClient.UploadAsync(stream, overwrite: true);
            }

            this.logger.LogDebug($"Blob {blobName} Uploaded to account {destinationContainerClient.AccountName} .... ");
            
        }
    }

}
