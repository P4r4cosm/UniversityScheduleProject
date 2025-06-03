using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Lab1.Contracts;

public record GetGroupReportRequest
{
    [Required]
    [DefaultValue("ДЕФ-02-24")]
    public string? GroupName { get; set; }
}