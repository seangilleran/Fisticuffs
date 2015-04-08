using UnityEngine;
using System.Collections;

[AddComponentMenu( "Animation/Shadow Animation" )]
public class ShadowAnimation : MonoBehaviour
{
    public float FlickerBuffer;

    Transform floor;

    
    void Awake()
    {
        floor = GameObject.FindGameObjectWithTag( "Floor" ).transform;
    }

    void Update()
    {
        var pos = transform.position;
        pos.y = floor.position.y + ( floor.localScale.y / 2f ) + FlickerBuffer;
        transform.position = pos;
    }
}