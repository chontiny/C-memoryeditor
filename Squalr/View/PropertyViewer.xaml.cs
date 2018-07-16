namespace Squalr.View
{
    using Source.PropertyViewer;
    using Squalr.Engine.Utils.Extensions;
    using Squalr.Source.Controls;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for PropertyViewer.xaml.
    /// </summary>
    public partial class PropertyViewer : UserControl, IPropertyViewerObserver
    {
        /// <summary>
        /// The property grid to display selected objects.
        /// </summary>
        private System.Windows.Forms.PropertyGrid propertyGrid;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyViewer" /> class.
        /// </summary>
        public PropertyViewer()
        {
            this.InitializeComponent();

            // Windows Forms hosting -- TODO: Phase this out
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();

            // Use reflection to set all propertygrid colors to dark, since some are otherwise not publically accessible
            PropertyInfo[] allProperties = this.propertyGrid.GetType().GetProperties();
            IEnumerable<PropertyInfo> colorProperties = allProperties.Select(x => x).Where(x => x.PropertyType == typeof(Color));
            colorProperties.ForEach(x => x.SetValue(this.propertyGrid, DarkBrushes.SqualrColorBlack, null));

            this.propertyGrid.BackColor = DarkBrushes.SqualrColorBlack;
            this.propertyGrid.CommandsBackColor = DarkBrushes.SqualrColorBlack;
            this.propertyGrid.HelpBackColor = DarkBrushes.SqualrColorBlack;
            this.propertyGrid.SelectedItemWithFocusBackColor = DarkBrushes.SqualrColorBlack;
            this.propertyGrid.ViewBackColor = DarkBrushes.SqualrColorBlack;

            this.propertyGrid.CommandsActiveLinkColor = DarkBrushes.SqualrColorBlack;
            this.propertyGrid.CommandsDisabledLinkColor = DarkBrushes.SqualrColorBlack;

            this.propertyGrid.CategorySplitterColor = DarkBrushes.SqualrColorWhite;

            this.propertyGrid.CommandsBorderColor = DarkBrushes.SqualrColorGray20;
            this.propertyGrid.HelpBorderColor = DarkBrushes.SqualrColorGray20;
            this.propertyGrid.ViewBorderColor = DarkBrushes.SqualrColorGray20;

            this.propertyGrid.CategoryForeColor = DarkBrushes.SqualrColorWhite;
            this.propertyGrid.CommandsForeColor = DarkBrushes.SqualrColorWhite;
            this.propertyGrid.DisabledItemForeColor = DarkBrushes.SqualrColorWhite;
            this.propertyGrid.HelpForeColor = DarkBrushes.SqualrColorWhite;
            this.propertyGrid.SelectedItemWithFocusForeColor = DarkBrushes.SqualrColorWhite;
            this.propertyGrid.ViewForeColor = DarkBrushes.SqualrColorWhite;

            PropertyViewerViewModel.GetInstance().Subscribe(this);

            this.propertyViewer.Children.Add(WinformsHostingHelper.CreateHostedControl(this.propertyGrid));
        }

        /// <summary>
        /// Updates the selected target objects.
        /// </summary>
        /// <param name="targetObjects">The selected target objects.</param>
        public void Update(Object[] targetObjects)
        {
            ControlThreadingHelper.InvokeControlAction(
                this.propertyGrid,
                () =>
            {
                this.propertyGrid.SelectedObjects = targetObjects == null || targetObjects.Contains(null) ? new Object[] { } : targetObjects;
            });
        }
    }
    //// End class
}
//// End namespace