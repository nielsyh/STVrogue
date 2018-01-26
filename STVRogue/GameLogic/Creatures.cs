using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.Utils;

namespace STVRogue.GameLogic
{
    public class Creature
    {
        public String id;
        public String name;
        public int HP;
        public uint AttackRating = 1;
        public Node location;
        public Creature() { }
        virtual public void Attack(Creature foe)
        {
            foe.HP = (int)Math.Max(0, foe.HP - AttackRating);
            String killMsg = foe.HP == 0 ? ", KILLING it" : "";
            Logger.log("Creature " + id + " attacks " + foe.id + killMsg + ".");

        }
    }


    public class Monster : Creature
    {
        public Pack pack;
        public Monster() { }
        /* Create a monster with a random HP */
        public Monster(String id)
        {
            this.id = id; name = "Orc";
            HP = 1 + RandomGenerator.rnd.Next(6);
        }
    }

    public class Player : Creature
    {
        public Dungeon dungeon;
        public int HPbase = 100;
        public Boolean accelerated = false;
        public uint KillPoint = 0;
        public List<Item> bag = new List<Item>();
        public Player()
        {
            id = "player";
            AttackRating = 5;
            HP = HPbase;
        }

        public void use(Item item)
        {
            if (!bag.Contains(item) || item.used) throw new ArgumentException();
            item.use(this);
            bag.Remove(item);
        }

        public bool ContainsPotion()
        {
            foreach (Item item in bag)
                if (item is HealingPotion) return true;
            return false;
        }

        public bool ContainsCrystal()
        {
            foreach (Item item in bag)
                if (item is Crystal) return true;
            return false;
        }

        public int PotionCount()
        {
            int count = 0;
            foreach (Item item in bag)
                if (item is HealingPotion) count++;
            return count;
        }

        public int CrystalCount()
        {
            int count = 0;
            foreach (Item item in bag)
                if (item is Crystal) count++;
            return count;
        }

        override public void Attack(Creature foe)
        {
            if (!(foe is Monster)) throw new ArgumentException();
            Monster foe_ = foe as Monster;
            if (!accelerated)
            {
                base.Attack(foe);
                if (foe_.HP == 0)
                {
                    foe_.pack.members.Remove(foe_);
                    Console.Write("You have killed {0}", foe_.id);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" (^x_x^)_");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("_");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("_");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("_");
                    Console.ResetColor();
                    KillPoint++;
                }
            }
            else
            {
                foreach (Monster target in foe_.pack.members.ToList())
                {
                    base.Attack(target);
                    if (target.HP == 0)
                    {
                        target.pack.members.Remove(target);
                        Console.Write("You have killed {0}", foe_.id);
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" (^x_x^)_");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("_");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("_");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("_");
                        Console.ResetColor();
                        KillPoint++;
                    }
                }
                accelerated = false;
            }

            if (foe_.pack.members.Count == 0)
            {
                Console.WriteLine("You have killed the entire pack!");
                foe_.pack.location.packs.Remove(foe_.pack);
                dungeon.alivePacks.Remove(foe_.pack);
                if (foe_.location.packs.Count == 0)
                    foe_.location.contested = false;
            
            }
        }



        // parameter = visit node
        public void Move(Node move)
        {
            if (move == null) { throw new ArgumentException(); }
            Node old_location = this.location;
            
            if (dungeon.level(old_location) != dungeon.level(move)){
                Console.WriteLine("You entered zone: " + dungeon.level(move));            
                undo_alert_packs(old_location, move);   //checks if it is necessary to undo alert packs (Ralert rule)
                enter_last_zone(move);         //checks if it is really the last zone and apply RendZone rule
            }

            if (move != null && this.location.neighbors.Contains(move))
            {
                this.location.contested = false;
                this.location = move;
                move.visited = true;            
                
                //Pick up items.
                foreach (Item potion in this.location.items.ToList())
                {
                    if (potion is HealingPotion)                    
                        Console.WriteLine("You found a potion with {0} healing. Added to bag.", (potion as HealingPotion).HPvalue);
                    else
                        Console.WriteLine("You found a crystal! Added to bag.");

                    this.location.items.Remove(potion);
                    this.bag.Add(potion);
                    
                }
                if (this.location.packs.Count > 0)
                {
                    this.location.contested = true;
                    alert_packs(this.location); //(Ralert rule)
                }                                    
            }
            else
                throw new ArgumentException();
        }

        //alert all packs in zone of node parameter
        public void alert_packs(Node u)
        {
            uint zone_level = dungeon.level(u);
            foreach (Pack p in dungeon.alivePacks)
            {       
                if (dungeon.level(p.location) == zone_level) {p.alert = true;}
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
            if (dungeon.level(u) == (dungeon.level(dungeon.exitNode))) alert_packs(u);
        }
    }
}
