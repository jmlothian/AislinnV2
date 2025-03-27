using System;
using System.Collections.Generic;
using System.Linq;
using Aislinn.Core.Models;

namespace Aislinn.Core.Activation
{
    /// <summary>
    /// Interface defining an activation model that can be plugged into the cognitive architecture
    /// </summary>
    public interface IActivationModel
    {
        /// <summary>
        /// Calculate the activation level for a chunk based on its history and current state
        /// </summary>
        /// <param name="chunk">The chunk to calculate activation for</param>
        /// <param name="currentTime">Current time for reference (defaults to DateTime.Now)</param>
        /// <returns>The calculated activation level</returns>
        double CalculateActivation(Chunk chunk, DateTime? currentTime = null);

        /// <summary>
        /// Apply activation decay over time
        /// </summary>
        /// <param name="chunk">The chunk to apply decay to</param>
        /// <param name="timeSinceLastUpdate">Time elapsed since last update (in seconds)</param>
        /// <returns>The new activation level after decay</returns>
        double ApplyDecay(Chunk chunk, double timeSinceLastUpdate);

        /// <summary>
        /// Calculate spreading activation from source chunk to target chunk
        /// </summary>
        /// <param name="sourceChunk">The source of spreading activation</param>
        /// <param name="targetChunk">The target receiving activation</param>
        /// <param name="associationWeight">Weight of association between chunks</param>
        /// <param name="spreadingFactor">Factor controlling spreading intensity</param>
        /// <returns>Amount of activation to spread to target</returns>
        double CalculateSpreadingActivation(Chunk sourceChunk, Chunk targetChunk, double associationWeight, double spreadingFactor);
    }
}