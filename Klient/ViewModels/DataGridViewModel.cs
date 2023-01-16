using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Klient.Contracts.ViewModels;
using Klient.Core.Contracts.Services;
using Klient.Core.Models;

namespace Klient.ViewModels;

public class DataGridViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

    public DataGridViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetGridDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
        Source.Add(new SampleOrder() { OrderID = 1237, OrderDate = DateTime.Now, Status = "ready" });
    }

    public void OnNavigatedFrom()
    {
    }
}
