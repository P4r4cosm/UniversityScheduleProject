using Microsoft.EntityFrameworkCore;
using University_Schedule_Generator.Infrastructure.Generators.Data;
using University_Schedule_Generator.Interfaces.DataSaver;

namespace University_Schedule_Generator.Services.DataSavers;

public class PostgresDataSaver:IDataSaver<GeneratedData>
{
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;
    private readonly ILogger<PostgresDataSaver> _logger;
    public async Task<SaveResult> SaveAsync(GeneratedData data)
    {
        _logger.LogInformation("Saving data to PostgreSQL via EF Core...");
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            // Используем этот локальный 'dbContext' для всех операций EF
            await dbContext.Specialities.AddRangeAsync(data.Specialities);
            await dbContext.Universities.AddRangeAsync(data.Universities);
            await dbContext.Institutes.AddRangeAsync(data.Institutes);
            await dbContext.Departments.AddRangeAsync(data.Departments);
            await dbContext.Groups.AddRangeAsync(data.Groups);
            await dbContext.Students.AddRangeAsync(data.Students);
            await dbContext.Courses.AddRangeAsync(data.Courses);
            await dbContext.Lectures.AddRangeAsync(data.Lectures);
            await dbContext.Materials.AddRangeAsync(data.Materials);
            await dbContext.Schedules.AddRangeAsync(data.Schedules);
            await dbContext.Visits.AddRangeAsync(data.Visits);
            // Вызываем SaveChangesAsync для созданного контекста
            await dbContext.SaveChangesAsync();
            // // //обновляем значения для валидных id
            // data.Students=dbContext.Students.ToList();
            // data.Materials =dbContext.Materials.ToList();
            // data.Schedules = dbContext.Schedules.ToList();
            // data.Visits = dbContext.Visits.ToList();
            // data.Institutes = dbContext.Institutes.ToList();
            // data.Departments = dbContext.Departments.ToList();
            // data.Groups = dbContext.Groups.ToList();
            // data.Courses = dbContext.Courses.ToList();
            // data.Lectures = dbContext.Lectures.ToList();
            // data.Universities = dbContext.Universities.ToList();
            // data.Specialities = dbContext.Specialities.ToList();
        }// <-- Здесь 'dbContext' автоматически уничтожается (Dispose)
        _logger.LogInformation("PostgreSQL save complete.");
        return new SaveResult(true, "PostgreSQL save complete.");
    }
    public PostgresDataSaver(IDbContextFactory<ApplicationContext> dbContextFactory, ILogger<PostgresDataSaver> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
}

