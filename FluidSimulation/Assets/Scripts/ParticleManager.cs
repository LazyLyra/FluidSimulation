using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject particle;

    [Header("Particle Spawning")]
    public int spawnNumber;
    [SerializeField] float timer;
    [SerializeField] float spawnIntervals;
    [SerializeField] int spawnedParticles;
    [SerializeField] bool DONESPAWNING;

    [Header("Physics Adjustments")]
    [SerializeField] float minX;
    [SerializeField] float maxX;
    [SerializeField] float minY;
    [SerializeField] float maxY;

    public List<WaterParticleScript> particleArray = new List<WaterParticleScript>();
    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        spawnedParticles = 0;
        DONESPAWNING = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer > spawnIntervals && spawnedParticles < spawnNumber)
        {
            SpawnParticle();
            spawnedParticles += 1;
            timer = 0f;

            if (spawnedParticles == spawnNumber)
            {
                DONESPAWNING = true;
            }
        }

        if (DONESPAWNING)
        {
            for (int i = 0; i < spawnNumber; i++)
            {
                for (int j = i + 1; j < spawnNumber; j++)
                {
                    particleArray[i].HandleCollision(particleArray[j]);
                }
            }
        }
    }

    void SpawnParticle()
    {
        
        GameObject currentParticle = Instantiate(particle);
        WaterParticleScript WPS = currentParticle.GetComponent<WaterParticleScript>();
        WPS.velocity = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));

        if (WPS != null)
        {
            particleArray.Add(WPS);
        }
    }

}
