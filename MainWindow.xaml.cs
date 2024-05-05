using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    public partial class MainWindow : Window
    {
        private Stack<string> backHistory = new Stack<string>();
        private Stack<string> forwardHistory = new Stack<string>();
        int orientation = 0;
        public ObservableCollection<DirectoryItem> Directories { get; set; }
        public ObservableCollection<FileSystemItem> FileSystem { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Directories = new ObservableCollection<DirectoryItem>();
            FileSystem = new ObservableCollection<FileSystemItem>();
            treeView.ItemsSource = Directories;


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
                backHistory.Push(selectedItem.FullPath);
            }
        }

        private void PopulateListView(string path)
        {
            
            listView.Items.Clear();
            if (FileSystem.Count != 0)
            {
                FileSystem = new ObservableCollection<FileSystemItem>();
            }
            DirectoryInfo directory = new DirectoryInfo(path);

            try
            {


                foreach (var subDir in directory.GetDirectories())
                {
                 
                    FileSystemItem subDirItem = new FileSystemItem(subDir.Name,subDir.FullName,subDir.LastWriteTime);
                    FileSystem.Add(subDirItem);
                  
                    listView.Items.Add(subDirItem);
                }


               
                foreach (var file in directory.GetFiles())
                {

                    FileSystemItem subFile = new FileSystemItem(file.Name, file.FullName, file.LastWriteTime, false, file.Length);
                    FileSystem.Add(subFile);
                    listView.Items.Add(subFile);
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

        private void PopulateListView()
        {
            listView.Items.Clear();
            foreach(var item in FileSystem)
            {
                listView.Items.Add(item);                
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
                backHistory.Push(selectedItem.FullPath);
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

        private void NameColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            SortListView("Name");
        }

        private void SizeColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            SortListView("Size");
        }

        private void DateModifiedColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            SortListView("DateModified");
        }

        private void SortListView(string sortBy)
        {
            switch (sortBy)
            {
                case "Name":

                    if (orientation == 0) {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderByDescending(item => item.Name));
                        PopulateListView();
                        orientation = 1;
                    }
                    else
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderBy(item => item.Name));
                        PopulateListView();
                        orientation = 0;
                    }
                    break;
                case "Size":
                    if (orientation == 0)
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderByDescending(item => item.Size));
                        PopulateListView();
                        orientation = 1;
                    }
                    else
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderBy(item => item.Size));
                        PopulateListView();
                        orientation = 0;
                    }
                    break;
                case "DateModified":
                    if (orientation == 0)
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderByDescending(item => item.DateModified));
                        PopulateListView();
                        orientation = 1;
                    }else
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderBy(item => item.DateModified));
                        PopulateListView();
                        orientation = 0;
                    }
                    break;
            }
        }



        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            if (forwardHistory.Count >= 1)
            {
                string nextPath = forwardHistory.Pop();
                PopulateListView(nextPath);
            }
            else
            {
                MessageBox.Show("No deeper path available.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void searchField_TextChanged(object sender, TextChangedEventArgs e)
        {

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

        public FileSystemItem(string name, string FullPath, DateTime dateTime, bool isFolder = true, long size = 0)
        {
            Name = name;
            this.FullPath = FullPath;
            this.IsFolder = isFolder;
            DateModified = dateTime;
            Size = FileSizeFormatter.FormatFileSize(size);
        }
        public FileSystemItem()
        {

        }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string Size { get; set; }
        public bool IsFolder { get; set; }
        public DateTime DateModified { get; set; }
        public string Extension => Path.GetExtension(Name); 
    }

    public static class FileSizeFormatter
    {
        public static string FormatFileSize(long fileSize)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (fileSize > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(fileSize, max), order);

                max /= scale;
            }

            return "";
        }
    }


}
