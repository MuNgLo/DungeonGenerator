using Godot;
using Munglo.AI.Base;
using System.Linq;

namespace Munglo.Commons
{
    /// <summary>
    /// Drop this into scene under AIManager
    /// </summary>
    public partial class Factions : Node
    {
        private Faction[] factions;
        private Faction noFaction;

        public override void _EnterTree(){
            noFaction = new Faction(){id=-1,factionName="Unwanted"};
            factions = new Faction[]
            {
                new(){id=0,factionName="Players", color=Colors.Blue,
                    relations= new FactionRelation[] {
                        new() { id=-1, state= FACTIONRELATIONSTATE.NEUTRAL },
                        new() { id=1, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=2, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=3, state= FACTIONRELATIONSTATE.HOSTILE }
                    } 
                }, 
                new(){id=1,factionName="Neutral", color=Colors.Red, 
                    relations= new FactionRelation[] { 
                        new() { id=-1, state= FACTIONRELATIONSTATE.NEUTRAL },
                        new() { id=0, state= FACTIONRELATIONSTATE.NEUTRAL },
                        new() { id=2, state= FACTIONRELATIONSTATE.NEUTRAL },
                        new() { id=3, state= FACTIONRELATIONSTATE.NEUTRAL }

                    }
                },
                new(){id=2,factionName="DungeonNPC", color=Colors.Red,
                    relations= new FactionRelation[] {
                        new() { id=-1, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=0, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=2, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=3, state= FACTIONRELATIONSTATE.HOSTILE }
                    }
                },
                new(){id=3,factionName="GhostWiz", color=Colors.Red,
                    relations= new FactionRelation[] {
                        new() { id=-1, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=0, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=1, state= FACTIONRELATIONSTATE.HOSTILE },
                        new() { id=2, state= FACTIONRELATIONSTATE.HOSTILE }
                    }
                }
            };
        }
        /// <summary>
        /// Returns registered faction or a nofaction carrying the id -1 and name Unwanted
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Faction GetFaction(int id){
            for (int i = 0; i < factions.Length; i++)
            {
                if(factions[i].id == id){
                    return factions[i];
                }
            }
            return noFaction;
        }
        public FACTIONRELATIONSTATE GetFactionRelationship(int from, int to)
        {
            for (int i = 0; i < factions.Length; i++)
            {
                if (factions[i].id == from)
                {
                    for (int x = 0; x < factions[i].relations.Length; x++)
                    {
                        if(factions[i].relations[x].id == to){
                            return factions[i].relations[x].state;
                        }
                    }
                }
            }
            return FACTIONRELATIONSTATE.NEUTRAL;
        }
    }// EOF CLASS
}