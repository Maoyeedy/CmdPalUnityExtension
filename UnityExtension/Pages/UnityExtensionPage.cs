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
    public UnityExtensionPage()
    {
        Icon = Resources.IconUnity;
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

                foreach (var project in favoriteProjects)
                {
                    items.Add(CreateProjectListItem(project));
                }

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
                Icon = Resources.IconUrl
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No Unity projects found",
                Subtitle = "Make sure you have Unity Hub installed and have opened projects with it",
                Icon = Resources.IconUrl
            });
        }

        IsLoading = false;
        return items.ToArray();
    }

    private ListItem CreateProjectListItem(UnityProject project)
    {
        var command = new OpenUnityProjectCommand(project.Path);

        var tags = new List<Tag>();
        if (project.IsFavorite)
        {
            tags.Add(new Tag("⭐ Favorite") { Foreground = ColorHelpers.FromRgb(222, 186, 56) });
        }

        tags.Add(new Tag(project.Version));

        return new ListItem(command)
        {
            Title = project.Title,
            Subtitle = project.Path,
            Icon = Resources.IconUnity,
            Tags = tags.ToArray()
        };
    }

    private List<UnityProject> GetUnityProjects()
    {
        var result = new List<UnityProject>();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var projectsJsonFilePath = Path.Combine(appDataPath, Resources.ProjectsJsonPath);

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
}

internal sealed class UnityProject
{
    public string Path { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public long LastModified { get; init; }
    public bool IsFavorite { get; init; }
}

internal sealed partial class OpenUnityProjectCommand : InvokableCommand
{
    private readonly string _projectPath;

    public OpenUnityProjectCommand(string projectPath)
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
