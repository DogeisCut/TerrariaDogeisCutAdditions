using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DogeisCutAdditions.Content.Items
{ 
	public class DeveloperMultitool : ModItem
	{
		public override void SetDefaults()
		{
            Item.damage = 3000;
            Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 1;
			Item.useAnimation = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(platinum: 9999);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
            Item.scale = 4;

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
	}
}
