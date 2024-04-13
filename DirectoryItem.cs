using System;
using System.Collections.ObjectModel;
using System.IO;

public class DirectoryItem
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public ObservableCollection<DirectoryItem> SubDirectories { get; set; }

    public DirectoryItem()
    {
        SubDirectories = new ObservableCollection<DirectoryItem>();
    }

    public void PopulateSubDirectories()
    {
        try
        {
            string[] subdirectoryEntries = Directory.GetDirectories(FullPath);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(subdirectory);
                var subDirectoryItem = new DirectoryItem
                {
                    Name = directoryInfo.Name,
                    FullPath = directoryInfo.FullName
                };
                subDirectoryItem.PopulateSubDirectories(); // Recursively populate subdirectories
                SubDirectories.Add(subDirectoryItem);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Handle unauthorized access to directories
        }
        catch (DirectoryNotFoundException)
        {
            // Handle directory not found exception
        }
    }
}

