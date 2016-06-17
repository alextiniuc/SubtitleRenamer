using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Path = System.IO.Path;
namespace SubtitleRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _currentDirectory = @"c:\";
        private string _moviePattern = string.Empty;
        private string _subPattern = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            _currentDirectory = @"c:\";
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select Working Directory";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = @"c:\";

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = @"c:\";
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _currentDirectory = dlg.FileName;
                TbPath.Text = dlg.FileName;
            }
        }

        private void BtSelectMovie_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select Movie File";
            dlg.InitialDirectory = _currentDirectory;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = _currentDirectory;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;
            dlg.Filters.Add(new CommonFileDialogFilter("Movie Files", "*.mp4,*.avi,*.mkv"));
            dlg.Filters.Add(new CommonFileDialogFilter("All Files", "*.*"));
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TbMovie.Text = Path.GetFileName(dlg.FileName);
                TbSrt.Text = "";
                TbSrt.Visibility = Visibility.Visible;
                BtSelectSrt.Visibility = Visibility.Visible;
            }
        }

        private void TbPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = (TextBox)sender;
            if (!s.Text.Contains("\\")) return;
            var dirExists = Directory.Exists(s.Text);
            if (!dirExists)
            {
                MessageBox.Show("This Directory does not Exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetControls();
                return;
            }

            ResetControls();
            TbMovie.Visibility = Visibility.Visible;
            BtSelectMovie.Visibility = Visibility.Visible;
        }

        private void ResetControls()
        {
            TbMovie.Text = "";
            TbMovie.Visibility = Visibility.Hidden;
            TbMoviePattern.Text = "";
            TbMoviePattern.Visibility = Visibility.Hidden;
            TbSubPattern.Text = "";
            TbSubPattern.Visibility = Visibility.Hidden;
            TbSrt.Text = "";
            TbSrt.Visibility = Visibility.Hidden;
            BtSelectMovie.Visibility = Visibility.Hidden;
            BtSelectSrt.Visibility = Visibility.Hidden;
            BtGo.Visibility = Visibility.Hidden;
            ListView.Visibility = Visibility.Hidden;
            PbProgress.Visibility = Visibility.Hidden;
            BTPreview.Visibility = Visibility.Hidden;
        }

        private void BtSelectSrt_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select Movie File";
            dlg.InitialDirectory = _currentDirectory;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = _currentDirectory;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;
            dlg.Filters.Add(new CommonFileDialogFilter("Sub Files", "*.srt,*.sub"));
            dlg.Filters.Add(new CommonFileDialogFilter("All Files", "*.*"));
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TbSrt.Text = Path.GetFileName(dlg.FileName);
                TbMoviePattern.Text = "";
                TbMoviePattern.Visibility = Visibility.Visible;
            }
        }

        private void TBSrtPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = (TextBox)sender;
            if (string.IsNullOrEmpty(s.Text) || s.Text.Equals("Add Pattern", StringComparison.CurrentCultureIgnoreCase)) return;
            _subPattern = s.Text;
            BtGo.Visibility = Visibility.Visible;
            BTPreview.Visibility = Visibility.Visible;
        }

        private void TBMoviePattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = (TextBox)sender;
            if (string.IsNullOrEmpty(s.Text) || s.Text.Equals("Add Pattern", StringComparison.CurrentCultureIgnoreCase)) return;
            _moviePattern = s.Text;
            TbSubPattern.Visibility = Visibility.Visible;
        }

        private void BtGo_Click(object sender, RoutedEventArgs e)
        {
            var movieFiles = Directory.GetFiles(_currentDirectory, "*" + Path.GetExtension(_currentDirectory + "\\" + TbMovie.Text));
            var srtFiles = Directory.GetFiles(_currentDirectory, "*" + Path.GetExtension(_currentDirectory + "\\" + TbSrt.Text));
            var newRegex = new Regex(Regex.Escape(_moviePattern).Replace("\\{", "{").Replace("\\}", "}").Replace("{0}", "([^A-Za-z]*)"));
            ListView.Visibility = Visibility.Visible;
            PbProgress.Visibility = Visibility.Visible;
            PbProgress.Minimum = 1;
            PbProgress.Maximum = movieFiles.Length;
            PbProgress.Value = 1;

            foreach (var movieFile in movieFiles)
            {
                PbProgress.Value += 1;
                var season = newRegex.Match(Path.GetFileName(movieFile).Replace("(", "").Replace(")", "").Replace(".", " ")).Groups[1].Value.Trim();
                var episode =
                    newRegex.Match(Path.GetFileName(movieFile).Replace("(", "").Replace(")", "").Replace(".", " ")).Groups[2].Value.Trim();


                var subfile = srtFiles.FirstOrDefault(m => Path.GetFileName(m).Contains(string.Format(_subPattern, season, episode)));
                if (subfile == null) continue;
                var newFileName = _currentDirectory + "\\" + Path.GetFileNameWithoutExtension(movieFile) + Path.GetExtension(subfile);
                MoveFile(subfile, newFileName);
                ListView.Items.Add(new { Name = Path.GetFileName(subfile) + "-->" + Path.GetFileName(newFileName) });

            }

        }

        private void BtPreview_Click(object sender, RoutedEventArgs e)
        {
            var movieFiles = Directory.GetFiles(_currentDirectory, "*" + Path.GetExtension(_currentDirectory + "\\" + TbMovie.Text));
            var srtFiles = Directory.GetFiles(_currentDirectory, "*" + Path.GetExtension(_currentDirectory + "\\" + TbSrt.Text));
            var newRegex = new Regex(Regex.Escape(_moviePattern).Replace("\\{", "{").Replace("\\}", "}").Replace("{0}", "([^A-Za-z]*)"));
            ListView.Visibility = Visibility.Visible;
            PbProgress.Visibility = Visibility.Visible;
            PbProgress.Minimum = 1;
            PbProgress.Maximum = movieFiles.Length;
            PbProgress.Value = 1;

            foreach (var movieFile in movieFiles)
            {
                PbProgress.Value += 1;
                var season = newRegex.Match(Path.GetFileName(movieFile).Replace("(", "").Replace(")", "").Replace(".", " ")).Groups[1].Value.Trim();
                var episode =
                    newRegex.Match(Path.GetFileName(movieFile).Replace("(", "").Replace(")", "").Replace(".", " ")).Groups[2].Value.Trim();


                var subfile = srtFiles.FirstOrDefault(m => Path.GetFileName(m).Contains(string.Format(_subPattern, season, episode)));
                if (subfile == null) continue;
                var newFileName = _currentDirectory + "\\" + Path.GetFileNameWithoutExtension(movieFile) + Path.GetExtension(subfile);

                ListView.Items.Add(new { Name = Path.GetFileName(subfile) + "-->" + Path.GetFileName(newFileName) });

            }


        }
        private static void MoveFile(string file, string newFileName)
        {
            File.Copy(file, newFileName);
            File.Delete(file);
        }
    }
}
