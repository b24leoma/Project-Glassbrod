using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Entity : MonoBehaviour
    {
        public enum EntityType
        {
            HumanSpearman,
            HumanArcher,
            DemonSwordsman,
            DemonTank
        };

        public EntityType Type;
        private Animator animator;
        public string Name { get; protected set; }
        public string Age { get; protected set; }
        public string Description { get; protected set; }
        public int MaxHealth;
        public int CurrentHealth { get; protected set; }
        public int Damage;
        public int MoveRange;
        public int AttackRange;
        public bool IsMelee { get; protected set; }

        [SerializeField] private Slider healthBar;
        [HideInInspector] public bool isHuman;
        [HideInInspector] public bool hasMoved;
        [HideInInspector] public bool hasAttacked;

        public Vector2Int Position
        {
            get => new((int)(transform.position.x + 0.5f), (int)(transform.position.y + 0.5f));
            set => transform.position = new Vector3(value.x - 0.5f, value.y - 0.5f);
        }

        public void TakeDamage(float damage)
        {
            if (!healthBar.IsActive()) healthBar.gameObject.SetActive(true);
            CurrentHealth -= (int)damage;
            healthBar.value = CurrentHealth;
        }


        void Start()
        {
            animator = GetComponent<Animator>();
            hasMoved = false;
            hasMoved = false;
            CurrentHealth = MaxHealth;
            IsMelee = AttackRange <= 1;
            healthBar.maxValue = MaxHealth;
            healthBar.value = MaxHealth;
        }

        public void MoveToTile(Vector2Int pos)
        {
            Position = pos;
        }


        public virtual void SetMoving(bool moving)
        {
            hasMoved = moving;
        }

        public virtual void SetAttacking(bool attacking)
        {
            hasAttacked = attacking;
            if (attacking) PlayAttack();

        }

        public void Kill()
        {
            Destroy(gameObject);
        }

        protected void PlayAttack()
        {
            animator.SetTrigger("Attack");
        }
    }

    public static class NameGenerator
    {
        private static List<string[]> humanInfo;
        private static List<string[]> demonInfo;
        private static StreamReader reader;
        

        public static string[] GenerateIdentity(bool isHuman)
        {
            if (isHuman)
            {
                string[] name = humanInfo[Random.Range(1, humanInfo.Count)];
                humanInfo.Remove(name);
                return name;
            }
            else
            {
                string[] name = demonInfo[Random.Range(1, demonInfo.Count)];
                demonInfo.Remove(name);
                return name;
            }
        }

        public static void ReadFromFile(TextAsset human, TextAsset demons)
        {
            humanInfo = new List<string[]>();
            demonInfo = new List<string[]>();

            string[] info = human.text.Split('\n');
            if (info.Length % 3 != 0) Debug.Log("Human file has incorrect amount of lines");
            for (int i = 0; i < info.Length; i += 3)
            {
                humanInfo.Add(new[] { info[i], info[i + 1], info[i + 2] });
            }
            
            info = demons.text.Split('\n');
            if (info.Length % 3 != 0) Debug.Log("Demon file has incorrect amount of lines");
            for (int i = 0; i < info.Length; i+=3)
            {
                demonInfo.Add(new[] { info[i], info[i + 1], info[i + 2] });
            }

        }
    }
}