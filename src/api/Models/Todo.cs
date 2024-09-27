namespace Contoso.Models;

public class Todo
{
    public string Id { get; set; }
    public string TaskDescription { get; set; }
    public bool Completed { get; set; }
    public string Username { get; set; }
    public DateTime CreatedBy { get; set; }
    public DateTime? CompletedOn { get; set; }

    public Todo()
    {

    }
}