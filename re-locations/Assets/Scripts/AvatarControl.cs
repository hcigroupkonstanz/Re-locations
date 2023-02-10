using UnityEngine;

public class AvatarControl : MonoBehaviour
{
    public bool AvatarVisible = false;
    public Vector3 Position;
    public Vector3 LookAtPosition;
    public bool LookAtLocalUser = false;
    public bool GazeVisible = false;

    [Range(0f, 1f)]
    public float BodyMovement = 0.1f;

    [Range(10f, 100f)]
    public float RotateBodyAngle = 40f;

    [Tooltip("Degree per second")]
    [Range(90f, 360f)]
    public float RotateSpeed = 240f;

    [Tooltip("Alpha per second")]
    [Range(1f, 5f)]
    public float FadeSpeed = 2f;

    [Header("Body")]
    public GameObject LeftEye;
    public GameObject RightEye;
    public GameObject BodyMesh1;
    public GameObject BodyMesh2;
    public Material FadeMaterial;
    public Material OpaqueMaterial;

    private Animator animator;
    private LineRenderer lineRenderer;
    private Renderer rendererBodyMesh1;
    private Renderer rendererBodyMesh2;
    private bool rotatingBody = false;
    private bool lastFrameVisible = false;
    private bool isFading = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
        rendererBodyMesh1 = BodyMesh1.GetComponent<Renderer>();
        if (BodyMesh2) rendererBodyMesh2 = BodyMesh2.GetComponent<Renderer>();
    }

    void Update()
    {
        if (AvatarVisible)
        {
            // Only fade in when Avatar was not visible before
            if (!lastFrameVisible)
            {
                transform.position = Position;
                rendererBodyMesh1.material = FadeMaterial;
                Color bodyMeshColor = rendererBodyMesh1.material.color;
                bodyMeshColor.a = 0f;
                rendererBodyMesh1.material.color = bodyMeshColor;
                if (BodyMesh2)
                {
                    rendererBodyMesh2.material = FadeMaterial;
                    rendererBodyMesh2.material.color = bodyMeshColor;
                }
                isFading = true;
            }

            // Fade in animation
            if (isFading)
            {
                Color bodyMeshColor = rendererBodyMesh1.material.color;
                float newAlpha = bodyMeshColor.a + (FadeSpeed * Time.deltaTime);
                if (newAlpha >= 1f)
                {
                    rendererBodyMesh1.material = OpaqueMaterial;
                    if (BodyMesh2) rendererBodyMesh2.material = OpaqueMaterial;
                    isFading = false;
                }
                else
                {
                    bodyMeshColor.a = newAlpha;
                    rendererBodyMesh1.material.color = bodyMeshColor;
                    if (BodyMesh2) rendererBodyMesh2.material.color = bodyMeshColor;
                }
            }

            // Calculate angle to look object
            Vector2 forward = new Vector2(transform.forward.x, transform.forward.z);
            Vector2 toLookObj = new Vector2(LookAtPosition.x - transform.position.x, LookAtPosition.z - transform.position.z);
            float angle = Vector2.SignedAngle(toLookObj, forward);

            if (rotatingBody)
            {
                if (Mathf.Abs(angle) <= 2f)
                {
                    rotatingBody = false;
                }
                else
                {
                    if (angle > 0)
                    {
                        transform.Rotate(new Vector3(0f, 1f, 0f), RotateSpeed * Time.deltaTime); //2f
                    }
                    else
                    {
                        transform.Rotate(new Vector3(0f, 1f, 0f), -1f * RotateSpeed * Time.deltaTime); //-2f
                    }
                }
            }
            else if (Mathf.Abs(angle) >= RotateBodyAngle)
            {
                rotatingBody = true;
            }

            // Lerp to new position
            transform.position = Vector3.Lerp(transform.position, Position, 0.1f); //0.5f

            // Draw head gaze
            // Transform headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
            if (GazeVisible)
            {
                Vector3 leftEyePosition = LeftEye.transform.position;
                Vector3 rightEyePosition = RightEye.transform.position;
                Vector3 headPosition = Vector3.Lerp(leftEyePosition, rightEyePosition, 0.5f);
                Vector3[] linePositions = { headPosition, LookAtPosition };
                if (LookAtLocalUser)
                {
                    Vector3 lookVector = LookAtPosition - headPosition;
                    lookVector.Normalize();
                    Vector3 lineEndPosition = headPosition + (0.2f * lookVector);
                    linePositions = new Vector3[] { headPosition, lineEndPosition };
                }
                lineRenderer.SetPositions(linePositions);
            }
            else
            {
                Vector3[] zeroPositions = { Vector3.zero, Vector3.zero };
                lineRenderer.SetPositions(zeroPositions);
            }

            lastFrameVisible = true;
        }
        else
        {
            // Only fade out when Avatar was visible before
            if (lastFrameVisible)
            {
                rendererBodyMesh1.material = FadeMaterial;
                Color bodyMeshColor = rendererBodyMesh1.material.color;
                bodyMeshColor.a = 1f;
                rendererBodyMesh1.material.color = bodyMeshColor;
                if (BodyMesh2)
                {
                    rendererBodyMesh2.material = FadeMaterial;
                    rendererBodyMesh2.material.color = bodyMeshColor;
                }
                isFading = true;
            }

            // Fade out animation
            if (isFading)
            {
                Color bodyMeshColor = rendererBodyMesh1.material.color;
                float newAlpha = bodyMeshColor.a - (FadeSpeed * Time.deltaTime);
                if (newAlpha <= 0f)
                {
                    newAlpha = 0f;
                    isFading = false;
                }
                bodyMeshColor.a = newAlpha;
                rendererBodyMesh1.material.color = bodyMeshColor;
                if (BodyMesh2) rendererBodyMesh2.material.color = bodyMeshColor;
            }

            // Remove gaze
            Vector3[] zeroPositions = { Vector3.zero, Vector3.zero };
            lineRenderer.SetPositions(zeroPositions);

            lastFrameVisible = false;
        }
    }

    void OnAnimatorIK()
    {
        if (animator && AvatarVisible)
        {
            animator.SetLookAtWeight(1f, BodyMovement, 1f - BodyMovement);
            animator.SetLookAtPosition(LookAtPosition);
        }
    }
}
