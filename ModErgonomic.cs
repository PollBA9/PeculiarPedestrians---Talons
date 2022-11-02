using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class ModErgonomic : IModification
	{
		public ModErgonomic()
		{
		}

		public ModErgonomic(int Tier)
			: base(Tier)
		{
		}

		public override void Configure()
		{
			WorksOnSelf = true;
		}

		public override bool ModificationApplicable(GameObject Object)
		{
			string wornOn = Object.GetPart<Armor>()?.WornOn;
			return wornOn == "Feet" || wornOn == "*";
		}

		public override void ApplyModification(GameObject Object)
		{
			Object.SetStringProperty("Pedestrian_CanEquip", "True");
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (ID == GetShortDescriptionEvent.ID || ID == GetDisplayNameEvent.ID)
			{
				return true;
			}
			return base.WantEvent(ID, cascade);
		}

		public override bool HandleEvent(GetDisplayNameEvent E)
		{
			if (E.Understood() && !E.Object.HasProperName)
			{
				E.AddAdjective("{{Y|ergonomic}}");
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			E.Postfix.AppendRules("Ergonomic: This item is compatible with certain types of nonstandard anatomy.");
			return base.HandleEvent(E);
		}
	}
}
