using System;
using System.Collections.ObjectModel;
using System.Windows;
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
            
            SubDirectories.Clear();

            string[] subdirectoryEntries = Directory.GetDirectories(FullPath);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(subdirectory);
                var subDirectoryItem = new DirectoryItem
                {
                    Name = directoryInfo.Name,
                    FullPath = directoryInfo.FullName
                };
                subDirectoryItem.PopulateSubDirectories();
                SubDirectories.Add(subDirectoryItem);
            }
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show("Access to the directory is denied.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (DirectoryNotFoundException)
        {
            MessageBox.Show("Directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
