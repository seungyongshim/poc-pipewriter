using System.Buffers;
using System.IO.Pipelines;
using System.Net.Http.Json;
using System.Text;
using Proto;
using Proto.Timers;

var pipe = new Pipe();
var reader = pipe.Reader;
var writer = pipe.Writer;
var http = new HttpClient()
{
    BaseAddress = new Uri("https://httpbin.org/")
};

var system = new ActorSystem();
var root = system.Root;

var actor = root.Spawn(Props.FromFunc(ctx => ctx.Message switch
{
    Started => Task.Run(() => ctx.Scheduler().SendRepeatedly(TimeSpan.FromSeconds(1), ctx.Self, new Print())),
    string v => Task.Run(async () =>
    {
        var write = await writer.WriteAsync(Encoding.UTF8.GetBytes(v + "\n"));
    }),
    Print => Task.Run(async () =>
    {
        var read = await reader.ReadAsync();

        var ret = await http.PostAsync("post", new ByteArrayContent(read.Buffer.ToArray())
        {
            
        });

        Console.WriteLine(ret);
        reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);
    }),
    _ => Task.CompletedTask
}));


var i = 0;
while (true)
{
    root.Send(actor, $"{i}");
    i++;
    await Task.Delay(1);
}

record Print;
