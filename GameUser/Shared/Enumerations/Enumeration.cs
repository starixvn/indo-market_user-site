using System.Collections.Generic;
using System.Reflection;

namespace GameUser.Shared.Enumerations
{
    public class Enumeration
    {
        protected Enumeration()
        {
        }

        protected Enumeration(string value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }


        public string DisplayName { get; set; }

        public string Value { get; set; }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration, new()
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = new T();
                var locatedValue = info.GetValue(instance) as T;

                if (locatedValue != null)
                {
                    yield return locatedValue;
                }
            }
        }  
    }
}
