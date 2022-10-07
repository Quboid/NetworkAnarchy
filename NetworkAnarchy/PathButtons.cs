using ColossalFramework.UI;
using NetworkAnarchy.Localization;
using QCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NetworkAnarchy
{
    internal class PathButtons
    {
        UIButton ShipPath, AirplanePath;

        internal PathButtons()
        {
            try
            {
                PrefabCollection<NetInfo>.FindLoaded("Ship Path").m_availableIn |= ItemClass.Availability.Game;
                PrefabCollection<NetInfo>.FindLoaded("Airplane Path").m_availableIn |= ItemClass.Availability.Game;
                //PrefabCollection<TransportInfo>.FindLoaded("Airplane").m_pathVisibility |= ItemClass.Availability.Game;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to find paths:\n" + e);
            }

            TransportInfo ti = PrefabCollection<TransportInfo>.FindLoaded("Airplane");
            Debug.Log($"BBB {(ti == null ? "<null>" : $"{ti.name} - {ti.m_pathVisibility}")}");
        }

        internal void Destroy()
        {
            try
            {
                ShipPath.parent.RemoveUIComponent(ShipPath);
                ShipPath = null;

                AirplanePath.parent.RemoveUIComponent(AirplanePath);
                AirplanePath = null;
            }
            catch (Exception e)
            {
                Debug.Log($"Destroying pathing buttons failed!\n{e}");
            }
        }

        internal void CreateShipPathButton()
        {
            CreatePathButton(ShipPath, "Ship", "PublicTransportShipPanel", "Cargo Harbor", Str.buttons_shipPath);
        }

        internal void CreateAirplanePathButton()
        {
            CreatePathButton(AirplanePath, "Airplane", "PublicTransportPlanePanel", "Airport", Str.buttons_airplanePath);
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


        //internal void CreateShipPathButton()
        //{
        //    if (NetworkAnarchy.instance.m_inEditor) return;

        //    try
        //    {
        //        UIPanel panel = UIView.GetAView().FindUIComponent<UIPanel>("PublicTransportShipPanel");

        //        foreach (UIButton b in panel.GetComponentsInChildren<UIButton>())
        //        {
        //            if (b.name == "Ship Path")
        //            {
        //                Debug.Log($"NetworkAnarchy: Ship Path button already exists!");
        //                return;
        //            }
        //        }

        //        UIScrollablePanel shipScrollablePanel = (UIScrollablePanel)panel.components[1];

        //        UIButton harbour = null;
        //        foreach (UIButton b in panel.GetComponentsInChildren<UIButton>())
        //        {
        //            if (b.name == "Cargo Harbor")
        //            {
        //                harbour = b;
        //                break;
        //            }
        //        }

        //        if (harbour != null)
        //        {
        //            ShipPath = shipScrollablePanel.AddUIComponent<UIButton>();
        //            ShipPath.name = "Ship Path";
        //            ShipPath.size = harbour.size;
        //            ShipPath.atlas = QTextures.GetAtlas("InMapEditor");
        //            ShipPath.normalFgSprite = "ThumbnailShipPath";
        //            ShipPath.focusedFgSprite = "ThumbnailShipPathFocused";
        //            ShipPath.hoveredFgSprite = "ThumbnailShipPathHovered";
        //            ShipPath.pressedFgSprite = "ThumbnailShipPathPressed";
        //            ShipPath.tooltip = Str.buttons_shipPath;

        //            ShipPath.eventClicked += OpenPathTool;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log($"Creating Ship Path button failed!\n{e}");
        //    }
        //}
        //{
        //    if (NetworkAnarchy.instance.m_inEditor) return;

        //    try
        //    {
        //        UIPanel panel = UIView.GetAView().FindUIComponent<UIPanel>("PublicTransportPlanePanel");

        //        foreach (UIButton b in panel.GetComponentsInChildren<UIButton>())
        //        {
        //            if (b.name == "Airplane Path")
        //            {
        //                Debug.Log($"NetworkAnarchy: Airplane Path button already exists!");
        //                return;
        //            }
        //        }

        //        UIScrollablePanel airplaneScrollablePanel = (UIScrollablePanel)panel.components[1];

        //        UIButton airport = null;
        //        foreach (UIButton b in panel.GetComponentsInChildren<UIButton>())
        //        {
        //            if (b.name == "Airport")
        //            {
        //                airport = b;
        //                break;
        //            }
        //        }

        //        if (airport != null)
        //        {
        //            AirplanePath = airplaneScrollablePanel.AddUIComponent<UIButton>();
        //            AirplanePath.name = "Airplane Path";
        //            AirplanePath.size = airport.size;
        //            AirplanePath.atlas = QTextures.GetAtlas("InMapEditor");
        //            AirplanePath.normalFgSprite = "ThumbnailAirplanePath";
        //            AirplanePath.focusedFgSprite = "ThumbnailAirplanePathFocused";
        //            AirplanePath.hoveredFgSprite = "ThumbnailAirplanePathHovered";
        //            AirplanePath.pressedFgSprite = "ThumbnailAirplanePathPressed";
        //            AirplanePath.tooltip = Str.buttons_airplanePath;

        //            AirplanePath.eventClicked += OpenPathTool;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log($"Creating Airplane Path button failed!\n{e}");
        //    }
        //}
    }
}
