using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage.Channels.Writers
{
    public class PageBlobItemChannelWriter : IChannelWriter<Page<BlobItem>>
    {
        public async Task FetchBlobPagesAsync(BlobContainerClient containerClient, ChannelWriter<Page<BlobItem>> writer, int pageSize)
        {
            try
            {
                await foreach (var page in containerClient.GetBlobsAsync().AsPages(pageSizeHint: pageSize))
                {
                    await writer.WriteAsync(page); // Push page to channel
                }
            }
            finally
            {
                writer.Complete(); // Signal that there are no more pages
            }
        }

    }
}
