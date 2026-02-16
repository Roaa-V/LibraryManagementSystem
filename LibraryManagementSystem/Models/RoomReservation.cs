using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class RoomReservation
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int RoomId { get; set; }

    public DateTime ReservationDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public bool? IsConfirmedByAdmin { get; set; }

    public virtual MeetingRoom Room { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
