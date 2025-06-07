using University_Schedule_Generator.Services;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch.Security;

namespace University_Schedule_Generator;

public class ApplicationContext : DbContext
{
    public DbSet<University> Universities { get; set; } = null!;
    public DbSet<Institute> Institutes { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Speciality> Specialities { get; set; } = null!;
    public DbSet<Lecture> Lectures { get; set; } = null!;
    public DbSet<Material> Materials { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Visit> Visits { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        Database.EnsureCreated(); // создаем базу данных при первом обращении
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}