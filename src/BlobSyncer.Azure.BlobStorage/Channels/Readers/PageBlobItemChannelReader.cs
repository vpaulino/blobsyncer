using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage.Channels.Readers
{
    public class PageBlobItemChannelReader : IChannelReader<Page<BlobItem>>
    {
        public async Task ProcessBlobPagesAsync(BlobContainerClient containerClient, ChannelReader<Page<BlobItem>> reader, IDestinationHandler downloadHandler)
        {
            await foreach (var page in reader.ReadAllAsync())
            {
                Console.WriteLine($"Processing {page.Values.Count} blobs...");

                var downloadTasks = new List<Task>();

                foreach (BlobItem blobItem in page.Values)
                {

                    Task writeHandlerTask = downloadHandler.WriteBlobAsync(containerClient, blobItem);
                    downloadTasks.Add(writeHandlerTask);
                }

                await Task.WhenAll(downloadTasks); // Parallel download within the batch
            }
        }

    }
}
