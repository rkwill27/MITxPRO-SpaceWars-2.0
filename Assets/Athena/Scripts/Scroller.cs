using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] public RawImage _img;
    [SerializeField] public float _x, _y;
    [SerializeField] public float scrollSpeed = 1f; // Speed multiplier

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = new Vector2(_x, _y) * scrollSpeed * Time.deltaTime;
        _img.uvRect = new Rect(_img.uvRect.position + offset, _img.uvRect.size);
    }
}
