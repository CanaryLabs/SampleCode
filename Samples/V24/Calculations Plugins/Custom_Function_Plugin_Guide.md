# Custom Function Plugin Development Guide

This guide explains how to create custom calculation functions for Canary using the Function Plugin SDK.

Note:
> This will work for 26.3 and newer

## Overview

The Canary Calculations Service allows you to extend its functionality by creating custom function plugins. You can implement your own calculation functions that integrate seamlessly with the Canary UI and calculations engine.

### What You'll Get

When you install Canary, you receive:
- **Base Plugin SDK DLL**: `Canary.Calculations.FunctionPlugin.Base.dll` 
  - Location: `C:\Program Files\Canary\Calculations\`
  - Contains interfaces and base classes needed to create custom functions

- **Plugin Directory**: `C:\Program Files\Canary\Calculations\FunctionPlugins\`
  - Where your custom function plugins are deployed
  - Each plugin is a separate folder containing your plugin DLL and dependencies

## Getting Started

### Prerequisites

- Visual Studio 2019 or later (or compatible C# IDE)
- .NET 6.0 SDK or later
- Basic knowledge of C# and async/await patterns

### Create a New Plugin Project

1. Create a new C# Class Library project targeting .NET 6.0 or later
2. Add a reference to `Canary.Calculations.FunctionPlugin.Base.dll` from `C:\Program Files\Canary\Calculations\`
3. Implement the required interfaces (see below)

## Core Interfaces

### IStaticFunctionPlugin

The entry point for your plugin. Implement this interface in a single class per plugin DLL.

```csharp
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
                new MultiplyFunction(),
                // Add all your custom functions here
            };
        }
    }
}
```

**Namespace Rules:**
- May contain only alphanumeric characters and underscores
- Must begin with a letter or underscore
- The namespace may not be empty
- The namespace is prepended to function names with no separator

### IStaticFunction

Implement this interface for each custom function you want to provide.

```csharp
using Canary.Calculations.FunctionPlugin.Base;

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
```

## Key Components

### FunctionInformation

Describes your function to the UI and calculations engine:

```csharp
new FunctionInformation()
{
    FunctionName = "MyFunction",           // Required: alphanumeric only, starts with letter
    FunctionDescription = "...",           // Required: displayed in UI help
    ParameterList = new FunctionParameterGroup() { ... },  // Required: parameter definitions
    SyntaxHelp = "MyFunction(param1, ...)" // Optional: help text for syntax
}
```

### Parameter Definitions

Define what parameters your function accepts using `FunctionParameterSingle` and `FunctionParameterGroup`:

#### FunctionParameterSingle

A single parameter definition with the following properties:

```csharp
new FunctionParameterSingle(
    Name: "value",                    // Required: parameter name
    Type: typeof(double),             // Optional: expected type (null = any type)
    IsOptional: false,                // Optional: is this parameter optional?
    IsRepeatable: false,              // Optional: can this parameter appear multiple times?
    MustBeConstant: false,            // Optional: must the argument be a constant value?
    ShouldSkipTypeValidation: false   // Optional: skip type checking in UI and runtime?
)
```

**Parameter Details:**

- **Name** (string, required): The parameter name as it appears in function calls and UI documentation

- **Type** (Type?, optional): The expected data type. Examples: `typeof(double)`, `typeof(bool)`, `typeof(string)`
  - Set to `null` to accept any type
  - The system supports automatic type conversions for compatible types (e.g., `int` can be passed where `double` is expected)
  - Complex conversions that can't be done automatically require `ShouldSkipTypeValidation: true`

- **IsOptional** (bool, optional, defaults to false): Whether this parameter is required
  - `false` - Parameter must be provided
  - `true` - Parameter can be omitted
  - Optional parameters should typically be placed at the end of the parameter list
  - Example: `Func(required1, optional1)` or `Func(required1, required2, optional1, optional2)`

- **IsRepeatable** (bool, optional, defaults to false): Whether this parameter can appear multiple times
  - `false` - Parameter appears exactly once (or zero times if optional)
  - `true` - Parameter can be repeated multiple times in the function call
  - **Important**: Repeatable parameters **must be the last parameter** in the function, or the last parameter in a `FunctionParameterGroup`
  - **Why**: The parser needs to know when a repeatable parameter list ends and the next parameter begins. If a repeatable parameter isn't last, the parser can't determine where it ends.
  - Correct example:
    ```
    CanaryOr(condition1, condition2, condition3)  // ✓ repeatable is last
    ```
  - Incorrect example:
    ```
    Func(repeatable1, repeatable2, required2)  // ✗ repeatable in middle - ambiguous
    ```

- **MustBeConstant** (bool, optional, defaults to false): Whether the argument must be a constant value
  - `false` - Argument can be dynamic (e.g., tag data from a consumer, results from other functions)
  - `true` - Argument must be a static/constant value that doesn't change
  - Use when you need a fixed configuration value, not a dynamic expression
  - Examples of constant values: `123`, `'myKey'`, `true`, `3.14`
  - Examples of dynamic values (not allowed if `MustBeConstant: true`): tag references, expressions, function calls

- **ShouldSkipTypeValidation** (bool, optional, defaults to false): Whether to skip automatic type validation
  - `false` - The system validates that provided values match the specified Type (normal case)
  - `true` - Skip validation to allow special type conversions your code handles manually
  - **Only use when necessary** - When the automatic type conversion system can't handle your needs
  - When set to `true`, you're responsible for validating the type in your `EvaluateAsync` method

#### Examples

**Single required parameter:**
```csharp
new FunctionParameterSingle("number", typeof(double))
```

**Multiple parameters:**
```csharp
new FunctionParameterGroup()
{
    Parameters =
    {
        new FunctionParameterSingle("value1", typeof(double)),
        new FunctionParameterSingle("value2", typeof(double)),
    }
}
```

**Repeatable parameters (like AND/OR logic):**
```csharp
new FunctionParameterGroup()
{
    Parameters =
    {
        new FunctionParameterSingle("condition", typeof(bool), IsRepeatable: true)
    }
}
```

**Optional parameter:**
```csharp
new FunctionParameterGroup()
{
    Parameters =
    {
        new FunctionParameterSingle("value", typeof(double)),
        new FunctionParameterSingle("defaultValue", typeof(double), IsOptional: true)
    }
}
```

### EvaluateAsync Method

This method executes your function with the provided arguments.

**Parameters:**
- `FunctionArgument[] args` - Array of arguments to evaluate
- `FunctionEvaluationProperties evaluationProperties` - Context information including:
  - `EvaluationTime` (DateTime) - The time at which evaluation is occurring
  - `RelatedEventInfoAccessor` (optional) - Access to related event information

**Returns:**
- `(object value, ushort quality)` - The result and quality indicator
  - **value**: The calculated result (can be any primitive object type)
  - **quality**: A quality indicator (0-65535). Common value: `0xC0` (192) for successful evaluations

**Important Notes:**

1. **Async Evaluation**: Arguments are provided as `FunctionArgument` objects that must be awaited:
   ```csharp
   var evaluatedValue = await arg.EvaluateAsync();
   ```

2. **Type Checking**: Always validate that evaluated arguments are the expected type:
   ```csharp
   if (await arg.EvaluateAsync() is not double doubleValue)
       throw new FunctionArgumentValidationException("Expected a number");
   ```

## Example: Complete Boolean Logic Function

Here's a complete example implementing an OR function:

```csharp
using Canary.Calculations.FunctionPlugin.Base;

namespace YourCompany.Calculations.FunctionPlugin.Custom
{
    public class OrFunction : IStaticFunction
    {
        public OrFunction()
        {
            FunctionInformation = new FunctionInformation()
            {
                FunctionName = "Or",
                FunctionDescription = "Returns true if at least one condition is true",
                ParameterList = new FunctionParameterGroup()
                {
                    Parameters =
                    {
                        new FunctionParameterSingle("condition", typeof(bool), IsRepeatable: true)
                    }
                },
                SyntaxHelp = "Or(condition1, condition2, ...)"
            };
        }

        public FunctionInformation FunctionInformation { get; }
        public IFunctionValidator? FunctionValidator { get; } = null;

        public async Task<(object value, ushort quality)> EvaluateAsync(
            FunctionArgument[] args, 
            FunctionEvaluationProperties evaluationProperties)
        {
            foreach (var arg in args)
            {
                if (await arg.EvaluateAsync() is not bool boolValue)
                    throw new FunctionArgumentValidationException("All arguments must be boolean");

                if (boolValue)
                    return (true, 0xC0);  // Return early on first true
            }

            return (false, 0xC0);  // All conditions were false
        }
    }
}
```

## Deployment

### Build Your Plugin

1. Build your plugin project in Release configuration
2. The output DLL will be `YourPluginName.dll`

### Deploy to Canary

1. Create a folder in `C:\Program Files\Canary\Calculations\FunctionPlugins\`
   - Example: `C:\Program Files\Canary\Calculations\FunctionPlugins\MyCustomPlugin\`

2. Copy your plugin DLL to this folder:
   - `YourPluginName.dll`

3. Copy any third-party dependencies:
   - If a dependency is unique to your plugin, place it in the same folder
   - If a dependency is shared with other plugins or the main Canary installation, you can place it in `C:\Program Files\Canary\Calculations\` (the parent folder)
   - The Calculations Service will search both locations when loading your plugin

4. Restart the Canary Calculations Service (or the parent application that uses it)

5. Your custom functions will now be available in the UI

### Folder Structure Example

```
C:\Program Files\Canary\Calculations\
├── Canary.Calculations.FunctionPlugin.Base.dll    (provided by installer)
├── YourSharedDependency.dll                       (optional: shared by multiple plugins)
└── FunctionPlugins\
    └── MyCustomPlugin\
        ├── MyCustomPlugin.dll                     (your plugin)
        └── SomeUniqueLibrary.dll                  (optional: dependencies unique to this plugin)
```

## Best Practices

### Error Handling

Always validate inputs and provide clear error messages:

```csharp
public async Task<(object, ushort)> EvaluateAsync(...)
{
    var value1 = await args[0].EvaluateAsync();
    if (value1 == null)
        throw new FunctionArgumentValidationException(
            "First argument cannot be null");

    if (value1 is not double number)
        throw new FunctionArgumentValidationException(
            "First argument must be a number");
    
    // ... rest of implementation
}
```

### Type Safety

Use pattern matching and type checking:

```csharp
// Good - explicit type checking
if (await arg.EvaluateAsync() is not double value)
    throw new FunctionArgumentValidationException("Expected a double");

// Avoid - unsafe casting
var value = (double)await arg.EvaluateAsync();  // Could throw unexpected exceptions
```

### Async Operations

The evaluation method is async to support complex calculations:

```csharp
public async Task<(object, ushort)> EvaluateAsync(...)
{
    // Each argument evaluation is awaited separately
    var val1 = await args[0].EvaluateAsync();
    var val2 = await args[1].EvaluateAsync();
    
    // You can also perform async operations here
    var result = await SomeAsyncOperation(val1, val2);
    
    return (result, 0xC0);
}
```

### Quality Values

The quality value indicates the reliability/confidence of the result:

- `0xC0` (192) - Standard successful evaluation
- Other values can be used to indicate varying levels of data quality
- Choose appropriate quality values based on your calculation logic

## Troubleshooting

### Plugin Not Loading

1. **Check the folder structure** - Ensure your DLL is in `C:\Program Files\Canary\Calculations\FunctionPlugins\YourPlugin\`

2. **Verify DLL location** - The file should be accessible without admin elevation for the Calculations Service process

3. **Check dependencies** - Ensure all referenced assemblies are either:
   - In the plugin folder
   - In the parent `C:\Program Files\Canary\Calculations\` folder
   - In the GAC (Global Assembly Cache)
   - In the standard .NET directories

4. **Restart the service** - The plugin is loaded when the Calculations Service starts. Restart it after deploying

### Function Not Appearing in UI

1. **Check FunctionName** - Must be alphanumeric and start with a letter
2. **Check namespace prefix** - If you have a namespace, functions are called as `Namespace` + `FunctionName`
3. **Verify CreateStaticFunctions** - Ensure your function is returned from this method
4. **Review logs** - Check Canary application logs for load errors

### Runtime Errors

1. **Type mismatches** - Use the pattern matching approach shown above
2. **Null values** - Check for null returns from `EvaluateAsync()`
3. **Async issues** - Ensure you're properly awaiting argument evaluation

## Support

For issues or questions about the plugin SDK:
1. Check the example plugins provided with your Canary installation
2. Review this documentation
3. Contact Canary support with details about your implementation
