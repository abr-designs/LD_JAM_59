using System;
using System.Collections;
using System.Collections.Generic;
using Prototypes.Alex;
using Prototypes.Alex.Utilities;
using UnityEngine;

public class FlagHoister : MonoBehaviour
{
    public event Action<IReadOnlyList<FLAG>> OnFlagsChanged;
    public IReadOnlyList<FLAG> CurrentFlags { get; private set; }

    public int maxFlags;
    public SpriteRenderer flagPrefab;
    public float duration = 5.0f;
    public float flagSize;
    public Vector3 flagSpawnPosition;
    public Vector3 targetPosition;
    public float spacing;
    public float moveSpeed;


    private List<SpriteRenderer> m_activeFlagRenderers;

    private static GameFlowManager s_gameFlowManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        if (s_gameFlowManager == null)
            s_gameFlowManager = FindAnyObjectByType<GameFlowManager>();

        m_activeFlagRenderers = new List<SpriteRenderer>();


    }

    public void HoistFlags(List<FLAG> flags)
    {
        if (m_activeFlagRenderers.Count > 0)
        {
            StartCoroutine(RemoveFlagsCoroutine());
        }

        if (flags.Count == 0)
            return;

        CurrentFlags = new List<FLAG>(flags);
        StartCoroutine(HoistFlagsCoroutine(flags));
    }

    public void RemoveFlags()
    {
        if (m_activeFlagRenderers.Count == 0)
            return;

        StartCoroutine(RemoveFlagsCoroutine());
    }

    private IEnumerator HoistFlagsCoroutine(List<FLAG> flags)
    {
        var dir = (flagSpawnPosition - targetPosition).normalized;


        for (int i = 0; i < flags.Count; i++)
        {
            var flag = flags[i];

            var startPos = flagSpawnPosition;

            var flagSpriteRenderer = Instantiate(
                flagPrefab,
                transform.TransformPoint(startPos),
                Quaternion.identity,
                transform);




            flagSpriteRenderer.sprite = flag.GetSprite();

            m_activeFlagRenderers.Add(flagSpriteRenderer);
        }

        float elapsed = 0f;


        while (elapsed < duration)
        {

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < m_activeFlagRenderers.Count; i++)
            {
                var targetPos = targetPosition + (dir * ((flagSize / 2f) + (spacing * i) + (i * flagSize)));
                // Smooth interpolation
                var currentPos = Vector3.Lerp(flagSpawnPosition, targetPos, t);
                m_activeFlagRenderers[i].transform.position = transform.TransformPoint(currentPos);
            }
            yield return null; // wait one frame


        }
        yield break;
    }

    private IEnumerator RemoveFlagsCoroutine()
    {
        for (int i = m_activeFlagRenderers.Count - 1; i >= 0; i--)
        {
            Destroy(m_activeFlagRenderers[i].gameObject);
            m_activeFlagRenderers.RemoveAt(i);
        }

        CurrentFlags = new List<FLAG>();
        m_activeFlagRenderers.Clear();

        yield break;
    }

    //Unity Editor Functions
    //================================================================================================================//

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var dir = (flagSpawnPosition - targetPosition).normalized;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(targetPosition), 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(flagSpawnPosition), 0.1f);
        
        Gizmos.color = Color.white;
        for (int i = 0; i < maxFlags; i++)
        {
            var pos = transform.TransformPoint(targetPosition +  (dir * ((flagSize / 2f) + (spacing * i) + (i * flagSize)))) + (-transform.right.normalized * (flagSize / 2f));
            DrawSquare(pos, Vector3.back, flagSize);
        }
    }
    
    private static void DrawSquare(Vector3 pos, Vector3 normal, float size)
    {
        normal.Normalize();

        // Create a rotation that aligns with the normal
        Quaternion rotation = Quaternion.LookRotation(normal);

        // Get local axes
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;

        float half = size * 0.5f;
        
        Span<Vector3> points = stackalloc Vector3[4];

        // Define corners
        points[0] = pos + (right + up) * half;
        points[1] = pos + (right - up) * half;
        points[2] = pos + (-right - up) * half;
        points[3] = pos + (-right + up) * half;

        Gizmos.DrawLineStrip(points, true);
        
    }
#endif
}
