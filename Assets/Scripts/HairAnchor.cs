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
        Vector2 windStrength = new Vector2(windStrengthX, windStrengthY);
        Vector2 windVelocity = windStrength * maxWindVelocity;
        float windMag = Mathf.Clamp(windStrength.magnitude, 0f, 1.0f);
        Vector2 characterSpeed = character.GetComponent<Rigidbody2D>().linearVelocity;

        foreach (Transform hairSegment in hairSegments)
        {
            Vector2 positionOffset = Vector2.Lerp(
                hairSegmentOffset,
                new Vector2(Mathf.Abs(hairSegmentOffset.x), Mathf.Abs(hairSegmentOffset.y)),
                windMag
            );
        }
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
            sr.sortingOrder = -i - 1;
            sr.color = new Color(182f / 255f, 61f / 255f, 67f / 255f);

            hairSegment.transform.localPosition = previousHairPosition + hairSegmentOffset;
            previousHairPosition = hairSegment.transform.localPosition;

            float scalePercent = (float)i / numberOfSegments;
            hairSegment.transform.localScale = Vector2.Lerp(
                Vector2.one * hairSizeMax,
                Vector2.one * hairSizeMin,
                scalePercent
            );
            skipNextPhysicsUpdate = true;

            hairSegments[i] = hairSegment.transform;
        }
    }

    private float pixelsToUnits(float pixels)
    {
        return pixels / 16f;
    }
}
