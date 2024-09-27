using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace Contoso.Repository;

public class TaskRepository : ITaskRepository
{
    private readonly Container _container;

    public TaskRepository(IConfiguration configuration)
    {
        CosmosSerializationOptions options = new()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        };

        CosmosClient client = new CosmosClientBuilder(configuration["CosmosDBConnectionString"])
                                  .WithSerializerOptions(options)
                                  .Build();

        Database db = client.GetDatabase("todo");
        _container = db.GetContainer("task");
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        ItemResponse<Todo> response = await _container.CreateItemAsync(todo, new PartitionKey(todo.Username));
        return response.Resource;
    }

    public async Task<Todo> GetAsync(string id, string username)
    {
        try
        {
            ItemResponse<Todo> response = await _container.ReadItemAsync<Todo>(id, new PartitionKey(username));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<Todo>> GetByUserAsync(string username)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.username = @username")
                        .WithParameter("@username", username);

        FeedIterator<Todo> response = _container.GetItemQueryIterator<Todo>(query);

        List<Todo> todos = new();
        while (response.HasMoreResults)
        {
            var todo = await response.ReadNextAsync();
            todos.AddRange(todo);
        }
        return todos;
    }

    public async Task<Todo> UpdateAsync(string id, Todo todo)
    {
        ItemResponse<Todo> response = await _container.ReplaceItemAsync(todo, id, new PartitionKey(todo.Username));
        return response.Resource;
    }

    public async Task DeleteAllTaskByUsernameAsync(string username)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.username = @username")
                        .WithParameter("@username", username);

        FeedIterator<Todo> response = _container.GetItemQueryIterator<Todo>(query);

        List<Todo> todos = new();
        while (response.HasMoreResults)
        {
            var todo = await response.ReadNextAsync();
            todos.AddRange(todo);
        }

        foreach (var todo in todos)
        {
            await _container.DeleteItemAsync<Todo>(todo.Id, new PartitionKey(username));
        }
    }
}