using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameUser.Shared.Wrappers
{
    public interface IHttpRequestWrapper
    {
        string GetClientIp(IHttpContextAccessor accessor);
    }

    public class HttpRequestWrapper : IHttpRequestWrapper
    {
        public string GetClientIp(IHttpContextAccessor accessor)
            => GetRequestIP(accessor);

        private string GetRequestIP(IHttpContextAccessor accessor, bool tryUseXForwardHeader = true)
        {
            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For
            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763

            if (tryUseXForwardHeader)
            {
                ip = SplitCsv(GetHeaderValueAs<string>(accessor, "X-Forwarded-For"))?.FirstOrDefault();
            }

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && accessor.HttpContext?.Connection?.RemoteIpAddress != null)
            {
                ip = accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = GetHeaderValueAs<string>(accessor, "REMOTE_ADDR");
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new InvalidOperationException("Unable to determine caller's IP.");
            }

            return ip;
        }

        private T GetHeaderValueAs<T>(IHttpContextAccessor accessor, string headerName)
        {
            StringValues values;

            if (accessor.HttpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrWhiteSpace(rawValues))
                {
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
                }
            }

            return default(T);
        }

        private List<string> SplitCsv(string csvList)
            => csvList?
                .TrimEnd(',')
                .Split(',')
                .Select(s => s.Trim())
                .ToList();
    }
}