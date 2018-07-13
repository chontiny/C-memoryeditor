namespace Squalr.Engine.Scanning.Scanners.Constraints
{
    using System;

    /// <summary>
    /// Class for storing a collection of constraints to be used in a scan that applies more than one constraint per update.
    /// </summary>
    public class Operation : ConstraintNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanConstraintTree" /> class.
        /// </summary>
        public Operation(OperationType operation)
        {
            this.BinaryOperation = operation;
        }

        public OperationType BinaryOperation { get; private set; }

        public override Boolean IsValid()
        {
            return (this.Left?.IsValid() ?? false) && (this.Right?.IsValid() ?? false);
        }

        public enum OperationType
        {
            OR,
            AND,
        }
    }
    //// End class
}
//// End namespace