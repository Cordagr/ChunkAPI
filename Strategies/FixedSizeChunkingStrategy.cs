public class FixedSizeChunkingStrategy<TChunk> where TChunk : class
{
    private readonly int _chunkSize;

    public FixedSizeChunkingStrategy(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public IEnumerable<IEnumerable<TChunk>> Chunk(IEnumerable<TChunk> items)
    {
        var chunk = new List<TChunk>();
        foreach (var item in items)
        {
            chunk.Add(item);
            if (chunk.Count == _chunkSize)
            {
                yield return new List<TChunk>(chunk);
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
        {
            yield return new List<TChunk>(chunk);
        }
    }
}
