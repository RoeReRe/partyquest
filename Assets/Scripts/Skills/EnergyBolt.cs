using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Classes/Bishop/Energy Bolt")]
public class EnergyBolt : ActiveSkill
{
    public override void SenderAction(GameObject sender)
    {
        ParticleSystem PS = Instantiate(this.senderPS, sender.transform, false);
        PS.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        PS.Play(true);
    }
    
    public override void ReceiverAction(GameObject receiver)
    {
        ParticleSystem PS = Instantiate(this.receiverPS, receiver.transform, false);
        PS.Play();
    }
}
