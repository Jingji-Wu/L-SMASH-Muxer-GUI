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
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using MediaInfoLib;
using System.Diagnostics;
using TAlex.WPF.Controls;

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
            clearAudioTrack(index);
        }

        private void clearAudioTrack(int index)
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

        private TextBox getAudioPathTB(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            TextBox audio_path = grid.Children[1] as TextBox;
            return audio_path;
        }

        private TextBox getAudioNameTB(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            TextBox audio_name = grid.Children[6] as TextBox;
            return audio_name;
        }

        private ComboBox getAudioLanguageCB(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            ComboBox audio_language = grid.Children[4] as ComboBox;
            return audio_language;
        }

        private NumericUpDown getAudioDelayNUD(int index)
        {
            TabItem item = Audio_Tab.Items[index - 1] as TabItem;
            Grid grid = item.Content as Grid;
            NumericUpDown audio_delay = grid.Children[9] as NumericUpDown;
            return audio_delay;
        }

        private static readonly String[] AudioLanguage = new String[] {
            "abk",
            "aar",
            "afr",
            "aka",
            "alb",
            "amh",
            "ara",
            "arg",
            "arm",
            "asm",
            "ava",
            "ave",
            "aym",
            "aze",
            "bam",
            "bak",
            "baq",
            "bel",
            "ben",
            "bih",
            "bis",
            "bos",
            "bre",
            "bul",
            "bur",
            "cat",
            "cha",
            "che",
            "nya",
            "chi",
            "chv",
            "cor",
            "cos",
            "cre",
            "hrv",
            "cze",
            "dan",
            "div",
            "dut",
            "dzo",
            "eng",
            "epo",
            "est",
            "ewe",
            "fao",
            "fij",
            "fin",
            "fre",
            "ful",
            "glg",
            "geo",
            "ger",
            "gre",
            "grn",
            "guj",
            "hat",
            "hau",
            "heb",
            "her",
            "hin",
            "hmo",
            "hun",
            "ina",
            "ind",
            "ile",
            "gle",
            "ibo",
            "ipk",
            "ido",
            "ice",
            "ita",
            "iku",
            "jpn",
            "jav",
            "kal",
            "kan",
            "kau",
            "kas",
            "kaz",
            "khm",
            "kik",
            "kin",
            "kir",
            "kom",
            "kon",
            "kor",
            "kur",
            "kua",
            "lat",
            "ltz",
            "lug",
            "lim",
            "lin",
            "lao",
            "lit",
            "lub",
            "lav",
            "glv",
            "mac",
            "mlg",
            "may",
            "mal",
            "mlt",
            "mao",
            "mar",
            "mah",
            "mon",
            "nau",
            "nav",
            "nob",
            "nde",
            "nep",
            "ndo",
            "nno",
            "nor",
            "iii",
            "nbl",
            "oci",
            "oji",
            "chu",
            "orm",
            "ori",
            "oss",
            "pan",
            "pli",
            "per",
            "pol",
            "pus",
            "por",
            "que",
            "roh",
            "run",
            "rum",
            "rus",
            "san",
            "srd",
            "snd",
            "sme",
            "smo",
            "sag",
            "srp",
            "gla",
            "sna",
            "sin",
            "slo",
            "slv",
            "som",
            "sot",
            "azb",
            "spa",
            "sun",
            "swa",
            "ssw",
            "swe",
            "tam",
            "tel",
            "tgk",
            "tha",
            "tir",
            "tib",
            "tuk",
            "tgl",
            "tsn",
            "ton",
            "tur",
            "tso",
            "tat",
            "twi",
            "tah",
            "uig",
            "ukr",
            "urd",
            "uzb",
            "ven",
            "vie",
            "vol",
            "wln",
            "wel",
            "wol",
            "fry",
            "xho",
            "yid",
            "yor",
            "zha",
            "zul"};
        
        private Grid addNewAudioGrid(int ntracks)
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
            foreach (String language in AudioLanguage)
            {
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

        private void addAudioTrack(object sender, RoutedEventArgs e)
        {
            int ntracks = Audio_Tab.Items.Count;
            TabItem newAudioTrack = new TabItem()
            {
                Header = "Audio " + (++ntracks),
            };
            newAudioTrack.Content = addNewAudioGrid(ntracks);

            Audio_Tab.Items.Add(newAudioTrack);
            if (removeTrackMenu.IsEnabled == false)
            {
                removeTrackMenu.IsEnabled = true;
            }
            Audio_Tab.SelectedItem = newAudioTrack;
        }

        private void removeAudioTrack(object sender, RoutedEventArgs e)
        {
            int ntracks = Audio_Tab.Items.Count;
            Audio_Tab.Items.RemoveAt(--ntracks);
            if (ntracks == 1)
            {
                removeTrackMenu.IsEnabled = false;
            }
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

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("L-SMASH Muxer GUI\n\nThis is a GUI which use l-smash to mux video and audio");
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
                clearAudioTrack(i);
            }

            // clear chapter's args
            ic_path.Clear();

            // clear output's args
            out_path.Clear();

            logs.Clear();
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (out_path.Text == "")
            {
                MessageBox.Show("Output not set");
                return;
            }
            String arg_muxer = GetMuxerArgs();
            if (arg_muxer == null)
            {
                return;
            }
            logs.Clear();
            logs.AppendText("Processing...\r\n");
            start_button.IsEnabled = false;
            await ExcuteMuxer(arg_muxer);
        }

        private async Task ExcuteMuxer(String cmd)
        {
            String Excutable = "muxer";
            try
            {
                Process p = new Process
                {
                    StartInfo =
                    {
                        FileName = System.Windows.Forms.Application.StartupPath + "\\"+ Excutable,
                        Arguments = cmd,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    },
                };
                p.OutputDataReceived += outputHandler;
                p.ErrorDataReceived += outputHandler;
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                await Task.Factory.StartNew(p.WaitForExit);
                p.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("执行命令失败，请检查输入的命令是否正确！");
                MessageBox.Show(ex.Message);
            }
            setStartButton(true);
            appendLogText("Finished.\r\n");
        }

        private void outputHandler(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                appendLogText(e.Data);
            }
        }

        private void setStartButton(bool state)
        {
            if (!this.start_button.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => setStartButton(state));
            }
            else
            {
                this.start_button.IsEnabled = state;
            }
        }

        private void appendLogText(String text)
        {
            if (!this.logs.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => appendLogText(text));
            }
            else
            {
                this.logs.AppendText(text + "\r\n");
            }
        }

        private String GetMuxerArgs()
        {
            String arg_muxer = "";
            int n_track = 0;
            // set video track
            if (iv_path.Text != "")
            {
                int video_args = 0;
                arg_muxer += " -i \"" + iv_path.Text + "\"";
                if (iv_FPS.Text != "")
                {
                    arg_muxer += "?fps=" + iv_FPS.Text;
                    ++video_args;
                }
                if (iv_name.Text != "")
                {
                    if (video_args == 0)
                    {
                        arg_muxer += "?handler=" + iv_name.Text;
                    }
                    else
                    {
                        arg_muxer += ",handler=" + iv_name.Text;
                    }
                    ++video_args;
                }
                if (iv_PAR_numberator.Text != "" && iv_PAR_denominator.Text != "")
                {
                    if (video_args == 0)
                    {
                        arg_muxer += "?par=" + iv_PAR_numberator.Text + ":" + iv_PAR_denominator.Text;
                    }
                    else
                    {
                        arg_muxer += ",par=" + iv_PAR_numberator.Text + ":" + iv_PAR_denominator.Text;
                    }
                    ++video_args;
                }
                ++n_track;
            }
            // set audio track
            int ntracks = Audio_Tab.Items.Count;
            for (int i = 1; i <= ntracks; ++i)
            {
                TextBox path = getAudioPathTB(i);
                TextBox name = getAudioNameTB(i);
                ComboBox language = getAudioLanguageCB(i);
                NumericUpDown delay = getAudioDelayNUD(i);
                if (path.Text != "")
                {
                    int audio_args = 0;
                    arg_muxer += " -i \"" + path.Text + "\"";
                    if (language.Text != "")
                    {
                        arg_muxer += "?1:language=" + language.Text;
                        ++audio_args;
                    }
                    if (name.Text != "")
                    {
                        if (audio_args == 0)
                        {
                            arg_muxer += "?1:handler=" + name.Text;
                        }
                        else
                        {
                            arg_muxer += ",handler=" + name.Text;
                        }
                        ++audio_args;
                    }
                    if (delay.Value != 0)
                    {
                        if (audio_args == 0)
                        {
                            arg_muxer += "?1:encoder-delay=" + delay.Value;
                        }
                        else
                        {
                            arg_muxer += ",encoder-delay=" + delay.Value;
                        }
                        ++audio_args;
                    }
                    ++n_track;
                }
            }
            // set chapter track
            if (ic_path.Text != "")
            {
                arg_muxer += " --chapter \"" + ic_path.Text + "\"";
                ++n_track;
            }
            // set output
            if (n_track != 0)
            {
                arg_muxer += " -o \"" + out_path.Text + "\"";
            }
            else
            {
                MessageBox.Show("Nothing to mux!");
                return null;
            }
            return arg_muxer;
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
    }
}
