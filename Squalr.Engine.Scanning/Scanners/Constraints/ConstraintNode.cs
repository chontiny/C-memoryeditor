namespace Squalr.Engine.Scanning.Scanners.Constraints
{
    using System;

    public class ConstraintNode
    {
        public ConstraintNode()
        {
        }

        public ConstraintNode Left { get; set; }

        public ConstraintNode Right { get; set; }

        /// <summary>
        /// Gets the element type of this constraint manager.
        /// </summary>
        public Type ElementType { get; private set; }

        /// <summary>
        /// Sets the element type to which all constraints apply.
        /// </summary>
        /// <param name="elementType">The new element type.</param>
        public virtual void SetElementType(Type elementType)
        {
            this.ElementType = elementType;

            this.Left?.SetElementType(elementType);
            this.Right?.SetElementType(elementType);
        }

        public virtual Boolean IsValid()
        {
            return (this.Left?.IsValid() ?? true) && (this.Right?.IsValid() ?? true);
        }

        /// <summary>
        /// Determines if there is any constraint being managed which uses a relative value constraint, requiring previous values.
        /// </summary>
        /// <returns>True if any constraint has a relative value constraint.</returns>
        public virtual Boolean HasRelativeConstraint()
        {
            return (this.Left?.HasRelativeConstraint() ?? false) || (this.Right?.HasRelativeConstraint() ?? false);
        }

        public Int32 Count
        {
            get
            {
                return (this as ScanConstraint != null ? 1 : 0) + (this.Left?.Count ?? 0) + (this.Right?.Count ?? 0);
            }
        }
    }
    //// End class
}
//// End namespace