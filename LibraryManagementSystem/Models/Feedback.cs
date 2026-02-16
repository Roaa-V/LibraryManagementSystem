using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public bool? IsReviewed { get; set; }

    public virtual Student Student { get; set; } = null!;
}
