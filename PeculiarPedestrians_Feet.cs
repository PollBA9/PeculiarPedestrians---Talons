using System;
using System.Collections.Generic;
using System.Linq;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class PeculiarPedestrians_Feet : BaseDefaultEquipmentMutation
	{
		class HookType
		{
			public string Name = "Hooks for Feet";
			public string Descriptor = "sharp hooks";
			public string Blueprint = "Pedestrian_Hooks";
			public string Skill = "ShortBlades";
			public bool bEveryLimb = false;
			public HookType()
			{
			}
			public HookType(string name, string descriptor, string blueprint, string skill, bool everyLimb = false)
			{
				Name = name;
				Descriptor = descriptor;
				Blueprint = blueprint;
				Skill = skill;
				bEveryLimb = everyLimb;
			}
		}

		[NonSerialized]
		public List<BodyPart> RegisteredParts;
		[NonSerialized]
		public List<int> RegisteredPartIDs;

		[NonSerialized]
		private Dictionary<string, HookType> _variants;
		private Dictionary<string, HookType> Variants
		{
			get
			{
				if (_variants == null)
				{
					_variants = new Dictionary<string, HookType>
					{
						{ "Hooks", new HookType() },
						{ "Blades", new HookType("Blades for Feet", "sharp blades", "Pedestrian_Blades", "LongBlades") },
						{ "Mighty Hooves", new HookType("Mighty Hooves", "mighty hooves", "Pedestrian_Hooves", "Cudgel", true) },
						{ "Cloven Hooves", new HookType("Cloven Hooves", "cloven hooves", "Pedestrian_ClovenHooves", "Cudgel", true) },
						{ "Clawed Paws", new HookType("Clawed Paws", "clawed paws", "Pedestrian_Paws", "ShortBlades", true) },
						{ "Bappy Paws", new HookType("Bappy Paws", "bappy paws", "Pedestrian_BappyPaws", "Cudgel", true) },
						{ "Flippers", new HookType("Flippers", "flippers", "Pedestrian_Flippers", "Cudgel", true) }
					};
				}
				return _variants;
			}
		}

		public override void SaveData(SerializationWriter Writer)
		{
			Writer.Write<int>(RegisteredParts.Select(x => x.ID).ToList());
			base.SaveData(Writer);
		}

		public override void LoadData(SerializationReader Reader)
		{
			RegisteredPartIDs = Reader.ReadList<int>();
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
			return Variants[Variant ?? "Hooks"].Descriptor;
		}

		public string GetBlueprint()
		{
			return Variants[Variant ?? "Hooks"].Blueprint;
		}

		public string GetSkill()
		{
			return Variants[Variant ?? "Hooks"].Skill;
		}

		public bool IsEveryLimb()
		{
			return Variants[Variant ?? "Hooks"].bEveryLimb;
		}

		public override List<string> GetVariants()
		{
			return Variants.Keys.ToList();
		}

		public override void SetVariant(int n)
		{
			HookType selectedType = Variants.Values.ToList()[n];
			DisplayName = selectedType.Name + " ({{r|D}})";
			base.SetVariant(n);
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
			if(RegisteredParts == null)
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
				if (!GameObject.validate(ref foot))
				{
					//UnityEngine.Debug.Log("Regenerating " + GetDescriptor() + " for " + bodyPart.GetOrdinalName());
					foot = GameObjectFactory.Factory.CreateObject(GetBlueprint());
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