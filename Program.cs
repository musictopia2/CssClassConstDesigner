string commandName = cc1.GetCommandName();
switch (commandName)
{
    case "init":
        await InitClass.InitAsync();
        return;
    case "finalize":
        await FinalizeClass.CreateXmlFilesAsync();
        return;
    case null:
    case "":
        Console.Error.WriteLine("No command found");
        Environment.Exit(1);
        return;
    default:
        Console.Error.WriteLine($"Command '{commandName}' not supported");
        Environment.Exit(1);
        return;
}