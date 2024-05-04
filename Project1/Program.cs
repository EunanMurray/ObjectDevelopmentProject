using System;
using System.Collections.Generic;
using System.Linq;
using Project1;

namespace DataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create db model
            ProjectTasks db = new ProjectTasks("ProjectTasksV1");

            using (db)
            {
                // Create a task
                Task task1 = new Task() { Title = "Diagrams", Description = "Plot out diagrams for my Project", DueDate = new DateTime(2024, 03, 13), Status = "In Progress", Priority = "High" };

                // create an Image and associate it with the task
                Image task1Image = new Image() { Photo = new byte[] {  } };
                task1.Images.Add(task1Image);

                // Create a project and add the task to it
                Project p1 = new Project() { Name = "WebDesignProject", Description = "Project to be completed for Web design", StartDate = new DateTime(2024, 03, 11), EndDate = new DateTime(2024, 04, 10) };
                p1.Tasks.Add(task1);

                db.Projects.Add(p1);

                // Save changes to the database
                db.SaveChanges();
            }
        }
    }
}
