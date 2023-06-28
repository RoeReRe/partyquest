using UnityEngine;
using UnityEngine.VFX;

public abstract class Skill : ScriptableObject
{
    [Header("Basic Information")]
    public string skillName;
    public SkillRank skillRank;
    public Sprite skillIcon;
    public int CD;

    [TextArea(5, 10)]
    public string skillDesc;

    [Header("Animations")]
    public ParticleSystem senderPS;
    public VisualEffect senderVFX;
    public ParticleSystem receiverPS;
    public VisualEffect receiverVFX;
    
    public abstract void SenderAction(GameObject sender);
    public abstract void ReceiverAction(GameObject receiver);
}

public enum SkillRank : byte {
    Common,
    Unique,
    Ultimate,
    Divine
}

public enum DamageAmount : byte {
    NONE,
    Light,
    Medium,
    Heavy,
    Severe,
    Colossal
}
