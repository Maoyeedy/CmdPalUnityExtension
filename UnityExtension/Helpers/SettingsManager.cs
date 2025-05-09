using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.IO;

namespace UnityExtension;

internal sealed class SettingsManager : JsonSettingsManager
{
    private ToggleSetting GroupFavoritesFirst { get; }
    public bool IsFavoritesFirst => GroupFavoritesFirst.Value;

    private static string SettingsJsonPath()
    {
        var directory = Utilities.BaseSettingsPath("UnityExtension");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "settings.json");
    }

    public SettingsManager()
    {
        FilePath = SettingsJsonPath();

        GroupFavoritesFirst = new ToggleSetting(
            key: "groupFavoritesFirst",
            label: "Group favorites first",
            description: "Display favorite Unity projects at the top of the list",
            defaultValue: true);

        Settings.Add(GroupFavoritesFirst);

        LoadSettings();

        Settings.SettingsChanged += (sender, args) =>
        {
            SaveSettings();
        };
    }
}
