using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobSyncer.Azure.BlobStorage.Channels.Readers;
using BlobSyncer.Azure.BlobStorage.Channels.Writers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlobSyncer.Azure.BlobStorage
{

    public class PageBlobProcessPipeline 
    {
        private readonly PageBlobItemChannelWriter ChannelWriter;

        private readonly PageBlobItemChannelReader ChannelReader;


        public PageBlobProcessPipeline(PageBlobItemChannelWriter blobWriter, PageBlobItemChannelReader blobReader)
        {
            this.ChannelWriter = blobWriter ;
            this.ChannelReader = blobReader;
        }

        public async Task ProcessAsync(BlobContainerClient containerClient, IDestinationHandler destinationHandler,  int pageSize, int parallelTasks) 
        {

            var channel = Channel.CreateBounded<Page<BlobItem>>(capacity: parallelTasks * 2);

            // Start producer (fetch pages and enqueue them)
            var producerTask = Task.Run(() => ChannelWriter.FetchBlobPagesAsync(containerClient, channel.Writer, pageSize));

            // Start consumers (dequeue pages and process them)
            var consumerTasks = new List<Task>();
            for (int i = 0; i < parallelTasks; i++)
            {
                consumerTasks.Add(Task.Run(() => ChannelReader.ProcessBlobPagesAsync(containerClient, channel.Reader, destinationHandler)));
            }

            // Wait for all tasks to complete
            await producerTask;
            await Task.WhenAll(consumerTasks);

        }

    }

    public class AzureBlobDownloader
    {
        BlobServiceClient blobServiceClient;
        ILogger<AzureBlobDownloader> logger;
        public AzureBlobDownloader(string connectionString,  ILogger<AzureBlobDownloader> logger)
        {
            blobServiceClient = new BlobServiceClient(connectionString);
            this.logger = logger;
        } 

        public virtual async Task DownloadBlobsAsync(DownloadSettings downloadSettings, IDestinationHandler destinationBlobHandler)
        {
          
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(downloadSettings.SourceLocationName);

            logger.LogDebug($"Fetching blobs in pages of {downloadSettings.PageSize} with {downloadSettings.ParallelTasks} parallel consumers...");

            PageBlobProcessPipeline channelPipeline = new PageBlobProcessPipeline(new PageBlobItemChannelWriter(), new PageBlobItemChannelReader());

            await channelPipeline.ProcessAsync(containerClient, destinationBlobHandler, downloadSettings.PageSize, downloadSettings.ParallelTasks);

            logger.LogDebug("Download Complete!");
        }
         
    }
}
