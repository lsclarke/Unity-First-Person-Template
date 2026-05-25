using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKnockBackable
{
    void PlayKnockBack(GameObject other);

    IEnumerator StopKnockBack(float time);
}
