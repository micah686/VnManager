using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using NSubstitute;

namespace VisualNovelManagerv2.Tests.ViewModelTests
{
    [TestFixture()]
    public class AddVnViewModelTest
    {
        [TestCase()]
        public void TestVnId()
        {
            AddVnViewModel data = new AddVnViewModel();
            Random rnd = new Random();
            var test1 = Convert.ToUInt32(rnd.Next());
            //data.VnId = Convert.ToUInt32(test1);
            Assert.GreaterOrEqual(data.VnId, 0);
            Assert.IsNotNull(data.VnId);
        }

        [TestCase(@"C:\Data\File.exe")]
        public void TestFileName(string value)
        {
            AddVnViewModel data = new AddVnViewModel {FileName = value};
            Assert.IsNotNull(data.FileName);
        }

        
    }
}
