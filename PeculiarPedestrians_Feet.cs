using System;
using System.Collections.Generic;
using System.Linq;

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
		public Dictionary<BodyPart, GameObject> FeetObjects;

		[NonSerialized]
		private Dictionary<string, HookType> _variants;
		private Dictionary<string, HookType> Variants
		{
			get
			{
				if (_variants == null)
				{
					_variants = new Dictionary<string, HookType>();
					_variants.Add("Hooks", new HookType());
					_variants.Add("Blades", new HookType("Blades for Feet", "sharp blades", "Pedestrian_Blades", "LongBlades"));
					_variants.Add("Mighty Hooves", new HookType("Mighty Hooves", "mighty hooves", "Pedestrian_Hooves", "Cudgel", true));
					_variants.Add("Cloven Hooves", new HookType("Cloven Hooves", "cloven hooves", "Pedestrian_ClovenHooves", "Cudgel", true));
					_variants.Add("Clawed Paws", new HookType("Clawed Paws", "clawed paws", "Pedestrian_Paws", "ShortBlades", true));
					_variants.Add("Bappy Paws", new HookType("Bappy Paws", "bappy paws", "Pedestrian_BappyPaws", "Cudgel", true));
					_variants.Add("Flippers", new HookType("Flippers", "flippers", "Pedestrian_Flippers", "Cudgel", true));
				}
				return _variants;
			}
		}

		public override void SaveData(SerializationWriter Writer)
		{
			Writer.WriteGameObjectList(FeetObjects.Values.ToList());
			Dictionary<int, int> feetObjectIDs = new Dictionary<int, int>();
			foreach (KeyValuePair<BodyPart, GameObject> kvp in FeetObjects)
			{
				feetObjectIDs.Add(FeetObjects.Values.ToList().IndexOf(kvp.Value), kvp.Key.ID);
			}
			Writer.Write(feetObjectIDs); // primitive typed dictionary is fine
			base.SaveData(Writer);
		}

		public override void LoadData(SerializationReader Reader)
		{
			List<GameObject> FeetObjectList = new List<GameObject>();
			Reader.ReadGameObjectList(FeetObjectList);
			Dictionary<int, int> feetObjectIDs = Reader.ReadDictionary<int, int>();
			FeetObjects = new Dictionary<BodyPart, GameObject>();
			Dictionary<int, BodyPart> partIDs = new Dictionary<int, BodyPart>();
			foreach (BodyPart part in ParentObject.Body.LoopParts())
			{
				if(feetObjectIDs.Values.ToList().IndexOf(part.ID) != -1)
				{
					partIDs.Add(part.ID, part);
				}
			}
			foreach (GameObject foot in FeetObjectList)
			{
				FeetObjects.Add(partIDs[feetObjectIDs[FeetObjectList.IndexOf(foot)]], foot);
			}
			base.LoadData(Reader);
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
			DisplayName = Variants.Values.ToList()[n].Name + " ({{r|D}})";
			base.SetVariant(n);
		}

		public override string GetDescription()
		{
			return $"You have {GetDescriptor()} for feet.\n\nYou cannot wear shoes.";
		}

		public override string GetLevelText(int Level)
		{
			return "";
		}

		public override void OnRegenerateDefaultEquipment(Body Body)
		{
			if(FeetObjects == null)
			{
				FeetObjects = new Dictionary<BodyPart, GameObject>();
				foreach (BodyPart limb in Body.LoopPart("Feet"))
				{
					UnityEngine.Debug.Log("Initialising FeetObjects for " + limb.Name);
					if (limb.Type == "Feet")
					{
						FeetObjects[limb] = GameObjectFactory.Factory.CreateObject(GetBlueprint());
					}
					if (!IsEveryLimb())
					{
						break;
					}
				}
			}
			UnityEngine.Debug.Log("Regenerating FeetObjects for " + ParentObject.DisplayName);
			foreach (var pair in FeetObjects) 
			{ 
				UnityEngine.Debug.Log($"{pair.Key.Name}: {pair.Value?.DisplayName ?? "null"}");
			}
			Dictionary<BodyPart, GameObject> newFeetObjects = new Dictionary<BodyPart, GameObject>(FeetObjects);
			foreach (BodyPart bodyPart in FeetObjects.Keys)
			{
				UnityEngine.Debug.Log("Generating " + GetDescriptor() + " for " + bodyPart.Name);
				var foot = FeetObjects[bodyPart];
				if (!GameObject.validate(ref foot))
				{
					foot = GameObjectFactory.Factory.CreateObject(GetBlueprint());
				}
				if (bodyPart != null && bodyPart.Equipped != foot && bodyPart.ForceUnequip(Silent: true))
				{
					MeleeWeapon part = foot.GetPart<MeleeWeapon>();
					ParentObject.ForceEquipObject(foot, bodyPart, Silent: true, 0);
				}
				newFeetObjects[bodyPart] = foot;
			}
			FeetObjects = newFeetObjects;
			base.OnRegenerateDefaultEquipment(Body);
		}

		public override bool Unmutate(GameObject GO)
		{
			foreach (GameObject foot in FeetObjects.Values)
			{
				CleanUpMutationEquipment(GO, foot);
			}
			return base.Unmutate(GO);
		}
	}
}
