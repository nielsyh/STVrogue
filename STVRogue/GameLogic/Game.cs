using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.Utils;

namespace STVRogue.GameLogic
{
    public class Game
    {
        public Player player;
        public Dungeon dungeon;


        //store player executed commands 
        public GamePlay gp;

        public Specification specification;
        /* This creates a player and a random dungeon of the given difficulty level and node-capacity
         * The player is positioned at the dungeon's starting-node.
         * The constructor also randomly seeds monster-packs and items into the dungeon. The total
         * number of monsters are as specified. Monster-packs should be seeded as such that
         * the nodes' capacity are not violated. Furthermore the seeding of the monsters
         * and items should meet the balance requirements stated in the Project Document.
         */
        public Game(GamePlay gp)
        {
            Logger.log("Creating a game of difficulty level " + gp.difficultyLevel + ", node capacity multiplier "
           + gp.nodeCapcityMultiplier + ", and " + gp.numberOfMonsters + " monsters.");


            this.gp = gp;
            InitGame(gp.difficultyLevel, gp.nodeCapcityMultiplier, gp.numberOfMonsters, gp.seed);
        }

        //Added seed in overload for certain tests that require specific seed.
        public Game(uint difficultyLevel, uint nodeCapcityMultiplier, uint numberOfMonsters, int seed)
        {
            Logger.log("Creating a game of difficulty level " + difficultyLevel + ", node capacity multiplier "
                       + nodeCapcityMultiplier + ", and " + numberOfMonsters + " monsters.");

            gp = new GamePlay(seed);

            gp.difficultyLevel = difficultyLevel;
            gp.nodeCapcityMultiplier = nodeCapcityMultiplier;
            gp.numberOfMonsters = numberOfMonsters;

            InitGame(difficultyLevel, nodeCapcityMultiplier, numberOfMonsters,seed);
        }

        public Game(uint difficultyLevel, uint nodeCapcityMultiplier, uint numberOfMonsters)
        {
            Logger.log("Creating a game of difficulty level " + difficultyLevel + ", node capacity multiplier "
                       + nodeCapcityMultiplier + ", and " + numberOfMonsters + " monsters.");


            int seed = Environment.TickCount;
            gp = new GamePlay(seed);


            gp.difficultyLevel = difficultyLevel;
            gp.nodeCapcityMultiplier = nodeCapcityMultiplier;
            gp.numberOfMonsters = numberOfMonsters;

            InitGame(difficultyLevel, nodeCapcityMultiplier, numberOfMonsters, seed);
        }

        public void InitGame(uint difficultyLevel, uint nodeCapcityMultiplier, uint numberOfMonsters, int seed)
        {
            Logger.log("SEED " + seed);
            RandomGenerator.initializeWithSeed(seed);
            Logger.log("1 num " + RandomGenerator.rnd.Next(100));

            //welcome message
            Logger.log("Welcome to STVRogue");
            Logger.log("Q triggers MoveCommand");
            Logger.log("W triggers UseItemCommand");
            Logger.log("E triggers AttackCommand");

            dungeon = new Dungeon(difficultyLevel, nodeCapcityMultiplier);

            //spawn the monsters
            spawnMonsters(numberOfMonsters, difficultyLevel);
            SpawnPI(difficultyLevel);

            player = new Player();
            player.dungeon = dungeon;
            player.location = dungeon.startNode;

            if (player.location.packs.Count > 0)
                player.location.contested = true;
            Logger.log("1 num " + RandomGenerator.rnd.Next(100));
        }

        public void spawnMonsters(uint numberOfMonsters, uint difficultyLevel)
        {
            Random r = RandomGenerator.rnd;

            List<Node> availableNodes = Predicates.reachableNodes(dungeon.startNode);
            Logger.log("This dungeon consists of " + availableNodes.Count + " nodes.");

            uint monstersLeft = numberOfMonsters;
            double monstersinLevel = 0;
            uint packID = 0;

            for (uint i = 1; i < difficultyLevel; i++)
            {
                List<Node> availableNodesinLevel = new List<Node>();
                foreach (Node n in availableNodes.ToList())
                {
                    if (dungeon.level(n) == i)
                        availableNodesinLevel.Add(n);
                }
                monstersinLevel = Math.Round((2 * i * (double)numberOfMonsters / ((difficultyLevel + 2) * (difficultyLevel + 1))));
                while (monstersinLevel > 0)
                {
                    uint capacityLevel = availableNodesinLevel[0].Capacity(dungeon);
                    foreach (Node nd in availableNodesinLevel.ToList())
                    {
                        int creatureCount = nd.CountCreatures();
                        int space = (int)capacityLevel - creatureCount;
                        if (space == 0) { availableNodesinLevel.Remove(nd); }
                        if (availableNodesinLevel.Count() == 0) throw new GameCreationException("Too many monsters, not enough nodes");

                        int spawnX = r.Next(0, Math.Min(space, (int)monstersinLevel) + 1);
                        if (spawnX == 0) continue; //We increase randomness by potentially skipping a node while placing packs
                        monstersinLevel -= (double)spawnX;
                        monstersLeft -= (uint)spawnX;
                        Pack pack = new Pack(packID++.ToString(), (uint)spawnX, nd, dungeon);
                        nd.packs.Add(pack);
                        dungeon.alivePacks.Add(pack);

                    }
                    //Console.WriteLine("{0} monsters in level. {1} monsters left", monstersinLevel, monstersLeft);
                }
            }


            List<Node> availableNodesinExit = new List<Node>();
            foreach (Node n in availableNodes.ToList())
            {
                if (dungeon.level(n) == dungeon.level(dungeon.exitNode))
                    availableNodesinExit.Add(n);
            }
            while (monstersLeft > 0)
            {
                uint capacityLevel = dungeon.M * (dungeon.level(availableNodesinExit[0]) + 1);
                foreach (Node nd in availableNodesinExit.ToList())
                {
                    int creatureCount = nd.CountCreatures();
                    int space = (int)capacityLevel - creatureCount;
                    if (space == 0) { availableNodesinExit.Remove(nd); }
                    if (availableNodesinExit.Count() == 0) throw new GameCreationException("Too many monsters, not enough nodes");

                    int spawnX = r.Next(0, Math.Min(space, (int)monstersLeft) + 1);
                    if (spawnX == 0) continue;
                    monstersLeft -= (uint)spawnX;
                    Pack pack = new Pack(packID++.ToString(), (uint)spawnX, nd, dungeon);
                    pack.location = nd;
                    nd.packs.Add(pack);
                    dungeon.alivePacks.Add(pack);

                }
                //Console.WriteLine("Spreading monsters in last zone {0} monsters left", monstersLeft);
            }



        }

        //spawn pitems and 
        public void SpawnPI(uint difficultylevel)
        {
            Random r = RandomGenerator.rnd;
            r.Next();

            //spawn crystals
            for (int i = 0; i < r.Next(0, (int)difficultylevel) + 1; i++)
            {
                dungeon.graph[r.Next(1, dungeon.graph.Count - 1)].items.Add(new Crystal(i.ToString()));
                Console.WriteLine("CRYSTAL SPAWNED ON NODE {0}!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", dungeon.graph[i].id);
                //To do: comment out above line.
            }

            //spawn health potions
            uint potionHP = 0;
            int monsterHP = dungeon.MonsterHP();

            while (potionHP < monsterHP + 50)
            {
                HealingPotion potion = new HealingPotion(dungeon.totalPotions.Count().ToString());
                dungeon.totalPotions.Add(potion);
                dungeon.graph[r.Next(0, dungeon.graph.Count() - 1)].items.Add(potion);
                potionHP += dungeon.totalPotions.Last().HPvalue;
            }
        }
        /*
         * A single update turn to the game. 
         */
        public Boolean update(Command userCommand)
        {

            if (!gp.InSimulation())
                gp.NewPlayerCommand(userCommand);

            Console.WriteLine("Player does " + userCommand + " "+ userCommand.option);
            userCommand.Execute(player, dungeon);
            Logger.log("random " + RandomGenerator.rnd.Next(100));
            ExecuteSpecifications();

            PackTurn();

            ExecuteSpecifications();

            Predicates.reachableNodes(dungeon.startNode).ForEach(x => x.contested = false);
            player.location.contested = player.location.packs.Count > 0;
            return true;
        }

        public void PackTurn()
        {
            //bridge rule
            List<Pack> bridge_keepers = new List<Pack>();
            foreach (Bridge b in dungeon.bridges)
            {
                uint cap = (b.Capacity(dungeon)) / 2;
                if (cap < b.CountCreatures())
                {
                    //find dichtsbijzijnde pack
                    Pack a = dungeon.find_closest_pack(b);
                    if (a != null && !a.alert)
                    {
                        bridge_keepers.Add(a);
                        a.moveTowards(b, player.location);
                    }
                }
            }

            //turns off all packs.
            foreach (Pack p in dungeon.alivePacks.ToList())
            {
                int l = RandomGenerator.rnd.Next(100);
                if (p.alert && !p.location.contested)
                {
                    Console.WriteLine("Pack " + p.id + " is alert and moves from " + p.location.id + " towards " + player.location.id);
                    p.moveTowards(player.location, player.location);
                }

                //50pct kans dat wil pack chillen                    
                else if (l > 50 && !bridge_keepers.Contains(p))
                {
                    p.move(p.getRandomValidNeighbour(dungeon), player.location);
                }

                //tijd om te knokken?
                Node current_location = p.location;
                if (p.members.Count > 0 && current_location == player.location)
                {
                    //vechtennnn!!!
                    Console.WriteLine("A wild pack appeared!");
                    current_location.contested = true;
                    player.alert_packs(current_location);
                }
            }
        }

        //alert all packs in zone of node parameter
        public void alert_packs(Node u)
        {
            uint zone_level = dungeon.level(u);
            foreach (Pack p in dungeon.alivePacks)
            {
                if (dungeon.level(p.location) == zone_level) { p.alert = true; }
            }
        }

        //leaving zone, de alert all packs. u last node, v next node.
        public void undo_alert_packs(Node u, Node v)
        {
            if (dungeon.level(u) != dungeon.level(v))
            {
                foreach (Pack p in dungeon.alivePacks)
                {
                    p.alert = false;
                }
            }
        }

        //checks if in last dungeon, if yes alert all the Packs there.
        public void enter_last_zone(Node u)
        {
            //check if last zone
            if (dungeon.level(u) == dungeon.difficultyLevel) alert_packs(u);
        }

        public void ExecuteSpecifications()
        {

            if (specification != null)
                specification.test(this);
        }


    }

    public class GameCreationException : Exception
    {
        public GameCreationException(String explanation) : base(explanation) { }
    }
}
