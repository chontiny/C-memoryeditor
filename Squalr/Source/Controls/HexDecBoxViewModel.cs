namespace Squalr.Source.Controls
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Utils;
    using System;
    using System.Windows.Input;

    /// <summary>
    /// The view model for a HexDec box.
    /// </summary>
    public class HexDecBoxViewModel : ViewModelBase
    {
        /// <summary>
        /// The active text.
        /// </summary>
        private String text;

        /// <summary>
        /// The data type being represented.
        /// </summary>
        private DataType elementType;

        /// <summary>
        /// A value indicating whether the value is displayed as hex.
        /// </summary>
        private Boolean isHex;

        /// <summary>
        /// A value indicating whether the current value is valid for the current data type.
        /// </summary>
        private Boolean isValid;

        /// <summary>
        /// 
        /// </summary>
        public HexDecBoxViewModel()
        {
            this.ElementType = DataType.Int32;

            this.ConvertDecCommand = new RelayCommand(() => this.ConvertDec());
            this.ConvertHexCommand = new RelayCommand(() => this.ConvertHex());
            this.SwitchDecCommand = new RelayCommand(() => this.SwitchDec());
            this.SwitchHexCommand = new RelayCommand(() => this.SwitchHex());
        }

        /// <summary>
        /// Gets a command to reinterpret the text as decimal.
        /// </summary>
        public ICommand SwitchDecCommand { get; private set; }

        /// <summary>
        /// Gets a command to reinterpret the text as hex.
        /// </summary>
        public ICommand SwitchHexCommand { get; private set; }

        /// <summary>
        /// Gets a command to convert the text to decimal.
        /// </summary>
        public ICommand ConvertDecCommand { get; private set; }

        /// <summary>
        /// Gets a command to convert the text to hex.
        /// </summary>
        public ICommand ConvertHexCommand { get; private set; }

        /// <summary>
        /// Gets or sets the active text.
        /// </summary>
        public String ActiveText
        {
            get
            {
                return this.text;
            }

            set
            {
                this.text = value;
                this.RaisePropertyChanged(nameof(this.ActiveText));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value is displayed as hex.
        /// </summary>
        public Boolean IsHex
        {
            get
            {
                return this.isHex;
            }

            set
            {
                this.isHex = value;
                this.RaisePropertyChanged(nameof(this.IsHex));
            }
        }

        /// <summary>
        /// Gets or sets the data type being represented.
        /// </summary>
        public DataType ElementType
        {
            get
            {
                return this.elementType;
            }

            set
            {
                this.elementType = value;
                this.RaisePropertyChanged(nameof(this.ElementType));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current value is valid for the current data type.
        /// </summary>
        public Boolean IsValid
        {
            get
            {
                return this.isValid;
            }

            set
            {
                this.isValid = value;
                this.RaisePropertyChanged(nameof(this.IsValid));
            }
        }

        /// <summary>
        /// Gets the value as its standard decimal representation.
        /// </summary>
        /// <returns>The decimal value.</returns>
        public String GetValueAsDecimal()
        {
            if (!this.IsValid)
            {
                return null;
            }

            if (this.IsHex)
            {
                return Conversions.ParseHexStringAsPrimitiveString(this.ElementType, this.ActiveText);
            }
            else
            {
                return this.ActiveText;
            }
        }

        /// <summary>
        /// Gets the value as a hexedecimal representation.
        /// </summary>
        /// <returns>The hexedecimal value string.</returns>
        public String GetValueAsHexidecimal()
        {
            if (!this.IsValid)
            {
                return null;
            }

            if (this.IsHex)
            {
                return this.ActiveText;
            }
            else
            {
                return Conversions.ParsePrimitiveStringAsHexString(this.ElementType, this.ActiveText);
            }
        }

        /// <summary>
        /// Gets the raw value being represented.
        /// </summary>
        /// <returns>The raw value.</returns>
        public Object GetValue()
        {
            if (!this.IsValid)
            {
                return null;
            }

            if (this.IsHex)
            {
                return Conversions.ParseHexStringAsPrimitive(this.ElementType, this.ActiveText);
            }
            else
            {
                return Conversions.ParsePrimitiveStringAsPrimitive(this.ElementType, this.ActiveText);
            }
        }

        /// <summary>
        /// Sets the raw value being represented.
        /// </summary>
        /// <param name="value">The raw value.</param>
        public void SetValue(Object value)
        {
            String valueString = value?.ToString();

            if (!SyntaxChecker.CanParseValue(this.ElementType, valueString))
            {
                return;
            }

            if (this.IsHex)
            {
                this.ActiveText = Conversions.ParsePrimitiveStringAsHexString(this.ElementType, valueString);
            }
            else
            {
                this.ActiveText = valueString;
            }
        }

        /// <summary>
        /// Reinterprets the text as decimal.
        /// </summary>
        private void SwitchDec()
        {
            this.IsHex = false;
        }

        /// <summary>
        /// Reinterprets the text as hex.
        /// </summary>
        private void SwitchHex()
        {
            this.IsHex = true;
        }

        /// <summary>
        /// Converts the text to decimal.
        /// </summary>
        private void ConvertDec()
        {
            if (SyntaxChecker.CanParseHex(this.ElementType, this.ActiveText))
            {
                this.ActiveText = Conversions.ParseHexStringAsPrimitiveString(this.ElementType, this.ActiveText);
            }

            this.SwitchDec();
        }

        /// <summary>
        /// Converts the text to hex.
        /// </summary>
        private void ConvertHex()
        {
            if (SyntaxChecker.CanParseValue(this.ElementType, this.ActiveText))
            {
                this.ActiveText = Conversions.ParsePrimitiveStringAsHexString(this.ElementType, this.ActiveText);
            }

            this.SwitchHex();
        }
    }
    //// End class
}
//// End namespace