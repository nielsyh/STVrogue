using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.Utils;

namespace  STVRogue.GameLogic
{
    [TestFixture]
    public class NTesT_Items
    {
        [Test]
        public void Test_useItem()
        {
            Item item = new Item();
            Player player = new Player();

            item.use(player);

            Assert.AreEqual(item.used, true);
        }
        [Test]
        public void Test_usePotion()
        {
            HealingPotion potion = new HealingPotion("0");
            Player player = new Player();

            potion.use(player);

            Assert.AreEqual(potion.used, true);
        }

        [Test]
        public void Test_useCrystal()
        {
            Crystal crystal1 = new Crystal("0");
            Player player = new Player();
            Node node = new Node("1");
            player.location = node;

            crystal1.use(player);

            Assert.AreEqual(crystal1.used, false);
            Assert.AreEqual(player.accelerated, false);

            Crystal crystal2 = new Crystal("2");
            player.location.contested = true;
            crystal2.use(player);

            Assert.AreEqual(crystal2.used, true);
            Assert.AreEqual(player.accelerated, true);
        }
    }
}