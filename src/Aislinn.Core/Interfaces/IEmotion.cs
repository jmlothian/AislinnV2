namespace Aislinn.Core.Interfaces;

public interface IEmotion
{
    public string EmotionName { get; set; }
    public List<double> Coordinates { get; set; }
}