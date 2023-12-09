using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Proto;

namespace ConsoleApp1;

internal class NxLogActor(PipeReader reader, PipeWriter writer, HttpClient http) : IActor
{
    internal int TotalSize { get; private set; } = 0;

    public Task ReceiveAsync(IContext ctx) => ctx.Message switch
    {
        Started => Task.Run(() => ctx.SetReceiveTimeout(TimeSpan.FromSeconds(1))),
        string v => Task.Run(async () =>
        {
            var jsonLine = Encoding.UTF8.GetBytes(v + "\n");

            if (TotalSize + jsonLine.Length > 1000)
            {
                await ReadAsync();
            }

            var write = await writer.WriteAsync(jsonLine);
            TotalSize += jsonLine.Length;
        }),
        ReceiveTimeout => ReadAsync(),
        _ => Task.CompletedTask
    };

    internal Task ReadAsync() => Task.Run(async () =>
    {
        var read = await reader.ReadAsync();
        var buffer = read.Buffer;

        using var request = new ByteArrayContent(buffer.ToArray());

        request.Headers.ContentType = new("application/x-ndjson");

        var ret = await http.PostAsync("post", request);

        Console.WriteLine(await ret.Content.ReadAsStringAsync());
        reader.AdvanceTo(buffer.End);
        TotalSize = 0;
    });
}
