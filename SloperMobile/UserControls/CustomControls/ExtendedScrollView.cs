using System;

using Xamarin.Forms;

namespace SloperMobile.CustomControls
{
    public class ExtendedScrollView : ScrollView
    {
        public int MaxHeight { get; set; }

        public ExtendedScrollView()
        {
            SizeChanged += CustomScrollView_SizeChanged;
        }

        void CustomScrollView_SizeChanged(object sender, EventArgs e)
        {
            var view = (ExtendedScrollView)sender;
            if (MaxHeight != 0 && view.Height > view.MaxHeight)
            {
                view.HeightRequest = MaxHeight;
            }
        }

    }
}

