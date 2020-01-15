using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace RN_Dev_Assistant
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
        private System.Windows.Forms.MenuItem ExitOption = new System.Windows.Forms.MenuItem();
        private System.Windows.Forms.MenuItem OpenOption = new System.Windows.Forms.MenuItem();
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

        private SplashScreen splashScreen = new SplashScreen();

        private Process shellProcess = new Process();
        private Process CMDProcess = new Process();
        private Process waitForDeviceProcess = new Process();
        private Project mainProject;
        private string ADBFilePath;

        public MainWindow()
        {
            InitializeComponent();

            // Geting adb path

            ADBFilePath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), @"Android\sdk\platform-tools\adb.exe");

            mainProject = new Project();
            mainProject.Accepted += OnProjectAccepted;
            mainProject.Rejected += OnProjectRejected;

            var currentDirectory = Environment.CurrentDirectory;
            mainProject.SetProject(
                new ProjectReference
                {
                    Name = ProjectReference.GetName(currentDirectory),
                    Path = currentDirectory,
                    UpdatedDate = ProjectReference.GetUpdatedDate(currentDirectory)
                }
            );

            DataContext = mainProject;

            splashScreen.DataContext = mainProject;
            splashScreen.SelectionChanged += OnSplashScreenSelectionChanged;
            splashScreen.DirectoryOpened += OnSplashScreenDirectoryOpened;
            splashScreen.ProjectUnpined += OnProjectUnpined;

            CMDProcess.StartInfo.FileName = "cmd";
            CMDProcess.StartInfo.RedirectStandardError = true;
            CMDProcess.StartInfo.RedirectStandardInput = true;
            CMDProcess.StartInfo.RedirectStandardOutput = true;
            CMDProcess.StartInfo.UseShellExecute = false;
            CMDProcess.StartInfo.CreateNoWindow = true;
            CMDProcess.EnableRaisingEvents = true;
            CMDProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            CMDProcess.OutputDataReceived += new DataReceivedEventHandler(OnProcessDataReceived);
            CMDProcess.ErrorDataReceived += new DataReceivedEventHandler(OnProcessErrorReceived);
            CMDProcess.Exited += OnProcessExited;

            CMDProcess.Start();
            CMDProcess.BeginOutputReadLine();
            CMDProcess.BeginErrorReadLine();

            if (File.Exists(ADBFilePath))
            {
                shellProcess.StartInfo.FileName = ADBFilePath;
                shellProcess.StartInfo.Arguments = "shell";
                shellProcess.StartInfo.RedirectStandardError = true;
                shellProcess.StartInfo.RedirectStandardInput = true;
                shellProcess.StartInfo.RedirectStandardOutput = true;
                shellProcess.StartInfo.UseShellExecute = false;
                shellProcess.StartInfo.CreateNoWindow = true;
                shellProcess.EnableRaisingEvents = true;
                shellProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                shellProcess.OutputDataReceived += new DataReceivedEventHandler(OnProcessDataReceived);
                shellProcess.ErrorDataReceived += new DataReceivedEventHandler(OnProcessErrorReceived);
                shellProcess.Exited += OnDviceLost;

                waitForDeviceProcess.StartInfo.FileName = ADBFilePath;
                waitForDeviceProcess.StartInfo.Arguments = "wait-for-any-device";
                waitForDeviceProcess.StartInfo.UseShellExecute = false;
                waitForDeviceProcess.StartInfo.CreateNoWindow = true;
                waitForDeviceProcess.EnableRaisingEvents = true;
                waitForDeviceProcess.Exited += OnDeviceFound;

                waitForDeviceProcess.Start();
                device_indicatior.IsEnabled = false;
            }

            this.notifyIcon.Icon = Properties.Resources._16x16;
            this.notifyIcon.Visible = true;
            this.notifyIcon.Text = "RN Dev Helper";
            this.notifyIcon.Click += new System.EventHandler(this.NotifyIconClick);

            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { ExitOption, OpenOption });
            this.ExitOption.Index = 1;
            this.ExitOption.Text = "E&xit";
            this.ExitOption.Click += new EventHandler(this.OnExit);
            this.OpenOption.Index = 0;
            this.OpenOption.Text = "Open";
            this.OpenOption.Click += new EventHandler(this.OnAbout);

            this.notifyIcon.ContextMenu = this.contextMenu;
        }

        //
        // FUNCTIONS
        //

        private void RunCMDCommand(string command)
        {
            CMDProcess.StandardInput.WriteLine(command);
        }

        private void RunShellCommand(string command)
        {
            shellProcess.StandardInput.WriteLine(command);
        }

        //
        // EVENT HANDLERS
        //

        private void OnProjectRejected(object sender, EventArgs e)
        {
            Console.WriteLine("rejected");
        }

        private void OnProjectAccepted(object sender, EventArgs e)
        {
            //Get to the top
            splashScreen.ProjectContainer.SelectedIndex = 0;
        }

        private void OnSplashScreenSelectionChanged(object sender, ProjectSelectionChangedEventArgs e)
        {
            mainProject.CurrentProject = e.ProjectReference;
            Directory.SetCurrentDirectory(mainProject.CurrentProject.Path);
            CMDProcess.Kill();
        }

        private void OnSplashScreenDirectoryOpened(object sender, string path)
        {
            if (Directory.Exists(path))
            {
                mainProject.SetProject(
                    new ProjectReference
                    {
                        Name = ProjectReference.GetName(path),
                        Path = path,
                        UpdatedDate = ProjectReference.GetUpdatedDate(path)
                    }
                );
            }
        }

        private void OnProjectUnpined(object sender, ProjectUnpinEventArgs e)
        {
            mainProject.RemoveProject(e.ProjectReference);
        }

        private void OnDeviceFound(object sender, EventArgs e)
        {
            try
            {
                //  Start the main adb shell process and start reading I/O
                shellProcess.Start();
                shellProcess.BeginOutputReadLine();
            }
            catch (Exception)
            {
                throw;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                device_indicatior.IsEnabled = true;
            }));
        }

        private void OnDviceLost(object sender, EventArgs e)
        {
            try
            {
                //  Start the waiting process

                waitForDeviceProcess.Start();

                //  Stop the existing shell process I/O

                shellProcess.CancelOutputRead();
            }
            catch (Exception)
            {
                throw;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                device_indicatior.IsEnabled = false;
            }));
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            Console.WriteLine("cmd exited");
            CMDProcess.Start();
        }

        private void OnProcessErrorReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void OnProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void OnAbout(object sender, EventArgs e)
        {
            splashScreen.Show();
        }

        private void OnExit(object sender, EventArgs e)
        {

            this.mainProject.SaveRecentProjects();
            this.Close();
            this.splashScreen.Close();

            shellProcess.Exited -= OnDviceLost;
            waitForDeviceProcess.Exited -= OnDeviceFound;
            CMDProcess.Exited -= OnProcessExited;

            CMDProcess.Kill();

            if (waitForDeviceProcess.HasExited)
            {
                shellProcess.Kill();
            }
            else
            {
                waitForDeviceProcess.Kill();
            }

            Process.GetCurrentProcess().Kill();
            App.Current.Shutdown();
        }

        private void NotifyIconClick(object sender, EventArgs e)
        {
            this.Focus();
            this.Show();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            string[] path = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (Directory.Exists(path[0]))
            {
                mainProject.SetProject(
                    new ProjectReference
                    {
                        Name = ProjectReference.GetName(path[0]),
                        Path = path[0],
                        UpdatedDate = ProjectReference.GetUpdatedDate(path[0])
                    }
                );
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void OnToggleDeviceMenuButtonClick(object sender, RoutedEventArgs e)
        {
            if(waitForDeviceProcess.HasExited)
                RunShellCommand("input keyevent 82");
        }

        private void OnReloadAppButtonClick(object sender, RoutedEventArgs e)
        {
            if (waitForDeviceProcess.HasExited)
                RunShellCommand("input text 'RR'");
        }

        private void OnRunOnDeviceButtonClick(object sender, RoutedEventArgs e)
        {
            if (!mainProject.IsEmpty)
                RunCMDCommand("start");
        }
    }
}
