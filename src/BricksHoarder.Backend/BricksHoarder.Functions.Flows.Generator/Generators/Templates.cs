namespace BricksHoarder.Functions.Flows.Generator.Generators
{
    public static class Templates
    {
        private const string TemplateCatalogPath = "..\\..\\..\\..\\BricksHoarder.Functions.Flows.Generator\\Templates";
        private const string ConfigurationCatalogPath = "..\\..\\..\\..\\BricksHoarder.AzureServiceBus\\ServiceCollectionExtensions.cs";

        public static string EventMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventMetadata.tmpl");
        public static string EventBatchMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventBatchMetadata.tmpl");

        public static string EventFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventFunction.tmpl");
        public static string EventBatchFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventBatchFunction.tmpl");
        public static string EventScheduleCommandFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\ScheduleCommandFunction.tmpl");

        public static string CommandMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandMetadata.tmpl");
        public static string CommandConsumedMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandConsumedMetadata.tmpl");
        public static string CommandFaultedMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandFaultedMetadata.tmpl");
        public static string CommandFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandFunction.tmpl");

        public static string SagaConsumer = File.ReadAllText($"{TemplateCatalogPath}\\SagaConsumer.tmpl");
    }
}