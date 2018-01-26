using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using STVRogue.GameLogic;
using STVRogue;
namespace ClassLibrary1
{
    [TestFixture]
    class NTest_Node
    {
        [Test]
        public void test_CountCreatures()
        {
            Node n = new Node("1");
            Assert.AreEqual(n.CountCreatures(), 0);

            uint aantal = 10;
            n.packs.Add(new Pack("1", aantal, n, null));
            Assert.AreEqual(n.CountCreatures(), aantal);
            
            n.packs.Add(new Pack("2", aantal, n, null));
            n.packs.Add(new Pack("3", aantal*2, n, null));
            n.packs.Add(new Pack("3", aantal*4, n, null));

            Assert.AreEqual(n.CountCreatures(), aantal*8);
        }
    }
}
