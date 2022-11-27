using CoreGraphics;
using Foundation;
using Google.Maps;
using MapKit;
using SloperMobile.iOS.Renderers;
using SloperMobile.UserControls.CustomControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.iOS;
using Xamarin.Forms.Platform.iOS;
using SloperMobile.DataBase.DataTables;
using System.Linq;
using SloperMobile.Model;

[assembly: ExportRenderer(typeof(ExtendedMap), typeof(CustomMapRenderer))]
namespace SloperMobile.iOS.Renderers
{
    public class CustomMapRenderer : MapRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                NativeMap.MarkerInfoWindow = markerInfoWindow;
                NativeMap.TappedMarker += TapperMarkerFunct;
            }
        }

        bool TapperMarkerFunct(MapView map, Marker marker)
        {
            var markerLatLong = marker.Position;
            var screenLocation = NativeMap.Projection.PointForCoordinate(markerLatLong);

            screenLocation.Y -= (int)(Map.Height / 4 );

            var offsetTarget = NativeMap.Projection.CoordinateForPoint(screenLocation);

            map.SelectedMarker = marker;

            TimeSpan ts = new TimeSpan(0,0,0,0,250);

            Map.AnimateCamera(CameraUpdateFactory.NewPosition(new Position(offsetTarget.Latitude, offsetTarget.Longitude)),ts);

            return true;
        }

        UIView markerInfoWindow(UIView view, Marker marker)
        {
            var pin = GetFormsPin(marker);
            if (pin.Tag is CragExtended crag)
            {
                var mapCallout = MapCallout.Create();
                mapCallout.InitView(pin.Tag as CragExtended);
                return mapCallout;
            }
            if (pin.Tag is SectorMapModel sector) {
                var sectorCallout = SectorCallout.Create();
                sectorCallout.InitView(sector);
                return sectorCallout;
            }
            return null;
        }

        Pin GetFormsPin(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, annotation.Position.Longitude);
            return ((Map)Element).Pins.FirstOrDefault(p => p.Position == position);
        }
    }
}
