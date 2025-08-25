using System;
using System.Threading;
using System.Threading.Tasks;
using ChunkProcessingSystem;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Chunk API Demo ===");
        Console.WriteLine("Select source: (array / list / file / db / ws / http)");
        string source = Console.ReadLine()?.ToLower();

        var controller = new ChunkController();
        controller.chooseDesiredChunkSize();

        switch (source)
        {
            case "array":
                controller.splitCollectionArray(new[] { 1, 2, 3, 4, 5, 6, 7 });
                break;

            case "list":
                controller.splitCollectionList(new() { "Alice", "Bob", "Charlie", "David" });
                break;

            case "file":
                Console.WriteLine("Enter file path:");
                string path = Console.ReadLine();
                controller.SplitFileIntoChunks(path, 1024);
                break;

            case "db":
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection("YourConnectionStringHere"))
                {
                    var dbSource = new DatabaseChunkSource(conn, batchSize: 50);
                    await foreach (var msg in dbSource.GetChunksAsync())
                    {
                        Console.WriteLine($"DB Chunk: {msg.Id} - {msg.Content}");
                    }
                }
                break;

            case "ws":
                var wsSource = new WebSocketChunkSource("wss://echo.websocket.events");
                await foreach (var chunk in wsSource.GetChunksAsync(CancellationToken.None))
                {
                    Console.WriteLine($"WebSocket: {chunk}");
                }
                break;

            case "http":
                var httpSource = new HTTPStreamProcessor();
                await httpSource.ProcessHTTPStreamAsync("https://stream.meetup.com/2/rsvps");
                break;

            default:
                Console.WriteLine("Unknown source.");
                break;
        }

        Console.WriteLine("=== Done ===");
    }
}
