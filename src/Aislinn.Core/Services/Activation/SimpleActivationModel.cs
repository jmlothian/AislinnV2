using Aislinn.Core.Activation;
using Aislinn.Core.Models;

public class SimpleActivationModel : IActivationModel
{
    private readonly double _baseActivation;
    private readonly double _decayRate;
    private readonly double _spreadingFactor;

    public SimpleActivationModel(double baseActivation = 1.0, double decayRate = 0.5, double spreadingFactor = 0.5)
    {
        _baseActivation = baseActivation;
        _decayRate = decayRate;
        _spreadingFactor = spreadingFactor;
    }

    public double CalculateActivation(Chunk chunk, DateTime? currentTime = null)
    {
        // Simple boost model - just add base activation to current level
        return chunk.ActivationLevel + _baseActivation;
    }

    public double ApplyDecay(Chunk chunk, double timeSinceLastUpdate)
    {
        // Simple linear decay
        double newActivation = chunk.ActivationLevel * (1 - _decayRate * timeSinceLastUpdate);
        return Math.Max(0, newActivation); // Don't allow negative activation
    }

    public double CalculateSpreadingActivation(Chunk sourceChunk, Chunk targetChunk, double associationWeight, double spreadingFactor)
    {
        // Simple spreading activation based on source activation, weight, and spreading factor
        return sourceChunk.ActivationLevel * associationWeight * spreadingFactor * _spreadingFactor;
    }
}