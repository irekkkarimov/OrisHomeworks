namespace MyORM;
using Npgsql;

public class MyDataContext : IMyDataContext
{
    public string ConnectionString { get; set; }

    public MyDataContext(string connectionString)
    {
        ConnectionString = connectionString;
    }
    
    public bool Add<T>(T item)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                connection.Open();
                var columns = typeof(T).GetProperties().Select(i => i.Name)
                    .Where(i => i != "Id" && i != "id");
                var values = "(" +
                             string.Join(",", typeof(T).GetProperties()
                                 .Skip(1)
                                 .Select(i => i.GetValue(item))
                                 .Select(i => $"'{i}'"))
                             + ")";
                var tableName = typeof(T).Name;
                var query = $"INSERT INTO {tableName}s ({string.Join(",", columns)}) VALUES {values}";
                var command = new NpgsqlCommand();
                command.CommandText = query;
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return true;
    }

    public bool Update<T>(T item)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                connection.Open();
                var columns = typeof(T).GetProperties().Select(i => i.Name)
                    .ToArray();
                var values = typeof(T).GetProperties()
                    .Select(i => i.GetValue(item).ToString())
                    .ToArray();

                var preQuery = new string[columns.Length];
                for (int i = 0; i < columns.Length; i++)
                {
                    if (int.TryParse(values[i], out _))
                        preQuery[i] = $"{columns[i]} = {values[i]}";
                    else
                        preQuery[i] = $"{columns[i]} = '{values[i]}'";
                }

                var id = typeof(T)
                    .GetProperties()
                    .First(i => i.Name == "Id" || i.Name == "id")
                    .GetValue(item);
                
                var tableName = typeof(T).Name;
                var query = $"UPDATE {tableName}s SET {string.Join(",", preQuery)} WHERE id = {id}";
                var command = new NpgsqlCommand();
                command.CommandText = query;
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return true;
    }

    public bool Delete<T>(T item)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                connection.Open();

                var id = typeof(T)
                    .GetProperties()
                    .First(i => i.Name == "Id" || i.Name == "id")
                    .GetValue(item);
                
                var tableName = typeof(T).Name;
                var query = $"DELETE FROM {tableName}s WHERE id = {id}";
                var command = new NpgsqlCommand();
                command.CommandText = query;
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return true;
    }

    public List<T> Select<T>()
    {
        var result = new List<T>();
        
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                connection.Open();
                var properties = typeof(T).GetProperties();
                var propertiesCount = properties.Length;
                
                var tableName = typeof(T).Name;
                var query = $"SELECT * FROM {tableName}s";
                var command = new NpgsqlCommand();
                command.CommandText = query;
                command.Connection = connection;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var newInstance = (T)Activator.CreateInstance(typeof(T))!;
                    for (var i = 0; i < propertiesCount; i++)
                        properties[i].SetValue(newInstance, reader[i]);
                    result.Add(newInstance);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return result;
    }

    public T? SelectById<T>(int id)
    where T : class
    {
        var result = new List<T>();
        
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            try
            {
                connection.Open();
                var properties = typeof(T).GetProperties();
                var propertiesCount = properties.Length;
                
                var tableName = typeof(T).Name;
                string query = $"SELECT * FROM {tableName}s WHERE id = {id}";
                var command = new NpgsqlCommand();
                command.CommandText = query;
                command.Connection = connection;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var newInstance = (T)Activator.CreateInstance(typeof(T))!;
                    for (var i = 0; i < propertiesCount; i++)
                        properties[i].SetValue(newInstance, reader[i]);
                    result.Add(newInstance);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return result.Any()
            ? result[0] 
            : null;
    }
}