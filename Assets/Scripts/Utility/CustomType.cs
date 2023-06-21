using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CustomType
{
    public static Dictionary<byte, object> SerializeBattleCode(Dictionary<BattleCodes, object> battleCode) {
        return battleCode.ToDictionary(keyValue => (byte) keyValue.Key, keyValue => keyValue.Value);
    }
    
    public static Dictionary<BattleCodes, object> DeserializeBattleCode(Dictionary<byte, object> data) {
        return data.ToDictionary(keyValue => (BattleCodes) keyValue.Key, keyValue => keyValue.Value);
    }
}
