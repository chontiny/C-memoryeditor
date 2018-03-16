namespace Squalr.Engine.Types
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// A class representing a serializable data type.
    /// </summary>
    [DataContract]
    public class DataType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataType" /> class.
        /// </summary>
        public DataType() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataType" /> class.
        /// </summary>
        /// <param name="type">The default type.</param>
        public DataType(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets the type wrapped by this class.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the string of the full namespace path representing this type.
        /// </summary>
        [DataMember]
        private String TypeString
        {
            get
            {
                return this.Type?.FullName;
            }

            set
            {
                this.Type = value == null ? null : Type.GetType(value);
            }
        }

        /// <summary>
        /// Implicitly converts a DataType to a Type for comparisons.
        /// </summary>
        /// <param name="dataType">The DataType to convert.</param>
        public static implicit operator Type(DataType dataType)
        {
            return dataType?.Type;
        }

        /// <summary>
        /// Implicitly converts a Type to a DataType for comparisons.
        /// </summary>
        /// <param name="type">The Type to convert.</param>
        public static implicit operator DataType(Type type)
        {
            return new DataType(type);
        }

        /// <summary>
        /// Indicates whether this object is equal to another.
        /// </summary>
        /// <param name="self">The object being compared.</param>
        /// <param name="other">The other object.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public static Boolean operator ==(DataType self, DataType other)
        {
            if (Object.ReferenceEquals(self, other))
            {
                return true;
            }

            return self?.Type == other?.Type;
        }

        /// <summary>
        /// Indicates whether this object is not equal to another.
        /// </summary>
        /// <param name="self">The object being compared.</param>
        /// <param name="other">The other object.</param>
        /// <returns>True if not equal, otherwise false.</returns>
        public static Boolean operator !=(DataType self, DataType other)
        {
            return !(self == other);
        }

        /// <summary>
        /// Indicates whether this object is equal to another.
        /// </summary>
        /// <param name="self">The object being compared.</param>
        /// <param name="other">The other object.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public static Boolean operator ==(DataType self, Type other)
        {
            if (Object.ReferenceEquals(self, other))
            {
                return true;
            }

            return self?.Type == other;
        }

        /// <summary>
        /// Indicates whether this object is not equal to another.
        /// </summary>
        /// <param name="self">The object being compared.</param>
        /// <param name="other">The other object.</param>
        /// <returns>True if not equal, otherwise false.</returns>
        public static Boolean operator !=(DataType self, Type other)
        {
            return !(self == other);
        }

        /// <summary>
        /// Indicates whether this object is equal to another.
        /// </summary>
        /// <param name="self">The object being compared.</param>
        /// <param name="other">The other object.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public static Boolean operator ==(Type self, DataType other)
        {
            if (Object.ReferenceEquals(self, other))
            {
                return true;
            }

            return self == other?.Type;
        }

        /// <summary>
        /// Indicates whether this object is not equal to another.
        /// </summary>
        /// <param name="self">The object being compared.</param>
        /// <param name="other">The other object.</param>
        /// <returns>True if not equal, otherwise false.</returns>
        public static Boolean operator !=(Type self, DataType other)
        {
            return !(self == other);
        }

        /// <summary>
        /// Returns a hashcode for this instance.
        /// </summary>
        /// <returns>A hashcode for this instance.</returns>
        public override Int32 GetHashCode()
        {
            return this.Type.GetHashCode();
        }

        /// <summary>
        /// Indicates whether <see cref="DataType" /> objects are equal.
        /// </summary>
        /// <param name="dataType">The other <see cref="DataType" />.</param>
        /// <returns>True if the objects have an equal value.</returns>
        public override Boolean Equals(Object dataType)
        {
            return this.Type == (dataType as DataType)?.Type;
        }

        /// <summary>
        /// Indicates whether <see cref="DataType" /> objects are equal.
        /// </summary>
        /// <param name="dataType">The other <see cref="DataType" />.</param>
        /// <returns>True if the objects have an equal value.</returns>
        public Boolean Equals(DataType dataType)
        {
            return this.Type == dataType?.Type;
        }

        /// <summary>
        /// Returns a <see cref="String" /> representing the name of the current <see cref="DataType" />.
        /// </summary>
        /// <returns>The <see cref="String" /> representing the name of the current <see cref="DataType" /></returns>
        public override String ToString()
        {
            return this.Type?.ToString();
        }
    }
    //// End class
}
//// End namespace