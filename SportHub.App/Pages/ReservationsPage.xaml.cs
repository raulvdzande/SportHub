using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

public partial class ReservationsPage : ContentPage
{
    private readonly ReservationsViewModel _vm;

    public ReservationsPage(ReservationsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
