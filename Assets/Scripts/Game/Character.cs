using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public class Character : MonoBehaviour, IGameReset
{
    protected Mediator mediator => Mediator.i;

    public delegate void OnDeadDelegate();
    public event OnDeadDelegate OnDeadCallback;

    [SerializeField]
    CharacterDescs characterDescs = null;

    [SerializeField]
    GameObject airPivot = null;

    [SerializeField]
    GameObject centerPivot = null;

    [SerializeField]
    GameObject roadPivot = null;

    [SerializeField]
    GameObject airPosition = null;

    [SerializeField]
    GameObject centerPosition = null;

    [SerializeField]
    GameObject roadPosition = null;

    [SerializeField]
    CharacterSpine character = null;

    [SerializeField]
    CharacterSpine airAlterEgo = null;

    [SerializeField]
    CharacterSpine centerAlterEgo = null;

    [SerializeField]
    CharacterSpine roadAlterEgo = null;

    [SerializeField]
    GameObject shadow = null;

    CharacterType currentCharacterType = CharacterType.None;

    NotePosition prevPosition = NotePosition.Road;
    NotePosition position = NotePosition.Road;
    float positionChangeStartTime = 0;
    float positionChangeDelay = 0.06f;
    float slowChangeDelay = 0.11f;
    float fastChangeDelay = 0.06f;

    int airPressStack = 0;
    int roadPressStack = 0;
    List<KeyCode> airKeys = new List<KeyCode>();
    List<KeyCode> roadKeys = new List<KeyCode>();
    float centerAttackDelay = 0.01f;
    Note currentHitNote = null;
    Note lastHitNote = null;

    List<Note> interactions = new List<Note>();

    float invisibleDelay = 0.5f;
    float lastDamageTime = float.MinValue;

    float maxHP = 100.0f;
    float currentHP = 100.0f;
    bool isDead = false;

    public float hp
    {
        get => currentHP;
        set
        {
            float prevHP = currentHP;
            currentHP = Mathf.Clamp(value, 0, maxHP);
            if (prevHP > 0 && currentHP == 0)
            {
                isDead = true;
                OnDead();
            }
            ApplyHP();
        }
    }

    public CharacterType characterType
    {
        get => currentCharacterType;
    }

    public NotePosition characterPosition
    {
        get => position;
    }

    public Vector2 hitSpaceLocalPos
    {
        get => mediator.hitPoint.transform.InverseTransformPoint(transform.position);
    }

    public float timeOfCharacterPos
    {
        get => mediator.music.LocalXToTime(hitSpaceLocalPos.x, mediator.music.adjustedTime);
    }

    public float ratioOfCharacterPos
    {
        get => mediator.music.TimeToRatio(timeOfCharacterPos, 1, mediator.music.adjustedTime);
    }

    void ApplyHP()
    {
        mediator.gameUI.SetHP(maxHP, currentHP);
    }

    public void TakeDamage()
    {
        if (isDead)
        {
            return;
        }

        float playingTime = mediator.music.adjustedTime;

        if (playingTime > lastDamageTime + invisibleDelay)
        {
            character.Do_Hurt();
            lastDamageTime = playingTime;

            hp -= 20;
        }
    }

    public void ClearState()
    {
        currentHitNote = null;
        lastHitNote = null;
        interactions.Clear();
        SetPositionImmediate(NotePosition.Road);
        character.Do_In();
        airPressStack = 0;
        roadPressStack = 0;

        lastDamageTime = float.MinValue;
        hp = maxHP;
        isDead = false;
    }

    CharacterSpine GetAlterEgo(NotePosition notePosition)
    {
        switch (notePosition)
        {
            case NotePosition.Air:
            return airAlterEgo;

            case NotePosition.Center:
            return centerAlterEgo;

            default:
            case NotePosition.Road:
            return roadAlterEgo;
        }
    }

    public void Init(CharacterType characterType)
    {
        int index = characterDescs.descs.FindIndex((x) => x.type == characterType);
        if (index == -1)
        {
            if (characterDescs.descs.Count > 0)
            {
                Debug.LogWarning($"There's no matched character type({characterType}) in {characterDescs.name}", this);

                index = 0;
                characterType = characterDescs.descs[index].type;
            }
            else
            {
                Debug.LogError($"There's no character desc in {characterDescs.name}", this);
                return;
            }
        }

        CharacterDesc desc = characterDescs.descs[index];
        if (desc != null)
        {
            // Init hp
            maxHP = desc.hp;
            hp = maxHP;
        }

        currentCharacterType = characterType;

        character.Init(characterType, false);
        airAlterEgo.Init(characterType, true);
        centerAlterEgo.Init(characterType, true);
        roadAlterEgo.Init(characterType, true);

        character.gameObject.SetActive(true);
        shadow.gameObject.SetActive(true);
        airAlterEgo.gameObject.SetActive(false);
        centerAlterEgo.gameObject.SetActive(false);
        roadAlterEgo.gameObject.SetActive(false);

        ClearState();
    }

    private void SetPosition(NotePosition position, float delay)
    {
        this.positionChangeDelay = delay;

        if (this.position == position)
        {
            return;
        }

        this.prevPosition = this.position;
        this.position = position;
        this.positionChangeStartTime = Time.time;
    }

    private void SetPositionImmediate(NotePosition position)
    {
        this.prevPosition = position;
        this.position = position;
        this.positionChangeStartTime = Time.time;
    }

    bool IsPlayingDoubleHit()
    {
        if (character.animationInfo != null)
        {
            switch (character.animationInfo.desc.type)
            {
                case CharacterAnimationType.Double_hit_1:
                case CharacterAnimationType.Double_hit_2:
                return true;
            }
        }
        return false;
    }

    Vector2 GetWorldPosition(NotePosition position)
    {
        switch (position)
        {
            case NotePosition.Air:
            return airPosition.transform.position;

            case NotePosition.Center:
            if (IsPlayingDoubleHit())
            {
                return roadPosition.transform.position;
            }
            return centerPosition.transform.position;

            case NotePosition.Road:
            return roadPosition.transform.position;

            default:
            return roadPosition.transform.position;
        }
    }

    Type GetMainInterationType()
    {
        if (interactions.Count == 0)
        {
            return null;
        }

        return interactions[0].GetType();
    }

    NotePosition ToInteractPosition(NotePosition attackPosition)
    {
        if (interactions.Count == 0)
        {
            if (character.desc.fly)
            {
                return attackPosition;
            }
            else
            {
                return NotePosition.Road;
            }
        }

        int air = 0;
        int center = 0;
        int road = 0;
        int mostWeight = interactions[0].interactWeight;
        for (int i = 0; i < interactions.Count; ++i)
        {
            int weight = interactions[i].interactWeight;
            if (weight < mostWeight)
            {
                break;
            }

            switch (interactions[i].data.position)
            {
                case NotePosition.Air:
                air++;
                break;

                case NotePosition.Center:
                center++;
                break;

                case NotePosition.Road:
                road++;
                break;
            }
        }

        int iPosition = (air > 0 ? +1 : 0) + (center > 0 ? 0 : 0) + (road > 0 ? -1 : 0);
        NotePosition position = (NotePosition)iPosition;
        return position;
    }

    void AddInteraction(Note note)
    {
        interactions.Add(note);

        // 내림차순 정렬합니다.
        interactions.Sort((lhs, rhs) => { return rhs.interactWeight.CompareTo(lhs.interactWeight); });
    }

    void RemoveInteration(Note note)
    {
        interactions.Remove(note);
    }

    void OnEndInteract(Note note, NoteResult result)
    {
        note.OnEndInteractCallback -= OnEndInteract;
        RemoveInteration(note);
        SetPosition(ToInteractPosition(position), slowChangeDelay);

        if (interactions.Count == 0)
        {
            if (note.GetType() == typeof(LongNote))
            {
                switch (note.data.position)
                {
                    case NotePosition.Air:
                    character.Do_EndPressInAir();
                    break;

                    case NotePosition.Road:
                    character.Do_EndPressInRoad();
                    break;
                }
            }
            else
            {
                character.Do_Run();
            }
        }

        if (result.hit)
        {
            mediator.gameState.Combo(note, result);
        }

        if (result.miss)
        {
            mediator.gameState.Miss(note);
        }

        if (result.damage)
        {
            TakeDamage();
        }
    }

    bool IsHitSameTimeNote(Note note)
    {
        return
            lastHitNote != null &&
            Mathf.Abs(note.data.time - lastHitNote.data.time) < centerAttackDelay &&
            lastHitNote.interactWeight <= 0;
    }

    bool IsDoubleHitNote(Note note)
    {
        // Hit same beam note
        if(lastHitNote != null)
        {
            Type curType = note.GetType();
            if(curType == typeof(BeamNote) && lastHitNote == note)
            {
                return true;
            }
        }

        return 
            lastHitNote != null &&
            Mathf.Abs(note.data.time - lastHitNote.data.time) < centerAttackDelay &&
            lastHitNote.interactWeight <= 0 &&
            lastHitNote.data.position != note.data.position;
    }

    void Attack(NotePosition attackPosition)
    {
        if (isDead)
        {
            return;
        }

        bool alterEgo = interactions.Count > 0;
        float delay = fastChangeDelay;
        NotePosition newHitPosition = attackPosition;
        Action<CharacterSpine> setAnimation = null;

        List<Note> visibleNotes = mediator.music.visibleNotes;
        bool used = false;
        foreach (Note note in visibleNotes)
        {
            NoteResult interactResult = note.Interact(attackPosition);

            if (interactResult.startInteract)
            {
                note.OnEndInteractCallback += OnEndInteract;
                AddInteraction(note);
            }
            if (interactResult.hit)
            {
                used = true;
                lastHitNote = currentHitNote;
                currentHitNote = note;

                mediator.gameState.Combo(note, interactResult);

                if (note.GetType() == typeof(LongNote))
                {
                    NotePosition newInteractPosition = ToInteractPosition(position);
                    if (newInteractPosition != position)
                    {
                        //if (lastHitNote != null &&
                        //    Mathf.Abs(lastHitNote.data.time - note.data.time) < centerAttackDelay &&
                        //    lastHitNote.interactWeight <= 0)
                        if (IsHitSameTimeNote(note)) 
                        {
                            CharacterSpine interactCS = GetAlterEgo(position);
                            interactCS.gameObject.SetActive(true);
                            interactCS.CopyAnimationState(character);

                            if (interactCS.animationInfo == null)
                            {
                                return;
                            }

                            switch (interactResult.precision)
                            {
                                case ComboPrecision.Great:
                                switch (interactCS.animationInfo.desc.type)
                                {
                                    case CharacterAnimationType.Up_hit:
                                    interactCS.Do_Hit(CharacterHitType.Air_hit_great);
                                    break;

                                    case CharacterAnimationType.Down_hit:
                                    interactCS.Do_Hit(CharacterHitType.Road_hit_great);
                                    break;
                                }
                                break;

                                case ComboPrecision.Perfect:
                                switch (interactCS.animationInfo.desc.type)
                                {
                                    case CharacterAnimationType.Up_hit:
                                    interactCS.Do_Hit(CharacterHitType.Air_hit_perfect);
                                    break;

                                    case CharacterAnimationType.Down_hit:
                                    interactCS.Do_Hit(CharacterHitType.Road_hit_perfect);
                                    break;
                                }
                                break;
                            }
                        }
                        switch (newInteractPosition)
                        {
                            case NotePosition.Air:
                            character.Do_PressToUp();
                            break;

                            case NotePosition.Road:
                            character.Do_PressToDown();
                            break;
                        }
                    }
                    else
                    {
                        character.Do_Press();
                    }
                    SetPosition(newInteractPosition, slowChangeDelay);
                }
                else if (note.GetType() == typeof(SandBagNote) || note.GetType() == typeof(BossRushNote))
                {
                    if (GetMainInterationType() == typeof(LongNote))
                    {
                        centerAlterEgo.gameObject.SetActive(true);
                        centerAlterEgo.Do_Hit(CharacterHitType.Air_hit_perfect);
                    }
                    else
                    {
                        SetPosition(ToInteractPosition(position), fastChangeDelay);
                        character.Do_Hit(CharacterHitType.Air_hit_perfect);
                    }
                }
            }
            if (interactResult.abort)
            {
                return;
            }

            NoteResult hitResult = note.Hit(attackPosition);
            if (hitResult.hit)
            {
                used = true;
                lastHitNote = currentHitNote;
                currentHitNote = note;

                mediator.gameState.Combo(note, hitResult);

                CharacterHitType hitType = CharacterHitType.None;
                bool diffPosition = attackPosition != position;
                //bool hitSameTimeNote =
                //    lastHitNote != null &&
                //    Mathf.Abs(note.data.time - lastHitNote.data.time) < centerAttackDelay &&
                //    lastHitNote.interactWeight <= 0;
                bool hitDoubleNote = !alterEgo && IsDoubleHitNote(note);
                bool hitSingleTapNote = note.GetType() == typeof(SandBagNote) || note.GetType() == typeof(BossRushNote);
                if (hitDoubleNote || hitSingleTapNote)
                {
                    newHitPosition = NotePosition.Center;
                    hitType = CharacterHitType.Double_hit;
                    delay = 0.0f;
                }
                else if (!alterEgo && diffPosition)
                {
                    if (attackPosition == NotePosition.Air)
                    {
                        hitType = CharacterHitType.Up_hit;
                    }
                    else if (attackPosition == NotePosition.Road)
                    {
                        hitType = CharacterHitType.Down_hit;
                    }
                }
                else
                {
                    switch (attackPosition)
                    {
                        case NotePosition.Air:
                        switch (hitResult.precision)
                        {
                            case ComboPrecision.Perfect:
                            hitType = CharacterHitType.Air_hit_perfect;
                            break;

                            case ComboPrecision.Great:
                            hitType = CharacterHitType.Air_hit_great;
                            break;
                        }
                        break;

                        case NotePosition.Road:
                        switch (hitResult.precision)
                        {
                            case ComboPrecision.Perfect:
                            hitType = CharacterHitType.Road_hit_perfect;
                            break;

                            case ComboPrecision.Great:
                            hitType = CharacterHitType.Road_hit_great;
                            break;
                        }
                        break;
                    }
                }

                setAnimation = delegate (CharacterSpine cs) { cs.Do_Hit(hitType); };
            }
            CharacterSpine hitCS = alterEgo ? GetAlterEgo(newHitPosition) : character;
            if (setAnimation != null)
            {
                hitCS.gameObject.SetActive(true);

                if (!alterEgo)
                {
                    SetPosition(newHitPosition, delay);
                }
            }
            setAnimation?.Invoke(hitCS);
            if (hitResult.abort)
            {
                return;
            }
        }

        if (!used)
        {
            Miss(attackPosition, character);
        }
    }

    void Miss(NotePosition attackPosition, CharacterSpine cs)
    {
        if (interactions.Count > 0)
        {
            return;
        }

        if (position != attackPosition)
        {
            switch (attackPosition)
            {
                case NotePosition.Air:
                cs.Do_Jump();
                SetPosition(attackPosition, fastChangeDelay);
                break;

                case NotePosition.Road:
                cs.Do_JumpToDown();
                SetPosition(attackPosition, fastChangeDelay);
                break;
            }
        }
        else if (position == attackPosition && character.animationInfo.desc.type == CharacterAnimationType.Run)
        {
            cs.Do_MissOnRoad();
            SetPosition(attackPosition, fastChangeDelay);
        }
    }

    void ReleasePress(NotePosition attackPosition)
    {
        List<Note> visibleNotes = mediator.music.visibleNotes;

        foreach (Note note in visibleNotes)
        {
            note.ReleaseHit(attackPosition);
        }
    }

    void OnGround()
    {
        if (!character.desc.fly)
        {
            SetPositionImmediate(NotePosition.Road);
        }
        else if (character.desc.fly)
        {
            if (position == NotePosition.Center)
            {
                SetPosition(NotePosition.Road, fastChangeDelay);
            }
        }
    }

    void OnCharacterAnimated()
    {
    }

    void OnDead()
    {
        SetPosition(NotePosition.Road, fastChangeDelay);
        character.Do_Die();
        OnDeadCallback?.Invoke();
    }

    private void Awake()
    {
        character.hurtColorVisibleDuration = invisibleDelay;
        character.OnGroundCallback += OnGround;
        character.OnAnimatedCallback += OnCharacterAnimated;
        character.Do_In();

        airKeys.AddRange(new KeyCode[] { Keys.AirHit0, Keys.AirHit1, Keys.AirHit2, Keys.AirHit3 });
        roadKeys.AddRange(new KeyCode[] { Keys.RoadHit0, Keys.RoadHit1, Keys.RoadHit2, Keys.RoadHit3 });

        airAlterEgo.transform.SetParent(airPosition.transform);
        airAlterEgo.transform.localPosition = Vector2.zero;
        centerAlterEgo.transform.SetParent(centerPosition.transform);
        centerAlterEgo.transform.localPosition = Vector2.zero;
        roadAlterEgo.transform.SetParent(roadPosition.transform);
        roadAlterEgo.transform.localPosition = Vector2.zero;
    }

    private void Update()
    {
        airPivot.transform.position = mediator.hitPoint.airPos;
        centerPivot.transform.position = mediator.hitPoint.Pos;
        roadPivot.transform.position = mediator.hitPoint.roadPos;

        // Update Character Position
        float changePercent = Mathf.Clamp01((Time.time - positionChangeStartTime) / positionChangeDelay);
        changePercent = positionChangeDelay == 0 /*NaN*/ ? 1 : changePercent;
        float changeT = Mathf.Pow(changePercent, 1 / 3f);
        Vector2 characterPosition = Vector2.Lerp(GetWorldPosition(prevPosition), GetWorldPosition(position), changeT);
        if (!character.desc.fly)
        {
            // Apply gravity

            if (changePercent >= 1.0f &&
                (int)position > (int)NotePosition.Road &&
                interactions.Count == 0 &&
                !character.animationInfo.desc.loop)
            {
                float fallPercent = (character.animationTime - positionChangeDelay) / (character.animationInfo.animation.Duration - positionChangeDelay);
                float fallT = Mathf.Pow(fallPercent, 5);
                characterPosition = Vector2.Lerp(GetWorldPosition(position), GetWorldPosition(NotePosition.Road), fallT);
            }
        }
        character.transform.position = characterPosition;

        int prevAirStack = airPressStack;
        int airKeyDown = 0;
        foreach (KeyCode keyCode in airKeys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                airPressStack = Mathf.Clamp(airPressStack + 1, 0, airKeys.Count);
                airKeyDown += 1;
            }
            else if (Input.GetKeyUp(keyCode))
            {
                airPressStack = Mathf.Clamp(airPressStack - 1, 0, airKeys.Count);
            }
        }

        int prevRoadStack = roadPressStack;
        int roadKeyDown = 0;
        foreach (KeyCode keyCode in roadKeys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                roadPressStack = Mathf.Clamp(roadPressStack + 1, 0, airKeys.Count);
                roadKeyDown += 1;
            }
            else if (Input.GetKeyUp(keyCode))
            {
                roadPressStack = Mathf.Clamp(roadPressStack - 1, 0, airKeys.Count);
            }
        }

        if (airKeyDown + roadKeyDown > 0)
        {
            mediator.music.visibleNotes.Sort((lhs, rhs) => lhs.ratio.CompareTo(rhs.ratio));
        }

        if (prevAirStack > airPressStack && airPressStack == 0)
        {
            ReleasePress(NotePosition.Air);
        }
        if (prevRoadStack > roadPressStack && roadPressStack == 0)
        {
            ReleasePress(NotePosition.Road);
        }

        for (int i = 0; i < airKeyDown; ++i)
        {
            Attack(NotePosition.Air);
        }
        for (int i = 0; i < roadKeyDown; ++i)
        {
            Attack(NotePosition.Road);
        }
    }

    public void GameReset()
    {
        ClearState();
    }
}
