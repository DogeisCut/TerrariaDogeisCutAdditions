using DogeisCutAdditions.Content.Items;
using DogeisCutAdditions.Content.Buffs;
using DogeisCutAdditions.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;

namespace DogeisCutAdditions.Content.Projectiles.Minions
{
     // This minion shows a few mandatory things that make it behave properly.
	// Its attack pattern is simple: If an enemy is in range of 43 tiles, it will fly to it and deal contact damage
	// If the player targets a certain NPC with right-click, it will fly through tiles to it
	// If it isn't attacking, it will float near the player with minimal movement
	public class PolyTankMinion : ModProjectile
	{
        private float orbitAngle;
        private int shootingCooldownTimer;
        private bool isPlayingShootingAnimation;
        private int shootingAnimationTimer;

		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Type] = 3;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			Main.projPet[Type] = true; // Denotes that this projectile is a pet or minion

			ProjectileID.Sets.MinionSacrificable[Type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.CultistIsResistantTo[Type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
		}

		public sealed override void SetDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.tileCollide = false; // Makes the minion go through tiles freely

			// These below are needed for a minion weapon
			Projectile.friendly = true; // Only controls if it deals damage to enemies on contact (more on that later)
			Projectile.minion = true; // Declares this as a minion (has many effects)
			Projectile.DamageType = DamageClass.Summon; // Declares the damage type (needed for it to deal damage)
			Projectile.minionSlots = 1f; // Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			Projectile.penetrate = -1; // Needed so the minion doesn't despawn on collision with enemies or tiles
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		// The AI of this minion is split into multiple methods to avoid bloat. This method just passes values between calls actual parts of the AI.
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

		// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
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
			idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
			// The index is projectile.minionPos
			float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -owner.direction;
			idlePosition.X += minionPositionOffsetX; // Go behind the player

			// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

			// Teleport to player if distance is too big
			vectorToIdlePosition = idlePosition - Projectile.Center;
			distanceToIdlePosition = vectorToIdlePosition.Length();

			if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;

			// Fix overlap with other minions
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
			// Starting search distance
			distanceFromTarget = 700f;
			targetCenter = Projectile.position;
			foundTarget = false;

			// This code is required if your minion weapon has the targeting feature
			if (owner.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);

				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 2000f) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
					foundTarget = true;
				}
			}

			if (!foundTarget) {
				// This code is required either way, used for finding a target
				foreach (var npc in Main.ActiveNPCs) {
					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 100f;

						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
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