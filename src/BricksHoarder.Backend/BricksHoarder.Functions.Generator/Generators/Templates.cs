namespace BricksHoarder.Functions.Generator.Generators
{
    public static class Templates
    {
        private const string TemplateCatalogPath = "BricksHoarder.Functions.Generator\\Templates";
        public static string EventMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventMetadata.tmpl");
        public static string EventFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventFunction.tmpl");
        public static string CommandMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandMetadata.tmpl");
        public static string CommandConsumedMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandConsumedMetadata.tmpl");
        public static string CommandFaultedMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandFaultedMetadata.tmpl");
        public static string CommandFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\CommandFunction.tmpl");
    }
}