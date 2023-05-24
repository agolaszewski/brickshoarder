using BricksHoarder.Commands;
using BricksHoarder.Core.Events;

var functions = Directory.GetFiles("..\\..\\..\\..\\..\\..\\src\\BricksHoarder.Backend\\BricksHoarder.Functions\\", "*.cs");

var eventsAssembly = typeof(BricksHoarderEventsAssemblyPointer).Assembly.GetTypes();
var events = eventsAssembly.Where(t => t.GetInterface(nameof(IEvent)) is not null).ToList();

foreach (var @event in events)
{
    
}