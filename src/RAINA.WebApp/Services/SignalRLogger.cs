using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RAINA.Web.Hubs;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RAINA.Logging
{
    /// <summary>
    /// Configuration options for SignalR logging provider
    /// </summary>
    public class SignalRLoggerConfiguration
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
        public List<string> AllowedCategories { get; set; } = new List<string> { "RAINA.*", "Aislinn.*" };
        public List<string> ExcludeCategories { get; set; } = new List<string> { "Microsoft.*", "System.*" };
        public string HubMethodName { get; set; } = "ReceiveLogMessage";
    }

    // Note: Using existing RainaHub instead of separate LoggingHub

    /// <summary>
    /// Log message structure sent to clients - matches React DebugLog interface
    /// </summary>
    public class DebugLog
    {
        public int Id { get; set; }
        public string Timestamp { get; set; }
        public string Level { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Logger implementation that broadcasts to SignalR clients
    /// </summary>
    public class SignalRLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<SignalRLoggerConfiguration> _getCurrentConfig;
        private static int _logIdCounter = 0;
        private IHubContext<RAINA.Web.Hubs.RainaHub> _hubContext;

        public SignalRLogger(
            string categoryName,
            IServiceProvider serviceProvider,
            Func<SignalRLoggerConfiguration> getCurrentConfig)
        {
            _categoryName = categoryName;
            _serviceProvider = serviceProvider;
            _getCurrentConfig = getCurrentConfig;
        }

        private IHubContext<RainaHub> GetHubContext()
        {
            if (_hubContext == null)
            {
                try
                {
                    _hubContext = _serviceProvider.GetService<IHubContext<RainaHub>>();
                }
                catch
                {
                    // Hub context not available yet, will try again later
                    return null;
                }
            }
            return _hubContext;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new SignalRLogScope<TState>(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            var config = _getCurrentConfig();

            // Check minimum level
            if (logLevel < config.MinimumLevel)
                return false;

            // Check category filters
            return IsCategoryAllowed(_categoryName, config);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var config = _getCurrentConfig();
            var message = formatter(state, exception);

            // Include exception in message if present
            if (exception != null)
            {
                message += $"\n{exception}";
            }

            var debugLog = new DebugLog
            {
                Id = Interlocked.Increment(ref _logIdCounter),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Level = ConvertLogLevel(logLevel),
                Category = _categoryName,
                Message = message
            };

            // Broadcast to all connected clients
            _ = Task.Run(async () =>
            {
                try
                {
                    var hubContext = GetHubContext();
                    if (hubContext != null)
                    {
                        //Console.WriteLine("SIGNALR LOG: " + debugLog);
                        await hubContext.Clients.All.SendAsync(config.HubMethodName, debugLog);
                    }
                    // If hubContext is null, just ignore - we're probably during startup
                }
                catch (Exception ex)
                {
                    // Avoid logging loops - could write to a fallback logger here
                    System.Diagnostics.Debug.WriteLine($"Failed to send log to SignalR: {ex.Message}");
                }
            });
        }

        private string ConvertLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "DEBUG",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARNING",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "ERROR",
                _ => "INFO"
            };
        }

        private bool IsCategoryAllowed(string categoryName, SignalRLoggerConfiguration config)
        {
            // Check exclusions first
            foreach (var exclude in config.ExcludeCategories)
            {
                if (IsWildcardMatch(categoryName, exclude))
                    return false;
            }

            // Check allowed categories
            if (config.AllowedCategories.Count == 0)
                return true; // No restrictions

            foreach (var allowed in config.AllowedCategories)
            {
                if (IsWildcardMatch(categoryName, allowed))
                    return true;
            }

            return false;
        }

        private bool IsWildcardMatch(string input, string pattern)
        {
            if (pattern.EndsWith("*"))
            {
                var prefix = pattern.Substring(0, pattern.Length - 1);
                return input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(input, pattern, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Scope implementation for SignalR logger
    /// </summary>
    internal class SignalRLogScope<TState> : IDisposable
    {
        private readonly TState _state;

        public SignalRLogScope(TState state)
        {
            _state = state;
        }

        public void Dispose()
        {
            // Scope cleanup if needed
        }
    }

    /// <summary>
    /// Provider that creates SignalR loggers
    /// </summary>
    [ProviderAlias("SignalR")]
    public class SignalRLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDisposable _optionsChangeToken;
        private SignalRLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, SignalRLogger> _loggers = new();

        public SignalRLoggerProvider(
            IServiceProvider serviceProvider,
            IOptionsMonitor<SignalRLoggerConfiguration> options)
        {
            _serviceProvider = serviceProvider;
            _currentConfig = options.CurrentValue;
            _optionsChangeToken = options.OnChange(config => _currentConfig = config);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName,
                name => new SignalRLogger(name, _serviceProvider, () => _currentConfig));
        }

        public void Dispose()
        {
            _optionsChangeToken?.Dispose();
            _loggers.Clear();
        }
    }

    /// <summary>
    /// Extension methods for registering SignalR logger
    /// </summary>
    public static class SignalRLoggerExtensions
    {
        public static ILoggingBuilder AddSignalRLogger(this ILoggingBuilder builder)
        {
            return builder.AddSignalRLogger(_ => { });
        }

        public static ILoggingBuilder AddSignalRLogger(
            this ILoggingBuilder builder,
            Action<SignalRLoggerConfiguration> configure)
        {
            builder.Services.Configure(configure);
            builder.Services.AddSingleton<ILoggerProvider, SignalRLoggerProvider>();
            return builder;
        }
    }
}