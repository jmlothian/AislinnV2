using System;
using System.Linq;
using Aislinn.Core.Models;
using Aislinn.Core.Services;

namespace Aislinn.Core.Activation
{
    /// <summary>
    /// ACT-R style activation model using internal system time instead of wall-clock time
    /// </summary>
    public class ActRActivationModel : IActivationModel
    {
        private readonly CognitiveTimeManager _timeManager;
        private readonly ActivationParametersRegistry _parametersRegistry;
        private readonly Random _random;

        public ActRActivationModel(
            CognitiveTimeManager timeManager,
            ActivationParametersRegistry parametersRegistry)
        {
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
            _parametersRegistry = parametersRegistry;

            _random = new Random();
        }

        public double CalculateActivation(Chunk chunk, DateTime? currentTime = null)
        {
            // Get parameters for this chunk type
            var parameters = _parametersRegistry.GetParameters(chunk);
            // Get current system time
            long currentSystemTime = _timeManager.GetCognitiveSteps();

            // Calculate base-level activation using ACT-R equation
            double baseLevelActivation = CalculateBaseLevelActivation(chunk, currentSystemTime, parameters.DecayRate);


            // Add noise (gaussian noise as in ACT-R)
            double noise = parameters.ActivationNoise > 0 ? GenerateActivationNoise(parameters.ActivationNoise) : 0;
            // Return total activation with dampening
            double totalActivation = baseLevelActivation + parameters.BaseActivationBoost + noise;
            totalActivation = ApplyActivationDampening(chunk.ActivationLevel, totalActivation);

            return Math.Min(parameters.ActivationCeiling, totalActivation);
        }
        //dampening method to reduce activation values the closer they get to the ceiling
        public double ApplyActivationDampening(double currentActivation, double newActivation)
        {
            // Diminishing returns for high activations
            if (newActivation > 1.0)
            {
                double excess = newActivation - 1.0;
                newActivation = 1.0 + (excess * 0.5); // Halve the excess
            }
            return newActivation;
        }
        private double CalculateBaseLevelActivation(Chunk chunk, long currentSystemTime, double decayRate)
        {
            if (chunk.ActivationHistory == null || chunk.ActivationHistory.Count == 0)
                return -2.0;

            double sum = 0;

            // Sum over all previous accesses
            foreach (var history in chunk.ActivationHistory)
            {
                // Calculate time since this access in system time units
                long timeElapsed = currentSystemTime - history.ActivationDate;

                // Avoid division by zero or negative time
                if (timeElapsed <= 1)
                    timeElapsed = 1;
                double timeElapsedSeconds = timeElapsed / 10000.0;

                // Add this access's contribution to activation
                sum += Math.Pow(timeElapsedSeconds, -decayRate);
            }

            // ACT-R equation: Bi = ln(Σj tj^-d)
            //return sum > 0 ? Math.Log(sum) : -10; // Floor value if no activations
            // Cap individual contributions to prevent runaway activation

            return sum > 0 ? Math.Log(Math.Min(sum, 100.0)) : -10.0;
        }

        private double GenerateActivationNoise(double noiseLevel)
        {
            // Box-Muller transform for Gaussian noise
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return z * noiseLevel;
        }

        public double ApplyDecay(Chunk chunk, double timeSinceLastUpdate)
        {
            // ACT-R doesn't use a separate decay function - decay is built into the base-level 
            // learning equation. Recalculate activation which will reflect the passage of time.
            return CalculateActivation(chunk);
        }

        public double CalculateSpreadingActivation(Chunk sourceChunk, Chunk targetChunk, double associationWeight, double spreadingFactor)
        {
            // Get parameters for the source chunk type
            var parameters = _parametersRegistry.GetParameters(sourceChunk);

            // Use the most recent activation increment (change) for spreading
            double activationIncrement = 0.0;
            if (sourceChunk.ActivationHistory != null && sourceChunk.ActivationHistory.Count > 0)
            {
                activationIncrement = sourceChunk.ActivationHistory[0].Change;
            }
            else
            {
                activationIncrement = sourceChunk.ActivationLevel;
            }

            // ACT-R spreading activation calculation with type-specific factor
            return activationIncrement * associationWeight * spreadingFactor * parameters.SpreadingFactor;
            //return sourceChunk.ActivationLevel * associationWeight * spreadingFactor * parameters.SpreadingFactor;
        }
    }
}