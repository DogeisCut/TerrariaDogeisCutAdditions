
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DogeisCutAdditions.Content.Items
{
    public class DeveloperHousePlacer : ModItem
    {

        public override void SetStaticDefaults() {
            ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true;

            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults() {
            Item.damage = 30;
            Item.knockBack = 3f;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Cyan;

            Item.noMelee = true;
        }

        public override bool? UseItem(Player player)
        {
            Point placementCenter = Main.MouseWorld.ToTileCoordinates();

            int leftOffset = placementCenter.X - 0;
            int topOffset = placementCenter.Y - 9;

            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    int tileX = leftOffset + x;
                    int tileY = topOffset + y;

                    bool border = x == 0 || x == 5 || y == 0 || y == 9;

                    if (border)
                    {
                        WorldGen.PlaceTile(tileX, tileY, TileID.Platforms, false, false, -1, 0);
                    }
                }
            }

            WorldGen.KillTile(leftOffset + 3, topOffset + 9, false, false, true);
            WorldGen.KillTile(leftOffset + 4, topOffset + 9, false, false, true);
            WorldGen.PlaceTile(leftOffset + 3, topOffset + 9, TileID.WoodBlock);
            WorldGen.PlaceTile(leftOffset + 4, topOffset + 9, TileID.WoodBlock);

            WorldGen.PlaceTile(leftOffset + 2, topOffset + 8, TileID.Tables);
            WorldGen.PlaceTile(leftOffset + 4, topOffset + 8, TileID.Chairs, false, false, -1, 0);

            for (int x = 0; x < 6; x++)
            {
                int tileX = leftOffset + x;
                int tileY = topOffset + 4;
                WorldGen.PlaceWall(tileX, tileY, WallID.Wood);
            }

            WorldGen.PlaceTile(leftOffset + 2, topOffset + 4, TileID.Torches, false, false, -1, 0);

            return true;
        }
    }
}
