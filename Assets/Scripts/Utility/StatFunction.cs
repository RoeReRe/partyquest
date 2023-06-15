using System;

public static class StatFunction
{
    /*
    Vitality -> HP; Phy Def
    Mind -> MP; Mag Def / Status Res
    Strength -> Phy Attk; Phy Def / HP
    Intelligence -> Mag Attk; Mag Def
    Dexterity -> Evasion / Spd; Critical rate
    Endurance -> Status Res / Knockback
    */

    public static Tuple<string, int> calculateChange(string name, int change, int currStat,
        StatusChangeType setOrChangeType, StatusChangeType absOrPercentType) {
        
        int stat = Math.Abs(change);
        int sign = Math.Sign(change);        
        if (setOrChangeType == StatusChangeType.SETSTAT) {
            if (absOrPercentType == StatusChangeType.ABSOLUTE) {
                stat = stat - currStat;
            } else {
                stat = (int) Math.Round(
                    (stat / 100f * currStat) - currStat
                );
            }
        } else {
            if (absOrPercentType == StatusChangeType.ABSOLUTE) {
                stat = stat * sign;
            } else {
                stat = (int) Math.Round(stat / 100f * currStat) * sign;
            }
        }
        return new Tuple<string, int>(name, stat);
    }
    
    public static float logisticFunction(int x, float translationx, float asymptote, float power, float translationy) {
        float a = translationx;
        float c = asymptote;
        float k = power;
        float d = translationy;

        float exponentConst = -k * x;
        return (float) (c / (1 + (a * Math.Exp(exponentConst)))) + d;
    }

    public static int statToHP(int stat) {
        return (int) Math.Ceiling(logisticFunction(
            stat * 20,
            4400f,
            4900f,
            0.016f,
            100f
        ));
    }

    public static int statToMP(int stat) {
        return (int) Math.Ceiling(logisticFunction(
            stat * 20,
            1350f,
            2950f,
            0.016f,
            50f
        ));
    }
}