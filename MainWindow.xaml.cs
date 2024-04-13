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
            var selectedItem = (DirectoryItem)treeView.SelectedItem;
            if (selectedItem != null)
            {
                PopulateListView(selectedItem.FullPath);
            }
        }

        private void PopulateListView(string path)
        {
            listView.Items.Clear();
            DirectoryInfo directory = new DirectoryInfo(path);
            foreach (FileInfo file in directory.GetFiles())
            {
                listView.Items.Add(new FileItem
                {
                    Name = file.Name,
                    Size = file.Length,
                    DateModified = file.LastWriteTime
                });
            }
        }
    }

    public class FileItem
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime DateModified { get; set; }
    }
}
