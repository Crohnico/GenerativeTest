//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine;
using PlayerX;

namespace PlayerX
{
    public class PX_EnemySpawner : MonoBehaviour
    {
        [Header("Player-X [Enemy Spawner]")]
        
        [Space]
        
        [Header("- Enemies")]
        public GameObject[] enemies;
        
        [Header("- Spawn References")]
        public Transform spawnPoint;

        [Header("- Enemy Target")]
        public GameObject player;
        
        [Header("- Particle Properties")]
        public Transform particlePoint;
        public GameObject spawnParticle;
        
        [Header("- Audio")]
        public AudioSource spawnAudioSource;

        [Header("- Scene Setup")]
        public GameObject[] weapons;
        public Transform particleContainer;
        public Transform dismemberContainer;
        public Transform weaponContainer;
        public Transform collectibleContainer;
        public Transform audioContainer;
        public Transform resetPoint;
        public AudioSource resetAudioSource;
        public PX_Score scoreReference;



        //... Spawn and setup random enemy
        public void Spawn()
        {
            //... Spawn
            var randomEnemy = Random.Range(0, enemies.Length);
            var spawnedEnemy = Instantiate(enemies[randomEnemy], spawnPoint.position, Quaternion.identity);

            //... Scene Setup
            spawnedEnemy.GetComponent<PX_Dependencies>().controller.headTrackContainer.Add(player.GetComponent<PX_Dependencies>().player.headPhysics.transform);
            spawnedEnemy.GetComponent<PX_Dependencies>().state.particleContainer = particleContainer;
            spawnedEnemy.GetComponent<PX_Dependencies>().dismember.dismemberContainer = dismemberContainer;
            spawnedEnemy.GetComponent<PX_Dependencies>().weapons.weaponContainer = weaponContainer;
            spawnedEnemy.GetComponent<PX_Dependencies>().weapons.particleContainer = particleContainer;
            spawnedEnemy.GetComponent<PX_Dependencies>().outOfBound.resetPoint = resetPoint;
            spawnedEnemy.GetComponent<PX_Dependencies>().outOfBound.particleContainer = particleContainer;
            spawnedEnemy.GetComponent<PX_Dependencies>().outOfBound.particlePoint = resetPoint;
            spawnedEnemy.GetComponent<PX_Dependencies>().outOfBound.resetAudioSource = resetAudioSource;
            spawnedEnemy.GetComponent<PX_Dependencies>().sound.audioContainer = audioContainer;
            spawnedEnemy.GetComponent<PX_Collectible_Dropper>().score = scoreReference;
            spawnedEnemy.GetComponent<PX_Collectible_Dropper>().collectPlayer = player.GetComponent<PX_Dependencies>().player.bodyUpperPhysics.transform;
            spawnedEnemy.GetComponent<PX_Collectible_Dropper>().collectibleContainer = collectibleContainer;
            spawnedEnemy.GetComponent<PX_Collectible_Dropper>().particleContainer = particleContainer;
            spawnedEnemy.GetComponent<PX_Collectible_Dropper>().audioContainer = audioContainer;

            //... Add weapons to list
            foreach(GameObject weapon in weapons)
            {
                spawnedEnemy.GetComponent<PX_Dependencies>().weapons.weapons.Add(weapon);
            }

            //... Add new target to player head tracking
            player.GetComponent<PX_Dependencies>().controller.headTrackContainer.Add(spawnedEnemy.GetComponent<PX_Dependencies>().player.headPhysics.transform);

            //... Spawn particle
            var particle = Instantiate(spawnParticle, spawnPoint.position, Quaternion.identity);
            particle.transform.parent = particleContainer;

            //... Play spawn sound
            spawnAudioSource.Play();
        }
    }
}
