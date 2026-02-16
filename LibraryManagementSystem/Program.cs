using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using Bogus;
using System.Linq;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectionString")));


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromHours(1);
//});
//app.UseSession();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");





// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    context.Database.EnsureCreated();

    if (!context.Books.Any())
    {
        var books = new List<Book>
        {
            new Book { Title = "The Trial", Author = "Franz Kafka", TotalCopies = 5, CopiesAvailable = 5 },
            new Book { Title = "The Metamorphosis", Author = "Franz Kafka", TotalCopies = 5, CopiesAvailable = 5 },
            new Book { Title = "Crime and Punishment", Author = "Fyodor Dostoevsky", TotalCopies = 5, CopiesAvailable = 5 },
            new Book { Title = "The Brothers Karamazov", Author = "Fyodor Dostoevsky", TotalCopies = 5, CopiesAvailable = 5 }
        };

        // Faker additional books
        var faker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.TotalCopies, f => f.Random.Int(1, 10))
            .RuleFor(b => b.CopiesAvailable, f => f.Random.Int(1, 10));

        for (int i = 0; i < 496; i++) // to reach 500 total
            books.Add(faker.Generate());

        context.Books.AddRange(books);
    }

    if (!context.Students.Any())
    {
        var studentFaker = new Faker<Student>()
            .RuleFor(s => s.Name, f => f.Name.FullName())
            .RuleFor(s => s.Email, f => f.Internet.Email())
            .RuleFor(s => s.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(s => s.IsBlacklisted, f => false);

        context.Students.AddRange(studentFaker.Generate(150));
    }

    if (!context.MeetingRooms.Any())
    {
        var rooms = new List<MeetingRoom>();
        for (int i = 1; i <= 20; i++)
            rooms.Add(new MeetingRoom { RoomName = $"Room {i}", Location = $"Floor {i % 5 + 1}" });

        context.MeetingRooms.AddRange(rooms);
    }

    context.SaveChanges();
}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");















app.Run();
