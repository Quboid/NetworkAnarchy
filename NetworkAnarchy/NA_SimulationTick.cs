using ICities;
using System;
using UnityEngine;

namespace NetworkAnarchy
{
    public partial class NetworkAnarchy : MonoBehaviour
    {
        public class AfterSimulationTick : ThreadingExtensionBase
        {
            public override void OnAfterSimulationTick()
            {
                if (instance == null || !instance.enabled) return;

                try
                {
                    instance.OnAfterSimulationTick();
                }
                catch (Exception e)
                {
                    Log.Error(e, "[NA23]");
                }
            }
        }

        public virtual void OnAfterSimulationTick()
        {
            if (m_buildingTool == null)
            {
                return;
            }

            // Removes HeightTooHigh & TooShort errors
            if (Anarchy && IsBuildingToolEnabled())
            {
                var errors = (ToolBase.ToolErrors)m_placementErrorsField.GetValue(m_buildingTool);
                if ((errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
                {
                    errors &= ~ToolBase.ToolErrors.HeightTooHigh;
                    m_placementErrorsField.SetValue(m_buildingTool, errors);
                }

                if ((errors & ToolBase.ToolErrors.TooShort) == ToolBase.ToolErrors.TooShort)
                {
                    errors &= ~ToolBase.ToolErrors.TooShort;
                    m_placementErrorsField.SetValue(m_buildingTool, errors);
                }

                if ((errors & ToolBase.ToolErrors.SlopeTooSteep) == ToolBase.ToolErrors.SlopeTooSteep)
                {
                    errors &= ~ToolBase.ToolErrors.SlopeTooSteep;
                    m_placementErrorsField.SetValue(m_buildingTool, errors);
                }
            }

            // Resume fixes
            if (m_fixTunnelsCount != 0)
            {
                FixTunnels();
            }
            if (m_fixNodesCount != 0)
            {
                FixNodes();
            }

            // Stop here if neither active nor bulldozer tool enabled
            if (!IsActive && !IsBulldozeToolEnabled())
            {
                return;
            }

            // Check if segment have been created/deleted/updated
            if (m_segmentCount != NetManager.instance.m_segmentCount || (bool)m_upgradingField.GetValue(m_netTool))
            {
                m_segmentCount = NetManager.instance.m_segmentCount;

                m_fixTunnelsCount = 0;
                m_fixNodesCount = 0;

                FixTunnels();
                FixNodes();
            }

            if (!IsActive)
            {
                return;
            }

            // Fix first control point elevation
            int count = (int)m_controlPointCountField.GetValue(m_netTool);
            if (count != m_controlPointCount && m_controlPointCount == 0 && count == 1)
            {
                if (FixControlPoint(0))
                {
                    m_elevation = Mathf.RoundToInt(Mathf.RoundToInt(m_controlPoints[0].m_elevation / elevationStep) * elevationStep * 256f / 12f);
                    UpdateElevation();

                    Log.Debug($"OnAfterSimulationTick: m_controlPointCountField:{m_controlPointCountField.GetValue(m_netTool)}, m_controlPointCount:{m_controlPointCount}", "[NA71]");
                    m_toolOptionButton?.UpdateButton();
                }
            }
            // Fix last control point elevation
            else if (count == ((m_netTool.m_mode == NetTool.Mode.Curved || m_netTool.m_mode == NetTool.Mode.Freeform) ? 2 : 1))
            {
                FixControlPoint(count);
            }
            m_controlPointCount = count;
        }
    }
}
