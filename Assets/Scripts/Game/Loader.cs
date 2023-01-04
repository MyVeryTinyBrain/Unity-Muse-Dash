using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public enum Mode
    {
        Play,
        TestPlay,
        Editor,
    }

    public class Desc
    {
        public string filePath;
        public Mode mode = Mode.Editor;
        public float editorTime = 0.0f;
        public float volume = 0.25f;
        public float offset = 0.0f;
        public float pitch = 1.0f;
        public CharacterType character = CharacterType.Clear;
        public bool clapper = false;
        public Desc editorState = null;

        public Desc() { }
        public Desc(Mediator mediator)
        {
            filePath = mediator.music.filePath;
            editorTime = mediator.music.playingTime;
            volume = mediator.music.volume;
            pitch = mediator.music.pitch;
            offset = mediator.gameSettings.offset;
            character = mediator.character.characterType;
            clapper = mediator.clapper.gameObject.activeSelf;
        }
    }

    static Desc recordedState = null;
    static Desc lastLoadDesc = null;
    public Desc desc
    {
        get;
        private set;
    } = null;

    private static void Load_Internal(Desc desc, bool withoutLoadMusic)
    {
        lastLoadDesc = desc;

        Mediator mediator = Mediator.i;

        GameSettings gameSettings = mediator.gameSettings;
        gameSettings.isEditor = (desc.mode == Mode.Editor);
        gameSettings.offset = desc.offset;

        GameState gameState = mediator.gameState;
        gameState.GameReset();

        Music music = mediator.music;
        music.LoadSerializedMusic(desc.filePath, withoutLoadMusic);

        Character character = mediator.character;
        GameEditor gameEditor = mediator.gameEditor;
        GamePlay gamePlay = mediator.gamePlay;
        GameUI gameUI = mediator.gameUI; ;
        PauseMenu pauseMenu = mediator.pauseMenu;
        Clapper clapper = mediator.clapper;
        HitPoint hitPoint = mediator.hitPoint;
        Boss boss = mediator.boss;
        MainCamera mainCamera = mediator.mainCamera;
        TapComboCountUI tapComboCountUI = mediator.tapComboCountUI;

        character.Init(desc.character);
        gameUI.GameReset();
        pauseMenu.GameReset();
        clapper.gameObject.SetActive(desc.clapper);
        hitPoint.Init();
        boss.GameReset();
        mainCamera.GameReset();
        tapComboCountUI.GameReset();

        switch (desc.mode)
        {
            case Mode.Play:
            {
                gameEditor.gameObject.SetActive(false);
                character.gameObject.SetActive(true);
                gamePlay.gameObject.SetActive(true);

                MusicSourcePrecision musicSourcePlayMethod = new MusicSourcePrecision();
                musicSourcePlayMethod.startDelay = music.musicConfigData.startDelay;
                musicSourcePlayMethod.endDelay = music.musicConfigData.endDelay;

                music.playMethod = musicSourcePlayMethod;
                music.Play();

                gameState.allowFullCombo = true;
                gameUI.Show_ReadyGo();
            }
            break;

            case Mode.TestPlay:
            {
                gameEditor.gameObject.SetActive(false);
                character.gameObject.SetActive(true);
                gamePlay.gameObject.SetActive(true);

                music.playMethod = new MusicSourceDefault();
                music.PlayAt(desc.editorTime);
            }
            break;

            case Mode.Editor:
            {
                gameEditor.gameObject.SetActive(true);
                character.gameObject.SetActive(false);
                gamePlay.gameObject.SetActive(false);

                music.playMethod = new MusicSourceDefault();
                music.playingTime = desc.editorTime;
                music.pitch = desc.pitch;
            }
            break;
        }

        music.volume = desc.volume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= this.OnSceneLoaded;

        Load_Internal(desc, false);

        Destroy(this.gameObject);
    }

    public static Loader Load(Desc desc)
    {
        GameObject loaderObject = new GameObject();
        Loader loader = loaderObject.AddComponent<Loader>();
        loader.desc = desc;

        DontDestroyOnLoad(loaderObject);
        SceneManager.sceneLoaded += loader.OnSceneLoaded;

        SceneManager.LoadScene(Scenes.Game, LoadSceneMode.Single);

        return loader;
    }

    public static void LoadInEditor(Desc desc)
    {
        Load_Internal(desc, true);
    }

    public static void RecordState(Mediator mediator)
    {
        recordedState = new Desc(mediator);
    }

    public static void ClearRecordedState()
    {
        recordedState = null;
    }

    public static Desc GetRecordedState()
    {
        return recordedState;
    }

    public static Desc GetLastLoadDesc()
    {
        return lastLoadDesc;
    }
}
