using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Contest_Killer.Utils
{
    public class ScrollViewerHelper
    {
        public static readonly DependencyProperty AlwaysScrollToEndProperty = DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(false, AlwaysScrollToEndChanged));
        private static bool autoScroll;
        
        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (viewer == null) throw new ArgumentException("sender is not ScrollViewer");
            bool alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
            if (alwaysScrollToEnd)
            {
                viewer.ScrollToEnd();
                viewer.ScrollChanged += ScrollChanged;
            }
            else
            {
                viewer.ScrollChanged -= ScrollChanged;
            }
        }

        public static bool GetAlwaysScrollToEnd(ScrollViewer viewer)
        {
            if(viewer == null) throw new ArgumentNullException("viewer");
            return (bool)viewer.GetValue(AlwaysScrollToEndProperty);
        }

        public static void SetAlwaysScrollToEnd(ScrollViewer viewer, bool propertyVal)
        {
            if (viewer == null) throw new ArgumentNullException("viewer");
            viewer.SetValue(AlwaysScrollToEndProperty, propertyVal);
        }

        public static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (viewer == null) throw new ArgumentException("sender is not ScrollViewer");

            if (e.ExtentHeightChange == 0) autoScroll = viewer.VerticalOffset == viewer.ScrollableHeight;
            if (autoScroll && e.ExtentHeightChange != 0) viewer.ScrollToVerticalOffset(viewer.ExtentHeight);
        }
    }
}
