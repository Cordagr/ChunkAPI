using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Buffers.Binary;

namespace ChunkProcessingSystem
{
    public enum Filters
    {
        Keyword,
        StreamContentType,
        JsonFields,
        TimeFrame,
    }

    public class ChunkController
    {
        public string Strategy = "";
        public int chunkSize = 0;
        public Filters[] selectedFilters = Array.Empty<Filters>();
        public DefaultJsonChunkProcessor<DataChunk> processor = new DefaultJsonChunkProcessor<DataChunk>();

        public void selectFilter(Filters selectedFilter)
        {
            Array.Resize(ref selectedFilters, selectedFilters.Length + 1);
            selectedFilters[selectedFilters.Length - 1] = selectedFilter;
        }

        public void chooseStrategy(string inputStrategy)
        {
            Strategy = inputStrategy;
        }

        public void chooseDesiredChunkSize()
        {
            Console.WriteLine("Please enter a desired chunk size (Requirement: <= 15)");
            int chosenChunkSize = Convert.ToInt32(Console.ReadLine());
            if (chosenChunkSize >= 15)
            {
                Console.WriteLine("Please choose a smaller chunk size");
            }
            else
            {
                chunkSize = chosenChunkSize;
            }
        }

        public void splitCollectionArray(int[] array)
        {
            var chunks = array.Chunk(chunkSize);

            foreach (var chunk in chunks)
            {
                Console.WriteLine(string.Join(", ", chunk));

                byte[] intBytes = BitConverter.GetBytes(123);
                int actual = BinaryPrimitives.ReadInt32LittleEndian(intBytes);

                var dataChunk = new DataChunk
                {
                    Name = "IntArrayChunk",
                    // serialize ints properly into bytes
                    Payload = chunk.SelectMany(i => BitConverter.GetBytes(i)).ToArray()
                };

                processor.ProcessChunk(dataChunk);
            }
        }

        public void splitCollectionList(List<string> names)
        {
            var nameChunks = names.Chunk(chunkSize);

            foreach (var chunk in nameChunks)
            {
                Console.WriteLine(string.Join(", ", chunk));

                var dataChunk = new DataChunk
                {
                    Name = "StringChunk",
                    // convert string chunk into UTF8 byte array
                    Payload = System.Text.Encoding.UTF8.GetBytes(string.Join(",", chunk))
                };

                processor.ProcessChunk(dataChunk);
            }
        }

        public void SplitFileIntoChunks(string filePath, int chunkSizeInBytes)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                char[] buffer = new char[chunkSizeInBytes];
                int charsRead;
                int chunkNumber = 0;

                while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string chunkText = new string(buffer, 0, charsRead);
                    string outputFileName = $"chunk_{chunkNumber}.txt";

                    File.WriteAllText(outputFileName, chunkText);

                    var dataChunk = new DataChunk
                    {
                        Name = $"FileChunk_{chunkNumber}",
                        Payload = System.Text.Encoding.UTF8.GetBytes(chunkText)
                    };

                    processor.ProcessChunk(dataChunk);

                    Console.WriteLine($"Wrote {outputFileName} with {charsRead} characters.");
                    chunkNumber++;
                }
            }
        }
    }
}
