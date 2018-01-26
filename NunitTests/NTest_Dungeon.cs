using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.Utils;

namespace STVRogue.GameLogic
{
    [TestFixture]
    public class NTest_Dungeon
    {

        [Test]
        public void ShortestPathTest()
        {
            Dungeon a = new Dungeon(1, 1);
            a.graph.Clear();
            //Build a dungeon
            Node u = new Node();
            Node v = new Node();
            Node w = new Node();
            Node x = new Node();
            
            u.id = ("" + 0);
            v.id = ("" + 1);
            w.id = ("" + 2);
            x.id = ("" + 3);

            // 0 --> 1 --> 2
            u.connect(v);
            v.connect(w);

            a.graph.Add(u);
            a.graph.Add(v);
            a.graph.Add(w);
            a.graph.Add(x);

            a.dungeon_size = a.graph.Count;
            // u --> v = 2 (Min path size)
            List<Node> result = Predicates.shortestpath(u, v, a.dungeon_size);
            Assert.AreEqual(Predicates.find_all_paths_extract_shortest(u,v,a.dungeon_size), result.Count);
            // u --> w = 3 (path size)
            result = Predicates.shortestpath(u, w, a.dungeon_size);
            Assert.AreEqual(Predicates.find_all_paths_extract_shortest(u, w, a.dungeon_size), result.Count);
            // u --> x = 0 (not possible)
            result = Predicates.shortestpath(u, x, a.dungeon_size);
            Assert.AreEqual(Predicates.find_all_paths_extract_shortest(u, x, a.dungeon_size), result.Count);
        }

        [Test]
        public void generateRandomDungeonTest_level5()
        {
            Dungeon a = new Dungeon(5, 1);
            int num_of_bridges = 0;
            for(int i = 0; i < a.graph.Count; i++)
            {
                if(a.graph[i] is Bridge) {
                    num_of_bridges++;
                }
            }
            Assert.AreEqual(Predicates.isValidDungeon(a.startNode,a.exitNode,5), true);
        }

        [Test]
        public void generateRandomDungeonTest_level10()
        {
            Dungeon a = new Dungeon(10, 1);
            Assert.AreEqual(Predicates.isValidDungeon(a.startNode, a.exitNode, 10), true);
        }

        [Test]
        public void DungeonIsValidTest_1()
        {
            Dungeon a = new Dungeon(10, 1);
            a.graph.Clear();

            Node u = new Node();
            Node v = new Node();
            Node w = new Node();
            Node x = new Node();
            Node y = new Node();
            Node z = new Node();

            u.id = ("" + 0);
            v.id = ("" + 1);
            w.id = ("" + 2);
            x.id = ("" + 3);
            y.id = ("" + 4);
            z.id = ("" + 5);

            // 0 --> 1 --> 2
            u.connect(v);
            v.connect(w);

            a.graph.Add(u);
            a.graph.Add(v);
            a.graph.Add(w);
            a.graph.Add(x);
            a.graph.Add(y);
            a.graph.Add(z);

            a.dungeon_size = a.graph.Count;
            bool result = a.dungeon_is_ok();
            Assert.AreEqual(true, result);

            a.graph.Add(y);
            u.connect(w);
            u.connect(x);
            u.connect(y);
            u.connect(z);

            a.dungeon_size = a.graph.Count;
            result = a.dungeon_is_ok();
            Assert.AreEqual(false, result);

            //clean
            a.graph.Clear();
            u.neighbors.Clear();
            v.neighbors.Clear();
            w.neighbors.Clear();
            x.neighbors.Clear();
            y.neighbors.Clear();
            z.neighbors.Clear();

            u.connect(v);
            u.connect(w);
            u.connect(x);
            u.connect(y);

            v.connect(u);
            v.connect(w);
            v.connect(x);
            v.connect(y);

            w.connect(u);
            w.connect(v);
            w.connect(x);
            w.connect(y);

            x.connect(u);
            x.connect(w);
            x.connect(v);
            x.connect(y);

            y.connect(u);
            y.connect(w);
            y.connect(x);
            y.connect(v);

            a.graph.Add(u);
            a.graph.Add(v);
            a.graph.Add(w);
            a.graph.Add(x);
            a.graph.Add(y);

            a.dungeon_size = a.graph.Count;
            result = a.dungeon_is_ok();
            Assert.AreEqual(false, result);

        }

        [Test]
        public void DungeonIsValidTest_2()
        {
            Dungeon a = new Dungeon(1, 1);
            a.dungeon_size = a.graph.Count;
            a.graph.Clear();

            Node u = new Node();
            a.graph.Add(u);
            Assert.AreEqual(false, a.dungeon_is_ok());
            
            Node v = new Node();
            Node w = new Node();
            Node x = new Node();
 
            u.id = ("" + 0);
            v.id = ("" + 1);
            w.id = ("" + 2);
            x.id = ("" + 3);

            a.graph.Add(v);
            a.graph.Add(w);
            a.graph.Add(x);

            u.connect(v);
            v.connect(w);
            w.connect(x);
            a.startNode = u;
            a.exitNode = x;
            a.dungeon_size = a.graph.Count;
            Assert.AreEqual(false, a.dungeon_is_ok());
        }

        [Test]
        public void shortestPathTest_hard()
        {
            Dungeon a = new Dungeon(10, 1);
            a.dungeon_size = a.graph.Count;

            int d = Predicates.shortestpath(a.startNode, a.exitNode, a.dungeon_size).Count;
            //Oracle: Find all paths extract shorest and compare with found shortest path.
            Assert.AreEqual(Predicates.find_all_paths_extract_shortest(a.startNode, a.exitNode, a.dungeon_size), d);
        }

        

        [Test]
        public void shuffleConnectTest()
        {
            Dungeon a = new Dungeon(10, 1);
            Node[] a_array = new Node[5];

            for(int i = 0; i < 5; i++)
            {
                Node n = new Node("" + i);
                a_array[i] = n;
            }
            Node[] b_array = a.shuffle_connect(a_array, 5, 5);
            bool changed = false;

            for (int i = 0; i < 5; i++)
            {
                if (a_array[i] != b_array[i]) changed = true;
            }

            Assert.AreEqual(true, changed);          
        }

        [Test]
        public void Level_Simple()
        {
            //tets a simple graph
            Dungeon a = new Dungeon(1, 1);
            a.graph.Clear();
            a.startNode = null;
            a.exitNode = null;

            Node[] xs = new Node[2];
            //level 1
            xs[0] = new Node("0");
            xs[0].nodeLevel = 1;
            a.startNode = xs[0];

            Bridge b1 = new Bridge("2");
            b1.nodeLevel = 1;

            xs[1] = new Node("1");
            xs[1].nodeLevel = 2;

            b1.connectToNodeOfSameZone(xs[0]);
            b1.connectToNodeOfSameZone(xs[1]);

            a.exitNode = xs[1];

            a.bridges = new Bridge[] { b1 };

            a.dungeon_size = 3;
            a.graph.Add(xs[0]);
            a.graph.Add(b1);
            a.graph.Add(xs[1]);


            //startnode - bridge - exitnode (startnode and bridge are lvl 1 and exitnode lvl 0)
            foreach (Node n in Predicates.reachableNodes(a.startNode))
                Assert.AreEqual(n.nodeLevel, a.level(n));
        }
        [Test]
        public void Level_Test_Groot()
        {

            //test a bigger graph (figure 1 from project description)
            Dungeon a = new Dungeon(1, 1);
            a.graph.Clear();
            a.startNode = null;
            a.exitNode = null;

            Node[] xs = new Node[10];
            //startnode
            xs[0] = new Node("0");
            xs[0].nodeLevel = 1;
            a.startNode = xs[0];

            xs[1] = new Node("1");
            xs[1].nodeLevel = 1;

            xs[2] = new Node("2");
            xs[2].nodeLevel = 1;

            xs[3] = new Node("3");
            xs[3].nodeLevel = 1;

            xs[4] = new Node("4");
            xs[4].nodeLevel = 1;

            Bridge  b1 = new Bridge("10");
            b1.nodeLevel = 1;

            xs[0].connect(xs[1]);
            xs[0].connect(xs[3]);
            xs[0].connect(xs[2]);

            xs[2].connect(xs[1]);
            xs[2].connect(xs[3]);

            xs[3].connect(xs[4]);

            b1.connectToNodeOfSameZone(xs[1]);
            b1.connectToNodeOfSameZone(xs[2]);

            //level 2
            xs[5] = new Node("5");
            xs[5].nodeLevel = 2;

            xs[6] = new Node("6");
            xs[6].nodeLevel = 2;

            b1.connectToNodeOfNextZone(xs[5]);
            b1.connectToNodeOfNextZone(xs[6]);

            Bridge b2 = new Bridge("11");
            b2.nodeLevel = 2;

            xs[5].connect(xs[6]);

            b2.connectToNodeOfSameZone(xs[5]);
            b2.connectToNodeOfSameZone(xs[6]);

            //level 0
            xs[7] = new Node("7");
            xs[7].nodeLevel = 3;
            b2.connectToNodeOfNextZone(xs[7]);

            xs[8] = new Node("8");
            xs[8].nodeLevel = 3;
            b2.connectToNodeOfNextZone(xs[8]);

            //exit node
            xs[9] = new Node("9");
            xs[9].nodeLevel = 3;

            xs[9].connect(xs[7]);
            xs[9].connect(xs[8]);

            a.startNode = xs[0];
            a.exitNode = xs[9];

            a.bridges = new Bridge[] { b1, b2 };

            for (int i = 0; i < 10; i++) {
                a.graph.Add(xs[i]);
            }
            a.graph.Add(b1);
            a.graph.Add(b2);

            a.dungeon_size = 12;

            foreach (Node n in Predicates.reachableNodes(a.startNode))
            {
                Assert.AreEqual(n.nodeLevel, a.level(n));
            }
        }
        [Test]
        public void DisconnectTest()
        {
            Dungeon a = new Dungeon(10, 1);

            
            Random rnd = RandomGenerator.rnd;
            int i = rnd.Next(a.bridges.Length);
            string bridgeid = a.bridges[i].id;

            //maak lijst van nodes die allemaal behouden moeten worden
            List<string> keep = new List<string>();

            foreach (Node n in Predicates.reachableNodes(a.startNode))
                if (a.level(n) > a.level(a.bridges[i]) || a.level(n) == 0)
                    keep.Add(n.id);

            keep.Add(a.bridges[i].id);

            a.disconnect(a.bridges[i]);

            foreach(Node n in Predicates.reachableNodes(a.startNode))
                Assert.IsTrue(keep.Contains(n.id));
                
        }

        [Test]
        public void genGraphTest()
        {
            uint level = 5;
            Dungeon a = new Dungeon(level, 1);
            Assert.AreEqual(level, Predicates.countNumberOfBridges(a.startNode, a.exitNode));
            Assert.AreEqual(true, Predicates.isValidDungeon(a.startNode, a.exitNode, level));

        }

        [Test]
        //small test for number of bridges
        public void numberOfBridgesTest()
        {
            uint level = (uint)Utils.RandomGenerator.rnd.Next(1,20);
            Dungeon a = new Dungeon(level, 1);
            Assert.AreEqual(level, Predicates.countNumberOfBridges(a.startNode, a.exitNode));

        }

        [Test]
        //small test for average Connection
        public void averageConnectionTest()
        {
            uint level = (uint)Utils.RandomGenerator.rnd.Next(1, 10);
            Dungeon a = new Dungeon(level, 1);

            double connections = 0;
            foreach(Node n in a.graph)
            {
                connections = connections + (double)n.neighbors.Count;
            }
            double avg = connections / (double)a.graph.Count;
            Assert.IsTrue(avg <= 3.0);

        }

        [Test]
        //small test for maximum neighbours
        public void maxNeighbourTest()
        {
            uint level = (uint)Utils.RandomGenerator.rnd.Next(1, 10);
            Dungeon a = new Dungeon(level, 1);
            foreach (Node n in a.graph)
            {
                Assert.IsTrue(n.neighbors.Count <= 4.0);
            }
        }
    }
}