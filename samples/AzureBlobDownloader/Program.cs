namespace AzureBlobDownloader
{
    using BlobSyncer.Azure.BlobStorage;
    using BlobSyncer.Azure.BlobStorage.Channels._Readers;
    using BlobSyncer.Azure.BlobStorage.Channels._Readers.FileSystem;
    using BlobSyncer.Azure.BlobStorage.Channels.Readers;
    using CommandLine.ArgumentsParser;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class Program
    {
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
            using IHost host = Host.CreateDefaultBuilder(args)
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

           
            // Retrieve the logger factory (which gives you access to logging providers)
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<AzureBlobDownloader>();

            logger.LogInformation("Application has started.");
            
            // If you specifically want to fetch the logger providers, you can resolve them as follows:
            var loggerProviders = host.Services.GetServices<ILoggerProvider>();

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var sourceConnectionString = configuration.GetConnectionString("blobStorage");


            // Create the parser and parse the command-line arguments.
            var parser = new CommandLineParser();

            parser.AddParameter("--names", ParseNames)
                  .AddParameter("--destination", ParseDestination);

            var arguments = parser.Parse(args); 

              string[] containerNames = arguments.GetByName("names")?.Values; // Replace with your Blob Container Name
            string rootFolder = arguments.GetByName("destination").Values.ElementAt(0); // Replace with your desired local folder path

            List<Task> tasksExecuting = new List<Task>();

            

            AzureBlobDownloader azureBlobDownloader = new AzureBlobDownloader(sourceConnectionString, logger);

            IDestinationHandler destinationBlobHandler = new FileSystemDestinationHandler(rootFolder);

            foreach (var containername in containerNames)
            {
                tasksExecuting.Add(azureBlobDownloader.DownloadBlobsAsync(new DownloadSettings(containername, 10, 10), destinationBlobHandler));

            }

            await Task.WhenAll(tasksExecuting);

            Console.WriteLine("Download completed.");
        }
    }
}
