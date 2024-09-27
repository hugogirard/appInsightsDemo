var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var todoDictionary = new Dictionary<string, Todo>();

app.MapPost("/todo", (Todo newTodo) =>
{
    newTodo.Id = Guid.NewGuid().ToString();
    newTodo.Username = newTodo.Username;
    newTodo.CreatedBy = DateTime.UtcNow;
    newTodo.Completed = false;
    todoDictionary[newTodo.Id] = newTodo;
    return Results.Created($"/todo/{newTodo.Id}", newTodo);
})
.WithName("CreateTodo")
.WithOpenApi(); ;

app.MapGet("/todo/{username}", (string username) =>
{
    var userTodos = todoDictionary.Values.Where(todo => todo.Username == username).ToList();
    return Results.Ok(userTodos);
})
.WithName("GetTodoByUsername")
.WithOpenApi(); ;

app.MapPut("/todo/{id}", (string id, Todo updatedTodo) =>
{
    if (todoDictionary.ContainsKey(id))
    {
        var existingTodo = todoDictionary[id];
        existingTodo.TaskDescription = updatedTodo.TaskDescription;
        existingTodo.Completed = updatedTodo.Completed;
        return Results.Ok(existingTodo);
    }
    return Results.NotFound();
})
.WithName("GetTodoById")
.WithOpenApi(); ;

app.MapPost("/todo/completed/{id}", (string id) =>
{
    if (todoDictionary.ContainsKey(id))
    {
        var existingTodo = todoDictionary[id];
        existingTodo.Completed = true;
        existingTodo.CompletedOn = DateTime.UtcNow;
        return Results.Ok(existingTodo);
    }
    return Results.NotFound();
})
.WithName("CompleteTodo")
.WithOpenApi();

app.MapDelete("/todo/{username}", (string username) =>
{
    var userTodos = todoDictionary.Values.Where(todo => todo.Username == username).ToList();
    foreach (var todo in userTodos)
    {
        todoDictionary.Remove(todo.Id);
    }
    return Results.NoContent();
})
.WithName("DeleteAllTasks")
.WithOpenApi();

app.Run();

