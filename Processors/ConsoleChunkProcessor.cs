using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChunkProcessingSystem
{
    public class DefaultJsonChunkProcessor<TChunk> where TChunk : class
    {
        public List<TChunk> ProcessedChunks { get; } = new();

        public virtual void ProcessChunk(TChunk chunk)
        {
            if (chunk is string chunkString)
            {
                var deserialized = JsonConvert.DeserializeObject<TChunk>(chunkString);
                if (deserialized != null)
                    ProcessedChunks.Add(deserialized);
                else
                    Console.WriteLine("Failed to deserialize chunk.");
            }
            else
            {
                ProcessedChunks.Add(chunk);
            }
        }

        public virtual void ProcessChunks(IEnumerable<TChunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                ProcessChunk(chunk);
            }
        }
    }
}