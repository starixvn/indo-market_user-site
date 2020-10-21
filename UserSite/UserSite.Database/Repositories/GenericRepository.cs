using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UserSite.Database.Entities;

namespace UserSite.Database.Repositories
{
    public interface IGenericRepository<TDocument> where TDocument : IDocument
    {
        IQueryable<TDocument> AsQueryable();

        IEnumerable<TDocument> FilterBy(
            Expression<Func<TDocument, bool>> filterExpression);

        IEnumerable<TProjected> FilterBy<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression);

        Task<IEnumerable<TDocument>> FilterByAsync(
           Expression<Func<TDocument, bool>> filterExpression);

        TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression);

        Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression);

        TDocument FindById(string id);

        Task<TDocument> FindByIdAsync(string id);

        void InsertOne(TDocument document);

        Task InsertOneAsync(TDocument document);

        void InsertMany(ICollection<TDocument> documents);

        Task InsertManyAsync(ICollection<TDocument> documents);

        void ReplaceOne(TDocument document);

        Task ReplaceOneAsync(TDocument document);

        void DeleteOne(Expression<Func<TDocument, bool>> filterExpression);

        Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression);

        void DeleteById(string id);

        Task DeleteByIdAsync(string id);

        void DeleteMany(Expression<Func<TDocument, bool>> filterExpression);

        Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression);

        void Update(Expression<Func<TDocument, bool>> filterExpression, UpdateDefinition<TDocument> updateDefinition);

        Task UpdateAsync(Expression<Func<TDocument, bool>> filterExpression, UpdateDefinition<TDocument> updateDefinition);
    }

    public class GenericRepository<TDocument> : IGenericRepository<TDocument> where TDocument : IDocument
    {
        private readonly IMongoCollection<TDocument> collection;

        public GenericRepository(IMongoDatabase database)
        {
            collection = database.GetCollection<TDocument>(Document.GetCollectionName(typeof(TDocument)));
        }

        public virtual IQueryable<TDocument> AsQueryable()
        {
            return collection.AsQueryable();
        }

        public virtual IEnumerable<TDocument> FilterBy(
            Expression<Func<TDocument, bool>> filterExpression)
        {
            return collection.Find(filterExpression).ToEnumerable();
        }

        public virtual IEnumerable<TProjected> FilterBy<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression)
        {
            return collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
        }

        public virtual async Task<IEnumerable<TDocument>> FilterByAsync(
           Expression<Func<TDocument, bool>> filterExpression)
        {
            return (await collection.FindAsync(filterExpression)).ToEnumerable();
        }

        public virtual TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
        => collection.Find(filterExpression).FirstOrDefault();

        public virtual Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        => collection.Find(filterExpression).FirstOrDefaultAsync();

        public virtual TDocument FindById(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);

            return collection.Find(filter).SingleOrDefault();
        }

        public virtual async Task<TDocument> FindByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);

            return (await collection.FindAsync(filter)).FirstOrDefault();
        }

        public virtual void InsertOne(TDocument document)
        {
            document.Id = ObjectId.GenerateNewId();

            collection.InsertOne(document);
        }

        public virtual Task InsertOneAsync(TDocument document)
        {
            document.Id = ObjectId.GenerateNewId();

            return collection.InsertOneAsync(document);
        }

        public void InsertMany(ICollection<TDocument> documents)
        {
            collection.InsertMany(documents);
        }

        public virtual async Task InsertManyAsync(ICollection<TDocument> documents)
        {
            await collection.InsertManyAsync(documents);
        }

        public void ReplaceOne(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            collection.FindOneAndReplace(filter, document);
        }

        public virtual async Task ReplaceOneAsync(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            await collection.FindOneAndReplaceAsync(filter, document);
        }

        public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            collection.FindOneAndDelete(filterExpression);
        }

        public Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        => collection.FindOneAndDeleteAsync(filterExpression);

        public void DeleteById(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            collection.FindOneAndDelete(filter);
        }

        public Task DeleteByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);

            return collection.FindOneAndDeleteAsync(filter);
        }

        public void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
        {
            collection.DeleteMany(filterExpression);
        }

        public Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
        => collection.DeleteManyAsync(filterExpression);

        public Task UpdateAsync(Expression<Func<TDocument, bool>> filterExpression, UpdateDefinition<TDocument> updateDefinition)
        => collection.UpdateOneAsync(filterExpression, updateDefinition);

        public void Update(Expression<Func<TDocument, bool>> filterExpression, UpdateDefinition<TDocument> updateDefinition)
        {
            collection.UpdateOne(filterExpression, updateDefinition);
        }
    }
}