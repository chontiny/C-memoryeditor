/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Squalr Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://Squalr.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

using Squalr.Theme.Layout;
using System.ComponentModel;

namespace Squalr.Theme
{
    public class DocumentClosingEventArgs : CancelEventArgs
    {
        public DocumentClosingEventArgs(LayoutDocument document)
        {
            Document = document;
        }

        public LayoutDocument Document
        {
            get;
            private set;
        }
    }
}
