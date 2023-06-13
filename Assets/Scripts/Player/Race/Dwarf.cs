using UnityEngine;

[CreateAssetMenu(menuName = "Race/Dwarf")]
public class Dwarf : Race
{
    public override string nounName => "Dwarf";

    public override string adjectiveName => "Dwarven";

    public override int vitality => 15;

    public override int mind => 6;

    public override int strength => 13;

    public override int intelligence => 5;

    public override int dexterity => 7;

    public override int endurance => 8;
}
