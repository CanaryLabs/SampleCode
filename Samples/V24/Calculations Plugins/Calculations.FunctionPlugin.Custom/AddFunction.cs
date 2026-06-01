using Canary.Calculations.FunctionPlugin.Base;

namespace Calculations.FunctionPlugin.Custom;

public class AddFunction : IStaticFunction
{
    public FunctionInformation FunctionInformation { get; }
    public IFunctionValidator? FunctionValidator { get; }

    public AddFunction()
    {
        FunctionInformation = new FunctionInformation()
        {
            FunctionName = "Add",
            FunctionDescription = "Adds two numbers together",
            ParameterList = new FunctionParameterGroup()
            {
                Parameters =
                {
                    new FunctionParameterSingle("value1", typeof(double)),
                    new FunctionParameterSingle("value2", typeof(double)),
                }
            },
            SyntaxHelp = "Add(value1, value2)"
        };
    }

    public async Task<(object value, ushort quality)> EvaluateAsync(
        FunctionArgument[] args,
        FunctionEvaluationProperties evaluationProperties)
    {
        // Validate argument count
        if (args.Length != 2)
            throw new FunctionArgumentValidationException("Add requires exactly 2 arguments");

        // Evaluate and validate first argument
        if (await args[0].EvaluateAsync() is not double value1)
            throw new FunctionArgumentValidationException("First argument must be a number");

        // Evaluate and validate second argument
        if (await args[1].EvaluateAsync() is not double value2)
            throw new FunctionArgumentValidationException("Second argument must be a number");

        // Perform calculation
        double result = value1 + value2;

        // Return value and quality
        // Quality is a ushort (0-65535). 0xC0 (192) is commonly used for successful calculations
        return (result, 0xC0);
    }
}
