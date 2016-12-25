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
        private Script script;

        public ScriptItem() : this("New Script", null)
        {
        }

        public ScriptItem(String description, Script script) : base(description)
        {
            this.Script = script;

            ScriptManager = null;
        }

        [DataMember]
        [TypeConverter(typeof(ScriptConverter))]
        [Editor(typeof(ScriptEditorModel), typeof(UITypeEditor))]
        [Category("Properties"), DisplayName("Script"), Description("C# script to interface with engine")]
        public Script Script
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
                if (!ScriptManager.RunActivationFunction(script?.Payload))
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