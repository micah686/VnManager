using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Input.Manipulations;
using Stylet;
using VnManager.Extensions;
using VnManager.Models.Db.User;

namespace VnManager.ViewModels.UserControls.MainPage.Vndb
{
    public class VndbContentViewModel: Conductor<Screen>.Collection.OneActive
    {

        internal static int VnId { get; private set; }

        internal static UserDataGames SelectedGame { get; private set; }
        

        internal bool IsGameRunning { get; set; }
        internal List<Process> ProcessList { get; set; } = new List<Process>();
        internal Stopwatch GameStopwatch { get; set; } = new Stopwatch();
        internal Timer CheckProcessesTimer { get; set; }
        
        public VndbContentViewModel()
        {
            CheckProcessesTimer = new Timer() {AutoReset = true, Interval = 30000};
            CheckProcessesTimer.Elapsed += CheckProcessesTimerOnElapsed;
            
            
            var vInfo = new VndbInfoViewModel { DisplayName = App.ResMan.GetString("Main") };
            var vChar = new VndbCharactersViewModel { DisplayName = App.ResMan.GetString("Characters") };
            var vScreen = new VndbScreensViewModel { DisplayName = App.ResMan.GetString("Screenshots") };

            Items.Add(vInfo);
            Items.Add(vChar);
            Items.Add(vScreen);
        }

        private void CheckProcessesTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var newProcList = new List<Process>();
            newProcList.AddRange(ProcessList);
            var existingProcessIds = new List<int>();
            existingProcessIds.AddRange(ProcessList.Select(x => x.Id));
            foreach (var process in ProcessList)
            {
                var childProcesses = process.GetChildProcesses().ToList();
                childProcesses = childProcesses.Where(p => existingProcessIds.Any(p2 => p2 != p.Id)).ToList();
                existingProcessIds.AddRange(childProcesses.Select(p => p.Id));
                newProcList.AddRange(childProcesses);
            }

            newProcList = newProcList.Where(p => p.HasExited == false).ToList();
            ProcessList = newProcList;
            if (ProcessList.Count == 0)
            {
                CheckProcessesTimer.Stop();
            }
        }


        internal static void SetSelectedGame(UserDataGames game)
        {
            SelectedGame = game;
            VnId = SelectedGame.GameId;
        }
        

        public static void CloseClick()
        {
            RootViewModel.Instance.ActivateMainClick();
            SelectedGame = new UserDataGames();
        }
    }

    
}
