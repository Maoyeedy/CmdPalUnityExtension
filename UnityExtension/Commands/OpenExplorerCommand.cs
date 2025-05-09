﻿using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace UnityExtension;

internal sealed partial class OpenExplorerCommand : InvokableCommand
{
    private readonly string _projectPath;

    public OpenExplorerCommand(UnityProject project)
    {
        Name = "Open in Explorer";
        Icon = Resources.IconUrl;

        _projectPath = project.Path;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            ShellHelpers.OpenInShell("explorer.exe", $"\"{_projectPath}\"", null, ShellHelpers.ShellRunAsType.None, false);
            return CommandResult.Hide();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast($"Error opening project folder: {ex.Message}");
        }
    }
}
