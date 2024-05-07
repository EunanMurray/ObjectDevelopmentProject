using Microsoft.VisualStudio.TestTools.UnitTesting;
using Project1;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class ProjectTests
    {
        [TestMethod]
        public void TestCreateProject()
        {
            // Arrange
            var db = new ProjectTasks();
            var project = new Project
            {
                Name = "Test Project",
                Description = "A test project.",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10)
            };

            // Act
            db.Projects.Add(project);
            db.SaveChanges();

            // Assert
            var exists = db.Projects.Any(p => p.Name == "Test Project" && p.Description == "A test project.");
            Assert.IsTrue(exists, "Project was not added to the database.");
        }

        [TestMethod]
        public void TestDeleteProject()
        {
            // Arrange
            var db = new ProjectTasks();
            var project = new Project
            {
                Name = "Delete Me",
                Description = "Project to be deleted.",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };
            db.Projects.Add(project);
            db.SaveChanges();

            // Act
            db.Projects.Remove(project);
            db.SaveChanges();

            // Assert
            var exists = db.Projects.Any(p => p.Name == "Delete Me");
            Assert.IsFalse(exists, "Project was not successfully deleted.");
        }

        [TestMethod]
        public void TestAddTaskToProject()
        {
            // Arrange
            var db = new ProjectTasks();
            var project = new Project
            {
                Name = "Task Project",
                Description = "Project with task.",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5)
            };
            db.Projects.Add(project);
            db.SaveChanges();

            var task = new Task
            {
                Title = "New Task",
                Description = "Test adding a new task.",
                DueDate = DateTime.Now.AddDays(3),
                ProjectId = project.ProjectId
            };

            // Act
            db.Tasks.Add(task);
            db.SaveChanges();

            // Assert
            var exists = db.Tasks.Any(t => t.Title == "New Task" && t.ProjectId == project.ProjectId);
            Assert.IsTrue(exists, "Task was not added to the project.");
        }

        [TestMethod]
        public void TestUpdateTaskDetails()
        {
            // Arrange
            var db = new ProjectTasks();
            var project = new Project
            {
                Name = "Update Task Project",
                Description = "Project to update task.",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5)
            };
            db.Projects.Add(project);
            db.SaveChanges();

            var task = new Task
            {
                Title = "Update This Task",
                Description = "Original description",
                DueDate = DateTime.Now.AddDays(2),
                ProjectId = project.ProjectId
            };
            db.Tasks.Add(task);
            db.SaveChanges();

            // Act
            task.Description = "Updated description";
            db.SaveChanges();

            // Assert
            var updatedTask = db.Tasks.FirstOrDefault(t => t.TaskId == task.TaskId);
            Assert.AreEqual("Updated description", updatedTask.Description, "Task description was not updated.");
        }

        [TestMethod]
        public void TestLoadProjectsAndTasks()
        {
            // Arrange
            var db = new ProjectTasks();

            // Act
            var projects = db.Projects.Include("Tasks").ToList();

            // Assert
            Assert.IsNotNull(projects, "Failed to load projects and their tasks.");
            Assert.IsTrue(projects.All(p => p.Tasks != null), "Tasks are not loaded with projects.");
        }
        [TestMethod]
        public void TestUpdateProjectDetails()
        {
            // Arrange
            var db = new ProjectTasks();
            var project = new Project
            {
                Name = "Update Project",
                Description = "Original description",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10)
            };
            db.Projects.Add(project);
            db.SaveChanges();

            // Act
            project.Description = "Updated description";
            db.SaveChanges();

            // Assert
            var updatedProject = db.Projects.FirstOrDefault(p => p.ProjectId == project.ProjectId);
            Assert.AreEqual("Updated description", updatedProject.Description, "Project description was not updated.");
        }


    }
}
