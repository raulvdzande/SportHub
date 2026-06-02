using SportHub.App.Services.Api;
using SportHub.App.ViewModels;
using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.Pages;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _vm;

    public SchedulePage(ScheduleViewModel vm)
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
