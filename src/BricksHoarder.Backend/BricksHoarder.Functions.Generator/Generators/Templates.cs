namespace BricksHoarder.Functions.Generator.Generators
{
    public static class Templates
    {
        private const string TemplateCatalogPath = "BricksHoarder.Functions.Generator\\Templates";
        public static string EventMetadataTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventMetadata.tmpl");
        public static string EventFunctionTemplate = File.ReadAllText($"{TemplateCatalogPath}\\EventFunction.tmpl");
    }
}