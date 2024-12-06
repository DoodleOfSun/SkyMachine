using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGround : MonoBehaviour
{
    public float scrollSpeed;
    private Renderer backGroundRenderer;


    void Start()
    {
        backGroundRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Mathf.Repeat(Time.time * scrollSpeed, 1);
        Vector2 offset = new Vector2(-x, 0);
        backGroundRenderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
    }
}