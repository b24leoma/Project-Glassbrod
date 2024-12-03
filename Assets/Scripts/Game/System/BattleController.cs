using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class BattleController : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField] public List<Level> LevelEntities;
        [Header("Events")]
        [SerializeField] private UnityEvent Player1TurnStart;
        [SerializeField] private UnityEvent Player2TurnStart;
        [Header("Components")]
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private bool isPlayer1Turn;  
        [Header("Assets")]
        [SerializeField] private GameObject infoDisplay;
        [SerializeField] private TextMeshProUGUI displayStats;
        [SerializeField] private TextMeshProUGUI displayName;
        [SerializeField] private Transform entityParent;
        [SerializeField] private GameObject humanSpearman;
        [SerializeField] private GameObject humanArcher;
        [SerializeField] private GameObject demonSwordsman;
        [SerializeField] private GameObject demonTank;
        private List<Entity> characters;
        private int level;
        [HideInInspector] public List<Vector2> demons;
        [HideInInspector] public List<Vector2> humans;
        void Start()
        {
            characters = new List<Entity>();
            gridSystem.ClearGrid();
            LoadLevel();
        }

        void LoadLevel()
        {
            characters.Clear();
            humans = new List<Vector2>();
            demons = new List<Vector2>();
            foreach (SpawnEntity spawn in LevelEntities[level].spawnList)
            {
                CreateEntity(new Vector2Int((int)spawn.Position.x, (int)spawn.Position.y), spawn.Type);
            }
            infoDisplay.SetActive(false);
            Player1TurnStart.Invoke();
        }

        public void NextLevel()
        {
            level++;
            if (level >= LevelEntities.Count)
            {
                Debug.Log("Win");
            }
            else
            {
                gridSystem.ClearGrid();
                LoadLevel();
            }
        }

        void CreateEntity(Vector2Int pos, Entity.EntityType type)
        {
            GameObject g = null;
            switch (type)
            {
                case Entity.EntityType.HumanSpearman:
                    g = Instantiate(humanSpearman, Vector3.zero, Quaternion.identity, entityParent);
                    break;
                case Entity.EntityType.HumanArcher:
                    g = Instantiate(humanArcher, Vector3.zero, Quaternion.identity, entityParent);
                    break;
                case Entity.EntityType.DemonSwordsman:
                    g = Instantiate(demonSwordsman, Vector3.zero, Quaternion.identity, entityParent);
                    break;
                case Entity.EntityType.DemonTank:
                    g = Instantiate(demonTank, Vector3.zero, Quaternion.identity, entityParent);
                    break;
            }
            Entity e = g.GetComponent<Entity>();
            if (e.isHuman) humans.Add(pos);
            else demons.Add(pos);
            gridSystem.ConnectToTile(pos, e);
            characters.Add(e);
        }


        public void Move(Vector2Int from, Vector2Int to)
        {
            gridSystem.MoveUnit(from, to);
            FMODManager.instance.OneShot("GenericWalk", new Vector3(to.x, to.y, 0));
        }
        public void Attack(Entity attacker, Entity target)
        {
            float reduction = 1 - (gridSystem.GetTile(new Vector2Int((int)(target.Position.x+0.6f), (int)(target.Position.y+0.6f))) //0.6 due to floating point math suck
                .DamageReductionPercent / 100f);
            target.TakeDamage(reduction * attacker.Damage);
            FMODManager.instance.OneShot("GenericAttack", attacker.transform.position);
            FMODManager.instance.OneShot("GenericHit", target.transform.position);
            
        }

        public void EndTurn()
        {
            isPlayer1Turn = !isPlayer1Turn;
            if(isPlayer1Turn) Player1TurnStart?.Invoke();
            else Player2TurnStart?.Invoke();
        }


        public void UpdateCharacterDisplay(bool showDisplay, Entity entity)
        {
            infoDisplay.SetActive(showDisplay);
            if (showDisplay)
            {
                displayStats.text = $"{entity.Damage}\n{entity.CurrentHealth}/{entity.MaxHealth}\n{(entity.IsMelee ? "MELEE" : "RANGED")}";
                displayName.text = entity.Name;
            }
        }



        public List<Entity> GetCharacters(){return characters;}
        public Entity GetCharacterAt(int i){return characters[i];}
    
        
        
        
        
        
        
        
        [System.Serializable] public class SpawnEntity
        {
            public Vector2 Position;
            public Entity.EntityType Type;
        }

        [System.Serializable]
        public class Level
        {
            public List<SpawnEntity> spawnList;
        }
    }
}
