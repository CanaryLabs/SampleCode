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
        double value1;
        try
        {
            value1 = Convert.ToDouble(await args[0].EvaluateAsync());
        }
        catch
        {
            throw new FunctionArgumentValidationException("First argument must be a number");
        }

        // Evaluate and validate second argument
        double value2;
        try
        {
            value2 = Convert.ToDouble(await args[1].EvaluateAsync());
        }
        catch
        {
            throw new FunctionArgumentValidationException("Second argument must be a number");
        }

        // Perform calculation
        double result = value1 + value2;

        // Return value and quality
        // Quality is a ushort (0-65535). 0xC0 (192) is commonly used for successful calculations
        return (result, 0xC0);
    }
}
