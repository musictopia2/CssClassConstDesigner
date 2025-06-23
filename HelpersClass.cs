namespace CssClassConstDesigner;
internal static class HelpersClass
{
    public static string GetJsonFile()
    {
        if (string.IsNullOrWhiteSpace(cc1.GetValue(ArgumentClasses.Json)) == true)
        {

            string workingDir = Directory.GetCurrentDirectory();
            string resourcesFolder = Path.Combine(workingDir, "Resources");
            Directory.CreateDirectory(resourcesFolder); // will do nothing if it already exists
            string jsonPath = Path.Combine(resourcesFolder, "generatedCssConfig.json");
            return jsonPath;
        }
        return cc1.GetValue(ArgumentClasses.Json);
    }
}
