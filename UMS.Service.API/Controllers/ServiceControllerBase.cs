using Microsoft.AspNetCore.Mvc;
using UMS.Service.API.Common;
using FluentValidation;
namespace UMS.Service.API.Controllers
{
    public class ServiceControllerBase : ControllerBase
    {
        protected ILogger Logger { get; }

        public ServiceControllerBase(ILogger logger)
        {
            Logger = logger;
        }

        protected IActionResult HandleValidationException(ValidationException ex)
        {
            var response = ServiceResponse.ErrorResponse(ex);
            response.Msg = "Validation failed.";
            response.Errorlst = ex.Errors.Select(e => new ErrorMessage { Error = e.ErrorMessage, Value = e.AttemptedValue?.ToString() }).ToList();
            return BadRequest(response);
        }

        protected IActionResult OkServiceResponse(object payload, string message = null)
        {
            return Ok(ServiceResponse.SuccessResponse(message, payload));
        }

        protected IActionResult CreatedServiceResponse(object payload, string message = null)
        {
            return Created("", ServiceResponse.SuccessResponse(message, payload));
        }
        protected IActionResult BadRequestResponse(string message = null)
        {
            return BadRequest(ServiceResponse.ErrorResponse(message));
        }
        protected IActionResult HandleOtherException(Exception ex)
        {
            Logger.LogError(ex, "Exception occured while processing request.");
            var response = ServiceResponse.ErrorResponse(ex);
            response.Msg = "Processing request failed.";
            response.Errorlst = new List<ErrorMessage>() { new ErrorMessage() { Error = ex.Message } };
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
}

