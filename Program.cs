using UserApi.Models;
using UserApi.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var userService = new UserService();

// Middleware de logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
});

// GET all users
app.MapGet("/users", () => Results.Ok(userService.GetAll()));

// GET user by id
app.MapGet("/users/{id:int}", (int id) =>
{
    var user = userService.GetById(id);
    return user is not null ? Results.Ok(user) : Results.NotFound(new { Message = "User not found" });
});

// POST create user con validación
app.MapPost("/users", (User user) =>
{
    var validationErrors = ValidateUser(user);
    if (validationErrors.Any())
        return Results.BadRequest(new { Errors = validationErrors });

    var createdUser = userService.Create(user);
    return Results.Created($"/users/{createdUser.Id}", createdUser);
});

// PUT update user con validación
app.MapPut("/users/{id:int}", (int id, User user) =>
{
    var validationErrors = ValidateUser(user);
    if (validationErrors.Any())
        return Results.BadRequest(new { Errors = validationErrors });

    return userService.Update(id, user) ? Results.Ok(user) : Results.NotFound(new { Message = "User not found" });
});

// DELETE user
app.MapDelete("/users/{id:int}", (int id) =>
    userService.Delete(id) ? Results.Ok(new { Message = "User deleted" }) : Results.NotFound(new { Message = "User not found" })
);

app.Run();

// Función de validación centralizada
List<string> ValidateUser(User user)
{
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(user.Name))
        errors.Add("Name is required.");

    if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
        errors.Add("Valid email is required.");

    if (user.Age < 0)
        errors.Add("Age must be a positive number.");

    return errors;
}
