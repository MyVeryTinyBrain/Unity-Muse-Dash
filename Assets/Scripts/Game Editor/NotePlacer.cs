using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotePlacer : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    NoteButton noteButtonTemplate = null;

    [SerializeField]
    RectTransform contents = null;

    List<NoteButton> noteButtons = new List<NoteButton>();

    List<Note> noteTemplates = new List<Note>();

    [ReadOnly]
    int selectedIndex = -1;

    void GenerateNoteTemplates()
    {
        // 프리팹의 견본들을 생성하여 리스트에 보관합니다.
        // 해당 견본들을 수정하거나, 객체화 할 수 있습니다.

        noteTemplates.ForEach((noteTemplate) => Destroy(noteTemplate));
        noteTemplates.Clear();

        mediator.noteGenerater.templates.ForEach(delegate (Note noteTemplatePrefab)
        {
            Note noteTemplate = Instantiate<Note>(noteTemplatePrefab, this.transform);
            noteTemplate.gameObject.SetActive(false);
            noteTemplates.Add(noteTemplate);
        });
    }

    void GenerateNoteButtons(List<Note> noteTemplates)
    {
        noteButtons.ForEach((noteButton) => Destroy(noteButton));
        noteButtons.Clear();

        for(int i = 0; i < noteTemplates.Count; ++i)
        {
            NoteButton button = Instantiate<NoteButton>(noteButtonTemplate, contents);
            button.Init(noteTemplates[i]);
            button.gameObject.SetActive(true);
            noteButtons.Add(button);

            int capture_i = i;
            button.button.onClick.AddListener(() =>
            {
                Select(capture_i);
            });
        }
    }

    void Select(int index)
    {
        if (selectedIndex < noteButtons.Count && selectedIndex != -1)
        {
            ColorBlock colorBlock = noteButtons[selectedIndex].button.colors;
            colorBlock.normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            noteButtons[selectedIndex].button.colors = colorBlock;
        }

        selectedIndex = index;
        if(selectedIndex < noteButtons.Count)
        {
            ColorBlock colorBlock = noteButtons[selectedIndex].button.colors;
            colorBlock.normalColor = new Color(1, 1, 1, 0.5f);
            noteButtons[selectedIndex].button.colors = colorBlock;
        }
    }

    public Note GetSelectedNoteTemplate()
    {
        if(selectedIndex == -1)
        {
            return null;
        }

        return noteTemplates[selectedIndex];
    }

    void OnSelectBeat(float time, NotePosition position) 
    {
        if (!isActiveAndEnabled)
        {
            // 비활성화 상태에서는 콜백을 처리하지 않습니다.
            return;
        }

        Note template = GetSelectedNoteTemplate();
        if(template == null)
        {
            return;
        }

        if (!template.IsPlaceable(position))
        {
            return;
        }

        Note newNote = Instantiate<Note>(template);
        newNote.gameObject.SetActive(true);
        newNote.data.time = time;
        newNote.data.position = position;
        newNote.Init_CreateInEditor(false);
        mediator.music.AddNote(newNote);
    }

    private void Awake()
    {
        noteButtonTemplate.gameObject.SetActive(false);

        GenerateNoteTemplates();
        GenerateNoteButtons(noteTemplates);
        Select(0);
    }

    private void OnEnable()
    {
        mediator.noteViewport.OnSelectBeat += OnSelectBeat;
    }

    private void OnDisable()
    {
        if(mediator != null)
        {
            mediator.noteViewport.OnSelectBeat -= OnSelectBeat;
        }
    }
}
