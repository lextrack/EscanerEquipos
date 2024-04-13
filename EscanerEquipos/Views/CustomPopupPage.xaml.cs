namespace EscanerEquipos.Views;

public partial class CustomPopupPage : ContentPage
{
    public CustomPopupPage(string content)
    {
        InitializeComponent();
        contentEntry.Text = content;
    }

    private void CopyButton_Clicked(object sender, EventArgs e)
    {
        Clipboard.SetTextAsync(contentEntry.Text);
        DisplayAlert("Copiar", "El contenido ha sido copiado al portapapeles.", "OK");
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}