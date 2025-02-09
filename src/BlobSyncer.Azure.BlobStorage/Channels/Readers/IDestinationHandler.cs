using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage.Channels.Readers
{
    public interface IDestinationHandler
    {
        Task WriteBlobAsync(BlobContainerClient blobSourceContainerClient, BlobItem blobItem);
    }
}