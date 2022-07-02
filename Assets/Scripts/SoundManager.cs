using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource aSource;
    public AudioClip[] audioClips = new AudioClip[30];
    public AudioClip[] ambientClips = new AudioClip[5];

    public AudioSource globalA; // Global sounds
    public AudioSource globalB; // Ambient/Music

    int global = 0;

    public static SoundManager inst;
    public void Awake()
    {
        inst = this;
    }

    /*
     * -- Main Sounds --
     * 
     * 0 - Miss
     * 1 - Miss
     * 2 - Miss
     * 3 - Miss
     * 4 - Sword Hit
     * 5 - Rapier hit
     * 6 - Slash hit
     * 7 - Crossbow shoot 1
     * 8 - Crossbow shoot 2
     * 9 - Crossbow shoot 3
     * 10 - Bow shoot 1
     * 11 - ???
     * 12 - Defeat | Version A
     * 13 - Explosion (Fireball)
     * 14 - Victory | Version A
     * 15 - Magical Healing
     * 16 - Flesh2Stone (Magic woosh)
     * 17 - Generic Magic
     * 18 - Magic Shoot
     * 19 - Magic Shoot 2
     * 20 - Victory | Version B (MAIN)
     * 21 - Magic wacky
     * 
     *  -- Ambient Sounds (music)
     * 0 - Darkest Dungeon Short 1 (Color of Madness)
     * 1 - Darkest Dungeon Long 1 (Warrens)
     * 2 - Darkest Dungeon Long 2 (Return to Warrens)
     * 3 - Darkest Dungeon Short 2 (Combat in the Ruins)
     * 4 - Black Mesa (Surface Tension)
     */

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TurnManager.musicStart)
        {
            StartCoroutine(PlayNextAmbient());
            StopCoroutine(PlayNextAmbient());
        }

    }

    public void PlayMeleeSound(bool didItHit, AudioSource source)
    {
        int random;

        if(source != null)
        {
            if (didItHit)
            { // Yes, target was hit
                random = Random.Range(4, 6);

                source.PlayOneShot(audioClips[random]);
            }
            else // No, target missed
            {
                random = Random.Range(0, 3);

                source.PlayOneShot(audioClips[random]);
            }
        }
    }

    public void PlayRangedSound(bool didItHit, string type, AudioSource source)
    {
        int random;

        if (source != null)
        {
            switch (type)
            {
                case "icon_crossbow":
                    random = Random.Range(7, 9);

                    source.PlayOneShot(audioClips[random]);
                    break;

                case "icon_shortbow":
                    source.PlayOneShot(audioClips[10]);
                    break;

                case "icon_javelin":
                    random = Random.Range(4, 6);

                    source.PlayOneShot(audioClips[random]);
                    break;

                default:

                    break;
            }

        }
    }

    public void PlayMagicSound(bool didItHit, string type, AudioSource source)
    {
        //int random;

        if (source != null)
        {
            switch (type)
            {
                case "icon_fireball":

                    source.PlayOneShot(audioClips[13]);
                    break;

                case "icon_curewounds":
                    source.PlayOneShot(audioClips[15]);
                    break;

                case "icon_healingword":
                    source.PlayOneShot(audioClips[15]);
                    break;

                case "icon_actionsurge":
                    source.PlayOneShot(audioClips[15]);
                    break;

                case "icon_secondwind":
                    source.PlayOneShot(audioClips[16]);
                    break;

                case "icon_firebolt":
                    source.PlayOneShot(audioClips[18]);
                    break;

                case "icon_sacredflame":
                    source.PlayOneShot(audioClips[19]);
                    break;

                case "icon_shieldoffaith":
                    source.PlayOneShot(audioClips[21]);
                    break;

                case "icon_shield":
                    source.PlayOneShot(audioClips[21]);
                    break;

                case "icon_chaosbolt":
                    source.PlayOneShot(audioClips[21]);
                    break;

                default:

                    break;
            }

        }
    }

    public void PlayGlobalSound(int id)
    {
        globalA.PlayOneShot(audioClips[id]);
    }

    public void PlayAmbientAudio(int id)
    {
        //this.GetComponent<AudioSource>().loop = true;
        globalB.PlayOneShot(ambientClips[id]);
        global++;
    }

    public void StopAmbient()
    {
        globalB.Stop();
        globalB.mute = true;
    }

    IEnumerator PlayNextAmbient()
    {
        yield return new WaitForSeconds(1f); // Wait a second

        if (!globalB.isPlaying) // If the last audio clip isn't playing, play the next one
        {
            globalB.PlayOneShot(ambientClips[global]);
        }
    }
}
