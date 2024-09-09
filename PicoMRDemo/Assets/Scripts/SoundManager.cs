using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager instance
    {
        get => _instance;
        private set
        {
            if (_instance != null)
                Debug.LogWarning("Second attempt to get SoundManager");
            _instance = value;
        }
    }

    [Header("Audiclips for interactions")]
    public AudioClip audiConfirm;
    public AudioClip audiHover;
    public AudioClip audiNoti;
    public AudioClip audiSuccess;
    public AudioClip audiFail;
    public AudioClip audiCheer;
    public AudioClip audiShipHit;
    public AudioClip audiBlaster;

    public AudioSource bgAudiS;
    private AudioSource audiS;


    private void Awake()
    {
        instance = this;
        audiS = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        

    }

    public void PlayBG() 
    { 
        if(!bgAudiS.isPlaying)  
            bgAudiS.Play(); 
    }
    public void StopBG() {  bgAudiS.Stop(); }
    //Play for guide


    //Play for main stereo source
    public void PlayConfirm()
    {
        if(audiConfirm != null)
            audiS.PlayOneShot(audiConfirm);
    }

    public void PlayCheer()
    {
        if (audiCheer != null)
            audiS.PlayOneShot(audiCheer);
    }


    public void PlayHover()
    {
        if (audiHover != null)
            audiS.PlayOneShot(audiHover);
    }

    public void PlayNotification()
    {
        if (audiNoti != null)
            audiS.PlayOneShot(audiNoti);
    }
    public void PlaySuccess()
    {
        if (audiSuccess != null)
            audiS.PlayOneShot(audiSuccess);
    }
    public void PlayFail()
    {
        if (audiFail != null)
            audiS.PlayOneShot(audiFail);
    }

    public void PlayShipHit()
    {
        if(audiShipHit)
            audiS.PlayOneShot(audiShipHit);
    }

    public void PlayBlaster()
    {
        if (audiBlaster)
            audiS.PlayOneShot(audiBlaster);
    }

    public void StopMainSound()
    {
        if(audiS)
            audiS.Stop();
    }

    private void OnApplicationQuit()
    {
        StopMainSound();
    }

}
