using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DogeisCutAdditions.Content.Items.Ammo
{
	public class UltimatePurifier : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ItemID.Sets.SortingPriorityTerraforming[Type] = 101;
		}

		public override void SetDefaults() {
			Item.DefaultToSolution(ModContent.ProjectileType<UltimatePurifierProjectile>());
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
		}
	}

	public class UltimatePurifierProjectile : ModProjectile
	{
		public ref float Progress => ref Projectile.ai[0];

		public bool ShotFromTerraformer => Projectile.ai[1] == 1f;

		public override void SetDefaults() {
			Projectile.DefaultToSpray();
			Projectile.aiStyle = 0;
		}

		public override bool? CanDamage() => false;

		public override void AI() {

			if (Projectile.timeLeft > 133)
				Projectile.timeLeft = 133;

			if (Projectile.owner == Main.myPlayer) {
				Point tileCenter = Projectile.Center.ToTileCoordinates();
                int deletionRadius = ShotFromTerraformer ? 4 : 2;

                for (int x = tileCenter.X - deletionRadius; x <= tileCenter.X + deletionRadius; x++) {
                    for (int y = tileCenter.Y - deletionRadius; y <= tileCenter.Y + deletionRadius; y++) {
                        if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
                            continue;

                        Tile tile = Main.tile[x, y];

                        if (tile.HasTile) {
                            WorldGen.KillTile(x, y, false, false, true);
                        }

                        if (tile.WallType > 0) {
                            WorldGen.KillWall(x, y, false);
                        }

                        tile.LiquidAmount = 0;
                        tile.LiquidType = 0;

                        tile.RedWire = false;
                        tile.GreenWire = false;
                        tile.BlueWire = false;
                        tile.YellowWire = false;

                        tile.HasActuator = false;
                        tile.IsActuated = false;
                    }
                }

                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendTileSquare(-1, tileCenter.X, tileCenter.Y, deletionRadius + 1);
                }
			}

			int spawnDustTreshold = 7;
			if (ShotFromTerraformer)
				spawnDustTreshold = 3;

			if (Progress > (float)spawnDustTreshold) {
				float dustScale = 1f;
				int dustType = DustID.RainbowTorch;

				if (Progress == spawnDustTreshold + 1)
					dustScale = 0.2f;
				else if (Progress == spawnDustTreshold + 2)
					dustScale = 0.4f;
				else if (Progress == spawnDustTreshold + 3)
					dustScale = 0.6f;
				else if (Progress == spawnDustTreshold + 4)
					dustScale = 0.8f;

				int dustArea = 0;
				if (ShotFromTerraformer) {
					dustScale *= 1.2f;
					dustArea = (int)(12f * dustScale);
				}

				Dust sprayDust = Dust.NewDustDirect(new Vector2(Projectile.position.X - dustArea, Projectile.position.Y - dustArea), Projectile.width + dustArea * 2, Projectile.height + dustArea * 2, dustType, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100);
				sprayDust.noGravity = true;
				sprayDust.scale *= 1.75f * dustScale;
			}

			Progress++;
			Projectile.rotation += 0.3f * Projectile.direction;
		}
	}
}