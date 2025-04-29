using Microsoft.CommandPalette.Extensions.Toolkit;

namespace UnityExtension;

/// <summary>
/// Command to open a Unity Project
/// </summary>
internal sealed partial class OpenUnityCommand : InvokableCommand
{
    public override string Name => "Open Unity";
    private string _projectPath;
    private string _executablePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenUnityCommand"/> class.
    /// </summary>
    /// <param name="executablePath">The path to the Unity Editor executable.</param>
    /// <param name="projectPath">The path to the project.</param>
    public OpenUnityCommand(string executablePath, string projectPath)
    {
        this._projectPath = projectPath;
        this._executablePath = executablePath;
    }

    /// <summary>
    /// Invokes the command to open the project with corresponding version of Editor.
    /// </summary>
    /// <returns>The result of the command execution.</returns>
    public override CommandResult Invoke()
    {
        string? arguments;

        arguments = $"--folder-uri \"{_projectPath}\"";

        ShellHelpers.OpenInShell(_executablePath, arguments, null, ShellHelpers.ShellRunAsType.None, false);

        return CommandResult.Hide();
    }
}
