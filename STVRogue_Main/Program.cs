using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STVRogue.GameLogic;
using STVRogue.Utils;
using STVRogue_Main;
namespace STVRogue
{
    /* A dummy top-level program to run the STVRogue game */
    class Program
    {
        static Game game;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            /*
            Game(L, M, Nmonsters) creates a game with a random dungeon of level L and M as the multiplier for the nodes’ capacity.Nmonsters specifies the number of monsters that are ’randomly’ spread in the dungeon.
            */
            //game = new Game(5, 5, 20);

            Console.WriteLine("Welcome to STV Rogue!");

            Agent agent = ChooseAgent();
            //turns
            while (game.player.location != game.dungeon.exitNode || game.player.HP >= 0)
            {

                agent.DoTurn();
            }
        }

        public static Agent ChooseAgent()
        {
            Console.WriteLine("[1] Use custom seed.");
            Console.WriteLine("Press any other key to generate a seed.");
            int seed;
            if (Console.ReadKey().Key == ConsoleKey.D1)
            {
                Console.Clear();
                Console.WriteLine("Enter seed(signed int):");
                int.TryParse(Console.ReadLine(), out seed);
            }
            else
                seed = new Random(Environment.TickCount).Next(0, int.MaxValue);
            //Console.WriteLine(seed);
            game = new Game(5, 5, 20, seed);

            Console.WriteLine("[1] Player agent.");
            Console.WriteLine("[2] PeaceFullRandomAgent.");
            Console.WriteLine("[3] BloodthirstyAgent.");
            Console.WriteLine("[4] PrioritizeNewNodeAgent.");
            Console.WriteLine("[5] GoalAgent.");
            
            Agent agent;
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    agent = new PlayerAgent(game);
                    break;
                case ConsoleKey.D2:
                    agent = new PeaceFullRandomAgent(game, seed);
                    break;
                case ConsoleKey.D3:
                    agent = new BloodthirstyAgent(game, seed);
                    break;
                case ConsoleKey.D4:
                    agent = new PrioritizeNewNodesAgent(game, seed);
                    break;
                case ConsoleKey.D5:
                    //TupleList<string, int> goals = new TupleList<string, int> { { "exit", 100 }};
                    TupleList<string, int> goals = new TupleList<string, int> { { "reachup",3},{ "reachdown", 1 },{ "kill",4}, {"exit",0}  };
                    agent = new GoalAgent(game, seed, goals);
                    break;
                //other key
                default:
                    agent = new PlayerAgent(game);
                    break;

            }
            return agent;
        }
        /*
        public static void sound()
        {
            WindowsMediaPlayer myplayer = new WindowsMediaPlayer();
            myplayer.URL = "sound.mp3";
            myplayer.controls.play();
        }
        */

    }
}

