namespace Backend.API.Models;

public record UserVM
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Family { get; set; }
    public DateTime CreationDate { get; set; }
    public string Mobile { get; set; }
    public string Id { get; set; }
    public List<string> Role { get; set; }
}

public record UserListVM
{
    public List<UserVM> Users { get; set; }
    public int TotalRow { get; set; }
}