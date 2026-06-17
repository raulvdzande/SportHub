using SportHub.App.Services.Api;
using SportHub.Shared.DTOs.CheckIns;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class HistoryViewModel : ViewModelBase
{
    private readonly ICheckInsApiClient _checkInsApi;

    public HistoryViewModel(ICheckInsApiClient checkInsApi)
    {
        _checkInsApi = checkInsApi;
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    private ObservableCollection<CheckInDto> _history = [];
    public ObservableCollection<CheckInDto> History { get => _history; set => SetProperty(ref _history, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)RefreshCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private bool _hasHistory;
    public bool HasHistory { get => _hasHistory; set => SetProperty(ref _hasHistory, value); }

    public ICommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var history = await _checkInsApi.GetMemberHistoryAsync();
            if (history != null)
            {
                History = new ObservableCollection<CheckInDto>(history.OrderByDescending(h => h.CheckedInAtUtc));
                HasHistory = History.Count > 0;
            }
            else
            {
                HasHistory = false;
            }
        }
        catch (TaskCanceledException)
        {
            Error = "Time-out — check network.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
