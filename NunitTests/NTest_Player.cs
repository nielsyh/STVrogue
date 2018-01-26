using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace STVRogue.GameLogic
{

    /* An example of a test class written using NUnit testing framework. 
     * This one is to unit-test the class Player. The test is incomplete though, 
     * as it only contains two test cases. 
     */
    [TestFixture]
    public class NTest_Player
    {

        [Test]
        public void NTest_use_onEmptyBag()
        {
            Player P = new Player();
            Assert.Throws<ArgumentException>(() => P.use(new Item()));
        }

        [Test]
        public void NTest_use_item_in_bag()
        {
            Player P = new Player();
            Item x = new HealingPotion("pot1");
            P.bag.Add(x);
            P.use(x);
            Assert.True(x.used) ;
            Assert.False(P.bag.Contains(x));
        }
        [Test]
        public void NTest_use_used_item()
        {
            Player P = new Player();
            Item x = new HealingPotion("pot1");
            P.bag.Add(x);
            P.use(x);
            Assert.True(x.used);
            Assert.False(P.bag.Contains(x));
            P.bag.Add(x);
            //P.use(x);
            Assert.Throws<ArgumentException>(() => P.use(x));

        }
        [Test]
        public void NTest_use_crystal_on_bridge()
        {
            Dungeon a = new Dungeon(10, 1);
            Player P = new Player();
            
            P.location = a.bridges[0];

            a.bridges[0].contested = true;

            P.dungeon = a;

            Item x = new Crystal("c1");
            P.bag.Add(x);
            P.use(x);

            Assert.True(x.used);
            Assert.False(P.bag.Contains(x));
            Assert.True(P.location == a.startNode);
        }
        [Test]
        public void NTest_use_crystal_on_bridge_not_in_combat()
        {
            Dungeon a = new Dungeon(10, 1);
            Player P = new Player();

            P.location = a.bridges[0];

            a.bridges[0].contested = false;

            P.dungeon = a;

            Item x = new Crystal("c1");
            P.bag.Add(x);
            Assert.Throws<ArgumentException>(() => P.use(x));
            Assert.True(P.bag.Contains(x));
            Assert.True(P.location == a.bridges[0]);
        }
        [Test]
        public void NTest_item_use()
        {
            Dungeon a = new Dungeon(10, 1);
            Player P = new Player();
            P.dungeon = a;
            P.location = a.startNode;
            a.startNode.contested = true;
            Item x = new HealingPotion("p1");
            x.use(P);

            Assert.Throws<ArgumentException>(() => x.use(P));
        }
        [Test]
        public void NTest_Player_Move()
        {
            Dungeon a = new Dungeon(10, 1);
            Player P = new Player();
            P.dungeon = a;
            P.location = a.startNode;
            P.Move(a.startNode.neighbors[0]);

            Assert.True(P.location == a.startNode.neighbors[0]);
        }
        [Test]
        public void NTest_Player_Move_To_Invalid_Location()
        {
            Dungeon a = new Dungeon(10, 1);
            Player P = new Player();
            P.dungeon = a;
            P.location = a.startNode;

            Assert.Throws<ArgumentException>(() => P.Move(null));
        }

        [Test]
        public void NTest_Player_Move_To_Node_With_Pack()
        {
            Dungeon a = new Dungeon(10, 1);
            Player P = new Player();
            P.dungeon = a;
            P.location = a.startNode;

            //move to node with pack
            Node to = P.location.neighbors[0];
            to.contested = false;
            Pack pack = new Pack("1", 2, to, a);
            to.packs.Add(pack);

            P.Move(to);

            //check if player arrived and node is contested
            Assert.True(P.location == to && to.contested);

        }
    }
}
