using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BExplorer.Shell
{
    public interface IChildInfo
    {        
        Rect GetChildRect(int itemIndex);
    }

}
