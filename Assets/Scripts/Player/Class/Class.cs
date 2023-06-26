using System.Collections.Generic;
using UnityEngine;
public abstract class Class : ScriptableObject
{
    public abstract string nounName { get; }
    public List<Skill> skillList = new List<Skill>();
}
