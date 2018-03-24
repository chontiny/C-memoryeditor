namespace Squalr.Source.ProjectItems
{
    using Editors.ScriptEditor;
    using Squalr.Content;
    using Squalr.Engine.Scripting;
    using Squalr.Source.Controls;
    using Squalr.Source.Utils.TypeConverters;
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Defines a script that can be added to the project explorer.
    /// </summary>
    [DataContract]
    public class ScriptItem : ProjectItem
    {
        /// <summary>
        /// The raw script text.
        /// </summary>
        [Browsable(false)]
        private String script;

        /// <summary>
        /// The base 64 encoded compiled script.
        /// </summary>
        [Browsable(false)]
        private ModScript modScript;

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
        public ScriptItem(String description, String script) : base(description)
        {
            // Initialize script and bypass setters
            this.script = script;
            this.modScript = new ModScript();

            this.ModScript.SetScript(script);
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
                this.ModScript?.SetScript(this.Script);

                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Gets or sets the base 64 encoded compiled script.
        /// </summary>
        [Browsable(false)]
        public ModScript ModScript
        {
            get
            {
                return this.modScript;
            }

            private set
            {

                this.modScript = value;

                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Gets the image associated with this project item.
        /// </summary>
        public override BitmapSource Icon
        {
            get
            {
                return Images.Script;
            }
        }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context.</param>
        [OnDeserialized]
        public new void OnDeserialized(StreamingContext streamingContext)
        {
            base.OnDeserialized(streamingContext);

            // Avoid using public Properties to bypass setter/getters
            if (this.modScript == null)
            {
                this.modScript = new ModScript(this.script);
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
            if (this.IsActivated)
            {
                // Try to run script.
                if (!this.ModScript.RunActivationFunction())
                {
                    // Script error -- clear activation.
                    this.ResetActivation();
                    return;
                }

                // Run the update loop for the script
                this.ModScript.RunUpdateFunction();
            }
            else
            {
                // Try to deactivate script (we do not care if this fails)
                this.ModScript.RunDeactivationFunction();
            }
        }
    }
    //// End class
}
//// End namespace