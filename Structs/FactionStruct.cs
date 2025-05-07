using System;
namespace Munglo.Commons{
    public struct Faction
    {
        public int id;
        public string factionName;
        public Godot.Color color;
        public FactionRelation[] relations;
    }
}