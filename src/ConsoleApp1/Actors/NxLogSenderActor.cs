using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;
using Proto;

namespace ConsoleApp1.Actors;

public record NxLogSenderRequest(byte[] Value);

public class NxLogSenderActor(HttpClient http) : IActor
{
    public Task ReceiveAsync(IContext context) => context.Message switch
    {
        NxLogSenderRequest { Value : { } v } => Task.Run(async () =>
        {
            using var request = new ByteArrayContent(v);

            request.Headers.ContentType = new("application/x-ndjson");
            var ret = await http.PostAsync("post", request);

            Console.WriteLine(await ret.Content.ReadAsStringAsync());
        }),
        _ => Task.CompletedTask,
    };
}
