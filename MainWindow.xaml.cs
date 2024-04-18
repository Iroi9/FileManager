using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;

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
                Directories.Add(new DirectoryItem { Name = drive.Name, FullPath = drive.RootDirectory.FullName });
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

    }
    public class FileSystemItem
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public bool IsFolder { get; set; }
        public DateTime DateModified { get; set; }
        public string Extension => Path.GetExtension(Name); // Compute file extension from the file name
    }



}
