using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteModifier : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    public enum SelectMode
    {
        ReplaceOrDrag,
        Toggle,
        Append,
    }

    struct DragNote
    {
        public float originTime;
        public INote note;
    }

    [SerializeField]
    Button destroyButton = null;

    [SerializeField]
    Button swapYButton = null;

    [SerializeField]
    Button shiftToLeftBeatButton = null;

    [SerializeField]
    Button shiftToRightBeatButton = null;

    [SerializeField]
    Button fitToLeftBeatButton = null;

    [SerializeField]
    Button fitToRightBeatButton = null;

    [SerializeField]
    Button fitToClosetBeatButton = null;

    [SerializeField]
    Button copyButton = null;

    [SerializeField]
    Button pasteToClosetBeatButton = null;

    List<Note> selectedNotes = new List<Note>();
    SelectMode selectMode = SelectMode.ReplaceOrDrag;

    GameObject copiedTempNoteStorageObject = null;
    List<Note> copiedTempNotes = new List<Note>();

    bool dragSelecteNotes = false;
    List<DragNote> dragNotes = new List<DragNote>();
    float dragStartClosetBeatTime = 0;

    void OnClickDestroyButton()
    {
        DestroySelectedNotes();
    }

    void OnClickSwapYButton()
    {
        SwapYSelectedNotes();
    }

    void OnClickShiftToLeftBeatButton()
    {
        ShiftSelectedNotes(HDirection.Left);
    }

    void OnClickShiftToRightBeatButton()
    {
        ShiftSelectedNotes(HDirection.Right);
    }

    void OnClickFeatToLeftBeatButton()
    {
        FeatSelectedNotesToBeat(HDirection.Left);
    }

    void OnClickFeatToRightBeatButton()
    {
        FeatSelectedNotesToBeat(HDirection.Right);
    }

    void OnClickFeatToClosetBeatButton()
    {
        FeatSelectedNotesToClosetBeat();
    }

    void OnClickCopyButton()
    {
        CopySelectedNotes();
    }

    void OnClickPasteToClosetBeatButton()
    {
        List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
        int closetIndex = mediator.music.GetClosetBeatIndexAtTime(mediator.music.playingTime, mediator.noteViewport.beatType);
        PasteToTime(beats[closetIndex]);
    }

    void DestroySelectedNotes()
    {
        selectedNotes.ForEach((x) => mediator.music.DestroyNote(x));
        Clear();
    }

    void SwapYSelectedNotes()
    {
        selectedNotes.ForEach((x) => {
            x.SwapY();
            x.SetSelect(true);
            });
    }

    void ShiftSelectedNotes(HDirection direction)
    {
        selectedNotes.ForEach((x) =>
        {
            x.Shift(mediator.noteViewport.beatType, direction);
        });
    }

    void FeatSelectedNotesToBeat(HDirection direction)
    {
        selectedNotes.ForEach((x) =>
        {
            x.FitToBeat(mediator.noteViewport.beatType, direction);
        });
    }

    void FeatSelectedNotesToClosetBeat()
    {
        selectedNotes.ForEach((x) =>
        {
            x.FitToClosetBeat(mediator.noteViewport.beatType);
        });
    }

    void CopySelectedNotes()
    {
        // 복사되어 임시 저장된 노트들은 초기화 과정을 거치지 않은 노드입니다.
        // 이에 따라서 OnDestroyInstance 함수가 호출되지 않습니다.
        copiedTempNotes.ForEach((x) => mediator.music.DestroyNote(x));
        copiedTempNotes.Clear();

        selectedNotes.ForEach((x) =>
        {
            Note copied = Instantiate<Note>(x);
            copied.Init_CopyInEditor(x);
            copied.transform.SetParent(copiedTempNoteStorageObject.transform);
            copiedTempNotes.Add(copied);
        });

        // 시간 기준으로 오름차순 정렬합니다.
        copiedTempNotes.Sort((lhs, rhs) => lhs.data.time.CompareTo(rhs.data.time));

        if(copiedTempNotes.Count > 0)
        {
            // 복사한 노트들은 모두 가장 시간이 빠른 노드를 기준으로 시작점 앞으로 이동하게 됩니다.

            // ex) 10, 15, 20, 30, 40, 60 인 노트가 있습니다.
            // 가장 빠른 시간의 노트의 시간인 10만큼 모든 노트의 시간에서 감소시킵니다.
            // 복사한 임시 노트들의 시간은 0, 5, 10, 20, 30, 50 이 됩니다.

            // 이 후 붙여넣기를 할 때 붙여넣기를 한 시간과 더해져서
            // 붙여넣기를 한 시간부터 차례대로 배치됩니다.

            float minTime = copiedTempNotes[0].data.time;
            copiedTempNotes.ForEach((x) => x.data.time -= minTime);
        }
    }

    void PasteToTime(float time)
    {
        // 기존 선택을 취소합니다.
        Clear();

        copiedTempNotes.ForEach((x) =>
        {
            Note newNote = Instantiate<Note>(x);
            newNote.gameObject.SetActive(true);
            newNote.data.time += time;
            newNote.Init_PasteInEditor(x);
            newNote.Init_CreateInEditor(false);
            mediator.music.AddNote(newNote);

            newNote.UpdateNote();

            // 붙여넣기 한 노트들을 선택합니다.
            ToSelect(newNote);
        });
    }

    void ToSelect(Note note)
    {
        int index = selectedNotes.FindIndex((x) => x.data.hash == note.data.hash);
        if(index == -1)
        {
            selectedNotes.Add(note);
            note.SetSelect(true); 
        }

        if(selectedNotes.Count == 1)
        {
            mediator.noteInsepctor.SetNote(note);
        }
        else
        {
            mediator.noteInsepctor.Clear();
        }
    }

    void ToggleSelect(Note note)
    {
        int index = selectedNotes.FindIndex((x) => x.data.hash == note.data.hash);
        if (index >= 0)
        {
            selectedNotes.RemoveAt(index);
            note.SetSelect(false);
        }
        else
        {
            selectedNotes.Add(note);
            note.SetSelect(true);
        }

        mediator.noteInsepctor.Clear();
    }

    void Clear()
    {
        ClearSelect();
        ClearDrag();
    }

    void ClearSelect()
    {
        selectedNotes.ForEach((x) =>
        {
            if (x && x.gameObject)
            {
                x.SetSelect(false);
            }
        });
        selectedNotes.Clear();

        mediator.noteInsepctor.Clear();
    }

    void ClearDrag()
    {
        dragSelecteNotes = false;
        dragNotes.Clear();
    }

    void OnSelectBeat(float time, NotePosition position)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
    }

    void OnSelectNotes(List<Note> notes, NoteElementHandle elementHandle, SelectMouseType eventType)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (eventType == SelectMouseType.MouseDown)
        {
            dragSelecteNotes = false;
            if (selectMode == SelectMode.ReplaceOrDrag && notes.Count == 1)
            {
                if (selectedNotes.Contains(notes[0]))
                {
                    // 노트 드래그 시작

                    dragNotes.Clear();

                    if (elementHandle)
                    {
                        DragNote dragNoteElementHandle = new DragNote()
                        {
                            originTime = elementHandle.GetTime(),
                            note = elementHandle,
                        };
                        dragNotes.Add(dragNoteElementHandle);
                    }
                    else
                    {
                        selectedNotes.ForEach((x) =>
                        {
                            DragNote dragNote = new DragNote()
                            {
                                originTime = x.data.time,
                                note = x,
                            };
                            dragNotes.Add(dragNote);
                        });
                    }

                    Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 hitPointSpaceLocalMouse = mediator.hitPoint.transform.InverseTransformPoint(worldMouse);
                    float mouseTime = mediator.music.LocalXToTime(hitPointSpaceLocalMouse.x, mediator.music.playingTime);

                    int closetBeatIndex;
                    NotePosition closetBeatPosition;
                    if (mediator.noteViewport.ClosetBeatOnWorld(worldMouse, out closetBeatIndex, out closetBeatPosition))
                    {
                        List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
                        dragStartClosetBeatTime = beats[closetBeatIndex];

                        dragSelecteNotes = true;
                        return;
                    }
                }
            }
        }

        switch (selectMode)
        {
            case SelectMode.ReplaceOrDrag:
            ClearSelect();
            break;

            case SelectMode.Toggle:
            break;

            case SelectMode.Append:
            break;
        }

        foreach (Note note in notes)
        {
            switch (selectMode)
            {
                case SelectMode.ReplaceOrDrag:
                ToSelect(note);
                break;

                case SelectMode.Toggle:
                ToggleSelect(note);
                break;

                case SelectMode.Append:
                ToSelect(note);
                break;
            }
        }
    }

    void OnSelectEmpty(SelectMouseType eventType)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (eventType == SelectMouseType.MouseUp && dragSelecteNotes)
        {
            return;
        }

        Clear();
    }

    private void Awake()
    {
        copiedTempNoteStorageObject = new GameObject($"{gameObject.name}(Copied Note Storage)");
        copiedTempNoteStorageObject.SetActive(false);

        destroyButton.onClick.AddListener(OnClickDestroyButton);
        swapYButton.onClick.AddListener(OnClickSwapYButton);
        shiftToLeftBeatButton.onClick.AddListener(OnClickShiftToLeftBeatButton);
        shiftToRightBeatButton.onClick.AddListener(OnClickShiftToRightBeatButton);
        fitToLeftBeatButton.onClick.AddListener(OnClickFeatToLeftBeatButton);
        fitToRightBeatButton.onClick.AddListener(OnClickFeatToRightBeatButton);
        fitToClosetBeatButton.onClick.AddListener(OnClickFeatToClosetBeatButton);
        copyButton.onClick.AddListener(OnClickCopyButton);
        pasteToClosetBeatButton.onClick.AddListener(OnClickPasteToClosetBeatButton);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            dragSelecteNotes = false;
        }
        if (dragSelecteNotes)
        {
            Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 hitPointSpaceLocalMouse = mediator.hitPoint.transform.InverseTransformPoint(worldMouse);
            float mouseTime = mediator.music.LocalXToTime(hitPointSpaceLocalMouse.x, mediator.music.playingTime);

            int closetBeatIndex;
            NotePosition closetBeatPosition;
            if (mediator.noteViewport.ClosetBeatOnWorld(worldMouse, out closetBeatIndex, out closetBeatPosition))
            {
                List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
                float closetBeatTime = beats[closetBeatIndex];
                float timeDiff = closetBeatTime - dragStartClosetBeatTime;

                foreach(DragNote dragNote in dragNotes)
                {
                    dragNote.note.SetTime(dragNote.originTime + timeDiff);
                }
            }
        }

        if(Input.GetKey(Keys.LShift) || Input.GetKey(Keys.RShift))
        {
            selectMode = SelectMode.Append;
        }
        else if(Input.GetKey(Keys.LCnt) || Input.GetKey(Keys.RCnt))
        {
            selectMode = SelectMode.Toggle;
        }
        else
        {
            selectMode = SelectMode.ReplaceOrDrag;
        }

        if (!Utility.UsingInputField())
        {
            if (Input.GetKeyDown(Keys.DestroyNote))
            {
                DestroySelectedNotes();
            }
            else if (Input.GetKeyDown(Keys.SwapNoteY))
            {
                SwapYSelectedNotes();
            }
            else if (Input.GetKeyDown(Keys.ShiftNoteToLeft))
            {
                ShiftSelectedNotes(HDirection.Left);
            }
            else if (Input.GetKeyDown(Keys.ShiftRightToLeft))
            {
                ShiftSelectedNotes(HDirection.Right);
            }
            else if (Input.GetKeyDown(Keys.FitNoteToBeat))
            {
                OnClickFeatToClosetBeatButton();
            }
            else if ((Input.GetKey(Keys.LCnt) || Input.GetKey(Keys.RCnt)) && Input.GetKeyDown(Keys.CopyNote))
            {
                CopySelectedNotes();
            }
            else if ((Input.GetKey(Keys.LCnt) || Input.GetKey(Keys.RCnt)) && Input.GetKeyDown(Keys.PasteNote))
            {
                int closetBeatIndex;
                NotePosition closetNotePosition;
                if (mediator.noteViewport.ClosetBeatOnScreen(Input.mousePosition, out closetBeatIndex, out closetNotePosition))
                {
                    List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
                    PasteToTime(beats[closetBeatIndex]);
                }
            }
        }
    }

    private void OnEnable()
    {
        mediator.noteViewport.OnSelectBeat += OnSelectBeat;
        mediator.noteViewport.OnSelectNotes += OnSelectNotes;
        mediator.noteViewport.OnSelectEmpty += OnSelectEmpty;

        mediator.noteViewport.enableDrag = true;

        Clear();
    }

    private void OnDisable()
    {
        if (mediator)
        {
            mediator.noteViewport.OnSelectBeat -= OnSelectBeat;
            mediator.noteViewport.OnSelectNotes -= OnSelectNotes;
            mediator.noteViewport.OnSelectEmpty -= OnSelectEmpty;

            mediator.noteViewport.enableDrag = false;

            Clear();
        }
    }
}
