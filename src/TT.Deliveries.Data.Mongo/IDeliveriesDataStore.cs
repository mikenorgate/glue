namespace TT.Deliveries.Data.Mongo;

public interface IDeliveriesDataStore
{
    Task<OperationStatus> InsertDelivery(Delivery delivery, CancellationToken cancellationToken = default);
    Task<ICollection<Delivery>> GetAll(CancellationToken cancellationToken = default);
    Task<Delivery?> GetByOrderId(string orderId, CancellationToken cancellationToken = default);
    Task<OperationStatus> CancelDelivery(string orderId, CancellationToken cancellationToken = default);
    Task<OperationStatus> ApproveDelivery(string orderId, CancellationToken cancellationToken = default);
    Task<OperationStatus> CompleteDelivery(string orderId, CancellationToken cancellationToken = default);
}