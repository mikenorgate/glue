using TT.Deliveries.Data.Dto;

namespace TT.Deliveries.Data.Mongo;

public static class DataStore
{
    public static Delivery FromCreateDeliveryRequest(CreateDeliveryRequestDto obj)
    {
        return new Delivery
        {
            OrderId = obj.Order.OrderNumber,
            Sender = obj.Order.Sender,
            StartTime = obj.AccessWindow.StartTime.ToUniversalTime(),
            EndTime = obj.AccessWindow.EndTime.ToUniversalTime(),
            Recipient = new Recipient
            {
                Name = obj.Recipient.Name,
                Email = obj.Recipient.Email,
                Address = obj.Recipient.Address,
                Phone = obj.Recipient.PhoneNumber
            }
        };
    }

    public static DeliveryResponseDto ToDeliveryResponse(Delivery obj)
    {
        return new DeliveryResponseDto
        {
            AccessWindow = new AccessWindowDto
            {
                StartTime = obj.StartTime,
                EndTime = obj.EndTime
            },
            Recipient = new RecipientDto
            {
                Address = obj.Recipient.Address,
                Email = obj.Recipient.Email,
                Name = obj.Recipient.Name,
                PhoneNumber = obj.Recipient.Phone
            },
            Order = new OrderDto
            {
                OrderNumber = obj.OrderId,
                Sender = obj.Sender
            },
            State = GetState(obj)
        };
    }

    private static DeliveryState GetState(Delivery obj)
    {
        if (obj.CompletedTime.HasValue) return DeliveryState.Completed;

        if (obj.CancellationTime.HasValue) return DeliveryState.Cancelled;

        if (obj.EndTime < DateTime.UtcNow) return DeliveryState.Expired;

        if (obj.ApprovalTime.HasValue) return DeliveryState.Approved;

        return DeliveryState.Created;
    }
}