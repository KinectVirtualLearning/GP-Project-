using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coding4Fun.Kinect.Wpf.Controls;
namespace Real_Final_Project
{
    public static class Generics
    {

        public static int LoadingStatus = 0;

        public static void ResetHandPosition(HoverButton btnHoverButton)
        {
            btnHoverButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            btnHoverButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

        }

    }
}
