using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net.WebSockets;
using System.Net.Http;
using System.Data; 
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using MassTransit; 

namespace ChunkProcessingSystem
{
    // Interface to define async chunk data sources
    public interface IAsyncChunkSource<T>
    {
        IAsyncEnumerable<T> GetChunksAsync(CancellationToken cancellationToken = default);
    }

    // Database chunk source //  
    public class DatabaseChunkSource : IAsyncChunkSource<MyMessage>
    {
        private readonly IDbConnection _connection;
        private readonly int _batchSize;

        public DatabaseChunkSource(IDbConnection connection, int batchSize = 100)
        {
            // connection string // 
            // Example: _connection = new SQLiteAsyncConnection(SQLitePath)
            _connection = connection;
            _batchSize = batchSize;
        }

        public async IAsyncEnumerable<MyMessage> GetChunksAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int offset = 0;
            while (true)
            {
                var results = await _connection.QueryAsync<MyMessage>(
                    "SELECT * FROM Messages ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @BatchSize ROWS ONLY",
                    new { Offset = offset, BatchSize = _batchSize });

                var batch = results.ToList();
                if (!batch.Any())
                    yield break;

                foreach (var message in batch)
                {
                    yield return message;
                }

                offset += _batchSize;
            }
        }
    }

    // WebSocket-based chunk source
    public class WebSocketChunkSource : IAsyncChunkSource<string>
    {
        private readonly Uri _uri;

        public WebSocketChunkSource(string wsUrl)
        {
            _uri = new Uri(wsUrl);
        }
        public async IAsyncEnumerable<string> GetChunksAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var client = new ClientWebSocket();
            await client.ConnectAsync(_uri, cancellationToken);

            // Send a message after connecting
            var helloBytes = Encoding.UTF8.GetBytes("Hello from client!+");
            var NoBytes = Encoding.UTF8.GetBytes("NO! NO! ");
            await client.SendAsync(new ArraySegment<byte>(helloBytes), WebSocketMessageType.Text, true, cancellationToken);

            var buffer = new byte[4096];

            while (!cancellationToken.IsCancellationRequested && client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", cancellationToken);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                yield return message;
            }
        }
    }

    // TODO: Update interface
    public interface IConsumer<MyMessage>
    {

    }


    // Byte processor that reads from a FileStream
    public class ByteProcessor
    {
        private const int BufferSize = 100;
        private readonly Stream fileStreamDeviceData;

        public ByteProcessor(Stream stream)
        {
            fileStreamDeviceData = stream;
        }

        public async Task<byte[]> ReadByteAsync()
        {
            using var memoryStream = new MemoryStream();
            var buffer = new byte[BufferSize];
            int bytesRead;

            while ((bytesRead = await fileStreamDeviceData.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
            }


            return memoryStream.ToArray();
        }
    }

    public class HTTPStreamProcessor
    {
        public async Task<bool> ProcessHTTPStreamAsync(string apiUrl, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(responseStream);

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    Console.WriteLine($"Received: {line}");
                }

                return true;
            }

            return false;
        }
    }
}
