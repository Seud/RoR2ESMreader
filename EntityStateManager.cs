using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ESMReader
{
    class EntityStateManager
    {
        // Current state of the Reader
        private ReaderState rs = ReaderState.MagicBytesCheck;

        // Remaining Magic Bytes to find
        private int remainingMagicBytes = 2;

        // Asset ID of ther ESM
        private int esmAssetID = 0;

        // Name of the ESM
        private String esmName;

        // Entity States
        private JObject entityStates;

        // Remaining Entity States in current Entity State Manager
        private int remainingESs;

        // Name of the current Entity State
        private String esName;

        // Current Entity State Attributes
        private JObject esAttributes;

        // Remaining attributes in current Entity State
        private int remainingAttributes;

        // Name of the current Entity State Attribute
        private String esaName;

        // Type of the current Entity State Attribute
        private ESAType esaType;

        // Bytes of the current Entity State Attribute
        private List<byte[]> esaBytes;

        // Current EOA state of the Entity State Attribute
        private EOAState eoaState;

        // Current stream
        private StringReader sr;

        public void Read(byte[] bytes)
        {
            int bytei = BitConverter.ToInt32(bytes, 0);
            bool flag;

            switch(rs)
            {
                // Checking for Magic bytes at the start of the file
                // There are two magic bytes 0x00000001. After them, the file starts.
                case ReaderState.MagicBytesCheck:
                    switch(bytei)
                    {
                        case 0: break;
                        case 1:
                            remainingMagicBytes--;
                            break;
                        default: Logger.Error("Wrong magic byte : " + bytei + ", needed " + remainingMagicBytes + " more");
                            break;
                    }
                    if (remainingMagicBytes == 0) rs = ReaderState.AssetID;
                    break;

                // Reading the Unity Asset ID of the ESM
                case ReaderState.AssetID:
                    esmAssetID = bytei;
                    rs = ReaderState.ESMNameL;
                    break;

                // Reading the length of the ESM name
                case ReaderState.ESMNameL:
                    if (bytei == 0) break;
                    sr = new StringReader(bytei);
                    rs = ReaderState.ESMName;
                    break;

                // Reading the ESM Name
                case ReaderState.ESMName:
                    flag = sr.Add(bytes);
                    if (flag)
                    {
                        esmName = sr.Read();
                        rs = ReaderState.ESCount;
                    }
                    break;

                // Reading the amount of Entity States
                case ReaderState.ESCount:
                    entityStates = new JObject();
                    remainingESs = bytei;
                    rs = ReaderState.ESNameL;
                    break;

                // Reading the next Entity State name length
                case ReaderState.ESNameL:
                    sr = new StringReader(bytei);
                    rs = ReaderState.ESName;
                    break;

                // Reading the next Entity State name
                case ReaderState.ESName:
                    flag = sr.Add(bytes);
                    if (flag)
                    {
                        esName = sr.Read();
                        rs = ReaderState.ESACount;
                    }
                    break;

                // Reading the next Entity State Attribute count
                case ReaderState.ESACount:
                    esAttributes = new JObject();
                    remainingAttributes = bytei;
                    if (remainingAttributes == 0)
                        rs = ReaderState.ESNameL;
                    else
                        rs = ReaderState.ESANameL;
                    break;

                // Reading the next Entity State Attribute name length
                case ReaderState.ESANameL:
                    sr = new StringReader(bytei);
                    rs = ReaderState.ESAName;
                    break;

                // Reading the next Entity State Attribute name
                case ReaderState.ESAName:
                    flag = sr.Add(bytes);
                    if (flag)
                    {
                        esaName = sr.Read();
                        rs = ReaderState.ESAType;
                    }
                    break;

                // Reading the next Entity State Attribute type
                case ReaderState.ESAType:
                    switch(bytei)
                    {
                        case 1: esaType = ESAType.Int; break;
                        case 2: esaType = ESAType.Float; break;
                        case 3: esaType = ESAType.String; break;
                        case 4: esaType = ESAType.UnityObject; break;
                        case 5: esaType = ESAType.Boolean; break;
                        case 6: esaType = ESAType.AnimationCurve; break;
                        default: Logger.Error("Unknown type found : " + bytei); break;
                    }
                    esaBytes = new List<byte[]>();
                    eoaState = EOAState.Viewed0;
                    rs = ReaderState.ESAValue;
                    break;

                // Reading the next Entity State Attribute value
                case ReaderState.ESAValue:
                    esaBytes.Add(bytes);
                    eoaState = Util.NextEOAState(eoaState, bytei);
                    if(eoaState == EOAState.EndOfAttribute)
                    {
                        // Remove EOA bytes
                        esaBytes.RemoveRange(esaBytes.Count - 3, 3);
                        Util.AddValue(esAttributes, esaName, esaType, esaBytes);
                        remainingAttributes--;
                        if(remainingAttributes > 0)
                        {
                            rs = ReaderState.ESANameL;
                        } else
                        {
                            entityStates[esName] = esAttributes;
                            remainingESs--;
                            rs = ReaderState.ESNameL;
                        }
                    }
                    break;
            }
        }

        public void Output(String fileName)
        {
            Logger.Log("Successfully decoded " + esmName + "(UnityObject PathID " + esmAssetID + ")");
            String json_raw = entityStates.ToString();
            System.IO.File.WriteAllText(fileName + ".json", json_raw);
        }
    }
}
