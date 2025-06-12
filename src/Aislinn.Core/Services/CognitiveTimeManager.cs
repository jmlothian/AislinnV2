using System;
using System.IO;
using System.Text.Json;

namespace Aislinn.Core.Services
{
    /// <summary>
    /// Manages three distinct time references for the cognitive system
    /// </summary>
    public class CognitiveTimeManager
    {
        private const string DEFAULT_STATE_FILE = "agent_state.json";
        private string _stateFilePath;

        // Real wall-clock time
        public DateTime RealTime => DateTime.Now;

        // Agent processing time (only advances when agent is actively thinking)
        public double AgentTime { get; private set; } = 0;
        private DateTime _lastAgentTimeUpdate = DateTime.Now;
        private bool _isProcessing = false;

        // Cognitive step time (discrete units, advances manually)
        public long CognitiveSteps { get; private set; } = 0;

        public CognitiveTimeManager(string stateFilePath = DEFAULT_STATE_FILE)
        {
            _stateFilePath = stateFilePath;
            LoadState();
        }

        /// <summary>
        /// Start counting agent processing time
        /// </summary>
        public void StartProcessing()
        {
            if (!_isProcessing)
            {
                _isProcessing = true;
                _lastAgentTimeUpdate = DateTime.Now;
            }
        }

        /// <summary>
        /// Stop counting agent processing time (for pauses, debugging, etc.)
        /// </summary>
        public void PauseProcessing()
        {
            if (_isProcessing)
            {
                UpdateAgentTime();
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Advance cognitive step time by one unit
        /// </summary>
        public long AdvanceStep(long milliseconds = 100)
        {
            CognitiveSteps += milliseconds;
            return CognitiveSteps;
        }

        /// <summary>
        /// Get current agent time (updates if currently processing)
        /// </summary>
        public double GetAgentTime()
        {
            if (_isProcessing)
            {
                UpdateAgentTime();
            }
            return AgentTime;
        }

        /// <summary>
        /// Get current cognitive steps
        /// </summary>
        public long GetCognitiveSteps()
        {
            return CognitiveSteps;
        }

        /// <summary>
        /// Get real time
        /// </summary>
        public DateTime GetRealTime()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Update agent time based on elapsed real time (only if processing)
        /// </summary>
        private void UpdateAgentTime()
        {
            if (_isProcessing)
            {
                TimeSpan elapsed = DateTime.Now - _lastAgentTimeUpdate;
                AgentTime += elapsed.TotalSeconds;
                _lastAgentTimeUpdate = DateTime.Now;
            }
        }

        /// <summary>
        /// Convert a real DateTime to agent time (approximate)
        /// </summary>
        public double ConvertToAgentTime(DateTime dateTime)
        {
            // This is approximate - assumes continuous processing
            return AgentTime - (DateTime.Now - dateTime).TotalSeconds;
        }

        /// <summary>
        /// Load state from file
        /// </summary>
        public void LoadState()
        {
            if (File.Exists(_stateFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_stateFilePath);
                    var state = JsonSerializer.Deserialize<TimeState>(json);

                    if (state != null)
                    {
                        AgentTime = state.AgentTime;
                        CognitiveSteps = state.CognitiveSteps;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading time state: {ex.Message}");
                    InitializeNewState();
                }
            }
            else
            {
                InitializeNewState();
            }

            _lastAgentTimeUpdate = DateTime.Now;
        }

        /// <summary>
        /// Save state to file
        /// </summary>
        public void SaveState()
        {
            UpdateAgentTime(); // Make sure agent time is current

            var state = new TimeState
            {
                AgentTime = AgentTime,
                CognitiveSteps = CognitiveSteps,
                LastSaveTime = DateTime.Now
            };

            string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_stateFilePath, json);
        }

        /// <summary>
        /// Initialize new state
        /// </summary>
        private void InitializeNewState()
        {
            AgentTime = 0;
            CognitiveSteps = 0;
        }

        /// <summary>
        /// State persistence class
        /// </summary>
        private class TimeState
        {
            public double AgentTime { get; set; }
            public long CognitiveSteps { get; set; }
            public DateTime LastSaveTime { get; set; }
        }
    }
}