using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleFileBrowser;
using System.IO;

public class GameEditor : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    void OnLoadFile(string[] paths)
    {
        mediator.music.LoadSerializedMusic(paths[0]);
    }

    void OnSaveFile(params string[] paths)
    {
        mediator.music.SaveSerializedMusic(paths[0]);
    }

    public void Load()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Text files", ".txt"));
        FileBrowser.SetDefaultFilter(".txt");

        string initialPath = "C:\\Users\\[YourPath]";
        string initialFilename = "myFile.txt";
        FileBrowser.ShowLoadDialog(OnLoadFile, null, FileBrowser.PickMode.Files, false, initialPath, initialFilename, "Load", "Select");
    }

    public void Save()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Text files", ".txt"));
        FileBrowser.SetDefaultFilter(".txt");

        string initialPath = "C:\\Users\\[YourPath]";
        string initialFilename = "myFile.txt";
        FileBrowser.ShowSaveDialog(OnSaveFile, null, FileBrowser.PickMode.Files, false, initialPath, initialFilename, "Save As", "Save");
    }

    public void SaveToCurrent()
    {
        if (mediator.music.filePath != "")
        {
            OnSaveFile(mediator.music.filePath);
        }
    }

    public void Play()
    {
        Loader.Desc desc = new Loader.Desc();
        desc.filePath = mediator.music.filePath;
        desc.volume = mediator.music.volume;
        desc.mode = Loader.Mode.Play;
        desc.offset = mediator.gameSettings.offset;
        desc.character = mediator.character.characterType;
        desc.clapper = mediator.clapper.gameObject.activeSelf;
        Loader.RecordState(mediator);
        Loader.LoadInEditor(desc);
    }

    public void PlayAt()
    {
        Loader.Desc desc = new Loader.Desc();
        desc.filePath = mediator.music.filePath;
        desc.volume = mediator.music.volume;
        desc.mode = Loader.Mode.TestPlay;
        desc.offset = mediator.gameSettings.offset;
        desc.character = mediator.character.characterType;
        desc.editorTime = mediator.music.playingTime;
        desc.editorState = new Loader.Desc(mediator);
        desc.clapper = mediator.clapper.gameObject.activeSelf;
        //Loader.Load(desc);
        Loader.RecordState(mediator);
        Loader.LoadInEditor(desc);
    }
}
