using Godot;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Munglo.DungeonGenerator
{
	[Tool]
	public partial class ModuleVariant : Node3D
	{
		[Export] ModuleVariantResource[] variants;
		[Export] Node3D[] parts;

		public string VariantNames => GetAllNames();

        private string GetAllNames()
        {
			string names = string.Empty;
			foreach (var variant in variants) { names += variant.variantName + " "; }
			return names;
        }

        private void HideAllParts()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].Hide();
            }
        }

       
        public bool SetVariant(int variantID)
		{
            HideAllParts();
            if (VariantExist(variantID, out ModuleVariantResource variantResource))
            {
                GD.Print($"ModuleVariant::SetVariant(variantID={variantID}) variantResource[{variantResource.variantName}]");
                foreach (int partID in variantResource.parts)
                {
                    if (parts.Length > partID)
                    {

                        parts[partID].Show();
                    }
                    else
                    {
                        GD.PrintErr($"ModuleVariant::SetVariant(variantID={variantID}) PartID[{partID}] outside partlist");
                        return false;
                    }
                }
            }
            return true;
        }

        private bool VariantExist(int variantID, out ModuleVariantResource variantResource)
        {
            variantResource = null;

            if (variantID < variants.Length)
            {
                variantResource = variants[variantID];
                return true;
            }
            return false;
        }
    }
}
