using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndPanelBehavior : MonoBehaviour
{
    [SerializeField] float animationDuration = 1;
    [SerializeField] AnimationCurve scaleCurve;
    bool animate = false;
    float animateTimer;

    private void OnEnable()
    {
        transform.localScale = new Vector3(0,1,1);
        animate = true;
        animateTimer = 0;
    }

    void Update()
    {
        if (animate)
        {
            if (animateTimer < 1)
            {
                animateTimer += Time.deltaTime / animationDuration;
                transform.localScale = new Vector3(0, 1, 1) + Vector3.right * scaleCurve.Evaluate(animateTimer);
            }
            else
            {
                transform.localScale = Vector3.one;
                animate = false;
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
