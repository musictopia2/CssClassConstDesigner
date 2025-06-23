namespace CssClassConstDesigner;
public static class ParseConfigClass
{
    public static void CreateXml(CssToolConfiguration config)
    {
        foreach (var configItem in config.Files)
        {
            CreateXml(configItem, config);
        }
    }
    private static void CreateXml(CssFileConfig file, CssToolConfiguration config)
    {
        string outputFolder = config.DefaultOutputPath;
        string outputPath = Path.Combine(outputFolder, file.OutputName);
        string newPath = Path.GetFullPath(outputPath);
        string cssPath;
        if (string.IsNullOrWhiteSpace(file.BasePath))
        {
            cssPath = Path.Combine(config.DefaultBasePath, file.FileName);
        }
        else
        {
            cssPath = Path.Combine(file.BasePath, file.FileName);
        }
        string content = ff1.AllText(cssPath);
        var list = CssClassExtractor.ExtractStandaloneClasses(content)
               .Where(item => !file.LocalExcludedClasses.Contains(item, StringComparer.OrdinalIgnoreCase))
               .Where(item => !config.GlobalExcludedClasses.Contains(item, StringComparer.OrdinalIgnoreCase));
        XElement source = new("Root",
            new XElement("DoAllClasses", "true")
        );
        foreach (var item in list)
        {
            var nexts = GetOverride(item, file);
            source.Add(CreateXmlElement(nexts, item, file));
        }
        source.Save(newPath);
    }

    private static XElement CreateXmlElement(GroupOverride? group, string cssName, CssFileConfig file)
    {
        return new XElement("ClassInformation",
            new XElement("ClassName", GetGeneratedClassName(group, file)),
            new XElement("MethodName", GetMethodName(cssName)),
            new XElement("Namespace", GetNamespace(group, file)),
            new XElement("Value", cssName)
        );
    }
    private static  string GetNamespace(GroupOverride? group, CssFileConfig file)
    {
        if (group is not null)
        {
            if (string.IsNullOrWhiteSpace(group.Namespace) == false)
            {
                return group.Namespace;
            }
        }
        if (string.IsNullOrWhiteSpace(file.DefaultNamespace) == false)
        {
            return file.DefaultNamespace;
        }
        return "CssHelpers"; //i think.
    }
    private static string GetGeneratedClassName(GroupOverride? group, CssFileConfig file)
    {
        //i think this should go ahead and give all details necessary so the source generator becomes easier.
        if (group is not null)
        {
            return $"{group.DisplayName.ToTitleCase(false)}CssClasses";
        }
        if (file.DefaultCategory != "")
        {
            return file.DefaultCategory;
        }
        return "MiscCssClasses";
    }
    private static string GetMethodName(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return string.Empty;
        }

        var parts = className
            .Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries);

        var result = string.Concat(parts.Select(FirstCharToUpper));
        return result;
    }
    private static string FirstCharToUpper(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }
    private static GroupOverride? GetOverride(string className, CssFileConfig file)
    {
        foreach (var item in file.Overrides)
        {
            if (className.StartsWith(item.OriginalName))
            {
                return item;
            }
        }
        return null;
    }
}