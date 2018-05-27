namespace Squalr.Source.SolutionExplorer
{
    using GalaSoft.MvvmLight;

    /// <summary>
    /// View model for the right side pane
    /// </summary>
    public class UselessViewModel : ViewModelBase
    {
        private SolutionExplorerViewModel _evm;
        private DirInfo _currentItem;

        public UselessViewModel(SolutionExplorerViewModel evm)
        {
            _evm = evm;
        }

        /// <summary>
        /// Indicates the current directory in the Directory view pane
        /// </summary>
        public DirInfo CurrentItem
        {
            get { return _currentItem; }
            set { _currentItem = value; }
        }

        /// <summary>
        /// processes the current object. If this is a file then open it or if it is a directory then return its subdirectories
        /// </summary>
        public void OpenCurrentObject()
        {
            int objType = CurrentItem.DirType; //Dir/File type

            if ((ObjectType)CurrentItem.DirType == ObjectType.File)
            {
                System.Diagnostics.Process.Start(CurrentItem.Path);
            }
            else
            {
                _evm.CurrentDirectory = CurrentItem;
                _evm.ExpandToCurrentNode(_evm.CurrentDirectory);
            }
        }
    }
    //// End class
}
//// End namespace