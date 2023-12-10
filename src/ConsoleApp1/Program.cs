using System.Buffers;
using System.IO.Pipelines;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ConsoleApp1;
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
var actor = root.Spawn(Props.FromProducer(ctx => new NxLogActor(reader, writer, http)));

var i = 0;
while (true)
{
    root.Send(actor, JsonSerializer.Serialize(new
    {
        number = i++
    }));
    await Task.Delay(1);
}


