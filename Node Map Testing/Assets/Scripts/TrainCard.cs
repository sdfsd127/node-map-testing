using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTR.TTR_Trains;

[CreateAssetMenu(fileName = "New Train Card", menuName = "Train Card")]
public class TrainCard : ScriptableObject
{
    public Sprite sprite;
    public TRAIN_COLOUR colour;
}
