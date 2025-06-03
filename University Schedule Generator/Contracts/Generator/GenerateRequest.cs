using System.ComponentModel;

namespace University_Schedule_Generator.Contracts.Generator
{
    public class GenerateRequest
    {
        [DefaultValue(300)] public int SpecialtiesCount { get; set; }
        [DefaultValue(3)] public int UniversityCount { get; set; }
        [DefaultValue(30)] public int InstitutionCount { get; set; }
        [DefaultValue(300)] public int DepartmentCount { get; set; }
        [DefaultValue(100)] public int GroupCount { get; set; }
        [DefaultValue(1000)] public int StudentCount { get; set; }
        [DefaultValue(100)] public int CourseCount { get; set; }
    }
}