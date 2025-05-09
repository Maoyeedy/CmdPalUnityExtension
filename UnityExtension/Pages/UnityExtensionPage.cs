// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UnityExtension;

internal sealed partial class UnityExtensionPage : ListPage
{
    private readonly SettingsManager _settingsManager;
    public UnityExtensionPage(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        Icon = Resources.IconUnity;
        Title = "Recent Unity Projects";
        Name = "Recent Unity Projects";
        ShowDetails = false;
    }

    public override IListItem[] GetItems()
    {
        var items = new List<ListItem>();
        IsLoading = true;

        try
        {
            var projects = ProjectParser.GetUnityProjects();

            if (projects.Count == 0)
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "No Unity projects found",
                    Subtitle = "Make sure you have Unity Hub installed and have opened projects with it",
                    Icon = Resources.IconUrl
                });
            }
            else
            {
                projects.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

                if (_settingsManager.GroupFavoritesFirst)
                {
                    var favoriteProjects = projects.Where(p => p.IsFavorite).ToList();
                    var nonFavoriteProjects = projects.Where(p => !p.IsFavorite).ToList();

                    items.AddRange(favoriteProjects.Select(CreateProjectListItem));
                    items.AddRange(nonFavoriteProjects.Select(CreateProjectListItem));
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

        IsLoading = false;
        return items.ToArray();
    }

    private static ListItem CreateProjectListItem(UnityProject project)
    {
        var defaultCommand = new OpenUnityCommand(project);

        var contextCommands = new List<IContextItem>
        {
            new CommandContextItem(new OpenExplorerCommand(project))
            {
                Title = "Open in File Explorer"
            }
        };

        var tags = new List<Tag>();
        if (project.IsFavorite)
        {
            tags.Add(new Tag("⭐ Favorite") { Foreground = ColorHelpers.FromRgb(222, 186, 56) });
        }

        tags.Add(new Tag(project.Version));

        return new ListItem(defaultCommand)
        {
            Title = project.Title,
            Subtitle = project.Path,
            Icon = Resources.IconUnity,
            Tags = tags.ToArray(),
            MoreCommands = contextCommands.ToArray()
        };
    }
}
