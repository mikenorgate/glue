using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TT.Deliveries.Data.Dto;
using TT.Deliveries.Data.Mongo;

namespace TT.Deliveries.Web.Api.Controllers;

[Route("deliveries")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveriesDataStore _dataStore;

    public DeliveriesController(IDeliveriesDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <summary>
    /// Creates a new delivery.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>A newly created delivery</returns>
    /// <response code="200">Returns the newly created item</response>
    /// <response code="400">If there is validation errors on the request</response>
    /// <response code="409">If there is already a delivery for the required order id</response>
    [HttpPost]
    [Authorize(Roles = "partner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeliveryResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(void))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(void))]
    public async Task<IActionResult> CreateDelivery(CreateDeliveryRequestDto requestData,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var delivery = DataStore.FromCreateDeliveryRequest(requestData);
        var result = await _dataStore.InsertDelivery(delivery, cancellationToken);
        if (result != OperationStatus.Completed)
            return result == OperationStatus.Conflict ? Conflict() : StatusCode(500);

        var response = DataStore.ToDeliveryResponse(delivery);
        return Ok(response);
    }

    /// <summary>
    /// Get all deliveries
    /// </summary>
    /// <returns>An array of all deliveries</returns>
    /// <response code="200">Returns an array of all deliveries</response>
    [HttpGet]
    [Authorize(Roles = "partner,user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeliveryResponseDto[]))]
    public async Task<IActionResult> GetDeliveries(CancellationToken cancellationToken = default)
    {
        var deliveries = await _dataStore.GetAll(cancellationToken);
        return Ok(deliveries.Select(DataStore.ToDeliveryResponse));
    }

    /// <summary>
    /// Get a single delivery based on the order id
    /// </summary>
    /// <param name="orderId">The order id</param>
    /// <returns>A delivery corresponding to the order id</returns>
    /// <response code="200">Returns a delivery</response>
    /// <response code="404">If there is no delivery for the given order id</response>
    [HttpGet]
    [Route("{orderId}")]
    [Authorize(Roles = "partner,user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeliveryResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDelivery(string orderId, CancellationToken cancellationToken = default)
    {
        var delivery = await _dataStore.GetByOrderId(orderId, cancellationToken);
        if (delivery == null) return NotFound();

        return Ok(DataStore.ToDeliveryResponse(delivery));
    }

    /// <summary>
    /// Cancel a delivery based on the order id
    /// </summary>
    /// <param name="orderId">The order id</param>
    /// <returns></returns>
    /// <response code="202">If the delivery was successfully cancelled</response>
    /// <response code="400">If there it was not possible to cancel the delivery</response>
    [HttpDelete]
    [Route("{orderId}")]
    [Authorize(Roles = "partner,user")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelDelivery(string orderId, CancellationToken cancellationToken = default)
    {
        var result = await _dataStore.CancelDelivery(orderId, cancellationToken);
        if (result != OperationStatus.Completed)
            return result == OperationStatus.InvalidState ? BadRequest() : StatusCode(500);

        return Accepted();
    }


    /// <summary>
    /// Mark delivery as approved
    /// </summary>
    /// <param name="orderId">The order id</param>
    /// <returns></returns>
    /// <response code="202">If the delivery was successfully approved</response>
    /// <response code="400">If there it was not possible to approve the delivery</response>
    [HttpPost]
    [Route("{orderId}/approve")]
    [Authorize(Roles = "user")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveDelivery(string orderId, CancellationToken cancellationToken = default)
    {
        var result = await _dataStore.ApproveDelivery(orderId, cancellationToken);
        if (result != OperationStatus.Completed)
            return result == OperationStatus.InvalidState ? BadRequest() : StatusCode(500);

        return Accepted();
    }


    /// <summary>
    /// Mark the delivery as complete
    /// </summary>
    /// <param name="orderId">The order id</param>
    /// <returns></returns>
    /// <response code="202">If the delivery was successfully completed</response>
    /// <response code="400">If there it was not possible to complete the delivery</response>
    [HttpPost]
    [Route("{orderId}/complete")]
    [Authorize(Roles = "partner")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteDelivery(string orderId, CancellationToken cancellationToken = default)
    {
        var result = await _dataStore.CompleteDelivery(orderId, cancellationToken);
        if (result != OperationStatus.Completed)
            return result == OperationStatus.InvalidState ? BadRequest() : StatusCode(500);

        return Accepted();
    }
}