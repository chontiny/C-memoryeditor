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
    using System.Runtime.Serialization;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class PointerItemDecorator : PointerItem
    {
        /// <summary>
        /// Gets or sets the identifier for the base address of this object.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Name"), Description("The module to use as a base address")]
        public override String ModuleName
        {
            get
            {
                return base.ModuleName;
            }

            set
            {
                base.ModuleName = value;
            }
        }

        /// <summary>
        /// Gets or sets the base address of this object. This will be added as an offset from the resolved base identifier.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(AddressConverter))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Offset"), Description("The offset from the module address. If no module address, then this is the base address.")]
        public override UInt64 ModuleOffset
        {
            get
            {
                return base.ModuleOffset;
            }

            set
            {
                base.ModuleOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the pointer offsets of this address item.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(OffsetConverter))]
        [Editor(typeof(OffsetEditorModel), typeof(UITypeEditor))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Pointer Offsets"), Description("The pointer offsets used to calculate the final address")]
        public override IEnumerable<Int32> PointerOffsets
        {
            get
            {
                return base.PointerOffsets;
            }

            set
            {
                base.PointerOffsets = value;
            }
        }
    }
    //// End class
}
//// End namespace