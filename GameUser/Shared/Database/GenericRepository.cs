using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GameUser.Shared.Database
{
    public interface IGenericRepository<TDocument> where TDocument : IDocument
    {
        Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression);

        Task InsertOneAsync(TDocument document);

        Task UpdateOneAsync(Expression<Func<TDocument, bool>> filterExpression, UpdateDefinition<TDocument> update);
    }

    public class GenericRepository<TDocument> : IGenericRepository<TDocument> where TDocument : IDocument
    {
        private readonly IMongoCollection<TDocument> collection;

        public GenericRepository(IMongoDatabase database)
        {
            collection = database.GetCollection<TDocument>(Document.GetCollectionName(typeof(TDocument)));
        }

        public virtual Task UpdateOneAsync(Expression<Func<TDocument, bool>> filterExpression, UpdateDefinition<TDocument> update)
        => collection.UpdateOneAsync(filterExpression, update);

        public virtual Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        => collection.Find(filterExpression).FirstOrDefaultAsync();

        public virtual Task InsertOneAsync(TDocument document)
        {
            document.Id = ObjectId.GenerateNewId();

            return collection.InsertOneAsync(document);
        }
    }
}