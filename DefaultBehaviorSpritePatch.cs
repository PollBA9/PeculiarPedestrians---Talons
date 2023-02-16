using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using XRL.UI;
using XRL.World;

namespace PeculiarPedestrians.Harmony
{
	[HarmonyPatch]
	public class DefaultBehaviorSpritePatch
	{
		public static bool DefaultBehaviorHasColorInMenu(GameObject DefaultBehavior)
		{
			return DefaultBehavior.HasTag("DefaultBehaviorHasColorInMenu");
		}
		[HarmonyPatch(typeof(EquipmentScreen), nameof(EquipmentScreen.Show))]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var code = new List<CodeInstruction>(instructions);
			// If DefaultBehaviorHasColorInMenu(DefaultBehavior) is true, then
			// ldnull, ldnull, ldc.i4.0, initobj Nullable<char>
			// else, ldstr "&K", ldstr "&K", ldc.i4.s 75, newobj Nullable<char>.ctor(char)
			// insert idx is:
			//    ldfld class XRL.World.GameObject XRL.World.BodyPart::DefaultBehavior
			//    callvirt instance class XRL.World.RenderEvent XRL.World.GameObject::RenderForUI()
			// To find this, we use Select() to transform each to a bool, corresponding to the filter condition; then, we get the first true value.
			var insertidx = code.Select((x, idx) => (x.Calls(AccessTools.PropertyGetter(typeof(XRL.World.Anatomy.BodyPart), nameof(XRL.World.Anatomy.BodyPart.DefaultBehavior))) && idx < code.Count() && code[idx+1].Calls(AccessTools.Method(typeof(XRL.World.GameObject), nameof(XRL.World.GameObject.RenderForUI))))).ToList().IndexOf(true);
			if (insertidx != -1)
			{
				// Create a new local bool variable
				var local = generator.DeclareLocal(typeof(bool));
				// Create a new local Nullable<char> variable, for some reason.
				var nullableChar = generator.DeclareLocal(typeof(Nullable<char>));
				// Create a new label, nullLabel.
				var nullLabel = generator.DefineLabel();
				// Create a new label, endLabel.
				var endLabel = generator.DefineLabel();
				// dup after ldfld (idx)
				code.Insert(insertidx+1, new CodeInstruction(OpCodes.Dup));
				// Call DefaultBehaviorHasColorInMenu on the dup'd value.
				code.Insert(insertidx+2, CodeInstruction.Call(typeof(DefaultBehaviorSpritePatch), nameof(DefaultBehaviorHasColorInMenu)));
				// Store it in our local variable.
				code.Insert(insertidx+3, new CodeInstruction(OpCodes.Stloc, local));

				// Ldloc local
				code.Insert(insertidx+7, new CodeInstruction(OpCodes.Ldloc, local));
				// Brtrue nullLabel
				code.Insert(insertidx+8, new CodeInstruction(OpCodes.Brtrue, nullLabel));
				// ldstr "&K", ldstr "&K", etc etc - already there
				// br endlabel
				code.Insert(insertidx+13, new CodeInstruction(OpCodes.Br, endLabel));
				// ldnull
				code.Insert(insertidx+14, new CodeInstruction(OpCodes.Ldnull));
				// nullLabel:
				code[insertidx+14].labels = new List<Label>() { nullLabel };
				// ldnull
				code.Insert(insertidx+15, new CodeInstruction(OpCodes.Ldnull));
				// ldloca.s nullableChar
				code.Insert(insertidx+16, new CodeInstruction(OpCodes.Ldloca_S, nullableChar));
				// initobj Nullable<char>
				code.Insert(insertidx+17, new CodeInstruction(OpCodes.Initobj, typeof(Nullable<char>)));
				// ldloc nullableChar
				code.Insert(insertidx+18, new CodeInstruction(OpCodes.Ldloc, nullableChar));
				// endlabel
				code[insertidx+19].labels.Add(endLabel);
			}
			else
			{
				UnityEngine.Debug.Log("PeculiarPedestrians.DefaultBehaviorSpritePatch: Could not find insertion point.");
			}
			return code;
		}
	}
}