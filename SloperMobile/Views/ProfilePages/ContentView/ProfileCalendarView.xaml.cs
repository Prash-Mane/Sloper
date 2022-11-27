using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Prism.Mvvm;
using SloperMobile.ViewModel.ProfileViewModels;
using Syncfusion.SfCalendar.XForms;
using Xamarin.Forms;
using SelectionChangedEventArgs = Syncfusion.SfCalendar.XForms.SelectionChangedEventArgs;

namespace SloperMobile
{
    public partial class ProfileCalendarView : ContentView
    {
        private const string OtherMonthNoSendColor = "#333333";
        private const string OtherMonthHasSendColor = "#803C00";
        private const string CurrentMonthHasSendColor = "#FF8E2D";

        private DateTime month;

        public ProfileCalendarView()
        {
            InitializeComponent();

            //BindingContext = new ProfileCalendarViewModel(this.Navigation);

            CalendarHeader.Text = DateTime.Now.ToString("Y").Replace(",", "").ToUpper();

            SetupMonthViewSettings();
           
            month = DateTime.Now;
        }


        private void SetupMonthViewSettings()
        {
            var monthViewSettings = new MonthViewSettings
            {
                BorderColor = Color.FromHex("#191919"),
                CurrentMonthBackgroundColor = Color.Transparent,
                CurrentMonthTextColor = Color.White,
                PreviousMonthBackgroundColor = Color.Transparent,
                PreviousMonthTextColor = Color.FromHex(OtherMonthNoSendColor),
                DayHeaderBackgroundColor = Color.Transparent,
                DayHeaderTextColor = Color.White,
                DateSelectionColor = Color.Black,
                TodayTextColor = Color.FromHex(CurrentMonthHasSendColor),
                SelectedDayTextColor = Color.FromHex(CurrentMonthHasSendColor),
                DayLabelTextAlignment = DayLabelTextAlignment.Center,
            };

            calendar.NullableSelectedDate = null;
            calendar.MonthViewSettings = monthViewSettings;
        }

        private void OnMonthChanged(object sender, MonthChangedEventArgs args)
        {
            CalendarHeader.Text = args.args.CurrentValue.ToString("Y").Replace(",", "").ToUpper();
            month = args.args.CurrentValue;
        }

        private void OnCalendarTapped(object sender, CalendarTappedEventArgs args)
        {
            CalendarEventCollection events = args.Calendar.DataSource;
            if (events.Any(e => args.datetime.ToString("yyyy/MM/dd").Equals(e.StartTime.ToString("yyyy/MM/dd"))))
                (BindingContext as ProfileCalendarViewModel).GoToPointsDate(args.datetime);
        }

        private void OnCalendarSelectionChanged(object sender, SelectionChangedEventArgs args)
        {

            if (month.Month == args.Calendar.SelectedDate.Month)
            {
                args.Calendar.MonthViewSettings.CurrentMonthBackgroundColor = Color.Transparent;

                if (args.Calendar.SelectedDate.Date == DateTime.Now.Date)
                {
                    //Setting another day to make not today day selected
                    args.Calendar.MonthViewSettings.SelectedDayTextColor = Color.FromHex(CurrentMonthHasSendColor);
                    args.Calendar.MonthViewSettings.TodayTextColor = Color.Transparent;
                }
                else
                {
                    //Setting white text color to days of current month
                    args.Calendar.MonthViewSettings.SelectedDayTextColor = Color.White;
                    args.Calendar.MonthViewSettings.TodayTextColor = Color.FromHex(CurrentMonthHasSendColor);

                }
            }
            else
            {
                //Setting text color to days not of other month
                args.Calendar.MonthViewSettings.SelectedDayTextColor = Color.FromHex(OtherMonthNoSendColor);
            }

            //TODO: remove this when SFCalendar will be fixed (android)
            if (Device.RuntimePlatform == Device.Android)
            {
                var tempMonthSettings = calendar.MonthViewSettings;
                args.Calendar.MonthViewSettings = null;
                args.Calendar.MonthViewSettings = tempMonthSettings;
            }
        }
    }
}
