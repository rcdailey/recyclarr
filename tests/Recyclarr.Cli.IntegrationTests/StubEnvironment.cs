using System.IO.Abstractions;
using Recyclarr.Common.Extensions;
using Recyclarr.Platform;

namespace Recyclarr.Cli.IntegrationTests;

public class StubEnvironment(IFileSystem fs) : IEnvironment
{
    private readonly Dictionary<string, string> _env = new();

    private IDirectoryInfo BuildSpecialFolderPath(Environment.SpecialFolder folder)
    {
        return fs.CurrentDirectory().SubDirectory("special_folder", folder.ToString());
    }

    public string GetFolderPath(Environment.SpecialFolder folder)
    {
        return BuildSpecialFolderPath(folder).FullName;
    }

    public string GetFolderPath(Environment.SpecialFolder folder, Environment.SpecialFolderOption folderOption)
    {
        var path = BuildSpecialFolderPath(folder);
        if (folderOption == Environment.SpecialFolderOption.Create)
        {
            path.Create();
        }

        return path.FullName;
    }

    public void AddEnvironmentVariable(string variable, string value)
    {
        _env.Add(variable, value);
    }

    public string? GetEnvironmentVariable(string variable)
    {
        return _env.GetValueOrDefault(variable);
    }
}
