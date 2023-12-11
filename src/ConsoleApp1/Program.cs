using System.IO.Pipelines;
using System.Text.Json;
using ConsoleApp1.Actors;
using Proto;

var pipe = new Pipe();
var reader = pipe.Reader;
var writer = pipe.Writer;
var http = new HttpClient()
{
    BaseAddress = new Uri("https://httpbin.org/")
};

var system = new ActorSystem();
var root = system.Root;
var senderActor = root.Spawn(Props.FromProducer(ctx => new NxLogSenderActor(http)));
var parserActor = root.Spawn(Props.FromProducer(ctx => new NxLogParserActor(reader, writer)));


var i = 0;
while (true)
{
    root.Send(parserActor, JsonSerializer.Serialize(new
    {
        number = i++
    }));
    await Task.Delay(1);
}


public static class ActorPath
{
    public static PID NxLogSenderActorPid { get; } = new PID(ActorSystem.NoHost, nameof(NxLogSenderActor));
    public static PID NxLogParserActorPid { get; } = new PID(ActorSystem.NoHost, nameof(NxLogParserActor));
}
