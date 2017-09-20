namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Controls;
    using Editors.ScriptEditor;
    using Scripting;
    using Squalr.Source.Analytics;
    using Squalr.Source.Editors.StreamIconEditor;
    using Squalr.Source.Output;
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using Utils.TypeConverters;

    /// <summary>
    /// Defines a script that can be added to the project explorer.
    /// </summary>
    [DataContract]
    internal class ScriptItem : ProjectItem
    {
        /// <summary>
        /// The raw script text.
        /// </summary>
        [Browsable(false)]
        private String script;

        /// <summary>
        /// Whether the script is compiled.
        /// </summary>
        [Browsable(false)]
        private Boolean isCompiled;

        /// <summary>
        /// The stream icon path associated with this project item.
        /// </summary>
        [Browsable(false)]
        protected String streamIconPath;

        /// <summary>
        /// The stream command associated with this project item.
        /// </summary>
        [Browsable(false)]
        protected String streamCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptItem" /> class.
        /// </summary>
        public ScriptItem() : this("New Script", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptItem" /> class.
        /// </summary>
        /// <param name="description">The description of the project item.</param>
        /// <param name="script">The raw script text.</param>
        /// <param name="compiled">Whether or not this script is compiled.</param>
        public ScriptItem(String description, String script, Boolean compiled = false) : base(description)
        {
            this.script = script;
            this.isCompiled = compiled;

            this.ScriptManager = null;
        }

        /// <summary>
        /// Gets or sets the raw script text.
        /// </summary>
        [DataMember]
        [ReadOnly(false)]
        [TypeConverter(typeof(ScriptConverter))]
        [Editor(typeof(ScriptEditorModel), typeof(UITypeEditor))]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Script"), Description("C# script to interface with engine")]
        public String Script
        {
            get
            {
                return this.script;
            }

            set
            {
                if (this.script == value)
                {
                    return;
                }

                this.script = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the script is compiled.
        /// </summary>
        [DataMember]
        [ReadOnly(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Compiled"), Description("Whether or not this script has been compiled.")]
        public Boolean IsCompiled
        {
            get
            {
                return this.isCompiled;
            }

            set
            {
                if (this.isCompiled == value)
                {
                    return;
                }

                this.isCompiled = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Gets or sets the stream icon path for this project item.
        /// </summary>
        [DataMember]
        [Editor(typeof(StreamIconEditorModel), typeof(UITypeEditor))]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Stream Icon"), Description("The stream icon for this item")]
        public String StreamIconPath
        {
            get
            {
                return this.streamIconPath;
            }

            set
            {
                if (this.streamIconPath == value)
                {
                    return;
                }

                this.streamIconPath = value;

                if (this.AssociatedCheat != null)
                {
                    this.AssociatedCheat.StreamIcon = value;
                }

                this.NotifyPropertyChanged(nameof(this.StreamIconPath));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the stream command for this project item.
        /// </summary>
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Stream Command"), Description("The stream command for this item. Limit of 6 characters.")]
        public String StreamCommand
        {
            get
            {
                return this.streamCommand;
            }

            set
            {
                if (this.streamCommand == value)
                {
                    return;
                }

                this.streamCommand = value?.ToLower();

                if (this.AssociatedCheat != null)
                {
                    this.AssociatedCheat.StreamCommand = value;
                }

                this.NotifyPropertyChanged(nameof(this.StreamCommand));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the script manager associated with this script.
        /// </summary>
        [Browsable(false)]
        private ScriptManager ScriptManager { get; set; }

        /// <summary>
        /// Clones the project item.
        /// </summary>
        /// <returns>The clone of the project item.</returns>
        public override ProjectItem Clone()
        {
            ScriptItem clone = new ScriptItem();
            clone.description = this.Description;
            clone.extendedDescription = this.ExtendedDescription;
            clone.parent = this.Parent;
            clone.script = this.Script;
            clone.isCompiled = this.IsCompiled;

            return clone;
        }

        /// <summary>
        /// Clones the script item and compiles it.
        /// </summary>
        /// <returns>The clone of the project. Returns null on compilation failure.</returns>
        public ScriptItem Compile()
        {
            if (this.ScriptManager == null)
            {
                this.ScriptManager = new ScriptManager();
            }

            if (this.IsCompiled)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Script already compiled");
                return null;
            }

            try
            {
                ScriptItem clone = this.Clone() as ScriptItem;
                clone.isCompiled = true;
                clone.script = this.ScriptManager.CompileScript(clone.script);
                return clone;
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to complete compile request", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
                return null;
            }
        }

        /// <summary>
        /// Update event for this project item.
        /// </summary>
        public override void Update()
        {
        }

        /// <summary>
        /// Called when the activation state changes. Toggles this script.
        /// </summary>
        protected override void OnActivationChanged()
        {
            if (this.ScriptManager == null)
            {
                this.ScriptManager = new ScriptManager();
            }

            if (this.IsActivated)
            {
                // Try to run script.
                if (!this.ScriptManager.RunActivationFunction(this))
                {
                    // Script error -- clear activation.
                    this.ResetActivation();
                    return;
                }

                // Run the update loop for the script
                this.ScriptManager.RunUpdateFunction(this);
            }
            else
            {
                // Try to deactivate script (we do not care if this fails)
                this.ScriptManager.RunDeactivationFunction(this);
            }
        }
    }
    //// End class
}
//// End namespace