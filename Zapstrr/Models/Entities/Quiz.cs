
namespace Zapstrr.Models.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int QuestionPoints { get; set; }
    public List<Question> Questions { get; set; }
}