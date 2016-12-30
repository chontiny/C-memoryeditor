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

        /// <summary>
        /// Clones the project item.
        /// </summary>
        /// <returns>The clone of the project item.</returns>
        public override ProjectItem Clone()
        {
            ScriptItem clone = new ScriptItem();
            clone.description = this.Description;
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
                // TODO: Log to user, already compiled
                return null;
            }

            try
            {
                ScriptItem clone = this.Clone() as ScriptItem;
                clone.description += " - [Compiled]";
                clone.isCompiled = true;
                clone.script = this.ScriptManager.CompileScript(clone.script);

                return clone;
            }
            catch
            {
                return null;
            }
        }

        public override void Update()
        {
        }

        protected override void OnActivationChanged()
        {
            if (this.ScriptManager == null)
            {
                this.ScriptManager = new ScriptManager();
            }

            if (this.IsActivated)
            {
                // Try to run script.
                if (!this.ScriptManager.RunActivationFunction(this.Script, this.IsCompiled))
                {
                    // Script error -- clear activation.
                    this.ResetActivation();
                    return;
                }

                // Run the update loop for the script
                this.ScriptManager.RunUpdateFunction();
            }
            else
            {
                // Try to deactivate script (we do not care if this fails)
                this.ScriptManager.RunDeactivationFunction();
            }
        }
    }
    //// End class
}
//// End namespace