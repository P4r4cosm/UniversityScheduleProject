using Microsoft.AspNetCore.CookiePolicy;
using University_Schedule_Lab1;
using University_Schedule_Lab1.EndPoints;
using University_Schedule_Lab1.Repositories;
using University_Schedule_Lab1.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// *** 1. Добавьте сервисы CORS ***
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; // Имя для политики
services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            // Укажите ТОЧНЫЙ URL вашего ШЛЮЗА
            policy.WithOrigins("https://localhost:7249")
                .AllowAnyHeader() 
                .AllowAnyMethod(); 

        });
});
builder.Logging.AddConsole();
// Swagger/OpenAPI
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Lab1 Service API", Version = "v1" });
});


//добавляем базы
services.AddPostgres(builder.Configuration);
services.AddNeo4j(builder.Configuration);
services.AddRedis(builder.Configuration);
services.AddMongoDb(builder.Configuration);
services.AddElastic(builder.Configuration);



//добавляем репозитории 
services.AddScoped<ElasticMaterialsRepository>();
services.AddScoped<LectureRepository>();
services.AddScoped<ScheduleRepository>();
services.AddScoped<VisitsRepository>();
services.AddScoped<StudentRepository>();
//добавляем сервис
services.AddScoped<FindBadStudentsService>();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
// Включаем middleware для Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "University Schedule API V1");
    options.RoutePrefix = ""; // Доступ по /
});

app.MapGetStudentsEndpoint();

app.Run();