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
            try
            {
                var projects = db.Projects.ToList();
                projectComboBox.ItemsSource = projects;
                projectComboBox.DisplayMemberPath = "Name";
                projectComboBox.SelectedValuePath = "ProjectId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading projects: " + ex.Message);
            }
        }


        private void LoadProjectsAndTasks()
        {
            try
            {
                var projects = db.Projects.Include("Tasks").ToList(); // Fetches all projects with their tasks
                treeViewProjects.ItemsSource = projects;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading projects and tasks: " + ex.Message);
            }
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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error adding project: " + ex.Message);
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error adding task: " + ex.Message);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReLoadProjectsAndTasks();
                LoadProjectsIntoEditProjectComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing projects and tasks: " + ex.Message);
            }
        }

        private void ReLoadProjectsAndTasks()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error reloading projects and tasks: " + ex.Message);
            }
        }

        private List<byte[]> selectedImagesData = new List<byte[]>();

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting images: " + ex.Message);
            }
        }

        private void ListBoxTasksInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying images: " + ex.Message);
            }
        }



        private ImageSource ConvertByteArrayToImageSource(byte[] imageData)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error converting byte array to image source: " + ex.Message);
                return null;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting project or task: " + ex.Message);
            }
        }

        public class BinaryToImageConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is byte[] byteArray && byteArray.Length > 0)
                {
                    try
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
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error converting byte array to image source: " + ex.Message);
                        return null;
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
            try
            {
                var projects = db.Projects.ToList();
                editProjectComboBox.ItemsSource = projects;
                editProjectComboBox.DisplayMemberPath = "Name";
                editProjectComboBox.SelectedValuePath = "ProjectId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading projects for editing: " + ex.Message);
            }
        }

        private void EditProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (editProjectComboBox.SelectedItem is Project selectedProject)
                {
                    var tasks = db.Tasks.Where(t => t.ProjectId == selectedProject.ProjectId).ToList();
                    editTaskComboBox.ItemsSource = tasks;
                    editTaskComboBox.DisplayMemberPath = "Title";
                    editTaskComboBox.SelectedValuePath = "TaskId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading tasks for editing: " + ex.Message);
            }
        }

        private void EditTaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error loading task details for editing: " + ex.Message);
            }
        }


        private void UpdateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error updating task: " + ex.Message);
            }
        }

        // Search method
        // Updated SearchProjectsAndTasks method with null checks
        private void SearchProjectsAndTasks(string searchText)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error searching projects and tasks: " + ex.Message);
            }
        }

        // Search box text changed event
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = searchTextBox.Text;

                SearchProjectsAndTasks(searchText);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling search text changed event: " + ex.Message);
            }
        }

    }
}