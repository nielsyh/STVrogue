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
    class NTest_Commands
    {
        [Test]
        public void MoveCommand()
        {
            Game game = new Game(5, 2, 20);
           
            while (game.player.location != game.dungeon.exitNode)
            {
                MoveCommand m = new MoveCommand(RandomGenerator.rnd.Next(game.player.location.neighbors.Count));
                m.Execute(game.player,game.dungeon);
            }

            Assert.AreEqual(game.player.location.id, game.dungeon.exitNode.id);
        }

        [Test]
        public void MoveCommandInvalidDungeon()
        {
            Game game = new Game(5, 2, 20);

            Dungeon d = new Dungeon(1, 1);
            d.startNode = new Node("1");
            d.exitNode = new Node("2");
            game.player.location = d.startNode;

            game.dungeon = d;

            MoveCommand cmd = new MoveCommand();

            Assert.Throws<ArgumentException>(() => cmd.Execute(game.player,game.dungeon));
        }

        [Test]
        public void UseItemCommand()
        {
            Dungeon d = new Dungeon(1, 1);
            Game game = new Game(5, 2, 20);

            game.player.location = d.startNode;
            game.player.dungeon = d;

            game.player.bag.Add(new HealingPotion("p1"));
            game.player.bag.Add(new HealingPotion("p2"));

            UseItemCommand cmd = new UseItemCommand();
            cmd.Execute(game.player,game.dungeon);

            Assert.AreEqual(1, game.player.bag.Count);

            cmd = new UseItemCommand();
            cmd.Execute(game.player,game.dungeon);

            Assert.AreEqual(0, game.player.bag.Count);
        }

        [Test]
        public void UseItemCommandWithEmptyBag()
        {
            Game game = new Game(5, 2, 20);
            Dungeon d = new Dungeon(1, 1);
            game.dungeon = d;
            game.player.location = d.startNode;
            UseItemCommand cmd = new UseItemCommand();
            Assert.Throws<ArgumentException>(() => cmd.Execute(game.player,game.dungeon));
        }
        [Test]
        public void UpdateGameCommand()
        {
            Game g = new Game(1,1,1);
            g.update(new MoveCommand());
            Assert.True(g.dungeon.startNode != g.player.location);
        }

        [Test]
        public void AttackCommand()
        {
            //check if attackcommand attacks monsters
            Game game = new Game(5, 2, 20);

            Dungeon d = new Dungeon(1, 0);
            Pack pack = new Pack("1", 1, d.startNode, d);
            game.dungeon = d;
            game.player.location = d.startNode;
            d.startNode.packs.Add(pack);
            d.startNode.contested = true;
            Player p = new Player();
            p.location = d.startNode;
            p.dungeon = d;

            //superpowers
            p.AttackRating = 100;

            AttackCommand cmd = new AttackCommand();
            cmd.Execute(game.player,game.dungeon);

            Assert.True(!p.location.contested && p.location.packs.Count == 0);
        }

        [Test]
        public void AttackCommandNoEnemy()
        {
            //check if attackcommand throws exception on no enemy to attack
            Game game = new Game(5, 2, 20);
            Dungeon d = new Dungeon(1, 0);
            game.dungeon = d;

            game.player.location = d.startNode;

            AttackCommand cmd = new AttackCommand();
            Assert.Throws<ArgumentException>(() => cmd.Execute(game.player,game.dungeon));
        }
    }
}
