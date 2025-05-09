﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CameraSystem.Managers;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using HarmonyLib;
using VoiceChat;
using VoiceChat.Networking;

namespace CameraSystem.Patches;
[HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
internal sealed class VoiceChattingPatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

        int index = newInstructions.FindIndex(instruction =>
            instruction.opcode == OpCodes.Callvirt &&
            instruction.operand is MethodInfo method &&
            method.Name == "ValidateReceive");

        index -= 3;

        newInstructions.InsertRange(index, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(VoiceMessage), nameof(VoiceMessage.Speaker))),
            new CodeInstruction(OpCodes.Ldloc_S, 4),
            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VoiceChattingPatch), nameof(ModifyChannel))),
            new CodeInstruction(OpCodes.Stloc_1)
        });

        for (int z = 0; z < newInstructions.Count; z++)
            yield return newInstructions[z];

        ListPool<CodeInstruction>.Pool.Return(newInstructions);
    }


    private static VoiceChatChannel ModifyChannel(ReferenceHub speaker, ReferenceHub receiver, VoiceChatChannel originalChannel)
    {
        try
        {
            if (originalChannel != VoiceChatChannel.ScpChat)
                return originalChannel;

            Player speakerPlayer = Player.Get(speaker);
            Player receiverPlayer = Player.Get(receiver);

            if (CameraManager.Instance.IsWatching(speakerPlayer) || CameraManager.Instance.IsWatching(receiverPlayer))
                return VoiceChatChannel.None;

            return originalChannel;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return originalChannel;
        }
    }
}