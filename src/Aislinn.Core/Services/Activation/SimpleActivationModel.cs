using Aislinn.Core.Activation;
using Aislinn.Core.Models;

public class SimpleActivationModel : IActivationModel
{
    private readonly ActivationParametersRegistry _parametersRegistry;

    public SimpleActivationModel(ActivationParametersRegistry parametersRegistry = null)
    {
        _parametersRegistry = parametersRegistry ?? new ActivationParametersRegistry();
    }

    public double CalculateActivation(Chunk chunk, DateTime? currentTime = null)
    {
        if (chunk == null)
            throw new ArgumentNullException(nameof(chunk));

        // Get parameters for this chunk type
        var parameters = _parametersRegistry.GetParameters(chunk);

        // Simple boost model - add type-specific base boost to current level
        double newActivation = chunk.ActivationLevel + parameters.BaseActivationBoost;

        // Apply activation ceiling
        return Math.Min(parameters.ActivationCeiling, newActivation);
    }

    public double ApplyDecay(Chunk chunk, double timeSinceLastUpdate)
    {
        if (chunk == null)
            throw new ArgumentNullException(nameof(chunk));

        // Get parameters for this chunk type
        var parameters = _parametersRegistry.GetParameters(chunk);

        // Simple linear decay with type-specific decay rate
        double newActivation = chunk.ActivationLevel * (1 - parameters.DecayRate * timeSinceLastUpdate);

        // Don't allow negative activation
        return Math.Max(0, newActivation);
    }

    public double CalculateSpreadingActivation(Chunk sourceChunk, Chunk targetChunk, double associationWeight, double spreadingFactor)
    {
        if (sourceChunk == null)
            throw new ArgumentNullException(nameof(sourceChunk));

        // Get parameters for the source chunk type
        var parameters = _parametersRegistry.GetParameters(sourceChunk);

        // Simple spreading activation based on source activation, weight, and spreading factor
        return sourceChunk.ActivationLevel * associationWeight * spreadingFactor * parameters.SpreadingFactor;
    }
}