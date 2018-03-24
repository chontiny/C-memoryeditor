/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Squalr Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://Squalr.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

using System;

namespace Squalr.Wpf.AvalonDock.Themes
{
    public class DarkTheme : Theme
    {
        public override Uri GetResourceUri()
        {
            return new Uri(
                "/Squalr.Wpf.AvalonDock;component/Themes/DarkTheme.xaml",
                UriKind.Relative);
        }
    }
}
