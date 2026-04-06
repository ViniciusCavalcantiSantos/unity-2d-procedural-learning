using UnityEngine;

public class HairAnchor : MonoBehaviour
{
    private Transform character;
    private Transform hairAnchor;
    private Transform[] hairSegments;

    public Sprite hairSegmentSprite;

    [Header("Hair Settings")]
    public int numberOfSegments = 5;
    public Vector2 hairSegmentOffset = new Vector2(0.5f, -0.125f);
    public float hairSizeMax = 1f;
    public float hairSizeMin = 0.2f;
    public float maxSegmentDistance = 4f;

    public Vector2 velocityMult = new Vector2(1f, 1.45f);
    public float lerpSpeed = 10f;

    [Header("Wind Settings")]
    [Range(-1f, 1f)]
    public float windStrengthX = 0;
    [Range(-1f, 1f)]
    public float windStrengthY = 0;
    public float maxWindVelocity = 100f;
    public float minWindInfluence = 0.25f;
    private bool skipNextPhysicsUpdate = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        character = transform.parent.parent;
        hairAnchor = transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("Generate Hair Segments")]
    private void GenerateHairSegments()
    {
        Transform hairAnchor = GetComponent<Transform>();
        if (hairAnchor.childCount > 0)
        {
            for (int i = hairAnchor.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(hairAnchor.GetChild(i).gameObject);
            }
        }

        if (hairSegmentSprite == null)
        {
            Debug.LogError("Hair segment sprite is not assigned.");
            return;
        }

        Vector2 previousHairPosition = Vector2.zero;
        for (int i = 0; i < numberOfSegments; i++)
        {
            GameObject hairSegment = new GameObject("HairSegment_" + (i + 1));
            hairSegment.transform.SetParent(hairAnchor);
            SpriteRenderer sr = hairSegment.AddComponent<SpriteRenderer>();
            sr.sprite = hairSegmentSprite;

            hairSegment.transform.localPosition = previousHairPosition + hairSegmentOffset;
            previousHairPosition = hairSegment.transform.localPosition;

            float scalePercent = (float)i / numberOfSegments;
            hairSegment.transform.localScale = Vector2.Lerp(
                Vector2.one * hairSizeMax,
                Vector2.one * hairSizeMin,
                scalePercent
            );

            // hairSegment.transform.localPosition = previousHairPosition + hairSegmentOffset;
            // hairSegment.transform.localRotation = Quaternion.identity;

            // SpriteRenderer sr = hairSegment.AddComponent<SpriteRenderer>();
            // sr.sprite = hairSegmentSprite;
            // float t = (float)i / (numberOfSegments - 1);
            // sr.color = Color.Lerp(Color.black, Color.gray, t);

            // previousHairPosition = hairSegment.transform.localPosition;
        }
    }

    private float pixelsToUnits(float pixels)
    {
        return pixels / 16f;
    }
}
