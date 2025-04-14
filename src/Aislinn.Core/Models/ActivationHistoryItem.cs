

using System.Dynamic;
using Aislinn.Core.Interfaces;

namespace Aislinn.Core.Models;
public class ActivationHistoryItem : IEmotion
{
    public double PreviousValue { get; set; } = 0.0;
    public double NewValue { get; set; } = 0.0;
    //So we can calculate the intensity of the change
    public double Change { get; set; } = 0.0;
    //we'll keep a track of EVERY activation and iterate this number
    // we can look at using this for Frequency calculations instead of date/time which may not make sense
    public ulong SequenceNumber { get; set; } = 0;

    public DateTime ActivationDate { get; set; } = DateTime.Now;

    //just a hunch, but it may be useful to track the emotion experienced during this activation
    public string EmotionName { get; set; }
    public List<double> Coordinates { get; set; } = new List<double>();
    //list from most recent to grandparent for tracking spreading activation to this chunk
    public List<Guid> ActivatedBy { get; set; } = new List<Guid>();
    public Guid ActivatedByChunk { get; set; } = Guid.Empty;
    public string FormatElapsedTime()
    {
        TimeSpan timeSince = DateTime.Now - ActivationDate;

        if (timeSince.TotalSeconds < 60)
        {
            return "recently ";
        }
        else if (timeSince.TotalDays < 1)
        {
            return $"earlier ";
        }
        else if (timeSince.TotalDays < 30)
        {
            return $"{timeSince.Days} days, {timeSince.Hours} hours ago ";
        }
        else if (timeSince.TotalDays < 365)
        {
            int months = (int)(timeSince.TotalDays / 30);
            return $"{months} months, {(int)(timeSince.TotalDays % 30)} days ago ";
        }
        else
        {
            int years = (int)(timeSince.TotalDays / 365);
            return $"{years} years, {timeSince.Days} days ago ";
        }
    }
}