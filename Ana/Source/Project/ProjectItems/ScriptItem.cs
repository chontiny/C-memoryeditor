namespace Ana.Source.Project.ProjectItems
{
    using ScriptEngine;
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using Utils.ScriptEditor;
    using Utils.TypeConverters;

    [DataContract]
    internal class ScriptItem : ProjectItem
    {
        [Browsable(false)]
        private String script;

        [Browsable(false)]
        private Boolean isCompiled;

        public ScriptItem() : this("New Script", null)
        {
        }

        public ScriptItem(String description, String script, Boolean compiled = false) : base(description)
        {
            this.Script = script;
            this.IsCompiled = compiled;

            this.ScriptManager = null;
        }

        [DataMember]
        [ReadOnly(false)]
        [TypeConverter(typeof(ScriptConverter))]
        [Editor(typeof(ScriptEditorModel), typeof(UITypeEditor))]
        [Category("Properties"), DisplayName("Script"), Description("C# script to interface with engine")]
        public String Script
        {
            get
            {
                return this.script;
            }

            set
            {
                this.script = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
            }
        }

        [DataMember]
        [ReadOnly(true)]
        [RefreshProperties(RefreshProperties.All)]
        [Category("Properties"), DisplayName("Compiled"), Description("Whether or not this script has been compiled.")]
        public Boolean IsCompiled
        {
            get
            {
                return this.isCompiled;
            }

            set
            {
                this.isCompiled = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
            }
        }

        [Browsable(false)]
        private ScriptManager ScriptManager { get; set; }

        public override void Update()
        {
        }

        protected override void OnActivationChanged()
        {
            if (ScriptManager == null)
            {
                ScriptManager = new ScriptManager();
            }

            if (this.IsActivated)
            {
                // Try to run script.
                if (!ScriptManager.RunActivationFunction(this.Script, this.IsCompiled))
                {
                    // Script error -- clear activation.
                    this.ResetActivation();
                    return;
                }

                // Run the update loop for the script
                ScriptManager.RunUpdateFunction();
            }
            else
            {
                // Try to deactivate script (we do not care if this fails)
                ScriptManager.RunDeactivationFunction();
            }
        }
    }
    //// End class
}
//// End namespace