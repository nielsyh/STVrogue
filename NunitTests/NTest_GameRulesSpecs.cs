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
        List<string> allReplays = new List<string>() { "Combat1", "Combat2", "Combat3", "Combat4", "Combat5", "Combat6" };

        //checks the RZone rule
        [Test]
        public void Always_RZone_Test()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);

            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                List<Tuple<uint, string>> tplist = new List<Tuple<uint, string>>();

                Game game = new Game(gp);

                foreach (Pack p in game.dungeon.alivePacks)
                {
                    Tuple<uint, string> a = new Tuple<uint, string>(game.dungeon.level(p.location), p.id);
                }

                Specification S = new Always(G => check_packs(G, tplist));
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());              
            }
        }

        bool check_packs(Game g, List<Tuple<uint, string>> tplis) {
            foreach (Pack p in g.dungeon.alivePacks) {
                foreach(Tuple<uint, string> t in tplis) {
                    if (t.Item2 == p.id) {
                        if (t.Item1 != g.dungeon.level(p.location)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        //checks the RNode rule
        [Test]
        public void Always_RNode_Test()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);

            foreach (GamePlay gp in plays)
            {
                Specification S = new Always(G => check_nodes(G));
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        bool check_nodes(Game g) {
            foreach (Node n in g.dungeon.graph) {
                if (n.Capacity(g.dungeon) < n.CountCreatures()) return false;
            }
            return true;
        }



        [Test]
        public void Unless_RAlert_Test()
        {
            List<GamePlay> plays = loadSavedGamePlays(allReplays);
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                Game game = gp.getGame();
                bool[] zone_alert = new bool[game.dungeon.difficultyLevel + 2];
                Specification S = new Unless(G => contested_then_alert(G, zone_alert) , G => player_other_zone(G, zone_alert));
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        bool contested_then_alert(Game g, bool[] zone_alert) {
            
            foreach (Node n in g.dungeon.graph) {
                if (n.contested || zone_alert[g.dungeon.level(n)]) {
                    zone_alert[g.dungeon.level(n)] = true;
                    foreach (Pack p in g.dungeon.alivePacks) {
                        if(g.dungeon.level(p.location) == g.dungeon.level(n))
                        {
                            if (!p.alert) return false;
                        }
                    }
                }
            }
            return true;
        }

        bool player_other_zone(Game g, bool[] zone_alert) {
            for (int i = 0; i < zone_alert.Count(); i++) {
                if (zone_alert[i]) { if (g.dungeon.level(g.player.location) == i) return false; } else { zone_alert[i] = false; }
            }
            return true;
        }


        [Test]
        public void LeadsTo_REndZone_Test()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "End" });
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                Game game = gp.getGame();
                bool[] zone_alert = new bool[game.dungeon.difficultyLevel + 2];
                Specification S = new LeadsTo(G => player_last_zone(G), G => packs_in_last_zone_are_alert(G));
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        bool player_last_zone(Game g)
        {
            if(g.dungeon.level(g.player.location) == g.dungeon.level(g.dungeon.exitNode)) { return true; }
            return false;
        }

        bool packs_in_last_zone_are_alert(Game g) {
            foreach (Pack p in g.dungeon.alivePacks) {
                if (g.dungeon.level(p.location) == g.dungeon.level(g.dungeon.exitNode)) {
                    if (!p.alert) return false;
                }
            }
            return true;
        }

        [Test]
        public void LeadsTo_RBridge_Test()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "End" });
            foreach (GamePlay gp in plays)
            {
                // test that this specifation is respected by each game-play:
                Game game = gp.getGame();
                List<Tuple<Bridge, int>> hungry_bridges = new List<Tuple<Bridge, int>>();
                Specification S = new LeadsTo(G => bridge_is_hungry(G, hungry_bridges), G => packs_in_last_zone_are_alert(G));
                gp.replay(S);
                Assert.IsTrue(S.getVerdict());
            }
        }

        bool bridge_is_hungry(Game g, List<Tuple<Bridge, int>> list) {         
            foreach (Bridge b in g.dungeon.bridges)
            {
                uint cap = (b.Capacity(g.dungeon)) / 2;
                if (cap < b.CountCreatures())
                {
                    //find dichtsbijzijnde pack
                    Pack a = g.dungeon.find_closest_pack(b);
                    if (a != null && !a.alert)
                    {
                        Tuple<Bridge, int> t = new Tuple<Bridge, int>(b, Predicates.shortestpath(a.location, b, g.dungeon.graph.Count).Count);
                        list.Add(t);
                        return true;                 
                    }
                }
            }
            return false;
        }

        bool bridge_less_hungry(Game g, List<Tuple<Bridge, int>> list) {

            foreach (Tuple<Bridge, int> t in list) {

            }

            return true;
        }


        [Test]
        public void LevelShrinksTest()
        {
            List<GamePlay> plays = loadSavedGamePlays(new List<string>() { "Shrink" });
            foreach (GamePlay gp in plays)
            {
                int startGraphSize = Predicates.reachableNodes(gp.getGame().dungeon.startNode).Count;
                List<Specification> XS = new List<Specification>() { new LeadsTo(G => startGraphSize == Predicates.reachableNodes(G.dungeon.startNode).Count, G => Predicates.reachableNodes(G.dungeon.startNode).Count < startGraphSize) };
            

                Specification S = new Always(G => G.gp.PlayerCommands.OfType<UseItemCommand>().Where(x => x.option == 1).Count() > 1); 

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
                    gp.CreateGame();
                    result.Add(gp);
                }
                Console.WriteLine("GAME CREATED FROM SAVEFILE ________________________");
            }
            return result;
        }
    }
}
