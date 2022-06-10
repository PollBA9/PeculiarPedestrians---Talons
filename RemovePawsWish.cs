using System.Collections.Generic;
using System.Linq;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
[HasWishCommand]
public static class Wishes
{
	[WishCommand(Command = "noelle:removepaws")]
	public static void removePaws()
	{
		Mutations mutations = XRL.The.Player.GetPart("Mutations") as Mutations;
		if (mutations != null)
		{
			PeculiarPedestrians_Feet feet = mutations.GetMutation("Hooks for Feet") as PeculiarPedestrians_Feet;
			if (feet != null)
			{
				feet.Unmutate(XRL.The.Player);
			}
		}
		List<GameObject> equippedObjects = new List<GameObject>();
		XRL.The.Player.Body._Body.GetEquippedObjects(equippedObjects);
		foreach (GameObject foot in equippedObjects.Where(x => x.GetBlueprint().InheritsFrom("Pedestrian_Hooks")))
		{
			foot.ForceUnequipAndRemove(Silent: true);
		}
	}
	[WishCommand(Command = "noelle:regenpaws")]
	public static void regenPaws()
	{
		Mutations mutations = XRL.The.Player.GetPart("Mutations") as Mutations;
		if (mutations != null)
		{
			PeculiarPedestrians_Feet feet = mutations.GetMutation("Hooks for Feet") as PeculiarPedestrians_Feet;
			if (feet != null)
			{
				feet.Mutate(XRL.The.Player, feet.Level);
			}
		}
	}
}