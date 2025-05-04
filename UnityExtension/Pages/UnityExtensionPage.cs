// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UnityExtension;

internal sealed partial class UnityExtensionPage : ListPage
{
    private static string ProjectsJsonPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UnityHub\\projects-v1.json");

    public UnityExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Unity Projects";
        Name = "Open";
        ShowDetails = true;
    }

    public override IListItem[] GetItems()
    {
        var items = new List<ListItem>();
        IsLoading = true;

        try
        {
            var projects = GetUnityProjects();
            if (projects.Count > 0)
            {
                // Sort projects by last modified date (newest first)
                projects.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

                // Show favorite projects first
                var favoriteProjects = projects.Where(p => p.IsFavorite).ToList();
                var nonFavoriteProjects = projects.Where(p => !p.IsFavorite).ToList();

                // Create list items for favorite projects
                foreach (var project in favoriteProjects)
                {
                    items.Add(CreateProjectListItem(project));
                }

                // Create list items for non-favorite projects
                foreach (var project in nonFavoriteProjects)
                {
                    items.Add(CreateProjectListItem(project));
                }
            }
        }
        catch (Exception ex)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Error loading Unity projects",
                Subtitle = ex.Message,
                //TODO: error icon
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No Unity projects found",
                Subtitle = "Make sure you have Unity Hub installed and have opened projects with it",
                Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png")
            });
        }

        IsLoading = false;
        return items.ToArray();
    }

    private ListItem CreateProjectListItem(UnityProject project)
    {
        var command = new OpenUnityProjectCommand(project.Path);

        var details = new Details
        {
            Title = project.Title,
            Metadata = new[]
            {
                    new DetailsElement
                    {
                        Key = "Unity Version",
                        Data = new DetailsTags { Tags = [new Tag(project.Version)] }
                    },
                    new DetailsElement
                    {
                        Key = "Path",
                        Data = new DetailsTags { Tags = [new Tag(project.Path)] }
                    },
                    new DetailsElement
                    {
                        Key = "Last Modified",
                        Data = new DetailsTags { Tags = [new Tag(UnixTimeToDateTime(project.LastModified).ToString(System.Globalization.CultureInfo.InvariantCulture))] }
                    }
                }
        };

        var tags = new List<Tag>();
        if (project.IsFavorite)
        {
            tags.Add(new Tag("⭐ Favorite") { Foreground = ColorHelpers.FromRgb(255, 215, 0) });
        }

        tags.Add(new Tag(project.Version));

        return new ListItem(command)
        {
            Title = project.Title,
            Subtitle = project.Path,
            Details = details,
            Icon = IconHelpers.FromRelativePath("Assets\\UnityLogo.png"),
            Tags = tags.ToArray()
        };
    }

    private List<UnityProject> GetUnityProjects()
    {
        var result = new List<UnityProject>();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var projectsJsonFilePath = Path.Combine(appDataPath, ProjectsJsonPath);

        if (!File.Exists(projectsJsonFilePath))
        {
            return result;
        }

        try
        {
            var jsonContent = File.ReadAllText(projectsJsonFilePath);
            var projectsRoot = JsonDocument.Parse(jsonContent).RootElement;

            if (projectsRoot.TryGetProperty("data", out var data))
            {
                foreach (var property in data.EnumerateObject())
                {
                    var projectPath = property.Name;
                    var projectInfo = property.Value;

                    var project = new UnityProject
                    {
                        Path = projectPath,
                        Title = projectInfo.GetProperty("title").GetString() ?? Path.GetFileName(projectPath),
                        Version = projectInfo.GetProperty("version").GetString() ?? "Unknown",
                        LastModified = projectInfo.GetProperty("lastModified").GetInt64(),
                        IsFavorite = projectInfo.TryGetProperty("isFavorite", out var isFavorite) && isFavorite.GetBoolean()
                    };

                    result.Add(project);
                }
            }
        }
        catch (Exception)
        {
            // Any error in parsing will result in an empty list
        }

        return result;
    }

    private DateTime UnixTimeToDateTime(long unixTimeMilliseconds)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddMilliseconds(unixTimeMilliseconds).ToLocalTime();
    }
}

internal sealed class UnityProject
{
    public string Path { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public long LastModified { get; set; }
    public bool IsFavorite { get; set; }
}

internal sealed partial class OpenUnityProjectCommand : InvokableCommand
{
    private readonly string _projectPath;

    public OpenUnityProjectCommand(string projectPath)
    {
        _projectPath = projectPath;
        Name = "Open Unity Project";
        Icon = IconHelpers.FromRelativePath("Assets\\UnityLogo.png");
    }

    public override ICommandResult Invoke()
    {
        try
        {
            // For now, we just open the folder containing the project
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
