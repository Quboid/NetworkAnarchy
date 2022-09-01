using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace NetworkAnarchy.Patches
{
    [HarmonyPatch(typeof(NetTool))]
    [HarmonyPatch("CreateNode")]
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
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // NetInfo info, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint, FastList<NetTool.NodePosition> nodeBuffer, int maxSegments, 
            // bool test, bool testEnds, bool visualize, bool autoFix, bool needMoney, bool invert, bool switchDir,
            // ushort relocateBuildingID, out ushort firstNode, out ushort lastNode, out ushort segment, out int cost, out int productionRate

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
