using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace RN_Dev_Assistant
{
    class Project: INotifyPropertyChanged
    {
        public ObservableCollection<ProjectReference> ProjectStack { get; }

        public bool IsEmpty { get { return ProjectStack.Count == 0; } }

        private ProjectReference currentProject;
        public ProjectReference CurrentProject
        {
            get
            {
                return currentProject;
            }
            set
            {
                currentProject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentProject"));
            }
        }

        public delegate void AcceptedEventHandler(object sender, EventArgs e);
        public delegate void RejectedEventHandler(object sender, EventArgs e);

        public event AcceptedEventHandler Accepted;
        public event RejectedEventHandler Rejected;
        public event PropertyChangedEventHandler PropertyChanged;

        public Project()
        {
            ProjectStack = new ObservableCollection<ProjectReference>();

            // Get all saved projects
            foreach (var rawString in Properties.Settings.Default.RecentProjectReference)
            {
                ProjectStack.Add(StringToProject(rawString));
            }
        }

        public void SetProject(ProjectReference project)
        {
            if (CheckForGradle(project.Path))
            {
                try
                {
                    ProjectStack.Remove(ProjectStack.Single<ProjectReference>(x => x.Path == project.Path));
                }
                catch
                {
                    // Terat as a New Project
                }
                finally
                {
                    CurrentProject = project;

                    ProjectStack.Insert(0, project);

                    OnAccepted();
                }
            }
            else
            {
                OnRejected();
            }
        }

        public void RemoveProject(ProjectReference project)
        {
            ProjectStack.Remove(project);
        }

        public void SaveRecentProjects()
        {
            Properties.Settings.Default.Reset();
            foreach (var project in ProjectStack)
            {
                Properties.Settings.Default.RecentProjectReference.Add(ProjectToString(project));
            }
            Properties.Settings.Default.Save();
        }

        private bool CheckForGradle(string path)
        {
            return File.Exists(path + @"\android\gradlew.bat");
        }

        private string ProjectToString(ProjectReference project)
        {
            return project.Name + "," + project.Path + "," + project.UpdatedDate;
        }

        private ProjectReference StringToProject(string rawString)
        {
            string[] parts = rawString.Split(',');
            return new ProjectReference
            {
                Name = parts[0],
                Path = parts[1],
                UpdatedDate = parts[2]
            };
        }

        protected virtual void OnAccepted()
        {
            Accepted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRejected()
        {
            Rejected?.Invoke(this, EventArgs.Empty);
        }
    }
}
