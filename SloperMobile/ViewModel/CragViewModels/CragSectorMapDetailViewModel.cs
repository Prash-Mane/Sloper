using SloperMobile.ViewModel;
using Prism.Navigation;
using SloperMobile.Model.CragModels;
using Xamarin.Forms;
using SloperMobile.Common.Constants;
using SloperMobile.ViewModel.SectorViewModels;
using SloperMobile.Common.Extentions;
using System.Threading.Tasks;
using System;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using Acr.UserDialogs;

namespace SloperMobile
{
    public class CragSectorMapDetailViewModel : BaseViewModel
    {
        CragMapModel cragMap;

        public CragSectorMapDetailViewModel(
                    INavigationService navigationService,
                    IExceptionSynchronizationManager exManager
                ) : base(navigationService, exManager)
        {
            IsBackButtonVisible = true;
        }

        public CragMapModel CragMap
        {
            get => cragMap; 
            set => SetProperty(ref cragMap, value);
        }

        public Command<int> BoxCommand { get => new Command<int>((id) => GoToSectorAsync(id)); }


        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (parameters.TryGetValue("cragMapModel", out CragMapModel tempCragMap))
            {
                CragMap = tempCragMap;
                PageHeaderText = CragMap.Map.map_name;
                PageSubHeaderText = CragMap.CragName;
            }
        }

        async Task GoToSectorAsync(int sectorId) 
        {
            try
            {
                var mapListModel = await MapModelHelper.GetFromSectorIdAsync(sectorId);

                var navParams = new NavigationParameters();
                navParams.Add(NavigationParametersConstants.SelectedSectorObjectParameter, mapListModel);
                await navigationService.NavigateAsync<SectorRoutesViewModel>(navParams);
            }
            catch (Exception ex)
            {
                exceptionManager.LogException( new ExceptionTable { 
                    Method = nameof(GoToSectorAsync),
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Page = GetType().Name,
                    Data = $"sectorId: {sectorId}"
                });
                UserDialogs.Instance.Alert("Error getting sector data");
            }
        }
    }
}
