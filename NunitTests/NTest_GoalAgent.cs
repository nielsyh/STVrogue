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
    class NTest_GoalAgent
    {
        [Test]
        public void NoOldZoneTest()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "ZoneUpAgent" }); //moves to Zone 3.

            foreach (GamePlay gp in plays)
            {
                List<Specification> antecedents = new List<Specification>() { { new Always(G => false) }, { new LeadsTo(G => true, G => G.player.location.nodeLevel == 1) },
                    { new LeadsTo(G => G.player.location.nodeLevel == 1, G => G.player.location.nodeLevel > 1) }, { new LeadsTo(G => G.player.location.nodeLevel == 2, G => G.player.location.nodeLevel > 2) },
                    { new Always(G => !(G.player.location.nodeLevel >= 4)) } };
                Specification consequent = new LeadsTo(G => true, G => G.player.location.nodeLevel == 3);
                // test that this specifation is respected by each game-play:
                Specification S = new Condition(antecedents, consequent);
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        [Test]
        public void NotPastZoneXTest()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "ZoneDownAgent" }); //moves up to Zone 4, then down to zone 2.

            foreach (GamePlay gp in plays)
            {
                List<Specification> antecedents = new List<Specification>() { { new Always(G => false) }, { new LeadsTo(G => true, G => G.player.location.nodeLevel == 1) },
                    { new LeadsTo(G => G.player.location.nodeLevel == 1, G => G.player.location.nodeLevel > 1) }, { new LeadsTo(G => G.player.location.nodeLevel == 2, G => G.player.location.nodeLevel > 2) },
                    { new LeadsTo(G => G.player.location.nodeLevel == 3, G => G.player.location.nodeLevel > 3) }, { new LeadsTo(G => G.player.location.nodeLevel == 4, G => G.player.location.nodeLevel < 4) },
                    { new LeadsTo(G => G.player.location.nodeLevel == 3, G => G.player.location.nodeLevel < 3) }, { new Always(G => !(G.player.location.nodeLevel >= 5)) } };
                Specification consequent = new LeadsTo(G => true, G => G.player.location.nodeLevel == 2);
                // test that this specifation is respected by each game-play:
                Specification S = new Condition(antecedents, consequent);
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        [Test]
        public void MurderAgent()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "KillAgent" }); //kills 17 monsters (without using a crystal -> can not accidentally kill more than 17)

            foreach (GamePlay gp in plays)
            {
                List<Specification> antecedents = new List<Specification>() { { new Always(G => false) }, { new LeadsTo(G => true, G => G.player.KillPoint == 0) }, { new LeadsTo(G => G.player.KillPoint == 0, G => G.player.KillPoint > 0)},
                    { new Always(G=> !(G.player.KillPoint > 17) ) } };
                Specification consequent = new LeadsTo(G => true, G => G.player.KillPoint == 17);
                // test that this specifation is respected by each game-play:
                Specification S = new Condition(antecedents, consequent);
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        [Test]
        public void GoalAgent()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "GoalAgent" }); //Passive agent that only focuses on finding the exit, and finishes the game.

            foreach (GamePlay gp in plays)
            {
                List<Specification> antecedents = new List<Specification>() { { new Always(G => false) }, { new LeadsTo(G => true, G => G.player.KillPoint == 0) }, { new LeadsTo(G => true, G => G.player.location.nodeLevel == 1)},
                    { new LeadsTo(G => G.player.location.nodeLevel == 1, G => G.player.location.nodeLevel > 1) },
                    { new Always(G=> (G.player.KillPoint == 0) ) } };
                Specification consequent = new LeadsTo(G => true, G => G.player.location == G.dungeon.exitNode);
                // test that this specifation is respected by each game-play:
                Specification S = new Condition(antecedents, consequent);
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
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
                    result.Add(gp);
                }
                Console.WriteLine("GAME CREATED FROM SAVEFILE ________________________");
            }
            return result;
        }
    }
}
