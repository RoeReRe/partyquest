using UnityEngine;

[CreateAssetMenu(menuName = "Race/Human")]
public class Human : Race
{
    public override string nounName => "Human";

    public override string adjectiveName => "Human";

    public override int vitality => 12;

    public override int mind => 7;

    public override int strength => 11;

    public override int intelligence => 8;

    public override int dexterity => 9;

    public override int endurance => 7;
}
