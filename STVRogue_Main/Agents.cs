using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue;
using STVRogue.GameLogic;
using STVRogue.Utils;

namespace STVRogue_Main
{
    public abstract class Agent
    {
        public Game game;

        public abstract void DoTurn();
        public void exit_reached()
        {
            Console.WriteLine("You have reached the last node!");
            Console.ForegroundColor = ConsoleColor.Red;

            string filepath = ReplayDirectory.Get("end_ascii.txt");
            string[] end = System.IO.File.ReadAllLines(filepath, Encoding.UTF8);
            for (int i = 0; i < end.Count(); i++)
            {
                Console.WriteLine(end[i]);
            }
            Console.WriteLine("You have killed {0} monsters.", game.player.KillPoint);
            Console.ResetColor();
            Console.ReadKey();
            //game.gp.Serialize("Combat6");
            Environment.Exit(0);
        }

    }
    public abstract class AI_Agent : Agent
    {
        public void AggressiveTurn(Random r)
        {
            if (game.player.location.contested)
            {
                game.update(new AttackCommand(0));
            }
            else
            {
                game.update(new MoveCommand(r.Next(game.player.location.neighbors.Count)));
            }
        }

        public void MoveZoneUpTurn(Random r)
        {
            bool newNode = false;
            foreach (Node node in game.player.location.neighbors)
            {
                if (!node.visited)
                {
                    newNode = true;
                    break;
                }
            }

            if (newNode)
            {
                newNode = false;
                int index = 0;
                while (!newNode)
                {
                    index = r.Next(game.player.location.neighbors.Count);
                    if (!game.player.location.neighbors[index].visited)
                        newNode = true;
                }
                game.update(new MoveCommand(index));
            }
            else
            {
                game.update(new MoveCommand(r.Next(game.player.location.neighbors.Count)));
            }
        }

        public void MoveZoneDownTurn(Random r) //only allows the player to move along visited nodes, which will eventually bring them back to earlier zones.
        {
            bool oldNode = false;
            int index = 0;
            while (!oldNode)
            {
                index = r.Next(game.player.location.neighbors.Count);
                if (game.player.location.neighbors[index].visited)
                    oldNode = true;
            }
            game.update(new MoveCommand(index));

        }
    }

    public class PlayerAgent : Agent
    {
        public PlayerAgent(Game G)
        {
            base.game = G;
        }

        public override void DoTurn()
        {
            //player turn
            PrintInfo();

            Console.WriteLine("It is your turn!");
            PrintCommands();
            ConsoleKey key = Console.ReadKey(true).Key;
            Console.Clear();
            ExecuteKey(key, game.player.location.contested);
        }

        public void PrintInfo()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" HP   : {0} ", game.player.HP);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(" Kills: {0} ", game.player.KillPoint);
            if (game.player.location.contested)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(" Packs: {0} ", game.player.location.packs.Count);
            }
            Console.ResetColor();
        }

        public void PrintCommands()
        {
            if (game.player.location.contested) Console.WriteLine("[Q] Flee from combat.");
            else Console.WriteLine("[Q] View neighbours");

            Console.WriteLine("[W] View items.");
            if (game.player.location.contested) Console.WriteLine("[E] Attack.");
            else Console.WriteLine("[S] Save progress.");
        }

        public void ExecuteKey(ConsoleKey key, bool contested, bool recursion = false)
        {
            List<Tuple<ConsoleKey, int>> keys;
            ConsoleKey pressedKey;



            if (recursion)
            {
                PrintInfo();
                PrintCommands();
                key = Console.ReadKey(true).Key;
                recursion = false;
            }
            //move
            if (key == ConsoleKey.Q)
            {
                Console.Clear();
                PrintInfo();
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("(¯`·._.·(¯`·._.· Travel ·._.·´¯)·._.·´¯)");
                Console.BackgroundColor = default(ConsoleColor);

                //game.update(new MoveCommand() { option = 1 });
                for (int i = 0; i < game.player.location.neighbors.Count; i++)
                {
                    Console.Write("[{0}] Move to node {1}.", i + 1, game.player.location.neighbors[i].id);
                    if (game.player.location.neighbors[i].visited) Console.WriteLine(" You have already visited this node.");
                    else Console.WriteLine("");
                }
                Console.WriteLine("Press any key to return.");

                pressedKey = Console.ReadKey(true).Key;
                Console.Clear();

                //Console.WriteLine(pressedKey);
                keys = new List<Tuple<ConsoleKey, int>>();
                for (int i = 0; i < game.player.location.neighbors.Count; i++)
                {
                    keys.Add(new Tuple<ConsoleKey, int>((ConsoleKey)Enum.Parse(typeof(ConsoleKey), $"D{i + 1}"), i));
                }

                foreach (var x in keys)
                {
                    if (pressedKey == x.Item1)
                    {
                        if (game.player.location.contested)
                        {
                            Console.Write("You flee to {0}.", game.player.location.neighbors[x.Item2].id);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(" (°∟ _°\")//");
                            Console.ResetColor();
                            Console.WriteLine(" = = - -");
                        }
                        else
                        {
                            Console.Write("You move to {0}.", game.player.location.neighbors[x.Item2].id);
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine(" . . . ( O wO)");
                            Console.ResetColor();
                        }
                        game.update(new MoveCommand(x.Item2));
                        if (game.player.location == game.dungeon.exitNode)
                            exit_reached();
                        break;
                    }
                }
            }

            //use item
            if (key == ConsoleKey.W)
            {
                Console.Clear();
                PrintInfo();
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("(¯`·._.·(¯`·._.· Inventory ·._.·´¯)·._.·´¯)");
                Console.BackgroundColor = default(ConsoleColor);

                keys = new List<Tuple<ConsoleKey, int>>();
                if (game.player.ContainsPotion())
                {
                    Console.WriteLine("[1] Use potion. {0} Remaining.", game.player.PotionCount());
                    keys.Add(new Tuple<ConsoleKey, int>((ConsoleKey)Enum.Parse(typeof(ConsoleKey), $"D1"), 0));
                }
                if (game.player.ContainsCrystal() && contested && game.player.location.packs.Count != 0)
                {
                    Console.WriteLine("[2] Use crystal. {0} Remaining.", game.player.CrystalCount());
                    keys.Add(new Tuple<ConsoleKey, int>((ConsoleKey)Enum.Parse(typeof(ConsoleKey), $"D2"), 1));
                }
                Console.WriteLine("Press any key to return.");
                pressedKey = Console.ReadKey(true).Key;
                Console.Clear();

                foreach (var x in keys)
                {
                    if (pressedKey == x.Item1)
                    {
                        game.update(new UseItemCommand(x.Item2));
                        break;
                    }
                }
                ExecuteKey(ConsoleKey.Backspace, contested, true);
            }

            //combat?
            if (key == ConsoleKey.E && contested)
            {
                Console.Clear();
                PrintInfo();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("(¯`·._.·(¯`·._.· Attack  ·._.·´¯)·._.·´¯)");
                Console.BackgroundColor = default(ConsoleColor);

                keys = new List<Tuple<ConsoleKey, int>>();               
                for (int i = 0; i < game.player.location.packs.Count; i++)
                {
                    int packHP = 0;
                    foreach (Monster monster in game.player.location.packs[i].members)
                    {
                        packHP += monster.HP;
                    }
                    Console.WriteLine("[{0}] Attack pack {1}. {2} Monsters left in pack. {3} HP remaining.", i + 1, game.player.location.packs[i].id, game.player.location.packs[i].members.Count, packHP);
                    keys.Add(new Tuple<ConsoleKey, int>((ConsoleKey)Enum.Parse(typeof(ConsoleKey), $"D{i + 1}"), i));
                }
                Console.WriteLine("Press any key to return.");
                pressedKey = Console.ReadKey(true).Key;
                Console.Clear();

                foreach (var x in keys)
                {
                    if (pressedKey == x.Item1)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("You attack! ");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("(O_ O)=");
                        Console.ResetColor();
                        Console.Write("(===||::::::::::::");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(":::>");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" (^._.^)_");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("/");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("¯");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("¯");
                        Console.ResetColor();
                        game.update(new AttackCommand(x.Item2));
                        break;
                    }
                }

            }

            //save game
            if (key == ConsoleKey.S && !contested)
            {
                Logger.log("Replay saved, enter path:");
                game.gp.Serialize(Console.ReadLine());
            }
        }
    }



    public class PeaceFullRandomAgent : AI_Agent
    {
        Random r;
        int steps;

        public PeaceFullRandomAgent(Game G, int seed)
        {
            base.game = G;
            r = new Random(seed);
        }

        public override void DoTurn()
        {
            if (game.player.location == game.dungeon.exitNode)
            {
                Console.WriteLine("It took the agent " + steps + " steps to find the exit node.");
                exit_reached();
            }
            game.update(new MoveCommand(r.Next(game.player.location.neighbors.Count)));
            steps++;

        }
    }

    public class PrioritizeNewNodesAgent : AI_Agent
    {
        Random r;
        int steps;

        public PrioritizeNewNodesAgent(Game G, int seed)
        {
            base.game = G;
            r = new Random(seed);
        }

        public override void DoTurn()
        {
            if (game.player.location == game.dungeon.exitNode)
            {
                Console.WriteLine("It took the agent " + steps + " steps to find the exit node.");
                exit_reached();
            }
            MoveZoneUpTurn(r);
            steps++;
        }
    }

    public class BloodthirstyAgent : AI_Agent
    {
        Random r;
        public BloodthirstyAgent(Game G, int seed)
        {
            base.game = G;
            r = new Random(seed);
        }

        public override void DoTurn()
        {
            if (game.player.location == game.dungeon.exitNode)
                exit_reached();
            AggressiveTurn(r);
        }
    }

    public class GoalAgent : AI_Agent
    {
        Random r;
        TupleList<string, int> goals = new TupleList<string, int> { };
        int goalIndex, steps;
        bool goalReached = false;
        int goalKillPoint, prevTurnKillpoint;

        public GoalAgent(Game G, int seed, TupleList<string, int> goalsList)
        {
            this.game = G;
            r = new Random(seed);
            goals = goalsList;
        }

        public override void DoTurn()
        {
            if (game.player.location == game.dungeon.exitNode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Agent found the exit before completing all goals.");
                Console.WriteLine("Agent completed {0} out of {1} goals.", goalIndex, goals.Count);
                Console.ResetColor();
                exit_reached();
            }
            if (goalIndex < goals.Count)
            {
                switch (goals[goalIndex].Item1)
                {
                    case "kill": //Send the agent out to kill some monsters.
                        if (goalKillPoint >= goals[goalIndex].Item2)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Killcount reached! Goal complete.");
                            Console.ResetColor();
                            goalReached = true;
                            break;
                        }

                        if (game.player.KillPoint != prevTurnKillpoint)
                            goalKillPoint += ((int)game.player.KillPoint - prevTurnKillpoint);
                        prevTurnKillpoint = (int)game.player.KillPoint;
                        AggressiveTurn(r);
                        break;
                    case "reachup": //Reach a specified ZONE [above or at] player current zone level
                        if (game.player.location.nodeLevel == goals[goalIndex].Item2)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Move zone up reached.");
                            Console.ResetColor();
                            goalReached = true;
                            break;
                        }
                        MoveZoneUpTurn(r);
                        break;
                    case "reachdown": //Reach a specified ZONE [below or at] play current zone level
                        if (game.player.location.nodeLevel == goals[goalIndex].Item2)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Move Zone Down reached.");
                            Console.ResetColor();
                            goalReached = true;
                            break;
                        }
                        MoveZoneDownTurn(r);
                        break;
                    case "exit": //Find the exit.
                    default:
                        MoveZoneUpTurn(r);
                        if (game.player.location == game.dungeon.exitNode)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Agent found the exit as his last goal.");
                            goalIndex++;
                            Console.WriteLine("Agent completed {0} out of {1} goals.", goalIndex, goals.Count);
                            Console.ResetColor();
                            exit_reached();
                        }

                        break;
                }

                if (goalReached)
                {
                    goalReached = false;
                    goalIndex++;
                }
                steps++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Agent has run out of goals, and has not reached the exit node.");
                Console.ResetColor();
                Console.ReadKey();
                game.gp.Serialize("GoalsAgent");
                Environment.Exit(0);
            }

        }
    }
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }
}
