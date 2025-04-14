using System.Text;
using Aislinn.Core.Models;
namespace RAINA.Services;
public class UserContext
{
    public string UserId { get; set; }
    public string CurrentTopic { get; set; }
    public Chunk CurrentUtterance { get; set; }
    public Chunk LastSystemUtterance { get; set; }
    public Chunk CurrentConversationChunk { get; set; }
    public List<Chunk> ActiveMemoryChunks { get; set; } = new List<Chunk>();
    public Dictionary<string, object> ContextVariables { get; set; } = new Dictionary<string, object>();

    // Recent utterances in this context
    private List<Chunk> _utteranceHistory = new List<Chunk>();
    private const int MaxUtteranceHistory = 20;

    public void AddUtterance(Chunk utteranceChunk)
    {
        _utteranceHistory.Add(utteranceChunk);
        if (_utteranceHistory.Count > MaxUtteranceHistory)
        {
            _utteranceHistory.RemoveAt(0);
        }
    }

    public List<Chunk> GetRecentUtterances(int count = 5)
    {
        return _utteranceHistory.OrderByDescending(u =>
            u.Slots.ContainsKey("Timestamp") ?
            (DateTime)u.Slots["Timestamp"].Value :
            DateTime.MinValue)
            .Take(count)
            .ToList();
    }

    public string GetConversationContext(int utteranceCount = 5)
    {
        var recentUtterances = GetRecentUtterances(utteranceCount);
        var contextBuilder = new StringBuilder();

        foreach (var utterance in recentUtterances.OrderBy(u =>
            u.Slots.ContainsKey("Timestamp") ?
            (DateTime)u.Slots["Timestamp"].Value :
            DateTime.MinValue))
        {
            string speaker = utterance.Slots.ContainsKey("Speaker") ?
                utterance.Slots["Speaker"].Value.ToString() : "Unknown";

            string text = utterance.Slots.ContainsKey("Text") ?
                utterance.Slots["Text"].Value.ToString() : "";

            contextBuilder.AppendLine($"{speaker}: {text}");
        }

        return contextBuilder.ToString();
    }
}
