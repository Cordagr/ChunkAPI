using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAsyncChunkSource<T>
{
    IAsyncEnumerable<T> GetChunksAsync(CancellationToken cancellationToken = default);


    
}
