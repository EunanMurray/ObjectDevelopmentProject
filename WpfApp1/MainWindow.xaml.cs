using System.Windows;
using Project1;
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
using Newtonsoft.Json;

namespace Project1
{
    public partial class MainWindow : Window
    {
        private ProjectTasks db;

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
                RefreshComboBoxes();

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
                db.SaveChanges();
                RefreshComboBoxes();

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

        //Button to refresh the projects and tasks
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

        //Button to refresh the projects and tasks and reload UI
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

                //I'm forgetting a function
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reloading projects and tasks: " + ex.Message);
            }
        }

        //List of byte arrays to store inputted images
        private List<byte[]> selectedImagesData = new List<byte[]>();


        //Button to select images function. Used to save them
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
                    // Clear previously selected images out of the array
                    selectedImagesData.Clear();

                    // Load the new images into the array
                    foreach (var filePath in openFileDialog.FileNames)
                    {
                        var imageData = File.ReadAllBytes(filePath);
                        selectedImagesData.Add(imageData);
                    }

                    // For preview display first image
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

       //To view the images, cant remeber why I named it this
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


        //Converting the image from bytes back to images
        private ImageSource ConvertByteArrayToImageSource(byte[] imageData)
        {
            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; //Needed or the image would "lock"? Not sure why but stack overflow said so
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

                // Make sure that the select item is in fact a project
                if (selectedItem is Project selectedProject)
                {
                    // DONE:  Possibly might implement a cascade delete so I can delete entire projects 
                    db.Projects.Remove(selectedProject);
                }
                // Check if the selected item is a Task
                else if (selectedItem is Task selectedTask)
                {
                    db.Tasks.Remove(selectedTask);
                }
                else
                {
                    // If the selected item is neither a Task nor a Project throw this error
                    MessageBox.Show("Please select a Project or Task to delete.");
                    return;
                }
                
                //save the changes
                db.SaveChanges();

                // Refresh everything
                ReLoadProjectsAndTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting project or task: " + ex.Message);
            }
        }

        //Image to Binary converter
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

        //Loading Projects into the EditrojectComboBox (This is defunct as it was painful to try make work)
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

        //With above
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


        private void RefreshComboBoxes()
        {
            
            if (editTaskComboBox.SelectedItem is Task selectedTask)
            {
                editTaskStatusComboBox.SelectedValue = selectedTask.Status;

                editTaskPriorityComboBox.SelectedValue = selectedTask.Priority;
            }
            else
            {
       
                editTaskStatusComboBox.SelectedIndex = -1;
                editTaskPriorityComboBox.SelectedIndex = -1;
            }
        }

        //This is the used edit function for tasks instead
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

                    RefreshComboBoxes();
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
                    taskToUpdate.DueDate = DateTime.Parse(editTaskDueDateTextBox.Text);
                    taskToUpdate.Status = editTaskStatusComboBox.Text;
                    taskToUpdate.Priority = editTaskPriorityComboBox.Text;

                    db.SaveChanges();
                    MessageBox.Show("Task updated successfully.");
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating task: " + ex.Message);
            }
        }

        // Search method for projects
        private void SearchProjectsAndTasks(string searchText)
        {
            try
            {
                if (treeViewProjects != null && treeViewProjects.Items != null)
                {
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        treeViewProjects.Items.Filter = null;
                    }
                    else
                    {
                        treeViewProjects.Items.Filter = item =>
                        {

                            if (item is Project project)
                            {

                                return project.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       project.Tasks.Any(task => task.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                            }

                            else if (item is Task task)
                            {

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

        //Refresh button for edit
        private void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReLoadProjectsAndTasks(); // Assuming this method refreshes all necessary data
                MessageBox.Show("All data has been refreshed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing all data: " + ex.Message);
            }
        }

        //button for exporting data as a json
        private void ExportDataToJson()
        {
            try
            {
                using (var db = new ProjectTasks())
                {
                    var projects = db.Projects.Include("Tasks").ToList(); 
                    foreach (var project in projects)
                    {
                        foreach (var task in project.Tasks)
                        {
                            task.Images = null; 
                        }
                    }

                    string json = JsonConvert.SerializeObject(projects, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore 
                        });

                    File.WriteAllText(@"..\..\ExportedData.json", json);
                    MessageBox.Show("Data exported successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data: " + ex.Message);
            }
        }


        private void ExportDataToJson_Click(object sender, RoutedEventArgs e)
        {
            ExportDataToJson();
        }


    }
}