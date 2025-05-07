using Godot;
using Munglo.Commons;

namespace Munglo.AI
{
    internal partial class AIPlayerAvatar : AIObject
    {
        public override void _Ready()
        {
            if (GetTree().GetMultiplayer().IsServer() && AIManager.Instance is not null)
            {
                AIManager.Instance.RegisterAIObject(this, false);
                body = GetParent<RigidBody3D>();
            }
        }

        internal void SetFaction(int id)
        {
            AIManager.UpdateFaction(aiObjectID, id);
            factionID = id;
        }
    }
}
