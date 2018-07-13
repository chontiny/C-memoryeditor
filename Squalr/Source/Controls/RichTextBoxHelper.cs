namespace Squalr.Source.Controls
{
    using Squalr.Source.Output;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    /// Class to allow the text in a rich text box text to be updated as part of a two way binding.
    /// </summary>
    public class RichTextBoxHelper : DependencyObject
    {
        /// <summary>
        /// Specifies the two way binding for a rich text box.
        /// </summary>
        public static readonly DependencyProperty DocumentRTFProperty = DependencyProperty.RegisterAttached(
            "DocumentRTF",
            typeof(String),
            typeof(RichTextBoxHelper),
            new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (obj, e) =>
                {
                    RichTextBox richTextBox = obj as RichTextBox;

                    // Parse the XAML to a document (or use XamlReader.Parse())
                    String rtf = GetDocumentRTF(richTextBox);
                    FlowDocument doc = new FlowDocument();
                    TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);

                    if (!String.IsNullOrEmpty(rtf))
                    {
                        range.Load(new MemoryStream(Encoding.UTF8.GetBytes(rtf)), DataFormats.Rtf);
                    }

                    // Set the document
                    richTextBox.Document = doc;

                    // Bind the hyperlink click events to the output view model handler
                    // Note: This violates the decoupling of this class from other classes by explicitly referencing OutputViewModel, but it will be permitted for now
                    foreach (Block block in richTextBox.Document.Blocks.ToArray())
                    {
                        if (!(block is Paragraph))
                        {
                            continue;
                        }

                        foreach (Inline inline in (block as Paragraph).Inlines.ToArray())
                        {
                            if (!(inline is Hyperlink))
                            {
                                continue;
                            }

                            Hyperlink link = inline as Hyperlink;
                            link.RequestNavigate += OutputViewModel.GetInstance().LinkRequestNavigate;

                            // Apply link coloring. Again, this violates decoupling
                            TextRange textRange = new TextRange(block.ContentStart, block.ContentEnd);
                            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DeepSkyBlue);
                        }
                    }

                    // When the document changes update the source
                    range.Changed += (obj2, e2) =>
                    {
                        if (richTextBox.Document == doc)
                        {
                            MemoryStream buffer = new MemoryStream();
                            range.Save(buffer, DataFormats.Rtf);
                            SetDocumentRTF(richTextBox, Encoding.UTF8.GetString(buffer.ToArray()));
                        }
                    };
                }
            });

        /// <summary>
        /// Gets the full RTF text of the given object.
        /// </summary>
        /// <param name="dependencyObject">The richtextbox dependency object.</param>
        /// <returns>The full RTF text of the given object.</returns>
        public static String GetDocumentRTF(DependencyObject dependencyObject)
        {
            return (String)dependencyObject.GetValue(RichTextBoxHelper.DocumentRTFProperty);
        }

        /// <summary>
        /// Sets the full RTF text of the given object.
        /// </summary>
        /// <param name="dependencyObject">The richtextbox dependency object.</param>
        /// <param name="value">The new text.</param>
        public static void SetDocumentRTF(DependencyObject dependencyObject, String value)
        {
            dependencyObject.SetValue(RichTextBoxHelper.DocumentRTFProperty, value);
        }
    }
    //// End class
}
//// End namespace