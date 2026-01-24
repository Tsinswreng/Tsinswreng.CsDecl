
namespace Test1.Interfaces{
public interface IService
{
    void Execute();
    string GetResult();
    event EventHandler Completed;
}

public interface IRepository<T>
{
    T GetById(int id);
    void Save(T entity);
    IEnumerable<T> GetAll();
}

public interface ILogger
{
    void Log(string message);
    void LogError(string message, Exception ex);
}
}
