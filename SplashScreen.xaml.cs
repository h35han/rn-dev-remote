using System;
using System.Windows;
using System.Windows.Controls;


namespace RN_Dev_Assistant
{
    public class ProjectSelectionChangedEventArgs : EventArgs
    {
        public ProjectReference ProjectReference { get; set; }
    }

    public class ProjectUnpinEventArgs : EventArgs
    {
        public ProjectReference ProjectReference { get; set; }
    }

    public partial class SplashScreen : Window
    {
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;

        public delegate void RejectedEventHandler(object sender, ProjectSelectionChangedEventArgs e);
        public event RejectedEventHandler SelectionChanged;

        public delegate void OpenDirectoryEventHandler(object sender, string e);
        public event OpenDirectoryEventHandler DirectoryOpened;

        public delegate void ProjectUnpinEventHandler(object sender, ProjectUnpinEventArgs e);
        public event ProjectUnpinEventHandler ProjectUnpined;

        protected virtual void OnSelectionChanged(object sender, ProjectSelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        protected virtual void OnDirectoryOpened(object sender, string e)
        {
            DirectoryOpened?.Invoke(this, e);
        }

        protected virtual void OnProjectUnpined(object sender, ProjectUnpinEventArgs e)
        {
            ProjectUnpined?.Invoke(this, e);
        }

        public SplashScreen()
        {
            InitializeComponent();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        public void OnProjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSelectionChanged(this,new ProjectSelectionChangedEventArgs
            {
                ProjectReference = ((ProjectReference)((ListBox)sender).SelectedValue)
            });
        }

        private void OnAddButtonPressed(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                OnDirectoryOpened(this, folderBrowserDialog.SelectedPath);
            }
        }

        private void OnCloseButtonPressed(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void OnUnpin(object sender, RoutedEventArgs e)
        {
            OnProjectUnpined(this, new ProjectUnpinEventArgs
            {
                ProjectReference = ((ProjectReference)((Button)sender).DataContext)
            });
        }

        private void OnOpenDirectory(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((ProjectReference)((Button)sender).DataContext).Path);
        }
    }
}
