using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using NUnit.Framework.Internal;
using VisualNovelManagerv2.ViewModel.VisualNovels;
using VndbSharp.Models;

namespace VisualNovelManagerv2.Tests
{
    [TestFixture()]
    public class UnitTest1
    {
        [TestCase]
        public void TestMethod1()
        {
            AddVnViewModel data = new AddVnViewModel();
            data.VnId = 0;
            NUnit.Framework.Assert.GreaterOrEqual(data.VnId, 0);


        }
    }
}
