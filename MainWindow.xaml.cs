using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace FileManager
{
    public partial class MainWindow : Window
    {
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
                rootDirectory.PopulateSubDirectories(); // Populate subdirectories
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
            listView.Items.Clear();
            DirectoryInfo directory = new DirectoryInfo(path);

            try
            {
                // Display subdirectories
                foreach (var subDir in directory.GetDirectories())
                {
                    listView.Items.Add(new FileSystemItem
                    {
                        Name = subDir.Name,
                        IsFolder = true,
                        DateModified = subDir.LastWriteTime
                    });
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
