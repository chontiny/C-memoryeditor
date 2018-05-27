using System.Windows.Controls;

namespace Squalr.View
{
    /// <summary>
    /// Interaction logic for SolutionExplorer.xaml
    /// </summary>
    internal partial class SolutionExplorer : UserControl
    {
        /*
        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public SolutionExplorerViewModel SolutionExplorerViewModel
        {
            get
            {
                return this.DataContext as SolutionExplorerViewModel;
            }
        }

        private void TreeViewItem_Expanded(Object sender, RoutedEventArgs e)
        {
            TreeViewItem currentTreeNode = sender as TreeViewItem;

            if (currentTreeNode == null)
            {
                return;
            }

            if (currentTreeNode.ItemsSource == null)
            {
                return;
            }

            DirInfo parentDirectory = currentTreeNode.Header as DirInfo;

            if (parentDirectory == null)
            {
                return;
            }

            foreach (DirInfo d in currentTreeNode.ItemsSource)
            {
                if (this.SolutionExplorerViewModel.CurrentDirectory.Path.Equals(d.Path))
                {
                    d.IsSelected = true;
                    d.IsExpanded = true;
                    break;
                }
            }

            e.Handled = true;
        }

        private void DirectoryTree_SelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            // this.ExplorerWindowViewModel.FileTreeVM.CurrentTreeItem = DirectoryTree.SelectedItem as DirInfo;
        }*/
    }
    //// End class
}
//// End namespace