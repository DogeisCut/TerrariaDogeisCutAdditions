using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DogeisCutAdditions.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class ChunkDeleter : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.DogeisCutAdditions.hjson' file.
		public override void SetDefaults()
		{
			Item.damage = 3;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 1;
			Item.useAnimation = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(platinum: 9999);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;

			Item.pick = 11220;
			Item.hammer = 11220;
			Item.axe = 11220;
			Item.attackSpeedOnlyAffectsWeaponAnimation = true;
			Item.useTurn = true;
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

			Lighting.AddLight(position, 1f, 0f, 0.8f);
		}

		public override void PostUpdate() {
			Lighting.AddLight(Item.Center, 1f, 0f, 0.8f);
		}
		

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 1);
			recipe.AddTile(TileID.Dirt);
			recipe.Register();
		}

		//public override void 
	}
}
