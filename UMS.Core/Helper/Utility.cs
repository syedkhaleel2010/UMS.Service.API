using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Core.Helper
{
    public static class Utility
    {
        public static string GetIP()
        {
            var Host = Dns.GetHostName();
            return string.Join(",",
                              Dns.GetHostAddresses(Host)
                                 .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));

        }
        public class EntityChange
        {
            public string ColumnTitle { get; set; }
            public string NewValue { get; set; }
            public string OldValue { get; set; }
        }

        public static class EntityChangeHelper
        {
            // List of property names to ignore
            private static readonly HashSet<string> IgnoreProperties = new HashSet<string>
            {
                "CreatedOn", "ModifiedOn", "CreatedBy", "ModifiedBy"
            };

            private static readonly Dictionary<string, string> PropertyTitleMap = new Dictionary<string, string>();
            public static List<EntityChange> GetEntityChanges<T>(T oldEntity, T newEntity)
            {
                var changes = new List<EntityChange>();
                if (oldEntity == null || newEntity == null) return changes;

                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    var oldValue = prop.GetValue(oldEntity)?.ToString() ?? "";
                    var newValue = prop.GetValue(newEntity)?.ToString() ?? "";

                    if (IgnoreProperties.Contains(prop.Name))
                        continue; // Skip ignored properties

                    if (string.IsNullOrEmpty(newValue))
                        continue; // Skip request null/empty properties

                    // If both are valid numbers, compare numerically
                    if (IsValidNumber(oldValue) && IsValidNumber(newValue))
                    {
                        decimal oldNum = decimal.Parse(oldValue);
                        decimal newNum = decimal.Parse(newValue);
                        if (oldNum == newNum)
                            continue; // Skip if numerically equal
                    }
                    else
                    {
                        if (oldValue == newValue)
                            continue; // Skip if equal as string
                    }


                    if (oldValue != newValue)
                    {
                        changes.Add(new EntityChange
                        {
                            ColumnTitle = PropertyTitleMap.ContainsKey(prop.Name) ? PropertyTitleMap[prop.Name] : prop.Name,
                            OldValue = oldValue,
                            NewValue = newValue
                        });
                    }
                }
                return changes;
            }
            private static bool IsValidNumber(string input)
            {
                return decimal.TryParse(input, out _);
            }

            
        }
    }
}
