using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Response chuẩn toàn hệ thống trả về kết quả cho Frontend
    /// </summary>
    public class BaseApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        public BaseApiResponse() { }

        public BaseApiResponse(bool success, string message, object? data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static BaseApiResponse Ok(string message = "Thao tác thành công", object? data = null)
        {
            return new BaseApiResponse(true, message, data);
        }

        public static BaseApiResponse Fail(string message = "Đã xảy ra lỗi", object? data = null)
        {
            return new BaseApiResponse(false, message, data);
        }
    }

    /// <summary>
    /// Response chuẩn có kiểu dữ liệu cụ thể (Generic)
    /// </summary>
    public class BaseApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public BaseApiResponse() { }

        public BaseApiResponse(bool success, string message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static BaseApiResponse<T> Ok(string message = "Thao tác thành công", T? data = default)
        {
            return new BaseApiResponse<T>(true, message, data);
        }

        public static BaseApiResponse<T> Fail(string message = "Đã xảy ra lỗi", T? data = default)
        {
            return new BaseApiResponse<T>(false, message, data);
        }
    }
}
