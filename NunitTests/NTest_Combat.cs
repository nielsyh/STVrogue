using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using STVRogue.GameLogic;
using STVRogue;
using STVRogue.Utils;
using NUnit.Framework.Constraints;

namespace STVRogue.GameLogic
{
    [TestFixture]
    class Ntest_Combat
    {
        void SpawnWithSeed()
        {
        }

        [Test]
        public void killCreature()//Kill pack AND creature
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            game.player.dungeon = game.dungeon;


            Node n = new Node();
            Pack pack = new Pack("999", 1, n, null);
            game.player.location = n;
            n.packs.Add(pack);
            n.contested = true;
            pack.members[0].HP = 5;

            Console.WriteLine(pack.members[0].HP); //hp is 5

            AttackCommand attack = new AttackCommand();
            attack.Execute(game.player,game.dungeon);


            Assert.AreEqual(1, game.player.KillPoint);
            Assert.AreEqual(0, n.packs.Count);
            Assert.AreEqual(0, pack.members.Count);
        }

        [Test]
        public void acceleratedAttack()
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            game.player.dungeon = game.dungeon;


            Node n = new Node();
            Pack pack = new Pack("999", 5, n, null);
            game.player.location = n;
            n.packs.Add(pack);
            n.contested = true;
            game.player.accelerated = true;

            //set HP to 10, so player can't kill them.
            foreach (Monster m in pack.members.ToList())
                m.HP = 10;

            AttackCommand attack = new AttackCommand();
            attack.Execute(game.player,game.dungeon);


            Assert.AreEqual(0, game.player.KillPoint);
            Assert.AreEqual(1, n.packs.Count);
            Assert.AreEqual(5, pack.members.Count);

            foreach (Monster m in pack.members.ToList())
                Assert.AreEqual(5, m.HP);
        }

        [Test]
        public void acceleratedKillAllMonsters()
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            game.player.dungeon = game.dungeon;


            Node n = new Node();
            Pack pack = new Pack("999", 5, n, null);
            game.player.location = n;
            n.packs.Add(pack);
            n.contested = true;
            game.player.accelerated = true;

            //set HP to 10, so player can't kill them.
            foreach (Monster m in pack.members.ToList())
                m.HP = 1;

            AttackCommand attack = new AttackCommand();
            attack.Execute(game.player,game.dungeon);


            Assert.AreEqual(5, game.player.KillPoint);
            Assert.AreEqual(0, n.packs.Count);
            Assert.AreEqual(0, pack.members.Count);
        }


        [Test]
        public void PackAttackPlayer()
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            game.player.dungeon = game.dungeon;


            Node n = new Node();
            Pack pack = new Pack("999", 5, n, null);
            game.player.location = n;
            n.packs.Add(pack);
            n.contested = true;

            Assert.IsTrue(game.player.HP == 100);

            n.fight(game.player, game.player.dungeon);

            Assert.IsTrue(game.player.HP == 95);

        }

        [Test]
        public void PackTest()
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            game.player.dungeon = game.dungeon;


            Node n = new Node();
            Pack pack = new Pack("999", 5, n, null);
            game.player.location = n;
            n.packs.Add(pack);
            n.contested = true;

            Assert.IsTrue(game.player.HP == 100);

            n.fight(game.player, game.player.dungeon);

            Assert.IsTrue(game.player.HP == 95);

        }

        [Test]
        public void KillPlayer()
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            game.player.dungeon = game.dungeon;


            Node n = new Node();
            Pack pack = new Pack("999", 5, n, null);
            game.player.location = n;
            n.packs.Add(pack);
            n.contested = true;
            game.player.HP = 5;

            Assert.IsTrue(game.player.HP == 5);

            n.fight(game.player, game.player.dungeon);

            Assert.IsTrue(game.player.HP == 0);
            Assert.IsFalse(n.contested);
        }

        [Test]
        public void TryCommitSudoku()
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            game.player.dungeon = game.dungeon;
            Node n = new Node("0000");
            game.player.location = n;

            Assert.Throws<ArgumentException>(() => game.player.Attack(game.player));
        }

        [Test]
        public void getNoRandomFleeNode()
        {
            RandomGenerator.initializeWithSeed(123);
            Game game = new Game(5, 2, 20);
            Pack pack = new Pack("999", 5, game.dungeon.graph[5], game.dungeon);
            game.dungeon.graph[5].packs.Add(pack);
            pack.dungeon = game.dungeon;
            game.player.location = pack.location;

            foreach (Node neighbour in pack.location.neighbors)
            {
                if (neighbour.CountCreatures() < neighbour.Capacity(game.dungeon))
                {
                    uint space = neighbour.Capacity(game.dungeon) - (uint)neighbour.CountCreatures();
                    Pack neighbourPack = new Pack(neighbour.id + "abc", space, neighbour, game.dungeon);
                    neighbour.packs.Add(neighbourPack);
                }

            }
            pack.startingHP = 10000;
            foreach (Monster member in pack.members.ToList())
                member.HP = 1;

            pack.location.fight(game.player, game.dungeon);
            Assert.AreEqual(pack.location, game.player.location); //location stays same(in other words, pack didnt move)
        }

        [Test]
        public void GetRandomFleeNode()
        {
            Game game = new Game(5, 2, 20,100);
            Pack pack = new Pack("999", 1, game.dungeon.startNode, game.dungeon);
            game.dungeon.graph[5].packs.Add(pack);
            pack.dungeon = game.dungeon;   

            pack.startingHP = 10000;
            foreach (Monster member in pack.members.ToList())
                member.HP = 1;
            Node randomNode = pack.getRandomValidNeighbour(game.dungeon);
            Assert.IsTrue(pack.location.id != randomNode.id && pack.location.neighbors.Contains(randomNode)); 
        }

        
    }
}