using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace DoctorMobileApp.CommonClass
{
    public static class DataRowExtensions
    {
        public static int ToInt(this DataRow row, string column)
        {
            return row[column] != DBNull.Value ? Convert.ToInt32(row[column]) : 0;
        }
        public static string ToStr(this DataRow row, string column)
        {
            return row[column] != DBNull.Value ? row[column].ToString() : string.Empty;
        }
        public static bool HasColumn(this IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        public static List<T> ToList<T>(this DataTable dt) where T : new()
        {
            var list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                T obj = new T();

                foreach (DataColumn col in dt.Columns)
                {
                    var prop = typeof(T).GetProperty(col.ColumnName);

                    if (prop != null && row[col] != DBNull.Value)
                    {
                        Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        object value = row[col];

                        // Special handling for DateTime
                        if (targetType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(value.ToString(), out DateTime dtValue))
                                value = dtValue;
                            else
                                value = null;
                        }
                        else
                        {
                            value = Convert.ChangeType(value, targetType);
                        }

                        prop.SetValue(obj, value);
                    }
                }

                list.Add(obj);
            }
            return list;
        }


    }
}
