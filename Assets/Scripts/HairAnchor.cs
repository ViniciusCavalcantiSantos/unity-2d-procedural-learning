using UnityEngine;

public class HairAnchor : MonoBehaviour
{
    public Vector2 partOffset = Vector2.zero;
    public float lerpSpeed = 10f;

    public Transform[] hairParts;

    public Transform hairAnchor;

    void Awake()
    {
       hairAnchor = GetComponent<Transform>();
       hairParts = GetComponentsInChildren<Transform>(); 
    }

    void Update()
    {
        Transform pieceToFollow = hairAnchor;
        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);

        foreach(Transform hairPart in hairParts)
        {
            if(!hairPart.Equals(hairAnchor))
            {
                Vector2 targetPosition = (Vector2) pieceToFollow.position + partOffset;
                Vector2 newPostionLerp = Vector2.Lerp(hairPart.position, targetPosition, t);

                hairPart.position = newPostionLerp;
                pieceToFollow = hairPart;
            }
        }
    }
}
