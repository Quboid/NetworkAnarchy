using ColossalFramework.UI;
using NetworkAnarchy.Localization;
using QCommonLib;
using System;
using UnityEngine;

namespace NetworkAnarchy
{
    internal class PathButtons
    {
        UIButton ShipPath, AirplanePath;

        internal void Destroy()
        {
            try
            {
                ShipPath?.parent.RemoveUIComponent(ShipPath);
                ShipPath = null;

                AirplanePath?.parent.RemoveUIComponent(AirplanePath);
                AirplanePath = null;
            }
            catch (Exception e)
            {
                Debug.Log($"Destroying pathing buttons failed!\n{e}");
            }
        }

        internal void CreateShipPathButton()
        {
            //CreatePathButton(ShipPath, "Ship", "PublicTransportShipPanel", "Cargo Harbor", Str.buttons_shipPath);
        }

        internal void CreateAirplanePathButton()
        {
            //CreatePathButton(AirplanePath, "Airplane", "PublicTransportPlanePanel", "Airport", Str.buttons_airplanePath);
        }

        internal void CreatePathButton(UIButton button, string prefixName, string panelName, string egName, string toolTip)
        {
            if (!NetworkAnarchy.InGame()) return;
            string prefabName = prefixName + " Path";

            try
            {
                UIPanel panel = UIView.GetAView().FindUIComponent<UIPanel>(panelName);

                foreach (UIButton b in panel.GetComponentsInChildren<UIButton>())
                {
                    if (b.name == prefabName)
                    {
                        Debug.Log($"NetworkAnarchy: {prefabName} button already exists!");
                        return;
                    }
                }

                UIScrollablePanel scrollablePanel = (UIScrollablePanel)panel.components[1];

                UIButton example = null;
                foreach (UIButton b in panel.GetComponentsInChildren<UIButton>())
                {
                    if (b.name == egName)
                    {
                        example = b;
                        break;
                    }
                }

                if (example != null)
                {
                    button = scrollablePanel.AddUIComponent<UIButton>();
                    button.name = prefabName;
                    button.size = example.size;
                    button.atlas = QTextures.GetAtlas("InMapEditor");
                    button.normalFgSprite = $"Thumbnail{prefixName}Path";
                    button.focusedFgSprite = $"Thumbnail{prefixName}PathFocused";
                    button.hoveredFgSprite = $"Thumbnail{prefixName}PathHovered";
                    button.pressedFgSprite = $"Thumbnail{prefixName}PathPressed";
                    button.tooltip = toolTip;

                    button.eventClicked += OpenPathTool;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Creating {prefabName} button failed!\n{e}");
            }
        }

        private void OpenPathTool(UIComponent component, UIMouseEventParameter eventParam)
        {
            ToolController toolController = GameObject.FindObjectOfType<ToolController>();
            NetTool netTool = ToolsModifierControl.GetTool<NetTool>();
            toolController.CurrentTool = netTool;
            netTool.Prefab = GetPathPrefab(component.name);
        }

        private NetInfo GetPathPrefab(string name)
        {
            return PrefabCollection<NetInfo>.FindLoaded(name);
        }
    }
}
