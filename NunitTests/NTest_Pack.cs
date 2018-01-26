using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using STVRogue.GameLogic;
using STVRogue;
using STVRogue.Utils;
namespace ClassLibrary1
{
    [TestFixture]
    class NTest_Pack
    {
        [Test]
        public void NTest_Pack_Move_Small()
        {
            Dungeon d = new Dungeon(10, 1);
            Pack p = new Pack("1", 1, d.startNode, d);
            p.dungeon = d;
            p.location = d.startNode;
            d.startNode.packs.Add(p);
            p.move(d.startNode.neighbors[0]);
            Assert.True(p.location == d.startNode.neighbors[0]);
        }
        [Test]
        public void NTest_Pack_Move_To_ExitNode()
        {
            Dungeon d = new Dungeon(10, 1);
            Pack p = new Pack("1", 1, d.exitNode.neighbors[0], d);
            p.dungeon = d;
            p.location = d.exitNode.neighbors[0];
            d.exitNode.neighbors[0].packs.Add(p);
            Node moveTo = d.exitNode;
            p.move(moveTo);
            Assert.IsFalse(moveTo == p.location);
        }
        [Test]
        public void NTest_Pack_Move_To_Invalid()
        {
            Dungeon d = new Dungeon(10, 1);
            Pack p = new Pack("1", 1, d.startNode,d);
            p.dungeon = d;
            p.location = d.startNode;
            d.startNode.packs.Add(p);
            Assert.Throws<ArgumentException>(() => p.move(d.exitNode));
        }
        [Test]
        public void NTest_Pack_No_Capacity()
        {
            Dungeon d = new Dungeon(10, 1);
            Pack p = new Pack("1", 10000, d.startNode,d);
            p.dungeon = d;
            p.location = d.startNode;
            d.startNode.packs.Add(p);
            Node moveTo = d.startNode.neighbors[0];
            p.move(moveTo);
            Assert.IsFalse(moveTo == p.location);
        }

        [Test]
        public void NTest_Pack_Move_Towards()
        {
            Dungeon d = new Dungeon(2, 1);
            Pack p = new Pack("1", 0, d.startNode,d);

            p.dungeon = d;

            Node start = d.startNode;
            Node goal = d.bridges.First();

            
            while(p.location != goal)//foreach(Node _ in path)
            {
                p.moveTowards(goal);
                Console.WriteLine($"loc: {p.location.id}");

            }
            Assert.True(p.location.id == goal.id);
        }
    }
}
