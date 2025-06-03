namespace University_Schedule_Generator.Interfaces.DataSaver;

public record SaveResult(bool Success, string Message, Exception? Error = null);

public interface IDataSaver<TData>
{
    Task<SaveResult> SaveAsync(TData data);
}