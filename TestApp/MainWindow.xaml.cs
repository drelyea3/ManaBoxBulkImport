using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Scryfall;

namespace TestApp;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public class ApplicationViewModel : ViewModelBase
{
    public CollectionViewSource SetsViewSource { get; } = new();
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ApplicationViewModel _viewModel = new();
    private ScryfallClient _scryfallClient = new("MBBI.Application");

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        _viewModel.SetsViewSource.SortDescriptions.Add(new SortDescription("Code", ListSortDirection.Ascending));

        Task.Run(async () =>
        {
            var sets = await _scryfallClient.GetSetsAsync();
            _ = Dispatcher.BeginInvoke(() =>
            {
                Debug.WriteLine($"Downloaded {sets.Count} sets.");
                 _viewModel.SetsViewSource.Source = sets?.Values;
                 _viewModel.SetsViewSource.View.Refresh();
            });
        });
    }
}