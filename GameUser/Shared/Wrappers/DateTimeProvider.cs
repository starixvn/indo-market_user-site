using System;

namespace GameUser.Shared.Wrappers
{
    public interface IDateTimeProvider
    {
        DateTime Now();

        DateTime UtcNow();
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }

        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}