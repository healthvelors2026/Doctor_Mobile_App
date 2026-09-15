using Microsoft.Data.SqlClient;
using System.Data;

namespace DoctorMobileApp.CommonClass
{
    public interface IDbConnectionFactory
    {
        IDbConnection GetConnection();
        void LogError(Exception ex, string procedureName = "", SqlParameter[] parameters = null);
        Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null);
        Task<object> ExecuteScalarAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null);
        Task<DataTable> ExecuteDataTableAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null);
        Task<int> ExecuteTransactionAsync(string commandText, CommandType commandType, SqlParameter[] parameters = null);
        Task<List<T>> QueryAsync<T>(string commandText, CommandType commandType, SqlParameter[] parameters = null) where T : new();
        Task<List<object>> QueryMultipleAsync(string commandText, Func<SqlDataReader, object>[] mappers, SqlParameter[] parameters = null);
    }
}
