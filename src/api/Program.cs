using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskRepository, TaskRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var todoDictionary = new Dictionary<string, Todo>();

app.MapPost("/todo", async ([FromBody] Todo newTodo, ITaskRepository taskRepository) =>
{
    newTodo.Id = Guid.NewGuid().ToString();
    newTodo.Username = newTodo.Username;
    newTodo.CreatedBy = DateTime.UtcNow;
    newTodo.Completed = false;

    await taskRepository.CreateAsync(newTodo);

    //todoDictionary[newTodo.Id] = newTodo;
    return Results.Created($"/todo/{newTodo.Id}", newTodo);
})
.WithName("CreateTodo")
.WithOpenApi(); ;

app.MapGet("/todo/{username}", async (ITaskRepository taskRepository, string username) =>
{
    var userTodos = await taskRepository.GetByUserAsync(username);

    //var userTodos = todoDictionary.Values.Where(todo => todo.Username == username).ToList();
    return Results.Ok(userTodos);
})
.WithName("GetTodoByUsername")
.WithOpenApi(); ;

// app.MapPut("/todo/{id}", async (ITaskRepository taskRepository, string id, [FromBody] Todo updatedTodo) =>
// {
//     var existingTodo = await taskRepository.GetAsync(id, updatedTodo.Username);

//     if (existingTodo == null)
//     {
//         return Results.NotFound();
//     }

//     existingTodo.TaskDescription = updatedTodo.TaskDescription;
//     existingTodo.Completed = updatedTodo.Completed;

//     await taskRepository.UpdateAsync(id, existingTodo);

//     return Results.Ok(existingTodo);


//     // if (todoDictionary.ContainsKey(id))
//     // {
//     //     var existingTodo = todoDictionary[id];
//     //     existingTodo.TaskDescription = updatedTodo.TaskDescription;
//     //     existingTodo.Completed = updatedTodo.Completed;
//     //     return Results.Ok(existingTodo);
//     // }
//     //return Results.NotFound();
// })
// .WithName("GetTodoById")
// .WithOpenApi(); ;

app.MapPost("/todo/completed/{id}/{username}", async (ITaskRepository taskRepository, string id, string username) =>
{

    var existingTodo = await taskRepository.GetAsync(id, username);

    if (existingTodo == null)
    {
        return Results.NotFound();
    }

    existingTodo.Completed = true;
    existingTodo.CompletedOn = DateTime.UtcNow;

    await taskRepository.UpdateAsync(id, existingTodo);

    return Results.Ok(existingTodo);

})
.WithName("CompleteTodo")
.WithOpenApi();

app.MapDelete("/todo/{username}", async (ITaskRepository taskRepository, string username) =>
{
    await taskRepository.DeleteAllTaskByUsernameAsync(username);
    return Results.NoContent();

    //var userTodos = todoDictionary.Values.Where(todo => todo.Username == username).ToList();
    // foreach (var todo in userTodos)
    // {
    //     todoDictionary.Remove(todo.Id);
    // }
    //return Results.NoContent();
})
.WithName("DeleteAllTasks")
.WithOpenApi();

app.Run();

