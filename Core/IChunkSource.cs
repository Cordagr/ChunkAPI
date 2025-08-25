namespace Core;
using System.Collections.Generic;

// json // 
public class IChunkSourceJson<TChunk> where TChunk : class
{
    private int chunkID { get; set; }
    private string documentID { get; set; }
    private string pageNumber { get; set; }
    private string section_heading { get; set; }

    private string section_text { get; set; }

    private string start_index { get; set; }
    private string end_index{ get; set; }

    public virtual IEnumerable<TChunk> getChunks()
    {
        yield break;
    }
}

public class IChunkSourceBinary<TChunk> where TChunk : class
{
    private int chunkID { get; set; }
    public int start_time_ms { get; set; }
    public int end_time_ms { get; set; }
    private string format { get; set; }
    private string data { get; set; }
}

