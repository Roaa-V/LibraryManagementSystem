using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public bool? IsBlacklisted { get; set; }

    //public string Password { get; set; } = null!;

    public virtual ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<RoomReservation> RoomReservations { get; set; } = new List<RoomReservation>();
}
