using System.Threading.Tasks;

namespace RAINA.Modules
{
    /// <summary>
    /// Interface for intent modules in RAINA
    /// </summary>
    public interface IIntentModule
    {
        /// <summary>
        /// Gets the unique identifier for this intent type
        /// </summary>
        string GetIntentType();

        /// <summary>
        /// Gets the description of this intent for use in the OpenAI prompt
        /// </summary>
        string GetPromptDescription();

        /// <summary>
        /// Gets examples of this intent type for use in the OpenAI prompt
        /// </summary>
        string[] GetPromptExamples();

        /// <summary>
        /// Gets the expected entities for this intent type
        /// </summary>
        string[] GetExpectedEntities();

        /// <summary>
        /// Gets the expected parameters for this intent type
        /// </summary>
        string[] GetExpectedParameters();

        /// <summary>
        /// Handles this specific intent type
        /// </summary>
        Task<Response> HandleAsync(string userInput, Intent intent, UserContext context);
    }
}