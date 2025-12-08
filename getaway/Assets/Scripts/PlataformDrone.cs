using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallDistance = 3f;
    public float fallSpeed = 2f;
    public float returnSpeed = 1.5f;
    public float returnDelay = 1.5f;

    private Vector3 startPos;
    private Vector3 fallTarget;
    private bool playerOnPlatform;
    private Coroutine currentAction;
    private bool isReturning;
    private Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        fallTarget = startPos + Vector3.down * fallDistance;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; 
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerOnPlatform = true;


        collision.transform.SetParent(transform);


        if (isReturning)
        {
            StopCurrentAction();
            isReturning = false;
        }

        if (currentAction == null)
        {
            currentAction = StartCoroutine(Fall());
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerOnPlatform = false;


        collision.transform.SetParent(null);


        if (currentAction == null && !isReturning)
        {
            StartReturn();
        }
    }

    System.Collections.IEnumerator Fall()
    {
        Vector3 targetPos = fallTarget;


        while (Vector3.Distance(rb.position, targetPos) > 0.01f)
        {

            if (!playerOnPlatform)
                break;

            Vector3 newPos = Vector3.MoveTowards(
                rb.position,
                targetPos,
                fallSpeed * Time.deltaTime
            );
            rb.MovePosition(newPos);
            yield return null;
        }


        if (!playerOnPlatform && Vector3.Distance(rb.position, targetPos) > 0.01f)
        {
            while (Vector3.Distance(rb.position, targetPos) > 0.01f)
            {
                Vector3 newPos = Vector3.MoveTowards(
                    rb.position,
                    targetPos,
                    fallSpeed * Time.deltaTime
                );
                rb.MovePosition(newPos);
                yield return null;
            }
        }


        rb.MovePosition(targetPos);


        currentAction = null;


        if (!playerOnPlatform)
        {
            StartReturn();
        }
    }

    void StartReturn()
    {
        if (currentAction != null)
            StopCurrentAction();

        currentAction = StartCoroutine(ReturnAfterDelay());
    }

    System.Collections.IEnumerator ReturnAfterDelay()
    {
        isReturning = true;


        yield return new WaitForSeconds(returnDelay);


        if (playerOnPlatform)
        {
            isReturning = false;
            currentAction = null;
            yield break;
        }


        while (Vector3.Distance(rb.position, startPos) > 0.01f)
        {
          
            if (playerOnPlatform)
            {
                isReturning = false;
                currentAction = null;
                yield break;
            }

            Vector3 newPos = Vector3.MoveTowards(
                rb.position,
                startPos,
                returnSpeed * Time.deltaTime
            );
            rb.MovePosition(newPos);
            yield return null;
        }

        rb.MovePosition(startPos);

        isReturning = false;
        currentAction = null;
    }

    void StopCurrentAction()
    {
        if (currentAction != null)
        {
            StopCoroutine(currentAction);
            currentAction = null;
        }
    }

    void OnDestroy()
    {
  
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Player"))
                {
                    child.SetParent(null);
                }
            }
        }
    }
}