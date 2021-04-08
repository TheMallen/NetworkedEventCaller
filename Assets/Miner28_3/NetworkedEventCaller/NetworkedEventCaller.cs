
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

public class NetworkedEventCaller : UdonSharpBehaviour
{


    #region SyncedEventCaller
    [NonSerialized] [UdonSynced] public string methodCaller = "";

    public override void OnDeserialization()
    {
        _RecieveMethod(methodCaller);
    }

    private object[] parameters;
    private byte paramcounter = 0;
    
    [Tooltip("Recommended maxium is 25")]
    public uint maxParameterCount = 25;
    private void Start()
    {
        parameters = new object[(int)maxParameterCount];
    }

    public void _RecieveMethod(string encodedMethod)
    {
        Debug.Log(encodedMethod);
        string[] split = encodedMethod.Split('Œ');
        if (split.Length < 2) return;

        string target = split[0];
        if (target == "All")
        {
        }
        else if (target == "Master" && !Networking.LocalPlayer.isMaster)
        {
            return;
        }
        else if (target[0] == 'P' && target != $"P{Networking.LocalPlayer.playerId}")
        {
            return;
        }
        else
        {
            Debug.Log($"<color=#ff0000>[ERROR-NetworkedEventCaller]</color> - Invalid target [{target}]");
        }
        string methodName = split[1];
        paramcounter = 0;
        if (split.Length > 2)
        {
            for (int i = 2; i < split.Length; i++)
            {
                string[] split2 = split[i].Split('‰');
                _SetToCorrectType(split2[0], split2[1]);
            }
        }
        SendCustomEvent(methodName);
    }

    //Possible targets: All, Master, Player syntax: P{PlayerId} (P15)
    //Possible parameters: Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, String, VRCPlayerAPI, Boolean (Soft/Recommended-Max/ 25 - [Search for variable "parameters" and increase 25 to your desired number])
    //Example: _SendMethod("All", nameof(TeleportToPlayer), new object[]{VRCPlayerApi.GetPlayerById(500)});
    //Example recieving method:
    //    public void TeleportToPlayer()
    //    {
    //    VRCPlayerApi player = (VRCPlayerApi) parameters[0];
    //    Networking.LocalPlayer.TeleportTo(player.GetPosition(), player.GetRotation());
    //    }
    public void _SendMethod(string target, string methodName, object[] paramsObj)
    {
        string output = $"{target}Œ{methodName}";
        if (paramsObj != null)
        {
            for (int i = 0; i < paramsObj.Length; i++)
            {
                var type = paramsObj[i].GetType().ToString();
                switch (type)
                {
                    case "System.Byte":
                        output += $"ŒB‰{paramsObj[i]}";
                        break;
                    case "System.SByte":
                        output += $"ŒSB‰{paramsObj[i]}";
                        break;
                    case "System.Int32":
                        output += $"ŒI‰{paramsObj[i]}";
                        break;
                    case "System.UInt32":
                        output += $"Œi‰{paramsObj[i]}";
                        break;
                    case "System.Int64":
                        output += $"ŒL‰{paramsObj[i]}";
                        break;
                    case "System.UInt64":
                        output += $"Œl‰{paramsObj[i]}";
                        break;
                    case "System.Int16":
                        output += $"ŒS‰{paramsObj[i]}";
                        break;
                    case "System.UInt16":
                        output += $"Œs‰{paramsObj[i]}";
                        break;
                    case "System.Single":
                        output += $"Œf‰{paramsObj[i]}";
                        break;
                    case "System.Double":
                        output += $"Œd‰{paramsObj[i]}";
                        break;
                    case "System.String":
                        output += $"Œstr‰{paramsObj[i]}";
                        break;
                    case "System.Boolean":
                        output += $"Œb‰{((bool) paramsObj[i] ? 'T' : 'F')}";
                        break;
                    case "VRC.SDKBase.VRCPlayerApi":
                    {
                        VRCPlayerApi player = (VRCPlayerApi) paramsObj[i];
                        output += $"ŒP‰{player.playerId}";
                        break;
                    }
                    case "System.Byte[]":
                    {
                        output += "ŒBA‰";
                        byte[] param = (byte[]) paramsObj[i];
                        if (param.Length != 0)
                        {
                            foreach (var b in param)
                            {
                                output += $"{b}";
                            }
                            output = output.Remove(output.Length - 1);
                        }

                        break;
                    }
                    case "System.SByte[]":
                    {
                        output += "ŒSBA‰";
                        sbyte[] param = (sbyte[]) paramsObj[i];
                        if (param.Length != 0)
                        {
                            foreach (var b in param)
                            {
                                output += $"{b}";
                            }
                            output = output.Remove(output.Length - 1);
                        }

                        break;
                    }
                    case "System.Int32[]":
                    {
                        output += "ŒIA‰";
                        int[] param = (int[]) paramsObj[i];
                        if (param.Length != 0)
                        {
                            foreach (var b in param)
                            {
                                output += $"{b}";
                            }
                            output = output.Remove(output.Length - 1);
                        }

                        break;
                    }
                    case "System.UInt32[]":
                    {
                        output += "ŒiA‰";
                        uint[] param = (uint[]) paramsObj[i];
                        if (param.Length != 0)
                        {
                            foreach (var b in param)
                            {
                                output += $"{b}";
                            }
                            output = output.Remove(output.Length - 1);
                        }

                        break;
                    }
                }
            }
        }

        if (target == "Local")
        {
            _RecieveMethod(output);
            return;
        }
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        methodCaller = output;
        RequestSerialization();
        if (target != "Others")
        {
            _RecieveMethod(output);
        }
    }

    public void _SetToCorrectType(string type, string param)
    {
        string[] split;
        switch (type)
        {
            case "B":
                parameters[paramcounter] = Convert.ToByte(param);
                break;
            case "SB":
                parameters[paramcounter] = Convert.ToSByte(param);
                break;
            case "I":
                parameters[paramcounter] = Convert.ToInt32(param);
                break;
            case "i":
                parameters[paramcounter] = Convert.ToUInt32(param);
                break;
            case "L":
                parameters[paramcounter] = Convert.ToInt64(param);
                break;
            case "l":
                parameters[paramcounter] = Convert.ToUInt64(param);
                break;
            case "S":
                parameters[paramcounter] = Convert.ToInt16(param);
                break;
            case "s":
                parameters[paramcounter] = Convert.ToUInt16(param);
                break;
            case "f":
                parameters[paramcounter] = Convert.ToSingle(param);
                break;
            case "d":
                parameters[paramcounter] = Convert.ToDouble(param);
                break;
            case "str":
                parameters[paramcounter] = param;
                break;
            case "b":
                parameters[paramcounter] = param == "T";
                break;
            case "P":
                parameters[paramcounter] = VRCPlayerApi.GetPlayerById(Convert.ToInt32(param));
                break;
            case "BA":
                split = param.Split('');
                byte[] outBytes = new byte[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    outBytes[i] = Convert.ToByte(split[i]);
                }

                parameters[paramcounter] = outBytes;
                break;
            case "SBA":
                split = param.Split('');
                sbyte[] outSBytes = new sbyte[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    outSBytes[i] = Convert.ToSByte(split[i]);
                }

                parameters[paramcounter] = outSBytes;
                break;
            case "IA":
                split = param.Split('');
                int[] outInt = new int[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    outInt[i] = Convert.ToInt32(split[i]);
                }

                parameters[paramcounter] = outInt;
                break;
            case "iA":
                split = param.Split('');
                uint[] outUInt = new uint[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    outUInt[i] = Convert.ToUInt32(split[i]);
                }

                parameters[paramcounter] = outUInt;
                break;
        }
        paramcounter++;
    }
    #endregion


}
