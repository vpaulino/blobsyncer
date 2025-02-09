using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage.Channels.Writers
{
    public interface IChannelWriter<T>
    {
        Task FetchBlobPagesAsync(BlobContainerClient containerClient, ChannelWriter<T> writer, int pageSize);
    }
}