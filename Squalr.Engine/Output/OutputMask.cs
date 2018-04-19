namespace Squalr.Engine.Output
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Masks sensitive data from appearing in the output.
    /// </summary>
    public class OutputMask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputMask" /> class.
        /// </summary>
        /// <param name="filterRegex">The regular expression used to filter output text.</param>
        /// <param name="replacementText">The text that replaces the redacted text.</param>
        public OutputMask(String filterRegex, String replacementText = "{{REDACTED}}")
        {
            this.ReplacementText = replacementText;

            this.FilterRegex = new Regex(filterRegex);
        }

        /// <summary>
        /// Gets or sets the text that replaces redacted text.
        /// </summary>
        private String ReplacementText { get; set; }

        /// <summary>
        /// Gets or sets the regular expression used to filter output text.
        /// </summary>
        private Regex FilterRegex { get; set; }

        /// <summary>
        /// Applies the regular expression filter to the given message.
        /// </summary>
        /// <param name="message">The message to filter.</param>
        /// <returns>The filtered message.</returns>
        public String ApplyFilter(String message)
        {
            return message == null ? null : this.FilterRegex.Replace(message, this.ReplacementText);
        }
    }
    //// End class
}
//// End namespace