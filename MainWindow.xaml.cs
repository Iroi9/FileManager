using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace FileManager
{
    public partial class MainWindow : Window
    {
        private Stack<string> backHistory = new Stack<string>();
        private Stack<string> forwardHistory = new Stack<string>();
        public ObservableCollection<DirectoryItem> Directories { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Directories = new ObservableCollection<DirectoryItem>();
            treeView.ItemsSource = Directories;

            // Load root directories
            foreach (var drive in DriveInfo.GetDrives())
            {
                var rootDirectory = new DirectoryItem { Name = drive.Name, FullPath = drive.RootDirectory.FullName };
                rootDirectory.PopulateSubDirectories();
                Directories.Add(rootDirectory);
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DirectoryItem selectedItem = (DirectoryItem)treeView.SelectedItem;
            if (selectedItem != null)
            {
                PopulateListView(selectedItem.FullPath);
            }
        }

        private void PopulateListView(string path)
        {
            backHistory.Push(path);
            listView.Items.Clear();
            DirectoryInfo directory = new DirectoryInfo(path);

            try
            {
                // Display subdirectories
                foreach (var subDir in directory.GetDirectories())
                {
                    // Create a new FileSystemItem object for the subdirectory
                    FileSystemItem subDirItem = new FileSystemItem
                    {
                        Name = subDir.Name,
                        FullPath = subDir.FullName, // Assign the full path of the subdirectory
                        IsFolder = true,
                        DateModified = subDir.LastWriteTime
                    };

                    // Add the FileSystemItem object to the ListView
                    listView.Items.Add(subDirItem);
                }


                // Display files
                foreach (var file in directory.GetFiles())
                {
                    listView.Items.Add(new FileSystemItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        Size = file.Length,
                        IsFolder = false,
                        DateModified = file.LastWriteTime
                    });
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

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileSystemItem selectedItem = (FileSystemItem)listView.SelectedItem;
            if (selectedItem != null && !selectedItem.IsFolder && selectedItem.Extension == ".txt")
            {
                DisplayTextFileContent(selectedItem.FullPath);
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileSystemItem selectedItem = (FileSystemItem)listView.SelectedItem;
            if (selectedItem != null && selectedItem.IsFolder)
            {
                listView.Items.Clear();

        
                PopulateListView(selectedItem.FullPath);
            }
        }


        private void DisplayTextFileContent(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                txtFileContent.Text = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (backHistory.Count > 1)
            {
                forwardHistory.Push(backHistory.Pop());
                string previousPath = backHistory.Peek();
                PopulateListView(previousPath);
            }
            else
            {
                MessageBox.Show("No previous path available.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            if (forwardHistory.Count > 1)
            {
                string previousPath = forwardHistory.Pop();
                
                
                    PopulateListView(previousPath);
                
            }
            else
            {
                MessageBox.Show("No deeper path available.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


    }

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
                // Clear existing subdirectories before repopulating
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

    public class FileSystemItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public bool IsFolder { get; set; }
        public DateTime DateModified { get; set; }
        public string Extension => Path.GetExtension(Name); 
    }

}
