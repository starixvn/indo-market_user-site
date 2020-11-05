using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Web.Shared.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
