using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using Bogus;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectionString")));

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // session lasts 1 hour
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MUST be before UseAuthorization
app.UseSession();

app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    context.Database.EnsureCreated();
    
    // Seed Books
if (!context.Books.Any())
    {
        var books = new List<Book>
    {
        new Book { Title = "The Trial", Author = "Franz Kafka", TotalCopies = 5, CopiesAvailable = 5 },
        new Book { Title = "The Metamorphosis", Author = "Franz Kafka", TotalCopies = 5, CopiesAvailable = 5 },
        new Book { Title = "Crime and Punishment", Author = "Fyodor Dostoevsky", TotalCopies = 5, CopiesAvailable = 5 },
        new Book { Title = "The Brothers Karamazov", Author = "Fyodor Dostoevsky", TotalCopies = 5, CopiesAvailable = 5 }
    };

        // Generate 496 fake books
        var faker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.TotalCopies, f => f.Random.Int(1, 10))
            .RuleFor(b => b.CopiesAvailable, f => f.Random.Int(1, 10));

        for (int i = 0; i < 496; i++)
            books.Add(faker.Generate());

        // Get existing titles in DB
        var existingTitles = context.Books.Select(b => b.Title).ToHashSet();

        // Only add books that are not already in DB
        var newBooks = books.Where(b => !existingTitles.Contains(b.Title)).ToList();

        context.Books.AddRange(newBooks);
        context.SaveChanges();
    }

    // Seed Students
    if (!context.Students.Any())
    {
        var studentFaker = new Faker<Student>()
            .RuleFor(s => s.Name, f => f.Name.FullName())
            .RuleFor(s => s.Email, f => f.Internet.Email())
            .RuleFor(s => s.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(s => s.IsBlacklisted, f => false);

        context.Students.AddRange(studentFaker.Generate(150));
    }

    // Seed Meeting Rooms
    if (!context.MeetingRooms.Any())
    {
        var rooms = new List<MeetingRoom>();
        for (int i = 1; i <= 20; i++)
            rooms.Add(new MeetingRoom { RoomName = $"Room {i}", Location = $"Floor {i % 5 + 1}" });

        context.MeetingRooms.AddRange(rooms);
    }

    context.SaveChanges();
}

app.Run();