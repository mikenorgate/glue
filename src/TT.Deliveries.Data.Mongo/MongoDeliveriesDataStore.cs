using MongoDB.Driver;

namespace TT.Deliveries.Data.Mongo;

public class MongoDeliveriesDataStore : IDeliveriesDataStore
{
    private readonly IMongoCollection<Delivery> _collection;

    public MongoDeliveriesDataStore(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Delivery>("deliveries");
    }

    public async Task<OperationStatus> InsertDelivery(Delivery delivery, CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.InsertOneAsync(delivery, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return OperationStatus.Conflict;
        }
        catch (MongoException)
        {
            return OperationStatus.Failed;
        }

        return OperationStatus.Completed;
    }

    public async Task<ICollection<Delivery>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _collection.FindAsync(Builders<Delivery>.Filter.Empty, cancellationToken: cancellationToken);
        var deliveries = new List<Delivery>();
        while (await result.MoveNextAsync(cancellationToken)) deliveries.AddRange(result.Current);

        return deliveries;
    }

    public async Task<Delivery?> GetByOrderId(string orderId, CancellationToken cancellationToken = default)
    {
        var result = await _collection.FindAsync(Builders<Delivery>.Filter.Eq(x => x.OrderId, orderId),
            new FindOptions<Delivery>
            {
                Limit = 1
            }, cancellationToken);
        await result.MoveNextAsync(cancellationToken);
        return result.Current.FirstOrDefault(defaultValue: null);
    }

    public async Task<OperationStatus> CancelDelivery(string orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _collection.UpdateOneAsync(Builders<Delivery>.Filter.And(
                    Builders<Delivery>.Filter.Eq(x => x.OrderId, orderId),
                    Builders<Delivery>.Filter.Exists(x => x.CompletedTime, false),
                    Builders<Delivery>.Filter.Lte(x => x.EndTime, DateTime.UtcNow)),
                Builders<Delivery>.Update.Set(x => x.CancellationTime, DateTime.UtcNow),
                new UpdateOptions
                {
                    IsUpsert = false
                },
                cancellationToken);
            if (result.ModifiedCount == 1) return OperationStatus.Completed;

            return OperationStatus.InvalidState;
        }
        catch (MongoException)
        {
            return OperationStatus.Failed;
        }
    }

    public async Task<OperationStatus> ApproveDelivery(string orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _collection.UpdateOneAsync(Builders<Delivery>.Filter.And(
                    Builders<Delivery>.Filter.Eq(x => x.OrderId, orderId),
                    Builders<Delivery>.Filter.Exists(x => x.CompletedTime, false),
                    Builders<Delivery>.Filter.Exists(x => x.CancellationTime, false),
                    Builders<Delivery>.Filter.Lte(x => x.EndTime, DateTime.UtcNow)),
                Builders<Delivery>.Update.Set(x => x.ApprovalTime, DateTime.UtcNow),
                new UpdateOptions
                {
                    IsUpsert = false
                },
                cancellationToken);
            if (result.ModifiedCount == 1) return OperationStatus.Completed;

            return OperationStatus.InvalidState;
        }
        catch (MongoException)
        {
            return OperationStatus.Failed;
        }
    }

    public async Task<OperationStatus> CompleteDelivery(string orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _collection.UpdateOneAsync(Builders<Delivery>.Filter.And(
                    Builders<Delivery>.Filter.Eq(x => x.OrderId, orderId),
                    Builders<Delivery>.Filter.Exists(x => x.ApprovalTime),
                    Builders<Delivery>.Filter.Exists(x => x.CancellationTime, false),
                    Builders<Delivery>.Filter.Lte(x => x.EndTime, DateTime.UtcNow)),
                Builders<Delivery>.Update.Set(x => x.ApprovalTime, DateTime.UtcNow),
                new UpdateOptions
                {
                    IsUpsert = false
                },
                cancellationToken);
            if (result.ModifiedCount == 1) return OperationStatus.Completed;

            return OperationStatus.InvalidState;
        }
        catch (MongoException)
        {
            return OperationStatus.Failed;
        }
    }
}