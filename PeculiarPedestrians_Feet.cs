using System;
using System.Collections.Generic;
using System.Linq;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class PeculiarPedestrians_Feet : BaseDefaultEquipmentMutation
	{
		[NonSerialized]
		public List<BodyPart> RegisteredParts;

		[NonSerialized]
		public List<int> RegisteredPartIDs;

		public string BlueprintName => Variant.Coalesce("Pedestrian_Hooks");

		[NonSerialized] protected GameObjectBlueprint _Blueprint;
		public GameObjectBlueprint Blueprint
		{
			get
			{
				_Blueprint ??= GameObjectFactory.Factory.GetBlueprint(BlueprintName);
				return _Blueprint;
			}
		}

		public override void SaveData(SerializationWriter Writer)
		{
			Writer.Write<int>(RegisteredParts?.Select(x => x.ID).ToList());
			base.SaveData(Writer);
		}

		public override void LoadData(SerializationReader Reader)
		{
			RegisteredPartIDs = Reader.ReadList<int>() ?? new List<int>();
			base.LoadData(Reader);
		}

		public override void FinalizeLoad()
		{
			/*
			foreach (int id in RegisteredPartIDs)
			{
				UnityEngine.Debug.Log("BodyPart id: " + id + ", name: " + ParentObject?.Body?._Body?.GetPartByID(id)?.GetOrdinalName());
			}
			*/
			RegisteredParts = RegisteredPartIDs.Select(x => ParentObject.Body._Body.GetPartByID(x)).ToList();
			base.FinalizeLoad();
		}

		public PeculiarPedestrians_Feet()
		{

			DisplayName = "Hooks for Feet ({{r|D}})";
		}

		public override bool UseVariantName => false; // Does not currently respect defect status

		public override bool CanLevel()
		{
			return false;
		}

		public override bool GeneratesEquipment()
		{
			return true;
		}

		public string GetDescriptor()
		{
			return Blueprint.GetTag("Pedestrian_Descriptor", "sharp hooks");
		}

		public string GetSkill()
		{
			return Blueprint.GetPartParameter<string>("MeleeWeapon", "Skill", "ShortBlades");
		}

		public bool IsEveryLimb()
		{
			return Blueprint.GetTag("Pedestrian_bEveryLimb", "false").EqualsNoCase("true");
		}

		public override void SetVariant(string Variant)
		{
			base.SetVariant(Variant);
			DisplayName = GetVariantName().Coalesce("Hooks for Feet") + " ({{r|D}})";
			_Blueprint = null;
		}

		public override string GetDescription()
		{
			return $"You have {GetDescriptor()} for feet.\n\nBladed or hooked feet cannot wear shoes; other variants can only wear modified shoes.";
		}

		public override string GetLevelText(int Level)
		{
			return "";
		}

		public override void OnRegenerateDefaultEquipment(Body Body)
		{
			if (RegisteredParts == null)
			{
				RegisteredParts = new List<BodyPart>();
				foreach (BodyPart limb in Body.LoopPart("Feet"))
				{
					//UnityEngine.Debug.Log("Initialising FeetObjects for " + limb.Name);
					if (limb.Type == "Feet")
					{
						RegisteredParts.Add(limb);
					}
					if (!IsEveryLimb())
					{
						break;
					}
				}
			}
			if (RegisteredParts.Count() < (IsEveryLimb() ? Body.GetPartCount("Feet") : 1))
			{
				var targetCount = IsEveryLimb() ? Body.GetPartCount("Feet") : 1;
				//UnityEngine.Debug.Log("Regenerating missing feet objects for " + ParentObject.DisplayName);
				foreach (BodyPart limb in Body.LoopPart("Feet"))
				{
					if (RegisteredParts.Count() >= targetCount)
					{
						break;
					}
					if (limb.Type == "Feet" && !RegisteredParts.Contains(limb))
					{
						RegisteredParts.Add(limb);
					}
				}
			}
			//UnityEngine.Debug.Log("Regenerating ID list for " + ParentObject.DisplayName);
			RegisteredPartIDs = RegisteredParts.Select(x => x.ID).ToList();
			//UnityEngine.Debug.Log("Checking feet objects for " + ParentObject.DisplayName);
			foreach (BodyPart bodyPart in RegisteredParts)
			{
				//UnityEngine.Debug.Log("Checking " + GetDescriptor() + " on " + bodyPart.GetOrdinalName());
				var foot = bodyPart.DefaultBehavior;
				if (!GameObject.Validate(ref foot))
				{
					//UnityEngine.Debug.Log("Regenerating " + GetDescriptor() + " for " + bodyPart.GetOrdinalName());
					foot = GameObjectFactory.Factory.CreateObject(Blueprint);
				}
				else // Nothing needs to be done here.
				{
					//UnityEngine.Debug.Log(bodyPart.GetOrdinalName() + " already has " + GetDescriptor());
					continue;
				}
				if (bodyPart != null)
				{
					if (bodyPart.Equipped != null && !bodyPart.Equipped.HasTagOrStringProperty("Pedestrian_CanEquip"))
					{
						//UnityEngine.Debug.Log("Unequipping " + bodyPart.Equipped.DisplayName + " on " + bodyPart.GetOrdinalName());
						bodyPart.ForceUnequip(Silent: true);
					}
					bodyPart.DefaultBehavior = foot;
				}
			}
			base.OnRegenerateDefaultEquipment(Body);
		}

		public override bool Unmutate(GameObject GO)
		{
			foreach (BodyPart bodyPart in RegisteredParts)
			{
				CleanUpMutationEquipment(GO, bodyPart.DefaultBehavior);
			}
			return base.Unmutate(GO);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (ID == GetExtraPhysicalFeaturesEvent.ID)
			{
				return true;
			}
			return base.WantEvent(ID, cascade);
		}

		public override bool HandleEvent(GetExtraPhysicalFeaturesEvent E)
		{
			E.Features.Add(GetDescriptor());
			return base.HandleEvent(E);
		}
	}
}
