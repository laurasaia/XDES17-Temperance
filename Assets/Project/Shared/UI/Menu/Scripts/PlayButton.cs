using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "TesteBase";

    public void Play()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}