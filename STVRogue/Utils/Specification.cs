using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.GameLogic;
using System.IO;
using System.Xml.Serialization;
namespace STVRogue.Utils
{
    public abstract class Specification
    {
        public bool verdict = true;
        public bool getVerdict() { return verdict; }
        public abstract void test(Game G);
    }

    public class Always : Specification
    {
        private Predicate<Game> p;
        public Always(Predicate<Game> p) { this.p = p; }
        public override void test(Game G) { verdict = verdict && p(G); }

    }

    public class Unless : Specification
    {
        private Predicate<Game> p;
        private Predicate<Game> q;
        public Unless(Predicate<Game> p, Predicate<Game> q) { this.p = p; this.q = q; }

        //history
        bool last = true;

        //first iteration
        bool first = true;
        public override void test(Game G)
        {
            bool newVerdict;
            if (!first)
            {
                // calculate whether the previous and current state
                // satisfies the unless property:
                newVerdict = !last || (last && (p(G) || q(G)));
            }
            else
            {
                newVerdict = true;
                first = true;
            }
            // update accumulated verdict:
            verdict = verdict && newVerdict;
            // save p && !q:
            last = p(G) && !q(G);
        }
    }

    public class LeadsTo : Specification
    {
        private Predicate<Game> p;
        private Predicate<Game> q;
        public LeadsTo(Predicate<Game> p, Predicate<Game> q) { this.p = p; this.q = q; }
        bool pValid = false;
        bool qValid = false;

        public override void test(Game G)
        {
            if (p(G))
                pValid = true;
            if ((pValid || p(G)) && q(G))
            {
                pValid = true;
                qValid = true;
            }
            verdict = pValid && qValid;
        }
    }

    public class Condition : Specification
    {
        //antecedents
        private List<Specification> p;
        private Specification q;
        private Specification falseSpec;
        public Condition(List<Specification> antecedents, Specification consequent) { this.p = antecedents; this.q = consequent; }

        
        public override void test(Game G)
        {
            foreach(Specification spec in p)
            {
                spec.test(G);
                Console.WriteLine(spec.getVerdict());
                if (!spec.getVerdict() && falseSpec == null)
                    falseSpec = spec;
                else if (falseSpec == spec)
                    spec.verdict = true;
            }
            q.test(G);
            verdict = p.TrueForAll(x => { return x.getVerdict(); }) && q.getVerdict();
        }
    }
        [Serializable]
    [XmlRoot("GamePlay")]
    public class GamePlay
    {
        Game G;
        public int seed;
        public uint difficultyLevel;
        public uint nodeCapcityMultiplier;
        public uint numberOfMonsters;

        private bool inSim = false;

        [XmlArray("PlayerCommands"), XmlArrayItem(typeof(Command), ElementName = "Command")]
        public List<Command> PlayerCommands = new List<Command>();

        //public string hoi;
        public GamePlay() { }
        public GamePlay(int seed)
        {
            this.seed = seed;
        }

        public void NewPlayerCommand(Command cmd)
        {
            PlayerCommands.Add(cmd);
        }

        public void Serialize(string fileName)
        {
            System.Xml.Serialization.XmlSerializer serial = new System.Xml.Serialization.XmlSerializer(typeof(GamePlay), new Type[] { typeof(MoveCommand), typeof(UseItemCommand), typeof(AttackCommand) });
            Logger.log(ReplayDirectory.Get(fileName+".rpl"));
            using (StreamWriter sw = new StreamWriter(new FileStream(ReplayDirectory.Get(fileName+".rpl"), FileMode.Create)))
                serial.Serialize(sw, this);
        }

        public void CreateGame()
        {
            G = new Game(this);
        }

        public void replay(Specification S)
        {
            CreateGame();
            StartSimulation();
            Logger.log("Start replay");
            G.specification = S;
            G.ExecuteSpecifications();

            for (int i = 0; i < PlayerCommands.Count; i++)
            {
                G.update(PlayerCommands[i]);
            }
        }

        public void StartSimulation()
        {
            inSim = true;
        }
        public bool InSimulation()
        {
            return inSim;
        }
        public Game getGame()
        {
            return G;
        }
    }
}
