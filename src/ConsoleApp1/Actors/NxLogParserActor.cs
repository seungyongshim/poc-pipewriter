using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Proto;

namespace ConsoleApp1.Actors;

public class NxLogParserActor(PipeReader reader, PipeWriter writer) : IActor
{
    internal int TotalSize { get; private set; } = 0;

    public Task ReceiveAsync(IContext ctx) => ctx.Message switch
    {
        Started => Task.Run(() => ctx.SetReceiveTimeout(TimeSpan.FromSeconds(1))),
        string v => WriteAsync(ctx, v),
        ReceiveTimeout => ReadAsync(ctx),
        Stop => ReadAsync(ctx),
        _ => Task.CompletedTask
    };

    internal async Task WriteAsync(IContext ctx, string v)
    {
        var jsonLine = Encoding.UTF8.GetBytes(v + "\n");

        if (TotalSize + jsonLine.Length > 1000)
        {
            await ReadAsync(ctx);
        }

        _ = await writer.WriteAsync(jsonLine);
        TotalSize += jsonLine.Length;
    }

    internal async Task ReadAsync(IContext ctx)
    {
        var read = await reader.ReadAsync();
        var buffer = read.Buffer;
        var bufferArray = buffer.ToArray();
        reader.AdvanceTo(buffer.End);
        TotalSize = 0;

        ctx.Send(ActorPath.NxLogSenderActorPid, new NxLogSenderRequest(bufferArray));
    }
}
