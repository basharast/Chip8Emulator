using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.Brushes;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI.Core;
using Windows.System;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Popups;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Windows.Storage.AccessCache;
using Windows.Storage.Search;
using System.IO;
using System.Collections.ObjectModel;

namespace UWPChip8
{
    /// <summary>
    /// Our main draw page
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// An image that we consider to be the pixel 'on' color, ie. a pixel byte of 1.
        /// </summary>
        CanvasImageBrush _brush;

        /// <summary>
        /// An image that we consider to be the pixel 'off' color, ie. a pixel byte of 0.
        /// </summary>
        CanvasImageBrush _darkBrush;

        /// <summary>
        /// Reference to our emulator that handles the ticks
        /// </summary>
        static Emulator _emulator;

        /// <summary>
        /// Pixel density. A representation of a pixel with a given width/height. Use for scaling to resolutions
        /// ie. default Chip 8 is 64x32, so for 640x320 the pixel size should be 10.
        /// </summary>
        int pixelSize = 10;
        int MonitorSpan = 2;
        int MonitorSpanTemp = 2;
        double DefaultKeyBoardScale = 200;
        double DefaultScreenScale = 0.7;
        List<RepeatButton> Keys = new List<RepeatButton>();
        bool isInitializeProgress = true;
        static bool SoundActive = true;
        string audioTune = "long1";
        static bool isSoundActive
        {
            get
            {
                return SoundActive;
            }
            set
            {
                SoundActive = value;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("sound", value);
            }
        }
        bool? soundState
        {
            get
            {
                return isSoundActive;
            }
            set
            {
                isSoundActive = (bool)value;
            }
        }
        /// <summary>
        /// The constructor for this instance of MainPage
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            try
            {
                BigTitle.Text = $"CHIP-8 EMULATOR {GetAppVersion()}";
            }
            catch (Exception ex)
            {

            }
            try
            {
                HideWelcomeEvent += HideWelcome;
            }
            catch (Exception ex)
            {

            }
            try
            {
                var mscale = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("mscale", 0.7);
                DefaultScreenScale = mscale;
            }
            catch (Exception ex)
            {

            }
            try
            {
                var kscale = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("kscale", 200);
                DefaultKeyBoardScale = kscale;
            }
            catch (Exception ex)
            {

            } 
            
            try
            {
                ActionsTransformXCurrent = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ActionsTransformXCurrent", ActionsTransformXCurrent);
                ActionsTransformYCurrent = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("ActionsTransformYCurrent", ActionsTransformYCurrent);
            }
            catch (Exception ex)
            {

            }
            SetDefaultZoom();
            AdjustScreen();
            try
            {
                defaultBrush = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("brush", defaultBrush);
            }
            catch (Exception ex)
            {

            }
            try
            {
                isSoundActive = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("sound", true);
            }
            catch (Exception ex)
            {

            }
            try
            {
                audioTune = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("tune", audioTune);
            }
            catch (Exception ex)
            {

            }
            _emulator = new Emulator(); // This could be moved to CreateResources if there were any asset dependencies.

            try
            {
                _emulator.SetAudioTune(audioTune);
            }
            catch(Exception ex)
            {

            }
            try
            {
                var keyboardState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("keyboard", true);
                ShowKeyBoard.IsChecked = keyboardState;
            }
            catch (Exception ex)
            {

            }
            try
            {
                var keyboardPCState = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("keyboardpc", false);
                ShowKeyBoardPC.IsChecked = keyboardPCState;
                if (keyboardPCState)
                {
                    ButtonsPCMode();
                }
                else
                {
                    ButtonsOriginalMode();
                }
            }
            catch (Exception ex)
            {

            }
            try
            {
                MonitorSpan = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("extend", 2);
                MonitorSpanTemp = MonitorSpan;
                MonitorSpanButton.IsChecked = (MonitorSpan == 2);
                this.Bindings.Update();
            }
            catch (Exception ex)
            {

            }

            try
            {
                Keys.Add(B1);
                Keys.Add(B2);
                Keys.Add(B3);
                Keys.Add(B4C);
                Keys.Add(B54);
                Keys.Add(B65);
                Keys.Add(B76);
                Keys.Add(B8D);
                Keys.Add(B97);
                Keys.Add(B108);
                Keys.Add(B119);
                Keys.Add(B12E);
                Keys.Add(B13A);
                Keys.Add(B140);
                Keys.Add(B15B);
                Keys.Add(B16F);
            }
            catch (Exception ex)
            {

            }
            callConnectionTimer(true);
            try
            {
                GamesList.ItemsSource = GamesArray;
            }
            catch (Exception ex)
            {

            }
            HideGamesList += HideGamesListCall;
            GetGames();
           
            Window.Current.SizeChanged += (sender, args) =>
            {
                AdjustScreen();
            };
            isInitializeProgress = false;
        }

        private void ButtonsPCMode()
        {
            B1.Content = "1";
            B2.Content = "2";
            B3.Content = "3";
            B4C.Content = "4";
            B54.Content = "Q";
            B65.Content = "W";
            B76.Content = "E";
            B8D.Content = "R";
            B97.Content = "A";
            B108.Content = "S";
            B119.Content = "D";
            B12E.Content = "F";
            B13A.Content = "Z";
            B140.Content = "X";
            B15B.Content = "C";
            B16F.Content = "V";
        }
        private void ButtonsOriginalMode()
        {
            B1.Content = "1";
            B2.Content = "2";
            B3.Content = "3";
            B4C.Content = "C";
            B54.Content = "4";
            B65.Content = "5";
            B76.Content = "6";
            B8D.Content = "D";
            B97.Content = "7";
            B108.Content = "8";
            B119.Content = "9";
            B12E.Content = "E";
            B13A.Content = "A";
            B140.Content = "0";
            B15B.Content = "B";
            B16F.Content = "F";
        }
        private async void SetDefaultZoom()
        {
            try
            {
                await Task.Delay(2000);
                MonitorContainer.ChangeView(0, 0, (float?)MonitorScale.Value);
                this.Bindings.Update();
            }
            catch (Exception ex)
            {

            }
        }

        private void AdjustScreen()
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                if (ShowKeyBoard.IsChecked && height > width)
                {
                    animatedControl.VerticalAlignment = VerticalAlignment.Top;
                    animatedControl.Margin = new Thickness(0, 35, 0, 0);
                    MonitorSpan = 2;
                    MonitorSpanButton.IsChecked = (MonitorSpan == 2);
                }
                else
                {
                    MonitorSpan = MonitorSpanTemp;
                    MonitorSpanButton.IsChecked = (MonitorSpan == 2);
                    animatedControl.VerticalAlignment = VerticalAlignment.Stretch;
                    animatedControl.Margin = new Thickness(0, 0, 0, 0);
                }
                this.Bindings.Update();
            }
            catch(Exception ex)
            {

            }
        }
        /// <summary>
        /// Handle the Control loaded event for this page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            // Register for keyboard events
            Window.Current.CoreWindow.KeyDown += KeyDown_UIThread;
            Window.Current.CoreWindow.KeyUp += KeyUp_UIThread;
            chooseROMButton.Click += new RoutedEventHandler(ChooseROM_Click);
        }

        /// <summary>
        /// Handle the Control unloaded event for this page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= KeyDown_UIThread;
            Window.Current.CoreWindow.KeyUp -= KeyUp_UIThread;

            animatedControl.RemoveFromVisualTree();
            animatedControl = null;
        }

        /// <summary>
        /// Handle the key up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void KeyUp_UIThread(CoreWindow sender, KeyEventArgs args)
        {
            char pressedLetter = GetPressedLetter(args);

            if (pressedLetter == 0)
                return;

            args.Handled = true;

            var action = animatedControl.RunOnGameLoopThreadAsync(() => _emulator.ProcessKey(pressedLetter, 0));
        }

        /// <summary>
        /// Handle the key down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void KeyDown_UIThread(CoreWindow sender, KeyEventArgs args)
        {
            char pressedLetter = GetPressedLetter(args);

            if (pressedLetter == 0)
            {
                return;
            }

            args.Handled = true;

            var action = animatedControl.RunOnGameLoopThreadAsync(() => _emulator.ProcessKey(pressedLetter, 1));
        }

        /// <summary>
        /// Converts virtual keys into chars. This is taken from an example on keyboard handling in Win2D
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static char GetPressedLetter(KeyEventArgs args)
        {
            var key = args.VirtualKey;
            char pressed = (char)0;

            switch (key)
            {
                case VirtualKey.Number1: pressed = '1'; break;
                case VirtualKey.Number2: pressed = '2'; break;
                case VirtualKey.Number3: pressed = '3'; break;
                case VirtualKey.Number4: pressed = '4'; break;
                case VirtualKey.Number6: pressed = '6'; break;
                case VirtualKey.Number7: pressed = '7'; break;
                case VirtualKey.Number8: pressed = '8'; break;
                case VirtualKey.Number9: pressed = '9'; break;
                case VirtualKey.Number0: pressed = '0'; break;
                case VirtualKey.A: pressed = 'A'; break;
                case VirtualKey.C: pressed = 'C'; break;
                case VirtualKey.D: pressed = 'D'; break;
                case VirtualKey.E: pressed = 'E'; break;
                case VirtualKey.F: pressed = 'F'; break;
                case VirtualKey.Q: pressed = 'Q'; break;
                case VirtualKey.R: pressed = 'R'; break;
                case VirtualKey.S: pressed = 'S'; break;
                case VirtualKey.V: pressed = 'V'; break;
                case VirtualKey.W: pressed = 'W'; break;
                case VirtualKey.X: pressed = 'X'; break;
                case VirtualKey.Z: pressed = 'Z'; break;
            }

            return pressed;
        }
        private static char GetPressedLetter(string key)
        {
            char pressed = (char)0;
            switch (key)
            {
                case "1": pressed = '1'; break;
                case "2": pressed = '2'; break;
                case "3": pressed = '3'; break;
                case "4": pressed = '4'; break;
                case "5": pressed = '5'; break;
                case "6": pressed = '6'; break;
                case "7": pressed = '7'; break;
                case "8": pressed = '8'; break;
                case "9": pressed = '9'; break;
                case "0": pressed = '0'; break;
                case "A": pressed = 'A'; break;
                case "C": pressed = 'C'; break;
                case "D": pressed = 'D'; break;
                case "E": pressed = 'E'; break;
                case "F": pressed = 'F'; break;
                case "Q": pressed = 'Q'; break;
                case "R": pressed = 'R'; break;
                case "S": pressed = 'S'; break;
                case "V": pressed = 'V'; break;
                case "W": pressed = 'W'; break;
                case "X": pressed = 'X'; break;
                case "Z": pressed = 'Z'; break;
            }

            return pressed;
        }
        /// <summary>
        /// Our 60hz update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CanvasAnimatedControl_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            try
            {
                if (!_emulator.PoweredUp)
                    return;

                _emulator.ExecuteIteration();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Draws the 'pixel' blocks stored in the CPUs display buffer at a density of _pixelSize;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CanvasAnimatedControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            try
            {
                if (!_emulator.PoweredUp)
                    return;

                bool[] displayBuffer = _emulator.DisplayBuffer;
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        if (displayBuffer[(y * 64) + x] != false)
                        {
                            args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x * pixelSize, y * pixelSize, pixelSize, pixelSize), _brush);
                        }
                        else
                        {
                            args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x * pixelSize, y * pixelSize, pixelSize, pixelSize), _darkBrush);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Handle the Create Resources event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CanvasAnimatedControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            // Ensure the CreateResources task is completed before considering that the internal create resources is itself completed.
            args.TrackAsyncAction(CreateResources(sender).AsAsyncAction());
        }


        Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl senderGlobal;
        string defaultBrush = "Assets/brush.jpg";
        /// <summary>
        /// Task to load our resources
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        async Task CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender)
        {
            try
            {
                senderGlobal = sender;
                _darkBrush = new CanvasImageBrush(sender);
                _darkBrush.Image = await CanvasBitmap.LoadAsync(sender, "Assets/dark_brush.jpg");
                _brush = new CanvasImageBrush(sender);
                _brush.Image = await CanvasBitmap.LoadAsync(sender, defaultBrush);
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        /// <summary>
        /// Handles the file picker event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async static void LoadFileFromOpen(StorageFile openFile)
        {
            try
            {
                if (openFile != null)
                {
                    fileTemp = openFile;
                    _emulator.Initialize(isSoundActive);
                    await _emulator.LoadRom(openFile);
                    HideWelcomeEvent.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {

            }
        }

        bool calledByGamesList = false;
        public static EventHandler HideWelcomeEvent;
        private void HideWelcome(object sender, EventArgs args)
        {
            BlankPage2.Visibility = Visibility.Collapsed;
            if (ShowKeyBoard.IsChecked && !calledByGamesList)
            {
                KeyBoardGrid.Visibility = Visibility.Visible;
            }
            calledByGamesList = false;
        }

        static StorageFile fileTemp;
        private async void ChooseROM_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                openPicker.FileTypeFilter.Add(".ch8");
                StorageFile file = await openPicker.PickSingleFileAsync();
                fileTemp = file;
                if (file != null)
                {
                    _emulator.Initialize(isSoundActive);
                    await _emulator.LoadRom(file);

                    HideWelcomeEvent.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {

            }
        }


        string tempButton = "";
        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                char pressedLetter = GetPressedLetter((string)((RepeatButton)sender).Tag);
                var action = animatedControl.RunOnGameLoopThreadAsync(() => _emulator.ProcessKey(pressedLetter, 1));
            }
            catch (Exception ex)
            {

            }

        }

        string[] buttons = new string[] { "1", "2", "3", "4", "Q", "W", "E", "R", "A", "S", "D", "F", "Z", "X", "C", "V" };
        Timer ButtonsTimer;
        bool isButtonsTimerInProgress = false;
        private void callConnectionTimer(bool startState = false)
        {
            try
            {
                ButtonsTimer?.Dispose();
                if (startState)
                {
                    ButtonsTimer = new Timer(async delegate { if (isButtonsTimerInProgress) { return; } else { CheckForPressedButtons(); } }, null, 0, 100);
                }
            }
            catch (Exception e)
            {
            }
        }
        private async void CheckForPressedButtons()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    if (!ShowKeyBoard.IsChecked || _emulator == null || !_emulator.PoweredUp)
                    {
                        return;
                    }
                    isButtonsTimerInProgress = true;
                    try
                    {
                        foreach (var buttonItem in Keys)
                        {
                            try
                            {
                                char pressedLetter = GetPressedLetter((string)buttonItem.Tag);
                                if (!buttonItem.IsPressed && _emulator.GetKeyState(pressedLetter) > 0)
                                {
                                    var action = animatedControl.RunOnGameLoopThreadAsync(() => _emulator.ProcessKey(pressedLetter, 0));
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    isButtonsTimerInProgress = false;
                }
                catch (Exception ex)
                {

                }
            });
        }
        private void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
        {
            try
            {
                int count = VisualTreeHelper.GetChildrenCount(startNode);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                    if ((current.GetType()).Equals(typeof(T)) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                    {
                        T asType = (T)current;
                        results.Add(asType);
                    }
                    FindChildren<T>(results, current);
                }
            }
            catch (Exception e)
            {

            }
        }
        private void RepeatButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                char pressedLetter = GetPressedLetter((string)((RepeatButton)sender).Tag);
                var action = animatedControl.RunOnGameLoopThreadAsync(() => _emulator.ProcessKey(pressedLetter, 0));
            }
            catch (Exception ex)
            {

            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            string message = $"Chip8 Emulator {GetAppVersion()}\nCreated by Jody Andrews\nEnhanced by Bashar Astifan\nThanks to Mario Perić\nAlso thanks to Danny Tuppeny\n\nGitHub: https://github.com/JodyAndrews/UWPChip8";
            ShowDialog(message);
        }
        private async void ShowDialog(string message)
        {
            var messageDialog = new MessageDialog(message);
            messageDialog.Commands.Add(new UICommand(
                "Close"));
            await messageDialog.ShowAsync();
        }
        private async void ShowDialog(Exception ex)
        {
            var messageDialog = new MessageDialog(ex.Message);
            messageDialog.Commands.Add(new UICommand(
                "Close"));
            await messageDialog.ShowAsync();
        }
        public static string GetAppVersion()
        {
            try
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
            catch (Exception ex)
            {

            }
            return "1.0.4.0";
        }
        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (fileTemp != null)
                {
                    _emulator.Initialize(isSoundActive);
                    await _emulator.LoadRom(fileTemp);
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void MonitorScale_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isInitializeProgress)
            {
                return;
            }
            try
            {
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("mscale", e.NewValue);
                MonitorContainer.ChangeView(0, 0, (float?)e.NewValue);
                DefaultScreenScale = e.NewValue;
            }
            catch (Exception ex)
            {

            }
        }

        private void MonitorSpan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MonitorSpanButton.IsChecked.Value)
                {
                    MonitorSpan = 2;
                }
                else
                {
                    MonitorSpan = 1;
                }
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("extend", MonitorSpan);
                MonitorSpanTemp = MonitorSpan;
                this.Bindings.Update();
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void KeyBoardScale_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isInitializeProgress)
            {
                return;
            }
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("kscale", (int)e.NewValue);
            DefaultKeyBoardScale = e.NewValue;
        }

        private void ShowKeyBoard_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("keyboard", ShowKeyBoard.IsChecked);
            AdjustScreen();
        }
        private void ShowKeyBoardPC_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("keyboardpc", ShowKeyBoardPC.IsChecked);
            if (ShowKeyBoardPC.IsChecked)
            {
                ButtonsPCMode();
            }
            else
            {
                ButtonsOriginalMode();
            }
        }

        public string RememberFile(StorageFolder file)
        {
            string token = Guid.NewGuid().ToString();
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
            Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GamesFolder", token);
            return token;
        }
        public async Task<StorageFolder> GetFileForToken(string token)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(token)) return null;
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
        }

        public static bool GamesListVisible = false;
        public static EventHandler HideGamesList;
        private void HideGamesListCall(object sender, EventArgs args)
        {
            try
            {
                GamesGrid.Visibility = Visibility.Collapsed;
                GamesListVisible = false;
                MonitorSpan = MonitorSpanTemp;
                MonitorSpanButton.IsChecked = (MonitorSpan == 2);
                if (ShowKeyBoard.IsChecked)
                {
                    KeyBoardGrid.Visibility = Visibility.Visible;
                }
                this.Bindings.Update();
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }
        private void ShowGamesListCall()
        {
            try
            {
                GamesGrid.Visibility = Visibility.Visible;
                GamesListVisible = true;
                MonitorSpan = 1;
                MonitorSpanButton.IsChecked = (MonitorSpan == 2);
                KeyBoardGrid.Visibility = Visibility.Collapsed;
                this.Bindings.Update();
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }
        StorageFolder GamesFolder = null;
        ObservableCollection<GameItem> GamesArray = new ObservableCollection<GameItem>();
        int TotalGames = 0;
        private async void GetGames()
        {
            GetGamesButton.IsEnabled = false;
            GamesLoading.Visibility = Visibility.Visible;
            if (!GamesListVisible)
            {
                GamesLoadingGlobal.Visibility = Visibility.Visible;
            }
            try
            {
                if (GamesFolder == null)
                {
                    var fileToken = Plugin.Settings.CrossSettings.Current.GetValueOrDefault("GamesFolder", "");
                    if (fileToken.Length > 0)
                    {
                        GamesFolder = await GetFileForToken(fileToken);
                    }
                }

                if (GamesFolder == null)
                {
                    GamesFolder = await GetLocalFolder();
                    GamesLocation.Text = "Built-in ROMs";
                }
                else if (GamesFolder != null)
                {
                    GamesLocation.Text = $"ROMs from ({GamesFolder.Name})";
                }
                {
                    if (GamesFolder != null)
                    {
                        GamesArray.Clear();
                        TotalGames = 0;

                        List<string> fileTypeFilter = new List<string>();
                        fileTypeFilter.Add(".ch8");
                        QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);
                        queryOptions.FolderDepth = FolderDepth.Deep;
                        queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
                        var files = GamesFolder.CreateFileQueryWithOptions(queryOptions);
                        var folderFiles = await files.GetFilesAsync();
                        {
                            foreach (var fItem in folderFiles)
                            {
                                try
                                {
                                    var fileName = Path.GetFileNameWithoutExtension(fItem.Name);
                                    var folderName = Path.GetFileName(Path.GetDirectoryName(fItem.Path));
                                    string key = $"{fileName} ({folderName})";
                                    GameItem gameItem = new GameItem(key, fItem);
                                    GamesArray.Add(gameItem);
                                    TotalGames++;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
            if (TotalGames == 0)
            {
                BlankPage.Visibility = Visibility.Visible;
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("GamesFolder", "");
            }
            else
            {
                BlankPage.Visibility = Visibility.Collapsed;
            }
            GamesLoading.Visibility = Visibility.Collapsed;
            if (!GamesListVisible)
            {
                GamesLoadingGlobal.Visibility = Visibility.Collapsed;
            }
            GetGamesButton.IsEnabled = true;
        }
        private async Task<StorageFolder> GetLocalFolder()
        {
            try
            {
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var GamesF = (StorageFolder)await appInstalledFolder.TryGetItemAsync($"Chip8");
                return GamesF;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void GamesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                GameItem key = (GameItem)e.ClickedItem;
                StorageFile gameFile = key.gameFile;
                calledByGamesList = true;
                LoadFileFromOpen(gameFile);
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            if (GamesListVisible)
            {
                HideGamesListCall(null, EventArgs.Empty);
            }
            else
            {
                ShowGamesListCall();
            }
        }

        private async void GetGamesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderPicker selectWpFolder = new FolderPicker();
                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder GamesFolderTest = await folderPicker.PickSingleFolderAsync();
                if (GamesFolderTest != null)
                {
                    GamesFolder = GamesFolderTest;
                    RememberFile(GamesFolder);
                    GetGames();
                }
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private async void GreenColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _brush.Image = await CanvasBitmap.LoadAsync(senderGlobal, "Assets/brush.jpg");
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("brush", "Assets/brush.jpg");
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private async void RedColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _brush.Image = await CanvasBitmap.LoadAsync(senderGlobal, "Assets/brush2.jpg");
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("brush", "Assets/brush2.jpg");
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private async void BlueColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _brush.Image = await CanvasBitmap.LoadAsync(senderGlobal, "Assets/brush3.jpg");
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("brush", "Assets/brush3.jpg");
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private async void WhiteColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _brush.Image = await CanvasBitmap.LoadAsync(senderGlobal, "Assets/brush4.jpg");
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("brush", "Assets/brush4.jpg");
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private async void OrangeColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _brush.Image = await CanvasBitmap.LoadAsync(senderGlobal, "Assets/brush5.jpg");
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("brush", "Assets/brush5.jpg");
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void Profile1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tune = "long0";
                _emulator.SetAudioTune(tune);
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("tune", tune);
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void Profile2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tune = "long1";
                _emulator.SetAudioTune(tune);
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("tune", tune);
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void Profile3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tune = "long2";
                _emulator.SetAudioTune(tune);
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("tune", tune);
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void Profile4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tune = "long3";
                _emulator.SetAudioTune(tune);
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("tune", tune);
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        private void Profile5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tune = "long4";
                _emulator.SetAudioTune(tune);
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("tune", tune);
            }
            catch (Exception ex)
            {
                ShowDialog(ex);
            }
        }

        double ActionsTransformXCurrent = 0.0;
        double ActionsTransformYCurrent = 0.0;
        bool AdjustMode = false;
        private void ActionsControlSetup(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (!AdjustMode) return;
                var PositionX = e.Delta.Translation.X;
                var PositionY = e.Delta.Translation.Y;
                ActionsTransformXCurrent += PositionX;/// (KeyBoardScale.Value / 200);
                ActionsTransformYCurrent += PositionY;/// (KeyBoardScale.Value / 200);
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ActionsTransformXCurrent", ActionsTransformXCurrent);
                Plugin.Settings.CrossSettings.Current.AddOrUpdateValue("ActionsTransformYCurrent", ActionsTransformYCurrent);
                this.Bindings.Update();
            }
            catch (Exception ex)
            {

            }
        }

    }
    class GameItem
    {
        public string gameIcon = "ms-appx:///Assets/fileIconS.png";
        public string gameTitle;
        public StorageFile gameFile;
        public GameItem(string title, StorageFile storageFile)
        {
            gameTitle = title;
            gameFile = storageFile;
        }
    }
}
