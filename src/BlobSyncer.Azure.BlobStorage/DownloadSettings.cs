namespace BlobSyncer.Azure.BlobStorage
{
    public class DownloadSettings (string sourceLocationName, int parallelTasks, int pageSize)
    {
        public string SourceLocationName { get; set; } = sourceLocationName;

        public int ParallelTasks { get; init; } = parallelTasks;

        public int PageSize { get; init; } = pageSize;
    }
}
