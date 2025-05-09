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
            var projects = ProjectParser.GetUnityProjects();
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

    private static ListItem CreateProjectListItem(UnityProject project)
    {
        // var command = new OpenExplorerCommand(project.Path);

        var command = new OpenUnityCommand(project);

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
}
