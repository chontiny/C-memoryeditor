namespace Ana.View
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for Output.xaml
    /// </summary>
    internal partial class Output : UserControl
    {
        private Boolean autoScroll;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelThresholder" /> class
        /// </summary>
        public Output()
        {
            this.InitializeComponent();
        }

        private void OutputScrollViewerScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {
                // Content unchanged : user scroll event
                if (this.OutputScrollViewer.VerticalOffset == this.OutputScrollViewer.ScrollableHeight)
                {
                    // Scroll bar is in bottom Set autoscroll mode
                    autoScroll = true;
                }
                else
                {
                    // Scroll bar isn't in bottomUnset autoscroll mode
                    autoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (autoScroll && e.ExtentHeightChange != 0)
            {
                // Content changed and autoscroll mode set Autoscroll
                this.OutputScrollViewer.ScrollToVerticalOffset(this.OutputScrollViewer.ExtentHeight);
            }
        }
    }
    //// End class
}
//// End namespace