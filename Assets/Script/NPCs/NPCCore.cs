using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class NPCCore : MonoBehaviour
{
    public string race = "";
    public string uid;

    [Range(0, 1)]
    public float hunger = 1f;
    [Range(0, 1)]
    public float thirst = 1f;
    [Range(0, 1)]
    public float sleepNeed = 1f;


    [Header("Hunger")]
    [Range(0.1f, 0.5f)]
    public float hungerThreshold;
    [Range(0.001f, 0.01f)]
    public float hungerLost;
    [Range(0.1f, 0.2f)]
    public float hungerGain;
    public bool isEating;

    [Header("Thirst")]
    [Range(0.1f, 0.5f)]
    public float thirstThreshold;
    [Range(0.001f, 0.01f)]
    public float thirstLost;
    [Range(0.1f, 0.2f)]
    public float thirstGain;
    public bool isDrinking;

    [Header("Sleep")]
    [Range(0.1f, 0.5f)]
    public float sleepThreshold;
    [Range(0.001f, 0.01f)]
    public float sleepLost;
    [Range(0.01f, 0.02f)]
    public float sleepGain;
    public bool isSleeping;

    public bool isExited = false;


    [Space(10)]
    public bool isWandering;
    public bool isAnAlpha = false;
    public int velocity;
    protected bool isMoving;

    [Header("Visuals")]
    protected bool walkingAnimation;
    public float height = .5f;
    public float walkAnimationTime = .2f; //La del player tiene que ser
    public IACell currentCell;
    private IACell destinyPos = null;

    [Range(1, 20)]
    public int sightLenght;
    public int sightWitdh;
    protected List<IACell> vision = new List<IACell>();
    protected List<IACell> seashoresOnSight = new List<IACell>();
    public bool isAgressive;
    public bool isTerritorial;

    public bool isSociable;
    public NPCCore familyLeader;

    public IACell waterSource;
    public IACell familyWaterSourcer;

    public IACell eatSource;
    public IACell familyEatSourcer;

    public IACell sleepNode;
    public IACell familySleepNode;

    public GameObject visualObject;

    public Vector2Int currentDirection = new Vector2Int(1, 0);

    private Tweener turnTweener;
    public bool isOnCameraVision = true;

    private NPCTemplate _template;
    public SpriteRenderer visual;
    private Dictionary<Scale, ScaleSet> scaleSets;

    [Header("Animations")]
    public GameObject sleepAnimation;
    public GameObject eatAnimation;
    public GameObject drinkAnimation;
    public Animator surpriseAnimator;


    #region cons
    public class ScaleSet
    {
        public Vector3 position;
        public Vector3 scale;
    }

    private ScaleSet _shortScale => SetScaleRange(new Vector3(-0.09f, 0.7f, 0f), 0.15f);
    private ScaleSet _mediumScale => SetScaleRange(new Vector3(-0.09f, 0.974f, 0f), 0.2f);
    private ScaleSet _largeScale => SetScaleRange(new Vector3(-0.09f, 1.19f, 0.21f), 0.25f);


    private ScaleSet SetScaleRange(Vector3 pos, float scale)
    {
        ScaleSet set = new ScaleSet();
        set.position = pos;
        set.scale = new Vector3(scale, scale, scale);
        return set;
    }

    #endregion

    private bool OnSight()
    {
        if (isOnCameraVision != currentCell.IsOnSight)
        {
            if (!isOnCameraVision) OnEnterVision();
            else OnOutVision();
        }

        return currentCell.IsOnSight;
    }

    public virtual void OnEndAnimation() 
    {

    }
    public void UpdateStats()
    {
        hunger = (isEating) ? (hunger + hungerGain) : (hunger - hungerLost);
        sleepNeed = (isSleeping) ? (sleepNeed + sleepGain) : (sleepNeed - sleepLost);
        thirst = (isDrinking) ? (thirst + thirstGain) : (thirst - thirstLost);
    }

    public virtual void SetUp(NPCTemplate template, string race, string uid)
    {
        this.race = race;
        this.uid = uid;
        _template = template;
            
        scaleSets = new Dictionary<Scale, ScaleSet>()
        {
            { Scale.SHORT, _shortScale},
            { Scale.MEDIUM, _mediumScale},
            { Scale.LARGE, _largeScale}
        };

        visual.sprite = _template.image;
        ScaleSet set = scaleSets[_template.scale];

        visual.transform.localPosition = set.position;
        visual.transform.localScale = set.scale;

        PutOnGrid();
    }

    public bool IsInDrinkBehaviour()
    {
        bool value = false;

        if (isDrinking)
        {
            if (thirst < 1) value = true;
            else isDrinking = false;
        }
        else
        {
            if (thirst <= thirstThreshold)
            {
                isEating = isSleeping = false;
                value = true;
            }
        }

        return value;
    }

    public bool IsInEatingBehaviour()
    {
        bool value = false;

        if (isEating)
        {
            if (hunger < 1) value = true;
            else isEating = false;
        }
        else
        {
            if (hunger <= hungerThreshold)
            {
                isDrinking = isSleeping = false;
                value = true;
            }
        }

        return value;
    }

    public bool IsInSleepingBehaviour()
    {
        bool value = false;

        if (isSleeping)
        {
            if (sleepNeed < 1) value = true;
            else isSleeping = false;
        }
        else
        {
            if (sleepNeed <= sleepThreshold)
            {
                isDrinking = isEating = false;
                value = true;
            }
        }

        return value;
    }

    protected virtual void OnEnable()
    {
        TurnHolder.resolveTurn += OnTurnUpdate;
    }
    protected virtual void OnDisable()
    {
        TurnHolder.resolveTurn -= OnTurnUpdate;
    }

    protected void OnTurnUpdate()
    {
        AnimalBehaviour();

        if (currentCell != null) isOnCameraVision = OnSight();
    }

    protected void AnimalBehaviour()
    {
        UpdateStats();

        if (IsInDrinkBehaviour())
        {
            DrinkBehaviour();
        }
        else if (IsInSleepingBehaviour())
        {
            SleepBehaviour();
        }
        else if (IsInEatingBehaviour())
        {
            EatBehaviour();
        }
        else
        {
            WanderBehaviour();
        }

        sleepAnimation.SetActive(isSleeping);
        eatAnimation.SetActive(isEating);
        drinkAnimation.SetActive(isDrinking);


        if (currentDirection.x != 0 && transform.localScale.x != currentDirection.x)
        {
            turnTweener?.Kill();
            turnTweener = transform.DOScale(new Vector3(currentDirection.x, transform.localScale.y, transform.localScale.z), .2f);
        }
    }

    protected virtual void UpdateVision()
    {
        if (currentCell == null) return;

        vision = new List<IACell>();
        seashoresOnSight = new List<IACell>();
        int newX = 0;
        int newY = 0;

        if (currentDirection.x != 0)
        {
            for (int x = 0; x <= sightLenght; x++)
            {
                for (int y = -sightWitdh; y <= sightWitdh; y++)
                {
                    newX = currentCell.GridX + (x * currentDirection.x);
                    newY = currentCell.GridZ + y;
                    AddToList(newX, newY);
                }
            }
        }
        else
        {
            for (int y = 0; y <= sightLenght; y++)
            {
                for (int x = -sightWitdh; x <= sightWitdh; x++)
                {
                    newX = currentCell.GridX + x;
                    newY = currentCell.GridZ + (y * currentDirection.y);
                    AddToList(newX, newY);
                }
            }
        }

        void AddToList(int x, int y)
        {
            IACell neightbour = Grid.Istance.GetGridCell(x, y);
            if (neightbour != null)
            {
                if (!isAnAlpha && isSociable && familyLeader == null && neightbour.ocupedAnimal != null && neightbour.ocupedAnimal != this)
                {
                    NPCCore animalInVision = neightbour.ocupedAnimal;
                    if (animalInVision.race == race)
                    {
                        if (animalInVision.familyLeader)
                            familyLeader = animalInVision.familyLeader;
                        else
                        {
                            animalInVision.isAnAlpha = true;
                            familyLeader = animalInVision;
                        }
                    }
                }

                vision.Add(neightbour);

                if (neightbour.isSeashore && waterSource == null && neightbour.owner == "")
                    waterSource = ReclameCell(neightbour);
            }
        }

    }

    protected IACell ReclameCell(IACell cell) 
    {
        cell.owner = uid;
        return cell;
    }
    protected IACell ReleaseCell(IACell cell)
    {
        cell.owner = "";
        return null;
    }
    protected virtual void DrinkBehaviour()
    {
        isDrinking = false;
        if (familyLeader != null && familyLeader.waterSource != null && waterSource != null &&
            Vector3.Distance(waterSource.Position, familyLeader.waterSource.Position) > 10)
            waterSource = ReleaseCell(waterSource);

        if (waterSource == null)
        {
            WanderBehaviour();
            return;
        }

        if (currentCell != waterSource)
        {
            List<Vector3> path = AStar.FindPath(currentCell, waterSource);

            if (path.Count > 0)
                WanderTillCell(path, velocity);

        }
        else
        {
            isDrinking = true;
            isWandering = false;
        }
    }
    protected virtual void SleepBehaviour() { }
    protected virtual void EatBehaviour() { }

    protected virtual void WanderBehaviour() { }

    protected virtual void OnEnterVision()
    {
        visualObject.SetActive(true);
    }

    protected virtual void OnOutVision()
    {
        visualObject.SetActive(false);
    }

    [ContextMenu("PutOnGrid")]
    public void PutOnGrid()
    {
        IACell cell = Grid.Istance.GetGridCell((int)transform.position.x, (int)transform.position.z);
        Vector3 newPosition = new Vector3(cell.GridX, cell.height, cell.GridZ);
        currentCell = cell;

        transform.position = newPosition;
    }

    protected virtual void WanderOption(int amount)
    {
        if (amount <= 0)
            return;

        currentDirection = GetRandomDirection(currentDirection);
        Vector2Int newDirection;
        IACell cell = null;
        float stay = UnityEngine.Random.Range(0f, 1f);
        if(stay <= .33f) 
        {
            if(currentCell == null)
                currentCell = Grid.Istance.GetGridCell((int)transform.position.x, (int)transform.position.z);

            return;
        }

        for (int i = 1; i <= amount; i++)
        {
            newDirection = currentDirection * i;
            IACell cellDa = Grid.Istance.GetGridCell((int)transform.position.x + newDirection.x, (int)transform.position.z + newDirection.y);

            if (cellDa == null) break;
            if (!cellDa.IsWalkable || cellDa.IsOccupped) break;

            cell = cellDa;
        }

        if (cell != null && cell.IsWalkable && !cell.IsOccupped)
        {
            Vector3 newPosition = new Vector3(cell.GridX, cell.height, cell.GridZ);
            Mover(newPosition, () => { });
        }
    }

    protected void Mover(Vector3 newPosition, Action onComplete)
    {
        if (!isOnCameraVision)
        {
            transform.position = newPosition;
            OnEndMovement();
            return;
        }
        float animationTime = walkAnimationTime;

        transform.DOMove(newPosition, animationTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            OnEndMovement();
        });

        WalkAnimation(animationTime);

        void OnEndMovement()
        {
            currentCell.IsOccupped = false;
            currentCell.UpdateOcupant(this, true);
            currentCell = Grid.Istance.GetGridCell((int)newPosition.x, (int)newPosition.z);
            if (currentCell != null)
                currentCell.IsOccupped = true;

            currentCell.UpdateOcupant(this, false);
            destinyPos = null;
            UpdateVision();
            onComplete?.Invoke();
        }
    }

    private void WalkAnimation(float animationTime)
    {
        if (!walkingAnimation)
        {
            walkingAnimation = true;


            Vector3 InitPos = Vector3.zero;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(visualObject.transform.DOLocalMoveY(height, animationTime / 2));
            sequence.Append(visualObject.transform.DOLocalMoveY(InitPos.y, animationTime / 2));
            sequence.OnComplete(() => walkingAnimation = false);


            sequence.Play();
        }
    }

    protected Vector2Int GetRandomDirection(Vector2Int cDirection)
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        int x = cDirection.x;
        int y = cDirection.y;

        Vector2Int direction;

        if (random < 0.5f) direction = new Vector2Int(x, y);
        else if (random < 0.75f) direction = new Vector2Int(y, x);
        else direction = new Vector2Int(-y, -x);

        return direction;
    }

    protected void WanderTillCell(List<Vector3> path, int amount)
    {
        if (amount <= 0)
            return;

        Vector3 newPosition = path[amount - 1];
        int x = 0;
        int y = 0;
        if (newPosition.x != transform.position.x)
        {
            x = (newPosition.x > transform.position.x) ? 1 : (newPosition.x < transform.position.x) ? -1 : 0;
        }
        if (newPosition.z != transform.position.z)
        {
            y = (newPosition.z > transform.position.z) ? 1 : (newPosition.z < transform.position.z) ? -1 : 0;
        }

        currentDirection = new Vector2Int(x, y);
        Mover(newPosition, () => { });

    }

}


