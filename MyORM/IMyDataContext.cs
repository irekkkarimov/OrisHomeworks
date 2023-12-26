namespace MyORM;

public interface IMyDataContext
{
    bool Add<T>(T item);
    bool Update<T>(T item);
    bool Delete<T>(T item);
    List<T> Select<T>();
    T? SelectById<T>(int id) where T : class;
}