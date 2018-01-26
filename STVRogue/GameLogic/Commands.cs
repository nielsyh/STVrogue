using System;
using STVRogue.GameLogic;
using STVRogue.Utils;
using System.Collections.Generic;
using System.Linq;

namespace STVRogue
{
    [Serializable]
    public abstract class Command
    {
        public int option = 0;

        public Command() { }
        public abstract void Execute(Player p, Dungeon d);

    }

    [Serializable]
    public class MoveCommand : Command
    {
        public MoveCommand()
        {
            base.option = 0;
        }
        public MoveCommand(int i) { base.option = i; }
        public override void Execute(Player p, Dungeon d)
        {
            if (p.location.neighbors.Count > 0)
            {
                Node move = p.location.neighbors[base.option];
                p.Move(move);

                Logger.log("player moved to " + p.location.id);
                //Logger.log("Location contains "+ p.location.packs.Count + " packs. Beware!!");
            }
            else
                throw new ArgumentException();
        }
        //override public string ToString() { return "MOVE!"; }

    }

    [Serializable]
    public class UseItemCommand : Command
    {
        public UseItemCommand() { base.option = 0; }

        public UseItemCommand(int i) { base.option = i; }
        //execute item use
        public override void Execute(Player p, Dungeon d)
        {

            if (p.bag.Count > 0)
            {
                Random r = RandomGenerator.rnd;
                Item item = null;
                while (item == null)
                {
                    int random = r.Next(p.bag.Count);
                    if (base.option == 0 && p.bag[random] is HealingPotion)
                        item = p.bag[random];
                    else if (base.option == 1 && p.bag[random] is Crystal)
                        item = p.bag[random];

                }
                p.use(item);
                Logger.log($"Player used {item.ToString()}");
            }
            else
                throw new ArgumentException();

        }
        //override public string ToString() { return "USE ITEM!"; }

    }

    [Serializable]
    public class AttackCommand : Command
    {
        public AttackCommand() { base.option = 0; }

        public AttackCommand(int i) { base.option = i; }
        public override void Execute(Player p, Dungeon d)
        {
            if (p.location.contested && p.location.packs.Count != 0)
            {
                p.Attack(p.location.packs[base.option].members.GetRandomItem());
                if (p.location.contested)
                {
                    p.location.fight(p, d);
                }
            }
            else
            {
                throw new ArgumentException();
            }

        }
    }






}
