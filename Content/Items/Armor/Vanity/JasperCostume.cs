using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using Terraria.DataStructures;

namespace DogeisCutAdditions.Content.Items.Armor.Vanity
{

	[AutoloadEquip(EquipType.Head)]
	public class JasperCostumeHead : ModItem
	{
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
		}

		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Pink;
            Item.vanity = true;
        }
	}

	[AutoloadEquip(EquipType.Body)]
	public class JasperCostumeBody : ModItem
	{
		public override void SetStaticDefaults() {
			ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
			ArmorIDs.Body.Sets.HidesArms[Item.bodySlot] = true;
		}

		public override void Load() {
			if (Main.netMode == NetmodeID.Server)
                return;
			
			EquipLoader.AddEquipTexture(Mod, $"{Texture}_Tail", EquipType.Waist, this);
        }

		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Pink;
            Item.vanity = true;
        }
	}

	[AutoloadEquip(EquipType.Legs)]
	public class JasperCostumeLegs : ModItem
	{
		public override void SetStaticDefaults() {
			ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = true;
		}

		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Pink;
            Item.vanity = true;
        }
	}

	[AutoloadEquip(EquipType.Wings)]
	public class JasperCostumeWings : ModItem
	{
		public override void SetStaticDefaults() {
			// These wings use the same values as the solar wings
			// Fly time: 180 ticks = 3 seconds
			// Fly speed: 9
			// Acceleration multiplier: 2.5
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(25);
		}

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 20;
			Item.rare = ItemRarityID.Pink;
			Item.accessory = true;
		}

		// public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
		// 	ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		// 	ascentWhenFalling = 0.85f; // Falling glide speed
		// 	ascentWhenRising = 0.15f; // Rising speed
		// 	maxCanAscendMultiplier = 1f;
		// 	maxAscentMultiplier = 3f;
		// 	constantAscend = 0.135f;
		// }
	}
}