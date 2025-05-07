using System.Collections.Generic;
using Godot;
using Munglo.Commons;
using Munglo.GameEvents;
using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace Munglo.AI.Inventory
{
    /// <summary>
    /// THis is a boilerplate default class. Make sure everything in here goes through the interface since most would implement their own inventory
    /// </summary>
    [GlobalClass]
    public partial class AIInventory : Node, IInventory<Node>
    {
        private AIUnit unit;
        public List<Node> items;
        [Export] public float upForce = 60.0f;
        [Export] public float rotationalForce = 60.0f;

        public List<Node> Items { get => items; }
        [Export] public int Count { get => items.Count; set { return; } }

        private float customDataUpdateInterval = 0.5f;
        private float lastCustomDataUpdate = -10.0f;

        public void AttachInventoryToUnit(AIUnit aiUnit)
        {
            this.unit = aiUnit;
            items= new List<Node>();
        }
    


    public bool AddItem(Node item, bool parentToInventory=true)
        {
            items.Add(item);
            if (parentToInventory)
            {
                item.ProcessMode = ProcessModeEnum.Disabled;
                
                if(item.GetParent() is Node3D)
                {
                    item.GetParent().Reparent(this);
                    Node3D n3D = (Node3D)item.GetParent();
                    n3D.Hide();
                    n3D.Position = Vector3.Zero;
                    n3D.Rotation = Vector3.Zero;
                }
                else
                {
                }
            }
            return HaveItemWithID(item.GetInstanceId());
        }

        public bool HaveItemWithID(ulong id)
        {
            return items.Exists(p => p.GetInstanceId() == id);
        }

        public bool HaveItems<T>(int count)
        {
            return items.FindAll(p => p is T).Count >= count;
        }

        public int CountItems(string typeName)
        {
            return items.FindAll(p => p.GetType().ToString() == "typeName").Count;

        }
        public int CountItems<T>()
        {
            return items.FindAll(p => p is T).Count;
        }

        public bool RemoveItem(Node item)
        {
            items.Remove(item); 
            return !items.Exists(p => p.GetInstanceId() == item.GetInstanceId());
        }
        public bool RemoveItem<T>()
        {
            for (int i = 0; i < items.Count; i++)
            { 
                if (items[i] is T)
                {
                    items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        public void RemoveLastItem()
        {
            items.RemoveAt(items.Count - 1);
        }
        private void Start()
        {
            //aiObject = GetComponent<AIObject>();
            Munglo.GameEvents.Events.Units.OnUnitDeath += OnUnitDeath;
        }
        private void OnUnitDeath(object sender, UnitDeathEventArguments arg0)
        {
            if(unit.aiObjectID != arg0.AIObject.aiObjectID) { return; }
            foreach (Node item in items)
            {
                //item.transform.position = transform.position + Vector3.up * 0.5f;
                //item.transform.SetParent(null);
                //item.gameObject.SetActive(true);
                //item.GetComponent<Rigidbody>().AddForce(Vector3.up * upForce, ForceMode.Impulse);
                //item.GetComponent<Rigidbody>().AddRelativeTorque(Vector3.right * rotationalForce, ForceMode.Impulse);
            }
        }
        public IFood GetMostEfficientFood()
        {
            List<Node> picks = items.FindAll(p => p is IFood);
            //Debug.Log($"Inventory::GetMostEfficientFood() picks = {picks.Count}", gameObject);
            if (picks.Count < 1) { return null; }
            List<Node> sorted = picks.OrderBy(p => (p as IFood).Efficiancy).Reverse().ToList();
            items.Remove(sorted.First());
            return sorted.First() as IFood;
        }

        public bool HasAmmo()
        {
            throw new NotImplementedException();
        }

        public int PayAmmo(int chargesMax)
        {
            throw new NotImplementedException();
        }

        /*
        public bool CheckRecipe<T4>(IRecipe<T4> recipe)
        {
            foreach (ComponentEntry component in recipe.Pattern)
            {
                if(items.FindAll(p => p.GetComponent(System.Type.GetType(component.typeAssemblyName)) is not null).Count < component.amount)
                {
                    return false;
                }
            }
            return true;
        }

        public bool FillRecipe(IRecipe<GameObject> recipe, out List<GameObject> comps)
        {
            comps = new List<GameObject>();
            int count = 0;
            foreach (ComponentEntry component in recipe.Pattern)
            {
                count += component.amount;
                for (int x = 0; x < component.amount; x++)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].GetComponent(System.Type.GetType(component.typeAssemblyName)) && !comps.Exists(p=>p == items[i]))
                        {
                            comps.Add(items[i].gameObject);
                        }
                    }
                }
            }
            if (count == comps.Count)
            {
                // Recipie is fullfilled remove stuff from inventory and send them back in the out List
                foreach (GameObject comp in comps) { RemoveItem(comp); }
                return true;
            }
            comps = null;
            return false;
        }*/



        /*public override void _Process(double delta)
    {
        // Update custom data 
        if (AIManager.Selection.Selected == null || AIManager.Selection.Selected.aiObjectID != unit.aiObjectID) { return; }
        if ((float)(Time.GetTicksMsec() * 0.001f) > lastCustomDataUpdate + customDataUpdateInterval)
        {
            lastCustomDataUpdate = (float)(Time.GetTicksMsec() * 0.001f);
            AIDebugSignals.RaiseCustomDataEvent(
                    new AICustomData()
                    {
                        message = "Inventory",
                        sourceClass = this.GetType().AssemblyQualifiedName,
                        value = Count,
                        normalizedValue = 0.0f
                    }
                );
        }
    }*/
    }// EOF CLASS
}