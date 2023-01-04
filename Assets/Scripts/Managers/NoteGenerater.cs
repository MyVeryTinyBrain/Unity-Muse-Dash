using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteGenerater : MonoBehaviour
{
    [SerializeField]
    List<Note> notePrefabs;

    List<Note> noteTemplates = null;

    public List<Note> templates
    {
        get
        {
            if(noteTemplates == null)
            {
                GenerateNoteTemplates();
            }

            return noteTemplates;
        }
    }

    public Note Generate(NoteData data)
    {
        foreach(Note template in noteTemplates)
        {
            if(NoteData.CompareType(template.data, data))
            {
                Note note = Instantiate<Note>(template, this.transform);
                note.data = data.Copy();
                note.gameObject.SetActive(true);
                return note;
            }
        }

        return null;
    }

    void GenerateNoteTemplates()
    {
        if(noteTemplates != null)
        {
            return;
        }

        if(noteTemplates == null)
        {
            noteTemplates = new List<Note>();
        }

        notePrefabs.ForEach(delegate (Note prefab)
        {
            Note noteTemplate = Instantiate<Note>(prefab, this.transform);
            noteTemplate.gameObject.SetActive(false);
            noteTemplates.Add(noteTemplate);
        });
    }

    private void Awake()
    {
        GenerateNoteTemplates();
    }
}
