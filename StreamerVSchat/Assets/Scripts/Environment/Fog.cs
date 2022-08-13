using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    public float speedFog = 2f;

    void Update()
    {
        if(this.transform.position.y >= 160)
            this.transform.position = new Vector3(this.transform.position.x, -170f, this.transform.position.z);

        transform.Translate(0f, 1 * (speedFog * Time.deltaTime), 0f);
    }
}
