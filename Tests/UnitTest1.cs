using Microsoft.VisualStudio.TestTools.UnitTesting;
using Project1;  // Import your namespace where Project and Task are defined
using System;

namespace Tests
{
    [TestClass]
    public class ProjectTests
    {
        [TestMethod]
        public void TestCreateProject()
        {
            // Arrange
            var db = new ProjectTasks(); // Ideally, use an in-memory database here
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
            Assert.AreEqual(1, db.Projects.Count());  // Assuming db.Projects.Count() correctly retrieves the count of projects
        }
    }
}
