using System;
using System.Collections.Generic;
using System.Timers;
using RingBuffer;

public class TimeBasedChunkingStrategy<TChunk> where TChunk : class
{
    private readonly System.Timers.Timer _timer;
    private readonly int _chunkDurationMs;
    private readonly BufferWithQueue<TChunk> _buffer;
    private readonly object _lock = new();

    public event Action<List<TChunk>>? ChunkReady;

    public TimeBasedChunkingStrategy(int chunkDurationMs, int bufferCapacity = 100)
    {
        _chunkDurationMs = chunkDurationMs;
        _buffer = new BufferWithQueue<TChunk>(bufferCapacity);

        _timer = new System.Timers.Timer(_chunkDurationMs);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = false; // we control restarts manually
    }

    private void ResetTimer()
    {
        _timer.Stop();
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        List<TChunk> chunk;
        lock (_lock)
        {
            if (_buffer.Count == 0) return; // no data, do nothing

            chunk = new List<TChunk>(_buffer);
            _buffer.Clear();
        }

        Console.WriteLine($"Timer expired at {e.SignalTime}, flushing {chunk.Count} items.");
        ChunkReady?.Invoke(chunk);
    }

    public void InsertData(TChunk item, int maxChunksBeforeSend = int.MaxValue)
    {
        List<TChunk>? chunk = null;

        lock (_lock)
        {
            if (_buffer.Count == 0)
                ResetTimer();

            _buffer.Insert(item);

            if (_buffer.Count >= maxChunksBeforeSend)
            {
                chunk = new List<TChunk>(_buffer);
                _buffer.Clear();
                _timer.Stop(); // stop timer since we already flushed
            }
        }

        if (chunk != null)
        {
            Console.WriteLine($"Buffer full, flushing {chunk.Count} items early.");
            ChunkReady?.Invoke(chunk);
        }
    }
}
