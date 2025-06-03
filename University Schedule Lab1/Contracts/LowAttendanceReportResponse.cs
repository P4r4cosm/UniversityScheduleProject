namespace University_Schedule_Lab1.Contracts;

public class LowAttendanceReportResponse
{
    public Student StudentInfo { get; set; } // Полная информация о студенте из Redis
    public double AttendancePercentage { get; set; } // Рассчитанный процент посещения
    public DateTime ReportStartDate { get; set; } // Начало периода отчета
    public DateTime ReportEndDate { get; set; }   // Конец периода отчета
    public string SearchTerm { get; set; }       // Термин, по которому искали лекции

    // Конструктор для удобства
    public LowAttendanceReportResponse(Student student, double attendance, DateTime start, DateTime end, string term)
    {
        StudentInfo = student;
        AttendancePercentage = attendance;
        ReportStartDate = start;
        ReportEndDate = end;
        SearchTerm = term;
    }
}