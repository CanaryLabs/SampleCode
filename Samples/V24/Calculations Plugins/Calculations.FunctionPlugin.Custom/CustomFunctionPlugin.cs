using Canary.Calculations.FunctionPlugin.Base;

namespace YourCompany.Calculations.FunctionPlugin.Custom
{
    public class CustomFunctionPlugin : IStaticFunctionPlugin
    {
        /// <summary>
        /// Required namespace prepended to all function names in this plugin.
        /// Use alphanumeric characters and underscores only, must start with letter or underscore.
        /// Example: If Namespace = "Custom" and function name = "Add", it's called as "Custom_Add()"
        /// </summary>
        public string Namespace { get; } = "Custom";

        /// <summary>
        /// Called by the Calculations Service to load all functions from this plugin.
        /// </summary>
        public List<IStaticFunction> CreateStaticFunctions()
        {
            return new List<IStaticFunction>
            {
                new AddFunction(),
                // Add all your custom functions here
            };
        }
    }
}
