using UnityEngine;
using UnityEngine.VFX;

public class Collectable : MonoBehaviour
{
    private bool collected = false;
    [SerializeField] private VisualEffect collectVFX;

    private void OnTriggerEnter(Collider other)
    {
        if(collected == false)
        {
            if (other.CompareTag("Player"))
            {
                collected = true;
                PlayCollectVFX();
                GameManager.instance.PlayerCollect();
                Destroy(gameObject);
            }
        }
    }

    public void PlayCollectVFX()
    {
        if (collectVFX != null)
        {
            collectVFX.Reinit();
            collectVFX.Play();
        }
    }

}