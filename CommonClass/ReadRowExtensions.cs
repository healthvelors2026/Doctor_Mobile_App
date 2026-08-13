using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Text.Json;

namespace DoctorMobileApp.CommonClass
{
    public static class ReadRowExtensions
    {
        public static T MapToClass<T>(SqlDataReader reader) where T : new()
        {
            T obj = new T();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                object value = reader.GetValue(i);

                if (value == DBNull.Value)
                    continue;

                var prop = typeof(T).GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop == null || !prop.CanWrite)
                    continue;

                try
                {
                    Type propType = prop.PropertyType;

                    // ✅ Handle Nullable<T>
                    Type underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

                    // ✅ Handle List<string>
                    if (propType == typeof(List<string>))
                    {
                        var strValue = value.ToString();

                        List<string> list;

                        // Detect JSON array
                        if (!string.IsNullOrWhiteSpace(strValue) && strValue.TrimStart().StartsWith("["))
                        {
                            list = JsonSerializer.Deserialize<List<string>>(strValue);
                        }
                        else
                        {
                            // Assume comma-separated
                            list = strValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(x => x.Trim())
                                           .ToList();
                        }

                        prop.SetValue(obj, list);
                    }
                    else
                    {
                        // ✅ Handle normal types safely
                        var safeValue = Convert.ChangeType(value, underlyingType);
                        prop.SetValue(obj, safeValue);
                    }
                }
                catch
                {
                    // Optional: log error here (don't crash mapping)
                    // Example: Console.WriteLine($"Mapping failed for {columnName}");
                }
            }

            return obj;
        }
        public static T? ReadSingle<T>(List<object> result, int index) where T : class
        {
            if (result.Count <= index)
                return null;

            return (result[index] as List<object>)?
                .Cast<T>()
                .FirstOrDefault();
        }
        public static List<T> ReadList<T>(List<object> result, int index)
        {
            if (result.Count <= index)
                return new List<T>();

            return (result[index] as List<object>)?
                .Cast<T>()
                .ToList() ?? new List<T>();
        }
    }

}
