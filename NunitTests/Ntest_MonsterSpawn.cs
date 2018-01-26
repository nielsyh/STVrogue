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
    class Ntest_MonsterSpawn
    {
        [Test]
        public void testSpawnNoMonster()
        {
            Game game = new Game(5, 1, 0);
            Assert.IsTrue(game.dungeon.MonsterHP() == 0);
            Assert.IsTrue(game.dungeon.alivePacks.Count() == 0);
        }

        void SpawnTooManyMonsters()
        {
            RandomGenerator.initializeWithSeed(123461);
            Game game = new Game(5, 1, 200);
        }

        [Test]
        public void testSpawnTooManyMonsters()
        {
            TestDelegate spawnTooManyMonsters = new TestDelegate(SpawnTooManyMonsters);
            Assert.Throws(typeof(GameCreationException), spawnTooManyMonsters);
            //http://www.blackwasp.co.uk/nunitexceptionasserts.aspx Example of implementation
        }

        void SpawnTooManyMonstersInExit()
        {
            RandomGenerator.initializeWithSeed(764);
            Game game = new Game(1, 1, 17);
        }

        [Test]
        public void TestSpawnTooManyNodesInExit()
        {
            TestDelegate spawnTooManyMonstersInExit = new TestDelegate(SpawnTooManyMonstersInExit);
            Assert.Throws(typeof(GameCreationException), spawnTooManyMonstersInExit);
            //Assert.Throws(typeof(Exception), spawnTooManyMonsters);
        }

        [Test]
        public void testHPProperty()
        {
            RandomGenerator.initializeWithSeed(55656565);
            Game game = new Game(5, 1, 20);
            int monsterhp = game.dungeon.MonsterHP();
            int potionhp = game.dungeon.PotionHP();

            List<Monster> totalMonsters = new List<Monster>();
            foreach (Pack pack in game.dungeon.alivePacks)
            {
                foreach (Monster monster in pack.members)
                {
                    if (monster.HP > 0)
                        totalMonsters.Add(monster);
                }
            }

            bool hpFormula = (potionhp <= 0.8 * monsterhp);
            


            Assert.AreEqual(Predicates.hpProperty(game.dungeon.totalPotions, totalMonsters), hpFormula);
        }


        //todo test monstter hp
    }
}
