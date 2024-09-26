using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoving : MonoBehaviour
{
    private Rigidbody2D rb2D;
    public float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = transform.position.x + Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        float vertical = transform.position.y + Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        rb2D.MovePosition(new Vector2(horizontal, vertical));
    }
}
