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
        private readonly double _decayParameter;
        private readonly double _spreadingFactor;
        private readonly double _activationNoise;
        private readonly double _baseConstant;
        private readonly Random _random;

        public ActRActivationModel(
            CognitiveTimeManager timeManager,
            double decayParameter = 0.5,
            double spreadingFactor = 0.2,
            double activationNoise = 0.1,
            double baseConstant = 0.0)
        {
            _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
            _decayParameter = decayParameter;
            _spreadingFactor = spreadingFactor;
            _activationNoise = activationNoise;
            _baseConstant = baseConstant;
            _random = new Random();
        }

        public double CalculateActivation(Chunk chunk, DateTime? currentTime = null)
        {
            // Get current system time
            double currentSystemTime = _timeManager.GetCurrentTime();

            // Calculate base-level activation using ACT-R equation
            double baseLevelActivation = CalculateBaseLevelActivation(chunk, currentSystemTime);

            // Add noise (gaussian noise as in ACT-R)
            double noise = _activationNoise > 0 ? GenerateActivationNoise() : 0;

            // Return total activation
            return baseLevelActivation + _baseConstant + noise;
        }

        private double CalculateBaseLevelActivation(Chunk chunk, double currentSystemTime)
        {
            if (chunk.ActivationHistory == null || chunk.ActivationHistory.Count == 0)
                return 0;

            double sum = 0;

            // Sum over all previous accesses
            foreach (var history in chunk.ActivationHistory)
            {
                // Convert the historical activation date to system time
                double historySystemTime = _timeManager.ConvertToSystemTime(history.ActivationDate);

                // Calculate time since this access in system time units
                double timeElapsed = currentSystemTime - historySystemTime;

                // Avoid division by zero or negative time
                if (timeElapsed <= 0.001)
                    timeElapsed = 0.001;

                // Add this access's contribution to activation
                sum += Math.Pow(timeElapsed, -_decayParameter);
            }

            // ACT-R equation: Bi = ln(Σj tj^-d)
            return sum > 0 ? Math.Log(sum) : -10; // Floor value if no activations
        }

        private double GenerateActivationNoise()
        {
            // Box-Muller transform for Gaussian noise
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return z * _activationNoise;
        }

        public double ApplyDecay(Chunk chunk, double timeSinceLastUpdate)
        {
            // ACT-R doesn't use a separate decay function - decay is built into the base-level 
            // learning equation. Recalculate activation which will reflect the passage of time.
            return CalculateActivation(chunk);
        }

        public double CalculateSpreadingActivation(Chunk sourceChunk, Chunk targetChunk, double associationWeight, double spreadingFactor)
        {
            // ACT-R spreading activation calculation
            return sourceChunk.ActivationLevel * associationWeight * spreadingFactor * _spreadingFactor;
        }
    }
}