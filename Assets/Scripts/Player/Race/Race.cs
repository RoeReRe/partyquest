using UnityEngine;
public abstract class Race : ScriptableObject
{
    // Noun
    public abstract string nounName { get; }
    public abstract string adjectiveName { get; }
    
    // Stats
    public abstract int vitality { get; }
    public abstract int mind { get; }
    public abstract int strength { get; }
    public abstract int intelligence { get; }
    public abstract int dexterity { get; }
    public abstract int endurance { get; }
}
