using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Payload đăng nhập gửi lên từ frontend (auth.js hoặc form)
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payload đăng ký tài khoản gửi lên từ frontend (auth.js)
    /// </summary>
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ (10 số, bắt đầu bằng 0 hoặc +84)")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có tối thiểu 6 ký tự")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payload cập nhật thông tin cá nhân (Profile.cshtml)
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }

    /// <summary>
    /// Payload đổi mật khẩu người dùng (Profile.cshtml)
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [JsonPropertyName("oldPassword")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có tối thiểu 6 ký tự")]
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
