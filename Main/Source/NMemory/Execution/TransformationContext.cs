using System.Linq.Expressions;

namespace NMemory.Execution
{
    public class TransformationContext
    {
        private ParameterExpression parameter;

        public TransformationContext(ParameterExpression parameter)
        {
            this.parameter = parameter;
        }

        public bool IsFinished 
        { 
            get; set; 
        }

        public ParameterExpression Parameter 
        {
            get { return this.parameter; }
        }
    }
}
