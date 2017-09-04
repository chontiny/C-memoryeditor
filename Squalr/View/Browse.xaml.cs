namespace Squalr.View
{
    using Source.Browse;

    /// <summary>
    /// Interaction logic for Browse.xaml.
    /// </summary>
    internal partial class Browse : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Browse" /> class.
        /// </summary>
        public Browse()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public BrowseViewModel BrowseViewModel
        {
            get
            {
                return this.DataContext as BrowseViewModel;
            }
        }
    }
    //// End class
}
//// End namespace