using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage.Channels.Readers
{
    public interface IChannelReader<T>
    {
        Task ProcessBlobPagesAsync(BlobContainerClient containerClient, ChannelReader<T> reader, IDestinationHandler downloadHandler);
    }
}