using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class RankUI : MonoBehaviour
{
    public TMP_Text textRank;
    public Image imageCar;
    public Sprite myCarSprite;
    public Sprite aiCarSprite;
    public TMP_Text textCarDetail;

    public void UpdateData(GameManager.CarResult carResult)
    {
        textRank.text = $"#{carResult.rank}";
        imageCar.sprite = carResult.isMyCar ? myCarSprite : aiCarSprite;
        textCarDetail.text = $"{carResult.carName}\n{carResult.duration.FormatTimeFromSecondsPretty()}";
    }
}
