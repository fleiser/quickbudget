using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using MahApps.Metro.Controls;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for ChooseBudget.xaml
    /// </summary>
    public partial class ChooseBudget : MetroWindow
    {
        private static readonly string[] Scopes = { DriveService.Scope.DriveAppdata};
        private const string ApplicationName = "QuickBudget";
        private readonly Thread _thread;
        public ChooseBudget()
        {
            InitializeComponent();
            //drive here
            _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _path = Path.Combine(_path, "Budgets");
            _thread = new Thread(Synchronize);
            _thread.Start();
            //drive

            _currencies.Add(new Currency(0, "$", "USD", false));
            _currencies.Add(new Currency(0, "€", "EUR", false));
            _currencies.Add(new Currency(0, "kr", "DKK", false));
            _currencies.Add(new Currency(0, "kr", "NOK", false));
            _currencies.Add(new Currency(0, "kr", "SEK", false));
            _currencies.Add(new Currency(0, "Kč", "CZK", false));
            _currencies.Add(new Currency(0, "£", "GBP", false));
            _currencies.Add(new Currency(0, "$", "CAD", false));
            _currencies.Add(new Currency(0, "$", "AUD", false));
            _currencies.Add(new Currency(0, "$", "NZD", false));
            _currencies.Add(new Currency(0, "Ft", "HUF", false));
            _currencies.Add(new Currency(0, "CHF", "CHF", false));
            _currencies.Add(new Currency(0, "zł", "PLN", false));
            _currencies.Add(new Currency(0, "lei", "RON", false));
            _currencies.Add(new Currency(0, "kn", "HRK", false));
            _currencies.Add(new Currency(0, "лв", "BGN", false));
            _currencies.Add(new Currency(0, "₽", "RUB", false));
            _currencies.Add(new Currency(0, "₺", "TRY", false));
            _currencies.Add(new Currency(0, "R$", "BRL", false));
            _currencies.Add(new Currency(0, "¥", "JPY", false));
            _currencies.Add(new Currency(0, "¥", "CNY", false));
            _currencies.Add(new Currency(0, "$", "HKD", false));
            _currencies.Add(new Currency(0, "Rp", "IDR", false));
            _currencies.Add(new Currency(0, "₪", "ILS", false));
            _currencies.Add(new Currency(0, "₹", "INR", false));
            _currencies.Add(new Currency(0, "₩", "KRW", false));
            _currencies.Add(new Currency(0, "$", "MXN", false));
            _currencies.Add(new Currency(0, "RM", "MYR", false));
            _currencies.Add(new Currency(0, "₱", "PHP", false));
            _currencies.Add(new Currency(0, "$", "SGD", false));
            _currencies.Add(new Currency(0, "฿", "THB", false));
            _currencies.Add(new Currency(0, "R", "ZAR", false));

            //RefreshDatabases();

        }

        private void Synchronize()
        {
            DownloadAll();
            // UploadAll();
            //TODO crash on close after start
            this.Dispatcher.Invoke((Action)(RefreshDatabases));
            #region

            /*
              UserCredential credential;

              using (var stream =
                  new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
              {
                  string credPath = System.Environment.GetFolderPath(
                      System.Environment.SpecialFolder.Personal);
                  credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                  credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                      GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                  Console.WriteLine("Credential file saved to: " + credPath);
              }

              // Create Drive API service.
              var service = new DriveService(new BaseClientService.Initializer()
              {
                  HttpClientInitializer = credential,
                  ApplicationName = ApplicationName,
              });

              // Define parameters of request.
              FilesResource.ListRequest listRequest = service.Files.List();
              listRequest.PageSize = 10;
              listRequest.Fields = "nextPageToken, files(id, name)";
              var request = service.Files.List();
              request.Spaces = "appDataFolder";
              request.Fields = "nextPageToken, files(id, name)";
              request.PageSize = 10;
              var result = request.Execute();
              var driveFiles = result.Files.ToList();
              Directory.CreateDirectory("Budgets");
              var localFiles = Directory.EnumerateFiles(_path, "*.sqlite").ToList();
              //
              try
              {

                  foreach (var file in localFiles)
                  {
                      var fileName = Path.GetFileName(file);
                      if (driveFiles.Any(x => x.Name.Equals(fileName) && File.GetLastWriteTime(file) > x.ModifiedTime))
                      {
                          //UPDATE EXISTING ON DRIVE
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              _budgetList.Add(new Budget(fileName, Path.GetFullPath(file), Path.GetFileNameWithoutExtension(file)));
                              var budget = _budgetList.First(x => x.Name.Equals(fileName));
                              budget.LoadAble = false;
                              budget.State = "Uploading to drive";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));

                          var body = new Google.Apis.Drive.v3.Data.File
                          {
                              Name = Path.GetFileName(file),
                              Description = "QuickBudget Database",
                              MimeType = ".sqlite database",
                              ModifiedTime = File.GetLastWriteTime(file)
                          };
                          var byteArray = File.ReadAllBytes(file);
                          MemoryStream stream = new MemoryStream(byteArray);
                          string id = result.Files.First(x => x.Name.Equals(body.Name)).Id;
                          FilesResource.UpdateMediaUpload req = service.Files.Update(body, id, stream, GetMimeType(Path.GetFileName(file)));
                          req.Fields = "id";
                          req.Upload();
                          IUploadProgress progress = req.GetProgress();
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              Budget budget = _budgetList.First(x => x.Name.Equals(fileName));
                              budget.LoadAble = true;
                              budget.State = "Upload succesful";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));
                      }
                      else if (driveFiles.Any(x => x.Name.Equals(fileName) && File.GetLastWriteTime(file) < x.ModifiedTime))
                      {
                          //DOWNLOAD NEW VERSION TO PC
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              _budgetList.Add(new Budget(fileName, Path.GetFullPath(file), Path.GetFileNameWithoutExtension(file)));
                              var budget = _budgetList.First(x => x.Name.Equals(fileName));
                              budget.LoadAble = false;
                              budget.State = "Updating";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));
                          var fileId = driveFiles.First(x => x.Name.Equals(Path.GetFileName(file))).Id;
                          var downloadRequest = service.Files.Get(fileId);
                          var stream = new MemoryStream();
                          downloadRequest.Download(stream);
                          var downloadPath = Path.Combine(_path, Path2.GetFileName(file));
                          stream.Position = 0;
                          var fileStream = File.Create(downloadPath);
                          stream.CopyTo(fileStream);
                          fileStream.Close();
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              var budget = _budgetList.First(x => x.Name.Equals(fileName));
                              budget.LoadAble = true;
                              budget.State = "Completed";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));
                      }
                      else if (!driveFiles.Any(x => x.Name.Equals(fileName)))
                      {
                          //UPLOAD NON-EXISTING TO DRIVE
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              _budgetList.Add(new Budget(fileName, Path.GetFullPath(file), Path.GetFileNameWithoutExtension(file)));
                              var budget = _budgetList.First(x => x.Name.Equals(fileName));
                              budget.LoadAble = false;
                              budget.State = "Uploading to drive";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));
                          var body = new Google.Apis.Drive.v3.Data.File
                          {
                              Name = Path.GetFileName(file),
                              Parents = new List<string>() { "appDataFolder" },
                              Description = "QuickBudget Database",
                              MimeType = ".sqlite database",
                              ModifiedTime = File.GetLastWriteTime(file)
                          };
                          var byteArray = File.ReadAllBytes(file);
                          MemoryStream stream = new MemoryStream(byteArray);

                          FilesResource.CreateMediaUpload req = service.Files.Create(body, stream, GetMimeType(Path.GetFileName(file)));
                          req.Fields = "id";
                          req.Upload();
                          IUploadProgress progress = req.GetProgress();
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              var budget = _budgetList.First(x => x.Name.Equals(fileName));
                              budget.LoadAble = true;
                              budget.State = "Completed";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));
                      }

                  }
                  //DOWNLOAD NEW ONES TO PC
                  foreach (var file in driveFiles)
                  {
                      if (!localFiles.Any(x => Path.GetFileName(x).Equals(file))) //TODO  existing downloads new, possibly different format? comparing "Test" to "Test.sqlite"??
                      {
                          //DOWNLOAD NEW BUDGET TO PC
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              string noExtension = file.Name;
                              string[] a = noExtension.Split('.');
                              noExtension = a[0];
                              _budgetList.Add(new Budget(file.Name, "",noExtension ));
                              var budget = _budgetList.First(x => x.Name.Equals(file.Name));
                              budget.LoadAble = false;
                              budget.State = "Downloading";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));
                          var fileId = file.Id;
                          var downloadRequest = service.Files.Get(fileId);
                          var stream = new System.IO.MemoryStream();
                          downloadRequest.MediaDownloader.ProgressChanged +=
                              (IDownloadProgress progress) =>
                              {
                                  switch (progress.Status)
                                  {
                                      case DownloadStatus.Downloading:
                                      {
                                              Console.WriteLine(progress.BytesDownloaded);
                                              break;
                                          }
                                      case DownloadStatus.Completed:
                                          {
                                              Console.WriteLine("Download complete.");
                                              break;
                                          }
                                      case DownloadStatus.Failed:
                                          {
                                              Console.WriteLine("Download failed.");
                                              break;
                                          }
                                  }
                              };
                          downloadRequest.Download(stream);
                          var downloadPath = Path.Combine(_path, file.Name);
                          stream.Position = 0;
                          var fileStream = File.Create(downloadPath);
                          stream.CopyTo(fileStream);
                          fileStream.Close();
                          this.Dispatcher.Invoke((Action)(() =>
                          {
                              var budget = _budgetList.First(x => x.Name.Equals(file.Name));
                              budget.Path = Path.GetFullPath(downloadPath);
                              budget.NameWithoutExtension = Path.GetFileNameWithoutExtension(downloadPath);
                              budget.LoadAble = false;
                              budget.State = "Completed";
                              ListViewBudgets.ItemsSource = _budgetList;
                              ListViewBudgets.Items.Refresh();
                          }));
                      }
                  }
              }
              catch (Google.GoogleApiException)
              {

                  throw;
              }*/

            #endregion

        }

        private readonly List<Currency> _currencies = new List<Currency>();

        public void RefreshDatabases()
        {
            _budgetList.Clear();

            foreach (var file in Directory.EnumerateFiles(_path, "*.sqlite").ToList())
            {
                var fileName = Path.GetFileName(file);
                _budgetList.Add(new Budget(fileName, Path.GetFullPath(file), Path.GetFileNameWithoutExtension(file)));
            }

                ListViewBudgets.ItemsSource = _budgetList;
                ListViewBudgets.Items.Refresh();

        }

        private void DownloadAll()
        {
            try
            {

                UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            var request = service.Files.List();
            request.Spaces = "appDataFolder";
            request.Fields = "nextPageToken, files(id, name)";
            request.PageSize = 10;
            var result = request.Execute();
            var driveFiles = result.Files.ToList();
            Directory.CreateDirectory("Budgets");
            var localFiles = Directory.EnumerateFiles(_path, "*.sqlite").ToList();


            foreach (var file in localFiles)
            {
                var fileName = Path.GetFileName(file);
                if (driveFiles.Any(x => x.Name.Equals(fileName) && File.GetLastWriteTime(file) < x.ModifiedTime))
                {
                    //DOWNLOAD NEW VERSION TO PC
                    Dispatcher.Invoke((Action)(() =>
                    {
                        _budgetList.Add(new Budget(fileName, Path.GetFullPath(file), Path.GetFileNameWithoutExtension(file)));
                        var budget = _budgetList.First(x => x.Name.Equals(fileName));
                        budget.LoadAble = false;
                        budget.State = "Updating";
                        ListViewBudgets.ItemsSource = _budgetList;
                        ListViewBudgets.Items.Refresh();
                    }));
                    var fileId = driveFiles.First(x => x.Name.Equals(Path.GetFileName(file))).Id;
                    var downloadRequest = service.Files.Get(fileId);
                    var stream = new MemoryStream();
                    downloadRequest.Download(stream);
                    var downloadPath = Path.Combine(_path, Path2.GetFileName(file));
                    stream.Position = 0;
                    var fileStream = File.Create(downloadPath);
                    stream.CopyTo(fileStream);
                    fileStream.Close();
                    Dispatcher.Invoke(() =>
                    {
                        var budget = _budgetList.First(x => x.Name.Equals(fileName));
                        budget.LoadAble = true;
                        budget.State = "Ready";
                        ListViewBudgets.ItemsSource = _budgetList;
                        ListViewBudgets.Items.Refresh();
                    });
                }
            }
            foreach (var file in driveFiles)
            {
                if (!localFiles.Any(x => Path.GetFileName(x).Equals(file.Name))) 
                {
                    //DOWNLOAD NEW BUDGET TO PC
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        string noExtension = file.Name;
                        string[] a = noExtension.Split('.');
                        noExtension = a[0];
                        _budgetList.Add(new Budget(file.Name, "", noExtension));
                        var budget = _budgetList.First(x => x.Name.Equals(file.Name));
                        budget.LoadAble = false;
                        budget.State = "Downloading";
                        ListViewBudgets.ItemsSource = _budgetList;
                        ListViewBudgets.Items.Refresh();
                    }));
                    var fileId = file.Id;
                    var downloadRequest = service.Files.Get(fileId);
                    var stream = new System.IO.MemoryStream();
                    downloadRequest.Download(stream);
                    var downloadPath = Path.Combine(_path, file.Name);
                    stream.Position = 0;
                    var fileStream = File.Create(downloadPath);
                    stream.CopyTo(fileStream);
                    fileStream.Close();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        var budget = _budgetList.First(x => x.Name.Equals(file.Name));
                        budget.Path = Path.GetFullPath(downloadPath);
                        budget.NameWithoutExtension = Path.GetFileNameWithoutExtension(downloadPath);
                        budget.LoadAble = false;
                        budget.State = "Ready";
                        ListViewBudgets.ItemsSource = _budgetList;
                        ListViewBudgets.Items.Refresh();
                    }));
                }
            }
            }
            catch (Exception exception) when (exception is Google.GoogleApiException || exception is HttpRequestException)
            {
                bool retry = false;
                Dispatcher.Invoke((Action)(() =>
                {
                    YesNo yesNo = new YesNo("Quickbudget failed to connect to Google Drive. Do you want to try again?") {Owner = GetWindow(this)};
                    yesNo.ShowDialog();
                    if (yesNo.IsSuccesful)
                    {
                        retry = true;
                    }
                }));
                if (retry)
                {
                    DownloadAll();
                }
                else
                {
                    //TODO load local
                }


            }
        }

        private void UploadAll()
        {
            try
            {
                UserCredential credential;

                using (var stream =
                    new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define parameters of request.
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";
                var request = service.Files.List();
                request.Spaces = "appDataFolder";
                request.Fields = "nextPageToken, files(id, name)";
                request.PageSize = 10;
                var result = request.Execute();
                var driveFiles = result.Files.ToList();
                Directory.CreateDirectory("Budgets");
                var localFiles = Directory.EnumerateFiles(_path, "*.sqlite").ToList();
                foreach (var file in localFiles)
                {
                    var fileName = Path.GetFileName(file);
                    if (driveFiles.Any(x => x.Name.Equals(fileName) && File.GetLastWriteTime(file) > x.ModifiedTime))
                    {
                        //UPDATE EXISTING ON DRIVE
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            _budgetList.Add(new Budget(fileName, Path.GetFullPath(file), Path.GetFileNameWithoutExtension(file)));
                            var budget = _budgetList.First(x => x.Name.Equals(fileName));
                            budget.LoadAble = false;
                            budget.State = "Uploading to drive";
                            ListViewBudgets.ItemsSource = _budgetList;
                            ListViewBudgets.Items.Refresh();
                        }));

                        var body = new Google.Apis.Drive.v3.Data.File
                        {
                            Name = Path.GetFileName(file),
                            Description = "QuickBudget Database",
                            MimeType = ".sqlite database",
                            ModifiedTime = File.GetLastWriteTime(file)
                        };
                        var byteArray = File.ReadAllBytes(file);
                        MemoryStream stream = new MemoryStream(byteArray);
                        string id = result.Files.First(x => x.Name.Equals(body.Name)).Id;
                        FilesResource.UpdateMediaUpload req = service.Files.Update(body, id, stream, GetMimeType(Path.GetFileName(file)));
                        req.Fields = "id";
                        req.Upload();
                        IUploadProgress progress = req.GetProgress();
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            Budget budget = _budgetList.First(x => x.Name.Equals(fileName));
                            budget.LoadAble = true;
                            budget.State = "Ready";
                            ListViewBudgets.ItemsSource = _budgetList;
                            ListViewBudgets.Items.Refresh();
                        }));
                    }
                    else if (!driveFiles.Any(x => x.Name.Equals(fileName)))
                    {
                        //UPLOAD NON-EXISTING TO DRIVE
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            _budgetList.Add(new Budget(fileName, Path.GetFullPath(file), Path.GetFileNameWithoutExtension(file)));
                            var budget = _budgetList.First(x => x.Name.Equals(fileName));
                            budget.LoadAble = false;
                            budget.State = "Uploading to drive";
                            ListViewBudgets.ItemsSource = _budgetList;
                            ListViewBudgets.Items.Refresh();
                        }));
                        var body = new Google.Apis.Drive.v3.Data.File
                        {
                            Name = Path.GetFileName(file),
                            Parents = new List<string>() { "appDataFolder" },
                            Description = "QuickBudget Database",
                            MimeType = ".sqlite database",
                            ModifiedTime = File.GetLastWriteTime(file)
                        };
                        var byteArray = File.ReadAllBytes(file);
                        MemoryStream stream = new MemoryStream(byteArray);

                        FilesResource.CreateMediaUpload req = service.Files.Create(body, stream, GetMimeType(Path.GetFileName(file)));
                        req.Fields = "id";
                        req.Upload();
                        IUploadProgress progress = req.GetProgress();
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            var budget = _budgetList.First(x => x.Name.Equals(fileName));
                            budget.LoadAble = true;
                            budget.State = "Ready";
                            ListViewBudgets.ItemsSource = _budgetList;
                            ListViewBudgets.Items.Refresh();
                        }));
                    }
                }
            }
            catch (Exception exception) when (exception is Google.GoogleApiException || exception is HttpRequestException)
            {
                bool retry = false;
                Dispatcher.Invoke((Action)(() =>
                {
                    YesNo yesNo = new YesNo("Quickbudget failed to connect to Google Drive. Do you want to try again?") { Owner = GetWindow(this) };
                    yesNo.ShowDialog();
                    if (yesNo.IsSuccesful)
                    {
                        retry = true;
                    }
                }));
                if (retry)
                {
                    UploadAll();
                }
                else
                {
                    //TODO load local
                }


            }
        }


        private readonly string _path;

        private readonly List<Budget> _budgetList = new List<Budget>();

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewBudgets.SelectedItem == null) return;
            GetWindow(this).IsEnabled = false;
            var budget = (Budget) ListViewBudgets.SelectedItem;
            var mainWindow = new MainWindow(budget.Name, budget.NameWithoutExtension, _currencies);
            mainWindow.Show();
            Close();

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var newBudget = new NewBudget(_currencies) {Owner = GetWindow(this)};
            newBudget.ShowDialog();
            Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewBudgets.SelectedItem != null)
            {
                var budget = (Budget)ListViewBudgets.SelectedItem;
                string name = budget.Path;
                YesNo yesNo = new YesNo($"Do you want to delete {budget.Name}?") {Owner = GetWindow(this)};
                yesNo.ShowDialog();
                if (yesNo.IsSuccesful)
                {
                    try
                    {
                        File.Delete(name);

                        //
                        UserCredential credential;

                        using (var stream =
                            new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                        {
                            string credPath = System.Environment.GetFolderPath(
                                System.Environment.SpecialFolder.Personal);
                            credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                        }

                        // Create Drive API service.
                        var service = new DriveService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = ApplicationName,
                        });

                        // Define parameters of request.
                        FilesResource.ListRequest listRequest = service.Files.List();
                        listRequest.PageSize = 10;
                        listRequest.Fields = "nextPageToken, files(id, name)";
                        var request = service.Files.List();
                        request.Spaces = "appDataFolder";
                        request.Fields = "nextPageToken, files(id, name)";
                        request.PageSize = 10;
                        var result = request.Execute();
                        var driveFiles = result.Files.ToList();
                        Directory.CreateDirectory("Budgets");
                        service.Files.Delete(driveFiles.First(x => x.Name.Equals(budget.Name)).Id).Execute();
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException)
                        {
                            MessageBox.Show($"Failed to delete {budget.Name}, check if it isnt opened in another program");

                        }

                    }
                }
            }
            _budgetList.Clear();
            RefreshDatabases();
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        private void SelectivelyIgnoreMousebutton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void ListViewBudgets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListViewBudgets.SelectedItem != null && CheckLoadable())
            {
                GetWindow(this).IsEnabled = false;
                Budget budget = (Budget)ListViewBudgets.SelectedItem;
                MainWindow mainWindow = new MainWindow(budget.Name, budget.NameWithoutExtension, _currencies);
                mainWindow.Show();
                Close();
            }
        }

        private void ListViewBudgets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckLoadable();
        }

        private bool CheckLoadable()
        {
            if (_budgetList.Any())
            {
                var row =
                    (DataGridRow) ListViewBudgets.ItemContainerGenerator.ContainerFromItem(ListViewBudgets.SelectedItem);
                if (row != null)
                {
                    bool loadable = _budgetList[row.GetIndex()].LoadAble;
                    if (ListViewBudgets.SelectedItem != null && loadable)
                    {
                        button.IsEnabled = true;
                        return true;
                    }
                    else
                    {
                        button.IsEnabled = false;
                        return false;
                    }
                }
            }
            return false;
        }



        private void ButtonSynchronize_Click(object sender, RoutedEventArgs e)
        {
            UploadAll();
            DownloadAll();
            RefreshDatabases();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {

        }

    }

    class Budget
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string NameWithoutExtension { get; set; }
        public string State { get; set; } = "Ready";
        public bool LoadAble { get; set; } = true;
        public Budget(string name, string path, string nameWithoutExtension)
        {
            Name = name;
            Path = path;
            NameWithoutExtension = nameWithoutExtension;
        }
    }
}
