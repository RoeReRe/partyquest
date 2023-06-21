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
    
    public static float polynomial(int x, params float[] coefficients) {
        double ans = 0;
        for (int i = 0; i < coefficients.Length; i++) {
            ans += coefficients[i] * Math.Pow(x, i);
        }
        return (float) ans;
    }
    
    // Outgoing Calculations
    public static int VitalityToHP(int vitality) {
        return (int) Math.Ceiling(polynomial(
            vitality,
            -2.1479f,
            26.321f,
            -2.3659f,
            0.2234f,
            -0.0029f 
        ));
    }

    public static int MindToMP(int mind) {
        return (int) Math.Ceiling(polynomial(
            mind,
            -0.5923f,
            15.39f,
            0.2718f,
            -0.1019f,
            0.0056f
        ));
    }

    public static int StrengthToPhysical(int strength) {
        return (int) Math.Ceiling(polynomial(
            strength,
            -0.01f,
            -1.4977f,
            1.164f,
            -0.0399f,
            0.0004f
        ));
    }

    // Incoming Calculations
    public static int PhysicalToHP(int attack, int vitality, int strength) {
        int vitalityReduction = (int) Math.Ceiling(polynomial(
            vitality,
            -0.2713f,
            4.8422f,
            -0.5369f,
            0.0177f
        ));

        int strengthReduction = (int) Math.Ceiling(polynomial(
            strength,
            0.3447f,
            1.0606f,
            -0.1424f,
            0.007f
        ));

        return Math.Max(attack - vitalityReduction - strengthReduction, 0);
    }
}