using FileForge.Maps;
using Flee.PublicTypes;

namespace FileForge.Helpers
{
    public class ExpressionEvaluation
    {
        public static bool Evaluate(string expression, IEnumerable<VariableMap> variables)
        {
            var context = new ExpressionContext();

            foreach (var variable in variables)
                context.Variables.Add(variable.Name, variable.Answer!);

            var genericExpression = context.CompileGeneric<bool>(expression);
            return genericExpression.Evaluate();
        }
    }
}
