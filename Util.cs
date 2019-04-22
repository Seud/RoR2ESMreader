using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ESMReader
{
    class Util
    {
        public static EOAState NextEOAState(EOAState eoaState, int bytei)
        {
            switch (eoaState)
            {
                case EOAState.Viewed0: return (bytei == 2) ? EOAState.Viewed1 : EOAState.Viewed0;
                case EOAState.Viewed1: return (bytei == 2) ? EOAState.Viewed2 : EOAState.Viewed0;
                case EOAState.Viewed2:
                    return (bytei == 4) ? EOAState.EndOfAttribute : 
                        (bytei == 2) ? EOAState.Viewed2 : EOAState.Viewed0;
                default:
                    Logger.Warn("In EOAState, did not use a standard path");
                    return EOAState.Viewed0;
            }
        }

        public static void AddValue(JObject esAttributes, string esaName, ESAType esaType, List<byte[]> esaBytes)
        {
            int length = esaBytes.Count;
            
            switch (esaType)
            {
                case ESAType.Int:
                    if (length == 0)
                        esAttributes[esaName] = 0;
                    else
                    {
                        int bytei = BitConverter.ToInt32(esaBytes[0], 0);
                        if (bytei != 0)
                            esAttributes[esaName] = bytei;
                        else
                        {
                            esaBytes.RemoveAt(0);
                            AddValue(esAttributes, esaName, esaType, esaBytes);
                        }
                    }
                    break;
                case ESAType.Float:
                    if (length == 0)
                        esAttributes[esaName] = 0;
                    else
                    {
                        float bytef = BitConverter.ToSingle(esaBytes[0], 0);
                        if (bytef != 0)
                            esAttributes[esaName] = bytef;
                        else
                        {
                            esaBytes.RemoveAt(0);
                            AddValue(esAttributes, esaName, esaType, esaBytes);
                        }
                    }
                    break;
                case ESAType.String:
                    if (length == 0)
                        esAttributes[esaName] = "";
                    else
                    {
                        if (esaName == "muzzleString")
                            ;
                        int bytei = BitConverter.ToInt32(esaBytes[0], 0);
                        if (bytei != 0) {
                            StringReader sr = new StringReader(bytei);
                            bool flag = false;
                            for(int i = 1; !flag && i < length; i++)
                                flag = sr.Add(esaBytes[i]);
                            esAttributes[esaName] = sr.Read();
                        }
                        else
                        {
                            esaBytes.RemoveAt(0);
                            AddValue(esAttributes, esaName, esaType, esaBytes);
                        }
                    }
                    break;
                case ESAType.UnityObject:
                    if (length == 0)
                        esAttributes[esaName] = "Unknown UnityObject";
                    else
                    {
                        int bytei = BitConverter.ToInt32(esaBytes[0], 0);
                        if (bytei != 0)
                            esAttributes[esaName] = "UnityObject (PathID : " + bytei + ")";
                        else
                        {
                            esaBytes.RemoveAt(0);
                            AddValue(esAttributes, esaName, esaType, esaBytes);
                        }
                    }
                    break;
                case ESAType.Boolean:
                    if (length == 0)
                        esAttributes[esaName] = false;
                    else
                    {
                        bool byteb = BitConverter.ToBoolean(esaBytes[0], 0);
                        if (byteb)
                            esAttributes[esaName] = true;
                        else
                        {
                            esaBytes.RemoveAt(0);
                            AddValue(esAttributes, esaName, esaType, esaBytes);
                        }
                    }
                    break;
                case ESAType.AnimationCurve:
                    esAttributes[esaName] = "AnimationCurve";
                    break;
            }
        }
    }
}
