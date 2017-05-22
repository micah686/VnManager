using System;
using System.Threading.Tasks;
using NUnit.Framework;
using VisualNovelManagerv2.ViewModel.VisualNovels;

namespace VisualNovelManagerv2.Tests.ViewModel.VisualNovels
{
    [TestFixture]
    public class AddVnViewModelTests
    {
        [TestCase()]
        public async Task SearchNameTest()
        {
            AddVnViewModel vm = new AddVnViewModel();
            vm.IsRunning = true;
            vm.IsDropDownOpen = false;
            vm.VnName = "muv";
            await vm.SearchName();
            Assert.IsNotNull(vm.SuggestedNamesCollection);


        }
    }
}