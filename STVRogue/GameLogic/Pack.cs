using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STVRogue.Utils;

namespace STVRogue.GameLogic
{
    public class Pack
    {
        public String id;
        public List<Monster> members = new List<Monster>();
        public int startingHP = 0;
        public Node location;
        public Dungeon dungeon;
        public bool alert = false;

        public Pack(String id, uint n, Node startinglocation, Dungeon d)
        {
            location = startinglocation;
            dungeon = d;

            this.id = id;
            for (int i = 0; i < n; i++)
            {
                Monster m = new Monster("" + id + "_" + i);
                members.Add(m);
                m.pack = this;
                m.location = this.location;
                startingHP += m.HP;
            }
        }

        public void Attack(Player p)
        {
            foreach (Monster m in members.ToList())
            {
                m.Attack(p);
                if (p.HP == 0) break;
                if (m.HP == 0) members.Remove(m);
            }

            location.contested = members.Count > 0 && location.packs.Count > 0;
        }

        /* Move the pack to an adjacent node. */
        public void move(Node u, Node playerlocation = null)
        {
            if (u == location) return;
            if (!location.neighbors.Contains(u) || u == null) throw new ArgumentException();
            uint capacity = dungeon.M * (dungeon.level(u) + 1);
            if (u.CountCreatures() + this.members.Count() >= capacity)
            {
                Logger.log("Pack " + id + " is trying to move to an already full node " + u.id + ". Rejected.");
                //throw new ArgumentException();
            }
            else if (u == dungeon.exitNode)
            {

                Logger.log("Pack " + id + " is trying to move to the exit node, rejected.");
                // throw new ArgumentException();
            }
            else
            {
                location.packs.Remove(this);
                location = u;
                if (location == playerlocation && members.Count > 0) location.contested = true;
                foreach (Monster m in members.ToList())
                    m.location = u;
                u.packs.Add(this);
            }
        }

        /* Move the pack one node further along a shortest path to u. */
        public void moveTowards(Node u, Node playerlocation = null)
        {
            //Console.WriteLine("from: " +  this.location.id + " to: " + u.id);
            List<Node> path = dungeon.shortestpath(location, u).Distinct().ToList();
            path.Reverse();
            path.Remove(path[0]);          
            if (!(path.Count > 0))
                return;
            move(path[0],playerlocation);
            if(playerlocation != null)
                playerlocation.contested = playerlocation.packs.Count > 0;
        }


        public Node getRandomValidNeighbour(Dungeon d)
        {
            List<Node> candidates = new List<Node>();

            foreach (Node n in this.location.neighbors.ToList())
                if (n.CountCreatures() + this.members.Count < n.Capacity(d) && d.level(n) == d.level(this.location) && n != dungeon.exitNode)
                    candidates.Add(n);
            if (candidates.Count() == 0)
                return this.location;
            else
                return candidates.GetRandomItem();
        }
    }
}
