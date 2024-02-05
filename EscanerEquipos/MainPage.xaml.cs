using EscanerEquipos.Views;
using System.Collections.ObjectModel;
using System.Globalization;

namespace EscanerEquipos
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<ScanItem> scanHistory;
        public MainPage()
        {
            InitializeComponent();

            // Inicializa la lista observable que almacenará el historial de escaneos
            scanHistory = new ObservableCollection<ScanItem>();

            // Recupera el historial almacenado previamente al iniciar la aplicación
            if (Preferences.ContainsKey("ScanHistory"))
            {
                var storedHistory = Preferences.Get("ScanHistory", string.Empty);

                // Verifica si el historial almacenado no está vacío
                if (!string.IsNullOrEmpty(storedHistory))
                {
                    // Divide la cadena de historial en elementos individuales
                    var historyItems = storedHistory.Split(',');

                    // Itera a través de los elementos del historial
                    foreach (var item in historyItems)
                    {
                        // Convierte cada elemento de historial en un objeto ScanItem
                        var scanItem = ScanItem.FromString(item);

                        // Si el objeto ScanItem es válido, agrégalo a la lista observable
                        if (scanItem != null)
                        {
                            scanHistory.Add(scanItem);
                        }
                    }
                }
            }
        }

        private void cameraView_CamerasLoaded(object sender, EventArgs e)
        {
            // Verifica si hay cámaras disponibles
            if (cameraView.Cameras.Count > 0)
            {
                // Establece la primera cámara como cámara actual para el CameraView
                cameraView.Camera = cameraView.Cameras.First();

                // Detiene y reinicia la cámara asincrónicamente en el subproceso principal
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await cameraView.StopCameraAsync();
                    await cameraView.StartCameraAsync();
                });
            }
        }


        private int currentCameraIndex = 0;

        private void ToggleCamera_Clicked(object sender, EventArgs e)
        {
            if (cameraView.Cameras.Count > 1)
            {
                // Cambia al siguiente índice de cámara disponible
                currentCameraIndex = (currentCameraIndex + 1) % cameraView.Cameras.Count;

                // Asigna la cámara actual al CameraView
                cameraView.Camera = cameraView.Cameras[currentCameraIndex];

                // Detiene y reinicia la cámara con la nueva selección
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await cameraView.StopCameraAsync();
                    await cameraView.StartCameraAsync();
                });
            }
            else
            {
                DisplayAlert("Cambio de cámara", "No hay cámaras disponibles para cambiar.", "OK");
            }
        }

        private void cameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
        {
            // Asegurarse de que la actualización de la interfaz de usuario se realice en el subproceso principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (args.Result != null && args.Result.Length > 0)
                {
                    // Obtener el texto del código de barras detectado
                    string originalBarcodeText = args.Result[0].Text;

                    // Reemplazar los saltos de línea en el texto del código de barras con saltos de línea de plataforma cruzada
                    string barcodeText = originalBarcodeText.Replace("\n", Environment.NewLine);

                    // Asignar el texto del código de barras al campo de entrada (Entry) en la interfaz de usuario
                    barcodeEntry.Text = barcodeText;
                }
                else
                {
                    // Mostrar una alerta en caso de que no se haya detectado ningún código de barras válido
                    DisplayAlert("Detalles del escaneo", "Escaneo erróneo", "OK");
                }
            });
        }

        private void Guardar_Clicked(object sender, EventArgs e)
        {
            string scannedContent = barcodeEntry.Text;
            string manualContent = manualEntry.Text;
            string manual2Content = manual2Entry.Text;

            if (!string.IsNullOrEmpty(scannedContent) || !string.IsNullOrEmpty(manualContent) || !string.IsNullOrEmpty(manual2Content))
            {
                string combinedContent = $"{scannedContent}\nEstado del equipo: {manualContent}\nObservación del equipo: {manual2Content}";

                DateTime scanDate = DateTime.Now; // Obtener la fecha y hora actual
                var scanItem = new ScanItem(scanDate, combinedContent);
                scanHistory.Add(scanItem);

                // Almacenar historial persistentemente
                var historyString = string.Join(",", scanHistory.Select(item => item.ToString()));
                Preferences.Set("ScanHistory", historyString);

                barcodeEntry.Text = "";
                manualEntry.Text = "";
                manual2Entry.Text = "";
            }
        }

        private async void Historia_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScanHistoryPage());
        }

        public class ScanItem
        {
            public DateTime ScanDate { get; set; } // Fecha y hora del escaneo
            public string Content { get; set; }    // Contenido del escaneo

            // Constructor de la clase que recibe la fecha y hora del escaneo y su contenido
            public ScanItem(DateTime scanDate, string content)
            {
                ScanDate = scanDate;
                Content = content;
            }

            // Método que convierte un objeto ScanItem a una cadena con formato específico para almacenar en Preferences
            public override string ToString()
            {
                // El formato de almacenamiento es: "dd-MM-yyyy HH:mm:ss|Contenido del escaneo"
                return $"{ScanDate:dd-MM-yyyy HH:mm:ss}|{Content}";
            }

            // Método estático que crea un objeto ScanItem a partir de una cadena con formato específico almacenada en Preferences
            public static ScanItem FromString(string value)
            {
                var parts = value.Split('|');
                if (parts.Length == 2 && DateTime.TryParseExact(parts[0], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime scanDate))
                {
                    // Si la cadena tiene el formato correcto, se crea y devuelve un nuevo objeto ScanItem
                    return new ScanItem(scanDate, parts[1]);
                }
                // Si la cadena no tiene el formato correcto, se devuelve null
                return null;
            }
        }
    }
}
