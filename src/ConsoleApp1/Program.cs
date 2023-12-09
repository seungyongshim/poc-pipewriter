
using System;
using System.IO.Pipelines;
using System.Text;
using System.Text.Unicode;
using Proto;
using Proto.Timers;

using var stream = new MemoryStream();

var pipe = new Pipe();
//var reader = PipeReader.Create(stream);
//var writer = PipeWriter.Create(stream);
var reader = pipe.Reader;
var writer = pipe.Writer;

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
        Console.WriteLine(Encoding.UTF8.GetString(read.Buffer));
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
