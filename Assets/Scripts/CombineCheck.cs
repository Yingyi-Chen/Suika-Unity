using System.Collections;
using TMPro;
using UnityEngine;

public class CombineCheck : MonoBehaviour
{
    public GameObject[] fruitPrefabs;

    private Coroutine mergingCoroutine;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == transform.gameObject.name)
        {
            // increase the score on the "score" TextMeshPro gameobject by floor(index * 2.5 + 1)
            GameObject scoreText = GameObject.Find("score");
            if (scoreText != null)
            {
                TextMeshProUGUI scoreTextMesh = scoreText.GetComponent<TextMeshProUGUI>();
                if (scoreTextMesh != null)
                {
                    int fruitIndex = System.Array.FindIndex(this.fruitPrefabs, fruit => fruit.name == transform.gameObject.name);
                    string newScore = (int.Parse(scoreTextMesh.text) + (int)Mathf.Floor(fruitIndex * 2.25f + 1f)).ToString();
                    scoreTextMesh.text = newScore;
                    GameObject.Find("FinalScore").GetComponent<TextMeshProUGUI>().text = newScore;
                }
            }


            // Determine the master fruit based on their unique identifiers
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
    }

    private CombineCheck DetermineMasterFruit(CombineCheck otherCombineCheck)
    {
        // Compare unique criteria to determine the master fruit
        // In this example, we use the instance ID as a simple criterion
        return (GetInstanceID() > otherCombineCheck.GetInstanceID()) ? this : otherCombineCheck;
    }

    private IEnumerator HandleCollision(Collision2D collision)
    {
        int fruitIndex = System.Array.FindIndex(this.fruitPrefabs, fruit => fruit.name == transform.gameObject.name);

        if (fruitIndex < this.fruitPrefabs.Length - 1)
        {
            Destroy(collision.gameObject);
            GameObject newFruit = Instantiate(this.fruitPrefabs[fruitIndex + 1], transform.position, Quaternion.identity);
            newFruit.name = this.fruitPrefabs[fruitIndex + 1].name;

            // Make sure the rigidbody is not kinematic
            newFruit.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

            // Delay the destruction of the current fruit
            yield return new WaitForSeconds(0.1f); // Adjust the delay as needed
            Destroy(gameObject);

            // Play the sound effect "pop.wav" in the "Audio Source" component in the "Spawner" gameobject
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
    }

    private void OnDestroy()
    {
        // Stop the coroutine when the script is destroyed
        if (mergingCoroutine != null)
        {
            StopCoroutine(mergingCoroutine);
        }
    }
}
