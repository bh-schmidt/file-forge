using FileForge.Maps;
using Flee.PublicTypes;

namespace FileForge.Helpers
{
    public class ExpressionEvaluation
    {
        public static bool Evaluate(string expression, IEnumerable<ParameterMap> parameters)
        {
            var context = new ExpressionContext();

            foreach (var parameter in parameters)
                context.Variables.Add(parameter.Name, parameter.Value!);

            var genericExpression = context.CompileGeneric<bool>(expression);
            return genericExpression.Evaluate();
        }
    }
}
