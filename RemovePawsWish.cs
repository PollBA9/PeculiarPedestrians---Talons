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
	public static void RemovePaws()
	{
		if (XRL.The.Player.GetPart("Mutations") is Mutations mutations)
		{
			if (mutations.GetMutation("Hooks for Feet") is PeculiarPedestrians_Feet feet)
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
	public static void RegenPaws()
	{
		if (XRL.The.Player.GetPart("Mutations") is Mutations mutations)
		{
			if (mutations.GetMutation("Hooks for Feet") is PeculiarPedestrians_Feet feet)
			{
				feet.Mutate(XRL.The.Player, feet.Level);
			}
		}
	}
}