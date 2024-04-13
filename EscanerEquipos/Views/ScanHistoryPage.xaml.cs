using System.Collections.ObjectModel;
using static EscanerEquipos.MainPage;

namespace EscanerEquipos.Views;

public partial class ScanHistoryPage : ContentPage
{
    private ObservableCollection<ScanItem> scanHistory;
    private int startIndex = 0; // Índice de inicio de visualización
    private int itemsPerPage = 10; // Cantidad de elementos por página

    public ScanHistoryPage()
    {
        InitializeComponent();
        scanHistory = new ObservableCollection<ScanItem>();
        scanHistoryListView.ItemsSource = scanHistory;

        // Recuperar historial almacenado previamente al iniciar la página
        if (Preferences.ContainsKey("ScanHistory"))
        {
            var storedHistory = Preferences.Get("ScanHistory", string.Empty);
            if (!string.IsNullOrEmpty(storedHistory))
            {
                var historyItems = storedHistory.Split(',');
                foreach (var item in historyItems)
                {
                    var scanItem = ScanItem.FromString(item);
                    if (scanItem != null)
                    {
                        scanHistory.Add(scanItem);
                    }
                }
            }
        }

        // Ordenar la lista por fecha en orden descendente (los más recientes primero)
        scanHistory = new ObservableCollection<ScanItem>(scanHistory.OrderByDescending(item => item.ScanDate));

        // Cargar solo los elementos de la página actual
        LoadPage(startIndex);
    }


    private void LoadPage(int startIndex)
    {
        scanHistoryListView.ItemsSource = scanHistory.Skip(startIndex).Take(itemsPerPage);
    }

    private void PreviousButton_Clicked(object sender, EventArgs e)
    {
        startIndex -= itemsPerPage;
        if (startIndex < 0)
        {
            startIndex = 0;
        }

        LoadPage(startIndex);
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        startIndex += itemsPerPage;
        if (startIndex >= scanHistory.Count)
        {
            startIndex -= itemsPerPage;
        }
        else
        {
            LoadPage(startIndex);
        }
    }

    private void DeleteOldEntries(int daysOld)
    {
        DateTime thresholdDate = DateTime.Now.AddDays(-daysOld);

        var itemsToRemove = scanHistory.Where(item => item.ScanDate < thresholdDate).ToList();

        foreach (var item in itemsToRemove)
        {
            scanHistory.Remove(item);
        }

        // Actualizar los Preferences después de la eliminación
        var updatedHistoryString = string.Join(",", scanHistory.Select(item => item.ToString()));
        Preferences.Set("ScanHistory", updatedHistoryString);
    }

    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        bool isConfirmed = await DisplayAlert(
            "Eliminar",
            "¿Quieres eliminar los registros que tienen 30 días de antiguedad?",
            "Sí",
            "No"
        );

        if (isConfirmed)
        {
            DeleteOldEntries(30);
        }
    }

    private async void scanHistoryListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ScanItem selectedScan)
        {
            CustomPopupPage popupPage = new CustomPopupPage(selectedScan.Content);
            await Navigation.PushModalAsync(popupPage);
        }
    }
}