using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using UnityEngine;

namespace NetworkAnarchy.Patches
{
    [HarmonyPatch(typeof(NetTool), "CheckStartAndEnd")]
    class NT_CheckStartAndEnd
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetTool), "CanAddSegment")]
    class NT_CanAddSegment
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    //bool CanAddNode(ushort segmentID, Vector3 position, Vector3 direction, bool checkDirection, ulong[] collidingSegmentBuffer)
    [HarmonyPatch(typeof(NetTool), "CanAddNode")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(ulong[]) })]
    class NT_CanAddNode
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetTool), "CheckCollidingSegments")]
    class NT_CheckCollidingSegments
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    // ToolBase.ToolErrors CanCreateSegment(NetInfo segmentInfo, ushort startNode, ushort startSegment, ushort endNode, ushort endSegment,
    // ushort upgrading, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, ulong[] collidingSegmentBuffer, bool testEnds)
    [HarmonyPatch(typeof(NetTool), "CanCreateSegment")]
    [HarmonyPatch(new[] { typeof(NetInfo), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(ulong[]), typeof(bool) })]
    class NT_CanCreateSegment
    {
        public static bool Prefix(ref ToolBase.ToolErrors __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = ToolBase.ToolErrors.None;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetTool), "TestNodeBuilding")]
    class NT_TestNodeBuilding
    {
        public static bool Prefix(ref ToolBase.ToolErrors __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = ToolBase.ToolErrors.None;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetTool), "CheckNodeHeights")]
    class NT_CheckNodeHeights
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result &= ~ToolBase.ToolErrors.SlopeTooSteep;
            }
        }
    }

    ////ToolBase.ToolErrors CreateNode(NetInfo info, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint,
    ////  FastList<NetTool.NodePosition> nodeBuffer, int maxSegments, bool test, bool testEnds, bool visualize, bool autoFix, bool needMoney, bool invert,
    ////  bool switchDir, ushort relocateBuildingID, out ushort firstNode, out ushort lastNode, out ushort segment, out int cost, out int productionRate)
    [HarmonyPatch(typeof(NetTool), "CreateNode")]
    [HarmonyPatch(new[] {
        typeof(NetInfo), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(FastList<NetTool.NodePosition>), typeof(int),
        typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool),
        typeof(ushort), typeof(ushort), typeof(ushort), typeof(ushort), typeof(int), typeof(int) },
        new[] {
        ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
        ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
        ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out }
    )]
    class NT_CreateNode
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result &= ~ToolBase.ToolErrors.InvalidShape;
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            //DumpCode(codes);
            int i;
            for (i = 500; i < 700; i++)
            {
                if (codes[i - 3].opcode == OpCodes.Stind_I2 && codes[i - 2].opcode == OpCodes.Ldarg_S && codes[i - 1].opcode == OpCodes.Ldloc_S)
                {
                    break;
                }
            }
            if (i == 700)
            {
                throw new Exception("Failed to find ILCode");
            }
            codes[i] = new CodeInstruction(OpCodes.Call, typeof(NT_CreateNode).GetMethod("GetMaxLength"));
            //DumpCode(codes, "ILCodeAfter.txt");

            return codes;
        }

        public static float GetMaxLength()
        {
            return NetworkAnarchy.instance.MaxSegmentLength + 2;
        }

        private static void DumpCode(IEnumerable<CodeInstruction> instructions, string fileName = "ILCode.txt")
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "Colossal Order\\Cities_Skylines\\" + fileName)))
            {
                foreach (CodeInstruction ci in instructions)
                {
                    outputFile.WriteLine($"{ci.opcode} - {ci.operand}");
                }
            }
        }
    }
}
