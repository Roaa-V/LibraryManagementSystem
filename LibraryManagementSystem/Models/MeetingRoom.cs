using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class MeetingRoom
{
    public int Id { get; set; }

    public string RoomName { get; set; } = null!;

    public string? Location { get; set; }

    public int? Capacity { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual ICollection<RoomReservation> RoomReservations { get; set; } = new List<RoomReservation>();
}
