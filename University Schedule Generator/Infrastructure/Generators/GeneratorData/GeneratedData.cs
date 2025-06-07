namespace University_Schedule_Generator.Infrastructure.Generators.Data;

public class GeneratedData
{
    public List<Speciality> Specialities { get; set; } = new();
    public List<University> Universities { get; set; } = new();
    public List<Institute> Institutes { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<Group> Groups { get; set; } = new();
    public List<Student> Students { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
    public List<Lecture> Lectures { get; set; } = new();
    public List<Material> Materials { get; set; } = new(); // Обычные материалы для PG
    public List<Schedule> Schedules { get; set; } = new();
    public List<Visit> Visits { get; set; } = new();
    public List<MaterialElastic> MaterialElastics { get; set; } = new(); // Материалы для Elastic
}