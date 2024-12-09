
using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    public int moveCounter;
   public int attackCounter;
   public UnityEvent onMove;
   public UnityEvent onAttack;
   public UnityEvent onFirstAttack;
   public UnityEvent onFirstMove;

   private BattleController battleController;
   private List<Entity> allUnits;

   private TutorialScript tutorialScript;


   private void Start()
   {
       if (battleController == null)
       {
         battleController = GetComponent<BattleController>();
       }
   }

   public void TotalStateChecker()
   {
       
       var characters = battleController.GetCharacters();

       if (characters == null)
       {
           Debug.Log("jaboba");
       }

       if (characters != null)
           foreach (var entity in characters)
           {
               tutorialScript = entity.GetComponent<TutorialScript>();
               tutorialScript.CheckingUnits();
           }
   }
}