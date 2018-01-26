using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.Utils;

namespace STVRogue.GameLogic
{
    public class Item
    {
        public String id;
	    public Boolean used = false ;
        public Item() { }
        public Item(String id) { this.id = id; }

        virtual public void use(Player player)
        {
            if (used) {
                Logger.log("" + player.id + " is trying to use an expired item: "
                              + this.GetType().Name + " " +  id 
                              + ". Rejected.");
                throw new ArgumentException();
            }
            Logger.log(""+ player.id + " uses " + this.GetType().Name + " " + id);
            used = true ;
        }
    }

    public class HealingPotion : Item
    {
        public uint HPvalue;

        /* Create a healing potion with random HP-value */
        public HealingPotion(String id) : base(id)
        {
            HPvalue = (uint) RandomGenerator.rnd.Next(10) + 1;
        }

        override public void use(Player player)
        {
            base.use(player);
            Console.Write("You use a potion!");
            int oldhp = player.HP;
            player.HP = (int) Math.Min(player.HPbase, player.HP + HPvalue);
            Console.WriteLine(" You healed for {0} HP.", player.HP - oldhp);

        }
    }

    public class Crystal : Item
    {
        //CAN ONLY BE USED DURING COMBAT
        public Crystal(String id) : base(id) { }
        override public void use(Player player)
        {
            if (!player.location.contested) throw new ArgumentException();
            base.use(player);
            Console.Write("You use a crystal!");
            player.accelerated = true;
            if (player.location is Bridge)
            {
                player.dungeon.disconnect(player.location as Bridge);
                player.location = player.dungeon.startNode;
                Console.Write(" You disconnected the bridge from the lower leveled zone.");
            }
            Console.WriteLine();
            
        }
    }
}
