using System;
using System.Collections.Generic;
using Android.Content;
using Android.Widget;
using Xamarin.Forms;
using SloperMobile.Droid.Renderers;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Xamarin.Forms.GoogleMaps.Android;
using Xamarin.Forms.GoogleMaps;
using System.ComponentModel;
using Xamarin.Forms.Platform.Android;
using SloperMobile.UserControls.CustomControls;
using System.Linq;
using Android.Views;
using SloperMobile.DataBase.DataTables;

[assembly: ExportRenderer(typeof(ExtendedMap), typeof(CustomMapRenderer))]
namespace SloperMobile.Droid.Renderers
{
    public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter, GoogleMap.IOnMarkerClickListener
    {
        bool isLoaded;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName.Equals("VisibleRegion") && !isLoaded)
            {
                isLoaded = true;
                NativeMap.SetInfoWindowAdapter(this);
                NativeMap.SetOnMarkerClickListener(this);
                //NativeMap.MarkerClick += (s, ea) => OnMarkerClick(ea.Marker);
            }
        }

        public Android.Views.View GetInfoContents(Marker marker)
        {
            var pin = GetFormsPin(marker);
            if (pin.Tag is CragExtended crag)
            {
                var cragInfoWindow = new CragInfoWindow();
                return cragInfoWindow.GetView(pin.Tag as CragExtended); ;
            }
            if (pin.Tag is SectorMapModel sector)
            {
                var sectorInfoWindow = new SectorInfoWindow();
                return sectorInfoWindow.GetView(sector); ;
            }
            return null;
        }

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }

        Pin GetFormsPin(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, annotation.Position.Longitude);
            return ((ExtendedMap)Element).Pins.FirstOrDefault(p => p.Position == position);
        }

        public bool OnMarkerClick(Marker marker)
        {
            try
            {
                var markerLatLong = marker.Position;
                var screenLocation = NativeMap.Projection.ToScreenLocation(markerLatLong);

                screenLocation.Y -= (int)(Map.Height / 2);

                var offsetTarget = NativeMap.Projection.FromScreenLocation(screenLocation);

                marker.ShowInfoWindow();
                NativeMap.AnimateCamera(Android.Gms.Maps.CameraUpdateFactory.NewLatLng(offsetTarget), 250, null);

                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }
   }
}