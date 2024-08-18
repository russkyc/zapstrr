
namespace Zapstrr.Models.Entities;

public class Question
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string Image { get; set; }
    public List<Choice> Choices { get; set; }
}