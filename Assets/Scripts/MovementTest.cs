using UnityEngine;

public class MovementTest : MonoBehaviour
{
    public bool smoothMovement = true;
    public float moveSpeed = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform transform = GetComponent<Transform>();

        if(smoothMovement)
        {
            float t = 1f - Mathf.Exp(-moveSpeed * Time.deltaTime);
            transform.position = Vector2.Lerp(transform.position, new Vector2(7, 2), t);
        } else
        {
            transform.position = Vector2.Lerp(transform.position, new Vector2(7, 2), moveSpeed * Time.deltaTime);
        }

        Debug.Log(transform.position);
    }
}
