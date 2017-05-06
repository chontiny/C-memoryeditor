namespace Squalr.Source.Controls
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public class RichTextBoxHelper : DependencyObject
    {
        public static String GetDocumentRTF(DependencyObject obj)
        {
            return (String)obj.GetValue(DocumentRTFProperty);
        }

        public static void SetDocumentRTF(DependencyObject obj, String value)
        {
            obj.SetValue(DocumentRTFProperty, value);
        }

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

                    range.Load(new MemoryStream(Encoding.UTF8.GetBytes(rtf)), DataFormats.Rtf);

                    // Set the document
                    richTextBox.Document = doc;

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
    }
    //// End class
}
//// End namespace