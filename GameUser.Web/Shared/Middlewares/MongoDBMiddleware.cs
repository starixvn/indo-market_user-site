using GameUser.Web.Shared.Configurations;
using GameUser.Web.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GameUser.Web.Shared.Middlewares
{
    public static class MongoDBMiddleware
    {
        public static void ConfigMongoDB(this IServiceCollection services, IConfiguration configuration)
        {
            var dbSettings = configuration.GetSection("DatabaseSettings");           
            var mongoClient = new MongoClient(dbSettings["ConnectionString"]);
            services.AddSingleton<IMongoClient>(mongoClient);
            services.AddSingleton(mongoClient.GetDatabase(dbSettings["DatabaseName"]));
        }
    }
}
