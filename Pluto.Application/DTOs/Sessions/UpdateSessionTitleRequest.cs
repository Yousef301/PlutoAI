namespace Pluto.Application.DTOs.Sessions;

public class UpdateSessionTitleRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; }
};