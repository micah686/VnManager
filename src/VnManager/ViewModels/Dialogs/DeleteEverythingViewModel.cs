using System;
using System.Collections.Generic;
using System.Media;
using System.Text;
using System.Timers;
using Stylet;

namespace VnManager.ViewModels.Dialogs
{
    public class DeleteEverythingViewModel: Screen, IDisposable
    {
        private readonly Timer _timer= new Timer(1500);
        private int _timerHitCount = 0;
        public bool CanPressDelete { get; set; }
        public string YesButtonTest { get; private set; }

        public DeleteEverythingViewModel()
        {
            CanPressDelete = false;
            YesButtonTest = "5";
            SystemSounds.Asterisk.Play();
            _timer.AutoReset = true;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            switch (_timerHitCount)
            {
                case 0:
                    _timerHitCount += 1;
                    YesButtonTest = "4";
                    break;
                case 1:
                    _timerHitCount += 1;
                    YesButtonTest = "3";
                    break;
                case 2:
                    _timerHitCount += 1;
                    YesButtonTest = "2";
                    break;
                case 3:
                    _timerHitCount += 1;
                    YesButtonTest = "1";
                    break;
                case 4:
                    _timerHitCount += 1;
                    YesButtonTest = App.ResMan.GetString("Yes");
                    CanPressDelete = true;
                    _timer.Stop();
                    break;
                default:
                    _timer.Stop();
                    break;
            }
        }

        public void DeleteEverything()
        {
            RequestClose(true);
        }

        public void Cancel()
        {
            RequestClose(false);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
