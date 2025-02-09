using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobSyncer.Azure.BlobStorage.Channels.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage.Channels._Readers.FileSystem
{
    public class FileSystemDestinationHandler : IDestinationHandler
    {
        private string rootFolder;

        public FileSystemDestinationHandler(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }
        private string SanitizeFileName(ReadOnlySpan<char> fileName)
        {
            if (fileName.IsEmpty)
                throw new ArgumentException("Filename cannot be null or empty.", nameof(fileName));

            // Get invalid filename characters
            ReadOnlySpan<char> invalidChars = Path.GetInvalidFileNameChars();

            // Allocate a buffer on the stack (Span<char>) for the output filename
            Span<char> buffer = stackalloc char[fileName.Length];
            int bufferIndex = 0;

            // Iterate through the input filename and replace invalid characters
            foreach (char c in fileName)
            {

                // Replace invalid characters and spaces with '_'
                buffer[bufferIndex++] = invalidChars.Contains(c) || c == ' ' ? '_' : c;
            }

            // Create the final string without extra heap allocations
            return new string(buffer.Slice(0, bufferIndex));
        }

        /// <summary>
        /// Downloads a single blob.
        /// </summary>
        private async Task DownloadBlobAsync(BlobContainerClient containerClient, string blobName, string filePath)
        {

            // Ensure directory exists
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Console.WriteLine($"Downloading: {blobName} to {filePath}...");
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DownloadToAsync(filePath);
        }

        public Task WriteBlobAsync(BlobContainerClient blobSourceContainerClient, BlobItem blobItem)
        {
            string sanitizedFileName = SanitizeFileName(blobItem.Name);
            string localFilePath = Path.Combine(rootFolder + blobSourceContainerClient.Name, sanitizedFileName);

            return DownloadBlobAsync(blobSourceContainerClient, blobItem.Name, localFilePath);


        }
    }
}
