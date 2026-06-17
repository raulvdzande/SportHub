using SportHub.App.ViewModels;
using System.ComponentModel;

namespace SportHub.App.Pages;

[QueryProperty(nameof(LessonId), "id")]
public partial class LessonDetailsPage : ContentPage
{
    private readonly LessonDetailsViewModel _vm;

    public LessonDetailsPage(LessonDetailsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LessonDetailsViewModel.Details))
        {
            UpdateCheckInCount();
            UpdateWaitlistInfo();
        }
    }

    private void UpdateCheckInCount()
    {
        if (_vm.Details?.Participants == null || _vm.Details.Participants.Count == 0)
        {
            CheckInCountLabel.Text = string.Empty;
            return;
        }

        var checkedInCount = _vm.Details.Participants.Count(p => p.HasCheckedIn);
        var totalCount = _vm.Details.Participants.Count;
        CheckInCountLabel.Text = $"Checked in: {checkedInCount}/{totalCount}";
    }

    private void UpdateWaitlistInfo()
    {
        if (_vm.Details?.Participants == null)
        {
            WaitlistCountLabel.Text = string.Empty;
            ReservedMembersView.ItemsSource = null;
            WaitlistMembersView.ItemsSource = null;
            return;
        }

        var reserved = _vm.Details.Participants
            .Where(p => p.ReservationStatus == "Reserved")
            .OrderBy(p => p.DisplayName)
            .ToList();

        var waitlisted = _vm.Details.Participants
            .Where(p => p.ReservationStatus == "Waitlisted")
            .OrderBy(p => p.DisplayName)
            .ToList();

        ReservedMembersView.ItemsSource = reserved;
        WaitlistMembersView.ItemsSource = waitlisted;

        var waitlistCount = waitlisted.Count;
        WaitlistCountLabel.Text = waitlistCount > 0
            ? $"Waitlist: {waitlistCount} people"
            : "No waitlist";
    }

    public string? LessonId
    {
        set
        {
            if (value is not null && Guid.TryParse(value, out var id))
                _ = _vm.LoadAsync(id);
        }
    }

    private void OnRemoveParticipantClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Shared.DTOs.Lessons.LessonParticipantDto participant)
        {
            _vm.RemoveParticipantCommand.Execute(participant.MemberId);
        }
    }
}
