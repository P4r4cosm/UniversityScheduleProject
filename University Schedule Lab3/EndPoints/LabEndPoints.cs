using Microsoft.AspNetCore.Mvc;
using University_Schedule_Lab1.Contracts;
using University_Schedule_Lab1.Services;

namespace University_Schedule_Lab1.EndPoints;

public static class LabEndPoints
{
    public static IEndpointRouteBuilder MapGetGroupReportEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/lab3", GetGroupReportAsync);
        return endpoints;
    }

    private static async Task<IResult> GetGroupReportAsync([AsParameters] GetGroupReportRequest request,
        GroupReportService service)
    {
        var result = await service.GetReport(request.GroupName);
        return Results.Ok(result);
    }
}