using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class BorrowedBook
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int BookId { get; set; }

    public DateTime? BorrowDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool? IsConfirmedByAdmin { get; set; }

    public int IsLate { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
