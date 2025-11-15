namespace UMS.Service.API.Common
{
    public class ServiceResponse
    {
        public static ServiceResponse<object> SuccessResponse()
        {
            return SuccessResponse<object>(null);
        }
        public static ServiceResponse<T> SuccessResponse<T>(T payload)
        {
            return new ServiceResponse<T>
            {
                Success = true,
                Data = payload
            };
        }
        public static ServiceResponse<T> SuccessResponse<T>(string message, T payload)
        {
            return new ServiceResponse<T>
            {
                Success = true,
                Data = payload,
                Msg = message
            };
        }
        public static ServiceResponse<object> SuccessResponseMessage(string message)
        {
            return new ServiceResponse<object>
            {
                Success = true,
                Data = null,
                Msg = message
            };
        }

        public static ServiceResponse<object> ErrorResponse(string message)
        {
            return new ServiceResponse<object>
            {
                Msg = message,
                Success = false,
                Data = null
            };
        }

        public static ServiceResponse<object> ErrorResponse(Exception exception)
        {
            return ErrorResponse(exception.Message);
        }
    }
    public class ServiceResponse<T>
    {
        public T Data { get; set; }

        public string Msg { get; set; }

        public bool Success { get; set; }

        public List<ErrorMessage> Errorlst { get; set; }
    }
    public class ErrorMessage
    {
        public string Error { get; set; }

        public string Value { get; set; }
    }
}