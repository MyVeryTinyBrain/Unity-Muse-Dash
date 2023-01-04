using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mediator : MonoBehaviour
{
    public GameSettings gameSettings;
    public Music music;
    public HitPoint hitPoint;
    public VolumeInspector volumeInspector;
    public NoteViewport noteViewport;
    public NotePlacer notePlacer;
    public NoteModifier noteModifier;
    public Boss boss;
    public InspectorPanel inspectorPanel;
    public NoteInspector noteInsepctor;
    public NoteGenerater noteGenerater;
    public SpineEffectPool spineEffectPool;
    public Character character;
    public EffectPool effectPool;
    public ComboCountUI comboCountUI;
    public ComboResultUI comboResultUI;
    public GameState gameState;
    public TapComboCountUI tapComboCountUI;
    public GameEditor gameEditor;
    public GamePlay gamePlay;
    public GameUI gameUI;
    public FxAudio fxAudio;
    public MainCamera mainCamera;
    public PlaySettingsPanel playSettingsPanel;
    public Clapper clapper;
    public PauseMenu pauseMenu;
    public Background background;

    private void Awake()
    {
        gameObject.tag = Tags.Mediator;
    }

    static Mediator _instance = null;
    public static Mediator i
    {
        get
        {
            if (!_instance)
            {
                _instance = Find();
            }
            return _instance;
        }
    }

    public static Mediator Find()
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag(Tags.Mediator);
        if(gameObject == null)
        {
            return null;
        }

        Mediator component = null;
        gameObject.TryGetComponent<Mediator>(out component);
        return component;
    }
}
