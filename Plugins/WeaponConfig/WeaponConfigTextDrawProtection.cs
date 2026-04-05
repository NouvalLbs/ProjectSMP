#nullable enable
using SampSharp.GameMode.Display;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static class WeaponConfigTextDrawProtection
    {
        public static bool CanDestroyTextDraw(TextDraw? td) => TextDrawManager.CanDestroy(td);
        public static bool CanDestroyPlayerTextDraw(PlayerTextDraw? ptd, BasePlayer player) => TextDrawManager.CanDestroy(player.Id, ptd);
        public static bool CanModifyTextDraw(TextDraw? td) => TextDrawManager.CanModify(td);
        public static bool CanModifyPlayerTextDraw(PlayerTextDraw? ptd, BasePlayer player) => TextDrawManager.CanModify(player.Id, ptd);
    }
}