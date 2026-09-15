using Microsoft.AspNetCore.Connections;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace DoctorMobileApp.CommonClass
{
    public class SqlHelper : IDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _logPath;
        public SqlHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            _logPath = configuration["AppSettings:LogPath"]
                ?? throw new InvalidOperationException("LogPath is missing in AppSettings.");
        }
        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        // 🔹 Error Logger
        public void LogError(Exception ex, string procedureName = "", SqlParameter[] parameters = null)
        {
            try
            {
                string logDirectory = Path.GetDirectoryName(_logPath);

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                using (StreamWriter sw = new StreamWriter(_logPath, true))
                {
                    sw.WriteLine("========== ERROR ==========");
                    sw.WriteLine($"Date: {DateTime.Now:dd-MM-yyyy hh:mm:ss tt}");

                    // 🔹 Stored Procedure / Method Name
                    if (!string.IsNullOrWhiteSpace(procedureName))
                        sw.WriteLine($"Procedure: {procedureName}");

                    // 🔹 Error Message
                    sw.WriteLine($"Message: {ex.Message}");

                    // 🔹 Inner Exception
                    if (ex.InnerException != null)
                        sw.WriteLine($"Inner Exception: {ex.InnerException.Message}");

                    // 🔹 Parameters
                    if (parameters != null && parameters.Length > 0)
                    {
                        sw.WriteLine("Parameters:");

                        foreach (var param in parameters)
                        {
                            string value = param.Value == null || param.Value == DBNull.Value
                                ? "NULL"
                                : param.Value.ToString();

                            sw.WriteLine($"   {param.ParameterName} = {value}");
                        }
                    }

                    // 🔹 Stack Trace
                    sw.WriteLine("StackTrace:");
                    sw.WriteLine(ex.StackTrace);

                    sw.WriteLine("====================================");
                    sw.WriteLine();
                }
            }
            catch
            {
                // Prevent logging failure crash
            }
        }
        // 🔹 Execute Non Query Async
        public async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(commandText, con))
                {
                    cmd.CommandType = commandType;
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    await con.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, commandText, parameters);
                throw;
            }
        }

        // 🔹 Execute Scalar Async
        public async Task<object> ExecuteScalarAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(commandText, con))
                {
                    cmd.CommandType = commandType;

                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    await con.OpenAsync();
                    return await cmd.ExecuteScalarAsync();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, commandText, parameters);
                throw;
            }
        }
        // 🔹 Execute DataTable Async
        public async Task<DataTable> ExecuteDataTableAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(commandText, con))
                {
                    cmd.CommandType = commandType;

                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    await con.OpenAsync();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        await Task.Run(() => da.Fill(dt)); // DataAdapter has no async
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, commandText, parameters);
                throw;
            }
        }
        // 🔹 Transaction Async
        public async Task<int> ExecuteTransactionAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(commandText, con, trans))
                    {
                        cmd.CommandType = commandType;

                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        int result = await cmd.ExecuteNonQueryAsync();
                        trans.Commit();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    LogError(ex, commandText, parameters);
                    throw;
                }
            }
        }
        public async Task<List<T>> QueryAsync<T>(string commandText, CommandType commandType, SqlParameter[] parameters = null) where T : new()
        {
            try
            {
                var list = new List<T>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(commandText, conn))
                {
                    cmd.CommandType = commandType;
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        while (await reader.ReadAsync())
                        {
                            T obj = new T();
                            foreach (var prop in props)
                            {
                                if (!reader.HasColumn(prop.Name) || reader[prop.Name] == DBNull.Value)
                                    continue;
                                var value = reader[prop.Name];
                                // Handle nullable types
                                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                prop.SetValue(obj, Convert.ChangeType(value, targetType));
                            }
                            list.Add(obj);
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                LogError(ex, commandText, parameters);
                throw;
            }
        }

        public async Task<List<object>> QueryMultipleAsync(string commandText, Func<SqlDataReader, object>[] mappers, SqlParameter[] parameters = null)
        {
            var results = new List<object>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(commandText, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            int i = 0;
            do
            {
                var list = new List<object>();
                var map = mappers.Length > i ? mappers[i] : null;
                while (await reader.ReadAsync())
                {
                    if (map != null)
                        list.Add(map(reader));
                }
                results.Add(list);
                i++;

            } while (await reader.NextResultAsync());

            return results;
        }

    }
}
