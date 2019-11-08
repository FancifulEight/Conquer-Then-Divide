using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : MonoBehaviour {
    int flagLMBAngle = 0;
    int flagRMBAngle = 0;

    public RectTransform leftFlagTransform;
    public RectTransform rightFlagTransform;
    
    public Animator kingAnim;

    void Start() {
        kingAnim = GetComponent<Animator>();
    }

    public void ChangeFlags(bool lmb, bool rmb) {
        flagLMBAngle = (lmb) ? Mathf.Max(-30, flagLMBAngle - 10):Mathf.Min(25, flagLMBAngle + 2);
        flagRMBAngle = (rmb) ? Mathf.Min(30, flagRMBAngle + 10):Mathf.Max(-25, flagRMBAngle - 2);
        
        //if (lmb || rmb) Debug.Log(string.Format("Left Angle: {0}, Right Angle: {1}", flagLMBAngle, flagRMBAngle));

        leftFlagTransform.localRotation = Quaternion.Euler(0, 0, flagLMBAngle);
        rightFlagTransform.localRotation = Quaternion.Euler(0, 0, flagRMBAngle);
    }
}
