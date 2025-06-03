using Microsoft.AspNetCore.Mvc;
using University_Schedule_Lab1.Contracts;
using University_Schedule_Lab1.Services;

namespace University_Schedule_Lab1.EndPoints;

public static class LabEndPoints
{
    public static IEndpointRouteBuilder MapGetStudentsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/lab1", GetStudentsAsync);
        return endpoints;
    }

    private static async Task<IResult> GetStudentsAsync([AsParameters] FindBadStudentsRequest request,
        FindBadStudentsService service)
    {
        var result = await service.GetStudents(request.SearchText, request.StartDate, request.EndDate);
        return Results.Ok(result);
    }
}