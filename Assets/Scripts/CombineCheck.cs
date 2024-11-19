using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombineCheck : MonoBehaviour
{
    [SerializeField] private GameObject[] fruitPrefabs;

    private Coroutine mergingCoroutine;
    private static HashSet<int> mergingFruits = new HashSet<int>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name != transform.gameObject.name)
            return; // Only merge fruits of the same type

        int thisID = GetInstanceID();
        int otherID = collision.gameObject.GetInstanceID();

        // Check if either fruit is already merging
        if (mergingFruits.Contains(thisID) || mergingFruits.Contains(otherID))
            return;

        // Add current fruit and the colliding fruit to the merging set
        mergingFruits.Add(thisID);
        mergingFruits.Add(otherID);

        CombineCheck masterFruit = DetermineMasterFruit(collision.gameObject.GetComponent<CombineCheck>());

        if (masterFruit == this)
        {
            if (mergingCoroutine != null)
            {
                StopCoroutine(mergingCoroutine);
            }

            mergingCoroutine = StartCoroutine(HandleCollision(collision));
        }
    }

    private CombineCheck DetermineMasterFruit(CombineCheck otherCombineCheck)
    {
        // Determine which fruit should control the merge based on instance ID
        return (GetInstanceID() > otherCombineCheck.GetInstanceID()) ? this : otherCombineCheck;
    }

    private IEnumerator HandleCollision(Collision2D collision)
    {
        int fruitIndex = System.Array.FindIndex(this.fruitPrefabs, fruit => fruit.name == transform.gameObject.name);

        if (fruitIndex < this.fruitPrefabs.Length - 1)
        {
            // Destroy the two merging fruits
            Destroy(collision.gameObject);

            // Instantiate the next level of fruit
            GameObject newFruit = Instantiate(this.fruitPrefabs[fruitIndex + 1], transform.position, Quaternion.identity);
            newFruit.name = this.fruitPrefabs[fruitIndex + 1].name;

            // Ensure the Rigidbody2D is set to Dynamic
            Rigidbody2D rb = newFruit.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

            // Delay to allow merge effect and to reset remaining fruit
            yield return new WaitForSeconds(0.1f);

            Destroy(gameObject);

            // Play merge sound and update score
            PlayMergeSound();
            UpdateScore(fruitIndex);
        }

        // Remove current fruit from merging list to allow further merges
        mergingFruits.Remove(GetInstanceID());
    }

    private void PlayMergeSound()
    {
        GameObject spawner = GameObject.Find("Spawner");
        if (spawner != null)
        {
            AudioSource audioSource = spawner.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    private void UpdateScore(int fruitIndex)
    {
        GameObject scoreText = GameObject.Find("score");
        TextMeshProUGUI scoreTextMesh = scoreText?.GetComponent<TextMeshProUGUI>();

        if (scoreTextMesh != null)
        {
            int newScore = int.Parse(scoreTextMesh.text) + (int)Mathf.Floor(fruitIndex * 2.25f + 1f);
            scoreTextMesh.text = newScore.ToString();

            // Fix the issue with FinalScore text
            GameObject finalScoreObject = GameObject.Find("FinalScore");
            if (finalScoreObject != null)
            {
                TextMeshProUGUI finalScoreText = finalScoreObject.GetComponent<TextMeshProUGUI>();
                if (finalScoreText != null)
                {
                    finalScoreText.text = newScore.ToString();
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Ensure fruit ID is removed from merging set on destruction
        mergingFruits.Remove(GetInstanceID());
    }
}
