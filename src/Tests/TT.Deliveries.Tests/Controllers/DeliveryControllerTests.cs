using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TT.Deliveries.Data.Dto;
using TT.Deliveries.Data.Mongo;
using TT.Deliveries.Web.Api.Controllers;

namespace TT.Deliveries.Tests.Controllers;

[TestFixture]
public class DeliveryControllerTests
{
    private DeliveriesController CreateSUT(IDeliveriesDataStore dataStore = null)
    {
        if (dataStore == null)
            dataStore = new Mock<IDeliveriesDataStore>().Object;

        var controller = new DeliveriesController(dataStore);

        return controller;
    }

    [Test]
    public async Task GetById_Should_Return_404_If_Delivery_Doesnt_Exist()
    {
        var orderId = Guid.NewGuid().ToString();
        var dataStore = new Mock<IDeliveriesDataStore>();
        dataStore.Setup(x => x.GetByOrderId(orderId, default)).Returns(Task.FromResult<Delivery>(null));

        var controller = CreateSUT(dataStore.Object);

        var result = await controller.GetDelivery(orderId);

        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public async Task GetById_Should_Return_Delivery_Details()
    {
        var orderId = Guid.NewGuid().ToString();
        var dataStore = new Mock<IDeliveriesDataStore>();
        var delivery = new Delivery
        {
            OrderId = orderId,
            Sender = "test",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Recipient = new Recipient
            {
                Address = "test",
                Email = "test@test.com",
                Name = "test",
                Phone = "0123456789"
            }
        };
        dataStore.Setup(x => x.GetByOrderId(orderId, default)).Returns(Task.FromResult(delivery));

        var controller = CreateSUT(dataStore.Object);

        var result = await controller.GetDelivery(orderId);

        Assert.IsInstanceOf<OkObjectResult>(result);

        var resultObject = ((OkObjectResult)result).Value;
        Assert.IsNotNull(resultObject);
        Assert.IsInstanceOf<DeliveryResponseDto>(resultObject);
        var actualResponseObject = (DeliveryResponseDto)resultObject;
        var expectedResultObject = DataStore.ToDeliveryResponse(delivery);
        Assert.AreEqual(expectedResultObject.State, actualResponseObject.State);
    }
}