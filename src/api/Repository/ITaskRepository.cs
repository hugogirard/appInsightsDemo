namespace Contoso.Repository;

public interface ITaskRepository
{
    Task<Todo> CreateAsync(Todo todo);
    Task<Todo> GetAsync(string id, string username);
    Task<List<Todo>> GetByUserAsync(string username);
    Task<Todo> UpdateAsync(string id, Todo todo);
    Task DeleteAllTaskByUsernameAsync(string username);
}