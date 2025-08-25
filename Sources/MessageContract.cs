namespace ChunkProcessingSystem
{
    public class MyMessage

    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int TotalSize { get; set; }
        public Type DataType { get; set; }
        public int UID { get; set; }
        public int ChunkAmount { get; set; }
        public int ChunkSize { get; set; }
        public List<DataChunk> Chunks { get; set; } = new();
    }
}