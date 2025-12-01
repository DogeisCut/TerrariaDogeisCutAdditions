using DogeisCutAdditions.Content.Buffs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DogeisCutAdditions.Content.Projectiles.Minions
{
	//TODO: Aim prediction (projectile tradjectory, and predicted enemy trajectory).
	public class PolyTankMinion : ModProjectile
	{
        private float orbitAngle;
        private int shootingCooldownTimer;
        private bool isPlayingShootingAnimation;
        private int shootingAnimationTimer;

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			Main.projPet[Type] = true; 

			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		public sealed override void SetDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.tileCollide = false;

			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
		}

		public override bool? CanCutTiles() {
			return false;
		}

		public override bool MinionContactDamage() {
			return true;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];

			if (!CheckActive(owner)) {
				return;
			}

			GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
			SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
			Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
			Visuals();
		}

		private bool CheckActive(Player owner) {
			if (owner.dead || !owner.active) {
				owner.ClearBuff(ModContent.BuffType<PolyTankBuff>());

				return false;
			}

			if (owner.HasBuff(ModContent.BuffType<PolyTankBuff>())) {
				Projectile.timeLeft = 2;
			}

			return true;
		}

		private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition) {
			Vector2 idlePosition = owner.Center;
			idlePosition.Y -= 48f;

			float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -owner.direction;
			idlePosition.X += minionPositionOffsetX;

			vectorToIdlePosition = idlePosition - Projectile.Center;
			distanceToIdlePosition = vectorToIdlePosition.Length();

			if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f) {
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			float overlapVelocity = 0.04f;

			foreach (var other in Main.ActiveProjectiles) {
				if (other.whoAmI != Projectile.whoAmI && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) {
						Projectile.velocity.X -= overlapVelocity;
					}
					else {
						Projectile.velocity.X += overlapVelocity;
					}

					if (Projectile.position.Y < other.position.Y) {
						Projectile.velocity.Y -= overlapVelocity;
					}
					else {
						Projectile.velocity.Y += overlapVelocity;
					}
				}
			}
		}

		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter) {
			distanceFromTarget = 700f;
			targetCenter = Projectile.position;
			foundTarget = false;

			if (owner.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);

				if (between < 2000f) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
					foundTarget = true;
				}
			}

			if (!foundTarget) {
				foreach (var npc in Main.ActiveNPCs) {
					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						bool closeThroughWall = between < 10f;

						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}

			Projectile.friendly = foundTarget;
		}

		private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition) {
            if (foundTarget) {
                float desiredOrbitDistance = 120f;
                float orbitAngularVelocity = 0.05f;
                orbitAngle += orbitAngularVelocity;

                Vector2 orbitOffset = new Vector2(
                    (float)Math.Cos(orbitAngle),
                    (float)Math.Sin(orbitAngle)
                ) * desiredOrbitDistance;

                Vector2 desiredPosition = targetCenter + orbitOffset;
                Vector2 movementDirection = desiredPosition - Projectile.Center;

                float movementSpeed = 10f;
                float movementInertia = 30f;

                if (movementDirection != Vector2.Zero) {
                    movementDirection = Vector2.Normalize(movementDirection) * movementSpeed;
                    Projectile.velocity = (Projectile.velocity * (movementInertia - 1f) + movementDirection) / movementInertia;
                }

                FaceTargetDirection(targetCenter);
                HandleShooting(targetCenter);
            } else {
                float movementSpeed = distanceToIdlePosition > 600f ? 12f : 4f;
                float movementInertia = distanceToIdlePosition > 600f ? 60f : 80f;

                if (distanceToIdlePosition > 20f) {
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= movementSpeed;
                    Projectile.velocity = (Projectile.velocity * (movementInertia - 1) + vectorToIdlePosition) / movementInertia;
                } else if (Projectile.velocity == Vector2.Zero) {
                    Projectile.velocity = new Vector2(-0.15f, -0.05f);
                }

                float targetIdleRotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
				float rotationLerpSpeed = 0.15f;
				float wrappedIdle = MathHelper.WrapAngle(targetIdleRotation - Projectile.rotation);
				Projectile.rotation += wrappedIdle * rotationLerpSpeed;
            }

			float maximumAllowedSpeed = 4f;

			if (Projectile.velocity.Length() > maximumAllowedSpeed) {
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maximumAllowedSpeed;
			}

        }

        private void FaceTargetDirection(Vector2 desiredTargetCenter) {
			Vector2 desiredDirection = desiredTargetCenter - Projectile.Center;
			if (desiredDirection != Vector2.Zero) {
				float desiredRotation = (float)Math.Atan2(desiredDirection.Y, desiredDirection.X);
				float rotationLerpSpeed = 0.15f;
				float wrapped = MathHelper.WrapAngle(desiredRotation - Projectile.rotation);
				Projectile.rotation += wrapped * rotationLerpSpeed;
			}
		}

        private void HandleShooting(Vector2 targetCenter) {
            shootingCooldownTimer++;
            if (shootingCooldownTimer >= 25) {
                shootingCooldownTimer = 0;
                Vector2 directionToTarget = targetCenter - Projectile.Center;
                if (directionToTarget != Vector2.Zero) {
                    directionToTarget.Normalize();

					Vector2 muzzleOffset = directionToTarget * 14f;
					Vector2 spawnPosition = Projectile.Center + muzzleOffset + Projectile.velocity;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPosition,
                        directionToTarget * 6f,
                        ModContent.ProjectileType<PolyTankMinionBullet>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );

					Projectile.velocity -= directionToTarget * 1.5f;

                    isPlayingShootingAnimation = true;
                    shootingAnimationTimer = 0;
                }
            }
        }

		private void Visuals() {
            if (isPlayingShootingAnimation) {
                if (shootingAnimationTimer == 0*4) {
                    Projectile.frame = 0;
                } else if (shootingAnimationTimer == 1*4) {
                    Projectile.frame = 1;
                } else if (shootingAnimationTimer == 2*4) {
                    Projectile.frame = 2;
                } else if (shootingAnimationTimer == 3*4) {
                    Projectile.frame = 1;
                } else if (shootingAnimationTimer == 4*4) {
                    Projectile.frame = 0;
                    isPlayingShootingAnimation = false;
                }
				shootingAnimationTimer++;
            } else {
                Projectile.frame = 0;
            }
        }
    }
}