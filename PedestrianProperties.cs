using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts
{
	[Serializable]
	public class PedestrianProperties : IPart
	{
		public bool Bleeds = false;
		private const string Damage = "1";
		private const int SaveTarget = 20;

		public override bool SameAs(IPart p)
		{
			if ((p as PedestrianProperties).Bleeds != Bleeds)
			{
				return false;
			}
			return base.SameAs(p);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != GetDebugInternalsEvent.ID)
			{
				return ID == GetShortDescriptionEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetDebugInternalsEvent E)
		{
			E.AddEntry(this, "Bleeds?", Bleeds);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			if (Bleeds)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("On penetration, this weapon causes bleeding: ").Append(Damage).Append(" damage per round, save difficulty ")
					.Append(SaveTarget);
				E.Postfix.AppendRules(stringBuilder.ToString());
			}
			return base.HandleEvent(E);
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "QueryWeaponSecondaryAttackChance");
			Object.RegisterPartEvent(this, "CanEquipOverDefaultBehavior");
			if (Bleeds) Object.RegisterPartEvent(this, "WeaponDealDamage");
			base.Register(Object);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "CanEquipOverDefaultBehavior")
			{
				UnityEngine.Debug.Log("CanEquipOverDefaultBehavior");
				/*
				GameObject equippingObject = E.GetGameObjectParameter("Object");
				GameObject who = E.GetGameObjectParameter("Subject");
				BodyPart part = E.GetBodyPartParameter("Part");
				// TODO: Check for ergonomic mod
				*/
				return false;
			}
			else if (E.ID == "WeaponDealDamage" && Bleeds)
			{
				if (E.GetIntParameter("Penetrations") > 0)
				{
					GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
					GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
					gameObjectParameter?.ApplyEffect(new Bleeding(Damage, SaveTarget, gameObjectParameter2));
				}
			}
			else if (E.ID == "QueryWeaponSecondaryAttackChance")
			{
				if ((E.GetStringParameter("Properties", "") ?? "").Contains("Charging"))
				{
					E.SetParameter("Chance", 100);
				}
				else
				{
					E.SetParameter("Chance", 20);
				}
			}
			return base.FireEvent(E);
		}
	}
}
