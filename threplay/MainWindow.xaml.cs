using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Ookii.Dialogs.Wpf;
using AutoUpdaterDotNET;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

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
        th125,
        th128,
        th13,
        th14,
        th143,
        th15,
        th16,
        th165,
        th17,
        th18,

        thLast
    };

    
    
    public partial class MainWindow : Window
    {
        public PaletteHelper palette;
        UpdateInfoEventArgs updateArgs;
        public MainWindow()
        {
            InitializeComponent();
            if(!File.Exists("portable.config"))
            {
                //first run?
                tutorialLayerView.Visibility = Visibility.Visible;
            }
            Bluegrams.Application.PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);


            AutoUpdater.RunUpdateAsAdmin = false;
            if((bool)Properties.Settings.Default["updates"])
            {
                AutoUpdater.CheckForUpdateEvent += CheckForUpdates;
                AutoUpdater.Start("https://raviddog.github.io/versioninfo-xml/threplay.xml");
            }


            if (((string)Properties.Settings.Default["gameVisibility"]).Length < (int)GameList.thLast)
            {
                int buffer = (int)GameList.thLast - ((string)Properties.Settings.Default["gameVisibility"]).Length;
                for(int i = 0; i < buffer; i++)
                {
                    Properties.Settings.Default["gameVisibility"] = string.Concat(Properties.Settings.Default["gameVisibility"], "Y");
                }
                //Properties.Settings.Default.Upgrade();
                //turns out the program just adds settings automatically if they're not there, which is awesome
                Properties.Settings.Default.Save();
            }


            GameHandler.gameListView = modeGameSelector;
            GameHandler.replayLiveView = oReplayLiveList;
            GameHandler.replayBackupView = oReplayBackupList;
            GameHandler.liveDir = iDirLive;
            GameHandler.backupDir = iDirBackup;
            GameHandler.InitializeGames();

            char[] v = ((string)Properties.Settings.Default["gameVisibility"]).ToCharArray();
            int p = 0;
            while (p < (int)GameList.thLast && v[p++] == 'N') { }
            if (p >= (int)GameList.thLast)
            {
                GameHandler.gameListView.SelectedIndex = -1;
            } else
            {
                GameHandler.gameListView.SelectedIndex = p - 1;
            }

            GameHandler.UpdateCurrentGame(ref oCurText, ref iDirGame, ref iDirLive, ref iDirBackup, ref outScoreLiveModified, ref outScoreBackupModified, ref fnBackupScorefile);
            CheckMove();

            palette = new PaletteHelper();
            ITheme theme = Theme.Create(Theme.Dark, Colors.DarkOrange, Colors.Yellow);
            //theme.SetBaseTheme(Theme.Dark);

            //PrimaryColor pc = PrimaryColor.DeepOrange;
            //Color c = SwatchHelper.Lookup[(MaterialDesignColor)PrimaryColor.DeepOrange];

            //theme.SetPrimaryColor(SwatchHelper.Lookup[(MaterialDesignColor)PrimaryColor.Cyan]);
            //theme.SetSecondaryColor(SwatchHelper.Lookup[(MaterialDesignColor)SecondaryColor.Red]);
            //palette.SetTheme(theme);

            ResourceDictionaryExtensions.SetTheme(Application.Current.Resources, theme);
            idRbtnTabInfo.IsChecked = true;
            
        }

        private void FnLaunchGame_Click(object sender, RoutedEventArgs e)
        {
            if(!GameHandler.LaunchGame())
            {
                SetErrorMessage("Error opening game");
            }
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
                //iViewDir.Focus();
                e.Handled = true;
            } else if(e.Key == Key.Enter)
            {
                //some weird ass fucking shit about to happen here
                if(odFileNameLive.IsFocused)
                {
                    //user hit enter after renaming
                    //probably want to put like an "are you sure" box or something i guess?
                    //something something dialoghost
                    if(oReplayLiveList.SelectedIndex != -1 && fnMultiEnabled.IsChecked == false)
                    {
                        try
                        {
                            FileInfo replayFile = new FileInfo(((ReplayEntry)oReplayLiveList.SelectedItem).FullPath);
                            replayFile.MoveTo(replayFile.DirectoryName + "\\" + odFileNameLive.Text + ".rpy");
                        } catch
                        {
                            //have actual messages later
                            SetErrorMessage("Error renaming file");
                        }
                        GameHandler.LoadLive();
                        oCurText.Focus();
                    }
                } else if(odFileNameBackup.IsFocused)
                {
                    if(oReplayBackupList.SelectedIndex != -1 && fnMultiEnabled.IsChecked == false)
                    {
                        try
                        {
                            FileInfo replayFile = new FileInfo(((ReplayEntry)oReplayBackupList.SelectedItem).FullPath);
                            replayFile.MoveTo(replayFile.DirectoryName + "\\" + odFileNameBackup.Text + ".rpy");
                        } catch
                        {
                            //have actual messages later
                            SetErrorMessage("Error renaming file");
                        }
                        GameHandler.LoadBackup();
                        oCurText.Focus();
                    }
                }
            }
        }

        private void IDirGame_GotFocus(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            if ((bool)dialog.ShowDialog(this))
            {
                iDirGame.Text = dialog.FileName;
                GameHandler.SetExe(dialog.FileName, out string replay);
                if (replay != "!")
                {
                    iDirLive.Text = replay;
                }
                GameHandler.LoadLive();
            }
            CheckMove();
            oCurText.Focus();
        }

        private void IDirLive_GotFocus(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msg = MessageBoxResult.No;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\ShanghaiAlice\\" + GameData.setting[GameHandler.currentGame] + "\\replay";

            if (Directory.Exists(path))
            {
                msg = MessageBox.Show("Replay folder detected at: \n\"" + path +
                    "\" \nWould you like to use this folder?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }

            if (msg == MessageBoxResult.Yes)
            {
                if (GameHandler.SetLive(path))
                {
                    iDirLive.Text = path;
                }
            }
            else if (msg == MessageBoxResult.No)
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Please select a folder";
                dialog.UseDescriptionForTitle = true;
                if ((bool)dialog.ShowDialog(this))
                {
                    path = dialog.SelectedPath;
                    if (GameHandler.SetLive(path))
                    {
                        iDirLive.Text = path;
                    }
                }

            }
            GameHandler.LoadLive();
            CheckMove();
            oCurText.Focus();
        }

        private void IDirBackup_GotFocus(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog(this))
            {
                if (GameHandler.SetBackup(dialog.SelectedPath))
                {
                    iDirBackup.Text = dialog.SelectedPath;
                }
                GameHandler.LoadBackup();
            }
            CheckMove();
            oCurText.Focus();
        }

        private void CheckMove()
        {
            GameHandler.CheckMove(out bool hasGame, out bool hasLive, out bool hasBackup);
            fnLaunchGame.IsEnabled = hasGame;
            fnLaunchFolder.IsEnabled = hasGame;
            fnLaunchLive.IsEnabled = hasLive;
            fnLaunchBackup.IsEnabled = hasBackup;
            fnTransferToBackup.IsEnabled = hasLive && hasBackup;
            fnTransferToLive.IsEnabled = hasLive && hasBackup;
            if (hasLive && hasBackup)
            {
                //iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderOpen;
            }
            else
            {
                //iViewDirIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderSearchOutline;
                if (hasLive && !hasBackup)
                {
                    SetErrorMessage("Please select a backup folder");
                }
                else if (!hasLive && hasBackup)
                {
                    SetErrorMessage("Please select a replay folder");
                }
                else if (!hasLive && !hasBackup)
                {
                    SetErrorMessage("Please select your current and backup replay folders");
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

        private void ModeGameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            oMessage.IsActive = false;
            GameHandler.UpdateCurrentGame(ref oCurText, ref iDirGame, ref iDirLive, ref iDirBackup, ref outScoreLiveModified, ref outScoreBackupModified, ref fnBackupScorefile);
            CheckMove();
        }

        private void FnLaunchFolder_Click(object sender, RoutedEventArgs e)
        {
            if(!GameHandler.OpenGameFolder())
            {
                SetErrorMessage("Error opening game folder");
            }
        }

        private void FnMultiEnabled_Checked(object sender, RoutedEventArgs e)
        {
            oReplayBackupList.SelectionMode = SelectionMode.Multiple;
            oReplayLiveList.SelectionMode = SelectionMode.Multiple;
            odFileGrid.Visibility = Visibility.Collapsed;
            CheckFileNameFocusable();
        }

        private void FnMultiEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            oReplayBackupList.SelectionMode = SelectionMode.Single;
            oReplayLiveList.SelectionMode = SelectionMode.Single;
            //if(iViewDir.IsExpanded == false)
            //{
                odFileGrid.Visibility = Visibility.Visible;
            //}

            CheckFileNameFocusable();
        }

        private void OReplayLiveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFileNameFocusable();
            //iViewDir.IsExpanded = false;
            if(fnMultiEnabled.IsChecked == false && oReplayLiveList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayLiveList.SelectedItem;
                odFileNameLive.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                GameReplayDecoder.ReadFile(ref replayEntry);
                if(replayEntry.replay != null)
                {
                    odFileDataLive.Text = replayEntry.replay.name;
                    odFileDateLive.Text = replayEntry.replay.date;
                    odFileShotLive.Text = replayEntry.replay.character;
                    odFileDifficultyLive.Text = replayEntry.replay.difficulty;
                    odFileScoreLive.Text = replayEntry.replay.score;
                    odFileStageLive.Text = replayEntry.replay.stage;
                    odFileStageLive.ToolTip = replayEntry.replay.stage;
                } else
                {
                    odFileDataLive.Text = null;
                    odFileDateLive.Text = null;
                    odFileShotLive.Text = null;
                    odFileDifficultyLive.Text = null;
                    odFileScoreLive.Text = null;
                    odFileStageLive.Text = null;
                    odFileStageLive.ToolTip = null;
                }

            } else
            {
                odFileDataLive.Text = null;
                odFileDateLive.Text = null;
                odFileShotLive.Text = null;
                odFileDifficultyLive.Text = null;
                odFileScoreLive.Text = null;
                odFileStageLive.Text = null;
                odFileStageLive.ToolTip = null;
            }
        }

        private void OReplayBackupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFileNameFocusable();
            //iViewDir.IsExpanded = false;
            if (fnMultiEnabled.IsChecked == false && oReplayBackupList.SelectedIndex != -1)
            {
                ReplayEntry replayEntry = (ReplayEntry)oReplayBackupList.SelectedItem;
                odFileNameBackup.Text = Path.GetFileNameWithoutExtension(replayEntry.Filename);
                GameReplayDecoder.ReadFile(ref replayEntry);
                if (replayEntry.replay != null)
                {
                    odFileDataBackup.Text = replayEntry.replay.name;
                    odFileDateBackup.Text = replayEntry.replay.date;
                    odFileShotBackup.Text = replayEntry.replay.character;
                    odFileDifficultyBackup.Text = replayEntry.replay.difficulty;
                    odFileScoreBackup.Text = replayEntry.replay.score;
                    odFileStageBackup.Text = replayEntry.replay.stage;
                    odFileStageBackup.ToolTip = replayEntry.replay.stage;
                } else
                {
                    odFileDataBackup.Text = null;
                    odFileDateBackup.Text = null;
                    odFileShotBackup.Text = null;
                    odFileDifficultyBackup.Text = null;
                    odFileScoreBackup.Text = null;
                    odFileStageBackup.Text = null;
                    odFileStageBackup.ToolTip = null;
                }
            } else
            {
                odFileDataBackup.Text = null;
                odFileDateBackup.Text = null;
                odFileShotBackup.Text = null;
                odFileDifficultyBackup.Text = null;
                odFileScoreBackup.Text = null;
                odFileStageBackup.Text = null;
                odFileStageBackup.ToolTip = null;
            }
        }

        private void FnSettings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void FnLaunchLive_Click(object sender, RoutedEventArgs e)
        {
            if(!GameHandler.OpenLiveFolder())
            {
                SetErrorMessage("Error opening replay folder");
            }
        }

        private void FnLaunchBackup_Click(object sender, RoutedEventArgs e)
        {
            if(!GameHandler.OpenBackupFolder())
            {
                SetErrorMessage("Error opening backup folder");
            }
        }

        private void OdFileNameLive_GotFocus(object sender, RoutedEventArgs e)
        {
            odLabelFileName.Text = "press enter to rename";
        }

        private void OdFileNameBackup_GotFocus(object sender, RoutedEventArgs e)
        {
            odLabelFileName.Text = "press enter to rename";
        }

        private void OdFileNameLive_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFileNameFocusable();
            odLabelFileName.Text = "File Name:";
        }

        private void OdFileNameBackup_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFileNameFocusable();
            odLabelFileName.Text = "File Name:";
        }

        private void CheckFileNameFocusable()
        {
            if(fnMultiEnabled.IsChecked == false)
            {
                if(oReplayBackupList.SelectedIndex != -1)
                {
                    odFileNameBackup.Focusable = true;
                    odFileNameBackup.Text = Path.GetFileNameWithoutExtension(((ReplayEntry)oReplayBackupList.SelectedItem).Filename);
                } else
                {
                    odFileNameBackup.Focusable = false;
                    odFileNameBackup.Text = "(no single replay selected)";
                }
                if(oReplayLiveList.SelectedIndex != -1)
                {
                    odFileNameLive.Focusable = true;
                    odFileNameLive.Text = Path.GetFileNameWithoutExtension(((ReplayEntry)oReplayLiveList.SelectedItem).Filename);
                } else
                {
                    odFileNameLive.Focusable = false;
                    odFileNameLive.Text = "(no single replay selected)";
                }
                
            }
        }

        public void SetErrorMessage(string s)
        {
            oMessage.Message = new MaterialDesignThemes.Wpf.SnackbarMessage() { Content = s };
            oMessage.IsActive = true;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += EndErrorMessage;
            timer.Start();
        }
        private void EndErrorMessage(object sender, EventArgs e)
        {
            oMessage.IsActive = false;
            ((DispatcherTimer)sender).Stop();
        }

        private void FnBackupScorefile_Click(object sender, RoutedEventArgs e)
        {
            //do error checks
            if(!GameHandler.BackupScore(ref outScoreBackupModified))
            {
                SetErrorMessage("Score.dat backup failed");     //if all goes well this shouldn't be able to trigger
            }
        }

        private void OReplayLiveList_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string err = GameHandler.AddReplay(false, files);
                SetErrorMessage(err);
                GameHandler.LoadLive();
            }
        }

        private void OReplayBackupList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string err = GameHandler.AddReplay(true, files);
                SetErrorMessage(err);
                GameHandler.LoadBackup();
            }
        }

        private void CheckForUpdates(UpdateInfoEventArgs args)
        {
            if(args != null)
            {
                if(args.IsUpdateAvailable)
                {
                    SetErrorMessage("New update available");
                    updateArgs = args;
                    fnUpdate.Visibility = Visibility.Visible;
                    fnUpdate.IsEnabled = true;
                }
            } else
            {
                //error checking updates
            }
        }

        private void FnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(AutoUpdater.DownloadUpdate(updateArgs))
                {
                    System.Windows.Application.Current.Shutdown();
                }

            } catch
            {
                SetErrorMessage("Error installing update");
            }
        }

        private void OMessage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            oMessage.IsActive = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //  first run autodetect modern replay folders
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    "\\ShanghaiAlice\\";
            MessageBoxResult msg;
            if(Directory.Exists(path)) {
                msg = MessageBoxResult.No;
                msg = MessageBox.Show("Found the %appdata%\\ShanghaiAlice\\ folder. Would you like to try and autodetect existing modern replay folders?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if(msg == MessageBoxResult.Yes) {
                    int count = 0;
                    for(int i = 0; i < GameData.setting.Length; i++) {
                        if(GameData.scorefileJ[i] == null) {
                            //  is in appdata
                            string repPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                "\\ShanghaiAlice\\" + GameData.setting[i] + "\\replay";
                            if(Directory.Exists(repPath)) {
                                GameHandler.currentGame = i;
                                GameHandler.SetLive(repPath);
                                count++;
                            }
                        }
                    }
                    MessageBox.Show("Updated " + count + " folders.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            msg = MessageBoxResult.No;
            msg = MessageBox.Show("Would you like to set your replay backup folder now?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if(msg == MessageBoxResult.Yes) {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Please select a folder";
                dialog.UseDescriptionForTitle = true;
                if((bool)dialog.ShowDialog(this)) {
                    if(!GameHandler.SetAllBackup(dialog.SelectedPath)) {
                        MessageBox.Show("Some folders failed to be set");
                    };
                }
            }


            oMessage.IsActive = false;
            GameHandler.UpdateCurrentGame(ref oCurText, ref iDirGame, ref iDirLive, ref iDirBackup, ref outScoreLiveModified, ref outScoreBackupModified, ref fnBackupScorefile);
            CheckMove();

            tutorialLayerView.Visibility = Visibility.Collapsed;
        }

        private void fnBackupScorefileAll_Click(object sender, RoutedEventArgs e)
        {
            //do error checks
            if(!GameHandler.BackupScoreAll(ref outScoreBackupModified)) {
                SetErrorMessage("Some backups were unable to be completed");     //if all goes well this shouldn't be able to trigger
            }
        }


        private void idRbtnTabDir_Checked(object sender, RoutedEventArgs e)
        {
            odDirGrid.Visibility = Visibility.Visible;
            odFileGrid.Visibility = Visibility.Collapsed;
        }

        private void idRbtnTabInfo_Checked(object sender, RoutedEventArgs e)
        {
            odDirGrid.Visibility = Visibility.Collapsed;
            odFileGrid.Visibility = Visibility.Visible;
        }

        private void idRbtnTabPacks_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void fnViewReplayAdvancedInfo_Click(object sender, RoutedEventArgs e)
        {
            ReplayEntry entry = (ReplayEntry)oReplayLiveList.SelectedItem;
            string temp = "";
            foreach(ReplayEntry.ReplayInfo.ReplaySplits st in entry.replay.splits) {
                temp += st.stage.ToString() + " - Score: " + st.score + " | Power: " + st.power + " | PIV: " + st.piv + " | Lives: " + st.lives + " | Graze: " + st.graze + "\n";
            }
            MessageBox.Show(temp);
        }
    }

}

