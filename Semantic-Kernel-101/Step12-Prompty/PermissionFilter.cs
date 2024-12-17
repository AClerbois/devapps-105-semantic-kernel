using Microsoft.SemanticKernel;

class PermissionFilter : IFunctionInvocationFilter
{
    public Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"Ok to invoke {context.Function.Name} with {string.Join(", ", context.Arguments)}, say y to continue");

        if (Console.ReadLine() == "y")
        {
            return next(context);
        }

        throw new InvalidOperationException("Permission denied");
    }
}