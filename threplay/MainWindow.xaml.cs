using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Ookii.Dialogs.Wpf;

namespace threplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum GameList
    {
        th06,
        th07,
        th08,
        th09,
        th095,
        th10,
        th11,
        th12,
        th123,
        th125,
        th128,
        th13,
        th135,
        th14,
        th143,
        th145,
        th15,
        th155,
        th16,
        th165,

        thLast
    };
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Bluegrams.Application.PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);

            GameHandler.gameListView = modeGameSelector;
            GameHandler.replayLiveView = oReplayLiveList;
            GameHandler.replayBackupView = oReplayBackupList;
            GameHandler.liveDir = iDirLive;
            GameHandler.backupDir = iDirBackup;
            GameHandler.InitializeGames();
            GameHandler.gameListView.SelectedIndex = 0;
        }

        private void FnLaunchGame_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.LaunchGame();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Tab)
            {
                e.Handled = true;
            } else if(e.Key == Key.Escape)
            {
                oReplayLiveList.SelectedIndex = -1;
                oReplayBackupList.SelectedIndex = -1;
                e.Handled = true;
            } else if(e.Key == Key.Enter)
            {
                //some weird ass fucking shit about to happen here
                if(odFileNameLive.IsFocused)
                {
                    //user hit enter after renaming
                    //probably want to put like an "are you sure" box or something i guess?
                    //something something dialoghost
                    if(oReplayLiveList.SelectedIndex != -1)
                    {
                        ReplayEntry replayEntry = (ReplayEntry)oReplayLiveList.SelectedItem;
                        FileInfo replayFile = new FileInfo(replayEntry.FullPath);
                        string oldFileName = replayFile.FullName;
                        string newFileName = replayFile.DirectoryName + "\\" + odFileNameLive.Text + ".rpy";
                        File.Move(oldFileName, newFileName);
                        GameHandler.LoadLive();
                    }
                } else if(odFileNameBackup.IsFocused)
                {
                    if(oReplayBackupList.SelectedIndex != -1)
                    {
                        ReplayEntry replayEntry = (ReplayEntry)oReplayBackupList.SelectedItem;
                        FileInfo replayFile = new FileInfo(replayEntry.FullPath);
                        string oldFileName = replayFile.FullName;
                        string newFileName = replayFile.DirectoryName + "\\" + odFileNameLive.Text + ".rpy";
                        File.Move(oldFileName, newFileName);
                        GameHandler.LoadBackup();
                    }
                }
            }
        }

        private void IDirLive_GotFocus(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            if ((bool)dialog.ShowDialog(this))
            {
                iDirLive.Text = System.IO.Path.GetDirectoryName(dialog.FileName);
                GameHandler.SetLive(dialog.FileName);
                GameHandler.LoadLive();
            }
            GameHandler.CheckMove(out bool hasGame, out bool hasBackup);
            fnLaunchGame.IsEnabled = hasGame;
            fnLaunchFolder.IsEnabled = hasGame;
            fnTransferToBackup.IsEnabled = hasGame && hasBackup;
            fnTransferToLive.IsEnabled = hasGame && hasBackup;
            if (hasGame && hasBackup)
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderOpen;
                oMessage.IsActive = false;
                iViewDir.IsExpanded = false;
            }
            else
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderSearchOutline;
                if (hasGame && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select a backup folder" };
                    oMessage.IsActive = true;
                }
                else if (!hasGame && hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please browse to your game exe" };
                    oMessage.IsActive = true;
                }
                else if (!hasGame && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select your game exe and backup folders" };
                    oMessage.IsActive = true;
                }
            }
        }

        private void IDirBackup_GotFocus(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog(this))
            {
                iDirBackup.Text = dialog.SelectedPath;
                GameHandler.SetBackup(dialog.SelectedPath);
                GameHandler.LoadBackup();
            }
            GameHandler.CheckMove(out bool hasGame, out bool hasBackup);
            fnLaunchGame.IsEnabled = hasGame;
            fnLaunchFolder.IsEnabled = hasGame;
            fnTransferToBackup.IsEnabled = hasGame && hasBackup;
            fnTransferToLive.IsEnabled = hasGame && hasBackup;
            if (hasGame && hasBackup)
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderOpen;
                oMessage.IsActive = false;
                iViewDir.IsExpanded = false;
            }
            else
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderSearchOutline;
                if (hasGame && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select a backup folder" };
                    oMessage.IsActive = true;
                }
                else if (!hasGame && hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please browse to your game exe" };
                    oMessage.IsActive = true;
                }
                else if (!hasGame && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select your game exe and backup folders" };
                    oMessage.IsActive = true;
                }
            }
        }

        private void FnTransferToLive_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.FileMove(false, (bool)fnTypeMove.IsChecked);
            GameHandler.LoadLive();
            GameHandler.LoadBackup();
        }

        private void FnTransferToBackup_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.FileMove(true, (bool)fnTypeMove.IsChecked);
            GameHandler.LoadLive();
            GameHandler.LoadBackup();
        }

        private void ModeGameViewToggle_MouseLeave(object sender, MouseEventArgs e)
        {
            modeGameViewToggle.Width = 65;
        }

        private void ModeGameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GameHandler.UpdateCurrentGame(out bool hasGame, out bool hasBackup, ref oCurText, ref iDirLive, ref iDirBackup);
            fnLaunchGame.IsEnabled = hasGame;
            fnLaunchFolder.IsEnabled = hasGame;
            fnTransferToBackup.IsEnabled = hasGame && hasBackup;
            fnTransferToLive.IsEnabled = hasGame && hasBackup;
            if(hasGame && hasBackup)
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderOpen;
                oMessage.IsActive = false;
            } else
            {
                iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderSearchOutline;
                if(hasGame && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select a backup folder" };
                    oMessage.IsActive = true;
                } else if(!hasGame && hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please browse to your game exe" };
                    oMessage.IsActive = true;
                } else if(!hasGame && !hasBackup)
                {
                    oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = "Please select your game exe and backup folders" };
                    oMessage.IsActive = true;
                }
            }
        }

        private void ModeGameSelector_MouseEnter(object sender, MouseEventArgs e)
        {
            modeGameViewToggle.Width = 250;
        }

        private void FnLaunchFolder_Click(object sender, RoutedEventArgs e)
        {
            GameHandler.OpenGameFolder();
        }

        private void IViewDir_Expanded(object sender, RoutedEventArgs e)
        {
            odFileGrid.Visibility = Visibility.Collapsed;
        }

        private void IViewDir_Collapsed(object sender, RoutedEventArgs e)
        {
            if(fnMultiEnabled.IsChecked == false)
            {
                odFileGrid.Visibility = Visibility.Visible;
            }
        }

        private void FnMultiEnabled_Checked(object sender, RoutedEventArgs e)
        {
            oReplayBackupList.SelectionMode = SelectionMode.Multiple;
            oReplayLiveList.SelectionMode = SelectionMode.Multiple;
            odFileGrid.Visibility = Visibility.Collapsed;
        }

        private void FnMultiEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            oReplayBackupList.SelectionMode = SelectionMode.Single;
            oReplayLiveList.SelectionMode = SelectionMode.Single;
            odFileGrid.Visibility = Visibility.Visible;

            if(oReplayLiveList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayLiveList.SelectedItem;
                odFileNameLive.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameLive.Focusable = true;
            }
            if(oReplayBackupList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayBackupList.SelectedItem;
                odFileNameBackup.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameBackup.Focusable = true;
            }
        }

        private void OReplayLiveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(fnMultiEnabled.IsChecked == false && oReplayLiveList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayLiveList.SelectedItem;
                odFileNameLive.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameLive.Focusable = true;
            } else
            {
                odFileNameLive.Text = "(disable multiselect to view file info)";
                odFileNameLive.Focusable = false;
            }
        }

        private void OReplayBackupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fnMultiEnabled.IsChecked == false && oReplayBackupList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayBackupList.SelectedItem;
                odFileNameBackup.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                odFileNameBackup.Focusable = true;
            } else
            {
                odFileNameBackup.Text = "(disable multiselect to view file info)";
                odFileNameBackup.Focusable = false;
            }
        }

        private void FnSettings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            
        }
    }

    public static class GameHandler
    {
        public static int currentGame;
        public static ListView gameListView = null;
        public static ListView replayLiveView = null;
        public static ListView replayBackupView = null;
        public static TextBox liveDir = null;
        public static TextBox backupDir = null;

        private static GameObject[] games;

        public static void SetLive(string path) { games[currentGame].SetLive(path);  }
        public static void SetBackup(string path) { games[currentGame].SetBackup(path); }
        public static void LoadLive() { games[currentGame].LoadLive(ref replayLiveView); }
        public static void LoadBackup() { games[currentGame].LoadBackup(ref replayBackupView); }
        public static void LaunchGame() { try { System.Diagnostics.Process.Start(games[currentGame].gameExe); } catch { } }
        public static void OpenGameFolder() { try { System.Diagnostics.Process.Start(games[currentGame].dirLive); } catch { } }
        public static ListViewItem GetGameListEntry(int i) { return games[i].listEntry; }
        public static void UpdateCurrentGame(out bool hasGame, out bool hasBackup, ref TextBlock title, ref TextBox live, ref TextBox backup)
        {
            currentGame = gameListView.SelectedIndex;
            title.Text = "Currently selected game: " + GameData.titles[currentGame];
            if (games[currentGame].dirBackup != "!")
            {
                backup.Text = games[currentGame].dirBackup;
                LoadBackup();
            } else
            {
                backup.Text = "click to browse for backup folder";
                List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                replayBackupView.ItemsSource = replayListBackup;
            }
            if (games[currentGame].dirLive != "!")
            {
                live.Text = games[currentGame].dirLive;
                LoadLive();
            } else
            {
                live.Text = "click to browse for game exe";
                List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                replayLiveView.ItemsSource = replayListLive;
            }

            CheckMove(out hasGame, out hasBackup);
        }

        public static void CheckMove(out bool hasGame, out bool hasBackup)
        {
            hasGame = games[currentGame].dirLive != "!" && games[currentGame].gameExe != "!" ? true : false;
            hasBackup = games[currentGame].dirBackup != "!" ? true : false;
        }

        public static void FileMove(bool toBackup, bool deleteOriginal)
        {
            if (games[currentGame].dirBackup != "!" && games[currentGame].dirLive != "!")
            {
                if (toBackup)
                {
                    foreach (ReplayEntry item in replayLiveView.SelectedItems)
                    {
                        //ReplayEntry replayItem = item.Content as ReplayEntry;
                        string source = System.IO.Path.Combine(games[currentGame].dirLive + "\\replay", item.Filename);
                        string dest = System.IO.Path.Combine(games[currentGame].dirBackup, item.Filename);
                        bool proceed = true;
                        if (System.IO.File.Exists(dest))
                        {
                            MessageBoxResult result = MessageBox.Show("The destination file \"" + dest + "\" already exists. Overwite?", "Info", MessageBoxButton.YesNo);
                            if(result == MessageBoxResult.No)
                            {
                                proceed = false;
                            }
                        }
                        if (proceed)
                        {
                            if (deleteOriginal)
                            {
                                System.IO.File.Move(source, dest);
                            }
                            else
                            {
                                System.IO.File.Copy(source, dest);
                            }
                        }
                    }
                }
                else
                {
                    foreach (ReplayEntry item in replayBackupView.SelectedItems)
                    {
                        //ReplayEntry replayItem = item.Content as ReplayEntry;
                        string source = System.IO.Path.Combine(games[currentGame].dirBackup, item.Filename);
                        string dest = System.IO.Path.Combine(games[currentGame].dirLive + "\\replay", item.Filename);
                        bool proceed = true;
                        if (System.IO.File.Exists(dest))
                        {
                            MessageBoxResult result = MessageBox.Show("The destination file \"" + dest + "\" already exists. Overwite?", "Info", MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.No)
                            {
                                proceed = false;
                            }
                        }
                        if (proceed)
                        {
                            if (deleteOriginal)
                            {
                                System.IO.File.Move(source, dest);
                            }
                            else
                            {
                                System.IO.File.Copy(source, dest);
                            }
                        }
                    }
                }
            }
        }

        public static void RegenerateGameList(string gameVisible)
        {
            Properties.Settings.Default["gameVisibility"] = gameVisible;
            Properties.Settings.Default.Save();
            for(int i = 0; i < (int)GameList.thLast; i++)
            {
                games[i].listEntry.Visibility = gameVisible[i] == 'Y' ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static bool InitializeGames()
        {
            if(gameListView != null)
            {
                games = new GameObject[(int)GameList.thLast];
                string gameVisible = (string)Properties.Settings.Default["gameVisibility"];
                for (int i = 0; i < (int)GameList.thLast; i++)
                {
                    games[i] = new GameObject(i);
                    games[i].listEntry.Visibility = gameVisible[i] == 'Y' ? Visibility.Visible : Visibility.Collapsed;
                    gameListView.Items.Add(games[i].listEntry);
                }
                return true;
            } else return false;
        }

        private class GameObject
        {
            public ListViewItem listEntry;
            public int number;
            public string dirLive, dirBackup, gameExe;

            private FileInfo[] replaysLive, replaysBackup;
            private DirectoryInfo dirLiveInfo, dirBackupInfo;

            public GameObject(int i)
            {
                number = i;
                CreateListEntry(i, ref listEntry);
                if((string)Properties.Settings.Default[GameData.setting[number] + "_l"] == "!")
                {
                    dirLive = "!";
                    gameExe = "!";
                } else
                {
                    dirLive = System.IO.Path.GetDirectoryName((string)Properties.Settings.Default[GameData.setting[number] + "_l"]);
                    gameExe = (string)Properties.Settings.Default[GameData.setting[number] + "_l"];
                }
                dirBackup = (string)Properties.Settings.Default[GameData.setting[number] + "_b"];
            }

            public void SetLive(string path)
            {
                dirLive = System.IO.Path.GetDirectoryName(path);
                gameExe = path;
                Properties.Settings.Default[GameData.setting[number] + "_l"] = gameExe;
                Properties.Settings.Default.Save();
            }

            public void SetBackup(string path)
            {
                dirBackup = path;
                Properties.Settings.Default[GameData.setting[number] + "_b"] = dirBackup;
                Properties.Settings.Default.Save();
            }

            public bool LoadLive(ref ListView list)
            {
                if (dirLive != "!")
                {
                    //check if there's a replay folder in this 
                    try
                    {
                        dirLiveInfo = new DirectoryInfo(dirLive + "\\replay");
                        replaysLive = dirLiveInfo.GetFiles("*.rpy");
                    }
                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        MessageBoxResult error = MessageBox.Show("Replay folder not detected. Check if you've selected" +
                            " a valid Touhou game executable.\nIf so, would you like to create the folder " +
                            dirLiveInfo.FullName + "?", "Error", MessageBoxButton.YesNo);
                        if (error == MessageBoxResult.Yes)
                        {
                            Directory.CreateDirectory(dirLiveInfo.FullName);
                            replaysLive = dirLiveInfo.GetFiles("*.rpy");
                        }
                    }
                    if (replaysLive != null)
                    {
                        List<ReplayEntry> replayListLive = new List<ReplayEntry>();
                        foreach (FileInfo curFile in replaysLive)
                        {
                            string name = curFile.Name;
                            float size = curFile.Length / 1024.0f;
                            string path = curFile.FullName;
                            replayListLive.Add(new ReplayEntry() { Filename = name, Filesize = size.ToString("#,###.#") + "KB", FullPath = path });
                        }

                        list.ItemsSource = replayListLive;
                        return true;
                    }
                    return false;
                }
                else return false;

            }

            public void LoadBackup(ref ListView list)
            {
                //this one shouldn't be able to fail
                if(dirBackup != "!")
                {
                    dirBackupInfo = new DirectoryInfo(dirBackup);
                    replaysBackup = dirBackupInfo.GetFiles("*.rpy");    
                    List<ReplayEntry> replayListBackup = new List<ReplayEntry>();
                    foreach (FileInfo curFile in replaysBackup)
                    {
                        string name = curFile.Name;
                        float size = curFile.Length / 1024.0f;
                        string path = curFile.FullName;
                        replayListBackup.Add(new ReplayEntry() { Filename = name, Filesize = (size.ToString("#,###.#") + "KB"), FullPath = path });
                    }
                    
                    list.ItemsSource = replayListBackup;
                }
            }

            private void CreateListEntry(int i, ref ListViewItem listItem)
            {
                ListViewItem entry = new ListViewItem();

                StackPanel contentPanel1 = new StackPanel();
                contentPanel1.Orientation = Orientation.Horizontal;

                StackPanel contentPanel2 = new StackPanel();
                contentPanel2.Orientation = Orientation.Vertical;
                contentPanel2.Margin = new Thickness(10, 10, 10, 10);

                Image image = new Image();
                BitmapImage imageSource = new BitmapImage(new Uri((String)("pack://application:,,,/Resources/img_game/img_" + GameData.setting[i] + ".jpg"), UriKind.Absolute));
                image.Source = imageSource;
                image.Width = 50;

                TextBlock entryTitle = new TextBlock();
                TextBlock entryName = new TextBlock();
                entryTitle.FontSize = 18;
                entryName.FontSize = 12;
                entryTitle.Text = GameData.titles[i];
                entryName.Text = GameData.names[i];

                contentPanel2.Children.Add(entryTitle);
                contentPanel2.Children.Add(entryName);

                contentPanel1.Children.Add(image);
                contentPanel1.Children.Add(contentPanel2);

                entry.Content = contentPanel1;
                listItem = entry;
            }
        }

    }

    public static class GameReplayDecoder
    {

    }

    public class ReplayEntry
    {
        public string Filename { get; set; }
        public string Filesize { get; set; }
        
        public string FullPath;
    }

    public static class GameData
    {
        /*
         *  In the future, I plan to have a data file that contains all the necessary
         *  data for each game to be displayed and for the replay files to be decoded
         *  if available. I'll also either create an external tool or bake in the capability
         *  to edit this data file to enable easy expansion of this program for new releases
         *  and additional fangames. For now though, baked in game information will suffice.
         *  
         *  I guess having it all in this class actually works nicely since I can just add in
         *  loading functions to this class and populate these string arrays with the same info
         */

        public static readonly String[] titles =
        {
            "Touhou 6",
            "Touhou 7",
            "Touhou 8",
            "Touhou 9",
            "Touhou 9.5",
            "Touhou 10",
            "Touhou 11",
            "Touhou 12",
            "Touhou 12.3",
            "Touhou 12.5",
            "Touhou 12.8",
            "Touhou 13",
            "Touhou 13.5",
            "Touhou 14",
            "Touhou 14.3",
            "Touhou 14.5",
            "Touhou 15",
            "Touhou 15.5",
            "Touhou 16",
            "Touhou 16.5"
        };

        public static readonly String[] names =
        {
            "Embodiment of Scarlet Devil",
            "Perfect Cherry Blossom",
            "Imperishable Night",
            "Phantasmagoria of Flower View",
            "Shoot the Bullet",
            "Mountain of Faith",
            "Subterranean Animism",
            "Unidentified Fantastic Object",
            "Hisoutensoku",
            "Double Spoiler",
            "Great Fairy Wars",
            "Ten Desires",
            "Hopeless Masquerade",
            "Double Dealing Character",
            "Impossible Spell Card",
            "Urban Legend in Limbo",
            "Legacy of Lunatic Kingdom",
            "Antinomy of Common Flowers",
            "Hidden Star in Four Seasons",
            "Violet Detector"
        };

        public static readonly String[] setting =
        {
            "th06",
            "th07",
            "th08",
            "th09",
            "th095",
            "th10",
            "th11",
            "th12",
            "th123",
            "th125",
            "th128",
            "th13",
            "th135",
            "th14",
            "th143",
            "th145",
            "th15",
            "th155",
            "th16",
            "th165"
        };
    }
}

