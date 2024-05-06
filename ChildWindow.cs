using System.Windows;
using System;
using System.IO;
using System.Diagnostics;

namespace FileManager
{
    public partial class ChildWindow : Window
    {
        public ChildWindow()
        {
            InitializeComponent();
        }


        public void DisplayTextFileContent(string filePath)
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

        public void DisplayPdfContent(string filePatch)
        {
            try 
            {
                Process.Start(filePatch);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DisplayName(string name)
        {
            txtFileContent.Text = name;
        }

    }
}