using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace UnityExtension;

internal sealed partial class OpenExplorerCommand : InvokableCommand
{
    private readonly string _projectPath;

    public OpenExplorerCommand(string projectPath)
    {
        _projectPath = projectPath;
        Name = "Open Unity Project";
        Icon = Resources.IconUnity;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _projectPath,
                UseShellExecute = true
            });

            return CommandResult.Hide();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast($"Error opening project: {ex.Message}");
        }
    }
}
