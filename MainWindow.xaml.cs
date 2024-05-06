using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager
{
    public partial class MainWindow : Window
    {
        private Stack<string> backHistory = new Stack<string>();
        private Stack<string> forwardHistory = new Stack<string>();
        bool orientation = false;
        bool searchOrientation = false;
        public ObservableCollection<DirectoryItem> Directories { get; set; }
        public ObservableCollection<FileSystemItem> FileSystem { get; set; }
        public ObservableCollection<FileSystemItem> search { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Directories = new ObservableCollection<DirectoryItem>();
            FileSystem = new ObservableCollection<FileSystemItem>();
            search = new ObservableCollection<FileSystemItem>();
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

        private void PopulateSearchListView()
        {
            listViewSearch.Items.Clear();
            foreach (var item in search)
            {
                listViewSearch.Items.Add(item);
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileSystemItem selectedItem;
            if (sender.Equals(listView))
            {
                selectedItem = (FileSystemItem)listView.SelectedItem;
            }
            else 
            {
                selectedItem = (FileSystemItem)listViewSearch.SelectedItem;
            }

            
            if (selectedItem != null && selectedItem.IsFolder)
            {
                backHistory.Push(selectedItem.FullPath);
                listView.Items.Clear();

        
                PopulateListView(selectedItem.FullPath);
            }

            if (selectedItem != null && !selectedItem.IsFolder && selectedItem.Extension == ".txt")
            {
                ChildWindow childWindow = new ChildWindow();
                childWindow.Owner = this;
                childWindow.DisplayTextFileContent(selectedItem.FullPath);
                childWindow.ShowDialog();
               
            }

            if(selectedItem != null && !selectedItem.IsFolder && selectedItem.Extension == ".pdf") 
            {
                ChildWindow childWindow = new ChildWindow();
                childWindow.Owner = this;
                childWindow.DisplayPdfContent(selectedItem.FullPath);
                
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

                    if (!orientation) {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderByDescending(item => item.Name));
                        PopulateListView();
                        orientation = true;
                    }
                    else
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderBy(item => item.Name));
                        PopulateListView();
                        orientation = false;
                    }
                    break;
                case "Size":
                    if (!orientation)
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderByDescending(item => item.Size));
                        PopulateListView();
                        orientation = true;
                    }
                    else
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderBy(item => item.Size));
                        PopulateListView();
                        orientation = false;
                    }
                    break;
                case "DateModified":
                    if (!orientation)
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderByDescending(item => item.DateModified));
                        PopulateListView();
                        orientation = true;
                    }else
                    {
                        FileSystem = new ObservableCollection<FileSystemItem>(FileSystem.OrderBy(item => item.DateModified));
                        PopulateListView();
                        orientation = false;
                    }
                    break;
            }
        }

        private void SearchNameColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            SearchSortListView("Name");
        }

        private void SearchSizeColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            SearchSortListView("Size");
        }

        private void SearchDateModifiedColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            SearchSortListView("DateModified");
        }

        private void SearchSortListView(string sortBy)
        {
            switch (sortBy)
            {
                case "Name":

                    if (!searchOrientation)
                    {
                        search = new ObservableCollection<FileSystemItem>(search.OrderByDescending(item => item.Name));
                        PopulateSearchListView();
                        searchOrientation = true;
                    }
                    else
                    {
                        search = new ObservableCollection<FileSystemItem>(search.OrderBy(item => item.Name));
                        PopulateSearchListView();
                        searchOrientation = false;
                    }
                    break;
                case "Size":
                    if (!searchOrientation)
                    {
                        search = new ObservableCollection<FileSystemItem>(search.OrderByDescending(item => item.Size));
                        PopulateSearchListView();
                        searchOrientation = true;
                    }
                    else
                    {
                        search = new ObservableCollection<FileSystemItem>(search.OrderBy(item => item.Size));
                        PopulateSearchListView();
                        searchOrientation = false;
                    }
                    break;
                case "DateModified":
                    if (!searchOrientation)
                    {
                        search = new ObservableCollection<FileSystemItem>(search.OrderByDescending(item => item.DateModified));
                        PopulateSearchListView();
                        searchOrientation = true;
                    }
                    else
                    {
                        search = new ObservableCollection<FileSystemItem>(search.OrderBy(item => item.DateModified));
                        PopulateSearchListView();
                        searchOrientation = false;
                    }
                    break;
            }
        }


        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            listViewSearch.Items.Clear();
            string text = searchField.Text;
            if(text == "")
            {
                MessageBox.Show("The searchfield is empty", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
            {
               await PopulateSearchView(drive.RootDirectory.Name , text);
            }

            foreach (FileSystemItem f in listViewSearch.Items)
                search.Add(f);

            if (listViewSearch.Items.Count == 0)
            {
                MessageBox.Show("No search result found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }



        }
        //TODO Go trough all filse
        private async Task PopulateSearchView(string path, string text)
        {
            try
            {
                await GetFiles(path, text);
                await GetDirs(path, text);
            }
            catch (UnauthorizedAccessException)
            {
                //MessageBox.Show("Access to the directory is denied.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DirectoryNotFoundException)
            {
                //MessageBox.Show("Directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }

        private async Task GetFiles(string path, string text)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                FileInfo fileInfo = new FileInfo(file);
                FileSystemItem subFile = new FileSystemItem(fileInfo.Name, fileInfo.FullName, fileInfo.LastWriteTime, false, fileInfo.Length);
                if (subFile != null && subFile.Name.Contains(text) || subFile.Extension.Contains(text))
                {
                    listViewSearch.Items.Add(subFile);
                }
            }
        }

        private async Task GetDirs(string path,string text)
        {
            foreach (var subDir in Directory.GetDirectories(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(subDir);
              
                
                FileSystemItem subDirItem = new FileSystemItem(directoryInfo.Name, directoryInfo.FullName, directoryInfo.LastWriteTime);
                if (subDirItem != null && subDirItem.Name.Contains(text))
                {
                    await GetFiles(subDirItem.FullPath, text);
                    listViewSearch.Items.Add(subDirItem);
                    await PopulateSearchView(subDirItem.FullPath, text);
                }
               
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
        public string Extension => System.IO.Path.GetExtension(Name); 
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
