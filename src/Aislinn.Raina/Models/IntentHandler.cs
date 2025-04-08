using System.Threading.Tasks;

namespace RAINA.IntentHandling
{
    /// <summary>
    /// Interface for intent handlers in RAINA (Realtime Adaptive Intelligence Neural Assistant)
    /// </summary>
    public interface IIntentHandler
    {
        /// <summary>
        /// Handles a specific intent type
        /// </summary>
        /// <param name="userInput">The original user input text</param>
        /// <param name="intent">The parsed intent with entities and parameters</param>
        /// <param name="context">The current user context</param>
        /// <returns>A response to be sent back to the user</returns>
        Task<Response> HandleAsync(string userInput, Intent intent, UserContext context);

        /// <summary>
        /// Gets a description of what this intent handler does
        /// </summary>
        string GetDescription();

        /// <summary>
        /// Gets the intent type this handler is responsible for
        /// </summary>
        string GetIntentType();
    }
}