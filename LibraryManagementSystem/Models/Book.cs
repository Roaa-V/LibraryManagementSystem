using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Author { get; set; }

    public string? Isbn { get; set; }

    public int? CopiesAvailable { get; set; }

    public int? TotalCopies { get; set; }

    public string? Description { get; set; }

    public DateTime? AddedDate { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<BorrowedBook> BorrowedBooks { get; set; } = new List<BorrowedBook>();
}
