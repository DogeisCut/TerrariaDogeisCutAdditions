using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System.Collections.Generic;

namespace DogeisCutAdditions.Content.Projectiles
{
	public class PolyTankMinionBullet : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 1;
		}

		public sealed override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
            Projectile.aiStyle = ProjAIStyleID.Arrow; 

			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = 1; 
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;

            AIType = ProjectileID.Bullet;
		}

		public override bool? CanCutTiles() {
			return true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindProjectiles.Add(index);
		}

        public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
}