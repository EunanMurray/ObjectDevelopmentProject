using System.Windows;
using Project1; // Your namespace with Project and Task classes defined
using System.Windows.Controls;
using WpfApp1;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Project1
{
    public partial class MainWindow : Window
    {
        private ProjectTasks db; // Handles your database operations

        public MainWindow()
        {
            InitializeComponent();
            listBoxTasksInfo.MouseUp += ListBoxTasksInfo_MouseUp;
            db = new ProjectTasks();
            LoadProjectsIntoComboBox();
            LoadProjectsAndTasks();
            LoadProjectsIntoEditProjectComboBox();
        }

        private void LoadProjectsIntoComboBox()
        {
            var projects = db.Projects.ToList();
            projectComboBox.ItemsSource = projects;
            projectComboBox.DisplayMemberPath = "Name";
            projectComboBox.SelectedValuePath = "ProjectId";
        }


        private void LoadProjectsAndTasks()
        {
            var projects = db.Projects.Include("Tasks").ToList(); // Fetches all projects with their tasks
            treeViewProjects.ItemsSource = projects;
        }

        private void TreeViewProjects_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue;
            if (selectedItem is Task selectedTask)
            {
                // If a task is selected, display its details in the listBoxTasksInfo
                listBoxTasksInfo.ItemsSource = new List<Task> { selectedTask };
            }
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var newProject = new Project
            {
                Name = projectNameTextBox.Text,
                Description = projectDescriptionTextBox.Text,
                StartDate = DateTime.Parse(projectStartDateTextBox.Text),
                EndDate = DateTime.Parse(projectEndDateTextBox.Text)
            };

            db.Projects.Add(newProject);
            db.SaveChanges();
            ReLoadProjectsAndTasks();

            //Clear TextBoxes
            projectNameTextBox.Text = string.Empty;
            projectDescriptionTextBox.Text = string.Empty;
            projectStartDateTextBox.Text = string.Empty;
            projectEndDateTextBox.Text = string.Empty;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProject = (Project)projectComboBox.SelectedItem;
            var newTask = new Task
            {
                Title = taskTitleTextBox.Text,
                Description = taskDescriptionTextBox.Text,
                DueDate = DateTime.Parse(taskDueDateTextBox.Text),
                Status = taskStatusComboBox.Text,
                Priority = taskPriorityComboBox.Text,
                ProjectId = selectedProject.ProjectId
            };

            db.Tasks.Add(newTask);
            db.SaveChanges(); // Necessary to get a TaskId for the new task

            foreach (var imageData in selectedImagesData)
            {
                var image = new Project1.Image
                {
                    Photo = imageData,
                    TaskId = newTask.TaskId
                };
                db.Images.Add(image);
            }
            db.SaveChanges();

            ReLoadProjectsAndTasks();

            // Reset the UI
            taskTitleTextBox.Text = "";
            taskDescriptionTextBox.Text = "";
            taskDueDateTextBox.Text = "";
            taskStatusComboBox.SelectedIndex = -1;
            taskPriorityComboBox.SelectedIndex = -1;
            selectedImagesData.Clear();
            selectedImagePreview.Source = null;
        }




        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ReLoadProjectsAndTasks();
            LoadProjectsIntoEditProjectComboBox();
        }

        private void ReLoadProjectsAndTasks()
        {
            // Clear the current items
            treeViewProjects.ItemsSource = null;

            // Fetch the updated list of projects and tasks from the database
            var projects = db.Projects.Include("Tasks").ToList();

            // Reset the TreeView to the new list of projects and tasks
            treeViewProjects.ItemsSource = projects;

            LoadProjectsAndTasks();
            LoadProjectsIntoComboBox();

            //I'm forgettinga function
        }

        private List<byte[]> selectedImagesData = new List<byte[]>();

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                Multiselect = true // Enable multiple file selection
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                // Clear previously selected images
                selectedImagesData.Clear();

                foreach (var filePath in openFileDialog.FileNames)
                {
                    var imageData = File.ReadAllBytes(filePath);
                    selectedImagesData.Add(imageData);
                }

                // For preview, display the first image
                if (selectedImagesData.Any())
                {
                    var firstImage = new BitmapImage(new Uri(openFileDialog.FileNames.First()));
                    selectedImagePreview.Source = firstImage;
                }
            }
        }

        private void ListBoxTasksInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (listBoxTasksInfo.SelectedItem is Task selectedTask)
            {
                var images = db.Images.Where(img => img.TaskId == selectedTask.TaskId).ToList();
                var imageSources = images.Select(img => ConvertByteArrayToImageSource(img.Photo)).ToList();

                if (imageSources.Any())
                {
                    var imageViewer = new ImageViewer(imageSources);
                    imageViewer.Show();
                }
            }
        }



        private ImageSource ConvertByteArrayToImageSource(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // Important to avoid locking the file
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = treeViewProjects.SelectedItem;

            // Check if the selected item is a Project
            if (selectedItem is Project selectedProject)
            {
                // Possibly might implement a cascade delete so I can delete entire projects (Done)
                db.Projects.Remove(selectedProject);
            }
            // Check if the selected item is a Task
            else if (selectedItem is Task selectedTask)
            {
                db.Tasks.Remove(selectedTask);
            }
            else
            {
                // If the selected item is neither a Task nor a Project
                MessageBox.Show("Please select a Project or Task to delete.");
                return;
            }

            // Persist the changes to the database
            db.SaveChanges();

            // Refresh the list to show the updated data
            ReLoadProjectsAndTasks();
        }

        public class BinaryToImageConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is byte[] byteArray && byteArray.Length > 0)
                {
                    using (var ms = new MemoryStream(byteArray))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        return image;
                    }
                }
                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        private void LoadProjectsIntoEditProjectComboBox()
        {
            var projects = db.Projects.ToList();
            editProjectComboBox.ItemsSource = projects;
            editProjectComboBox.DisplayMemberPath = "Name";
            editProjectComboBox.SelectedValuePath = "ProjectId";
        }

        private void EditProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editProjectComboBox.SelectedItem is Project selectedProject)
            {
                var tasks = db.Tasks.Where(t => t.ProjectId == selectedProject.ProjectId).ToList();
                editTaskComboBox.ItemsSource = tasks;
                editTaskComboBox.DisplayMemberPath = "Title";
                editTaskComboBox.SelectedValuePath = "TaskId";
            }
        }

        private void EditTaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editTaskComboBox.SelectedItem is Task selectedTask)
            {
                editTaskTitleTextBox.Text = selectedTask.Title;
                editTaskDescriptionTextBox.Text = selectedTask.Description;
                editTaskDueDateTextBox.Text = selectedTask.DueDate.ToString("yyyy-MM-dd");
                editTaskStatusComboBox.Text = selectedTask.Status;
                editTaskPriorityComboBox.Text = selectedTask.Priority;
            }
        }


        private void UpdateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (editTaskComboBox.SelectedItem is Task taskToUpdate)
            {
                taskToUpdate.Title = editTaskTitleTextBox.Text;
                taskToUpdate.Description = editTaskDescriptionTextBox.Text;
                taskToUpdate.DueDate = DateTime.Parse(editTaskDueDateTextBox.Text); // Need to Add error handling
                taskToUpdate.Status = editTaskStatusComboBox.Text;
                taskToUpdate.Priority = editTaskStatusComboBox.Text;

                db.SaveChanges();
                MessageBox.Show("Task updated successfully.");
                // Optionally, refresh your UI here to reflect the updated task details
            }
        }

        // Search method
        // Updated SearchProjectsAndTasks method with null checks
        private void SearchProjectsAndTasks(string searchText)
        {
            // Check if treeViewProjects is not null and its Items collection is not null
            if (treeViewProjects != null && treeViewProjects.Items != null)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // Reset the TreeView to display all projects and tasks
                    treeViewProjects.Items.Filter = null;
                }
                else
                {
                    // Filter projects and tasks based on search text
                    treeViewProjects.Items.Filter = item =>
                    {
                        // Check if item is a Project
                        if (item is Project project)
                        {
                            // Filter projects by name
                            return project.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   project.Tasks.Any(task => task.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                        }
                        // Check if item is a Task
                        else if (item is Task task)
                        {
                            // Filter tasks by title
                            return task.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        return false;
                    };
                }
            }
        }

        // Search box text changed event
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            string searchText = searchTextBox.Text;

            SearchProjectsAndTasks(searchText);
        }

    }
}