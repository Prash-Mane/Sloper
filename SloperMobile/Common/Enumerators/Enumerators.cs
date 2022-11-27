namespace SloperMobile.Common.Enumerators
{
    public enum ApplicationActivity
    {
        CheckForUpdatesPage = 1,
        HomePage = 3,
        UserLoginPage = 4,
        CragSectorsPage = 5,
        DrawerMenuListPage = 6,
        MasterNavigationPage = 7,
        NewsPage = 8,
        UserRegistrationPage = 11,
        //ProfileSendsPage = 13,
        MyProfilePage = 14,
        GoogleMapPinsPage = 15,
        //ProfilePointsPage = 16,
        //ProfileTickListPage = 17,
        //ProfileCalendarPage = 18,
        //ProfileRankingPage = 19,
        JournalFeedPage = 20,
        MemberProfilePage = 21,
        //ProfilePage = 22
        GuideBookPage = 23,
    }

    public enum ProfileViews
    {
        ProfileRanking = 0,
        ProfilePoints = 1,
        ProfileSends = 2,
        ProfileCalendar = 3,
        ProfileTickList = 4
    }

    public enum AppSteepness
    {
        Slab = 1,
        Vertical = 2,
        Overhanging = 4,
        Roof = 8
	}

	public enum ImageType
	{
		RouteImage = 1,
		SummaryImage = 2
	}

    public enum Offsets
    { 
        None, Header, Footer, Both
    }

    public enum Metrics
    { 
        imperial, metric
    }
}

