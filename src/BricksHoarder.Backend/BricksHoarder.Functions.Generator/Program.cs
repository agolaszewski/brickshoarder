//dotnet run --project ./BricksHoarder.Functions.Generator/BricksHoarder.Functions.Generator.csproj

using BricksHoarder.Functions.Generator.Generators;

var eventsGenerator = new EventGenerator();

eventsGenerator.Initialize();

eventsGenerator.GenerateMetadata();
eventsGenerator.GenerateFunctionsForSagas();

var commandsGenerator = new CommandsGenerator();
commandsGenerator.GenerateMetadata();
commandsGenerator.GenerateFunctions();