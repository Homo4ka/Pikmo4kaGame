using System.Text.Json;

namespace Pikmo4kaGame
{
    public partial class Form1 : Form
    {
        private long _matter = 0;
        private long _cps = 0;
        private long _droneCost = 15;
        private int _droneCount = 0;
        private long _stationCost = 100;
        private int _stationCount = 0;

        private System.Windows.Forms.Timer _gameTimer;

        private Microsoft.Web.WebView2.WinForms.WebView2 _webView;

        private readonly string _saveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gamesave.json");

        public Form1()
        {
            InitializeComponent();
            LoadGame();
            InitGame();
            this.FormClosing += Form1_FormClosing;
        }

        private void SaveGame()
        {
            try
            {
                var saveData = new GameSaveData
                {
                    Matter = _matter,
                    Cps = _cps,
                    DroneCost = _droneCost,
                    StationCost = _stationCost,
                    DroneCount = _droneCount,
                    StationCount = _stationCount
                };

                string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(_saveFilePath, jsonString);
                System.Diagnostics.Debug.WriteLine("Игра успешно сохранена!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void LoadGame()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    string jsonString = File.ReadAllText(_saveFilePath);
                    var saveData = JsonSerializer.Deserialize<GameSaveData>(jsonString);

                    if (saveData != null)
                    {
                        _matter = saveData.Matter;
                        _cps = saveData.Cps;
                        _droneCost = saveData.DroneCost;
                        _stationCost = saveData.StationCost;
                        _droneCount = saveData.DroneCount;
                        _stationCount = saveData.StationCount;
                        System.Diagnostics.Debug.WriteLine("Сохранение успешно загружено!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SaveGame();
        }


        private async void InitGame()
        {
            _webView = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_webView);

            await _webView.EnsureCoreWebView2Async(null);

            // D:
            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
            _webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);

            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            _webView.CoreWebView2.NavigationCompleted += (s, args) => { SyncUiWithEngine(); };

            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 1000;
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
        }

        private void OnWebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string command = e.TryGetWebMessageAsString();

            System.Diagnostics.Debug.WriteLine($"Получена команда от игры: {command}");

            if (command == "click")
            {
                _matter += 1;
            }
            else if (command == "buy_drone")
            {
                if (_matter >= _droneCost)
                {
                    _matter -= _droneCost;
                    _droneCount++;
                    _cps += 1;
                    _droneCost = (long)(_droneCost * 1.15);
                }
            }
            else if (command == "buy_station")
            {
                if (_matter >= _stationCost)
                {
                    _matter -= _stationCost;
                    _stationCount++;
                    _cps += 10;
                    _stationCost = (long)(_stationCost * 1.25);
                }
            }
            SyncUiWithEngine();
        }


        private void GameTimer_Tick(object sender, EventArgs e)
        {
            _matter += _cps;
            SyncUiWithEngine();
        }

        private void SyncUiWithEngine()
        {
            if (_webView?.CoreWebView2 == null) return;

            var gameState = new
            {
                matter = _matter,
                cps = _cps,
                droneCost = _droneCost,
                stationCost = _stationCost
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(gameState);

            _webView.CoreWebView2.PostWebMessageAsJson(jsonPayload);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    public class GameSaveData
    {
        public long Matter { get; set; }
        public long Cps { get; set; }
        public long DroneCost { get; set; }
        public long StationCost { get; set; }
        public int DroneCount { get; set; }
        public int StationCount { get; set; }
    }

}
