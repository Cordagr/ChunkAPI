## Chunk API
This project is a C# console application for splitting data into manageable chunks from different sources.
## Features
- File Chunking
-- Splits any file to a specified chunk size 
-- Each chunk can be processed and stored separately. 
-- Developers can easily extend with custom sources

## Installation
1. git clone https://github.com/Cordagr/ChunkAPI
2. Build the project using .NET cli (dotnet build)
3. Run the cli (dotnet run) 

## Usage
1. File Mode (split file to chunks)
``` csharp
case "file":
    Console.WriteLine("Enter file path:");
    string path = Console.ReadLine();
    controller.SplitFileIntoChunks(path, 1024); // chunk size in bytes
    break;
```
Example Run:
``` yaml
Choose mode: file/db
file
Enter file path:
README.md
```

2. Database Mode
``` csharp
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
``` 
- Update the connection string as needed

3. Http Usage
``` csharp
case "http":
    var httpSource = new HttpChunkSource("https://jsonplaceholder.typicode.com/comments", batchSize: 20);
    await foreach (var msg in httpSource.GetChunksAsync())
    {
        Console.WriteLine($"HTTP Chunk: {msg.Id} - {msg.Content}");
    }
    break;
```
- Update the wanted http address as warranted 

4. Array Usage
``` csharp
case "array":
    controller.splitCollectionArray(new[] { 1, 2, 3, 4, 5, 6, 7 });
    break;
```
5. List Usage
``` csharp
case "list":
    controller.splitCollectionList(new List<string> { "Alice", "Bob", "Charlie", "David" });
    break;
```




