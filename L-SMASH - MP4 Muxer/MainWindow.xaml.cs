using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MediaInfoLib;
using System.Diagnostics;
using TAlex.WPF.Controls;
using System.Collections;
using System.Globalization;
using System.Resources;
using L_SMASH___MP4_Muxer.Properties;
using L_SMASH___MP4_Muxer.Job;
using System.Text.RegularExpressions;

namespace L_SMASH___MP4_Muxer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            audio_languages = AudioLanguage.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in audio_languages)
            {
                String language = entry.Key.ToString();
                ia_language_1.Items.Add(new ComboBoxItem() { Content = language, });
            }
            FileInfo lsmash = new FileInfo(".\\muxer.exe");
            if (!lsmash.Exists)
            {
                MessageBox.Show("muxer.exe is missing");
                isProcessing = false;
                SetStartButton(false);
                SetProgressValue(0);
                SetStatusText("muxer.exe is missing");
            }
            jobProcessor = new JobProcessor(lsmash.FullName);
            jobProcessor.ProgressChanged += progress => SetProgressValue(progress);
            jobProcessor.LogChanged += data => AppendLogText(data);
            isProcessing = false;
            SetProgressValue(0);
            SetStatusText("Ready");
        }

        private String get_video_FPS(String fileName)
        {
            if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\MediaInfo.dll"))
            {
                MessageBox.Show("MediaInfo.dll is missing");
                return null;
            }
            else if (!File.Exists(fileName))
            {
                MessageBox.Show("Video file isn't existed");
                return null;
            }
            MediaInfo v_info = new MediaInfo();
            v_info.Open(fileName);
            String v_FPS = v_info.Get(StreamKind.Video, 0, "FrameRate_Num") + "/" + v_info.Get(StreamKind.Video, 0, "FrameRate_Den");
            if (v_FPS == "/")
            {
                v_FPS = (((int)Convert.ToSingle(v_info.Get(StreamKind.Video, 0, "FrameRate")) * 1000).ToString() + "/" + "1000");
            }
            return v_FPS;
        }

        private void FPS_Click(object sender, MouseButtonEventArgs e)
        {
            if (iv_path.Text != "")
            {
                iv_FPS.Text = get_video_FPS(iv_path.Text);
            }
        }

        private void iv_file_Click(object sender, RoutedEventArgs e)
        {
            // TODO: support remux mp4 file
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "All supported files|*.265;*.hevc;*.264;*.h264;*.avc|RAW MPEG-4 HEVC|*.265;*.hevc|RAW MPEG-4 AVC|*.264;*.h264;*.avc",
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String v_fileName = openFileDialog.FileName;
                iv_path.Text = v_fileName;
                iv_FPS.Text = get_video_FPS(v_fileName);
                out_path.Text = GetOutputFileName(v_fileName);
            }
        }

        private static readonly HashSet<String> AcceptableVideoExtension = new HashSet<String> { ".avc", ".h264", ".264", ".hevc", ".265" };

        private void iv_path_Drop(object sender, DragEventArgs e)
        {
            //drag video file and confirm format
            String v_fileName = ((String[])e.Data.GetData(DataFormats.FileDrop))[0];
            //confirm if path is a directort
            String path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            if (System.IO.Directory.Exists(path))
            {
                MessageBox.Show("Not a File!");
                return;
            }
            if (AcceptableVideoExtension.Contains(Path.GetExtension(v_fileName)?.ToLower()))
            {
                iv_path.Text = v_fileName;
                iv_FPS.Text = get_video_FPS(v_fileName);
                out_path.Text = GetOutputFileName(v_fileName);
            }
            else
            {
                MessageBox.Show("Unsupport Format!");
            }
        }

        private void iv_clear_Click(object sender, RoutedEventArgs e)
        {
            iv_path.Clear();
            iv_name.Clear();
            iv_PAR_numberator.Clear();
            iv_PAR_denominator.Clear();
            iv_FPS.Text = "";
        }

        private VideoInfo GenerateVideoInfo()
        {
            int PAR_n = (String.IsNullOrEmpty(iv_PAR_numberator.Text)) ? 0 : Convert.ToInt32(iv_PAR_numberator.Text);
            int PAR_d = (String.IsNullOrEmpty(iv_PAR_denominator.Text)) ? 0 : Convert.ToInt32(iv_PAR_denominator.Text);
            VideoInfo videoInfo = new VideoInfo(iv_path.Text, iv_FPS.Text, iv_name.Text, PAR_n, PAR_d);
            return videoInfo;
        }

        private void ia_file_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = (int)Char.GetNumericValue(button.Name[button.Name.Length - 1]);
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            TextBox tb;
            tb = grid.Children[1] as TextBox;   // ia_path
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "All supported files|*.aac;*.m4a;*.mp3;*.mp4|AAC|*.aac;*.m4a|MP3|*.mp3|MP4 File|*.mp4",
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb.Text = openFileDialog.FileName;
            }
        }

        private static readonly HashSet<String> AcceptableAudioExtension = new HashSet<String> { ".aac", ".m4a", ".mp3", ".mp4" };

        private void ia_path_Drop(object sender, DragEventArgs e)
        {
            TextBox audio_path = sender as TextBox;

            //drag video file and confirm format
            String a_fileName = ((String[])e.Data.GetData(DataFormats.FileDrop))[0];
            //confirm if path is a directort
            String path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            if (System.IO.Directory.Exists(path))
            {
                MessageBox.Show("Not a File!");
                return;
            }
            if (AcceptableAudioExtension.Contains(Path.GetExtension(a_fileName)?.ToLower()))
            {
                audio_path.Text = a_fileName;
            }
            else
            {
                MessageBox.Show("Unsupport Format!");
            }
        }

        private void ia_clear_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = (int)Char.GetNumericValue(button.Name[button.Name.Length - 1]);
            ClearAudioTrack(index);
        }

        private void ClearAudioTrack(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            TextBox tb;
            tb = grid.Children[1] as TextBox;   // ia_path
            tb.Clear();
            tb = grid.Children[6] as TextBox;   // ia_name
            tb.Clear();
            ComboBox cb = grid.Children[4] as ComboBox; //ia_language
            cb.Text = "";
            NumericUpDown nud = grid.Children[9] as NumericUpDown;  // ia_delay
            nud.Value = 0;
        }

        private TextBox GetAudioPathTB(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            TextBox audio_path = grid.Children[1] as TextBox;
            return audio_path;
        }

        private TextBox GetAudioNameTB(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            TextBox audio_name = grid.Children[6] as TextBox;
            return audio_name;
        }

        private ComboBox GetAudioLanguageCB(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            ComboBox audio_language = grid.Children[4] as ComboBox;
            return audio_language;
        }

        private NumericUpDown GetAudioDelayNUD(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            NumericUpDown audio_delay = grid.Children[9] as NumericUpDown;
            return audio_delay;
        }
        
        private Grid GenerateNewAudioGrid(int ntracks)
        {
            Thickness BlockThicknes = new Thickness(0, 0, 0, 0);
            Thickness TextBoxThickness = new Thickness(0, 2, 0, 2);
            Thickness ButtonThickness = new Thickness(8, 2, 8, 2);

            TextBlock AI_Block = new TextBlock()
            {
                Name = "AI_Block_" + ntracks,
                Margin = BlockThicknes,
                TextWrapping = TextWrapping.Wrap,
                Text = "Audio Input",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
            };
            TextBox AI_Path = new TextBox()
            {
                Name = "ia_path_" + ntracks,
                Margin = TextBoxThickness,
                FontSize = 14,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            AI_Path.PreviewDragOver += file_DragOver;
            AI_Path.PreviewDrop += ia_path_Drop;
            Button AI_File = new Button()
            {
                Name = "ia_file_" + ntracks,
                Content = "...",
                Margin = ButtonThickness,
            };
            AI_File.Click += ia_file_Click;
            Grid.SetRow(AI_Block, 0);
            Grid.SetColumn(AI_Block, 0);
            Grid.SetRow(AI_Path, 0);
            Grid.SetColumn(AI_Path, 1);
            Grid.SetColumnSpan(AI_Path, 3);
            Grid.SetRow(AI_File, 0);
            Grid.SetColumn(AI_File, 4);

            TextBlock Language_Block = new TextBlock()
            {
                Name = "Language_Block_" + ntracks,
                Margin = BlockThicknes,
                TextWrapping = TextWrapping.Wrap,
                Text = "Language",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
            };
            ComboBox AI_Language = new ComboBox()
            {
                Name = "ia_language_" + ntracks,
                Margin = TextBoxThickness,
                FontSize = 14,
                IsEditable = true,
            };
            foreach (DictionaryEntry entry in audio_languages)
            {
                String language = entry.Key.ToString();
                AI_Language.Items.Add(new ComboBoxItem() { Content = language, });
            }
            TextBlock AName_Block = new TextBlock()
            {
                Name = "AName_Block_" + ntracks,
                Margin = BlockThicknes,
                TextWrapping = TextWrapping.Wrap,
                Text = "Name",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            TextBox AI_Name = new TextBox()
            {
                Name = "ia_name_" + ntracks,
                Margin = TextBoxThickness,
                FontSize = 14,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            Button AI_Clear = new Button()
            {
                Name = "ia_clear_" + ntracks,
                Content = "X",
                Margin = ButtonThickness,
            };
            AI_Clear.Click += ia_clear_Click;
            Grid.SetRow(Language_Block, 1);
            Grid.SetColumn(Language_Block, 0);
            Grid.SetRow(AI_Language, 1);
            Grid.SetColumn(AI_Language, 1);
            Grid.SetRow(AName_Block, 1);
            Grid.SetColumn(AName_Block, 2);
            Grid.SetRow(AI_Name, 1);
            Grid.SetColumn(AI_Name, 3);
            Grid.SetRow(AI_Clear, 1);
            Grid.SetColumn(AI_Clear, 4);

            TextBlock Delay_Block = new TextBlock()
            {
                Name = "Delay_Block_" + ntracks,
                Margin = BlockThicknes,
                TextWrapping = TextWrapping.Wrap,
                Text = "Delay",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
            };
            NumericUpDown AI_Delay = new NumericUpDown()
            {
                Name = "ia_delay_" + ntracks,
                Margin = TextBoxThickness,
                Value = 0,
                Minimum = -10000,
                Maximum = 10000,
            };
            Grid.SetRow(Delay_Block, 2);
            Grid.SetColumn(Delay_Block, 0);
            Grid.SetRow(AI_Delay, 2);
            Grid.SetColumn(AI_Delay, 1);

            Grid grid = new Grid()
            {
                Margin = new Thickness(2, 1, 2, 1),
            };
            RowDefinition row0 = new RowDefinition();
            row0.Height = new GridLength(40, GridUnitType.Star);
            grid.RowDefinitions.Add(row0);
            RowDefinition row1 = new RowDefinition();
            row1.Height = new GridLength(40, GridUnitType.Star);
            grid.RowDefinitions.Add(row1);
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(40, GridUnitType.Star);
            grid.RowDefinitions.Add(row2);

            ColumnDefinition col0 = new ColumnDefinition();
            col0.Width = new GridLength(25, GridUnitType.Star);
            grid.ColumnDefinitions.Add(col0);
            ColumnDefinition col1 = new ColumnDefinition();
            col1.Width = new GridLength(35, GridUnitType.Star);
            grid.ColumnDefinitions.Add(col1);
            ColumnDefinition col2 = new ColumnDefinition();
            col2.Width = new GridLength(20, GridUnitType.Star);
            grid.ColumnDefinitions.Add(col2);
            ColumnDefinition col3 = new ColumnDefinition();
            col3.Width = new GridLength(30, GridUnitType.Star);
            grid.ColumnDefinitions.Add(col3);
            ColumnDefinition col4 = new ColumnDefinition();
            col4.Width = new GridLength(10, GridUnitType.Star);
            grid.ColumnDefinitions.Add(col4);

            grid.Children.Add(AI_Block);
            grid.Children.Add(AI_Path);
            grid.Children.Add(AI_File);
            grid.Children.Add(Language_Block);
            grid.Children.Add(AI_Language);
            grid.Children.Add(AName_Block);
            grid.Children.Add(AI_Name);
            grid.Children.Add(AI_Clear);
            grid.Children.Add(Delay_Block);
            grid.Children.Add(AI_Delay);

            return grid;
        }

        private void AddAudioTrack(object sender, RoutedEventArgs e)
        {
            int ntracks = Audio_Tab.Items.Count;
            TabItem newAudioTrack = new TabItem()
            {
                Header = "Audio " + (++ntracks),
            };
            newAudioTrack.Content = GenerateNewAudioGrid(ntracks);

            Audio_Tab.Items.Add(newAudioTrack);
            if (removeTrackMenu.IsEnabled == false)
            {
                removeTrackMenu.IsEnabled = true;
            }
            Audio_Tab.SelectedItem = newAudioTrack;
        }

        private void RemoveAudioTrack(object sender, RoutedEventArgs e)
        {
            int ntracks = Audio_Tab.Items.Count;
            Audio_Tab.Items.RemoveAt(--ntracks);
            if (ntracks == 1)
            {
                removeTrackMenu.IsEnabled = false;
            }
        }
        
        private List<AudioInfo> GenerateAudioInfoList()
        {
            List<AudioInfo> audioInfoList = new List<AudioInfo>();
            int ntracks = Audio_Tab.Items.Count;
            for (int i = 1; i <= ntracks; ++i)
            {
                AudioInfo audioInfo = new AudioInfo(
                    GetAudioPathTB(i).Text, GetAudioLanguageCB(i).Text,
                    GetAudioNameTB(i).Text, Convert.ToInt32(GetAudioDelayNUD(i).Value));
                audioInfoList.Add(audioInfo);
            }
            return audioInfoList;
        }

        private void ic_file_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Ogg Chapter File|*.txt",
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ic_path.Text = openFileDialog.FileName;
            }
        }

        private void ic_path_Drop(object sender, DragEventArgs e)
        {
            //drag chapter file and confirm format
            string c_fileName = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            //confirm if path is a directort
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            if (System.IO.Directory.Exists(path))
            {
                MessageBox.Show("Not a File!");
                return;
            }
            if (Path.GetExtension(c_fileName).ToLower() == (".txt"))
            {
                ic_path.Text = c_fileName;
            }
            else
            {
                MessageBox.Show("Unsupport Format!");
            }
        }

        private void Chapter_Clear(object sender, MouseButtonEventArgs e)
        {
            ic_path.Clear();
        }

        private ChapterInfo GenerateChapterInfo()
        {
            ChapterInfo chapterInfo = new ChapterInfo(ic_path.Text);
            return chapterInfo;
        }

        private void out_file_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "MP4 File|*.mp4",
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (out_path.Text != "")
            {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(out_path.Text);
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(out_path.Text);
            }
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                out_path.Text = saveFileDialog.FileName;

            }
        }

        private void logs_TextChanged(object sender, TextChangedEventArgs e)
        {
            logs.ScrollToEnd();
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("L-SMASH - MP4 Muxer\n\nThis is a GUI which use l-smash to mux video and audio");
        }

        private void ClearAll(object sender, RoutedEventArgs e)
        {
            // clear video's args
            iv_path.Clear();
            iv_name.Clear();
            iv_PAR_numberator.Clear();
            iv_PAR_denominator.Clear();
            iv_FPS.Text = "";

            // clear audio's args
            int ntracks = Audio_Tab.Items.Count;
            for (int i = 1; i <= ntracks; ++i)
            {
                ClearAudioTrack(i);
            }

            // clear chapter's args
            ic_path.Clear();

            // clear output's args
            out_path.Clear();

            if (!isProcessing)
            {
                logs.Clear();
                SetStatusText("Ready");
                SetProgressValue(0);
            } 
        }

        private void StartMuxer(object sender, RoutedEventArgs e)
        {
            if (out_path.Text == "")
            {
                MessageBox.Show("Output not set");
                return;
            }
            logs.Clear();
            Status_Block.Text = "Processing...";
            start_button.IsEnabled = false;
            SetProgressValue(0);

            job = new MuxerJob(GenerateVideoInfo(), GenerateAudioInfoList(), GenerateChapterInfo(), out_path.Text);
            isProcessing = true;
            jobProcessor.StartMuxer(job);
        }

        private void OpenQueue(object sender, RoutedEventArgs e)
        {
            QueueWindow qw = new QueueWindow();
            qw.Title = "queue window";
            //qw.ShowDialog();
            qw.Show();
        }

        private void SetStartButton(bool state)
        {
            if (!this.start_button.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => SetStartButton(state));
            }
            else
            {
                this.start_button.IsEnabled = state;
            }
        }

        private void AppendLogText(String text)
        {
            if (!this.logs.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => AppendLogText(text));
            }
            else
            {
                this.logs.AppendText(text + "\r\n");
            }
        }

        private void SetStatusText(String text)
        {
            if (!this.Status_Block.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => SetStatusText(text));
            }
            else
            {
                this.Status_Block.Text = text;
            }
        }

        private void SetProgressValue(double progress)
        {
            if (!this.Status_Progress.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => SetProgressValue(progress));
            }
            else
            {
                this.Status_Progress.Value = progress;
                if (progress == 100)
                {
                    SetStartButton(true);
                    Status_Block.Text = "Finish";
                    isProcessing = false;
                }
            }
        }

        private double GetProgressValue()
        {
            if (!this.Status_Progress.Dispatcher.CheckAccess())
            {
                return this.Dispatcher.Invoke(() => GetProgressValue());
            }
            else
            {
                return this.Status_Progress.Value;
            }
        }

        private void file_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
            e.Handled = true;
        }

        private static String GetOutputFileName(String fullPath)
        {
            String directory = Path.GetDirectoryName(fullPath) ?? "";
            String fileName = Path.GetFileNameWithoutExtension(fullPath) ?? "";
            String path = Path.Combine(directory, fileName + "_muxed" + ".mp4");
            for (int i = 1; File.Exists(path); ++i)
            {
                path = Path.Combine(directory, fileName + "_muxed" + $"_{i}.mp4");
            }
            return path;
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(String str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        private ResourceSet audio_languages;
        private JobProcessor jobProcessor;
        private MuxerJob job;
        private bool isProcessing;
    }
}
