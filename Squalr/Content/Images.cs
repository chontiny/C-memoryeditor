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
        public static readonly BitmapImage BlueBlocks1 = Images.LoadImage("BlueBlocks1.png");

        /// <summary>
        /// Image of two blue blocks.
        /// </summary>
        public static readonly BitmapImage BlueBlocks2 = Images.LoadImage("BlueBlocks2.png");

        /// <summary>
        /// Image of four blue blocks.
        /// </summary>
        public static readonly BitmapImage BlueBlocks4 = Images.LoadImage("BlueBlocks4.png");

        /// <summary>
        /// Image of eight blue blocks.
        /// </summary>
        public static readonly BitmapImage BlueBlocks8 = Images.LoadImage("BlueBlocks8.png");

        /// <summary>
        /// Image for cancel operations.
        /// </summary>
        public static readonly BitmapImage Cancel = Images.LoadImage("Cancel.png");

        /// <summary>
        /// Image for a changed scan.
        /// </summary>
        public static readonly BitmapImage Changed = Images.LoadImage("Changed.png");

        /// <summary>
        /// Image for a cog.
        /// </summary>
        public static readonly BitmapImage Cog = Images.LoadImage("Cog.png");

        /// <summary>
        /// Image for a coin.
        /// </summary>
        public static readonly BitmapImage Coin = Images.LoadImage("Coin.png");

        /// <summary>
        /// Image for a connection.
        /// </summary>
        public static readonly BitmapImage Connect = Images.LoadImage("Connect.png");

        /// <summary>
        /// Image for an active connection.
        /// </summary>
        public static readonly BitmapImage Connected = Images.LoadImage("Connected.png");

        /// <summary>
        /// Image for a value collection scan.
        /// </summary>
        public static readonly BitmapImage CollectValues = Images.LoadImage("CollectValues.png");

        /// <summary>
        /// Image for a microprocessor.
        /// </summary>
        public static readonly BitmapImage Cpu = Images.LoadImage("Cpu.png");

        /// <summary>
        /// Image for a curse.
        /// </summary>
        public static readonly BitmapImage Curse = Images.LoadImage("Curse.png");

        /// <summary>
        /// Image for a decreased scan.
        /// </summary>
        public static readonly BitmapImage Decreased = Images.LoadImage("Decreased.png");

        /// <summary>
        /// Image for an inactive connection.
        /// </summary>
        public static readonly BitmapImage Disconnected = Images.LoadImage("Disconnected.png");

        /// <summary>
        /// Image of two down arrows.
        /// </summary>
        public static readonly BitmapImage DownArrows = Images.LoadImage("DownArrows.png");

        /// <summary>
        /// Image for editing.
        /// </summary>
        public static readonly BitmapImage Edit = Images.LoadImage("Edit.png");

        /// <summary>
        /// Image representing mathmateical exponentiation.
        /// </summary>
        public static readonly BitmapImage ENotation = Images.LoadImage("ENotation.png");

        /// <summary>
        /// Image for an equal to scan.
        /// </summary>
        public static readonly BitmapImage Equal = Images.LoadImage("Equal.png");

        /// <summary>
        /// Image for a greater than scan.
        /// </summary>
        public static readonly BitmapImage GreaterThan = Images.LoadImage("GreaterThan.png");

        /// <summary>
        /// Image of a glitch.
        /// </summary>
        public static readonly BitmapImage Glitch = Images.LoadImage("Glitch.png");

        /// <summary>
        /// Image for a greater than or equal to scan.
        /// </summary>
        public static readonly BitmapImage GreaterThanOrEqual = Images.LoadImage("GreaterThanOrEqual.png");

        /// <summary>
        /// Image of a heart.
        /// </summary>
        public static readonly BitmapImage Heart = Images.LoadImage("Heart.png");

        /// <summary>
        /// Image representing a 'go home' operation.
        /// </summary>
        public static readonly BitmapImage Home = Images.LoadImage("Home.png");

        /// <summary>
        /// Image for an increased value scan.
        /// </summary>
        public static readonly BitmapImage Increased = Images.LoadImage("Increased.png");

        /// <summary>
        /// Image for a mathematical intersection.
        /// </summary>
        public static readonly BitmapImage Intersection = Images.LoadImage("Intersection.png");

        /// <summary>
        /// Image for histogram selection inversion.
        /// </summary>
        public static readonly BitmapImage Invert = Images.LoadImage("Invert.png");

        /// <summary>
        /// Image of a left arrow.
        /// </summary>
        public static readonly BitmapImage LeftArrow = Images.LoadImage("LeftArrow.png");

        /// <summary>
        /// Image of two left arrows.
        /// </summary>
        public static readonly BitmapImage LeftArrows = Images.LoadImage("LeftArrows.png");

        /// <summary>
        /// Image for a less than scan.
        /// </summary>
        public static readonly BitmapImage LessThan = Images.LoadImage("LessThan.png");

        /// <summary>
        /// Image for a less than or equal to scan.
        /// </summary>
        public static readonly BitmapImage LessThanOrEqual = Images.LoadImage("LessThanOrEqual.png");

        /// <summary>
        /// Image for the letter P.
        /// </summary>
        public static readonly BitmapImage LetterP = Images.LoadImage("LetterP.png");

        /// <summary>
        /// Image for the letter S.
        /// </summary>
        public static readonly BitmapImage LetterS = Images.LoadImage("LetterS.png");

        /// <summary>
        /// Image for a mathematical AND.
        /// </summary>
        public static readonly BitmapImage LogicalAnd = Images.LoadImage("LogicalAnd.png");

        /// <summary>
        /// Image for a mathematical OR.
        /// </summary>
        public static readonly BitmapImage LogicalOr = Images.LoadImage("LogicalOr.png");

        /// <summary>
        /// Image for the previous arrow.
        /// </summary>
        public static readonly BitmapImage Previous = Images.LoadImage("Previous.png");

        /// <summary>
        /// Image represeting a window maximize operation.
        /// </summary>
        public static readonly BitmapImage Maximize = Images.LoadImage("Maximize.png");

        /// <summary>
        /// Image represeting a merge operation.
        /// </summary>
        public static readonly BitmapImage Merge = Images.LoadImage("Merge.png");

        /// <summary>
        /// Image for an decreased by X scan.
        /// </summary>
        public static readonly BitmapImage MinusX = Images.LoadImage("MinusX.png");

        /// <summary>
        /// Image for a down arrow.
        /// </summary>
        public static readonly BitmapImage MoveDown = Images.LoadImage("MoveDown.png");

        /// <summary>
        /// Image for a left arrow.
        /// </summary>
        public static readonly BitmapImage MoveLeft = Images.LoadImage("MoveLeft.png");

        /// <summary>
        /// Image for a right arrow.
        /// </summary>
        public static readonly BitmapImage MoveRight = Images.LoadImage("MoveRight.png");

        /// <summary>
        /// Image for an up arrow.
        /// </summary>
        public static readonly BitmapImage MoveUp = Images.LoadImage("MoveUp.png");

        /// <summary>
        /// Image for a negated value.
        /// </summary>
        public static readonly BitmapImage Negation = Images.LoadImage("Negation.png");

        /// <summary>
        /// Image for a new scan.
        /// </summary>
        public static readonly BitmapImage New = Images.LoadImage("New.png");

        /// <summary>
        /// Image for the next arrow.
        /// </summary>
        public static readonly BitmapImage Next = Images.LoadImage("Next.png");

        /// <summary>
        /// Image for a next scan.
        /// </summary>
        public static readonly BitmapImage NextScan = Images.LoadImage("NextScan.png");

        /// <summary>
        /// Image for a not equal scan.
        /// </summary>
        public static readonly BitmapImage NotEqual = Images.LoadImage("NotEqual.png");

        /// <summary>
        /// Image represeting an open operation.
        /// </summary>
        public static readonly BitmapImage Open = Images.LoadImage("Open.png");

        /// <summary>
        /// Image of one orange block.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks1 = Images.LoadImage("OrangeBlocks1.png");

        /// <summary>
        /// Image of two orange blocks.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks2 = Images.LoadImage("OrangeBlocks2.png");

        /// <summary>
        /// Image of four orange blocks.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks4 = Images.LoadImage("OrangeBlocks4.png");

        /// <summary>
        /// Image of eight orange blocks.
        /// </summary>
        public static readonly BitmapImage OrangeBlocks8 = Images.LoadImage("OrangeBlocks8.png");

        /// <summary>
        /// Image for an increased by X scan.
        /// </summary>
        public static readonly BitmapImage PlusX = Images.LoadImage("PlusX.png");

        /// <summary>
        /// Image for properties.
        /// </summary>
        public static readonly BitmapImage Properties = Images.LoadImage("Properties.png");

        /// <summary>
        /// Image of one purple block.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks1 = Images.LoadImage("PurpleBlocks1.png");

        /// <summary>
        /// Image of two purple blocks.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks2 = Images.LoadImage("PurpleBlocks2.png");

        /// <summary>
        /// Image of four purple blocks.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks4 = Images.LoadImage("PurpleBlocks4.png");

        /// <summary>
        /// Image of eight purple blocks.
        /// </summary>
        public static readonly BitmapImage PurpleBlocks8 = Images.LoadImage("PurpleBlocks8.png");

        /// <summary>
        /// Image of a right arrow.
        /// </summary>
        public static readonly BitmapImage RightArrow = Images.LoadImage("RightArrow.png");

        /// <summary>
        /// Image of two right arrows.
        /// </summary>
        public static readonly BitmapImage RightArrows = Images.LoadImage("RightArrows.png");

        /// <summary>
        /// Image for redo.
        /// </summary>
        public static readonly BitmapImage Redo = Images.LoadImage("Redo.png");

        /// <summary>
        /// Image represeting a save operation.
        /// </summary>
        public static readonly BitmapImage Save = Images.LoadImage("Save.png");

        /// <summary>
        /// Image represeting a script.
        /// </summary>
        public static readonly BitmapImage Script = Images.LoadImage("Script.png");

        /// <summary>
        /// Image for search.
        /// </summary>
        public static readonly BitmapImage Search = Images.LoadImage("Search.png");

        /// <summary>
        /// Image for process selection.
        /// </summary>
        public static readonly BitmapImage SelectProcess = Images.LoadImage("SelectProcess.png");

        /// <summary>
        /// Image for stop operations.
        /// </summary>
        public static readonly BitmapImage Stop = Images.LoadImage("Stop.png");

        /// <summary>
        /// Image for Squalr.
        /// </summary>
        public static readonly BitmapImage Squalr = Images.LoadImage("Squalr.png");

        /// <summary>
        /// Image for Squalr Dev.
        /// </summary>
        public static readonly BitmapImage SqualrDev = Images.LoadImage("SqualrDev.png");

        /// <summary>
        /// Image for an unchanged value scan.
        /// </summary>
        public static readonly BitmapImage Unchanged = Images.LoadImage("Unchanged.png");

        /// <summary>
        /// Image for undo operations.
        /// </summary>
        public static readonly BitmapImage Undo = Images.LoadImage("Undo.png");

        /// <summary>
        /// Image for a mathematical union.
        /// </summary>
        public static readonly BitmapImage Union = Images.LoadImage("Union.png");

        /// <summary>
        /// Image for an unknown value scan.
        /// </summary>
        public static readonly BitmapImage UnknownValue = Images.LoadImage("UnknownValue.png");

        /// <summary>
        /// Image for an X.
        /// </summary>
        public static readonly BitmapImage X = Images.LoadImage("X.png");

        /// <summary>
        /// The base path for image content.
        /// </summary>
        private const String ImageBasePath = "pack://application:,,,/Squalr;component/Content/Images/";

        /// <summary>
        /// Loads the specified image.
        /// </summary>
        /// <param name="imageName">The name of the image.</param>
        /// <returns>The specified image loaded as a bitmap.</returns>
        private static BitmapImage LoadImage(String imageName)
        {
            return ImageUtils.LoadImage(Path.Combine(Images.ImageBasePath, imageName));
        }
    }
    //// End class
}
//// End namespace