namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using Squalr.Source.Controls;
    using Squalr.Source.Editors.OffsetEditor;
    using Squalr.Source.Utils.TypeConverters;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class PointerItemView : ProjectItemView
    {
        private PointerItem pointerItem;

        public PointerItemView(PointerItem pointerItem)
        {
            this.PointerItem = pointerItem;
        }

        [Browsable(false)]
        private PointerItem PointerItem
        {
            get
            {
                return this.pointerItem;
            }

            set
            {
                this.pointerItem = value;
                this.ProjectItem = value;
                this.RaisePropertyChanged(nameof(this.PointerItem));
            }
        }

        /// <summary>
        /// Gets or sets the description for this object.
        /// </summary>
        [Browsable(true)]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Name"), Description("The name of this pointer")]
        public String Name
        {
            get
            {
                return this.PointerItem.Name;
            }

            set
            {
                this.PointerItem.Name = value;
                this.RaisePropertyChanged(nameof(this.Name));
            }
        }

        /// <summary>
        /// Gets or sets the identifier for the base address of this object.
        /// </summary>
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Name"), Description("The module to use as a base address")]
        public String ModuleName
        {
            get
            {
                return this.PointerItem.ModuleName;
            }

            set
            {
                this.PointerItem.ModuleName = value;
                this.RaisePropertyChanged(nameof(this.ModuleName));
            }
        }

        /// <summary>
        /// Gets or sets the base address of this object. This will be added as an offset from the resolved base identifier.
        /// </summary>
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(AddressConverter))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Offset"), Description("The offset from the module address. If no module address, then this is the base address.")]
        public UInt64 ModuleOffset
        {
            get
            {
                return this.PointerItem.ModuleOffset;
            }

            set
            {
                this.PointerItem.ModuleOffset = value;
                this.RaisePropertyChanged(nameof(this.ModuleOffset));
            }
        }

        /// <summary>
        /// Gets or sets the pointer offsets of this address item.
        /// </summary>
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(OffsetConverter))]
        [Editor(typeof(OffsetEditorModel), typeof(UITypeEditor))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Pointer Offsets"), Description("The pointer offsets used to calculate the final address")]
        public IEnumerable<Int32> PointerOffsets
        {
            get
            {
                return this.PointerItem.PointerOffsets;
            }

            set
            {
                this.PointerItem.PointerOffsets = value;
                this.RaisePropertyChanged(nameof(this.PointerOffsets));
            }
        }
    }
    //// End class
}
//// End namespace