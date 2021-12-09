namespace GameUser.Shared.Enumerations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    public abstract class Enumeration : IComparable
    {
        public string DisplayName { get; }

        public string Value { get; }

        protected Enumeration()
        {
        }

        protected Enumeration(string value, string displayName) => (Value, DisplayName) = (value, displayName);    

        public override string ToString() => DisplayName;

        public static T FromDisplayName<T>(string displayName) where T : Enumeration, new()
        {
            var matchingItem = Parse<T, string>(displayName, "display name", item => item.DisplayName == displayName);
            return matchingItem;
        }

        public static T FromValue<T>(string value) where T : Enumeration, new()
        {
            var matchingItem = Parse<T, string>(value, "value", item => item.Value == value);
            return matchingItem;
        }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration
        {
            var fields = typeof(T).GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly);

            return fields.Select(f => f.GetValue(null)).Cast<T>();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherValue = obj as Enumeration;

            if (otherValue == null)
            {
                return false;
            }

            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = Value.Equals(otherValue.Value);

            return typeMatches && valueMatches;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public virtual int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return Value.CompareTo(((Enumeration)obj).Value);
        }

        protected static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration, new()
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);

            if (matchingItem == null)
            {
                var message = string.Format("'{0}' is not a valid {1} in {2}", value, description, typeof(T));
                throw new ArgumentOutOfRangeException(message);
            }

            return matchingItem;
        }
    }
}
