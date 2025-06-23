namespace CssClassConstDesigner;
internal static class InitClass
{
    public static async Task InitAsync()
    {
        if (cc1.HasOnlyCommandArgument())
        {
            await DefaultsAsync();
            return;
        }
        bool quiet = cc1.HasFlag(FlagClasses.Quiet);
        //will initialize the experiment tool
        BasicList<string> cssFiles = GetCssFiles();
        string jsonPath = HelpersClass.GetJsonFile();
        if (quiet == false)
        {
            Console.WriteLine($"Using JSON config file: {jsonPath}");
            Console.WriteLine($"Found {cssFiles.Count} CSS files.");
        }
        string outputFolder = GetOutputFolder();
        await css1.GenerateInitialConfigAsync(jsonPath, cssFiles, outputFolder);
        if (ff1.FileExists(jsonPath) == false)
        {
            Console.Error.WriteLine("Error: JSON file was not created.");
            Environment.Exit(1);
        }
        if (cc1.HasFlag(FlagClasses.NoOpen))
        {
            return;  //because you chose not to open the file.
        }
        string? editorPath = cc1.GetValue(ArgumentClasses.OpenWith);
        if (string.IsNullOrWhiteSpace(editorPath) == true)
        {
            editorPath = null; //will use default editor path.
        }
        OpenEditor(editorPath, jsonPath, quiet);
    }
    
    private static string GetOutputFolder()
    {
        string outputFolder = cc1.GetValue(ArgumentClasses.OutputFolder);
        if (string.IsNullOrWhiteSpace(outputFolder) == false)
        {
            return outputFolder;
        }
        return "Resources";
    }
    private static BasicList<string> GetCssFiles()
    {

        string cssList = cc1.GetValue(ArgumentClasses.CssList);
        string cssDir = cc1.GetValue(ArgumentClasses.CssDir);
        if (string.IsNullOrWhiteSpace(cssList) && string.IsNullOrWhiteSpace(cssDir))
        {
            return GetDefaultCssFiles();
        }
        if (!string.IsNullOrWhiteSpace(cssList) && !string.IsNullOrWhiteSpace(cssDir))
        {
            Console.Error.WriteLine("Error: Cannot specify both css-list and css-dir arguments.");
            Environment.Exit(1);
        }
        if (string.IsNullOrWhiteSpace(cssList) == false)
        {
            // css-list is specified
            string startList = cc1.GetValue(ArgumentClasses.CssList);

            var output = startList.Split(';').ToBasicList();
            foreach (var item in output)
            {
                if (ff1.FileExists(item) == false)
                {
                    Console.Error.WriteLine($"Error: CSS file '{item}' does not exist.");
                    Environment.Exit(1);
                }
            }
            return output;
        }
        return GetCssFiles(cc1.GetValue(ArgumentClasses.CssDir));
    }
    private static BasicList<string> GetCssFiles(string root)
    {
        if (Directory.Exists(root) == false)
        {
            Console.Error.WriteLine("Error: wwwroot directory not found.");
            Environment.Exit(1);
        }
        var files = Directory
            .GetFiles(root, "*.css", SearchOption.AllDirectories)
            .ToBasicList();
        if (files.Count == 0)
        {
            Console.Error.WriteLine("Error: No CSS files found in the specified directory.");
            Environment.Exit(1);
        }
        return files;
    }
    private static BasicList<string> GetDefaultCssFiles()
    {
        string root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        return GetCssFiles(root);
    }
    private static async Task DefaultsAsync()
    {
        if (cc1.HasOnlyCommandArgument() == false)
        {
            Console.Error.WriteLine("DefaultsAsync should only run when no other arguments are provided.");
            Environment.Exit(1);
        }
        BasicList<string> cssFiles = GetDefaultCssFiles();
        // Prepare Resources folder and default path
        string workingDir = Directory.GetCurrentDirectory();
        string resourcesFolder = Path.Combine(workingDir, "Resources");
        Directory.CreateDirectory(resourcesFolder); // will do nothing if it already exists
        string jsonPath = Path.Combine(resourcesFolder, "generatedCssConfig.json");
        await css1.GenerateInitialConfigAsync(jsonPath, cssFiles);
        //open this file.

        if (ff1.FileExists(jsonPath) == false)
        {
            Console.Error.WriteLine("Error: JSON file was not created.");
            Environment.Exit(1);
        }
        OpenEditor(null, jsonPath, false);
    }
    private static void OpenEditor(string? editorPath, string jsonPath, bool quiet)
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
                Arguments = $"\"{jsonPath}\"",
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