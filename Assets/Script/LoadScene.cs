using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Scene name constants based on your Build Settings
    public const string LEVEL_SATU = "LevelSatu";
    public const string CLOSE = "Close";
    public const string MAIN_MENU = "MainMenu";
    public const string SPLASH_SCENE = "SplashScene";
    public const string WIN = "Win";
    public const string LOADING = "Loading";
    public const string CHOOSE_LV = "ChooseLv";  // Note the space in the name
    public const string LEVEL_DUE = "LevelDua";
    public const string PAUSE = "Pause";
    public const string TUTORIAL = "Tutorial";

    // Main scene loading method with error checking
    public void LoadSceneByName(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            if (SceneExistsInBuild(LOADING))
            {
                SceneManager.LoadScene(LOADING);
                StartCoroutine(LoadAsyncScene(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in build settings!");
            // Fallback to main menu if scene doesn't exist
            SceneManager.LoadScene(MAIN_MENU);
        }
    }

    // Specific scene loaders for all your scenes
    public void LoadLevelSatu() => LoadSceneByName(LEVEL_SATU);
    public void LoadClose() => LoadSceneByName(CLOSE);
    public void LoadMainMenu() => LoadSceneByName(MAIN_MENU);
    public void LoadSplashScene() => LoadSceneByName(SPLASH_SCENE);
    public void LoadWinScene() => LoadSceneByName(WIN);
    public void LoadLoadingScreen() => LoadSceneByName(LOADING);
    public void LoadChooseLevel() => LoadSceneByName(CHOOSE_LV);
    public void LoadLevelDue() => LoadSceneByName(LEVEL_DUE);
    public void LoadPauseMenu() => LoadSceneByName(PAUSE);
    public void LoadTutorial() => LoadSceneByName(TUTORIAL);

    // Async loading with loading screen
    private IEnumerator LoadAsyncScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    // Check if scene exists in build settings
    private bool SceneExistsInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameInBuild == sceneName)
                return true;
        }
        return false;
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Optional: Load by build index if needed
    public void LoadSceneByIndex(int buildIndex)
    {
        if (buildIndex >= 0 && buildIndex < SceneManager.sceneCountInBuildSettings)
        {
            if (SceneExistsInBuild(LOADING))
            {
                SceneManager.LoadScene(LOADING);
                StartCoroutine(LoadAsyncScene(buildIndex));
            }
            else
            {
                SceneManager.LoadScene(buildIndex);
            }
        }
        else
        {
            Debug.LogError($"Build index {buildIndex} is out of range!");
            SceneManager.LoadScene(MAIN_MENU);
        }
    }

    public void paused()
    {
        Time.timeScale = 0;
    }

    public void resume()
    {
        Time.timeScale = 1;
    }

    private IEnumerator LoadAsyncScene(int buildIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}