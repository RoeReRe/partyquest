using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownBehaviour : MonoBehaviour
{
    Button skillButton;
    Image fillImage;
    TMP_Text currCDDisplay;
    float currCD;
    float maxCD;

    public void Init(float currCD) {
        fillImage = this.GetComponent<Image>();
        currCDDisplay = this.GetComponentInChildren<TMP_Text>();
        skillButton = this.transform.parent.Find("Icon").GetComponent<Button>();
        skillButton.interactable = false;
        this.currCD = currCD;
        this.maxCD = currCD;
    }

    // Update is called once per frame
    void Update()
    {
        currCD -= Time.deltaTime;

        if (currCD <= 0) {
            skillButton.interactable = true;
            Destroy(this.gameObject);
        }

        currCDDisplay.text = Mathf.Ceil(currCD).ToString();
        fillImage.fillAmount = Mathf.Min(currCD / maxCD, 1);
    }
}
