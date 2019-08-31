using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaPlayer
{
    public class ViewModel : INotifyPropertyChanged
    {
       
        private Source currentSource;
        public Source CurrentSource
        {
            get
            {
                return currentSource;
            }
            set
            {
                Status = Status.Stopped;
                if (currentSource != null)
                {
                    currentSource.IsSelected = false;
                }
                currentSource = value;
                if(currentSource != null)
                {
                    currentSource.IsSelected = true;
                }                
                RaisePropertyChanged("");
            }
        }

        private Source selectedSource;
        public Source SelectedSource
        {
            get
            {
                return selectedSource;
            }
            set
            {
                selectedSource = value;
                RaisePropertyChanged();
            }
        }


        
        private Status status;
        public Status Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                RaisePropertyChanged("");
            }
        }



        public bool CanPause
        {
            get
            {
                if (Status == Status.Playing)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        private bool HasNext
        {
            get
            {
                if ((CurrentSource != null) && (PlayList.Count > PlayList.IndexOf(CurrentSource) + 1)) return true;
                else return false;
            }            
        }


        private bool HasPrevious
        {
            get
            {
                if ((CurrentSource != null) && (PlayList.IndexOf(CurrentSource) > 0)) return true;
                else return false;
            }
        }

        private bool CanMoveUp
        {
            get
            {
                if ((SelectedSource != null) && (PlayList.IndexOf(SelectedSource) > 0)) return true;
                else return false;
            }

        }

        private bool CanMoveDown
        {
            get
            {
                if ((SelectedSource != null) && (PlayList.Count > PlayList.IndexOf(SelectedSource) + 1)) return true;
                else return false;
            }
        }



        public RelayCommand OpenCommand { get; private set; }
        public RelayCommand OpenURLCommand { get; private set; }

        public RelayCommand PlayPauseCommand { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand PauseCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; } 
        public RelayCommand NextCommand { get; private set; } 
        public RelayCommand PreviousCommand { get; private set; }
        public RelayCommand FullScreenCommand { get; private set; }
        public RelayCommand AddToPlayListCommand { get; private set; }
        public RelayCommand RemoveFromPlayListCommand { get; private set; }
        public RelayCommand MoveUpCommand { get; private set; }
        public RelayCommand MoveDownCommand { get; private set; }

        public RelayCommand ExitCommand { get; private set; }

        public event Action PlayRequested;
        public event Action PauseRequested;    
        public event Action StopRequested;

        public event Action ReloadRequested;
       

        public event Action FullScreenRequested;



        public ObservableCollection<Source> PlayList { get; private set; } = new ObservableCollection<Source>();
        

        public ViewModel()
        {
            OpenCommand = new RelayCommand(_ => Open() );
            OpenURLCommand = new RelayCommand(OpenURL);
            PlayPauseCommand = new RelayCommand(_ => PlayPause(),_=> PlayList.Count>0);
            PlayCommand = new RelayCommand(_ => PlayPause(), _ => !(CurrentSource is null) && !CanPause);
            PauseCommand = new RelayCommand(_ => PlayPause(), _ => !(CurrentSource is null) && CanPause);
            StopCommand  = new RelayCommand(_ => StopRequested(), _=> !(CurrentSource is null) && (Status != Status.Stopped) );
            NextCommand  = new RelayCommand(_ => Next(), _=> HasNext);
            PreviousCommand  = new RelayCommand(_ => Previous(), _=> HasPrevious);
            FullScreenCommand = new RelayCommand(_ => FullScreenRequested(), _=> !(CurrentSource is null));
            AddToPlayListCommand = new RelayCommand(_ => AddToPlayList());
            RemoveFromPlayListCommand = new RelayCommand(_ => RemoveFromPlayList(), _ => !(SelectedSource is null) && SelectedSource != CurrentSource);
            MoveUpCommand = new RelayCommand(_ => MoveUp(), _ => CanMoveUp);
            MoveDownCommand = new RelayCommand(_ => MoveDown(), _ => CanMoveDown);

            ExitCommand = new RelayCommand(_ =>  Application.Current.Shutdown() );
        }

        private void MoveDown()
        {
            if (CanMoveDown)
            {
                Source temp = SelectedSource;
                int i = PlayList.IndexOf(SelectedSource);
                PlayList[i] = PlayList[i + 1];
                PlayList[i + 1] = temp;
                SelectedSource = PlayList[i + 1];
            }
        }

        private void MoveUp()
        {
            if (CanMoveUp)
            {
                Source temp = SelectedSource;
                int i = PlayList.IndexOf(SelectedSource);
                PlayList[i] = PlayList[i - 1];
                PlayList[i - 1] = temp;
                SelectedSource = PlayList[i - 1];
            }
        }

        private void RemoveFromPlayList()
        {
            if (!(SelectedSource is null) && SelectedSource != CurrentSource)  
            {                
                PlayList.Remove(SelectedSource);
            }
        }

        private void AddToPlayList()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Media Files|*.wav;*.mp3;*.mp4;*.avi|All Files|*.*";

            if (dialog.ShowDialog() == true)
            {                
                PlayList.Add(new Source(dialog.FileName));
            }
        }

        private void OpenURL(object parameter)
        {
            OpenURLDialog dialog = new OpenURLDialog();
            dialog.Owner = parameter as Window;
            if (dialog.ShowDialog() == true )
            {
                if (dialog.url.Text != "")
                {
                    CurrentSource = new Source(dialog.url.Text);
                    PlayList.Add(currentSource);
                    ReloadRequested();
                }                
            }
            
        }

        private void Open()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Media Files|*.wav;*.mp3;*.mp4;*.avi|All Files|*.*";

            if(dialog.ShowDialog() == true)
            {
                
                CurrentSource = new Source(dialog.FileName); 
                PlayList.Add(CurrentSource);
                ReloadRequested();
            }
            
        }

        private void PlayPause()
        {
            if ((CurrentSource is null) && PlayList.Count>0)
            {
                CurrentSource = PlayList[0];
                ReloadRequested();
            }
            else
            {
                if (Status == Status.Playing)
                {
                    PauseRequested();
                }
                else
                {
                    PlayRequested();
                }
            }

            
        }


        
        

        public void Next()
        {
           if (HasNext)
           {
                CurrentSource = PlayList[PlayList.IndexOf(CurrentSource) + 1];
                ReloadRequested();
           }
           else
           {
                StopRequested();
           }

        }

        private void Previous()
        {
            if (HasPrevious)
            {
                CurrentSource = PlayList[PlayList.IndexOf(CurrentSource) - 1];
                ReloadRequested();
            }
        }





        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
