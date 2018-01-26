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

namespace STVRogue.GameLogic
{
    [TestFixture]
    class NTest_RGameRules
    {

        [Test]
        public void Always_RZone_Test()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "Combat1", "End" });

            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                List<Tuple<uint, Pack>> tplist = new List<Tuple<uint, Pack>>();
                  
                foreach (Pack p in G.dungeon.alivePacks) {
                    Tuple<uint, Pack> a = new Tuple<uint, Pack>(G.dungeon.level(p.location),p);
                }

                for (int i = 0; i < tplist.Count; i++) {            
                    Specification S = new Always(G => G.dungeon.level(tplist[i].Item2.location) == tplist[i].Item1);
                }
                
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        [Test]
        public void UnlessPredicate()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "Combat1", "End" });
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
                using (Stream reader = new FileStream(ReplayDirectory.Get(s + ".rpl"), FileMode.Open))
                {
                    GamePlay gp = (GamePlay)xml.Deserialize(reader);
                    gp.CreateGame();
                    result.Add(gp);
                }
                Console.WriteLine("GAME CREATED FROM SAVEFILE ________________________");
            }
            return result;
        }



    }
}
