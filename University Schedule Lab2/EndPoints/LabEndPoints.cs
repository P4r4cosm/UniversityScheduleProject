using Microsoft.AspNetCore.Mvc;
using University_Schedule_Lab1.Contracts;
using University_Schedule_Lab1.Services;

namespace University_Schedule_Lab1.EndPoints;

public static class LabEndPoints
{
    public static IEndpointRouteBuilder MapGetStudentsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/lab2", GetStudentsAsync);
        return endpoints;
    }

    private static async Task<IResult> GetStudentsAsync([AsParameters] FindAudienceRequest request,
        FindAudienceService service)
    {
        var result = await service.GetRequirements(request.CourseName,
             request.Year);
        return Results.Ok(result);
    }
}