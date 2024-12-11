using UnityEngine;

namespace Game
{
    public class Demon : Entity
    {
        [SerializeField] private GameObject AttackingImage;
        [HideInInspector] public Human target;
        private SpriteRenderer attackImg;
        // Start is called before the first frame update
        void Awake()
        {
            isHuman = false;
            Name = NameGenerator.GenerateName(false);
            attackImg = AttackingImage.GetComponent<SpriteRenderer>();
        }

        public void DisplayAttackingImage(bool display, Color color)
        {
            AttackingImage.SetActive(display);
            attackImg.color = color;
        }
    }
}
