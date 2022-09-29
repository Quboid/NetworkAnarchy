using ColossalFramework.UI;
using HarmonyLib;
using QCommonLib;
using System.Linq;

namespace NetworkAnarchy
{
    public class ChirperManager
    {
        static UITextureAtlas normalAtlas = null;
        static UITextureAtlas anarchyAtlas = null;
        static UIButton chirperButton = null;
        static string hoveredSprite, normalSprite, pressedSprite;

        public static void Initialise()
        {
            normalAtlas = NetworkAnarchy.GetAtlas("ChirperAtlas");
            anarchyAtlas = GetChirperAtlas();
            chirperButton = UIView.GetAView().FindUIComponent<UIButton>("Zone");

            if (normalAtlas == null) UnityEngine.Debug.Log($"NetworkAnarchy: Failed to load Chirper normal atlas");
            if (anarchyAtlas == null) UnityEngine.Debug.Log($"NetworkAnarchy: Failed to load Chirper anarchy atlas");
            if (chirperButton == null) UnityEngine.Debug.Log($"NetworkAnarchy: Failed to find Chirper button");
            if (normalAtlas == null || anarchyAtlas == null || chirperButton == null) return;

            hoveredSprite = chirperButton.hoveredBgSprite;
            normalSprite = chirperButton.normalBgSprite;
            pressedSprite = chirperButton.pressedBgSprite;
        }

        internal static void UpdateAtlas()
        {
            if (normalAtlas == null) return;
            if (anarchyAtlas == null) return;
            if (chirperButton == null) return;

            if (NetworkAnarchy.Anarchy)
            {
                chirperButton.atlas = anarchyAtlas;
                if (!anarchyAtlas.spriteNames.Contains(chirperButton.normalBgSprite))
                {
                    chirperButton.hoveredBgSprite = "ChirperHovered";
                    chirperButton.normalBgSprite = "ChirperIcon";
                    chirperButton.pressedBgSprite = "ChirperPressed";
                }
            }
            else
            {
                chirperButton.atlas = normalAtlas;
                chirperButton.hoveredBgSprite = hoveredSprite;
                chirperButton.normalBgSprite = normalSprite;
                chirperButton.pressedBgSprite = pressedSprite;
            }
        }

        public static UITextureAtlas GetChirperAtlas()
        {
            string[] spriteNames = new string[]
            {
                "Chirper",
                "ChirperChristmas",
                "ChirperChristmasDisabled",
                "ChirperChristmasFocused",
                "ChirperChristmasHovered",
                "ChirperChristmasPressed",
                "ChirperConcerts",
                "ChirperConcertsDisabled",
                "ChirperConcertsFocused",
                "ChirperConcertsHovered",
                "ChirperConcertsPressed",
                "Chirpercrown",
                "ChirpercrownDisabled",
                "ChirpercrownFocused",
                "ChirpercrownHovered",
                "ChirpercrownPressed",
                "ChirperDeluxe",
                "ChirperDeluxeDisabled",
                "ChirperDeluxeFocused",
                "ChirperDeluxeHovered",
                "ChirperDeluxePressed",
                "ChirperDisabled",
                "ChirperDisastersHazmat",
                "ChirperDisastersHazmatDisabled",
                "ChirperDisastersHazmatFocused",
                "ChirperDisastersHazmatHovered",
                "ChirperDisastersHazmatPressed",
                "ChirperDisastersPilot",
                "ChirperDisastersPilotDisabled",
                "ChirperDisastersPilotFocused",
                "ChirperDisastersPilotHovered",
                "ChirperDisastersPilotPressed",
                "ChirperDisastersWorker",
                "ChirperDisastersWorkerDisabled",
                "ChirperDisastersWorkerFocused",
                "ChirperDisastersWorkerHovered",
                "ChirperDisastersWorkerPressed",
                "ChirperFocused",
                "ChirperFootball",
                "ChirperFootballDisabled",
                "ChirperFootballFocused",
                "ChirperFootballHovered",
                "ChirperFootballPressed",
                "ChirperHovered",
                "ChirperIcon",
                "ChirperJesterhat",
                "ChirperJesterhatDisabled",
                "ChirperJesterhatFocused",
                "ChirperJesterhatHovered",
                "ChirperJesterhatPressed",
                "ChirperLumberjack",
                "ChirperLumberjackDisabled",
                "ChirperLumberjackFocused",
                "ChirperLumberjackHovered",
                "ChirperLumberjackPressed",
                "ChirperParkRanger",
                "ChirperParkRangerDisabled",
                "ChirperParkRangerFocused",
                "ChirperParkRangerHovered",
                "ChirperParkRangerPressed",
                "ChirperPressed",
                "ChirperRally",
                "ChirperRallyDisabled",
                "ChirperRallyFocused",
                "ChirperRallyHovered",
                "ChirperRallyPressed",
                "ChirperRudolph",
                "ChirperRudolphDisabled",
                "ChirperRudolphFocused",
                "ChirperRudolphHovered",
                "ChirperRudolphPressed",
                "ChirperSouvenirGlasses",
                "ChirperSouvenirGlassesDisabled",
                "ChirperSouvenirGlassesFocused",
                "ChirperSouvenirGlassesHovered",
                "ChirperSouvenirGlassesPressed",
                "ChirperSurvivingMars",
                "ChirperSurvivingMarsDisabled",
                "ChirperSurvivingMarsFocused",
                "ChirperSurvivingMarsHovered",
                "ChirperSurvivingMarsPressed",
                "ChirperTrafficCone",
                "ChirperTrafficConeDisabled",
                "ChirperTrafficConeFocused",
                "ChirperTrafficConeHovered",
                "ChirperTrafficConePressed",
                "ChirperTrainConductor",
                "ChirperTrainConductorDisabled",
                "ChirperTrainConductorFocused",
                "ChirperTrainConductorHovered",
                "ChirperTrainConductorPressed",
                "ChirperWintercap",
                "ChirperWintercapDisabled",
                "ChirperWintercapFocused",
                "ChirperWintercapHovered",
                "ChirperWintercapPressed",
                "ChirperZookeeper",
                "ChirperZookeeperDisabled",
                "ChirperZookeeperFocused",
                "ChirperZookeeperHovered",
                "ChirperZookeeperPressed",
                "EmptySprite",
                "ThumbChirperBeanie",
                "ThumbChirperBeanieDisabled",
                "ThumbChirperBeanieFocused",
                "ThumbChirperBeanieHovered",
                "ThumbChirperBeaniePressed",
                "ThumbChirperFlower",
                "ThumbChirperFlowerDisabled",
                "ThumbChirperFlowerFocused",
                "ThumbChirperFlowerHovered",
                "ThumbChirperFlowerPressed",
                "ThumbChirperTech",
                "ThumbChirperTechDisabled",
                "ThumbChirperTechFocused",
                "ThumbChirperTechHovered",
                "ThumbChirperTechPressed"
            };

            return QCommon.CreateTextureAtlas(typeof(ModInfo).Assembly, "ChirperAtlasAnarchy", spriteNames, "NetworkAnarchy.ChirperAtlas.");
        }
    }


    // Update the atlas if the player changes Chirper icon
    [HarmonyPatch(typeof(ChirpOptionsPanel), "OnSelected")]
    class CP_OnSelected
    {
        public static void Postfix()
        {
            ChirperManager.Initialise();
            ChirperManager.UpdateAtlas();
        }
    }
}
