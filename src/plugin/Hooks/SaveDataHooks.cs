using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace BeeWorld.Hooks
{
    public class SaveDataHooks
    {
        public static void Init()
        {
            On.MiscWorldSaveData.ctor += MiscWorldSaveData_ctor;
            On.MiscWorldSaveData.FromString += MiscWorldSaveData_FromString;
            On.MiscWorldSaveData.ToString += MiscWorldSaveData_ToString;
        }

        const string DataPrefix = "BeeWorldSaveData_";

        public static ConditionalWeakTable<MiscWorldSaveData, BeeWorldSaveData> SaveData = new();

        private static void MiscWorldSaveData_ctor(On.MiscWorldSaveData.orig_ctor orig, MiscWorldSaveData self, SlugcatStats.Name saveStateNumber)
        {
            orig(self, saveStateNumber);

            if (saveStateNumber.value == "bee")
            {
                SaveData.Add(self, new());
            }
        }

        private static string MiscWorldSaveData_ToString(On.MiscWorldSaveData.orig_ToString orig, MiscWorldSaveData self)
        {

            if (SaveData.TryGetValue(self, out var saveData))
            {
                var saveDataPos = -1;
                for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
                {
                    if (self.unrecognizedSaveStrings[i].StartsWith(DataPrefix))
                    {
                        saveDataPos = i;
                    }
                }

                if (saveDataPos > -1)
                {
                    self.unrecognizedSaveStrings[saveDataPos] = saveData.ToString();
                }
                else
                {
                    self.unrecognizedSaveStrings.Add(saveData.ToString());
                }
            }

            return orig(self);
        }

        private static void MiscWorldSaveData_FromString(On.MiscWorldSaveData.orig_FromString orig, MiscWorldSaveData self, string s)
        {
            orig(self, s);

            if (!SaveData.TryGetValue(self, out var saveData))
            {
                return;
            }

            var saveDataPos = -1;
            for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i].StartsWith(DataPrefix))
                {
                    saveDataPos = i;
                }
            }

            if (saveDataPos > -1)
            {
                saveData.FromString(self.unrecognizedSaveStrings[saveDataPos]);
            }
        }

        public class BeeWorldSaveData
        {
            public bool HasFlowerSI;
            public bool HasFlowerSB;
            public bool HasFlowerOE;

            public void FromString(string text)
            {
                text = text.Substring(DataPrefix.Length);
                var data = text.Split('.');
                for (var i = 0; i < data.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            HasFlowerSI = data[i] == "1";
                            break;
                        case 1:
                            HasFlowerSB = data[i] == "1";
                            break;
                        case 2:
                            HasFlowerOE = data[i] == "1";
                            break;
                    }
                }
            }

            public override string ToString()
            {
                var text = DataPrefix;
                text += HasFlowerSI ? 1 : 0;
                text += ".";
                text += HasFlowerSB ? 1 : 0;
                text += ".";
                text += HasFlowerOE ? 1 : 0;

                return text;
            }

            public bool GetHasFlowerForRegion(string region)
            {
                switch (region)
                {
                    case "SI":
                        return HasFlowerSI;
                    case "SB":
                        return HasFlowerSB;
                    case "OE":
                        return HasFlowerOE;
                    default:
                        return false;
                }
            }

            public void SetHasFlowerForRegion(string region, bool value)
            {
                switch (region)
                {
                    case "SI":
                        HasFlowerSI = value;
                        break;
                    case "SB":
                        HasFlowerSB = value;
                        break;
                    case "OE":
                        HasFlowerOE = value;
                        break;
                }
            }
        }
    }
}
