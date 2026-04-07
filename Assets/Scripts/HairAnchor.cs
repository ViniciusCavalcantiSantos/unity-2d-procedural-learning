using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class HairAnchor : MonoBehaviour
{
    private CharacterController characterController;
    private Transform character;
    private Transform hairAnchor;
    private Transform[] hairSegments;

    public Sprite hairSegmentSprite;

    [Header("Hair Settings")]
    public int numberOfSegments = 5;
    public Vector2 hairSegmentOffset = new Vector2(-0.1f, -0.1f);
    public float hairSizeMax = 1f;
    public float hairSizeMin = 0.2f;
    public float maxSegmentDistance = 0.15f;

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
        characterController = character.GetComponent<CharacterController>();
        hairAnchor = transform;
        hairSegments = GetComponentsInChildren<Transform>().Skip(1).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        // Ajusta a rotação do cabelo com base na direção que o personagem está olhando
        hairSegmentOffset.x = Mathf.Abs(hairSegmentOffset.x) * (characterController.isFacingRight ? -1 : 1);

        // Obtém as forças do vento
        Vector2 windStrength = new Vector2(windStrengthX, windStrengthY);
        Vector2 windVelocity = windStrength * maxWindVelocity * -1f;
        float windMag = Mathf.Clamp(windStrength.magnitude, 0f, 1.0f);

        // Obtém a velocidade do personagem
        Vector2 characterVelocity = character.GetComponent<Rigidbody2D>().linearVelocity;

        Transform segmentToFollow = hairAnchor;
        foreach (Transform hairSegment in hairSegments)
        {
            // Cacula a velocidade final influenciada pelo vento e pela velocidade do personagem
            Vector2 characterVelocityInfluence = characterVelocity * velocityMult;
            float characterScale = Mathf.Lerp(1.0f, minWindInfluence, windMag);
            Vector2 finalVelocity = (characterVelocityInfluence * characterScale) + windVelocity;

            // Calcula a posição alvo para o segmento de cabelo, considerando o offset, a influência do vento e a velocidade final
            Vector2 positionOffset = Vector2.Lerp(
                hairSegmentOffset,
                new Vector2(Mathf.Abs(hairSegmentOffset.x), Mathf.Abs(hairSegmentOffset.y)) * windStrength,
                windMag
            );
            Vector2 targetPosition = (Vector2)segmentToFollow.position + positionOffset;
            targetPosition += finalVelocity * 0.05f * -1f;

            // Aplica a interpolação para suavizar o movimento do segmento de cabelo em direção à posição alvo
            float hairPositionWeight = 1f - (float)Mathf.Exp(-lerpSpeed * Time.deltaTime);
            hairSegment.position = Vector2.Lerp(hairSegment.position, targetPosition, hairPositionWeight);

            // Limita a distância entre o segmento de cabelo e o segmento que ele está seguindo
            Vector2 distanceToLastSegment = hairSegment.position - segmentToFollow.position;
            float distanceMag = distanceToLastSegment.magnitude;
            float segmentScale = hairSegment.localScale.x;
            if (distanceMag > maxSegmentDistance * segmentScale)
            {
                // Aplica a limitação de distância, movendo o segmento de cabelo para a posição máxima permitida
                Vector2 direction = distanceToLastSegment.normalized * maxSegmentDistance * segmentScale;
                hairSegment.position = (Vector2)segmentToFollow.position + direction;
            }
            segmentToFollow = hairSegment;
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
        Color hairAnchorColor = GetComponent<SpriteRenderer>().color;
        for (int i = 0; i < numberOfSegments; i++)
        {
            GameObject hairSegment = new GameObject("HairSegment_" + (i + 1));
            hairSegment.transform.SetParent(hairAnchor);
            SpriteRenderer sr = hairSegment.AddComponent<SpriteRenderer>();
            sr.sprite = hairSegmentSprite;
            sr.sortingOrder = -i - 1;
            sr.color = hairAnchorColor;

            hairSegment.transform.localPosition = previousHairPosition + hairSegmentOffset;
            previousHairPosition = hairSegment.transform.localPosition;

            float scalePercent = (float)i / numberOfSegments;
            hairSegment.transform.localScale = Vector2.Lerp(
                Vector2.one * hairSizeMax,
                Vector2.one * hairSizeMin,
                scalePercent
            );
            skipNextPhysicsUpdate = true;
        }
    }
}
