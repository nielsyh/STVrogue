using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.Utils;

namespace STVRogue.GameLogic
{

    public class Dungeon
    {
        public List<Node> graph = new List<Node>() { };
        public List<HealingPotion> totalPotions = new List<HealingPotion>();
        public List<Pack> alivePacks = new List<Pack>();
        public Node startNode;
        public Node exitNode;
        public uint difficultyLevel;
        public int dungeon_size;
        /* a constant multiplier that determines the maximum number of monster-packs per node: */
        public uint M;

        public const int max_size = 20;             /* maximale grootte voor een zone. */
        public const int magic_shuffle_number = 1;  /*higher = more chance of higher connection avg. KEEP THIS 1 TO KEEP FUNCTIONAL DUNGEONS*/

        public Bridge[] bridges;
        int[] zone_size;
        int[] zone_level_arr;    //array to find id of a bridge of a level, so 0 does not exist because there is no bridge to level 0.

        /* To create a new dungeon with the specified difficult level and capacity multiplier */
        public Dungeon(uint level, uint nodeCapacityMultiplier)
        {
            Logger.log("Creating a dungeon of difficulty level " + level + ", node capacity multiplier " + nodeCapacityMultiplier + ".");
            M = nodeCapacityMultiplier;
            difficultyLevel = level;
            zone_size = new int[difficultyLevel + 1];
            zone_level_arr = new int[difficultyLevel + 1];
            bridges = new Bridge[difficultyLevel];
            genGraph();

            foreach (Node n in Predicates.reachableNodes(this.startNode).ToList())
                n.nodeLevel = setNodeLevel(n);
        }
        public uint setNodeLevel(Node current)
        {
            uint counter = 1;
            foreach (Node n in shortestpath(startNode, current).ToList())
                if (n is Bridge)
                    counter++;

            if (current is Bridge)
                return counter - 1;
            else
                return counter;
        }



        //Returns false if dungeon is not ok
        public bool dungeon_is_ok()
        {
            if (graph.Count < 2) { return false; }
            double total = 0.0;
            double s = (double)dungeon_size;
            for (int i = 0; i < dungeon_size; i++)
            {
                double n = (double)graph[i].neighbors.Count;
                if (n > 4)
                {
                    Console.WriteLine("Neighbours: " + n);
                    return false;
                }
                total = total + n;
            }

            double avg = total / dungeon_size;
            if (avg > 3.000)
            {
                Console.WriteLine("Average: " + avg);
                return false;
            }

            if (!Predicates.isValidDungeon(startNode, exitNode, difficultyLevel))
            {
                Console.WriteLine("Not valid");
                return false;
                //throw new GameCreationException();
            }

            Console.WriteLine("Dungeon valid with avg: " + avg);
            return true;
        }

        public int MonsterHP()
        {
            int hp = 0;
            foreach (Pack pack in alivePacks.ToList())
            {
                foreach (Monster monster in pack.members.ToList())
                { hp += monster.HP; }
            }
            return hp;
        }

        public Pack find_closest_pack(Bridge b)
        {

            List<Node> shortest_path = new List<Node>() { };
            Queue<Node> q = new Queue<Node> { };
            bool[] visited = new bool[graph.Count];
            uint currentlevel = level(b);
            //set_array_to_false(visited);
            q.Enqueue(b);

            while (q.Count > 0)
            {
                Node x = q.Dequeue();
                int options = x.neighbors.Count;
                for (int i = 0; i < options; i++)
                {
                    int witas = int.Parse(x.neighbors[i].id);       //witas is short for "why is this a string"
                    if (!visited[witas] && level(x) == currentlevel)
                    {
                        if (x.neighbors[i].packs.Count > 0)
                        {
                            return x.neighbors[i].packs.GetRandomItem();
                        }
                        q.Enqueue(x.neighbors[i]);
                        visited[witas] = true;
                    }
                }
            }
            return null;
        }

        public int PotionHP()
        {
            int hp = 0;

            foreach (HealingPotion pot in totalPotions.ToList())
            {
                if (!pot.used)
                    hp += (int)pot.HPvalue;
            }
            return hp;
        }

        /* Generates Random graph for dungeon class, uses const max_size, magicshuffle_number and regular dungeon options. */
        public void genGraph()
        {
            int cnt = 0;
            //Generate all zones:
            for (int i = 0; i < (difficultyLevel + 1); i++)
            {
                int rSize = RandomGenerator.rnd.Next(2, max_size);  //random size for current zone
                Node tmpStart = new Node();                         //startpoint of current zone (exitpoint of last zone)
                Node tmpExit = new Node();                          //exitpoint of current zone, bridge or exitnode.
                if (i == 0)                                         //first zone.
                {
                    startNode = new Node("0");
                    startNode.visited = true;
                    graph.Add(startNode);
                    tmpStart = startNode;
                    Bridge b = new Bridge("" + (rSize - 1));
                    graph.Add(b);
                    bridges[i] = b;
                    tmpExit = b;
                }
                else if (i < difficultyLevel)                       //not first not last zone.
                {
                    tmpStart = bridges[i - 1];
                    Bridge b = new Bridge("" + (rSize + cnt - 1));
                    graph.Add(b);
                    tmpExit = b;
                    bridges[i] = b;
                }
                else if (i == difficultyLevel)                      //last zone.
                {
                    tmpStart = bridges[i - 1];
                    exitNode = new Node("" + (rSize + cnt - 1));
                    graph.Add(exitNode);
                    tmpExit = exitNode;
                }
                Node[] nodeArray = new Node[rSize];
                nodeArray[0] = tmpStart;
                nodeArray[rSize - 1] = tmpExit;
                for (int j = 1; j < (rSize - 1); j++)   //we now know how big and what the tmp start/exit is.
                {
                    Node n = new Node("" + (cnt + j));  //build nodes
                    graph.Add(n);
                    nodeArray[j] = n;
                }
                bool once = false;
                int shuffle = 0;

                //TODO remove node array only use shuffle array.
                Node[] shuffleArray = new Node[rSize - 2];              //nodes we want to shuffle
                Array.Copy(nodeArray, 1, shuffleArray, 0, rSize - 2);       

                //Build zone until correct
                while ((Predicates.countNumberOfBridges(tmpStart, tmpExit) != 0) || !once)
                {   //connect and shuffle the nodes random:
                    shuffle_connect(shuffleArray, (rSize - 2), magic_shuffle_number);//MSN should be 1. but incase somebody want to change it..
                    tmpExit.neighbors.Clear();

                    if (i > 0)
                    {
                        foreach (Node x in bridges[i - 1].toNodes.ToList())
                        {
                            tmpStart.disconnect(x);
                            tmpStart.neighbors.Remove(x);
                        }
                        bridges[i - 1].toNodes.Clear();
                    }
                    else { tmpStart.neighbors.Clear(); }

                    int a = Utils.RandomGenerator.rnd.Next(1, rSize);
                    int b = a;
                    if (rSize != 2)
                    {
                        while (b == a)
                        {
                            b = Utils.RandomGenerator.rnd.Next(1, rSize);
                        }
                    }
                    tmpStart.connect(nodeArray[a]);
                    nodeArray[a].connect(tmpStart);
                    tmpStart.connect(nodeArray[b]);
                    nodeArray[b].connect(tmpStart);

                    if (i > 0)
                    {
                        bridges[i - 1].connectToNodeOfNextZone(nodeArray[a]);
                        bridges[i - 1].connectToNodeOfNextZone(nodeArray[b]);
                    }
                    if (nodeArray[a].id == tmpExit.id || nodeArray[b].id == tmpExit.id)
                    {
                        a = Utils.RandomGenerator.rnd.Next(1, rSize - 1);
                        if (i < difficultyLevel) bridges[i].connectToNodeOfSameZone(nodeArray[a]);
                        tmpExit.connect(nodeArray[a]);
                        nodeArray[a].connect(tmpExit);
                    }
                    else
                    {
                        a = Utils.RandomGenerator.rnd.Next(1, rSize - 1);
                        b = a;
                        while (b == a)
                        {
                            b = Utils.RandomGenerator.rnd.Next(1, rSize - 1);
                        }

                        if (i < difficultyLevel)
                        {
                            bridges[i].connectToNodeOfSameZone(nodeArray[a]);
                            bridges[i].connectToNodeOfSameZone(nodeArray[b]);
                        }
                        tmpExit.connect(nodeArray[a]);
                        nodeArray[a].connect(tmpExit);
                        tmpExit.connect(nodeArray[b]);
                        nodeArray[b].connect(tmpExit);
                    }
                    once = true;
                    //Console.WriteLine("shuffle: " + shuffle);
                    shuffle++;
                }
                // if (i == 0) cnt++;
                double t = 0;
                double avg = 0;
                for (int h = 0; h < rSize; h++)
                {
                    double n = (double)nodeArray[h].neighbors.Count;
                    t = t + n;
                }
                avg = avg + ((double)t / (double)rSize);
                Logger.log("zone " + i + " avg: " + avg);
                cnt = cnt + (rSize - 1);
            }
        }

        public Node[] shuffle_connect(Node[] arr, int s, int shuffle_times)
        {
            for (int i = 0; i < s; i++)
            {
                arr[i].neighbors.Clear();
            }
            for (int j = 0; j < shuffle_times; j++)
            {
                arr = F_Y_shuffle(arr).ToArray();
                //Fisher-Yates Shuffle
                for (int i = 0; i < s - 1; i++)
                {
                    if (arr[i].neighbors.Count > 3 || arr[i + 1].neighbors.Count > 3)
                    {
                        continue;       //skip more neighbours are not allowed
                    }
                    arr[i].connect(arr[i + 1]);
                }
            }
            return arr;
        }

        //fisher yates shuffle
        public IEnumerable<T> F_Y_shuffle<T>(T[] original)
        {
            T[] xs = new T[original.Length];
            for (var i = 0; i < xs.Length; i++)
            {
                var j = RandomGenerator.rnd.Next(0, i);
                if (j != i)
                    xs[i] = xs[j];
                xs[j] = original[i];
            }
            return xs;
        }

        public List<Node> shortestpath(Node u, Node v)
        {
            return Predicates.shortestpath(u, v, graph.Count);
        }

        /* To disconnect a bridge from the rest of the zone the bridge is in. */
        public void disconnect(Bridge b)
        {
            Logger.log("Disconnecting the bridge " + b.id + " from its zone.");

            Node newStart = new Node(b.id);
            newStart.neighbors = b.neighbors;
            newStart.nodeLevel = b.nodeLevel;

            Node[] allNodes = new Node[b.neighbors.Count];

            b.neighbors.CopyTo(allNodes);

            foreach (Node n in allNodes.ToList())
            {
                if (b.toNodes.Contains(n))
                    newStart.connect(n);

                n.disconnect(b);
            }

            startNode = newStart;
        }

        /* To calculate the level of the given node. */

        public uint level(Node d)
        {
            return d.nodeLevel;
        }
    }



    public class Node
    {
        public String id;
        public List<Node> neighbors = new List<Node>();
        public List<Pack> packs = new List<Pack>();
        public List<Item> items = new List<Item>();
        private uint node_level;
        public Boolean contested = false, visited = false; //contested / in combat

        public Node() { }
        public Node(String id) { this.id = id; }

        //spawn items and monsters / packs
        public int CountCreatures()
        {
            int counter = 0;
            foreach (Pack p in packs.ToList())
                counter += p.members.Count;
            return counter;
        }
        /* To connect this node to another node. */
        public void connect(Node nd)
        {
            if (!neighbors.Contains(nd))
            {
                neighbors.Add(nd);
            }
            if (!nd.neighbors.Contains(this))
            {
                nd.neighbors.Add(this);
            }
        }

        /* To disconnect this node from the given node. */
        public void disconnect(Node nd)
        {
            neighbors.Remove(nd); nd.neighbors.Remove(this);
        }

        /* Execute a fight between the player and the packs in this node.
         * Such a fight can take multiple rounds as describe in the Project Document.
         * A fight terminates when either the node has no more monster-pack, or when
         * the player's HP is reduced to 0. 
         */
        public void fight(Player player, Dungeon dungeon)
        {

            //select pack
            Pack randomPack = packs.GetRandomItem();
            //calc pack HP
            int currentHP = 0;
            foreach (Monster member in randomPack.members.ToList())
                currentHP += member.HP;

            float fleeProbability = (1 - (float)currentHP / (float)randomPack.startingHP) / 2 * 100;
            //Console.WriteLine(fleeProbability);

            //%%%%%%%%%%% Pack Turn %%%%%%%%%%%
            if (fleeProbability > (float)RandomGenerator.rnd.Next(0, 100))
            {
                //the pack flees
                Node randomNeighbor = randomPack.getRandomValidNeighbour(dungeon);
                if (randomNeighbor != null)
                {
                    Console.WriteLine("Pack {0} flees.", randomPack.id);
                    randomPack.move(randomNeighbor, player.location);                                 
                    if (this.packs.Count == 0)
                    {
                        contested = false;
                        return;
                    }

                    randomPack = packs.GetRandomItem();
                }
            }

            //pack attack
            randomPack.Attack(player);

            player.location.contested = player.location.packs.Count > 0 && player.HP > 0;

        }


        public uint nodeLevel
        {
            get { return node_level; }
            set { node_level = value; }
        }

        public uint Capacity(Dungeon d)
        {
            return d.M * (d.level(this) + 1);
        }
    }

    public class Bridge : Node
    {
        public List<Node> fromNodes = new List<Node>();
        public List<Node> toNodes = new List<Node>();
        public Bridge(String id) : base(id) { }

        /* Use this to connect the bridge to a node from the same zone. */
        public void connectToNodeOfSameZone(Node nd)
        {
            base.connect(nd);
            fromNodes.Add(nd);
        }

        /* Use this to connect the bridge to a node from the next zone. */
        public void connectToNodeOfNextZone(Node nd)
        {
            base.connect(nd);
            toNodes.Add(nd);
        }

    }

}
