using Acr.UserDialogs;
using Prism.Navigation;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.CustomControls;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.PointModels;
using SloperMobile.Model.ResponseModels;
using SloperMobile.Model.TopoModels;
using SloperMobile.ViewModel;
using SloperMobile.ViewModel.SectorViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XLabs.Platform.Device;

namespace SloperMobile.Views.SectorPages
{
	//TODO: Refactor
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SectorTopoDetailsPage : ContentPage, INavigationAware, INavigatedAware
    {
        private readonly IRepository<CragImageTable> cragImageRepository;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IUserDialogs userDialogs;
		private readonly IExceptionSynchronizationManager exceptionManager;

		private float drawnHeightRatio = 1;
		private float xOffset;
		private IDevice device;
        private const int BoxTextFontSize = 9;
        private int TopBarHeight = 40;
        private double GridBounds = 18;
        private IList<AddedPointModel> listDiamondsRoute = new List<AddedPointModel>();
        private TopoImageResponseModel topoImg = null;
        private ZoomableScrollView parent;
        private CragImageTable cragImage;
        private string stringBase64 = string.Empty;
        private float height, globalHeight, globalWidth;
        private int? routeId;
        private bool isDiamondSingleClick = false;
        private float yRatio;
        private int pageIndex;
        private bool singleRoute;
        private int hasBeingDrawen;
        private bool isDiamondSelected;
        private bool noLines;
        private bool isUnlocked;
        private int childrenCount;
        private double initialHeight;
        private double initialWidth;
        private float xRatio;
        private double scaleIn = 1;
        private double scaleOut = 1;
        private byte[] imageBytes;
        private int numberOfZomms = 0;
        private bool isNavigatedFromSectorImage;
        private MapListModel currentSector;
		private bool isDiamondDrawnOnCanvas = true;
		private SectorTopoDetailsViewModel sectorTopoDetailsViewModel;
		private bool redrawOnlyRoutes;

		public SectorTopoDetailsPage(
            IRepository<CragImageTable> cragImageRepository,
            IRepository<TopoTable> topoRepository,
            IUserDialogs userDialogs,
			IExceptionSynchronizationManager exceptionManager)
        {
            InitializeComponent();
            this.cragImageRepository = cragImageRepository;
            this.topoRepository = topoRepository;
            this.userDialogs = userDialogs;
			this.exceptionManager = exceptionManager;
		}

        public async Task SetAllData(
            MapListModel CurrentSector,
            TopoImageResponseModel topoImageResponse,
            int routeId,
            int pageIndex,
            bool singleRoute,
            bool noLines)
        {
            try
            {
                this.noLines = noLines;
                device = XLabs.Ioc.Resolver.Resolve<IDevice>();
                NavigationPage.SetHasNavigationBar(this, false);
                Title = CurrentSector.SectorName;
                currentSector = CurrentSector;
                sectorTopoDetailsViewModel.InvalidateSuface += OnInvalidateSurface;
                //cragImage = await cragImageRepository.FindAsync(tcragimg => tcragimg.crag_id == Settings.ActiveCrag);
                this.routeId = routeId;
                this.pageIndex = pageIndex;
                this.singleRoute = singleRoute;
                isDiamondSelected = singleRoute;
                topoImg = topoImageResponse;
                if (Device.RuntimePlatform == Device.iOS)
                {
                    TopBarHeight = 80;
                }
                else
                {
                    TopBarHeight = 20;
                }

                //if (!isUnlocked && !Settings.AppPurchased)
                //{
                //    sectorTopoDetailsViewModel.LoadCragAndDefaultImage();
                //    skCanvasAndroid.IsVisible = false;
                //    skCanvasiOS.IsVisible = false;
                //}


                if (topoImg == null) //|| !isUnlocked) && !Settings.AppPurchased)
                    return;

                InitializeHeights();
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.SetAllData),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"RouteId : {routeId} ; SectorName : {Title}, sender = {Newtonsoft.Json.JsonConvert.SerializeObject(CurrentSector)}"
                });
            }
        }

        private void InitializeHeights()
        {
            if (topoImg == null)
            {
                return;
            }

            var deviceHeight = (float)(device.Display.Height/1.4);//temp hack
                                                    //      - (FooterUC.Height * device.Display.Scale)
                                                        //  - (BackHeaderUC.Height * device.Display.Scale)
                                                          //- TopBarHeight * device.Display.Scale);
            yRatio = (float)deviceHeight / float.Parse(topoImg.image.height);
            height = deviceHeight;
            globalHeight = height;
            globalWidth = float.Parse(topoImg.image.width) * yRatio;

            AndroidAbsoluteLayout.HeightRequest = height / device.Display.Scale;
            AndroidAbsoluteLayout.WidthRequest = globalWidth / device.Display.Scale;
            iOSdAbsoluteLayout.HeightRequest = height / device.Display.Scale;
            iOSdAbsoluteLayout.WidthRequest = globalWidth / device.Display.Scale;

            if (Device.RuntimePlatform == Device.iOS)
            {
                initialWidth = iOSdAbsoluteLayout.WidthRequest;
                initialHeight = iOSdAbsoluteLayout.HeightRequest;
                skCanvasiOS.InvalidateSurface();
            }
            else
            {
                initialWidth = AndroidAbsoluteLayout.WidthRequest;
                initialHeight = AndroidAbsoluteLayout.HeightRequest;
                skCanvasAndroid.InvalidateSurface();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (this.Parent == null)
            {
                return;
            }

            if (childrenCount == 1)
            {
                rightArrow.IsVisible = false;
                leftArrow.IsVisible = false;

                //not to hide, because children will be reordered,but set opacity to 1, now to be visible
                ArrowBoxViewLeft.IsVisible = false;
                ArrowBoxViewRight.IsVisible = false;
            }

            if (childrenCount - 1 == pageIndex)
            {
                ArrowBoxViewRight.IsVisible = false;
                rightArrow.IsVisible = false;
            }

            if (pageIndex == 0)
            {
                ArrowBoxViewLeft.IsVisible = false;
                leftArrow.IsVisible = false;
            }

            InitializeHeights();
            if (Device.RuntimePlatform == Device.Android)
            {
                skCanvasAndroid.InvalidateSurface();
            }
        }

        private void ShowRoute(int? routeId)
        {
            isDiamondSelected = true;
            isDiamondSingleClick = true;
            this.routeId = routeId;
            if (Device.RuntimePlatform == Device.iOS)
            {
                skCanvasiOS.InvalidateSurface();
            }
            else
            {
                skCanvasAndroid.InvalidateSurface();
            }
        }

        private async void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            try
            {
                MainDrawing(e);
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnPaintSample),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"RouteId : {routeId} ; SectorName : {Title}, sender = {Newtonsoft.Json.JsonConvert.SerializeObject(sender)}"
				});
			}
            finally
            {
                //if (isUnlocked)
                //{
                    UserDialogs.Instance.HideLoading();
                //}
            }
        }

        private async void MainDrawing(SKPaintSurfaceEventArgs e)
        {
            if (topoImg == null)
            {
                return;
            }

            if (Device.RuntimePlatform == Device.Android && skCanvasAndroid.CanvasSize.Height > 4000)
            {
                return;
            }

            var canvas = e.Surface.Canvas;
			if (!redrawOnlyRoutes || isNavigatedFromSectorImage)
			{
				canvas.Clear();
			}

            if (string.IsNullOrEmpty(topoImg.image.data))
            {
                return;
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                parent = androidZoomScroll;
            }
            else
            {
                parent = iOSZoomScroll;
            }

            //code to draw image
            parent.DeviceScaleFactor = device.Display.Scale;
            parent.InitialHeight = initialHeight;
            parent.InitialWidth = initialWidth;

            if (parent.RescaleOniOS == null)
            {
                parent.RescaleOniOS += () =>
                {
                    Device.BeginInvokeOnMainThread(skCanvasiOS.InvalidateSurface);
                };
            }

            if (parent.RescaleOnAndroid == null)
            {
                parent.RescaleOnAndroid += () => Device.BeginInvokeOnMainThread(skCanvasAndroid.InvalidateSurface);
            }

			if (!redrawOnlyRoutes || isNavigatedFromSectorImage)
			{
				// decode the bitmap from the stream
				if (imageBytes == null)
				{
					var strimg64 = topoImg.image.data.Split(',')[1];
					if (string.IsNullOrEmpty(strimg64))
					{
						return;
					}

					imageBytes = Convert.FromBase64String(strimg64);
				}

				using (var fileStream = new MemoryStream(imageBytes))
				using (var stream = new SKManagedStream(fileStream))
				using (var bitmap = SKBitmap.Decode(stream))
				using (var paint = new SKPaint())
				{
					canvas.Clear();
					paint.FilterQuality = SKFilterQuality.High;

					if (Device.RuntimePlatform == Device.Android)
					{
						try
						{
							canvas.DrawBitmap(bitmap, SKRect.Create((float)(skCanvasAndroid.CanvasSize.Width),
									(float)(skCanvasAndroid.CanvasSize.Height)),
								paint);

							Debug.WriteLine($"Height = {AndroidAbsoluteLayout.Height}; Width = {AndroidAbsoluteLayout.Width}");
							Debug.WriteLine(
								$"Height = {skCanvasAndroid.CanvasSize.Height}; Width = {skCanvasAndroid.CanvasSize.Width}");
						}
						catch (Exception ex)
						{
							await exceptionManager.LogException(new ExceptionTable
							{
								Line = 295,
								Method = nameof(this.MainDrawing),
								Page = this.GetType().Name,
								StackTrace = ex.StackTrace,
								Exception = ex.Message,
								Data =
									$"RouteId : {routeId} ; SectorName : {Title}, SKPaintSerface = {Newtonsoft.Json.JsonConvert.SerializeObject(e)}"
							});
						}

						yRatio = (float)(skCanvasAndroid.CanvasSize.Height) / float.Parse(topoImg.image.height);
						xRatio = (float)(skCanvasAndroid.CanvasSize.Width) / float.Parse(topoImg.image.width);
						parent.ScaleFactor = 1;
					}
					else
					{

						canvas.DrawBitmap(bitmap,
							SKRect.Create((float)(skCanvasiOS.CanvasSize.Width), (float)(skCanvasiOS.CanvasSize.Height)), paint);
						yRatio = (float)skCanvasiOS.CanvasSize.Height / float.Parse(topoImg.image.height);
						xRatio = (float)(skCanvasiOS.CanvasSize.Width) / float.Parse(topoImg.image.width);
					}
				}
			}

            if (noLines)
            {
                return;
            }

            ////code to draw line
            using (new SKAutoCanvasRestore(canvas, false))
            {
                DrawLine(canvas);

				if (sectorTopoDetailsViewModel.singleRouteImageBytes != null)
				{
					//code to get single route image
					var image = e.Surface.Snapshot();
					var data = image.Encode(SKEncodedImageFormat.Png, 100);
					var bytes = data.ToArray();
					var basedata = Convert.ToBase64String(bytes);
					sectorTopoDetailsViewModel.singleRouteImageBytes = basedata;
				}

				redrawOnlyRoutes = false;
			}
        }

        public void DrawLine(SKCanvas _skCanvas)
        {
			if (routeId > 0 && topoImg.drawing.Count() > 1)
			{
				int index = topoImg.drawing.Where<TopoDrawingModel>(x => x.id == routeId).Select<TopoDrawingModel, int>(x => topoImg.drawing.IndexOf(x)).Single<int>();
				var item = topoImg.drawing[index];
				topoImg.drawing.RemoveAt(index);
				topoImg.drawing.Add(item);
			}
			using (var path = new SKPath())
            {
                for (int j = 0; j < topoImg.drawing.Count; j++)
                {
                    if (routeId == 0 || !isDiamondSelected)
                    {
                        DrawPathAndAnnotation(
                        _skCanvas,
                        j,
                        HexToColor(topoImg.drawing[j].line.style.color),
                        Device.RuntimePlatform == Device.Android ? SKPaintStyle.Fill : SKPaintStyle.Stroke);
                    }
                    else
                    {
                        var color = HexToColor(topoImg.drawing[j].line.style.color);
                        if (routeId != topoImg.drawing[j].id)
                            color = color.WithAlpha(128);
                        DrawPathAndAnnotation(
                        _skCanvas,
                        j,
                        color,
                        Device.RuntimePlatform == Device.Android ? SKPaintStyle.Fill : SKPaintStyle.Stroke);
                    }
                }

				drawnHeightRatio = skCanvasAndroid.CanvasSize.Width / (float)initialHeight;
				path.Close();
            }
        }

        private void DrawPathAndAnnotation(
            SKCanvas _skCanvas,
            int j,
            SKColor color,
            SKPaintStyle paintStyle)
        {
            float ptx1, ptx2 = 0, pty1, pty2 = 0;
            var drawingLine = topoImg.drawing[j].line;
            var strokeWidth = Convert.ToSingle(drawingLine.style.width) * (float)device.Display.Scale /
                              parent.ScaleFactor;
            var pathEffect = SKPathEffect.CreateDash(drawingLine.style.dashPattern
                .Select(item => item * (float)device.Display.Scale / parent.ScaleFactor)
                .ToArray(), 0);
            for (int k = 0; k < drawingLine.points.Count; k++)
            {
				bool isGapAnnotation = false;
                ptx1 = float.Parse(drawingLine.points[k].x) * xRatio ;
                pty1 = float.Parse(drawingLine.points[k].y) * yRatio;
                if (k != (drawingLine.points.Count - 1))
                {
                    ptx2 = float.Parse(drawingLine.points[k + 1].x) * xRatio ;
                    pty2 = float.Parse(drawingLine.points[k + 1].y) * yRatio;
					if (drawingLine.points[k + 1].type == "7")
						isGapAnnotation = true;
                }

                var rethinLinePaint = new SKPaint
                {
                    Style = paintStyle,
                    Color = color,
                    StrokeWidth = strokeWidth,
                    PathEffect = pathEffect
                };

                //draw line (don't draw line for 'Gap Annotation')
				if (!isGapAnnotation && ptx2 != 0 && pty2 != 0)
					_skCanvas.DrawLine(ptx1, pty1, ptx2, pty2, rethinLinePaint);
            }

            //draw annotation
            DrawAnnotation(drawingLine, _skCanvas, yRatio, topoImg.drawing[j].gradeBucket, (j + 1), topoImg.drawing[j].id, routeId);
        }

        public async void DrawAnnotation(TopoLineModel topoimgTop, SKCanvas _skCanvas, float ratio, string gradeBucket, int _routecnt, int id, int? _routeId)
        {
            for (int i = 0; i < topoimgTop.points.Count; i++)
            {
                string strimg64 = string.Empty;

                var faded = routeId > 0 && id != routeId;

			    if (topoimgTop.points[i].type == "1")
			    {
					//commented out this as its not in use now
					//if (sectorTopoDetailsViewModel.DisplayRoutePopupSm == true)
					//{
					//	if(isDiamondDrawnOnCanvas)
					//	SetupRectangular(topoimgTop, _skCanvas, i, GetGradeBucketHex(gradeBucket));
					//}
					AbsoluteLayout parentLayout;
				    double x = 0;
				    double y = 0;

				    var existInAdded = listDiamondsRoute
					    .FirstOrDefault(item => item.PointId == Convert.ToInt32(id)
                                        && item.Points.X == Convert.ToDouble(topoimgTop.points[i].x, CultureInfo.InvariantCulture)
                                        && item.Points.Y == Convert.ToDouble(topoimgTop.points[i].y, CultureInfo.InvariantCulture));

				   
                        var point = new Xamarin.Forms.Point(Convert.ToDouble(topoimgTop.points[i].x, CultureInfo.InvariantCulture),
                                                            Convert.ToDouble(topoimgTop.points[i].y, CultureInfo.InvariantCulture));
					if (existInAdded == null)
					{
						listDiamondsRoute.Add(new AddedPointModel
						{
							PointId = (int)id,
							Points = point
						});
					}

					    var color = GetGradeBucketHex(gradeBucket);
					    var gridWithId = new GridWithId(id, color, _routecnt.ToString(), BoxTextFontSize, GridBounds, GridBounds,
						    point);
                        if (faded)
                            gridWithId.Opacity = 0.5f;
					    SetupParentAndCoordinates(topoimgTop, ratio, i, out parentLayout, out x, out y);

					    //if (isDiamondSingleClick)
					    //{
						   // for (int c = (parentLayout.Children.Count() - 1); c > 0; c--)
						   // {
							  //  if (c > 0)
								 //   parentLayout.Children.RemoveAt(c);
						   // }

						   // isDiamondSingleClick = false;
					    //}

					    if (Device.RuntimePlatform == Device.iOS)
					    {
						    AbsoluteLayout.SetLayoutBounds(gridWithId, new Rectangle(x, y, GridBounds, GridBounds / parent.ScaleFactor));
					    }
					    else
					    {
						    AbsoluteLayout.SetLayoutBounds(gridWithId, new Rectangle(x, y, GridBounds, GridBounds));
					    }

					    AbsoluteLayout.SetLayoutFlags(gridWithId, AbsoluteLayoutFlags.None);

					    var centerOnlyFirst = listDiamondsRoute.Count(item => item.PointId == Convert.ToInt32(id));
					    if (singleRoute && centerOnlyFirst == 1)
					    {
						    CenterRoute(x + GridBounds / 2, 0);
					    }

					    var tapGesture = new TapGestureRecognizer();
					    tapGesture.Tapped += (item, eventArgs) =>
					    {
						 //   if (isDiamondSelected)
						 //   {
							//    return;
						 //   }

							//redrawOnlyRoutes = true;
							if (Cache.IsGlobalRouteId)
                            {                                
                                Cache.IsGlobalRouteId = false;
                                Cache.IsTapOnSectorImage = true;
								isDiamondDrawnOnCanvas = false;
							}
                            else
                            {
                                Cache.IsTapOnSectorImage = false;
                            }

                            smallPopup.IsVisible = true;
                            sectorTopoDetailsViewModel.IsNavigatingToTopos = true;
                            GridWithId grid;
                            if (item is Label || item is BoxView)
                            {
                                grid = (item as View).Parent as GridWithId;
                            }
                            else
                            {
                                grid = item as GridWithId;
							}

							ShowRoute((int?)grid.PointId);
							CenterRoute(grid.X + grid.Width / 2, 0);
							listDiamondsRoute = listDiamondsRoute.Where(element => element.PointId == grid.PointId).ToList();
							Task.Run(() =>
							{
								sectorTopoDetailsViewModel.LoadRouteData(grid.PointId);
							});

							//                     var children = parentLayout.Children.ToList();
							//                     foreach (var toDisable in children)
							//                     {
							//                         var asGrid = toDisable as GridWithId;
							//                         if ((asGrid != null) && (asGrid.PointId != grid.PointId))
							//                         {
							//                             parentLayout.Children.Remove(asGrid);
							//                         }
							//                      }
						};
					   var Remove_gridWithId = parentLayout.Children.FirstOrDefault(
				                    item => item is GridWithId && (item as GridWithId).PointId == gridWithId.PointId
								   && (item as GridWithId).Points.X == Convert.ToDouble(topoimgTop.points[i].x, CultureInfo.InvariantCulture)
								   && (item as GridWithId).Points.Y == Convert.ToDouble(topoimgTop.points[i].y, CultureInfo.InvariantCulture))
				                  as GridWithId;
						if (Remove_gridWithId != null)
						{
							parentLayout.Children.Remove(Remove_gridWithId);
						}

					   parentLayout.Children.Add(gridWithId);
					    if (Device.RuntimePlatform == Device.iOS)
					    {
						    gridWithId.ChangeBounds((double) parent.ScaleFactor);
					    }

					    gridWithId.GestureRecognizers.Add(tapGesture);
					    foreach (var child in gridWithId.Children)
					    {
						    child.GestureRecognizers.Add(tapGesture);
					    }
					//}
					//else if (parent.IsScalingUp || parent.IsScalingDown)
					//{
					// SetupParentAndCoordinates(topoimgTop, ratio, i, out parentLayout, out x, out y);
					// var gridWithId = parentLayout.Children.FirstOrDefault(
					//   item => item is GridWithId && (item as GridWithId).PointId == id
					//                    && (item as GridWithId).Points.X == Convert.ToDouble(topoimgTop.points[i].x, CultureInfo.InvariantCulture)
					//                    && (item as GridWithId).Points.Y == Convert.ToDouble(topoimgTop.points[i].y, CultureInfo.InvariantCulture))
					//  as GridWithId;

					// if (gridWithId == null)
					// {
					//  continue;
					// }

					// gridWithId.Opacity = faded ? 0.5f : 1;
					// gridWithId.ChangeBounds((double) parent.ScaleFactor);
					// AbsoluteLayout.SetLayoutBounds(gridWithId,
					//  new Rectangle(x, y, GridBounds / parent.ScaleFactor, GridBounds / parent.ScaleFactor));
					// AbsoluteLayout.SetLayoutFlags(gridWithId, AbsoluteLayoutFlags.None);

					// var centerOnlyFirst = listDiamondsRoute.Count(item => item.PointId == Convert.ToInt32(id));
					// if (singleRoute && parent.ScaleFactor == 1 && centerOnlyFirst == 1 && !parent.IsScrolling)
					// {
					//  CenterRoute(gridWithId.X + gridWithId.Width / 2, 0);
					// }
					//}
				}
				else if (topoimgTop.points[i].type == "3")
			    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type3IsDarkUnCheckedAndroid
						    : ImageByte64Strings.Type3IsDarkUnCheckediOS;
					    int minusX = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 66 : 48 : 30;
					    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, minusX, 10, faded);
			    }
				else if (topoimgTop.points[i].type == "22")
				{
					strimg64 = Device.RuntimePlatform == Device.Android
						? ImageByte64Strings.Type3IsDarkCheckedAndroid
						: ImageByte64Strings.Type3IsDarkCheckediOS;
					//SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 22, 15);
					int minusX = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 66 : 48 : 30;
					SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, minusX, 10, faded);
				}
				else if (topoimgTop.points[i].type == "2")
			    {
				    strimg64 = Device.RuntimePlatform == Device.Android
					    ? ImageByte64Strings.Type2IsDarkCheckedAndroid
					    : ImageByte64Strings.Type2IsDarkCheckediOS;                    
                    var xpoint = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 50 : 32 : (float)device.Display.Scale > 2 ? 23 : 22;
                    var ypoint = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 40 : 22 : (float)device.Display.Scale > 2 ? 20 : 20;
                    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64,
					    xpoint, ypoint, faded);
			    }
			    else if (topoimgTop.points[i].type == "17")
			    {
				    if (topoimgTop.style.is_dark_checked)
				    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type17IsDarkCheckedAndroid
						    : ImageByte64Strings.Type17IsDarkCheckediOS;
					    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 22, 15, faded);
				    }
				    else
				    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type17IsDarUnkCheckedAndroid
						    : ImageByte64Strings.Type17IsDarUnkCheckediOS;
					    int minusX = Device.RuntimePlatform == Device.Android ? 40 : 22;
					    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, minusX, 15, faded);
				    }
			    }
			    else if (topoimgTop.points[i].type == "4")
			    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type4IsDarkUnCheckedAndroid
						    : ImageByte64Strings.Type4IsDarkUnCheckediOS;
                        var xpoint = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 20 : 13 : (float)device.Display.Scale > 2 ? 13 : 12;
                        SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, xpoint, 14, faded);					   
			    }
				else if (topoimgTop.points[i].type == "23")
				{
					strimg64 = Device.RuntimePlatform == Device.Android
							? ImageByte64Strings.Type4IsDarkCheckedAndroid
							: ImageByte64Strings.Type4IsDarkCheckediOS;
					//SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 10, 14);
					var xpoint = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 20 : 13 : (float)device.Display.Scale > 2 ? 13 : 12;
					SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, xpoint, 14, faded);
				}
				else if (topoimgTop.points[i].type == "18")
			    {
				    if (topoimgTop.style.is_dark_checked)
				    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type18IsDarkCheckedAndroid
						    : ImageByte64Strings.Type18IsDarkCheckediOS;
					    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 8, 14, faded);
				    }
				    else
				    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type18IsDarUnkCheckedAndroid
						    : ImageByte64Strings.Type18IsDarUnkCheckediOS;
					    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 8, 14, faded);
				    }
			    }
			    else if (topoimgTop.points[i].type == "8")
			    {
				    if (topoimgTop.style.is_dark_checked)
				    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type8IsDarkCheckedAndroid
						    : ImageByte64Strings.Type8IsDarkCheckediOS;
					    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 14, 14, faded);
				    }
				    else
				    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type8IsDarkUnCheckedAndroid
						    : ImageByte64Strings.Type8IsDarkUnCheckediOS;
					    if (Device.RuntimePlatform == Device.Android)
					    {
						    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, (float)device.Display.Scale > 3 ? 46 : 32, 30, faded);
					    }
					    else
					    {
                            var xpoint = (float)device.Display.Scale > 2 ? 22 : 20;
                            SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, xpoint, 20, faded);
					    }
				    }
			    }
			    else if (topoimgTop.points[i].type == "16")
			    {
				    if (topoimgTop.style.is_dark_checked)
				    {
					    //16
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type16IsDarkCheckedAndroid
						    : ImageByte64Strings.Type16IsDarkCheckediOS;
					    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 15, 15, faded);
				    }
				    else
				    {
					    strimg64 = Device.RuntimePlatform == Device.Android
						    ? ImageByte64Strings.Type16IsDarkUnckeckedAndroid
						    : ImageByte64Strings.Type16IsDarkUncheckediOS;
					    if (Device.RuntimePlatform == Device.Android)
					    {
						    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 28, 28, faded);
					    }
					    else
					    {
						    SetupAnnotationIcon(topoimgTop, _skCanvas, ratio, i, strimg64, 15, 15, faded);
					    }
				    }
			    }
			    else if (topoimgTop.points[i].type == "5") //left side
			    {
                    if (Cache.IsGlobalRouteId == false)
                    {
                        if (Device.RuntimePlatform == Device.Android)
                        {
                            var xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 165 : 210 : 170;
                            var yval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 20 : 50 : 40;
                            var width = ((float)device.Display.Scale > 3) ? 150 : 120;
                            var height = ((float)device.Display.Scale > 3) ? 100 : 80;

                            // draw these at specific locations                       
                            var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) - xval,
                                ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);
                            using (var paint = new SKPaint())
                            {
                                if (faded)
                                    paint.Color = paint.Color.WithAlpha(128);
                                _skCanvas.Save();
                                _skCanvas.DrawRect(Rect, new SKPaint() { Color = SKColors.Black.WithAlpha(170) });
                            }
                        }
                        else
                        {
                            var xval = ((float)device.Display.Scale > 2) ? 180 : 125;
                            var yval = ((float)device.Display.Scale > 2) ? 50 : 45;
                            var width = ((float)device.Display.Scale > 2) ? 140 : 100;
                            var height = ((float)device.Display.Scale > 2) ? 100 : 60;

                            var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) - xval,
                                ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);
                            using (var paint = new SKPaint())
                            {
                                if (faded)
                                    paint.Color = paint.Color.WithAlpha(128);
                                _skCanvas.Save();
                                _skCanvas.DrawRect(Rect, new SKPaint() { Color = SKColors.Black.WithAlpha(170) });
                            }
                        }

                        using (var paint = new SKPaint())
                        {
                            if (Device.RuntimePlatform == Device.Android)
                            {
                                paint.TextSize = ((float)device.Display.Scale > 3) ? 50.0f : 35.0f;
                            }
                            else
                            {
                                paint.TextSize = ((float)device.Display.Scale > 2) ? 45.0f : 30.0f;
                            }
                            paint.IsAntialias = true;
                            paint.Color = SKColors.White;
                            paint.TextAlign = SKTextAlign.Center;
                            if (faded)
                                paint.Color = paint.Color.WithAlpha(128);
                            if (Device.RuntimePlatform == Device.Android)
                            {
                                var xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 90 : 140 : 110;
                                var yval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 42 : 22 : 10;

                                _skCanvas.DrawText((topoimgTop.points[i].label).ToString(),
                                    (float.Parse(topoimgTop.points[i].x) * ratio) - xval,
                                    (float.Parse(topoimgTop.points[i].y) * ratio) + yval, paint);
                            }
                            else
                            {
                                var xval = ((float)device.Display.Scale > 2) ? 103 : 70;
                                var yval = ((float)device.Display.Scale > 2) ? 15 : -3;
                                _skCanvas.DrawText((topoimgTop.points[i].label).ToString(),
                                    (float.Parse(topoimgTop.points[i].x) * ratio) - xval,
                                    (float.Parse(topoimgTop.points[i].y) * ratio) + yval, paint);
                            }
                        }
                    }
			    }
			    else if (topoimgTop.points[i].type == "6") //right
			    {
                    if (Cache.IsGlobalRouteId == false)
                    {
                        if (Device.RuntimePlatform == Device.Android)
                        {
                            var xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 90 : 60 : 45;
                            var yval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 10 : 40 : 40;
                            var width = ((float)device.Display.Scale > 3) ? 150 : 120;
                            var height = ((float)device.Display.Scale > 3) ? 100 : 80;

                            // draw these at specific locations                       
                            var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) + xval,
                                ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);

                            using (var paint = new SKPaint())
                            {
                                if (faded)
                                    paint.Color = paint.Color.WithAlpha(128);
                                _skCanvas.Save();
                                _skCanvas.DrawRect(Rect, new SKPaint() { Color = SKColors.Black.WithAlpha(170) });
                            }
                        }
                        else
                        {
                            var xval = ((float)device.Display.Scale > 2) ? 45 : 25;
                            var yval = ((float)device.Display.Scale > 2) ? 30 : 25;
                            var width = ((float)device.Display.Scale > 2) ? 140 : 100;
                            var height = ((float)device.Display.Scale > 2) ? 100 : 60;
                            // draw these at specific locations                       
                            var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) + xval,
                                ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);

                            using (var paint = new SKPaint())
                            {
                                if (faded)
                                    paint.Color = paint.Color.WithAlpha(128);
                                _skCanvas.Save();
                                _skCanvas.DrawRect(Rect, new SKPaint() { Color = SKColors.Black.WithAlpha(170) });
                            }
                        }
                        using (var paint = new SKPaint())
                        {
                            if (Device.RuntimePlatform == Device.Android)
                            {
                                paint.TextSize = ((float)device.Display.Scale > 3) ? 50.0f : 35.0f;
                            }
                            else
                            {
                                paint.TextSize = ((float)device.Display.Scale > 2) ? 45.0f : 30.0f;
                            }
                            paint.IsAntialias = true;
                            paint.Color = SKColors.White;
                            paint.TextAlign = SKTextAlign.Center;
                            if (faded)
                                paint.Color = paint.Color.WithAlpha(128);
                            if (Device.RuntimePlatform == Device.Android)
                            {
                                var xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 160 : 135 : 110;
                                var yval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 52 : 27 : 12;

                                _skCanvas.DrawText((topoimgTop.points[i].label).ToString(),
                                    (float.Parse(topoimgTop.points[i].x) * ratio) + xval,
                                    (float.Parse(topoimgTop.points[i].y) * ratio) + yval, paint);
                            }
                            else
                            {
                                var xval = ((float)device.Display.Scale > 2) ? 115 : 75;
                                var yval = ((float)device.Display.Scale > 2) ? 35 : 15;

                                _skCanvas.DrawText((topoimgTop.points[i].label).ToString(),
                                    (float.Parse(topoimgTop.points[i].x) * ratio) + xval,
                                    (float.Parse(topoimgTop.points[i].y) * ratio) + yval, paint);
                            }
                        }
                    }
			    }
			    if (Cache.IsGlobalRouteId == false)
			    {
				    //code to show left and right text
				    if (topoimgTop.pointsText != null)
				    {
					    for (int j = 0; j < topoimgTop.pointsText.Count; j++)
					    {
						    if (_routeId > 0)
						    {
							    //left side text                
							    if (topoimgTop.pointsText[j].point_id.Split('p')[1] == i.ToString())
                                {
								    if (topoimgTop.pointsText[j].text_id.IndexOf("L") > -1)
								    {
									    if (topoimgTop.pointsText[j].text_id.Split('L')[1] == i.ToString())
                                        {
										    if (Device.RuntimePlatform == Device.Android)
										    {
                                                var xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 340 : 370 : 300;
                                                var yval = ((float)device.Display.Scale > 3) ? 50 : 20;
                                                var width = ((float)device.Display.Scale > 3) ? 310 : 250;
                                                var height = ((float)device.Display.Scale > 3) ? 105 : 80;

                                                // draw these at specific locations                       
                                                var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) - xval,
												    ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);

											    using (var paint = new SKPaint())
											    {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    _skCanvas.Save();
												    _skCanvas.DrawRect(Rect, new SKPaint() {Color = SKColors.Black.WithAlpha(170)});
											    }
											    using (var paint = new SKPaint())
											    {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    paint.TextSize = ((float)device.Display.Scale > 3) ? 50.0f : 40.0f;
												    paint.IsAntialias = true;
												    paint.Color = SKColors.White;												    
												    paint.TextAlign = SKTextAlign.Center;

                                                    var _xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 185 : 205 : 170;
                                                    var _yval = ((float)device.Display.Scale > 3) ? 20 : 35;

                                                    _skCanvas.DrawText((topoimgTop.pointsText[j].text_value).ToString(),
													    (float.Parse(topoimgTop.points[i].x) * ratio) - _xval,
													    (float.Parse(topoimgTop.points[i].y) * ratio) + _yval, paint);
											    }
										    }
										    else
										    {
                                                var xval = ((float)device.Display.Scale > 2) ? 300 : 185;
                                                var yval = ((float)device.Display.Scale > 2) ? 65 : 40;
                                                var width = ((float)device.Display.Scale > 2) ? 260 : 160;
                                                var height = ((float)device.Display.Scale > 2) ? 110 : 60;
                                                // draw these at specific locations                       
                                                var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) - xval,
                                                    ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);

                                                using (var paint = new SKPaint())
											    {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    _skCanvas.Save();
												    _skCanvas.DrawRect(Rect, new SKPaint() {Color = SKColors.Black.WithAlpha(170) });
											    }
											    using (var paint = new SKPaint())
											    {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    paint.TextSize = ((float)device.Display.Scale > 2) ? 50.0f : 30.0f;
												    paint.IsAntialias = true;
												    paint.Color = SKColors.White;												    
												    paint.TextAlign = SKTextAlign.Center;

                                                    var _xval = ((float)device.Display.Scale > 2) ? 170 : 100;
                                                    var _yval = ((float)device.Display.Scale > 2) ? 8 : 0;

                                                    _skCanvas.DrawText((topoimgTop.pointsText[j].text_value).ToString(),
                                                        (float.Parse(topoimgTop.points[i].x) * ratio) - _xval, (float.Parse(topoimgTop.points[i].y) * ratio) + _yval,
                                                        paint);
                                                }
										    }
									    }
								    }
							    }

							    //right side text                
							    if (topoimgTop.pointsText[j].point_id.Split('p')[1] == i.ToString())
							    {
								    if (topoimgTop.pointsText[j].text_id.IndexOf("R") > -1)
								    {
									    if (topoimgTop.pointsText[j].text_id.Split('R')[1] == i.ToString())
									    {
										    if (Device.RuntimePlatform == Device.Android)
										    {
                                                var xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 100 : 60 : 50;
                                                var yval = ((float)device.Display.Scale > 3) ? 50 : 40;
                                                var width = ((float)device.Display.Scale > 3) ? 330 : 270;
                                                var height = ((float)device.Display.Scale > 3) ? 105 : 80;

                                                // draw these at specific locations                       
                                                var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) + xval,
												    ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);

											    using (var paint = new SKPaint())
											    {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    _skCanvas.Save();
												    _skCanvas.DrawRect(Rect, new SKPaint() {Color = SKColors.Black.WithAlpha(170) });
											    }
											    using (var paint = new SKPaint())
											    {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    paint.TextSize = ((float)device.Display.Scale > 3) ? 50.0f : 40.0f;
												    paint.IsAntialias = true;
												    paint.Color = SKColors.White;												    
												    paint.TextAlign = SKTextAlign.Center;

                                                    var _xval = ((float)device.Display.Scale > 3) ? ((int)device.Display.Height < 2400 && topoImg.image.width == "750") ? 235 : 225 : 180;
                                                    var _yval = ((float)device.Display.Scale > 3) ? 20 : 15;

                                                    _skCanvas.DrawText((topoimgTop.pointsText[j].text_value).ToString(),
													    (float.Parse(topoimgTop.points[i].x) * ratio) + _xval,
													    (float.Parse(topoimgTop.points[i].y) * ratio) + _yval, paint);
											    }
										    }
										    else
										    {
                                                var xval = ((float)device.Display.Scale > 2) ? 45 : 25;
                                                var yval = ((float)device.Display.Scale > 2) ? 40 : 20;
                                                var width = ((float)device.Display.Scale > 2) ? 265 : 170;
                                                var height = ((float)device.Display.Scale > 2) ? 110 : 60;
                                                // draw these at specific locations                       
                                                var Rect = SKRect.Create(((float.Parse(topoimgTop.points[i].x)) * ratio) + xval,
                                                    ((float.Parse(topoimgTop.points[i].y)) * ratio) - yval, width, height);

                                                using (var paint = new SKPaint())
                                                {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    _skCanvas.Save();
                                                    _skCanvas.DrawRect(Rect, new SKPaint() { Color = SKColors.Black.WithAlpha(170) });
                                                }
                                                using (var paint = new SKPaint())
                                                {
                                                    if (faded)
                                                        paint.Color = paint.Color.WithAlpha(128);
                                                    var _xval = ((float)device.Display.Scale > 2) ? 175 : 115;
                                                    var _yval = ((float)device.Display.Scale > 2) ? 30 : 20;

                                                    paint.TextSize = ((float)device.Display.Scale > 2) ? 50.0f : 30.0f;
                                                    paint.IsAntialias = true;
                                                    paint.Color = SKColors.White;                                                   
                                                    paint.TextAlign = SKTextAlign.Center;
                                                    _skCanvas.DrawText((topoimgTop.pointsText[j].text_value).ToString(), (float.Parse(topoimgTop.points[i].x) * ratio) + _xval, (float.Parse(topoimgTop.points[i].y) * ratio) + _yval, paint);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //showing arrow logic   
                var arrowlist = topoimgTop.pointsText.Where(x => x.isdirection.ToLower() == "true").ToList();
                foreach (var item in arrowlist)
                {
                    if (item.point_id.Split('p')[1] == i.ToString())
                    {
                        SetupArrow(topoimgTop, _skCanvas, i, faded);
                    }
                }
            }
        }

		private void SetupRectangular(TopoLineModel topoimgTop, SKCanvas skCanvas, int i, string color)
		{
			var path = new SKPath();
			var fillPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SKColors.White
			};

			var incrementFirstValue = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 45f : 35f : ((float)device.Display.Scale > 2) ? 40f : 22f;
			//if (ascentSummaryViewModel.ShareSelected)
			//{
			incrementFirstValue = incrementFirstValue * drawnHeightRatio;
			//}

			path.MoveTo(float.Parse(topoimgTop.points[i].x) * xRatio - incrementFirstValue + xOffset, (float.Parse(topoimgTop.points[i].y) * yRatio));
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio - incrementFirstValue);
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio + incrementFirstValue);

			skCanvas.DrawPath(path, fillPaint);

			path = new SKPath();
			path.MoveTo(float.Parse(topoimgTop.points[i].x) * xRatio + incrementFirstValue + xOffset, (float.Parse(topoimgTop.points[i].y) * yRatio));
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio + incrementFirstValue);
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio - incrementFirstValue);
			skCanvas.DrawPath(path, fillPaint);

			fillPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SKColor.Parse(color)
			};

			path = new SKPath();
			var incrementSecondCubeRatio = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 40f : 30f : ((float)device.Display.Scale > 2) ? 35f : 20f;
			//if (ascentSummaryViewModel.ShareSelected)
			//{
			incrementSecondCubeRatio = incrementSecondCubeRatio * drawnHeightRatio;
			//}

			path.MoveTo(float.Parse(topoimgTop.points[i].x) * xRatio - incrementSecondCubeRatio + xOffset, (float.Parse(topoimgTop.points[i].y) * yRatio));
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio - incrementSecondCubeRatio);
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio + incrementSecondCubeRatio);

			skCanvas.DrawPath(path, fillPaint);

			path = new SKPath();
			path.MoveTo(float.Parse(topoimgTop.points[i].x) * xRatio + incrementSecondCubeRatio + xOffset, (float.Parse(topoimgTop.points[i].y) * yRatio));
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio + incrementSecondCubeRatio);
			path.LineTo(
				float.Parse(topoimgTop.points[i].x) * xRatio + xOffset,
				float.Parse(topoimgTop.points[i].y) * yRatio - incrementSecondCubeRatio);
			skCanvas.DrawPath(path, fillPaint);
		}

		private void SetupArrow(TopoLineModel topoimgTop, SKCanvas skCanvas, int i, bool faded)
        {
            var path = new SKPath();
            var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse(topoimgTop.marker.fillColor)
            };
            if (faded)
                fillPaint.Color = fillPaint.Color.WithAlpha(128);
			var leftRightdist = Device.RuntimePlatform == Device.Android ? ((float)device.Display.Scale > 3) ? 30 : 25 : ((float)device.Display.Scale > 2) ? 25 : 15;
			path = new SKPath();
			path.MoveTo(float.Parse(topoimgTop.points[i].x) * xRatio, (float.Parse(topoimgTop.points[i].y) * yRatio + leftRightdist + 5));
			path.LineTo(float.Parse(topoimgTop.points[i].x) * xRatio - leftRightdist, float.Parse(topoimgTop.points[i].y) * yRatio - 15);
			path.LineTo(float.Parse(topoimgTop.points[i].x) * xRatio + leftRightdist, float.Parse(topoimgTop.points[i].y) * yRatio - 15);
			path.LineTo(float.Parse(topoimgTop.points[i].x) * xRatio, float.Parse(topoimgTop.points[i].y) * yRatio + leftRightdist + 5);

			//calculate angle 
			float pointX = 0, pointY = 0;
			var _fpt = new Xamarin.Forms.Point();
			var _spt = new Xamarin.Forms.Point();
			bool isLastPointArrow = (i + 1) < topoimgTop.points.Count ? false : true;
			_fpt.X = (i + 1) < topoimgTop.points.Count ? Convert.ToInt32(topoimgTop.points[i + 1].x) : Convert.ToInt32(topoimgTop.points[i - 1].x);
			_fpt.Y = (i + 1) < topoimgTop.points.Count ? Convert.ToInt32(topoimgTop.points[i + 1].y) : Convert.ToInt32(topoimgTop.points[i - 1].y);
			_spt.X = Convert.ToInt32(topoimgTop.points[i].x);
			_spt.Y = Convert.ToInt32(topoimgTop.points[i].y);
			double angleDeg = 0;
			if (_fpt.Y < _spt.Y)
			{
				angleDeg = Math.Round((Math.Atan2(_fpt.Y - _spt.Y, _fpt.X - _spt.X) * 180 / Math.PI) - 88);
				pointX = (float.Parse(topoimgTop.points[i].x) * xRatio);
				pointY = float.Parse(topoimgTop.points[i].y) * yRatio;
			}
			else
			{
				angleDeg = isLastPointArrow ? Math.Round((Math.Atan2(_spt.Y - _fpt.Y, _spt.X - _fpt.X) * 180 / Math.PI) + 268) : Math.Round((Math.Atan2(_spt.Y - _fpt.Y, _spt.X - _fpt.X) * 180 / Math.PI) + 88);
				pointX = float.Parse(topoimgTop.points[i].x) * xRatio;
				pointY = float.Parse(topoimgTop.points[i].y) * yRatio;
			}

			skCanvas.Save();
			skCanvas.RotateDegrees((float)angleDeg, pointX, pointY);
			skCanvas.DrawPath(path, fillPaint);
			skCanvas.Restore();
		}

        private void SetupAnnotationIcon(TopoLineModel topoimgTop, SKCanvas _skCanvas, float ratio, int i, string strimg64, int minusX, int minusY, bool faded)
        {
            var imageBytes = Convert.FromBase64String(strimg64);
            Stream fileStream = new MemoryStream(imageBytes);

            var sizeOfAnnotation = Device.RuntimePlatform == Device.Android ? (float)device.Display.Scale > 3 ? 90f : 65f : ((float)device.Display.Scale > 2) ? 70f : 40f;            

            // decode the bitmap from the stream			
            using (var stream = new SKManagedStream(fileStream))
            using (var bitmap = SKBitmap.Decode(stream))
            using (var paint = new SKPaint())
            {
                if (faded)
                    paint.Color = paint.Color.WithAlpha(128);               
                _skCanvas.DrawBitmap(
                      bitmap,
                    SKRect.Create(
                    Device.RuntimePlatform == Device.Android ? (float.Parse(topoimgTop.points[i].x) * xRatio) - minusX : ((float)device.Display.Scale > 2) ? (float.Parse(topoimgTop.points[i].x) * xRatio) - (minusX * 2) + ((float)device.Display.Scale * 3) : (float.Parse(topoimgTop.points[i].x) * xRatio) - minusX,
                    Device.RuntimePlatform == Device.Android ? (float.Parse(topoimgTop.points[i].y) * yRatio) - minusY : ((float)device.Display.Scale > 2) ? (float.Parse(topoimgTop.points[i].y) * yRatio) - (minusY * 2) : (float.Parse(topoimgTop.points[i].y) * yRatio) - minusY,
                    sizeOfAnnotation,
                    sizeOfAnnotation),
                    paint);
            }
        }

		private void CenterRoute(double X, double Y)
		{
			if (Device.RuntimePlatform == Device.Android)
			{
				var xPoint = X * parent.ScaleFactor - device.Display.Width / device.Display.Scale / 2;
                androidZoomScroll.ScrollToAsync(xPoint, Y, false);
            }
			else
			{
				if (parent.ScaleFactor != 1)
				{
					return;
				}

                var xPoint = X - device.Display.Width / device.Display.Scale / 2;

                if (xPoint < 0)
                {
                    Device.BeginInvokeOnMainThread(() => iOSZoomScroll.ScrollToAsync(0, Y, false));
                    return;
                }

                if (X > (iOSZoomScroll.ContentSize.Width - device.Display.Width / device.Display.Scale / 2))
                {
                    var pointToMove = iOSZoomScroll.ContentSize.Width - device.Display.Width / device.Display.Scale;
                    Device.BeginInvokeOnMainThread(() => iOSZoomScroll.ScrollToAsync(pointToMove, Y, false));
                    return;
                }

                Device.BeginInvokeOnMainThread(() => iOSZoomScroll.ScrollToAsync(xPoint, 0, false));
            }
        }

        private void SetupParentAndCoordinates(TopoLineModel topoimgTop, float ratio, int i, out AbsoluteLayout parentLayout, out double x, out double y)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                parentLayout = AndroidAbsoluteLayout;
            }
            else
            {
                parentLayout = skCanvasiOS.Parent as AbsoluteLayout;
            }

            x = float.Parse(topoimgTop.points[i].x) / device.Display.Scale * xRatio  - GridBounds / 2 / parent.ScaleFactor;
            y = float.Parse(topoimgTop.points[i].y) / device.Display.Scale * yRatio - GridBounds / 2 / parent.ScaleFactor;
        }

        public static SKColor HexToColor(string color)
        {
            if (color.StartsWith("#"))
                color = color.Remove(0, 1);

            byte r = 0, g = 0, b = 0;
            if (color.Length == 3)
            {
                r = Convert.ToByte(color[0] + "" + color[0], 16);
                g = Convert.ToByte(color[1] + "" + color[1], 16);
                b = Convert.ToByte(color[2] + "" + color[2], 16);
            }
            else if (color.Length == 6)
            {
                r = Convert.ToByte(color[0] + "" + color[1], 16);
                g = Convert.ToByte(color[2] + "" + color[3], 16);
                b = Convert.ToByte(color[4] + "" + color[5], 16);
            }
            else
            {
                //throw new ArgumentException("Hex color " + color + " is invalid.");
            }
            return new SKColor(r, g, b);
        }

        public string GetGradeBucketHex(string grade_bucket_id)
        {
            string color = string.Empty;
            switch (grade_bucket_id)
            {
                case "1":
                    return color = "#036177";// new SKColor(3, 97, 119);
                case "2":
                    return color = "#1f8a70";//new SKColor(31, 138, 112);
                case "3":
                    return color = "#91a537"; //new SKColor(145, 165, 55);
                case "4":
                    return color = "#b49800";//new SKColor(180, 152, 0);
                case "5":
                    return color = "#fd7400";//new SKColor(253, 116, 0);
                default:
                    return color = "#cccccc";//new SKColor(204, 204, 204);
            }
        }

        private void OnInvalidateSurface()
        {
			isDiamondDrawnOnCanvas = false;
			isDiamondSelected = false;
            isDiamondSingleClick = false;
            if (Device.RuntimePlatform == Device.iOS)
            {
                skCanvasiOS.InvalidateSurface();
            }
            else
            {
                skCanvasAndroid.InvalidateSurface();
            }
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            try
            {
                if (parameters.Count < 2)
                {
                    if (!parameters.TryGetValue(NavigationParametersConstants.IsUnlockedParameter, out bool key) || !key)
                    {
                        return;
                    }

                    if (Device.RuntimePlatform == Device.Android)
                    {
                        skCanvasAndroid.IsVisible = true;
                    }
                    else
                    {
                        skCanvasiOS.IsVisible = true;
                    }

                    isUnlocked = true;
                    InitializeHeights();
                    return;
                }


                parameters.TryGetValue(NavigationParametersConstants.SelectedSectorObjectParameter, out currentSector);
                parameters.TryGetValue(NavigationParametersConstants.RouteIdParameter, out routeId);
                parameters.TryGetValue(NavigationParametersConstants.SingleRouteParameter, out singleRoute);
                parameters.TryGetValue(NavigationParametersConstants.PageIndexParameter, out pageIndex);
                parameters.TryGetValue(NavigationParametersConstants.TopoImageResponseParameter, out topoImg);
                parameters.TryGetValue(NavigationParametersConstants.IsUnlockedParameter, out isUnlocked);
                parameters.TryGetValue(NavigationParametersConstants.ChildrenCountParameter, out childrenCount);
                parameters.TryGetValue(NavigationParametersConstants.IsNavigatedFromSectorImageParameter, out isNavigatedFromSectorImage);
				sectorTopoDetailsViewModel = (BindingContext as SectorTopoDetailsViewModel);
                await SetAllData(currentSector, topoImg, routeId ?? 0, pageIndex, singleRoute, false);
                OnAppearing();
                userDialogs.HideLoading();
            }
            catch (Exception exception)
            {
                var serializedTopoImageResponse =
                    Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.TopoImageResponseParameter]);
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatedTo),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $@"singleRoute = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["singleRoute"])},
							pageIndex = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["pageIndex"])},
							topoImageResponse = {serializedTopoImageResponse},
							childrenCount = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["childrenCount"])},
							routeId = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["routeId"])},"
                });
            }
        }

		public async void OnNavigatingTo(NavigationParameters parameters)
		{
			try
			{
				if (parameters.Count < 1)
				{
					return;
				}

				if (Device.RuntimePlatform == Device.Android)
				{
					androidZoomScroll.IsScalingDown = true;
					androidZoomScroll.IsScalingUp = true;
				}
			}
			catch (Exception exception)
			{
				var serializedTopoImageResponse =
					Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.TopoImageResponseParameter]);
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatingTo),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data = $@"singleRoute = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["singleRoute"])},
							pageIndex = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["pageIndex"])},
							topoImageResponse = {serializedTopoImageResponse},
							childrenCount = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["childrenCount"])},
							routeId = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters["routeId"])},"
				});
			}
        }

		private void SKCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			using (var localPaint = new SKPaint())			  
			{
				localPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 70f);
				localPaint.Color = SKColor.Parse("#99000000");
				localPaint.ImageFilter = SKImageFilter.CreateBlur(70f, 70f);
				canvas.DrawRect(
					SKRect.Create((float)(e.Info.Size.Width),
						(float)(e.Info.Size.Height)), localPaint);
			}

		}

		private async void ZoomIn(object sender, EventArgs e)
        {
            scaleOut = 1;
            scaleIn += 0.05;
            var heightLocal = AndroidAbsoluteLayout.Height * scaleIn;
            var widthLocal = AndroidAbsoluteLayout.Width * scaleIn;
            if (heightLocal > androidZoomScroll.InitialHeight * 1.95 || (Device.RuntimePlatform == Device.Android && skCanvasAndroid.CanvasSize.Height > 3500))
            {
                return;
            }

            if(imageBytes?.Count() > 450000 || numberOfZomms > 2)
            {
                return;
            }

            numberOfZomms++;
            AndroidAbsoluteLayout.HeightRequest = heightLocal;
            AndroidAbsoluteLayout.WidthRequest = widthLocal;

            await androidZoomScroll.ScrollToAsync(androidZoomScroll.ScrollX * scaleIn, androidZoomScroll.ScrollY * scaleIn, false).ConfigureAwait(false);
        }

        private async void ZoomOut(object sender, EventArgs e)
        {
            scaleIn = 1;
            scaleOut -= 0.05;
            var heightLocal = AndroidAbsoluteLayout.Height * scaleOut;
            var widthLocal = AndroidAbsoluteLayout.Width * scaleOut;
            if (heightLocal < androidZoomScroll.InitialHeight)
            {
                await AndroidAbsoluteLayout.LayoutTo(new Rectangle(AndroidAbsoluteLayout.X, AndroidAbsoluteLayout.Y, androidZoomScroll.InitialWidth, androidZoomScroll.InitialHeight));
                AndroidAbsoluteLayout.HeightRequest = androidZoomScroll.InitialHeight;
                AndroidAbsoluteLayout.WidthRequest = androidZoomScroll.InitialWidth;
                await androidZoomScroll.ScrollToAsync(androidZoomScroll.ScrollX * scaleOut, androidZoomScroll.ScrollY * scaleOut, false);
                androidZoomScroll.ForceLayout();
                skCanvasAndroid.InvalidateSurface();
                return;
            }

            numberOfZomms--;
            await AndroidAbsoluteLayout.LayoutTo(new Rectangle(AndroidAbsoluteLayout.X, AndroidAbsoluteLayout.Y, widthLocal, heightLocal));
            AndroidAbsoluteLayout.HeightRequest = heightLocal;
            AndroidAbsoluteLayout.WidthRequest = widthLocal;
            await androidZoomScroll.ScrollToAsync(androidZoomScroll.ScrollX * scaleOut, androidZoomScroll.ScrollY * scaleOut, false);
            androidZoomScroll.ForceLayout();
            skCanvasAndroid.InvalidateSurface();
        }
    }
}
