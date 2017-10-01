namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Controls;
    using Editors.ScriptEditor;
    using Scripting;
    using Squalr.Content;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Browse.StreamConfig;
    using Squalr.Source.Editors.StreamIconEditor;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using Utils.TypeConverters;

    /// <summary>
    /// Defines a script that can be added to the project explorer.
    /// </summary>
    [DataContract]
    internal class ScriptItem : ProjectItem, IStreamIconsLoadedObserver
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
        private String compiledScript;

        /// <summary>
        /// A value indicating if this script is enabled for stream interaction.
        /// </summary>
        [Browsable(false)]
        protected Boolean isStreamDisabled;

        /// <summary>
        /// The cooldown in milliseconds of this project item.
        /// </summary>
        [Browsable(false)]
        protected Single cooldown;

        /// <summary>
        /// The duration in milliseconds of this project item.
        /// </summary>
        [Browsable(false)]
        protected Single duration;

        /// <summary>
        /// The stream icon path associated with this project item.
        /// </summary>
        [Browsable(false)]
        protected String streamIconName;

        /// <summary>
        /// The current cooldown for this script item.
        /// </summary>
        private Single currentCooldown;

        /// <summary>
        /// The current duration for this script item.
        /// </summary>
        private Single currentDuration;

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
            this.ScriptManager = new ScriptManager();
            this.LastCooldownUpdate = DateTime.MinValue;

            // Initialize script and bypass setters
            this.script = script;

            Task.Run(() => this.compiledScript = this.ScriptManager.CompileScript(script));

            if (StreamIconEditorViewModel.GetInstance().IsStreamIconListLoading)
            {
                StreamIconEditorViewModel.GetInstance().Subscribe(this);
            }
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

                Task.Run(() => this.CompiledScript = this.ScriptManager?.CompileScript(value));

                this.script = value;
                ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Gets or sets the base 64 encoded compiled script.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public String CompiledScript
        {
            get
            {
                return this.compiledScript;
            }

            private set
            {

                this.compiledScript = value;

                ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if this script is disabled for stream interaction.
        /// </summary>
        [DataMember]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Stream Disabled"), Description("Indicates whether this item is available for activation via stream commands.")]
        public Boolean IsStreamDisabled
        {
            get
            {
                return this.isStreamDisabled;
            }

            set
            {
                if (this.isStreamDisabled == value)
                {
                    return;
                }

                this.isStreamDisabled = value;

                if (this.AssociatedCheat != null)
                {
                    this.AssociatedCheat.IsStreamDisabled = value;
                }

                StreamConfigViewModel.GetInstance().OnCheatListChange();

                ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.IsStreamDisabled));
                this.NotifyPropertyChanged(nameof(this.Icon));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the coolodown in milliseconds of this project item.
        /// </summary>
        [DataMember]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Cooldown"), Description("The cooldown (in milliseconds) for stream activation for this project item.")]
        public Single Cooldown
        {
            get
            {
                return this.cooldown;
            }

            set
            {
                if (this.cooldown == value)
                {
                    return;
                }

                this.cooldown = value;

                if (this.AssociatedCheat != null)
                {
                    this.AssociatedCheat.Cooldown = value;
                }

                ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.Cooldown));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the coolodown in milliseconds of this project item.
        /// </summary>
        [DataMember]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Duration"), Description("The duration (in milliseconds) for stream activation for this project item.")]
        public Single Duration
        {
            get
            {
                return this.duration;
            }

            set
            {
                if (this.duration == value)
                {
                    return;
                }

                this.duration = value;

                if (this.AssociatedCheat != null)
                {
                    this.AssociatedCheat.Duration = value;
                }

                ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.Duration));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the stream icon for this project item.
        /// </summary>
        [DataMember]
        [Editor(typeof(StreamIconEditorModel), typeof(UITypeEditor))]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Stream Icon"), Description("The stream icon for this item")]
        public String StreamIconName
        {
            get
            {
                return this.streamIconName;
            }

            set
            {
                if (this.streamIconName == value)
                {
                    return;
                }

                this.streamIconName = value;

                if (this.AssociatedCheat != null)
                {
                    this.AssociatedCheat.Icon = value;
                }

                this.NotifyPropertyChanged(nameof(this.StreamIconName));
                this.NotifyPropertyChanged(nameof(this.Icon));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets the image associated with this project item.
        /// </summary>
        [Browsable(false)]
        public override BitmapSource Icon
        {
            get
            {
                BitmapSource displayIcon = null;

                if (this.IsStreamDisabled)
                {
                    return Images.Cancel;
                }

                if (!StreamIconEditorViewModel.GetInstance().IsStreamIconListLoading)
                {
                    displayIcon = SqualrApi.GetStreamIcons()
                        .Select(icon => icon)
                        .Where(icon => icon.IconName == this.StreamIconName)
                        .FirstOrDefault()?.Icon;
                }

                return displayIcon ?? Images.Script;
            }
        }

        /// <summary>
        /// Gets or sets the current cooldown for this script item.
        /// </summary>
        [Browsable(false)]
        public Single CurrentCooldown
        {
            get
            {
                return this.currentCooldown;
            }

            set
            {
                this.currentCooldown = value;
                this.NotifyPropertyChanged(nameof(this.CurrentCooldown));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the current duration for this script item.
        /// </summary>
        [Browsable(false)]
        public Single CurrentDuration
        {
            get
            {
                return this.currentDuration;
            }

            set
            {
                this.currentDuration = value;
                this.NotifyPropertyChanged(nameof(this.CurrentDuration));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the script manager associated with this script.
        /// </summary>
        [Browsable(false)]
        private ScriptManager ScriptManager { get; set; }

        /// <summary>
        /// Gets or sets the time since the last cooldown update.
        /// </summary>
        private DateTime LastCooldownUpdate { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context.</param>
        [OnDeserialized]
        public new void OnDeserialized(StreamingContext streamingContext)
        {
            base.OnDeserialized(streamingContext);

            this.ScriptManager = new ScriptManager();
            this.LastCooldownUpdate = DateTime.MinValue;

            if (this.compiledScript == null)
            {
                Task.Run(() => this.compiledScript = this.ScriptManager.CompileScript(script));
            }

            if (StreamIconEditorViewModel.GetInstance().IsStreamIconListLoading)
            {
                StreamIconEditorViewModel.GetInstance().Subscribe(this);
            }
        }

        /// <summary>
        /// Recieves a notification of the loaded stream icons.
        /// </summary>
        /// <param name="streamIcons">The loaded stream icons.</param>
        public void Update(IEnumerable<StreamIcon> streamIcons)
        {
            this.NotifyPropertyChanged(nameof(this.Icon));
            StreamIconEditorViewModel.GetInstance().Unsubscribe(this);
        }

        /// <summary>
        /// Associates a cheat with this project item.
        /// </summary>
        /// <param name="cheat">The associated cheat</param>
        public override void AssociateCheat(Cheat cheat)
        {
            base.AssociateCheat(cheat);

            this.isStreamDisabled = cheat.IsStreamDisabled;
            this.cooldown = cheat.Cooldown;
            this.duration = cheat.Duration;
            this.streamIconName = cheat.Icon;
        }

        /// <summary>
        /// Update event for this project item.
        /// </summary>
        public override void Update()
        {
            this.UpdateCooldown();
        }

        /// <summary>
        /// Called when the activation state changes. Toggles this script.
        /// </summary>
        protected override void OnActivationChanged()
        {
            if (this.IsActivated)
            {
                if (this.CurrentCooldown > 0)
                {
                    this.IsActivated = false;
                    return;
                }

                // Try to run script.
                if (!this.ScriptManager.RunActivationFunction(this))
                {
                    // Script error -- clear activation.
                    this.ResetActivation();
                    return;
                }

                // Run the update loop for the script
                this.ScriptManager.RunUpdateFunction(this);

                this.CurrentCooldown = this.Cooldown;
                this.CurrentDuration = 0.0f;
            }
            else
            {
                // Try to deactivate script (we do not care if this fails)
                this.ScriptManager.RunDeactivationFunction(this);
            }
        }

        /// <summary>
        /// Updates the cooldown for this script.
        /// </summary>
        private void UpdateCooldown()
        {
            if (this.IsStreamDisabled || !this.IsActivated)
            {
                // Clear state
                this.LastCooldownUpdate = DateTime.MinValue;
                this.CurrentDuration = 0.0f;
                this.CurrentCooldown = 0.0f;
                return;
            }

            DateTime currentTime = DateTime.Now;

            if (this.LastCooldownUpdate == DateTime.MinValue)
            {
                this.LastCooldownUpdate = currentTime;
            }

            Single elapsedTime = (Single)(currentTime - this.LastCooldownUpdate).TotalSeconds;

            // Update current cooldown
            this.CurrentCooldown = (this.CurrentCooldown - elapsedTime).Clamp(0, this.Cooldown);

            // Update current duration
            this.CurrentDuration = (this.CurrentDuration + elapsedTime).Clamp(0, this.Duration);

            // Deactivate if exceeding the duration
            if (this.CurrentDuration >= this.Duration)
            {
                this.IsActivated = false;
            }

            this.LastCooldownUpdate = currentTime;
        }
    }
    //// End class
}
//// End namespace