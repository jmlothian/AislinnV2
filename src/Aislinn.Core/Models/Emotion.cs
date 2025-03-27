using System.Text.Json.Serialization;
using Aislinn.Core.Interfaces;

namespace Aislinn.Core.Models;

/*
The PAD emotional state model, also known as the Pleasure-Arousal-Dominance model, is a psychological framework that describes emotions based on three dimensions: Pleasure, Arousal, and Dominance. Each dimension ranges from -1 to 1, where -1 represents the lowest intensity and 1 represents the highest intensity.

In this model:

Pleasure (P) represents the degree of enjoyment or displeasure.
Arousal (A) represents the level of activation or arousal.
Dominance (D) represents the sense of control or lack of cont

*/
public class Emotion : IEmotion
{
    public string EmotionName { get; set; }
    public List<double> Coordinates { get; set; } = new List<double>();
}