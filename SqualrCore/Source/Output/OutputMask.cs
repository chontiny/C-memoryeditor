namespace SqualrCore.Source.Output
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Masks sensitive data from appearing in the output.
    /// </summary>
    public class OutputMask
    {
        /// <summary>
        /// Creates an instance of the <see cref="OutputMask" /> class.
        /// </summary>
        public OutputMask(String filterRegex, String replacementText = "{{REDACTED}}")
        {
            this.ReplacementText = replacementText;

            this.FilterRegex = new Regex(filterRegex);
        }

        /// <summary>
        /// The text that replaces redacted text.
        /// </summary>
        private String ReplacementText { get; set; }

        /// <summary>
        /// The regular expression used to filter output text.
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