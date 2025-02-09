namespace AzureBlobDownloader
{
    using BlobSyncer.Amazon.S3;
    using BlobSyncer.Azure.AISearch;
    using BlobSyncer.Azure.BlobStorage;
    using BlobSyncer.Azure.BlobStorage.Channels._Readers;
    using BlobSyncer.Azure.BlobStorage.Channels._Readers.FileSystem;
    using BlobSyncer.Azure.BlobStorage.Channels.Readers;
    using BlobSyncer.Azure.BlobStorage.Channels.Readers.AzureBlob;
    using CommandLine.ArgumentsParser;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    internal class Program
    {
        /// <summary>
        /// Handler for the <c>--names</c> parameter.
        /// Expects the next argument to be a comma-separated list of names.
        /// </summary>
        public static Argument ParseApiKey(string[] args, ref int index)
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Missing value for --names parameter.");

            // Advance to the value.
            index++;
            string namesArg = args[index];
            string[] values = namesArg
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(name => name.Trim())
                                .ToArray();

            return new Argument("apiKey", values);
        }

        /// <summary>
        /// Handler for the <c>--names</c> parameter.
        /// Expects the next argument to be a comma-separated list of names.
        /// </summary>
        public static Argument ParseModel(string[] args, ref int index)
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Missing value for --names parameter.");

            // Advance to the value.
            index++;
            string namesArg = args[index];
            string[] values = namesArg
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(name => name.Trim())
                                .ToArray();

            return new Argument("modelName", values);
        }


        /// <summary>
        /// Handler for the <c>--names</c> parameter.
        /// Expects the next argument to be a comma-separated list of names.
        /// </summary>
        public static Argument ParseNames(string[] args, ref int index)
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Missing value for --names parameter.");

            // Advance to the value.
            index++;
            string namesArg = args[index];
            string[] values = namesArg
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(name => name.Trim())
                                .ToArray();

            return new Argument("names", values);
        }

        /// <summary>
        /// Handler for the <c>destination</c> parameter.
        /// Expects the next argument to be a URI.
        /// </summary>
        public static Argument ParseDestination(string[] args, ref int index)
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Missing value for destination parameter.");

            // Advance to the value.
            index++;
            string destinationValue = args[index];
            return new Argument("destination", new[] { destinationValue });
        }

        static async Task Main(string[] args)
        {
            // Build the host using the default builder.
            IHost host;
            ILogger<AzureBlobDownloader> logger;
            string? sourceConnectionString, destinationConnectionString, rootFolder;
            string[] containerNames;
            List<Task> tasksExecuting;

            Setup(args, out host, out sourceConnectionString, out destinationConnectionString, out tasksExecuting);

            ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            // Create the parser and parse the command-line arguments.
            TransferToFileSystem(args, sourceConnectionString, out containerNames, tasksExecuting, loggerFactory);

            TransferToBlobStorage(loggerFactory, sourceConnectionString, destinationConnectionString, containerNames, tasksExecuting);

            TransferToAISearch(args, sourceConnectionString, containerNames, tasksExecuting, loggerFactory);

            TransferToAmazonS3(args, sourceConnectionString, containerNames, tasksExecuting, loggerFactory);

            await Task.WhenAll(tasksExecuting);

            Console.WriteLine("Download completed.");
        }

        private static void TransferToFileSystem(string[] args, string? sourceConnectionString, out string[] containerNames, List<Task> tasksExecuting, ILoggerFactory loggerFactory)
        {
            var parser = new CommandLineParser();

            parser.AddParameter("--names", ParseNames)
                  .AddParameter("--destination", ParseDestination);

            var arguments = parser.Parse(args);

            containerNames = arguments.GetByName("names")?.Values;
            var rootFolder = arguments.GetByName("destination").Values.ElementAt(0);

            TransferToFileSystem(loggerFactory, sourceConnectionString, rootFolder, containerNames, tasksExecuting);
        }

        private static void TransferToAISearch(string[] args, string? sourceConnectionString, string[] containerNames, List<Task> tasksExecuting, ILoggerFactory loggerFactory)
        {
            CommandLineParser parser;
            Arguments arguments;


            // Create the parser and parse the command-line arguments.
            parser = new CommandLineParser();
            parser.AddParameter("--names", ParseNames)
                  .AddParameter("--destination", ParseDestination)
                  .AddParameter("--apiKey", ParseApiKey)
                   .AddParameter("--modelName", ParseModel);

            arguments = parser.Parse(args);
            var searchEndpoint = arguments.GetByName("destination").Values.ElementAt(0);
            var indexName = arguments.GetByName("names").Values.ElementAt(0);
            var apiKey = arguments.GetByName("apiKey").Values.ElementAt(0);
            var modelName = arguments.GetByName("modelName").Values.ElementAt(0);



            TransferToAISearch(loggerFactory, sourceConnectionString, searchEndpoint, containerNames, indexName, apiKey, modelName, tasksExecuting);
        }

        private static void TransferToAmazonS3(string[] args, string? sourceConnectionString, string[] containerNames, List<Task> tasksExecuting, ILoggerFactory loggerFactory)
        {
            var parser = new CommandLineParser();

            parser.AddParameter("--names", ParseNames)
                  .AddParameter("--accessKey", ParseDestination)
                  .AddParameter("--accessSecret", ParseApiKey)
                   .AddParameter("--destinationBucket", ParseModel);

            var arguments = parser.Parse(args);

            var accessKey = arguments.GetByName("accessKey").Values.ElementAt(0);
            var accessSecret = arguments.GetByName("accessSecret").Values.ElementAt(0);
            var destinationBucket = arguments.GetByName("destinationBucket").Values.ElementAt(0);


            TransferToAmazonS3(loggerFactory, sourceConnectionString, accessKey, accessSecret, destinationBucket, containerNames, tasksExecuting);
        }

        private static void TransferToFileSystem(ILoggerFactory loggerFactory, string? sourceConnectionString, string? rootFolder, string[] containerNames, List<Task> tasksExecuting)
        {
            ILogger<AzureBlobDownloader> logger = loggerFactory.CreateLogger<AzureBlobDownloader>();

            AzureBlobDownloader azureBlobDownloader = new AzureBlobDownloader(sourceConnectionString, logger);

            IDestinationHandler destinationBlobHandler = new FileSystemDestinationHandler(rootFolder);

            foreach (var containername in containerNames)
            {
                tasksExecuting.Add(azureBlobDownloader.DownloadBlobsAsync(new DownloadSettings(containername, 10, 10), destinationBlobHandler));

            }
        }

        private static void TransferToAISearch(ILoggerFactory loggerFactory, string? sourceConnectionString, string? destinationConnectionString,string[] containerNames, string indexName,string apiKey,string modelName, List<Task> tasksExecuting)
        {
            // Retrieve the logger factory (which gives you access to logging providers)

            var loggerblob = loggerFactory.CreateLogger<AzureBlobDestinationHandler>();
            var azureBlobDownloaderLogger = loggerFactory.CreateLogger<AzureBlobDownloader>();


            AzureBlobDownloader azureBlobDownloader = new AzureBlobDownloader(sourceConnectionString, azureBlobDownloaderLogger);

            IDestinationHandler destinationHandler = new AzureAISearchDestinationHandler(destinationConnectionString, indexName, apiKey, modelName);

            foreach (var containername in containerNames)
            {
                tasksExecuting.Add(azureBlobDownloader.DownloadBlobsAsync(new DownloadSettings(containername, 10, 10), destinationHandler));

            }
        }

        private static void TransferToBlobStorage(ILoggerFactory loggerFactory, string? sourceConnectionString, string? destinationConnectionString, string[] containerNames, List<Task> tasksExecuting)
        {
            // Retrieve the logger factory (which gives you access to logging providers)
          
           var loggerblob = loggerFactory.CreateLogger<AzureBlobDestinationHandler>();
            var azureBlobDownloaderLogger = loggerFactory.CreateLogger<AzureBlobDownloader>();
       

            AzureBlobDownloader azureBlobDownloader = new AzureBlobDownloader(sourceConnectionString, azureBlobDownloaderLogger);

            IDestinationHandler destinationBlobHandler = new AzureBlobDestinationHandler(destinationConnectionString, loggerblob);

            foreach (var containername in containerNames)
            {
                tasksExecuting.Add(azureBlobDownloader.DownloadBlobsAsync(new DownloadSettings(containername, 10, 10), destinationBlobHandler));

            }
        }



        private static void TransferToAmazonS3(ILoggerFactory loggerFactory, string? sourceConnectionString,string accessKey,string accessSecret,  string? destinationBucket, string[] containerNames, List<Task> tasksExecuting)
        {
          

           
            var azureBlobDownloaderLogger = loggerFactory.CreateLogger<AzureBlobDownloader>();
            var loggerblob = loggerFactory.CreateLogger<AmazonS3DestinationHandler>();

            AzureBlobDownloader azureBlobDownloader = new AzureBlobDownloader(sourceConnectionString, azureBlobDownloaderLogger);

            IDestinationHandler destinationBlobHandler = new AmazonS3DestinationHandler(accessKey, accessSecret, destinationBucket, loggerblob);

            foreach (var containername in containerNames)
            {
                tasksExecuting.Add(azureBlobDownloader.DownloadBlobsAsync(new DownloadSettings(containername, 10, 10), destinationBlobHandler));

            }
        }


        private static void Setup(string[] args, out IHost host, out string? sourceConnectionString, out string? destinationConnectionString, out List<Task> tasksExecuting)
        {
            host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    // Load configuration sources
                    config
                        .SetBasePath(AppContext.BaseDirectory) // Optional: Set base path
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddUserSecrets<Program>() // ✅ Enable User Secrets
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .Build();


          

            // If you specifically want to fetch the logger providers, you can resolve them as follows:
            var loggerProviders = host.Services.GetServices<ILoggerProvider>();

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            sourceConnectionString = configuration.GetConnectionString("sourceBlobStorage");
            destinationConnectionString = configuration.GetConnectionString("destinationBlobStorage");
             
            
            tasksExecuting = new List<Task>();

        }
    }
}
