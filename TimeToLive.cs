using Godot;

namespace Munglo.Commons
{
    public partial class TimeToLive : Node3D
    {
        [Export] private double ttl = 10.0f;
        //public bool deRegAndDestroy = false;
        [Export] private bool targetParent = false;
        [Export] private bool destroy = false;

        private double timeleft;

        public override void _EnterTree()
        {
            timeleft = ttl;
        }
        public override void _Ready()
        {
            if(!IsMultiplayerAuthority()) { ProcessMode = ProcessModeEnum.Disabled; }
        }
        public void Enable()
        {
            if(!IsMultiplayerAuthority()) { ProcessMode = ProcessModeEnum.Inherit; }
        }
        public override void _Process(double delta)
        {
            timeleft -= delta;
            if (timeleft <= 0.0f)
            {
                if (targetParent)
                {
                    GetParent<Node3D>().Hide();
                }
                else
                {
                    Hide();
                }
                //if (deRegAndDestroy) { 
                //    AI.AIManager.Instance.DeRegisterAIObject(GetComponent<AIObject>().aiObjectID);
                //}
                if (destroy) { if(targetParent) { GetParent().QueueFree(); return; } else { QueueFree(); } }
            }
        }
    }
}