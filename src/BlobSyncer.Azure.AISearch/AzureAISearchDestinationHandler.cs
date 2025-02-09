using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using BlobSyncer.Azure.BlobStorage.Channels.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Search.Documents;
using OpenAI;

namespace BlobSyncer.Azure.AISearch
{
    public class AzureAISearchDestinationHandler : IDestinationHandler
    {
        private readonly SearchClient _searchClient;
        private readonly OpenAIClient _openAIClient;
        private string modelName;

        public AzureAISearchDestinationHandler(string searchServiceEndpoint, string indexName, string openAiApiKey, string modelName)
        {
            var credential = new AzureKeyCredential(openAiApiKey);
            _searchClient = new SearchClient(new Uri(searchServiceEndpoint), indexName, credential);
            _openAIClient = new OpenAIClient(openAiApiKey);
            this.modelName = modelName;
        }

        public async Task WriteBlobAsync(BlobContainerClient blobSourceContainerClient, BlobItem blobItem)
        {
            BlobClient blobClient = blobSourceContainerClient.GetBlobClient(blobItem.Name);

            using (var stream = await blobClient.OpenReadAsync())
            using (var reader = new StreamReader(stream))
            {
                string content = await reader.ReadToEndAsync();

                // 🔹 Generate Embeddings using OpenAI (or Azure AI)
                var embeddingsClient =  _openAIClient.GetEmbeddingClient(modelName);

                var embeddings = await embeddingsClient.GenerateEmbeddingsAsync(new string[] { content });

                // 🔹 Store in Azure AI Search
                var doc = new { Id = blobItem.Name, Content = content, Embeddings = embeddings };
                await _searchClient.UploadDocumentsAsync(new[] { doc });
            }
        }
    }

}
