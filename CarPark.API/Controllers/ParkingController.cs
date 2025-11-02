using CarPark.API.Contracts.ExitVehicle;
using CarPark.API.Contracts.GetOccupancy;
using CarPark.API.Contracts.ParkVehicle;
using CarPark.Application.Parking.ExitVehicle;
using CarPark.Application.Parking.GetOccupancy;
using CarPark.Application.Parking.ParkVehicle;
using CarPark.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkVehicleResponse = CarPark.API.Contracts.ParkVehicle.ParkVehicleResponse;

namespace CarPark.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ParkingController(IMediator mediator) => _mediator = mediator;

        /// <summary>Gets available and occupied number of spaces.</summary>
        [HttpGet("/parking")] // absolute route -> GET /parking
        [ProducesResponseType(typeof(GetOccupancyResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOccupancy(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOccupancyQuery(), ct);

            var response = new GetOccupancyResponse
            {
                AvailableSpaces = result.AvailableSpaces,
                OccupiedSpaces = result.OccupiedSpaces
            };

            return Ok(response);
        }

        /// <summary>
        /// Parks a given vehicle in the first available space and returns the vehicle and its space number.
        /// </summary>
        /// <response code="201">Vehicle parked successfully.</response>
        /// <response code="400">Invalid payload.</response>
        /// <response code="409">Vehicle already parked or no spaces available.</response>
        [HttpPost("/parking")]
        [ProducesResponseType(typeof(ParkVehicleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ParkAsync([FromBody] ParkVehicleRequest body, CancellationToken ct)
        {
            try
            {
                var result = await _mediator.Send(
                    new ParkVehicleCommand(body.VehicleReg, body.VehicleType), ct);

                var response = new ParkVehicleResponse
                {
                    VehicleReg = result.VehicleReg,
                    SpaceNumber = result.SpaceNumber,
                    TimeIn = result.TimeInUtc
                };

                return Created($"/parking/{response.SpaceNumber}", response);
            }
            catch (FluentValidation.ValidationException fv)
            {
                var errors = fv.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return ValidationProblem(new ValidationProblemDetails(errors));
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (VehicleAlreadyParkedException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Vehicle already parked",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict
                });
            }
            catch (NoSpacesAvailableException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "No spaces available",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict
                });
            }
        }

        /// <summary>Frees the vehicle's space and returns its final charge.</summary>
        [HttpPost("/parking/exit")]
        [ProducesResponseType(typeof(ExitVehicleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExitAsync([FromBody] ExitVehicleRequest body, CancellationToken ct)
        {
            try
            {
                var result = await _mediator.Send(new ExitVehicleCommand(body.VehicleReg), ct);

                var response = new ExitVehicleResponse
                {
                    VehicleReg = result.VehicleReg,
                    VehicleCharge = (double)result.VehicleCharge,
                    TimeIn = result.TimeInUtc,
                    TimeOut = result.TimeOutUtc
                };

                return Ok(response);
            }
            catch (FluentValidation.ValidationException fv)
            {
                var errors = fv.Errors.GroupBy(e => e.PropertyName)
                                      .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return ValidationProblem(new ValidationProblemDetails(errors));
            }
            catch (VehicleNotFoundException ex)
            {
                return NotFound(new ProblemDetails { Title = "Vehicle not found", Detail = ex.Message, Status = 404 });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ProblemDetails { Title = "Validation error", Detail = ex.Message, Status = 400 });
            }
        }
    }
}
