using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using STVRogue.GameLogic;
using STVRogue;
using STVRogue.Utils;
using System.IO;
using System.Xml.Serialization;
using STVRogue_Main;
namespace ClassLibrary1
{
    [TestFixture]
    class NTest_Replay
    {
        List<string> allReplays = new List<string>() { "Combat1", "Combat2", "Combat3", "Combat4", "Combat5", "Combat6" }; 

        //1
        [Test]
        public void AlwaysPredicate()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);

            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                Specification S = new Always(G => G.player.HP > 0);
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        //2
        [Test]
        public void UnlessPredicate()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                int M = (int)gp.numberOfMonsters;
                Specification S = new Unless(G => countMonsters(G) == M, G => countMonsters(G) < M);
                Console.WriteLine("Test");
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }
        //3
        [Test]
        public void LeadsToPredicate()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                int M = (int)gp.numberOfMonsters;
                Specification S = new LeadsTo(G => countMonsters(G) == M, G => countMonsters(G) < M); // countMonsters(G) < M
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }
        //3
        [Test]
        public void LeadsToPredicate2()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                Specification S = new LeadsTo(G => G.player.location == G.dungeon.startNode, G=> G.player.location != G.dungeon.startNode ); // countMonsters(G) < M
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        //example 4
        [Test]
        public void ConditionPredicateBridge()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                List<Specification> XS = new List<Specification>() { new LeadsTo(G => true, G => G.player.location == G.dungeon.exitNode) };
                //the Predicates.isBridge method removes nodes from the graph???? temp solution below
                Specification S = new LeadsTo(G => G.player.location == G.dungeon.startNode, G => G.player.location is Bridge); //werkt wel

                //Specification S = new LeadsTo(G => G.player.location == G.dungeon.startNode, G => Predicates.isBridge(G.dungeon.startNode,G.dungeon.exitNode,G.player.location));
                Specification todo = new Condition(XS, S);

                gp.replay(todo);
                Assert.IsTrue(todo.getVerdict());
            }

        }

        //example 5
        [Test]
        public void ConditionPredicateKP()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);
            foreach (GamePlay gp in plays)
            {
                int M = (int)gp.numberOfMonsters;

                // test that this specifation is respected by each game-play:
                List<Specification> XS = new List<Specification>() { new LeadsTo(G => true, G => G.player.KillPoint > 0) };

                Specification S = new LeadsTo(G => countMonsters(G)==M, G => countMonsters(G) < M); //werkt wel

                Specification todo = new Condition(XS, S);

                gp.replay(todo);
                Assert.IsTrue(todo.getVerdict());
            }

        }
        //'against' test 2.4
        [Test]
        public void AgainstTest()
        {
            Assert.IsTrue(RunTest(allReplays) && !RunTest(new List<string>() { "AgainstS" }));
        }

        public bool RunTest(List<string> replays)
        {
            bool verdict = true;
            List<GamePlay> plays = loadSavedGamePlays(replays);
            foreach (GamePlay gp in plays)
            {
                int M = (int)gp.numberOfMonsters;


                //player moves to exit node
                List<Specification> XS = new List<Specification>() { new Always(G => false), new LeadsTo(G => true, G => G.player.location == G.dungeon.exitNode) };

                //at least 1 mosnter died
                Specification S = new LeadsTo(G => countMonsters(G) == M, G => countMonsters(G) < M); //werkt wel

                Specification todo = new Condition(XS, S);

                gp.replay(todo);
                verdict = verdict && todo.getVerdict();
            }
            return verdict;
        }

        [Test]
        public void TestReplay()
        {

            Game g = new Game(5, 2, 10);
            Agent a = new BloodthirstyAgent(g, 1);

            while (g.player.location != g.dungeon.exitNode)
                a.DoTurn();

            Assert.IsFalse(g.gp.InSimulation());

            g.gp.Serialize("ReplayTest");

            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "ReplayTest" });
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                Specification S = new Always(G => G.player.HP > 0);

                gp.replay(S);
            }
        }
       
        public int countMonsters(Game G)
        {
            int sum = 0;
            var graph = Predicates.reachableNodes(G.dungeon.startNode);
            foreach (Node n in graph)
                foreach (Pack p in n.packs)
                    sum += p.members.Count;
            return sum;
        }


        public List<GamePlay> loadSavedGamePlays(List<string> xs)
        {
            List<GamePlay> result = new List<GamePlay>();
            XmlSerializer xml = new XmlSerializer(typeof(GamePlay), new Type[] { typeof(MoveCommand), typeof(UseItemCommand), typeof(AttackCommand) });
            foreach (string s in xs)
            {
                using (Stream reader = new FileStream(ReplayDirectory.Get(s+".rpl"), FileMode.Open))
                {
                    GamePlay gp = (GamePlay)xml.Deserialize(reader);
                    result.Add(gp);
                }
                Console.WriteLine("GAME CREATED FROM SAVEFILE ________________________");
            }
            return result;
        }
    }
}
