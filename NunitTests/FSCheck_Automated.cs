using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using NUnit.Framework;
using STVRogue.GameLogic;
using STVRogue.Utils;
using Moq;
using System.IO;
namespace ClassLibrary1.Automated
{
    [TestFixture]
    class FSCheck_Automated
    {
        
        public int[] getUniformDistributedArray(int step)
        {
            int max = int.MaxValue / step;
            int[] xs = new int[step];
            for (int i = 0; i < step; i++)
                xs[i] = i * max + 1;
            return xs;
        }


        [Test]
        public void Auto_FY_Suffle()
        {
            Dungeon d = new Dungeon(1, 1);
            Prop.ForAll<object[]>(data =>
            {
                var shuffle = d.F_Y_shuffle(data).ToArray();
                return (!data.SequenceEqual(shuffle))
                .When(data.Length > 1 && data.Distinct().Count() > 1)
                .Label($"The list after the shuffle == before")
                .And(data.All(x => shuffle.Contains(x))
                .Label($"List after suffle has to contain the same elements"));
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Auto_Dungeon()
        {
            Arb.Register<MyGenerators>();
            Prop.ForAll<int>( i =>
            {
                uint j = (uint) i;
                Console.WriteLine(j);
                Dungeon data = new Dungeon(j, 1);

                return Predicates.isValidDungeon(data.startNode, data.exitNode, j);

            }).QuickCheckThrowOnFailure();
        }
        [Test]
        public void Auto_Disconnect()
        {
            Arb.Register<MyGenerators>();
            Prop.ForAll<int>(x =>
            {
                uint j = (uint)x;
                Dungeon a = new Dungeon(j, 1);

                int i = RandomGenerator.rnd.Next(a.bridges.Length);
                string bridgeid = a.bridges[i].id;

                //maak lijst van nodes die allemaal behouden moeten worden
                List<string> keep = new List<string>();

                uint blevel = a.level(a.bridges[i]);

                foreach (Node n in Predicates.reachableNodes(a.startNode))
                    if (a.level(n) > blevel || a.level(n) == 0)
                        keep.Add(n.id);

                keep.Add(a.bridges[i].id);

                a.disconnect(a.bridges[i]);
                bool contains = true;
                foreach (Node n in Predicates.reachableNodes(a.startNode))
                    if (!keep.Contains(n.id)) contains = false;

                return contains && bridgeid == a.startNode.id;
            }).QuickCheckThrowOnFailure();

        }
        [Test]
        public void Auto_LevelCheck()
        {

            Arb.Register<MyGenerators>();
            Prop.ForAll<int>(i =>
            {
                uint j = (uint)i;
                Dungeon d = new Dungeon(j, 1);
                List<Node> nodes = Predicates.reachableNodes(d.startNode);
                nodes.Distinct();
                //get a random node
                Node n = nodes[RandomGenerator.rnd.Next(nodes.Count)];

                //check the level of the node
                int foundLevel = (int)d.level(n);

                if (n == d.startNode)
                    return true;

                List<Node> path = d.shortestpath(d.startNode, n);

                int actualLevel = path.OfType<Bridge>().Count() + 1;
                if (n is Bridge) actualLevel--;

                //check if the found level == actual level
                return foundLevel == actualLevel;// foundLevel == actualLevel;
            }).QuickCheckThrowOnFailure();
        }
        [Test]
        public void Auto_ShortestPath()
        {
            
            Arb.Register<MyGenerators>();
            Prop.ForAll<int>(i =>
            {
                uint j = (uint)i;
                Dungeon d = new Dungeon(j, 1);

                //get 2 random nodes
                List<Node> nodes = Predicates.reachableNodes(d.startNode);
                Node from = nodes.GetRandomItem();
                Node to = nodes.GetRandomItem();

                //check if found path == shortest 
                int shortest = Predicates.find_all_paths_extract_shortest(from, to, d.graph.Count);
                int foundPath = d.shortestpath(from, to).Count;
                return shortest == foundPath;

            }).QuickCheckThrowOnFailure();
        }
    }
    public static class MyNodeExtension
    {
        public static int GetHashCode(this Node x)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + x.neighbors.GetHashCode();
                hash = hash * 31 + x.id.GetHashCode();
                hash = hash * 31 + x.nodeLevel.GetHashCode();
                return hash;
            }
        }
    }
    
    public class MyGenerators
    {
        /*
        public static Arbitrary<Dungeon> GenerateDungeon()
        {

            return Arb.Generate<Dungeon>().Select(i => new Dungeon(i, 1)).ToArbitrary<Dungeon>();
        }*/

        public static Arbitrary<int> difficultyLevelGenerator()
        {

            return Gen.Choose(1, 12).ToArbitrary<int>();
        }

    }
}
