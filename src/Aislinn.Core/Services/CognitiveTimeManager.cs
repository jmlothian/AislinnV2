using System;
using System.IO;
using System.Text.Json;

namespace Aislinn.Core.Services
{
    /// <summary>
    /// Manages an internal time reference for the cognitive system that persists across sessions
    /// </summary>
    public class CognitiveTimeManager
    {
        private const string DEFAULT_STATE_FILE = "agent_state.json";
        private string _stateFilePath;

        // The time when the agent was first started (in seconds)
        public double SystemStartTime { get; private set; } = 0;

        // Current system time (in seconds since start)
        public double CurrentTime { get; private set; } = 0;

        // Last time the state was saved
        public DateTime LastSaveTime { get; private set; }

        public CognitiveTimeManager(string stateFilePath = DEFAULT_STATE_FILE)
        {
            _stateFilePath = stateFilePath;
            LoadState();
        }

        /// <summary>
        /// Load the time state from persistent storage
        /// </summary>
        public void LoadState()
        {
            if (File.Exists(_stateFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_stateFilePath);
                    var state = JsonSerializer.Deserialize<AgentState>(json);

                    if (state != null)
                    {
                        SystemStartTime = state.SystemStartTime;
                        CurrentTime = state.CurrentTime;

                        // Calculate time passed since last save
                        TimeSpan timeSinceLastSave = DateTime.Now - state.LastSaveTime;

                        // Update current time to account for the time that passed while system was offline
                        CurrentTime += timeSinceLastSave.TotalSeconds;
                        LastSaveTime = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading agent state: {ex.Message}");
                    InitializeNewState();
                }
            }
            else
            {
                InitializeNewState();
            }
        }

        /// <summary>
        /// Initialize a new time state
        /// </summary>
        private void InitializeNewState()
        {
            SystemStartTime = 0;
            CurrentTime = 0;
            LastSaveTime = DateTime.Now;
        }

        /// <summary>
        /// Save the current time state to persistent storage
        /// </summary>
        public void SaveState()
        {
            var state = new AgentState
            {
                SystemStartTime = SystemStartTime,
                CurrentTime = CurrentTime,
                LastSaveTime = DateTime.Now
            };

            string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_stateFilePath, json);
        }

        /// <summary>
        /// Update the current system time based on elapsed real time
        /// </summary>
        public double UpdateTime()
        {
            TimeSpan timeSinceLastSave = DateTime.Now - LastSaveTime;
            CurrentTime += timeSinceLastSave.TotalSeconds;
            LastSaveTime = DateTime.Now;
            return CurrentTime;
        }

        /// <summary>
        /// Gets the current system time, updating it first
        /// </summary>
        public double GetCurrentTime()
        {
            return UpdateTime();
        }

        /// <summary>
        /// Convert a real DateTime to internal system time
        /// </summary>
        public double ConvertToSystemTime(DateTime dateTime)
        {
            return (dateTime - LastSaveTime).TotalSeconds + CurrentTime;
        }

        /// <summary>
        /// Class to represent the agent's persistent state
        /// </summary>
        private class AgentState
        {
            public double SystemStartTime { get; set; }
            public double CurrentTime { get; set; }
            public DateTime LastSaveTime { get; set; }
        }
    }
}