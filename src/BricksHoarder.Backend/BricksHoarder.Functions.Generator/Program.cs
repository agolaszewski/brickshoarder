//dotnet run --project ./BricksHoarder.Functions.Generator/BricksHoarder.Functions.Generator.csproj

using BricksHoarder.Commands;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Domain;
using BricksHoarder.Events;
using BricksHoarder.Functions.Generator.Generators;

var eventsAssembly = typeof(BricksHoarderEventsAssemblyPointer).Assembly.GetTypes();
var events = eventsAssembly.Where(t => t.GetInterface(nameof(IEvent)) is not null).Where(t => !t.IsGenericType).ToList();

var functionsCatalog = "BricksHoarder.Functions";

var templateCatalogPath = "BricksHoarder.Functions.Generator\\Templates";
var eventsMetadataCatalog = $"BricksHoarder.Events\\Metadata";

var commandsAssembly = typeof(BricksHoarderCommandsAssemblyPointer).Assembly.GetTypes();
var commands = commandsAssembly.Where(t => t.GetInterface(nameof(ICommand)) is not null).ToList();

var commandMetadataTemplate = File.ReadAllText($"{templateCatalogPath}\\CommandMetadata.tmpl");
var commandConsumedMetadataTemplate = File.ReadAllText($"{templateCatalogPath}\\CommandConsumedMetadata.tmpl");
var commandFunctionTemplate = File.ReadAllText($"{templateCatalogPath}\\CommandFunction.tmpl");

var commandMetadataCatalog = $"BricksHoarder.Commands\\Metadata";

foreach (var command in commands)
{
    var compiled = commandMetadataTemplate.Replace("{{command}}", command.Name);
    File.WriteAllText($"{commandMetadataCatalog}\\{command.Name}Metadata.cs", compiled);

    compiled = commandConsumedMetadataTemplate.Replace("{{command}}", command.Name);
    File.WriteAllText($"{eventsMetadataCatalog}\\{command.Name}ConsumedMetadata.cs", compiled);

    compiled = commandFunctionTemplate.Replace("{{command}}", command.Name);
    compiled = compiled.Replace("{{commandNamespace}}", command.Namespace);
    File.WriteAllText($"{functionsCatalog}\\{command.Name}Function.cs", compiled);
}

var eventsGenerator = new EventGenerator();
eventsGenerator.GenerateMetadata();
eventsGenerator.GenerateFunctionsForSagas();