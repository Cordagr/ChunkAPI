using Microsoft.Identity.Client; // <-- This using is not needed here, safe to remove
using System.Buffers.Binary;
using System.Text; 
namespace ChunkProcessingSystem
{
    public class DataChunk
    {
        public int Id { get; set; }   // chunk index
        public string Name { get; set; }
        public byte[] Payload { get; set; }  // the actual chunk bytes

        public void SetPayload(int data)
        {
            // allocate 4 bytes for an int
            Payload = new byte[4];

            // store int in little-endian format
            BinaryPrimitives.WriteInt32LittleEndian(Payload, data);
        }

        public int GetPayloadAsInt()
        {
            if (Payload == null || Payload.Length < 4)
                throw new InvalidOperationException("Payload is not a valid int.");

            return BinaryPrimitives.ReadInt32LittleEndian(Payload);
        }
        public void SetPayloadFromInt(int data)
        {
            Payload = BitConverter.GetBytes(data);
        }

        public void SetPayloadFromString(string text)
        {
            Payload = Encoding.UTF8.GetBytes(text);
        }

    }
}
