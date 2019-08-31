using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private double x;
        private double y;

        private Storyboard storyboard;

        private bool seeking = false;
        private bool isFullScreen = false;

        


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            storyboard = (Storyboard)FindResource("storyboard");

            ((ViewModel)DataContext).PlayRequested += Play;
            ((ViewModel)DataContext).PauseRequested += Pause;
            ((ViewModel)DataContext).StopRequested += Stop;
            ((ViewModel)DataContext).FullScreenRequested += ToggleFullScreen;
            ((ViewModel)DataContext).ReloadRequested += Reload;

            Track track = timeSlider.Template.FindName("PART_Track", timeSlider) as Track;
            track.Thumb.MouseEnter += Thumb_MouseEnter;
            
        }


        private void Reload()
        {
            storyboard.Stop(this);
            storyboard.Begin(this, true);
        }

        private void ToggleFullScreen()
        {
            ToggleFullScreen(!isFullScreen);
        }

        private void ToggleFullScreen(bool enterFullscreen)
        {            
            if (enterFullscreen)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                PlayList.Visibility = Visibility.Collapsed;
                Splitter.Visibility = Visibility.Collapsed;
                togglePlaylist.IsChecked = false;
                MainMenu.Visibility = Visibility.Collapsed;
                Controls.Visibility = Visibility.Collapsed;
                MainGrid.ColumnDefinitions[0].Width = new GridLength(1,GridUnitType.Star);
                MainGrid.ColumnDefinitions[1].Width = GridLength.Auto;
                MainGrid.ColumnDefinitions[2].Width = GridLength.Auto;
                Topmost = true;                
                Hide();
                Show();
                Keyboard.Focus(this);
                isFullScreen = true;

            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                PlayList.Visibility = Visibility.Visible;
                Splitter.Visibility = Visibility.Visible;
                togglePlaylist.IsChecked = true;
                MainMenu.Visibility = Visibility.Visible;
                Controls.Visibility = Visibility.Visible;
                Topmost = false;                
                isFullScreen = false;

            }           
        }

        

        

        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                storyboard.Seek(this, TimeSpan.FromSeconds(timeSlider.Value), TimeSeekOrigin.BeginTime);
            }
        }

        private void Play()
        {
            if (((ViewModel)DataContext).CurrentSource == null) return;

           
            if (((ViewModel)DataContext).Status != Status.Paused)
            {
                storyboard.Begin(this, true);
            }
            else
            {               
                storyboard.Resume(this);
            }
          
        }

        private void Pause()
        {
            if (((ViewModel)DataContext).CurrentSource == null) return;

            storyboard.Pause(this);
            ((ViewModel)DataContext).Status = Status.Paused; 
        }

        private void Stop()
        {
            if (((ViewModel)DataContext).CurrentSource == null) return;

            storyboard.Stop(this);

        }



        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            
            timeSlider.Minimum = 0;
            if (mediaElement.NaturalDuration != Duration.Automatic)
            {
                ((ViewModel)DataContext).CurrentSource.Duration = mediaElement.NaturalDuration.TimeSpan;
                timeSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            }
           

        }

        private void TimeSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            seeking = true;
        }

        private void TimeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            storyboard.Seek(this, TimeSpan.FromSeconds(timeSlider.Value), TimeSeekOrigin.BeginTime);
            seeking = false;
        }

        private void MediaTimeline_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            if (seeking)
            {
                time.Text = TimeSpan.FromSeconds(timeSlider.Value).ToString(@"h\:mm\:ss");
            }
            else
            {
                TimeSpan? t = mediaElement.Clock.CurrentTime;
                if (t.HasValue)
                {
                    time.Text = t.Value.ToString(@"h\:mm\:ss");
                    timeSlider.Value = Math.Floor(t.Value.TotalSeconds);
                }

                ((ViewModel)DataContext).Status = Status.Playing; 
            }

        }

        private void MediaTimeline_CurrentStateInvalidated(object sender, EventArgs e)
        {
            if (storyboard.GetCurrentState(this) == ClockState.Stopped)
            {
                ((ViewModel)DataContext).Status = Status.Stopped;  
                timeSlider.Value = 0.0;
                time.Text = "0:00:00";
            }
        }

        private void TogglePlaylist_Click(object sender, RoutedEventArgs e)
        {
            
            if(((MenuItem)e.Source).IsChecked)
            {
                PlayList.Visibility = Visibility.Visible;
                Splitter.Visibility = Visibility.Visible;
                
            }
            else
            {
                PlayList.Visibility = Visibility.Collapsed;
                Splitter.Visibility = Visibility.Collapsed;
            }
            
        }

       

        private void MediaElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                ToggleFullScreen();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                ToggleFullScreen(false);
            }           
        }

        private void MediaTimeline_Completed(object sender, EventArgs e)
        {
            ((ViewModel)DataContext).Next();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Media Player version 1.0", "About Media Player");
        }

        private void PlayListItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                Source source = ((sender as StackPanel)?.DataContext) as Source;
                if (!(source is null))
                {
                    ((ViewModel)DataContext).CurrentSource = source;
                    Reload();
                }
            }
   
        }

        private void ListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((ViewModel)DataContext).SelectedSource = null;
        }
    }
}
