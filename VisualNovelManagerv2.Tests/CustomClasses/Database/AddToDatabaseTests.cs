using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace VisualNovelManagerv2.Tests.CustomClasses.Database
{
    [TestFixture()]
    public class AddToDatabaseTests
    {
        [TestCase(564, @"D:\Documents\Random Files\HxD.exe", @"D:\Documents\Random Files\kudo_by_vrtrojan-d5u468b.ico")]
        public void GetIdTest(int parameter, string fn, string icon)
        {
            var id = parameter;

            NUnit.Framework.Assert.IsNotNull(id);
            NUnit.Framework.Assert.Greater(parameter, 0);
            NUnit.Framework.Assert.IsNotNull(fn);
            
            //Assert.Fail();
        }

        [TestMethod()]
        public void CheckForDbNullTest()
        {
            Assert.Fail();
        }
    }
}