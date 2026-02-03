using DoomLauncher.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.IO; 
using System.Threading.Tasks; 
using DoomLauncher.Handlers; 
using DoomLauncher.DataSources; 
using System.Reflection; 

namespace DoomLauncher
{
    public partial class IdGamesTabViewCtrl : OptionsTabViewCtrl
    {
        private bool m_working;
        private string m_errorMessage;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;

        // Yedek link (Ayarlar boşsa bunu kullanır)
        private const string DEFAULT_SEARCH_URL = "https://www.doomworld.com/idgames/index.php?search=1&field=filename&word={0}&sort=time&order=asc&page=1";

        public IdGamesTabViewCtrl(object key, string title, IGameFileDataSourceAdapter adapter, GameFileFieldType[] selectFields, GameFileViewFactory factory)
            : base(key, title, adapter, selectFields, factory)
        {
            InitializeComponent();
            this.Load += (s, e) => InitBrowser();
        }

        private async void InitBrowser()
        {
            try
            {
                if (webView == null)
                {
                    webView = new Microsoft.Web.WebView2.WinForms.WebView2();
                    webView.Dock = DockStyle.Fill;
                    this.Controls.Add(webView);
                }

                await webView.EnsureCoreWebView2Async(null);

                // --- İNDİRME MANTIĞI (DOWNLOAD LOGIC) ---
                webView.CoreWebView2.DownloadStarting += (sender, args) =>
                {
                    string gameFilesFolder = DataCache.Instance.AppConfiguration.GameFileDirectory.GetFullPath();
                    if (!Directory.Exists(gameFilesFolder)) Directory.CreateDirectory(gameFilesFolder);

                    string downloadFileName = Path.GetFileName(args.ResultFilePath);
                    string finalFilePath = Path.Combine(gameFilesFolder, downloadFileName);

                    if (File.Exists(finalFilePath)) try { File.Delete(finalFilePath); } catch { }

                    args.ResultFilePath = finalFilePath;
                    args.Handled = true; 

                    this.Invoke(new MethodInvoker(delegate {
                        // [TR: İNDİRİLİYOR -> EN: DOWNLOADING]
                        SetDisplayText($"DOWNLOADING: {downloadFileName}...");
                        Form mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == "MainForm");
                        if(mainForm != null) mainForm.Text = $"DoomLauncher - DOWNLOADING: {downloadFileName}";
                    }));

                    args.DownloadOperation.StateChanged += (s, e) =>
                    {
                        if (args.DownloadOperation.State == Microsoft.Web.WebView2.Core.CoreWebView2DownloadState.Completed)
                        {
                            Task.Run(async () => {
                                try {
                                    await Task.Delay(500); 

                                    if (File.Exists(finalFilePath))
                                    {
                                        var existingFile = DataCache.Instance.DataSourceAdapter.GetGameFile(downloadFileName);

                                        if (existingFile == null)
                                        {
                                            GameFile newGameFile = new GameFile();
                                            newGameFile.FileName = downloadFileName;
                                            newGameFile.Title = Path.GetFileNameWithoutExtension(downloadFileName);
                                            newGameFile.Downloaded = DateTime.Now;
                                            DataCache.Instance.DataSourceAdapter.InsertGameFile(newGameFile);
                                        }
                                        else
                                        {
                                            existingFile.Downloaded = DateTime.Now;
                                            DataCache.Instance.DataSourceAdapter.UpdateGameFile(existingFile);
                                        }

                                        this.Invoke(new MethodInvoker(delegate {
                                            try {
                                                Form mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == "MainForm");
                                                if (mainForm != null)
                                                {
                                                    MethodInfo updateMethod = mainForm.GetType().GetMethod("UpdateLocal", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                                                    if(updateMethod != null) updateMethod.Invoke(mainForm, null);
                                                }
                                            } catch { }

                                            // [TR: TAMAMLANDI -> EN: COMPLETED]
                                            SetDisplayText($"COMPLETED: {downloadFileName}");
                                            Form mForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == "MainForm");
                                            if(mForm != null) mForm.Text = "DoomLauncher"; 
                                            
                                            MessageBox.Show($"{downloadFileName} downloaded successfully!", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }));
                                    }
                                    else 
                                    {
                                        // [TR: HATA -> EN: ERROR]
                                        this.Invoke(new MethodInvoker(delegate { SetDisplayText("ERROR: File could not be downloaded!"); }));
                                    }
                                }
                                catch (Exception ex) 
                                { 
                                    this.Invoke(new MethodInvoker(delegate { SetDisplayText($"Error: {ex.Message}"); })); 
                                }
                            });
                        }
                    };
                };

                // Başlangıç Sayfası
                string startUrl = DataCache.Instance.AppConfiguration.IdGamesUrl;
                if (string.IsNullOrEmpty(startUrl)) startUrl = "https://www.doomworld.com/idgames/";

                webView.CoreWebView2.Navigate(startUrl);
                webView.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Browser Error: " + ex.Message);
            }
        }

        // --- GELİŞMİŞ ARAMA SİSTEMİ (SEARCH SYSTEM) ---
        public override void SetGameFiles(IEnumerable<GameFileSearchField> searchFields)
        {
            string searchText = string.Empty;

            // 1. YÖNTEM: Normal yolla yazıyı almayı dene
            if (searchFields != null)
            {
                foreach (var field in searchFields)
                {
                    if (!string.IsNullOrEmpty(field.SearchText))
                    {
                        searchText = field.SearchText;
                        break; 
                    }
                }
            }

            // 2. YÖNTEM: Eğer normal yol boş geldiyse, MAYMUNCUK ile Ana Formdan çal
            if (string.IsNullOrEmpty(searchText))
            {
                try {
                    Form mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == "MainForm");
                    if (mainForm != null)
                    {
                        FieldInfo field = mainForm.GetType().GetField("ctrlSearch", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (field != null)
                        {
                            object searchControl = field.GetValue(mainForm);
                            if (searchControl != null)
                            {
                                PropertyInfo prop = searchControl.GetType().GetProperty("SearchText");
                                if (prop != null)
                                {
                                    object val = prop.GetValue(searchControl);
                                    if (val != null) searchText = val.ToString();
                                }
                            }
                        }
                    }
                } catch { /* Sessiz hata */ }
            }

            // --- ARAMA VEYA YÖNLENDİRME KISMI ---
            if (webView != null && webView.CoreWebView2 != null)
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    string searchSetting = DataCache.Instance.AppConfiguration.ApiPage;
                    string targetUrl = "";

                    // Bozuk ayar koruması
                    if (string.IsNullOrEmpty(searchSetting) || searchSetting.Contains("api.php"))
                    {
                        targetUrl = string.Format(DEFAULT_SEARCH_URL, searchText);
                    }
                    else
                    {
                        if (searchSetting.Contains("{0}"))
                            targetUrl = string.Format(searchSetting, searchText);
                        else
                            targetUrl = searchSetting + searchText;
                    }

                    webView.CoreWebView2.Navigate(targetUrl);
                }
                else
                {
                    string homeUrl = DataCache.Instance.AppConfiguration.IdGamesUrl;
                    if (string.IsNullOrEmpty(homeUrl)) homeUrl = "https://www.doomworld.com/idgames/";
                    webView.CoreWebView2.Navigate(homeUrl);
                }
            }

            // Eski sistemin araya girmesini engelle
            return;
        }

        public override void SetGameFiles() { SetGameFiles(null); }
        private void UpdateIdGamesView_Worker(object sender, DoWorkEventArgs e) { }
        private void UpdateIdGamesViewCompleted(object sender, EventArgs e) { }
        public override bool IsLocal { get { return false; } }
        public override bool IsEditAllowed { get { return false; } }
        public override bool IsDeleteAllowed { get { return false; } }
        public override bool IsSearchAllowed { get { return true; } }
        public override bool IsPlayAllowed { get { return false; } }
        public override bool IsAutoSearchAllowed { get { return false; } }
        public IEnumerable<IGameFile> IdGamesDataSource { get; set; }
        protected override bool FilterIWads { get { return false; } }
    }
}