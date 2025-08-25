using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChunkProcessingSystem
{

    public class MyMessageConsumer : IConsumer<MyMessage>
    {
        private readonly List<MyMessage> _buffer = new();  

        private readonly DefaultJsonChunkProcessor<MyMessage> _processor;

        public MyMessageConsumer(DefaultJsonChunkProcessor<MyMessage> processor)
        {
            _processor = processor;
        }
        public Task Consume(ConsumeContext<MyMessage> context)
        {
            _buffer.Add(context.Message);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(context.Message);
            Console.WriteLine($"Buffered message {context.Message.Id} as JSON: {json}");

            if (_buffer.Count >= 5)
            {
                Console.WriteLine("Processing chunk...");
                _processor.ProcessChunks(_buffer);   
                _buffer.Clear();
            }

            return Task.CompletedTask;
        }
    }
    }
