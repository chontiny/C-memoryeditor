namespace Squalr.Content
{
    using Squalr.Source.Utils;
    using System;
    using System.IO;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Static images for use by the entire project, which reference local content.
    /// </summary>
    public class Images
    {
        /// <summary>
        /// Image of one blue block.
        /// </summary>
        public static readonly BitmapImage BlueBlocks1 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "BlueBlocks1.png"));

        /// <summary>
        /// Image of two blue blocks.
        /// </summary>
        public static readonly BitmapImage BlueBlocks2 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "BlueBlocks2.png"));

        /// <summary>
        /// Image of four blue blocks.
        /// </summary>
        public static readonly BitmapImage BlueBlocks4 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "BlueBlocks4.png"));

        /// <summary>
        /// Image of eight blue blocks.
        /// </summary>
        public static readonly BitmapImage BlueBlocks8 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "BlueBlocks8.png"));

        /// <summary>
        /// Image for cancel operations.
        /// </summary>
        public static readonly BitmapImage Cancel = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Cancel.png"));

        /// <summary>
        /// Image for a changed scan.
        /// </summary>
        public static readonly BitmapImage Changed = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Changed.png"));

        /// <summary>
        /// Image for a cog.
        /// </summary>
        public static readonly BitmapImage Cog = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Cog.png"));

        /// <summary>
        /// Image for a coin.
        /// </summary>
        public static readonly BitmapImage Coin = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Coin.png"));

        /// <summary>
        /// Image for a connection.
        /// </summary>
        public static readonly BitmapImage Connect = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Connect.png"));

        /// <summary>
        /// Image for an active connection.
        /// </summary>
        public static readonly BitmapImage Connected = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Connected.png"));

        /// <summary>
        /// Image for a value collection scan.
        /// </summary>
        public static readonly BitmapImage CollectValues = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "CollectValues.png"));

        /// <summary>
        /// Image for a microprocessor.
        /// </summary>
        public static readonly BitmapImage Cpu = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Cpu.png"));

        /// <summary>
        /// Image for a curse.
        /// </summary>
        public static readonly BitmapImage Curse = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Curse.png"));

        /// <summary>
        /// Image for a decreased scan.
        /// </summary>
        public static readonly BitmapImage Decreased = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Decreased.png"));

        /// <summary>
        /// Image for an inactive connection.
        /// </summary>
        public static readonly BitmapImage Disconnected = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Disconnected.png"));

        /// <summary>
        /// Image of two down arrows.
        /// </summary>
        public static readonly BitmapImage DownArrows = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "DownArrows.png"));

        /// <summary>
        /// Image representing mathmateical exponentiation.
        /// </summary>
        public static readonly BitmapImage ENotation = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "ENotation.png"));

        /// <summary>
        /// Image for an equal to scan.
        /// </summary>
        public static readonly BitmapImage Equal = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Equal.png"));

        /// <summary>
        /// Image for a greater than scan.
        /// </summary>
        public static readonly BitmapImage GreaterThan = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "GreaterThan.png"));

        /// <summary>
        /// Image of a glitch.
        /// </summary>
        public static readonly BitmapImage Glitch = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Glitch.png"));

        /// <summary>
        /// Image for a greater than or equal to scan.
        /// </summary>
        public static readonly BitmapImage GreaterThanOrEqual = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "GreaterThanOrEqual.png"));

        /// <summary>
        /// Image of a heart.
        /// </summary>
        public static readonly BitmapImage Heart = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Heart.png"));

        /// <summary>
        /// Image representing a 'go home' operation.
        /// </summary>
        public static readonly BitmapImage Home = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Home.png"));

        /// <summary>
        /// Image for an increased value scan.
        /// </summary>
        public static readonly BitmapImage Increased = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Increased.png"));

        /// <summary>
        /// Image for a mathematical intersection.
        /// </summary>
        public static readonly BitmapImage Intersection = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Intersection.png"));

        /// <summary>
        /// Image for histogram selection inversion.
        /// </summary>
        public static readonly BitmapImage Invert = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Invert.png"));

        /// <summary>
        /// Image of a left arrow.
        /// </summary>
        public static readonly BitmapImage LeftArrow = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LeftArrow.png"));

        /// <summary>
        /// Image of two left arrows.
        /// </summary>
        public static readonly BitmapImage LeftArrows = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LeftArrows.png"));

        /// <summary>
        /// Image for a less than scan.
        /// </summary>
        public static readonly BitmapImage LessThan = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LessThan.png"));

        /// <summary>
        /// Image for a less than or equal to scan.
        /// </summary>
        public static readonly BitmapImage LessThanOrEqual = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LessThanOrEqual.png"));

        /// <summary>
        /// Image for the letter P.
        /// </summary>
        public static readonly BitmapImage LetterP = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LetterP.png"));

        /// <summary>
        /// Image for the letter S.
        /// </summary>
        public static readonly BitmapImage LetterS = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LetterS.png"));

        /// <summary>
        /// Image for a mathematical AND.
        /// </summary>
        public static readonly BitmapImage LogicalAnd = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LogicalAnd.png"));

        /// <summary>
        /// Image for a mathematical OR.
        /// </summary>
        public static readonly BitmapImage LogicalOr = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "LogicalOr.png"));

        /// <summary>
        /// Image for the previous arrow.
        /// </summary>
        public static readonly BitmapImage Previous = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Previous.png"));

        /// <summary>
        /// Image represeting a window maximize operation.
        /// </summary>
        public static readonly BitmapImage Maximize = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Maximize.png"));

        /// <summary>
        /// Image represeting a merge operation.
        /// </summary>
        public static readonly BitmapImage Merge = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Merge.png"));

        /// <summary>
        /// Image for an decreased by X scan.
        /// </summary>
        public static readonly BitmapImage MinusX = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "MinusX.png"));

        /// <summary>
        /// Image for a down arrow.
        /// </summary>
        public static readonly BitmapImage MoveDown = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "MoveDown.png"));

        /// <summary>
        /// Image for a left arrow.
        /// </summary>
        public static readonly BitmapImage MoveLeft = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "MoveLeft.png"));

        /// <summary>
        /// Image for a right arrow.
        /// </summary>
        public static readonly BitmapImage MoveRight = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "MoveRight.png"));

        /// <summary>
        /// Image for an up arrow.
        /// </summary>
        public static readonly BitmapImage MoveUp = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "MoveUp.png"));

        /// <summary>
        /// Image for a negated value.
        /// </summary>
        public static readonly BitmapImage Negation = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Negation.png"));

        /// <summary>
        /// Image for a new scan.
        /// </summary>
        public static readonly BitmapImage New = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "New.png"));

        /// <summary>
        /// Image for the next arrow.
        /// </summary>
        public static readonly BitmapImage Next = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Next.png"));

        /// <summary>
        /// Image for a next scan.
        /// </summary>
        public static readonly BitmapImage NextScan = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "NextScan.png"));

        /// <summary>
        /// Image for a not equal scan.
        /// </summary>
        public static readonly BitmapImage NotEqual = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "NotEqual.png"));

        /// <summary>
        /// Image represeting an open operation.
        /// </summary>
        public static readonly BitmapImage Open = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Open.png"));

        /// <summary>
        /// Image of one orange block.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks1 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "OrangeBlocks1.png"));

        /// <summary>
        /// Image of two orange blocks.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks2 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "OrangeBlocks2.png"));

        /// <summary>
        /// Image of four orange blocks.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks4 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "OrangeBlocks4.png"));

        /// <summary>
        /// Image of eight orange blocks.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks8 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "OrangeBlocks8.png"));

        /// <summary>
        /// Image for an increased by X scan.
        /// </summary>
        public static readonly BitmapImage PlusX = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "PlusX.png"));

        /// <summary>
        /// Image of one purple block.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks1 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "PurpleBlocks1.png"));

        /// <summary>
        /// Image of two purple blocks.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks2 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "PurpleBlocks2.png"));

        /// <summary>
        /// Image of four purple blocks.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks4 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "PurpleBlocks4.png"));

        /// <summary>
        /// Image of eight purple blocks.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks8 = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "PurpleBlocks8.png"));

        /// <summary>
        /// Image of a right arrow.
        /// </summary>
        public static readonly BitmapImage RightArrow = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "RightArrow.png"));

        /// <summary>
        /// Image of two right arrows.
        /// </summary>
        public static readonly BitmapImage RightArrows = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "RightArrows.png"));

        /// <summary>
        /// Image for redo.
        /// </summary>
        public static readonly BitmapImage Redo = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Redo.png"));

        /// <summary>
        /// Image represeting a save operation.
        /// </summary>
        public static readonly BitmapImage Save = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Save.png"));

        /// <summary>
        /// Image represeting a script.
        /// </summary>
        public static readonly BitmapImage Script = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Script.png"));

        /// <summary>
        /// Image for search.
        /// </summary>
        public static readonly BitmapImage Search = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Search.png"));

        /// <summary>
        /// Image for process selection.
        /// </summary>
        public static readonly BitmapImage SelectProcess = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "SelectProcess.png"));

        /// <summary>
        /// Image for stop operations.
        /// </summary>
        public static readonly BitmapImage Stop = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Stop.png"));

        /// <summary>
        /// Image for Squalr.
        /// </summary>
        public static readonly BitmapImage Squalr = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Squalr.png"));

        /// <summary>
        /// Image for Squalr Dev.
        /// </summary>
        public static readonly BitmapImage SqualrDev = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "SqualrDev.png"));

        /// <summary>
        /// Image for an unchanged value scan.
        /// </summary>
        public static readonly BitmapImage Unchanged = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Unchanged.png"));

        /// <summary>
        /// Image for undo operations.
        /// </summary>
        public static readonly BitmapImage Undo = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Undo.png"));

        /// <summary>
        /// Image for a mathematical union.
        /// </summary>
        public static readonly BitmapImage Union = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "Union.png"));

        /// <summary>
        /// Image for an unknown value scan.
        /// </summary>
        public static readonly BitmapImage UnknownValue = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "UnknownValue.png"));

        /// <summary>
        /// Image for an X.
        /// </summary>
        public static readonly BitmapImage X = ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, "X.png"));

        /// <summary>
        /// The base path for image content.
        /// </summary>
        private const String ImageBasePath = "pack://application:,,,/Squalr;component/Content/Images/";
    }
    //// End class
}
//// End namespace