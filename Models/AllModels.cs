using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TableTies.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<RoomBooking> RoomBookings { get; set; } = new List<RoomBooking>();
        public ICollection<TableBooking> TableBookings { get; set; } = new List<TableBooking>();
        public ICollection<ConsultantBooking> ConsultantBookings { get; set; } = new List<ConsultantBooking>();
    }

    public class Consultant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }

        public ICollection<ConsultantBooking> ConsultantBookings { get; set; } = new List<ConsultantBooking>();
    }

    public class ConsultantBooking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid ConsultantId { get; set; }

        [ForeignKey("ConsultantId")]
        public Consultant Consultant { get; set; } = default!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = default!;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime BookingDateTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Duration { get; set; }

        [StringLength(500)]
        public string? Details { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CancelledDateTime { get; set; }
    }

    public class Organization
    {
        public Guid Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    }

    public class Restaurant
    {
        public Guid Id { get; set; }

        [Required]
        public required Guid OrganizationId { get; set; }

        [Required]
        public Organization Organization { get; set; } = null!;

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Location { get; set; }

        public ICollection<RestaurantTable> RestaurantTables { get; set; } = new List<RestaurantTable>();

        public ICollection<TableBooking> TableBookings { get; set; } = new List<TableBooking>();

        public ICollection<Booking>? Bookings { get; set; }
    }

    public class RestaurantTable
    {
        public Guid Id { get; set; }

        [Required]
        public required Guid RestaurantId { get; set; }

        [Required]
        public Restaurant Restaurant { get; set; } = null!;

        [Required]
        public required string TableName { get; set; }

        [Required]
        public required int Capacity { get; set; }

        public ICollection<TableBooking> TableBookings { get; set; } = new List<TableBooking>();
    }

    public class TableBooking
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public required Guid RestaurantId { get; set; }

        [Required]
        public Restaurant Restaurant { get; set; } = null!;

        [Required]
        public required Guid TableId { get; set; }

        [Required]
        public RestaurantTable Table { get; set; } = null!;

        [Required]
        public required DateTime BookingDateTime { get; set; }

        [Required]
        public required int NumberOfGuests { get; set; }

        public string? SpecialRequests { get; set; }

        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? BookingType { get; set; }
    }

    public class Hotel
    {
        public Guid Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Location { get; set; }

        public ICollection<RoomBooking> RoomBookings { get; set; } = new List<RoomBooking>();
    }

    public class RoomBooking
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public required Guid HotelId { get; set; }

        [Required]
        public Hotel Hotel { get; set; } = null!;

        [Required]
        public required DateTime CheckInDate { get; set; }

        [Required]
        public required DateTime CheckOutDate { get; set; }

        [Required]
        public required int NumberOfGuests { get; set; }

        public string? SpecialRequests { get; set; }
    }

    public class Booking
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public ApplicationUser User { get; set; } = null!;

        public Guid? RestaurantId { get; set; }

        public Restaurant? Restaurant { get; set; }

        public Guid? TableId { get; set; }

        public RestaurantTable? Table { get; set; }

        [Required]
        public required DateTime BookingDateTime { get; set; }

        [Required]
        public required int NumberOfGuests { get; set; }

        public string? SpecialRequests { get; set; }

        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? BookingType { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CancelledAt { get; set; }
    }

    public class BookingDto
    {
        [Required(ErrorMessage = "User email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string UserEmail { get; set; }

        [Required(ErrorMessage = "Restaurant Table ID is required.")]
        public required Guid RestaurantTableId { get; set; }

        [Required(ErrorMessage = "Restaurant ID is required.")]
        public required Guid RestaurantId { get; set; }

        [Required(ErrorMessage = "Booking date is required.")]
        [DataType(DataType.Date)]
        public required DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Booking start time is required.")]
        [DataType(DataType.Time)]
        public required TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Booking end time is required.")]
        [DataType(DataType.Time)]
        public required TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1.")]
        public required int NumberOfGuests { get; set; }

        public string? SpecialRequests { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm password")]
        public string? ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Reset token is required.")]
        public required string Token { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        [Display(Name = "New password")]
        public required string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [Display(Name = "Confirm new password")]
        public string? ConfirmPassword { get; set; }
    }
}
