using System;

namespace GameUser.Shared.Wrapper
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