using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchstickMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject[] winSpots;
    [SerializeField] private GameObject levelWonUI;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        InitializeSpots();
    }
    void InitializeSpots()
    {
        // Check all holder spots
        GameObject[] holderSpots = GameObject.FindGameObjectsWithTag("HolderSpot");
        foreach (GameObject spot in holderSpots)
        {
            SpotOccupied so = spot.GetComponent<SpotOccupied>();
            if (so != null)
            {
                so.isOccupied = spot.transform.childCount > 0;
            }
        }

        // Check all empty spots
        GameObject[] emptySpots = GameObject.FindGameObjectsWithTag("EmptySpot");
        foreach (GameObject spot in emptySpots)
        {
            SpotOccupied so = spot.GetComponent<SpotOccupied>();
            if (so != null)
            {
                so.isOccupied = spot.transform.childCount > 0;
            }
        }
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Vector2 worldPos = cam.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

                if (hit.collider != null)
                {
                    GameObject clickedObject = hit.collider.gameObject;

                    if (clickedObject.CompareTag("MatchStick"))
                    {
                        TryMoveMatchstick(clickedObject);
                    }
                    else if (clickedObject.CompareTag("HolderSpot"))
                    {
                        TryFillHolder(clickedObject);
                    }
                }
            }
        }
    }

    void TryMoveMatchstick(GameObject matchstick)
    {
        GameObject[] emptySpots = GameObject.FindGameObjectsWithTag("EmptySpot");

        foreach (GameObject spot in emptySpots)
        {
            SpotOccupied so = spot.GetComponent<SpotOccupied>();
            if (!so.isOccupied)
            {
                // Free the current spot (if any) where matchstick was
                SpotOccupied currentSpot = GetParentSpot(matchstick);
                if (currentSpot != null)
                    currentSpot.isOccupied = false;

                StartCoroutine(MoveToSpot(matchstick, spot));
                so.isOccupied = true;
                return;
            }
        }

        Debug.Log("No empty spot available.");
    }

    void TryFillHolder(GameObject holderSpot)
    {
        SpotOccupied holderStatus = holderSpot.GetComponent<SpotOccupied>();
        if (holderStatus.isOccupied) return; // Already filled

        GameObject[] emptySpots = GameObject.FindGameObjectsWithTag("EmptySpot");

        foreach (GameObject spot in emptySpots)
        {
            SpotOccupied emptyStatus = spot.GetComponent<SpotOccupied>();
            if (emptyStatus.isOccupied)
            {
                Transform matchstick = spot.transform.GetChild(0);
                if (matchstick != null)
                {
                    // Move matchstick to holder
                    StartCoroutine(MoveToSpot(matchstick.gameObject, holderSpot));

                    emptyStatus.isOccupied = false;
                    holderStatus.isOccupied = true;

                    return;
                }
            }
        }

        Debug.Log("No matchstick available to move.");
    }

    SpotOccupied GetParentSpot(GameObject matchstick)
    {
        // Optional: link back from matchstick to its parent spot
        Collider2D[] colliders = Physics2D.OverlapPointAll(matchstick.transform.position);
        foreach (var col in colliders)
        {
            if ((col.CompareTag("EmptySpot") || col.CompareTag("HolderSpot")) && col.TryGetComponent(out SpotOccupied spot))
            {
                return spot;
            }
        }
        return null;
    }

    System.Collections.IEnumerator MoveToSpot(GameObject matchstick, GameObject spot)
    {
        Vector3 startPos = matchstick.transform.position;
        float startRotZ = matchstick.transform.eulerAngles.z;

        Vector3 targetPos = new Vector3(spot.transform.position.x, spot.transform.position.y, -0.2f);
        float targetRotZ = spot.transform.eulerAngles.z;
        Vector3 originalScale = matchstick.transform.localScale;

        // Step 1: Set parent first to avoid global rotation conflict
        matchstick.transform.SetParent(spot.transform);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;

            // Smooth position movement
            matchstick.transform.position = Vector3.Lerp(startPos, targetPos, elapsed);

            // Smooth z-axis rotation
            float newZ = Mathf.LerpAngle(startRotZ, targetRotZ, elapsed);
            matchstick.transform.rotation = Quaternion.Euler(0, 0, newZ);

            yield return null;
        }

        matchstick.transform.position = targetPos;
        matchstick.transform.rotation = Quaternion.Euler(0, 0, targetRotZ);
        matchstick.transform.localScale = originalScale;
        CheckWin();
    }

    void CheckWin()
    {
        foreach (GameObject spot in winSpots)
        {
            if (!spot.GetComponent<SpotOccupied>().isOccupied)
            {
                return;
            }
        }
        levelWonUI.SetActive(true);
    }


    public void ReplayBtn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
