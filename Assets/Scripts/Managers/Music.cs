using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MusicSource))]
public class Music : MonoBehaviour
{
    protected Mediator mediator => Mediator.i; 

    public delegate void OnLoadMusicDelegate();
    public delegate void OnStartMusicDelegate();
    public delegate void OnEndPlayDelegate();
    public event OnLoadMusicDelegate OnLoadMusic;
    public event OnStartMusicDelegate OnStartMusicWhenGamePlay;
    public event OnEndPlayDelegate OnEndPlayWhenGamePlay;

    [SerializeField, ReadOnlyInRuntime]
    MusicSource musicSource;

    [SerializeField]
    List<Note> notes;

    [SerializeField, HideInInspector]
    List<BPMData> bpmDatas;

    [SerializeField]
    public List<BPMData> interfaceBPMDatas;

    [SerializeField]
    List<float> beatDatas4;

    [SerializeField]
    List<float> beatDatas8;

    [SerializeField]
    List<float> beatDatas16;

    [SerializeField]
    List<float> beatDatas32;

    float[] left = null;
    float[] right = null;

    [SerializeField, HideInInspector]
    List<SpeedScaleData> speedScaleDatas;

    [SerializeField]
    public List<SpeedScaleData> interfaceSpeedScaleDatas;

    [SerializeField, HideInInspector]
    List<MapTypeData> mapTypeDatas;

    [SerializeField]
    public List<MapTypeData> interfaceMapTypeDatas;

    [SerializeField]
    List<BossAnimationData> bossAnimationsDatas;

    [SerializeField, ReadOnly]
    ulong nextNoteHash = 0;

    [SerializeField]
    public MusicConfigData musicConfigData = MusicConfigData.Default();

    float speedScaleAtNow = 1;
    float speedScaleAtAdjustedTime = 1;
    float speedScaleMultiplier = 1;

    [SerializeField]
    List<Note> currentVisibleNotes = new List<Note>();

    public string filePath { get; private set; } = "";

    public float currentGlobalSpeedScale => speedScaleAtNow;
    public float adjustedTimeGlobalSpeedScale => speedScaleAtAdjustedTime;

    public float globalSpeedScaleMultiplier
    {
        get => speedScaleMultiplier;
        set => speedScaleMultiplier = value;
    }

    public bool isPlaying
    {
        get => musicSource.isPlaying;
    }

    public bool isPaused
    {
        get => musicSource.isPaused;
    }

    public int playingTimeSample
    {
        get => musicSource.playingTimeSample;
        set => musicSource.playingTimeSample = value;
    }

    public float playingTime
    {
        get => musicSource.playingTime;
        set => musicSource.playingTime = value;
    }

    public float adjustedTime
    {
        get
        {
            if (mediator.gameSettings.isEditor)
            {
                return playingTime;
            }
            else
            {
                return ApplyOffset(playingTime);
            }
        }
    }

    public float normalizedPlayingTime
    {
        get => musicSource.normalizedPlayingTime;
        set => musicSource.normalizedPlayingTime = value;
    }

    public float volume
    {
        get => musicSource.volume;
        set => musicSource.volume = value;
    }

    public float pitch
    {
        get => musicSource.pitch;
        set => musicSource.pitch = value;
    }

    public int frequency
    {
        get => musicSource.clip ? musicSource.clip.frequency : 0;
    }

    public int timeSamples
    {
        get => musicSource.timeSamples;
    }

    public float length
    {
        get => musicSource.length;
    }

    public List<float> beats4
    {
        get => beatDatas4;
    }

    public List<float> beats8
    {
        get => beatDatas8;
    }

    public List<float> beats16
    {
        get => beatDatas16;
    }

    public List<float> beats32
    {
        get => beatDatas32;
    }

    public float[] leftVolumeData
    {
        get => left;
    }

    public float[] rightVolumeData
    {
        get => right;
    }

    public List<BossAnimationData> bossAnimations
    {
        get => bossAnimationsDatas;
    }

    public List<Note> visibleNotes => currentVisibleNotes;

    public float ApplyOffset(float time)
    {
        return time - mediator.gameSettings.offset;
    }

    public IMusicSourcePlayMethod playMethod
    {
        get => musicSource.musicPlayMethod;
        set => musicSource.musicPlayMethod = value;
    }

    public void Play()
    {
        musicSource.Play();
    }

    public void PlayAt(float time)
    {
        musicSource.PlayAt(time);
    }

    public void Stop()
    {
        musicSource.Stop();
    }

    public void Pause()
    {
        musicSource.Pause();
    }

    public void Resume()
    {
        musicSource.Resume();
    }

    public void TogglePause()
    {
        musicSource.TogglePause();
    }

    public int CalculateMaxAccuracy()
    {
        int n = 0;
        foreach (Note note in notes)
        {
            n += note.GetAccuracyScore();
        }
        return n;
    }

    public int CalculateMaxCombo()
    {
        int n = 0;
        foreach (Note note in notes)
        {
            n += note.GetMaxCombo();
        }
        return n;
    }

    public void GenerateBeats()
    {
        if (musicSource == null) musicSource = GetComponent<MusicSource>();

        if (beatDatas4 == null) beatDatas4 = new List<float>();
        if (beatDatas8 == null) beatDatas8 = new List<float>();
        if (beatDatas16 == null) beatDatas16 = new List<float>();
        if (beatDatas32 == null) beatDatas32 = new List<float>();

        beatDatas4.Clear();
        beatDatas8.Clear();
        beatDatas16.Clear();
        beatDatas32.Clear();

        // 시간 기준으로 오름차순 정렬합니다.
        bpmDatas.Sort((BPMData lhs, BPMData rhs) => lhs.time.CompareTo(rhs.time));
        for (int i = 0; i < bpmDatas.Count; ++i)
        {
            int current = i;
            int next = Mathf.Clamp(i + 1, 0, bpmDatas.Count - 1);
            if (current == next ||
                bpmDatas[next].transition == TransitionType.Constant ||
                bpmDatas[current].bpm == bpmDatas[next].bpm)
            {
                GenerateConstantBeats(current, BeatType.B4, beatDatas4);
                GenerateConstantBeats(current, BeatType.B8, beatDatas8);
                GenerateConstantBeats(current, BeatType.B16, beatDatas16);
                GenerateConstantBeats(current, BeatType.B32, beatDatas32);
            }
            else
            {
                GenerateContinuousBeats(current, next, BeatType.B4, beatDatas4);
                GenerateContinuousBeats(current, next, BeatType.B8, beatDatas8);
                GenerateContinuousBeats(current, next, BeatType.B16, beatDatas16);
                GenerateContinuousBeats(current, next, BeatType.B32, beatDatas32);
            }
        }

        void GenerateConstantBeats(int i, BeatType beatType, List<float> beatDatas)
        {
            float begin = bpmDatas[i].time;
            float end = (i < bpmDatas.Count - 1) ? bpmDatas[i + 1].time : musicSource.clip.length;
            float delta = (60f / bpmDatas[i].bpm) / (float)beatType;
            for (float beatTime = begin; beatTime < end; beatTime += delta)
            {
                beatDatas.Add(beatTime + bpmDatas[i].offset);
            }
        }

        void GenerateContinuousBeats(int current, int next, BeatType beatType, List<float> beatDatas)
        {
            float start = bpmDatas[current].time;
            float end = bpmDatas[next].time;
            float startBPM = bpmDatas[current].bpm;
            float endBPM = bpmDatas[next].bpm;
            while (start < end)
            {
                float beatDealy = CalculateNextBeatDelay(start, end, startBPM, endBPM, beatType);
                float beatTime = beatDealy + start;
                start = beatTime;
                startBPM = GetBPMAtTime(start);
                if (beatTime < end)
                {
                    beatDatas.Add(beatTime + bpmDatas[current].offset);
                }
            }
        }

        float CalculateNextBeatDelay(float startTime, float endTime, float startBPM, float endBPM, BeatType beatType)
        {
            float S = startTime;
            float E = endTime;
            float Sb = startBPM;
            float Eb = endBPM;
            float F = (float)beatType;
            float a = (F / (E - S)) * (Eb - Sb);
            float b = F * Sb;
            float c = -60;
            float x1 = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
            //float x2 = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
            return x1;
        }
    }

    public List<float> GetBeats(BeatType type)
    {
        switch (type)
        {
            case BeatType.B4: return beatDatas4;
            case BeatType.B8: return beatDatas8;
            case BeatType.B16: return beatDatas16;
            case BeatType.B32: return beatDatas32;
            default: return beatDatas4;
        }
    }

    public int GetClosetBeatIndexAtTime(float playingTime, BeatType type)
    {
        List<float> beats = GetBeats(type);
        int bsBegin = 0;
        int bsEnd = beats.Count - 1;
        int closetIndex = 0;
        // Binary search
        while (bsBegin <= bsEnd)
        {
            int mid = (bsBegin + bsEnd) / 2;
            if (beats[mid] <= playingTime)
            {
                closetIndex = mid;
                bsBegin = mid + 1;
            }
            else
            {
                bsEnd = mid - 1;
            }
        }
        return closetIndex;
    }

    public float GetSpeedScaleAtTime(float time)
    {
        if(speedScaleDatas.Count == 0)
        {
            return 1.0f * speedScaleMultiplier;
        }
        // Binary search
        int begin = 0;
        int end = speedScaleDatas.Count - 1;
        int closetIndex = 0;
        while(begin <= end)
        {
            int mid = (begin + end) / 2;
            if (speedScaleDatas[mid].time <= time)
            {
                begin = mid + 1;
                closetIndex = mid;
            }
            else
            {
                end = mid - 1;
            }
        }
        int nextIndex = Mathf.Clamp(closetIndex + 1, 0, speedScaleDatas.Count - 1);
        if (speedScaleDatas[nextIndex].transition == TransitionType.Constant || 
            speedScaleDatas[closetIndex].speedScale == speedScaleDatas[nextIndex].speedScale)
        {
            // 다음에 전환되어야 할 데이터의 트랜지션 타입이 Constant이면 선형보간을 하지 않습니다.
            // 현재 데이터와 다음 데이터의 값이 같아도 선형보간을 하지 않습니다.
            return speedScaleDatas[closetIndex].speedScale * speedScaleMultiplier; 
        }
        else 
        {
            float deltaTime = speedScaleDatas[nextIndex].time - speedScaleDatas[closetIndex].time;
            float ratio = (time - speedScaleDatas[closetIndex].time) / deltaTime;
            ratio = Mathf.Clamp(ratio, 0.0f, 1.0f);
            return Mathf.Lerp(speedScaleDatas[closetIndex].speedScale, speedScaleDatas[nextIndex].speedScale, ratio) * speedScaleMultiplier;
        }
    }

    public float TimeToRatio(float time, float speed, float playingTime)
    {
        return MusicUtility.TimeToRatio(time, speed * GetSpeedScaleAtTime(playingTime), playingTime);
    }

    public float TimeToRatioAtCurrentTime(float time, float speed)
    {
        return MusicUtility.TimeToRatio(time, speed * currentGlobalSpeedScale, playingTime);
    }

    public float TimeToRatioAtAdjustedTime(float time, float speed)
    {
        // speedScaleAtAdjustedTime: 오프셋이 적용된 게임 전체 배속
        // adjustedTime: 오프셋이 적용된 현재 음악 재생 시간
        return MusicUtility.TimeToRatio(time, speed * speedScaleAtAdjustedTime, adjustedTime);
    }

    public float TimeToLocalX(float time, float speed, float playingTime)
    {
        float ratio = TimeToRatio(time, speed, playingTime);
        return ratio * mediator.gameSettings.lengthPerSeconds;
    }

    public float TimeToLocalXAtCurrentTime(float time, float speed)
    {
        float ratio = TimeToRatioAtCurrentTime(time, speed);
        return ratio * mediator.gameSettings.lengthPerSeconds;
    }

    public float TimeToLocalXAtAdjustedTime(float time, float speed)
    {
        float ratio = TimeToRatioAtAdjustedTime(time, speed);
        return ratio * mediator.gameSettings.lengthPerSeconds;
    }

    struct LocalXToTime_Pair
    {
        public float time;
        public float localX;
    }
    List<LocalXToTime_Pair> LocalXToTime_Pairs = null;
    public float LocalXToTime(float localX, float playingTime)
    {
        LocalXToTime_Pair CalculatePair(float time)
        {
            LocalXToTime_Pair pair = new LocalXToTime_Pair();
            pair.time = time;
            float ratio = MusicUtility.TimeToRatio(time, 1 * GetSpeedScaleAtTime(playingTime), playingTime);
            pair.localX = ratio * mediator.gameSettings.lengthPerSeconds;
            return pair;
        }

        LocalXToTime_Pairs = LocalXToTime_Pairs ?? new List<LocalXToTime_Pair>();
        LocalXToTime_Pairs.Clear();

        LocalXToTime_Pair first = CalculatePair(0 - 60);
        LocalXToTime_Pairs.Add(first);

        foreach (SpeedScaleData data in speedScaleDatas)
        {
            LocalXToTime_Pair pair = CalculatePair(data.time);
            LocalXToTime_Pairs.Add(pair);
        }

        LocalXToTime_Pair last = CalculatePair(length + 60);
        LocalXToTime_Pairs.Add(last);

        // Binary search

        int begin = 0;
        int end = LocalXToTime_Pairs.Count - 1;
        int closetIndex = 0;
        while (begin <= end)
        {
            int mid = (begin + end) / 2;
            if (LocalXToTime_Pairs[mid].localX <= localX)
            {
                begin = mid + 1;
                closetIndex = mid;
            }
            else
            {
                end = mid - 1;
            }
        }

        int currentIndex = closetIndex;
        int nextIndex = Mathf.Clamp(closetIndex + 1, 0, LocalXToTime_Pairs.Count - 1);
        float t = MathUtility.InverseLerp(LocalXToTime_Pairs[currentIndex].localX, LocalXToTime_Pairs[nextIndex].localX, localX);
        float time = Mathf.Lerp(LocalXToTime_Pairs[currentIndex].time, LocalXToTime_Pairs[nextIndex].time, t);
        return time;
    }

    public void GetScreenTimes(float playingTime, out float leftTime, out float rightTime)
    {
        Camera cam = Camera.main;
        Vector2 worldCamSize = cam.OrthographicExtents();
        Vector2 worldCamPos = cam.transform.position;
        Vector2 localCamSize = mediator.hitPoint.transform.InverseTransformVector(worldCamSize);
        Vector2 localCamPos = mediator.hitPoint.transform.InverseTransformPoint(worldCamPos);
        float localCameraHalfSize = localCamSize.x / 2;
        float localCameraX = localCamPos.x;
        leftTime = LocalXToTime(localCameraX - localCameraHalfSize, playingTime);
        rightTime = LocalXToTime(localCameraX + localCameraHalfSize, playingTime);
    }

    public float GetBPMAtTime(float time)
    {
        if (bpmDatas.Count == 0)
        {
            return 0.0f;
        }

        // Binary search

        int begin = 0;
        int end = bpmDatas.Count - 1;
        int closetIndex = 0;
        while (begin <= end)
        {
            int mid = (begin + end) / 2;
            if (bpmDatas[mid].time <= time)
            {
                begin = mid + 1;
                closetIndex = mid;
            }
            else
            {
                end = mid - 1;
            }
        }

        int nextIndex = Mathf.Clamp(closetIndex + 1, 0, bpmDatas.Count - 1);
        if (bpmDatas[nextIndex].transition == TransitionType.Constant ||
            bpmDatas[closetIndex].bpm == bpmDatas[nextIndex].bpm)
        {
            // 다음에 전환되어야 할 데이터의 트랜지션 타입이 Constant이면 선형보간을 하지 않습니다.
            // 현재 데이터와 다음 데이터의 값이 같아도 선형보간을 하지 않습니다.
            return bpmDatas[closetIndex].bpm;
        }
        else
        {
            float deltaTime = bpmDatas[nextIndex].time - bpmDatas[closetIndex].time;
            float ratio = (time - bpmDatas[closetIndex].time) / deltaTime;
            ratio = Mathf.Clamp(ratio, 0.0f, 1.0f);
            return Mathf.Lerp(bpmDatas[closetIndex].bpm, bpmDatas[nextIndex].bpm, ratio);
        }
    }

    public MapType GetMapTypeAtTime(float time)
    {
        if (mapTypeDatas.Count == 0)
        {
            return MapType._01;
        }

        // Binary search

        int begin = 0;
        int end = mapTypeDatas.Count - 1;
        int closetIndex = 0;
        while (begin <= end)
        {
            int mid = (begin + end) / 2;
            if (mapTypeDatas[mid].time <= time)
            {
                begin = mid + 1;
                closetIndex = mid;
            }
            else
            {
                end = mid - 1;
            }
        }

        return mapTypeDatas[closetIndex].type;
    }

    public void AddNote(Note note) 
    {
        note.transform.SetParent(mediator.hitPoint.transform);
        note.Init();
        notes.Add(note);
    }

    public bool DestroyNote(Note note)
    {
        bool removed = notes.Remove(note);
        if (removed)
        {
            if (note.isInstance)
            {
                note.OnDestroyInstance();
            }
        }

        Destroy(note.gameObject);
        return removed;
    }

    public void DestroyAllNotes()
    {
        notes.ForEach((x) => Destroy(x.gameObject));
        notes.Clear();
        bossAnimationsDatas.Clear();
    }

    public void AddBossAnimationData(BossAnimationData data)
    {
        bossAnimationsDatas.Add(data);
        SortBossAnimationDatas();
    }

    public bool DestroyBossAnimationData(BossAnimationData data)
    {
        return bossAnimationsDatas.Remove(data);
    }

    public void SortBossAnimationDatas()
    {
        // 시간 기준으로 오름차순 정렬합니다.
        bossAnimationsDatas.Sort((lhs, rhs) => lhs.time.CompareTo(rhs.time));
    }

    void UpdateBossAnimationOrder()
    {
        // 애니메이션 데이터를 시간에 따라 오름차순으로 정렬합니다.
        for (int i = 0; i < bossAnimationsDatas.Count; i++)
        {
            int back = Mathf.Clamp(i - 1, 0, bossAnimationsDatas.Count - 1);
            int next = Mathf.Clamp(i + 1, 0, bossAnimationsDatas.Count - 1);
            if (bossAnimationsDatas[back].time > bossAnimationsDatas[i].time ||
                bossAnimationsDatas[next].time < bossAnimationsDatas[i].time)
            {
                SortBossAnimationDatas();
            }
        }
    }

    public int GetBossAnimationIndexAtTime(float playingTime)
    {
        int bsBegin = 0;
        int bsEnd = bossAnimationsDatas.Count - 1;
        int closetIndex = 0;

        // Binary search
        while (bsBegin <= bsEnd)
        {
            int mid = (bsBegin + bsEnd) / 2;
            if (bossAnimationsDatas[mid].time <= playingTime)
            {
                closetIndex = mid;
                bsBegin = mid + 1;
            }
            else
            {
                bsEnd = mid - 1;
            }
        }

        return closetIndex;
    }

    public ulong GetNextNoteHash()
    {
        return nextNoteHash++;
    }

    public int GetNearBeatIndex(float playingTime, BeatType beatType)
    {
        List<float> beats = GetBeats(beatType);
        int closetBeatIndex = GetClosetBeatIndexAtTime(playingTime, beatType);
        int nextBeatIndex = Mathf.Clamp(closetBeatIndex + 1, 0, beats.Count - 1);

        float diff1 = Mathf.Abs(beats[closetBeatIndex] - playingTime);
        float diff2 = Mathf.Abs(beats[nextBeatIndex] - playingTime);

        return diff1 < diff2 ? closetBeatIndex : nextBeatIndex;
    }

    public int GetBeatIndexToShift(float playingTime, BeatType beatType, HDirection direction)
    {
        List<float> beats = GetBeats(beatType);
        int closetBeatIndex = GetClosetBeatIndexAtTime(playingTime, beatType);
        if(direction == HDirection.Right)
        {
            closetBeatIndex = Mathf.Clamp(closetBeatIndex + 1, 0, beats.Count - 1);
        }

        if (Mathf.Abs(beats[closetBeatIndex] - playingTime) > 0.001f)
        {
            return closetBeatIndex;
        }

        return Mathf.Clamp(closetBeatIndex + (int)direction, 0, beats.Count - 1);
    }

    [ContextMenu("ApplyBPMDatas")]
    public void ApplyBPMDatas()
    {
        bpmDatas.Clear();
        bpmDatas.AddRange(interfaceBPMDatas);
        // GenerateBeats 함수 내부에서 정렬을 진행합니다.
        //bpmDatas.Sort((BPMData lhs, BPMData rhs) => lhs.time.CompareTo(rhs.time));
        GenerateBeats();
    }

    [ContextMenu("ApplySpeedScaleDatas")]
    public void ApplySpeedScaleDatas()
    {
        speedScaleDatas.Clear();
        speedScaleDatas.AddRange(interfaceSpeedScaleDatas);
        speedScaleDatas.Sort((SpeedScaleData lhs, SpeedScaleData rhs) => lhs.time.CompareTo(rhs.time));
    }

    [ContextMenu("ApplyMapTypeDatas")]
    public void ApplyMapTypeDatas()
    {
        mapTypeDatas.Clear();
        mapTypeDatas.AddRange(interfaceMapTypeDatas);
        mapTypeDatas.Sort((MapTypeData lhs, MapTypeData rhs) => lhs.time.CompareTo(rhs.time));
    }

    void OnStartMusic()
    {
        if (!mediator.gameSettings.isEditor)
        {
            OnStartMusicWhenGamePlay?.Invoke();
        }
    }

    void OnEndPlay()
    {
        if (!mediator.gameSettings.isEditor)
        {
            OnEndPlayWhenGamePlay?.Invoke();
        }
    }

    private void Awake()
    {
        Utility.CreateSaveDirectory();

        musicSource = GetComponent<MusicSource>();
        musicSource.OnStartMusic += OnStartMusic;
        musicSource.OnEndPlay += OnEndPlay;
    }

    private void OnDestroy()
    {
        DestoryCurrentClip();
    }

    private void Update()
    {
        speedScaleAtNow = GetSpeedScaleAtTime(playingTime);
        speedScaleAtAdjustedTime = GetSpeedScaleAtTime(adjustedTime);

        if (mediator.gameSettings.isEditor)
        {
            UpdateBossAnimationOrder();
        }

        // Note update
        currentVisibleNotes.Clear();
        foreach (var note in notes)
        {
            NoteVisibleState visibleState = note.UpdateNote();
            if(visibleState == NoteVisibleState.In)
            {
                currentVisibleNotes.Add(note);
            }
        }
    }

    void DestoryCurrentClip()
    {
        if (!musicSource.clip)
        {
            return;
        }

#if UNITY_EDITOR
        // 이 클립이 에셋인 경우에는 삭제할 수 없습니다.
        // 에디터 상에서 실수로 클립을 등록한 경우에 적용되는 예외 코드입니다.
        if (AssetDatabase.Contains(musicSource.clip))
        {
            return;
        }
#endif

        Destroy(musicSource.clip);
        musicSource.clip = null;
    }

    private void ResetMusic_Internal()
    {
        mediator.spineEffectPool.GameReset();
        mediator.effectPool.GameReset();
        mediator.character.GameReset();
        mediator.gameState.GameReset();

        DestroyAllNotes();

        bpmDatas.Clear();
        interfaceBPMDatas.Clear();
        beatDatas4.Clear();
        beatDatas8.Clear();
        beatDatas16.Clear();
        beatDatas32.Clear();
        speedScaleDatas.Clear();
        interfaceSpeedScaleDatas.Clear();
        mapTypeDatas.Clear();
        interfaceMapTypeDatas.Clear();
        nextNoteHash = 0;
        musicConfigData = MusicConfigData.Default();
    }

    public void ResetMusic()
    {
        ResetMusic_Internal();

        DestoryCurrentClip();

        filePath = "";

        GC.Collect(2, GCCollectionMode.Forced);
    }

    public bool LoadClip(string path)
    {
        if (!File.Exists(path))
            return false;

        AudioType audioType = AudioType.UNKNOWN;
        string extension = Path.GetExtension(path).ToLower();
        if(extension == ".mp3")
        {
            audioType = AudioType.MPEG;
        }
        else if(extension == ".wav")
        {
            audioType = AudioType.WAV;
        }
        else if(extension == ".ogg")
        {
            audioType = AudioType.OGGVORBIS;
        }
        else
        {
            return false;
        }

        path = Utility.ToStandardPathFormat(path);

        // 파일 경로를 파일을 저장하는 폴더 내부로 변경합니다.
        string newPath = $"{Utility.GetFileSaveDirectory()}\\{Path.GetFileName(path)}";
        // 파일을 저장하는 폴더가 없다면 생성합니다.
        Utility.CreateSaveDirectory();
        // 해당 파일이 이 경로에 존재하지 않을때는 이 경로로 복사합니다.
        if (!File.Exists(newPath))
        {
            File.Copy(path, newPath);
        }
        // 최종적으로 파일을 저장하는 폴더에 파일이 위치하게 됩니다.
        // 이 경로의 파일을 읽습니다.
        path = newPath;

        AudioClip clip = null;
        UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, audioType);
        try
        {
            req.SendWebRequest();
            while (!req.isDone) ;
            clip = DownloadHandlerAudioClip.GetContent(req);
        }
        catch
        {
            return false;
        }
        if (!clip)
            return false;
        // 오디오 클립을 교체할 때 이 함수를 호출해서 메모리 누적을 방지합니다.
        DestoryCurrentClip();

        musicSource.clip = clip;
        if (mediator.gameSettings.isEditor)
        {
            MusicUtility.ExtractAudioVolumes(musicSource.clip, out left, out right);
        }
        return true;
    }

    SerialziedMusic GenerateSerializedMusic()
    {
        SerialziedMusic serialzied = new SerialziedMusic();

        notes.ForEach((note) => serialzied.noteDatas.Add(note.data.Copy()));
        serialzied.bpmDatas.AddRange(bpmDatas);
        serialzied.interfaceBPMDatas.AddRange(interfaceBPMDatas);
        serialzied.beatDatas4.AddRange(beatDatas4);
        serialzied.beatDatas8.AddRange(beatDatas8);
        serialzied.beatDatas16.AddRange(beatDatas16);
        serialzied.beatDatas32.AddRange(beatDatas32);
        serialzied.speedScaleDatas.AddRange(speedScaleDatas);
        serialzied.interfaceSpeedScaleDatas.AddRange(interfaceSpeedScaleDatas);
        serialzied.mapTypeDatas.AddRange(mapTypeDatas);
        serialzied.interfaceMapTypeDatas.AddRange(interfaceMapTypeDatas);
        serialzied.nextNoteHash = nextNoteHash;
        serialzied.musicConfigData = musicConfigData;

        return serialzied;
    }

    void ReadSerializedMusic(SerialziedMusic serialzied, bool withoutLoadMusic = false)
    {
        ResetMusic_Internal();

        foreach (NoteData data in serialzied.noteDatas)
        {
            Note note = mediator.noteGenerater.Generate(data);

            // 음악의 보스 애니메이션은 이 함수에서 추가됩니다.
            note.Init_Load();

            if (mediator.gameSettings.isEditor)
            {
                note.Init_CreateInEditor(true);
            }
            else
            {
                note.Init_CreateInGame();
            }

            AddNote(note);
        }

        bpmDatas.AddRange(serialzied.bpmDatas);
        interfaceBPMDatas.AddRange(serialzied.interfaceBPMDatas);
        beatDatas4.AddRange(serialzied.beatDatas4);
        beatDatas8.AddRange(serialzied.beatDatas8);
        beatDatas16.AddRange(serialzied.beatDatas16);
        beatDatas32.AddRange(serialzied.beatDatas32);
        speedScaleDatas.AddRange(serialzied.speedScaleDatas);
        interfaceSpeedScaleDatas.AddRange(serialzied.interfaceSpeedScaleDatas);
        mapTypeDatas.AddRange(serialzied.mapTypeDatas);
        interfaceMapTypeDatas.AddRange(serialzied.interfaceMapTypeDatas);
        nextNoteHash = serialzied.nextNoteHash;
        musicConfigData = serialzied.musicConfigData;

        playingTime = 0;

        if (!withoutLoadMusic)
        {
            LoadClip($"{Utility.GetFileSaveDirectory()}\\{musicConfigData.musicFileName}");
        }

        GC.Collect(2, GCCollectionMode.Forced);

        OnLoadMusic?.Invoke();
    }

    public void LoadSerializedMusic(string path, bool withoutLoadMusic = false)
    {
        string json;
        using (StreamReader reader = new StreamReader(path))
        {
            json = reader.ReadToEnd();
            filePath = path;
        }
        SerialziedMusic serialzied = SerialziedMusic.DeserializeFromJson(json);
        ReadSerializedMusic(serialzied, withoutLoadMusic);
    }

    public void SaveSerializedMusic(string path)
    {
        SerialziedMusic serialzied = GenerateSerializedMusic();
        string json = serialzied.SerializeToJson();

        using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.Unicode))
        {
            writer.Write(json);
            filePath = path;
        }
    }
}

public class SerialziedMusic
{
    public List<NoteData> noteDatas = new List<NoteData>();
    public List<BPMData> bpmDatas = new List<BPMData>();
    public List<BPMData> interfaceBPMDatas = new List<BPMData>();
    public List<float> beatDatas4 = new List<float>();
    public List<float> beatDatas8 = new List<float>();
    public List<float> beatDatas16 = new List<float>();
    public List<float> beatDatas32 = new List<float>();
    public List<SpeedScaleData> speedScaleDatas = new List<SpeedScaleData>();
    public List<SpeedScaleData> interfaceSpeedScaleDatas = new List<SpeedScaleData>();
    public List<MapTypeData> mapTypeDatas = new List<MapTypeData>();
    public List<MapTypeData> interfaceMapTypeDatas = new List<MapTypeData>();
    public ulong nextNoteHash = 0;
    public MusicConfigData musicConfigData = MusicConfigData.Default();

    public string SerializeToJson(bool prettyPrint = true)
    {
        return JsonUtility.ToJson(this, prettyPrint);
    }

    public static SerialziedMusic DeserializeFromJson(string json)
    {
        return JsonUtility.FromJson<SerialziedMusic>(json);
    }
}

