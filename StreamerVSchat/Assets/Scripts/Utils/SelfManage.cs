using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfManage : MonoBehaviour
{
    public void SelfDeactive()
    {
        this.gameObject.SetActive(false);
    }

    public void SelfDestroy()
    {
        Destroy(this.gameObject);
    }
}
