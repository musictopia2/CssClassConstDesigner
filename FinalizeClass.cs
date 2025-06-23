namespace CssClassConstDesigner;
internal static class FinalizeClass
{
    public static async Task CreateXmlFilesAsync()
    {
        if (cc1.HasOnlyCommandArgument())
        {
            await DefaultsAsync();
            return;
        }
        string jsonPath = HelpersClass.GetJsonFile();
        if (ff1.FileExists(jsonPath) == false)
        {
            Console.Error.WriteLine("Error: JSON file does not exist. Please run the tool with the init option first.");
            Environment.Exit(1);
        }
        CssToolConfiguration config = await jj1.RetrieveSavedObjectAsync<CssToolConfiguration>(jsonPath);
        ParseConfigClass.CreateXml(config);
        bool onlyXml = cc1.HasFlag(FlagClasses.OnlyXml);
        bool skipGenerator = cc1.HasFlag(FlagClasses.SkipGenerator);
        if (onlyXml == true && skipGenerator == false && cc1.HasFlag(FlagClasses.Quiet) == false)
        {
            Console.WriteLine("You chose only XML.  Even you did did not choose to skip generator, will not generate code for generator because you only wanted xml");
            return;
        }
        if (onlyXml == false)
        {
            EditCsProjFile(skipGenerator, config);
        }
        if (cc1.HasFlag(FlagClasses.NoOpen))
        {
            return; //you chose no open, so we are done.
        }
        string? editorPath = cc1.GetValue(ArgumentClasses.OpenWith);
        if (string.IsNullOrWhiteSpace(editorPath) == true)
        {
            editorPath = null; //will use default editor path.
        }
        string csProjPath = GetCsProjectPath();
        OpenProject(editorPath, csProjPath, cc1.HasFlag(FlagClasses.Quiet));
    }

    private static void EditCsProjFile(bool skipGeneration, CssToolConfiguration config)
    {
        string csProjPath = GetCsProjectPath();
        XDocument doc = XDocument.Load(csProjPath);
        // Add PackageReference if missing
        XNamespace ns = doc.Root!.Name.Namespace;
        AddAdditionalFiles(doc, config, ns);
        string generatorName = cc1.GetValue(ArgumentClasses.GeneratorName);
        string generatorVersion = cc1.GetValue(ArgumentClasses.GeneratorVersion);
        if (skipGeneration)
        {
            if (string.IsNullOrWhiteSpace(generatorName) == false || string.IsNullOrWhiteSpace(generatorVersion) == false)
            {
                Console.Error.WriteLine("Error: Generator name and version cannot be specified because you chose to skip the generator.");
                Environment.Exit(1);
            }
            // Save changes
            doc.Save(csProjPath);
            return;
        }
        //figure out the generation part (since has options)
        if (string.IsNullOrWhiteSpace(generatorName) == false && string.IsNullOrWhiteSpace(generatorVersion))
        {
            Console.Error.WriteLine("Error: Version must be specified when you choose another generator.");
            Environment.Exit(1);
        }
        if (string.IsNullOrWhiteSpace(generatorName) && string.IsNullOrWhiteSpace(generatorVersion) == false)
        {
            Console.Error.WriteLine("Error: Generator name must be specified when you choose another version.");
            Environment.Exit(1);
        }
        bool hasPackageRef;
        if (string.IsNullOrWhiteSpace(generatorName) && string.IsNullOrWhiteSpace(generatorVersion))
        {
            hasPackageRef = doc.Descendants(ns + "PackageReference")
           .Any(e => e.Attribute("Include")?.Value == "CssClassConstGenerator");
            if (!hasPackageRef)
            {
                XElement itemGroup = new(ns + "ItemGroup",
                    new XElement(ns + "PackageReference",
                        new XAttribute("Include", "CssClassConstGenerator"),
                        new XAttribute("Version", "1.0.2"),
                        new XAttribute("PrivateAssets", "all")));
                doc.Root.Add(itemGroup);
            }
            doc.Save(csProjPath);
            return;
        }
        hasPackageRef = doc.Descendants(ns + "PackageReference")
           .Any(e => e.Attribute("Include")?.Value == generatorName);
        if (!hasPackageRef)
        {
            XElement itemGroup = new(ns + "ItemGroup",
                new XElement(ns + "PackageReference",
                    new XAttribute("Include", generatorName),
                    new XAttribute("Version", generatorVersion),
                    new XAttribute("PrivateAssets", "all")));
            doc.Root.Add(itemGroup);
        }
        doc.Save(csProjPath);
        return;
    }
    private static string GetCsProjectPath()
    {
        string requestedPath = cc1.GetValue(ArgumentClasses.CsProj);
        if (string.IsNullOrWhiteSpace(requestedPath) == false)
        {
            if (ff1.FileExists(requestedPath) == false)
            {
                Console.Error.WriteLine($"Error: The specified .csproj file '{requestedPath}' does not exist.");
                Environment.Exit(1);
            }
            return requestedPath;
        }

        return AutomateProjectPath();

    }
    private static string AutomateProjectPath()
    {
        string workingDir = Directory.GetCurrentDirectory();
        BasicList<string> csprojFiles = Directory.GetFiles(workingDir, "*.csproj").ToBasicList();
        if (csprojFiles.Count == 0)
        {
            Console.Error.WriteLine("Error: No .csproj file found in the current directory.");
            Environment.Exit(1);
        }
        if (csprojFiles.Count > 1)
        {
            Console.Error.WriteLine("Error: Multiple .csproj files found. Please ensure only one is present or run the tool in the correct project folder.");
            Environment.Exit(1);
        }
        string csprojPath = csprojFiles.Single();
        if (ff1.FileExists(csprojPath) == false)
        {
            Console.Error.WriteLine($"Error: The .csproj file '{csprojPath}' does not exist.");
            Environment.Exit(1);
        }
        return csprojPath;
    }
    private static async Task DefaultsAsync()
    {
        string workingDir = Directory.GetCurrentDirectory();
        string resourcesFolder = Path.Combine(workingDir, "Resources");
        string jsonPath = Path.Combine(resourcesFolder, "generatedCssConfig.json");
        if (ff1.FileExists(jsonPath) == false)
        {
            Console.Error.WriteLine("Error: JSON file does not exist. Please run the tool with the init option first.");
            Environment.Exit(1);
        }
        CssToolConfiguration config = await jj1.RetrieveSavedObjectAsync<CssToolConfiguration>(jsonPath);
        ParseConfigClass.CreateXml(config);
        //now has to figure out how to edit the csproj file.
        //and open that file.
        string csProjPath = AutomateProjectPath();
        XDocument doc = XDocument.Load(csProjPath);
        // Add PackageReference if missing
        XNamespace ns = doc.Root!.Name.Namespace;
        bool hasPackageRef = doc.Descendants(ns + "PackageReference")
            .Any(e => e.Attribute("Include")?.Value == "CssClassConstGenerator");

        if (!hasPackageRef)
        {
            XElement itemGroup = new(ns + "ItemGroup",
                new XElement(ns + "PackageReference",
                    new XAttribute("Include", "CssClassConstGenerator"),
                    new XAttribute("Version", "1.0.3"),
                    new XAttribute("PrivateAssets", "all")));
            doc.Root.Add(itemGroup);
        }

        AddAdditionalFiles(doc, config, ns);
        

        // Save changes
        doc.Save(csProjPath);
        OpenProject(null, csProjPath, false);
    }
    private static void AddAdditionalFiles(XDocument doc, CssToolConfiguration config, XNamespace ns)
    {
        // Add AdditionalFiles if missing
        string additionalPath = Path.Combine(config.DefaultOutputPath ?? "Resources", "**", "*.xml").Replace('\\', '/');
        bool hasAdditionalFiles = doc.Descendants(ns + "AdditionalFiles")
            .Any(e => e.Attribute("Include")?.Value == additionalPath);
        if (hasAdditionalFiles == false)
        {
            XElement itemGroup = new(ns + "ItemGroup",
                new XElement(ns + "AdditionalFiles",
                    new XAttribute("Include", additionalPath)));
            doc.Root!.Add(itemGroup);
        }
    }
    private static void OpenProject(string? editorPath, string csproj, bool quiet)
    {
        string editor = editorPath ?? @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe";
        if (File.Exists(editor) == false)
        {
            Console.Error.WriteLine($"Error: Editor path '{editor}' does not exist.");
            Environment.Exit(1);
        }
        try
        {
            var info = new ProcessStartInfo
            {
                FileName = editor,
                Arguments = $"\"{csproj}\"",
                UseShellExecute = true
            };
            Process.Start(info);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: Failed to launch editor: {ex.Message}");
            if (quiet == false)
            {
                Console.WriteLine("You may want to open the file manually.");
            }
            Environment.Exit(1);
        }
    }
}