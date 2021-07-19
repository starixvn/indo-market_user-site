using System;
using System.Collections.Generic;
using System.Linq;
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

        Task<IEnumerable<TDocument>> FindAsync(Expression<Func<TDocument, bool>> filterExpression, FindOptions<TDocument> options = null);

        Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> filterExpression);

        IQueryable<TDocument> AsQueryable();
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

        public virtual async Task<IEnumerable<TDocument>> FindAsync(Expression<Func<TDocument, bool>> filterExpression, FindOptions<TDocument> options = null)
        => (await collection.FindAsync(filterExpression, options)).ToEnumerable();

        public virtual Task InsertOneAsync(TDocument document)
        {
            document.Id = ObjectId.GenerateNewId();

            return collection.InsertOneAsync(document);
        }

        public virtual async Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            var result = await collection.FindAsync(filterExpression);

            return result.ToList();
        }

        public virtual IQueryable<TDocument> AsQueryable()
        {
            return collection.AsQueryable();
        }
    }
}