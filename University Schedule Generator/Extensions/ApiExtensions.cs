using System.Text;
using University_Schedule_Generator.Endpoints;
using University_Schedule_Generator.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace University_Schedule_Generator;

public static class ApiExtensions
{
    public static void AddDataBaseEndPoints(this IEndpointRouteBuilder app)
    {
        app
            .MapRedisEndpoints()
            .MapPostgresEndpoints()
            .MapElasticSearchEndpoints()
            .MapMongoEndpoints()
            .MapNeo4jEndpoints();
    }

    public static void AddGeneratorEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapGeneratorEndpoints();
    }
}