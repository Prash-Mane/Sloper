using System;
using Xamarin.Forms;
using SloperMobile.Model.CragModels;
using XLabs;
using Syncfusion.ListView.XForms;
using System.Windows.Input;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SloperMobile.CustomControls;
using SloperMobile.UserControls.CustomControls;

namespace SloperMobile.UserControls
{
    public enum MapAspectSize { Default, FitWidth, FitHeight }

    public class CragBoxesView : AbsoluteLayout
    {
        List<RelativeLayout> boxFrames = new List<RelativeLayout>();
        Image imgView;
        RelativeLayout mapName;

        public CragMapModel CragMap
        {
            get => (CragMapModel)GetValue(CragMapProperty);
            set => SetValue(CragMapProperty, value);
        }

        public Command<int> BoxCommand
        {
            get => (Command<int>)GetValue(BoxCommandProperty);
            set => SetValue(BoxCommandProperty, value);
        }

        public Command<CragMapModel> MapCommand
        { 
            get => (Command<CragMapModel>)GetValue(MapCommandProperty);
            set => SetValue(MapCommandProperty, value);
        }

        public bool IsMapNameVisible
        {
            get => (bool)GetValue(IsMapNameVisibleProperty);
            set => SetValue(IsMapNameVisibleProperty, value);
        }

        public bool IsRegionNameVisible
        {
            get => (bool)GetValue(IsRegionNameVisibleProperty);
            set => SetValue(IsRegionNameVisibleProperty, value);
        }

        public MapAspectSize MapAspect { get; set; }

        public static readonly BindableProperty CragMapProperty = BindableProperty.Create(nameof(CragMap), typeof(CragMapModel), typeof(CragBoxesView), null, BindingMode.OneWay, propertyChanged: ModelChanged);
        public static readonly BindableProperty IsMapNameVisibleProperty = BindableProperty.Create(nameof(IsMapNameVisible), typeof(bool), typeof(CragBoxesView), false, BindingMode.OneWay, propertyChanged: ModelChanged);
        public static readonly BindableProperty IsRegionNameVisibleProperty = BindableProperty.Create(nameof(IsRegionNameVisible), typeof(bool), typeof(CragBoxesView), false, BindingMode.OneWay, propertyChanged: ModelChanged);
        public static readonly BindableProperty BoxCommandProperty = BindableProperty.Create(nameof(BoxCommand), typeof(Command<int>), typeof(CragBoxesView), null, BindingMode.TwoWay);
        public static readonly BindableProperty MapCommandProperty = BindableProperty.Create(nameof(MapCommand), typeof(Command<CragMapModel>), typeof(CragBoxesView), null, BindingMode.TwoWay);


        static void ModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as CragBoxesView;
            var model = newValue as CragMapModel;
            view.boxFrames.Clear();
            view.Children.Clear();
            if (model == null)
                return;
            view.imgView = new Image { 
                Source = model.Map.imagedata.GetImageSource()
            };
            view.Children.Add(view.imgView, new Point(0, 0));

            var mapTap = new TapGestureRecognizer();
            mapTap.Tapped += (s, e) => {
                if (view.MapCommand != null && view.MapCommand.CanExecute(model))
                    view.MapCommand.Execute(model);
            };
            view.imgView.GestureRecognizers.Add(mapTap);

            foreach (var boxData in model.MapRegions)
            {
                var container = new RelativeLayout();
                view.boxFrames.Add(container);
                var boxFrame = new Frame { 
                    Padding = new Thickness(1, 1, 1, 1), 
                    BorderColor = Color.FromHex("#FF8E2D"), 
                    BackgroundColor = Color.Black,
                    Opacity = 0.4,
                    HasShadow = false,
                    CornerRadius = 2f
                };
                container.Children.Add(boxFrame, () => new Rectangle(0, 0, container.Width, container.Height));
                var boxTap = new TapGestureRecognizer();
                boxTap.Tapped += (s, e) => {
                    if (view.BoxCommand != null && view.BoxCommand.CanExecute(boxData.sector_id))
                        view.BoxCommand.Execute(boxData.sector_id);
                    else if (view.MapCommand != null && view.MapCommand.CanExecute(model))
                        view.MapCommand.Execute(model);
                };
                container.GestureRecognizers.Add(boxTap);

                if (view.IsRegionNameVisible)
                {
                    var lblBoxTitle = new Label { 
                        Text = boxData.SectorName,
                        FontSize = 12,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        LineBreakMode = LineBreakMode.WordWrap,
                        TextColor = Color.FromHex("#FF8E2D")
                    };
                    //lblBoxTitle.Effects.Add(new LabelSizeFontToFitEffect { Lines = 5 });
                    container.Children.Add(lblBoxTitle, () => new Rectangle(0, 0, container.Width, container.Height));
                }

                view.Children.Add(container, new Point(0, 0));
            }

            if (view.IsMapNameVisible) 
            {
                var container = view.mapName = new RelativeLayout { 
                    HeightRequest = 50,
                    IsEnabled = false
                };
                    
                var imgTitle = new Image { 
                    Source = ImageSource.FromFile("bg_gradient_trans_black"),
                    HeightRequest = 50,
                    Margin = 0,
                    Aspect = Aspect.Fill,
                };
                container.Children.Add(imgTitle, () => new Rectangle(0, 0, container.Width, container.Height));

                var lblMapTitle = new Label
                {
                    Text = $"Map: {model.Map.map_name}",
                    TextColor = Color.White,
                    FontSize = 20,
                    Margin = new Thickness(0, 0, 10, 0),
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Center
                };

                lblMapTitle.Effects.Add(new ShadowEffect { DistanceX = 1, DistanceY = 1, Color = Color.Gray, Radius = 1 });
                container.Children.Add(lblMapTitle, () => new Rectangle(0, 0, container.Width, container.Height));
                view.Children.Add(container);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (Width > 0 && Height > 0 && CragMap != null) //view not ready yet
            {
                UpdateSize();

                if (MapAspect == MapAspectSize.FitWidth)
                {
                    var aspect = CragMap.Map.width / CragMap.Map.height;
                    height = width / aspect;
                    if (HeightRequest != height)
                    {
                        HeightRequest = height;
                        WidthRequest = width;
                    }
                }
                else if (MapAspect == MapAspectSize.FitHeight)
                {
                    var aspect = CragMap.Map.width / CragMap.Map.height;
                    width = height * aspect;
                    if (WidthRequest != width)
                    {
                        HeightRequest = height;
                        WidthRequest = width;
                    }
                }
            }
        }

        void UpdateSize() {
            if (boxFrames.Count() != CragMap.MapRegions.Count) //boxes not added yet
                return;

            double coef = 1;
            var imgAspect = CragMap.Map.width / CragMap.Map.height;
            var containerAspect = Width / Height;
            coef = imgAspect > containerAspect ? Width /CragMap.Map.width : Height/ CragMap.Map.height;
            var imgBounds = imgView.Bounds;
            imgBounds.Width = imgAspect > containerAspect ? Width : Height * imgAspect;
            imgBounds.Height = imgBounds.Width / imgAspect;
            imgBounds.X = (Width - imgBounds.Width) / 2;
            imgBounds.Y = (Height - imgBounds.Height) / 2;
            SetLayoutBounds(imgView, imgBounds);

            for (int i = 0; i < CragMap.MapRegions.Count; i++)
            {
                var boxFrame = boxFrames.ElementAt(i);
                var mapRegion = CragMap.MapRegions[i];
                var bounds = boxFrame.Bounds;
                bounds.X = mapRegion.x * coef + imgBounds.X;
                bounds.Y = mapRegion.y * coef + imgBounds.Y;
                bounds.Width = mapRegion.width * coef;
                bounds.Height = mapRegion.height * coef;
                SetLayoutBounds(boxFrame, bounds);

                //to ensure that font is shrinked if needed
                //TODO: change it to work with size to fit effect. Android pre 26 is not handled yet
                if (IsRegionNameVisible)
                {
                    var lblBoxTitle = boxFrame.Children.OfType<Label>().FirstOrDefault();
                    if (lblBoxTitle != null)
                    {
                        var lineHeight = 2;
                        var charWidth = 0.5;
                        var charCount = lblBoxTitle.Text.Length;
                        var fontSize = (int)Math.Sqrt(bounds.Width * bounds.Height / (charCount * lineHeight * charWidth));
                        lblBoxTitle.FontSize = Math.Min(fontSize, 10);
                    }
                }
            }


            if (IsMapNameVisible && mapName != null) {
                var bounds = mapName.Bounds;
                bounds.Width = Width;
                bounds.Y = imgBounds.Height + imgBounds.Y - bounds.Height - 10;
                SetLayoutBounds(mapName, bounds);
            }
        }
    }
}
