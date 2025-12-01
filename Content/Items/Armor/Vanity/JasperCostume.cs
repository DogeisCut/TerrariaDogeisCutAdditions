using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
}