using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Project1
{
    // Project.cs
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<Task> Tasks { get; set; }

        public Project()
        {
            Tasks = new List<Task>();
        }
    }

    // Task.cs
    public class Task
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } // "Not Started", "In Progress", "Completed"
        public string Priority { get; set; } // "High", "Medium", "Low"    

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<Image> Images { get; set; } // Collection of Images

        public Task()
        {
            Images = new List<Image>();
        }
    }


    public class ProjectTasks : DbContext
    {
        public ProjectTasks(string dbName) : base("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\" + dbName + ".mdf;Integrated Security=True")
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory());
        }

        public ProjectTasks() : this("ProjectTasksV1")
        {

        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Image> Images { get; set; } // TODO: Fix this line
    }


    public class Image
    {
        public int ImageId { get; set; } // Primary key for the Image
        public byte[] Photo { get; set; }
        public int TaskId { get; set; } // Foreign key to Task
        public virtual Task Task { get; set; }
    }


}
