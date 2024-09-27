using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisCnxString"];
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddSingleton<ITaskRepository, TaskRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapPost("/todo", async ([FromBody] Todo newTodo, IDistributedCache cache, ITaskRepository taskRepository) =>
{
    newTodo.Id = Guid.NewGuid().ToString();
    newTodo.Username = newTodo.Username;
    newTodo.CreatedBy = DateTime.UtcNow;
    newTodo.Completed = false;

    await taskRepository.CreateAsync(newTodo);

    // Save the todo in the cache
    // Get all from cache    
    List<Todo>? userTodos = await GetTodosByUsername(newTodo.Username, cache);
    userTodos?.Add(newTodo);

    await cache.SetStringAsync(newTodo.Username, JsonSerializer.Serialize(userTodos));

    return Results.Created($"/todo/{newTodo.Id}", newTodo);
})
.WithName("CreateTodo")
.WithOpenApi(); ;

app.MapGet("/todo/{username}", async (ITaskRepository taskRepository, IDistributedCache cache, string username) =>
{
    // Validate cache first
    List<Todo>? userTodos = await GetTodosByUsername(username, cache);

    if (userTodos == null)
    {
        userTodos = await taskRepository.GetByUserAsync(username);
        await cache.SetStringAsync(username, JsonSerializer.Serialize(userTodos));
    }

    return Results.Ok(userTodos);
})
.WithName("GetTodoByUsername")
.WithOpenApi(); ;

app.MapPost("/todo/completed/{id}/{username}", async (ITaskRepository taskRepository, IDistributedCache cache, string id, string username) =>
{
    var existingTodo = await taskRepository.GetAsync(id, username);

    if (existingTodo == null)
    {
        return Results.NotFound();
    }

    existingTodo.Completed = true;
    existingTodo.CompletedOn = DateTime.UtcNow;

    await taskRepository.UpdateAsync(id, existingTodo);

    // Update cache
    List<Todo>? userTodos = await GetTodosByUsername(username, cache);
    if (userTodos != null)
    {
        var todo = userTodos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            todo.Completed = true;
            todo.CompletedOn = existingTodo.CompletedOn;
            userTodos[userTodos.IndexOf(todo)] = todo;
            await cache.SetStringAsync(username, JsonSerializer.Serialize(userTodos));
        }
    }

    return Results.Ok(existingTodo);

})
.WithName("CompleteTodo")
.WithOpenApi();

app.MapDelete("/todo/{username}", async (ITaskRepository taskRepository, IDistributedCache cache, string username) =>
{
    try
    {
        await cache.RemoveAsync(username);
    }
    catch (System.Exception)
    {

    }
    await taskRepository.DeleteAllTaskByUsernameAsync(username);
    return Results.NoContent();
})
.WithName("DeleteAllTasks")
.WithOpenApi();

app.Run();

async Task<List<Todo>?> GetTodosByUsername(string username, IDistributedCache cache)
{
    try
    {
        var todos = await cache.GetStringAsync(username);
        var userTodos = todos == null ? new List<Todo>() : JsonSerializer.Deserialize<List<Todo>>(todos);
        return userTodos;
    }
    catch (System.Exception)
    {
        return null;
    }

}