using Microsoft.Extensions.DependencyInjection;
using Scryfall;
using Scryfall.Models;
using System.ComponentModel;
using System.Windows.Data;

namespace TestApp.ViewModels;

public class ApplicationViewModel : ViewModelBase
{
    private CardSet? _selectedSetDefinition;
    private IEnumerable<CardDefinition>? _cardDefinitions;
    private ColorFilter _manaCostFilter;
    private ColorFilter _manaProducedFilter;

    public ColorFilter ManaCostFilter
    {
        get => _manaCostFilter;
        set => SetField(ref _manaCostFilter, value);
    }


    public ColorFilter ManaProducedFilter
    {
        get => _manaProducedFilter;
        set => SetField(ref _manaProducedFilter, value);
    }

    public CollectionViewSource SetDefinitions { get; } = new();
    public IEnumerable<CardDefinition>? CardDefinitions { get => _cardDefinitions; set => SetField(ref _cardDefinitions , value); }

    public CardSet? SelectedSetDefinition
    {
        get => _selectedSetDefinition;
        set 
        { 
            if (SetField(ref _selectedSetDefinition, value))
            {
                UpdateCardSet();
            }
        }
    }

    public ApplicationViewModel()
    {
        _manaCostFilter = new ColorFilter();
        _manaCostFilter.PropertyChanged += OnFilterPropertyChanged;

        _manaProducedFilter = new ColorFilter();
        _manaProducedFilter.PropertyChanged += OnFilterPropertyChanged;
    }

    private void OnFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ManaCostFilter));
    }

    private async void UpdateCardSet()
    {
        CardDefinitions = [];

        if (SelectedSetDefinition != null)
        {
            var client = Services.Provider.GetRequiredService<ScryfallClient>();
            CardDefinitions = await client.GetCards(SelectedSetDefinition);
        }
    }
}
