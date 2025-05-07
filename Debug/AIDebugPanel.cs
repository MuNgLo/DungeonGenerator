using Godot;
using Munglo.AI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Munglo.AI.Debug
{
    public partial class AIDebugPanel : Control
    {
        Control actionEntryPrefab;
        Control actionEntries;

        List<ActionDebugInfoStruct> actions;
        Dictionary<string, AICustomDataStruct> customData;

        RichTextLabel unitName;
        RichTextLabel id;
        RichTextLabel fid;
        RichTextLabel fname;
        RichTextLabel AIState;
        RichTextLabel FState;
        RichTextLabel MState;
        RichTextLabel currentAction;
        RichTextLabel customDataLabel;
        AIDebugSignals debugSignals;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            actionEntryPrefab = GetNode<Control>("Panel/VFlowContainer/pfActionEntry");
            actionEntryPrefab.GetParent().RemoveChild(actionEntryPrefab);
            actionEntries = GetNode<Control>("Panel/VFlowContainer");

            unitName = GetNode<RichTextLabel>("Panel/Panel/UnitName");
            id = GetNode<RichTextLabel>("Panel/Panel/ID");
            fid = GetNode<RichTextLabel>("Panel/Panel/FID");
            fname = GetNode<RichTextLabel>("Panel/Panel/FNAME");
            AIState = GetNode<RichTextLabel>("Panel/Panel/AIState2");
            FState = GetNode<RichTextLabel>("Panel/Panel/AIFightState2");
            MState = GetNode<RichTextLabel>("Panel/Panel/AIMovementState2");
            currentAction = GetNode<RichTextLabel>("Panel/Panel/CurrentAction2");

            customDataLabel = GetNode<RichTextLabel>("Panel/CDPanel/CustomData");

            GetNode<Button>("Panel/Buttons/btnKill").ButtonDown += AIManager.Selection.UnitKill;
            GetNode<Button>("Panel/Buttons/btnHeal").ButtonDown += AIManager.Selection.UnitHeal;
            GetNode<Button>("Panel/Buttons/btnInterupt").ButtonDown += AIManager.Selection.UnitInterupt;
            GetNode<Button>("Panel/Buttons/btnReset").ButtonDown += AIManager.Selection.UnitResetMind;

            debugSignals = new();

            debugSignals.OnActionPossible += AddAction;
            //debugSignals.OnActionNotPossible += AddNonAction;
            debugSignals.ClearDebugInfo += ClearInfo;
            debugSignals.OnAILog += OnAILog;
            debugSignals.OnAICustomData += OnAICustomdata;
            ClearInfo(null, null);
        }

        private void OnAILog(object sender, AILogMessageStruct e)
        {
            GD.Print($"AIDebugPanel::OnAILog() {e.message}.");

        }

        private void OnAICustomdata(object sender, AICustomDataStruct e)
        {
            //GD.Print($"AIDebugPanel::OnAICustomdata() {e.message}[{e.normalizedValue}].");
            customData[e.sourceClass] = e;
        }

        private void ClearInfo(object sender, EventArgs e)
        {
            actions = new List<ActionDebugInfoStruct>();
            customData = new Dictionary<string, AICustomDataStruct> { };
            customDataLabel.Text = string.Empty;
            if (AIManager.Selection != null && AIManager.Selection.SelectedUnit != null)
            {
                AIManager.Selection.SelectedUnit.DebugPath(false);
            }


            foreach (Control child in actionEntries.GetChildren())
            {
                child.Hide();
                child.QueueFree();
            }
        }

        private void AddAction(Object sender, ActionDebugInfoStruct incomming)
        {
            if (actions.Contains(incomming))
            {
                actions.RemoveAll(p => p == incomming);
            }
            actions.Add(incomming);
            //List<ActionNonPossibleDebugInfo> list2 = (List<ActionNonPossibleDebugInfo>)nonActions.itemsSource;
            //if (list2.Exists(p => p.name == incomming.name))
            //{
            //    list2.RemoveAll(p => p.name == incomming.name);
            //}
            actions = actions.OrderBy(p => p.clampedPriority).Reverse().ToList();
        }


        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            if (!Visible) return;

            if (AIManager.Selection.SelectedUnit != null)
            {
                AIUnit unit = AIManager.Selection.SelectedUnit;
                unit.DebugPath(true);

                unitName.Text = unit.Body.Name;

                id.Text = "ID: " + unit.aiObjectID.ToString();
                fid.Text = "FID: " + unit.factionID.ToString();
                Color col = AIManager.Factions.GetFaction(unit.factionID).color;
                fname.Text = $"[color={col.ToHtml(false)}]{AIManager.Factions.GetFaction(unit.factionID).factionName}[/color]";


                AIState.Text = unit.State.ToString();
                FState.Text = unit.FightState.ToString();
                MState.Text = unit.MovementState.ToString();
                currentAction.Text = unit.CurrentAction.Name;

                if (actions.Count > 0)
                {

                    for (int i = 0; i < actions.Count; i++)
                    {
                        if (i >= actionEntries.GetChildCount())
                        {
                            Control obj = actionEntryPrefab.Duplicate() as Control;
                            actionEntries.AddChild(obj);
                            obj.MouseEntered += () => { obj.GetNode<Control>("Extra").Show(); };
                            obj.MouseExited += () => { obj.GetNode<Control>("Extra").Hide(); };
                        }
                        actionEntries.GetChild(i).GetNode<RichTextLabel>("Panel2/FULLPRIO").Text = $"{ColourPrioText(actions[i].clampedPriority.ToString(), actions[i].clampedPriority, actions[i].minPriority, actions[i].maxPriority)}";
                        actionEntries.GetChild(i).GetNode<RichTextLabel>("Panel2/PRIOBREAKDOWN").Text = $"({ColourPrioText(actions[i].minPriority.ToString(), actions[i].maxPriority, actions[i].minPriority, actions[i].maxPriority, doGreen: false, doRed: false)}" +
                            $"{ColourPrioText("<->", actions[i].clampedPriority, actions[i].minPriority, actions[i].maxPriority, doYellow: false, doRed: false)}" +
                            $"{ColourPrioText(actions[i].maxPriority.ToString(), actions[i].priorityFull, actions[i].minPriority, actions[i].maxPriority, doGreen: false, doYellow: false)})" +
                            $"({ColourPrioText(actions[i].priorityFull.ToString(), actions[i].priorityFull, actions[i].minPriority, actions[i].maxPriority)})";
                        actionEntries.GetChild(i).GetNode<RichTextLabel>("Panel2/ACTIONNAME").Text = actions[i].name;

                        // build bar
                        float barWidth = 120.0f;
                        float barLength = Mathf.Abs(actions[i].priorityFull);
                        Vector2 size = actionEntries.GetChild(i).GetNode<Panel>("Panel2/BAR1").Size;
                        Vector2 pos = actionEntries.GetChild(i).GetNode<Panel>("Panel2/BAR1").Position;


                        size.X = ((actions[i].startPriority + actions[i].modInternal - Mathf.Clamp(actions[i].modMultiplierPriority - actions[i].modValuePriority, 0.0f, 1000.0f)) / actions[i].priorityFull) * barWidth;
                        actionEntries.GetChild(i).GetNode<Panel>("Panel2/BAR1").Size = size;

                        pos.X += size.X;
                        size.X = actions[i].modValuePriority > 0.0f ? (actions[i].modValuePriority / actions[i].priorityFull) * barWidth : 0.0f;
                        actionEntries.GetChild(i).GetNode<Panel>("Panel2/BAR2").Size = size;
                        actionEntries.GetChild(i).GetNode<Panel>("Panel2/BAR2").Position = pos;


                        pos.X += size.X;
                        size.X = actions[i].modMultiplierPriority > 0.0f ? (actions[i].modMultiplierPriority / actions[i].priorityFull) * barWidth : 0.0f;
                        actionEntries.GetChild(i).GetNode<Panel>("Panel2/BAR3").Size = size;
                        actionEntries.GetChild(i).GetNode<Panel>("Panel2/BAR3").Position = pos;

                        string breakdown = string.Empty;
                        for (int x = 0; x < actions[i].influences.Length; x++)
                        {
                            ActionDebugInfluenceStruct infl = actions[i].influences[x];
                            breakdown += $"{infl.source}[{infl.value}][*{infl.multiplier}]";
                        }

                        actionEntries.GetChild(i).GetNode<RichTextLabel>("Extra/BREAKDOWN").Text = breakdown;

                        //custom data
                        string cd = string.Empty;
                        foreach (KeyValuePair<string, AICustomDataStruct> data in customData)
                        {
                            cd += $"{data.Key}[{data.Value.message}({data.Value.normalizedValue.ToString("0.00")})]";
                        }
                        customDataLabel.Text = cd;

                    }
                }
            }
        }

        private string ColourPrioText(string text, int value, int min, int max, bool doGreen = true, bool doRed = true, bool doYellow = true)
        {
            if (doYellow && value <= min)
            {
                return $"[color=yellow]{text}[/color]";
            }
            if (doRed && value >= max)
            {
                return $"[color=red]{text}[/color]";
            }
            if (doGreen)
            {
                return $"[color=green]{text}[/color]";
            }
            return text;
        }
        /*private void OnReset()
    {
        if (!Visible) return;

        if (AIManager.Selection.SelectedUnit != null)
        {
            AIManager.Selection.SelectedUnit.DebugPath(true);

            AIManager.Selection.SelectedUnit.Mind.PoolReset();
            AIManager.Selection.SelectedUnit.Mind.StartAI();
        }
    }*/
    }// EOF CLASS
}